# 건랜스 공격 명령 전체 플로우 분석

**작성일**: 2024-12-29
**분석 목적**: TryCastShot가 어떻게 트리거되는지 전체 플로우 파악

## 전체 플로우 다이어그램

```
사용자 공격 명령 클릭
    ↓
Job 생성 (JobDefOf.AttackMelee)
    ↓
JobDriver_AttackMelee.MakeNewToils()
    ↓
Toils_Combat.FollowAndMeleeAttack() 생성
    ↓ (매 틱마다 실행되는 hitAction)
Pawn_MeleeVerbs.TryMeleeAttack()
    ↓
Verb.TryStartCastOn()
    ↓
[Warmup 있으면] Stance_Warmup → WarmupComplete()
    ↓
Verb.TryCastNextBurstShot()
    ↓
Verb_GunlanceFiring.TryCastShot() ⭐
```

## 상세 호출 스택

### 1. 사용자 명령 입력

**UI 클릭** → **Command_VerbTarget** 클릭 시:
```csharp
// VerbTracker.cs - GetVerbsCommands()
// UI에서 공격 버튼 클릭 → Job 생성
pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.AttackMelee, target))
```

### 2. Job 생성 및 시작

**JobDriver_AttackMelee.MakeNewToils()** (78-112):
```csharp
yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, TargetIndex.B, delegate()
{
    Thing thing = this.job.GetTarget(TargetIndex.A).Thing;
    // hitAction: 이 람다가 실행됨
    if (this.pawn.meleeVerbs.TryMeleeAttack(thing, this.job.verbToUse, false))
    {
        // 공격 성공 처리
    }
});
```

### 3. FollowAndMeleeAttack 틱 로직

**Toils_Combat.cs** - `FollowAndMeleeAttack` (114-163):
```csharp
Toil followAndAttack = ToilMaker.MakeToil("FollowAndMeleeAttack");
followAndAttack.tickIntervalAction = delegate(int delta)
{
    // 매 틱마다 실행
    if (actor.CanReachImmediate(target, PathEndMode.Touch))
    {
        hitAction();  // ← 여기서 TryMeleeAttack 호출
    }
};
```

### 4. TryMeleeAttack - Verb 선택

**Pawn_MeleeVerbs.cs** - `TryMeleeAttack` (100-139):
```csharp
public bool TryMeleeAttack(Thing target, Verb verbToUse = null, bool surpriseAttack = false)
{
    // verbToUse가 지정되어 있으면 사용, 없으면 선택
    Verb verb = verbToUse ?? this.TryGetMeleeVerb(target);
    if (verb == null)
    {
        return false;
    }
    
    // Verb 실행 시작
    verb.TryStartCastOn(target, surpriseAttack, true, false, false);
    return true;
}
```

**TryGetMeleeVerb** (52-59):
```csharp
public Verb TryGetMeleeVerb(Thing target)
{
    // 현재 Verb가 유효하지 않으면 재선택
    if (this.curMeleeVerb == null || this.curMeleeVerbTarget != target || 
        Find.TickManager.TicksGame >= this.curMeleeVerbUpdateTick + 60 || 
        !this.curMeleeVerb.IsStillUsableBy(this.pawn) || 
        !this.curMeleeVerb.IsUsableOn(target))
    {
        this.ChooseMeleeVerb(target);  // ← Tool 선택 로직
    }
    return this.curMeleeVerb;
}
```

### 5. TryStartCastOn - 캐스팅 시작

**Verb.cs** - `TryStartCastOn` (481-532):
```csharp
public virtual bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, ...)
{
    // 검증
    if (!this.caster.Spawned || this.state == VerbState.Bursting || !this.CanHitTarget(castTarg))
    {
        return false;
    }
    
    this.currentTarget = castTarg;
    this.currentDestination = destTarg;
    
    // Warmup이 있으면 Warmup 스턴스 시작
    if (this.CasterIsPawn && this.WarmupTime > 0f)
    {
        int ticks = (this.WarmupTime * statValue).SecondsToTicks();
        this.CasterPawn.stances.SetStance(new Stance_Warmup(ticks, castTarg, this));
    }
    else
    {
        // Warmup 없으면 즉시 실행
        this.WarmupComplete();
    }
    return true;
}
```

### 6. WarmupComplete - Burst 시작

**Verb.cs** - `WarmupComplete` (534-539):
```csharp
public virtual void WarmupComplete()
{
    this.burstShotsLeft = this.ShotsPerBurst;
    this.state = VerbState.Bursting;
    this.TryCastNextBurstShot();  // ← 첫 발사
}
```

**Stance_Warmup**에서 완료 시 호출:
- Stance_Warmup.WarmupStanceTick() → 0이 되면 WarmupComplete() 호출

### 7. TryCastNextBurstShot - 발사 처리

**Verb.cs** - `TryCastNextBurstShot` (615-675):
```csharp
protected void TryCastNextBurstShot()
{
    LocalTargetInfo localTargetInfo = this.currentTarget;
    
    // 핵심: TryCastShot 호출 ⭐
    if (this.Available() && this.TryCastShot())
    {
        // 사운드, 효과 처리
        if (this.verbProps.soundCast != null)
        {
            this.verbProps.soundCast.PlayOneShot(...);
        }
        
        // 여러 알림 처리
        this.CasterPawn.Notify_UsedVerb(this.CasterPawn, this);
        this.burstShotsLeft--;
    }
}
```

**VerbTick**에서 연속 발사 (541-578):
```csharp
public void VerbTick()
{
    if (this.state == VerbState.Bursting)
    {
        if (this.ticksToNextBurstShot <= 0)
        {
            this.TryCastNextBurstShot();  // ← 다음 발사
        }
        this.BurstingTick();
    }
}
```

### 8. TryCastShot - 실제 공격 실행

**Verb_GunlanceFiring.cs** - `TryCastShot` (14-91):
```csharp
protected override bool TryCastShot()
{
    Pawn casterPawn = CasterPawn;
    if (!casterPawn.Spawned || casterPawn.stances.FullBodyBusy)
    {
        return false;
    }

    Thing targetThing = currentTarget.Thing;
    
    // 탐지, 회전 등 처리
    casterPawn.rotationTracker.Face(targetThing.DrawPos);
    
    // 포격 실행
    verbProperties = verbProps as VerbProperties_Gunlance;
    GunlanceExplosion explosion = GenSpawn.Spawn(GunlanceDefOf.GunlanceExplosion, ...);
    explosion.PreStartExplosion(TeleUtils.circularSectorCellsStartedTarget(...));
    explosion.StartExplosion(null, null);
    
    return true;
}
```

## 핵심 포인트

### 1. Tool 선택 시점

**매 공격마다 선택됨**이 아니라:
- **60틱마다 선택** (or 조건 불만족 시 즉시)
- `TryGetMeleeVerb`에서 캐싱된 Verb 사용
- **같은 대상, 60틱 내**면 동일한 Verb 사용

```csharp
if (Find.TickManager.TicksGame >= this.curMeleeVerbUpdateTick + 60)
{
    this.ChooseMeleeVerb(target);  // 재선택
}
```

### 2. hitAction 실행 빈도

**FollowAndMeleeAttack.tickIntervalAction**:
- **매 틱** 확인하지만, 실제 공격은:
  - `verb.EquipmentSource`의 쿨다운에 따라 제한됨
  - 공격 성공하면 `numMeleeAttacksMade++`
  - `maxNumMeleeAttacks` 도달하면 Job 종료

### 3. TryCastShot vs ApplyMeleeDamageToTarget

**Verb_GunlanceFiring**은:
- `TryCastShot`를 오버라이드하여 포격 로직 실행
- `ApplyMeleeDamageToTarget`는 구현하지 않음 (실제로 호출 안 됨)
- 원본 `Verb_MeleeAttack`와 다른 방식

**원본 Verb_MeleeAttack**:
```csharp
protected override bool TryCastShot()
{
    // 명중률 체크
    if (Rand.Chance(this.GetNonMissChance(thing)))
    {
        // 피해 적용
        DamageWorker.DamageResult damageResult = this.ApplyMeleeDamageToTarget(this.currentTarget);
        return true;
    }
    return false;
}
```

**Verb_GunlanceFiring**:
```csharp
protected override bool TryCastShot()
{
    // 포격만 실행, 피해는 StartExplosion에서 처리
    explosion.StartExplosion(null, null);
    return true;
}
```

### 4. 쿨다운 처리

Tool의 `cooldownTime`은:
- `TryCastNextBurstShot`에서 처리되지 않음
- **Verb의 쿨다운은 다른 곳에서 처리됨**
- `FollowAndMeleeAttack`에서는 매 틱 확인

실제 쿨다운은:
- `EquipmentSource`의 stat으로 처리
- 또는 Verb 내부에서 처리

## VerbState 상태 머신

```
Idle → Warmup → Bursting → Idle
```

1. **Idle**: 대기 중
2. **Warmup**: 따뜻하게 하는 중 (Stance_Warmup)
3. **Bursting**: 연속 발사 중 (TryCastNextBurstShot 반복)
4. **Idle**: 종료

## 결론

**사용자가 공격 명령을 내리면:**
1. **Job 생성** → JobDriver_AttackMelee 시작
2. **FollowAndMeleeAttack** Toil이 매 틱 대상 확인
3. 도달하면 **TryMeleeAttack** → Verb 선택/사용
4. **TryStartCastOn** → Warmup 시작 (있으면)
5. **WarmupComplete** → **TryCastNextBurstShot** 첫 호출
6. **TryCastShot** ⭐ → 실제 공격 실행 (포격/공격)
7. **VerbTick** → 연속 발사 계속

**핵심**: `TryCastShot`는 **TryCastNextBurstShot**에서 호출되고, 이는 **WarmupComplete** 또는 **VerbTick**에서 호출됨.

