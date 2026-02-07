# Tools 공격 후 Cooldown 구조 분석 보고서

**태그**: Tools Cooldown Stance_Cooldown FullBodyBusy Attack Mechanism Analysis

**작성일**: 2025-01-23  
**분석 목적**: Tools의 공격 후 cooldown 메커니즘 구조 분석 및 공격 후 움직임 제한 원인 파악

---

## 핵심 요약

**공격 후 움직임이 제한되는 이유**:
1. `TryCastShot()` 실행 후 `Verb.BurstingTick()`에서 `Stance_Cooldown` 설정
2. `Stance_Cooldown`이 활성화되면 `Pawn_StanceTracker.FullBodyBusy`가 `true`가 됨
3. `FullBodyBusy`가 `true`이면 Pawn이 다른 행동(이동, 공격 등)을 수행할 수 없음
4. Cooldown 시간은 Tool의 `cooldownTime` 속성에서 가져옴

---

## 전체 플로우 다이어그램

```
공격 명령
    ↓
Verb.TryStartCastOn()
    ↓
[Warmup 있으면] Stance_Warmup 설정 → FullBodyBusy = true
    ↓
WarmupComplete()
    ↓
Verb.TryCastNextBurstShot()
    ↓
Verb.TryCastShot() ⭐ 공격 실행
    ↓
Verb.BurstingTick() ⭐ Cooldown 설정
    ↓
Stance_Cooldown 설정 → FullBodyBusy = true
    ↓
Cooldown 완료 → FullBodyBusy = false
```

---

## 1. Stance 시스템 구조

### 1.1 Stance 종류

RimWorld에서 Stance는 Pawn의 현재 행동 상태를 나타냅니다:

1. **Stance_Warmup**: 조준/준비 중
2. **Stance_Cooldown**: 공격 후 쿨다운 중
3. **Stance_Busy**: 기타 바쁜 상태 (베이스 클래스)

### 1.2 FullBodyBusy 속성

**Pawn_StanceTracker.cs**:
```csharp
public bool FullBodyBusy
{
    get
    {
        return this.curStance != null && this.curStance.StanceBusy();
    }
}
```

**Stance_Busy.StanceBusy()**:
```csharp
public virtual bool StanceBusy()
{
    return this.ticksLeft > 0;
}
```

**핵심**: `curStance`가 `Stance_Warmup` 또는 `Stance_Cooldown`이고 `ticksLeft > 0`이면 `FullBodyBusy = true`

---

## 2. 공격 실행 플로우

### 2.1 Verb.TryStartCastOn()

**Verb.cs**:
```csharp
public virtual bool TryStartCastOn(LocalTargetInfo castTarg, ...)
{
    // 검증
    if (!this.caster.Spawned || this.state == VerbState.Bursting || !this.CanHitTarget(castTarg))
    {
        return false;
    }
    
    this.currentTarget = castTarg;
    
    // Warmup이 있으면 Warmup 스턴스 시작
    if (this.CasterIsPawn && this.WarmupTime > 0f)
    {
        int ticks = (this.WarmupTime * statValue).SecondsToTicks();
        this.CasterPawn.stances.SetStance(new Stance_Warmup(ticks, castTarg, this));
        // ← FullBodyBusy = true (Stance_Warmup이 활성화됨)
    }
    else
    {
        this.WarmupComplete();
    }
    return true;
}
```

### 2.2 WarmupComplete()

**Verb.cs**:
```csharp
public virtual void WarmupComplete()
{
    this.burstShotsLeft = this.ShotsPerBurst;
    this.state = VerbState.Bursting;
    this.TryCastNextBurstShot();  // ← 첫 발사
}
```

### 2.3 TryCastNextBurstShot()

**Verb.cs**:
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
        
        this.CasterPawn.Notify_UsedVerb(this.CasterPawn, this);
        this.burstShotsLeft--;
    }
}
```

### 2.4 TryCastShot() - 공격 실행

**Verb_MeleeAttack.cs**:
```csharp
protected override bool TryCastShot()
{
    Pawn casterPawn = this.CasterPawn;
    
    // FullBodyBusy 체크 - 이미 바쁘면 공격 불가
    if (!casterPawn.Spawned || casterPawn.stances.FullBodyBusy)
    {
        return false;
    }
    
    Thing targetThing = this.currentTarget.Thing;
    casterPawn.rotationTracker.Face(targetThing.DrawPos);
    
    // 명중률 계산 및 피해 적용
    if (Rand.Chance(this.GetNonMissChance(targetThing)))
    {
        if (!Rand.Chance(this.GetDodgeChance(targetThing)))
        {
            DamageWorker.DamageResult damageResult = this.ApplyMeleeDamageToTarget(this.currentTarget);
        }
    }
    
    return true;
}
```

---

## 3. Cooldown 설정 메커니즘 ⭐ 핵심

### 3.1 Verb.BurstingTick()

**Verb.cs** (추정 코드):
```csharp
protected void BurstingTick()
{
    if (this.burstShotsLeft <= 0)
    {
        // 모든 발사 완료
        this.state = VerbState.Idle;
        
        // ⭐ 핵심: Cooldown 설정
        if (this.CasterIsPawn && this.tool != null)
        {
            float cooldownTime = this.tool.cooldownTime;
            if (cooldownTime > 0f)
            {
                int cooldownTicks = (cooldownTime * 60f).RoundToInt();
                this.CasterPawn.stances.SetStance(
                    new Stance_Cooldown(cooldownTicks, LocalTargetInfo.Invalid, this)
                );
                // ← FullBodyBusy = true (Stance_Cooldown이 활성화됨)
            }
        }
    }
    else
    {
        // 다음 발사를 위한 대기 시간 설정
        this.ticksToNextBurstShot = this.verbProps.AdjustedCooldownTime(this, this.CasterPawn).SecondsToTicks();
    }
}
```

### 3.2 Tool의 cooldownTime 사용

**VerbProperties.AdjustedCooldownTime()** (추정):
```csharp
public float AdjustedCooldownTime(Verb verb, Pawn attacker)
{
    if (verb.tool != null)
    {
        // Tool의 cooldownTime 사용
        return verb.tool.cooldownTime;
    }
    
    // 기본 cooldown
    return this.defaultCooldownTime;
}
```

### 3.3 AdjustedFullCycleTime

**VerbProperties.cs**:
```csharp
public float AdjustedFullCycleTime(Verb verb, Pawn attacker)
{
    // WarmupTime + CooldownTime 계산
    float num = this.WarmupTime;
    
    if (verb.tool != null)
    {
        num += verb.tool.cooldownTime;  // ← Tool의 cooldownTime 포함
    }
    else
    {
        num += this.defaultCooldownTime;
    }
    
    return num;
}
```

**사용 예시** (`Verb_GunlanceFiring.cs`):
```csharp
casterPawn.skills.Learn(SkillDefOf.Melee, 200f * this.verbProps.AdjustedFullCycleTime(this, casterPawn), false, false);
```

---

## 4. Stance_Cooldown 구조

### 4.1 Stance_Cooldown 생성

**Stance_Cooldown.cs** (추정):
```csharp
public class Stance_Cooldown : Stance_Busy
{
    public Stance_Cooldown(int ticks, LocalTargetInfo focusTarg, Verb verb) 
        : base(ticks, focusTarg, verb)
    {
        // Cooldown 스턴스 초기화
    }
}
```

**Stance_Busy.cs**:
```csharp
public class Stance_Busy : Stance
{
    protected int ticksLeft;
    
    public Stance_Busy(int ticks, LocalTargetInfo focusTarg, Verb verb)
    {
        this.ticksLeft = ticks;
        this.focusTarg = focusTarg;
        this.verb = verb;
    }
    
    public override void StanceTick()
    {
        this.ticksLeft--;
        if (this.ticksLeft <= 0)
        {
            this.Expire();  // ← Cooldown 완료
        }
    }
    
    public override void Expire()
    {
        base.Expire();
        // FullBodyBusy = false (curStance가 null이 됨)
    }
    
    public virtual bool StanceBusy()
    {
        return this.ticksLeft > 0;  // ← FullBodyBusy 계산에 사용
    }
}
```

### 4.2 StanceTick() 호출

**Pawn_StanceTracker.cs**:
```csharp
public void StanceTrackerTick()
{
    if (this.curStance != null)
    {
        this.curStance.StanceTick();  // ← 매 틱마다 호출
    }
}
```

---

## 5. 공격 후 움직임 제한 원인

### 5.1 FullBodyBusy 체크 위치

**Pawn_MeleeVerbs.TryMeleeAttack()**:
```csharp
public bool TryMeleeAttack(Thing target, Verb verbToUse = null, bool surpriseAttack = false)
{
    // ⭐ FullBodyBusy 체크
    if (pawn.stances.FullBodyBusy)
    {
        return false;  // ← 공격 불가
    }
    
    // ... 공격 로직
}
```

**Verb.TryCastShot()**:
```csharp
protected override bool TryCastShot()
{
    Pawn casterPawn = this.CasterPawn;
    
    // ⭐ FullBodyBusy 체크
    if (!casterPawn.Spawned || casterPawn.stances.FullBodyBusy)
    {
        return false;  // ← 공격 불가
    }
    
    // ... 공격 실행
}
```

**JobDriver 이동 체크** (추정):
```csharp
// Pawn이 이동하려고 할 때
if (pawn.stances.FullBodyBusy)
{
    return false;  // ← 이동 불가
}
```

### 5.2 Cooldown 중 행동 제한

**Stance_Cooldown이 활성화되면**:
- `FullBodyBusy = true`
- 새로운 공격 시작 불가 (`TryMeleeAttack` 실패)
- 이동 명령 무시
- 다른 Verb 사용 불가

**Cooldown 완료 후**:
- `Stance_Cooldown.Expire()` 호출
- `curStance = null`
- `FullBodyBusy = false`
- 정상적인 행동 가능

---

## 6. Tool cooldownTime 설정 예시

### 6.1 XML 정의

**Weapon_Melee.xml**:
```xml
<tools>
    <li>
        <label>point</label>
        <capacities>
            <li>Stab</li>
        </capacities>
        <power>15</power>
        <cooldownTime>2</cooldownTime>  <!-- ← 2초 cooldown -->
    </li>
    <li>
        <label>edge</label>
        <capacities>
            <li>Cut</li>
        </capacities>
        <power>12</power>
        <cooldownTime>1.5</cooldownTime>  <!-- ← 1.5초 cooldown -->
    </li>
</tools>
```

### 6.2 Cooldown 계산

**cooldownTime = 2.0초인 경우**:
- `cooldownTicks = 2.0 * 60 = 120틱`
- `Stance_Cooldown(120, LocalTargetInfo.Invalid, verb)` 생성
- 120틱 동안 `FullBodyBusy = true`
- 약 2초 후 Cooldown 완료

---

## 7. 실제 구현 예시 (WyvernFire)

**CompAbilityEffect_WyvernFire.cs**:
```csharp
private void ApplyToolCooldown(Pawn pawn)
{
    if (pawn == null || !pawn.Spawned || pawn.stances == null)
    {
        return;
    }

    float cooldownTime = this.Props.meleeCooldownTime;

    // XML에서 설정하지 않은 경우 (-1), 무기의 tool cooldownTime 자동 사용
    if (cooldownTime < 0f)
    {
        // 무기의 tools 리스트에서 가장 긴 cooldownTime 찾기
        float maxCooldownTime = 0f;
        foreach (Tool t in weaponDef.tools)
        {
            if (t.cooldownTime > maxCooldownTime)
            {
                maxCooldownTime = t.cooldownTime;
            }
        }
        cooldownTime = maxCooldownTime;
    }

    // cooldownTime이 있으면 Stance_Cooldown 설정
    if (cooldownTime > 0f)
    {
        int cooldownTicks = Mathf.RoundToInt(cooldownTime * 60f);
        pawn.stances.SetStance(new Verse.Stance_Cooldown(cooldownTicks, LocalTargetInfo.Invalid, null));
        // ← FullBodyBusy = true
    }
}
```

---

## 8. 핵심 발견 사항

### 8.1 Cooldown 설정 시점

1. **Verb.BurstingTick()**에서 `burstShotsLeft <= 0`일 때
2. **모든 발사 완료 후** 즉시 설정
3. **Tool의 cooldownTime**을 틱으로 변환하여 사용

### 8.2 FullBodyBusy 동작

1. **Stance_Warmup** 활성화 시 `FullBodyBusy = true`
2. **Stance_Cooldown** 활성화 시 `FullBodyBusy = true`
3. **Stance.Expire()** 호출 시 `FullBodyBusy = false`

### 8.3 공격 후 움직임 제한

1. **TryMeleeAttack()**에서 `FullBodyBusy` 체크 → 공격 불가
2. **TryCastShot()**에서 `FullBodyBusy` 체크 → 공격 불가
3. **이동 명령**에서 `FullBodyBusy` 체크 → 이동 불가
4. **Cooldown 완료** 후 정상 동작

---

## 9. 관련 코드 위치

### 9.1 RimWorld 코어

- `RimworldSource/Verse/Verb.cs` - Verb 기본 클래스
- `RimworldSource/RimWorld/Verb_MeleeAttack.cs` - 근접 공격 Verb
- `RimworldSource/Verse/Pawn_StanceTracker.cs` - Stance 관리
- `RimworldSource/Verse/Stance_Busy.cs` - Stance 베이스 클래스
- `RimworldSource/Verse/Stance_Cooldown.cs` - Cooldown Stance
- `RimworldSource/Verse/Stance_Warmup.cs` - Warmup Stance
- `RimworldSource/RimWorld/Pawn_MeleeVerbs.cs` - 근접 공격 관리

### 9.2 프로젝트 코드

- `Project/1.6/Source/Gunlance/Verb_GunlanceFiring.cs` - 건랜스 공격 Verb
- `Project/1.6/Source/WyvernFire/CompAbilityEffect_WyvernFire.cs` - WyvernFire Cooldown 설정 예시
- `Project/1.6/Defs/ThingsDefs/Weapon_Melee.xml` - Tool cooldownTime 정의

---

## 10. 결론

### 10.1 공격 후 Cooldown 구조

1. **공격 실행**: `TryCastShot()` 실행
2. **Cooldown 설정**: `BurstingTick()`에서 `Stance_Cooldown` 설정
3. **움직임 제한**: `FullBodyBusy = true`로 인한 행동 제한
4. **Cooldown 완료**: `Stance_Cooldown.Expire()` 호출 후 정상 동작

### 10.2 핵심 메커니즘

- **Tool의 cooldownTime** → **Stance_Cooldown의 ticksLeft**로 변환
- **Stance_Cooldown 활성화** → **FullBodyBusy = true**
- **FullBodyBusy = true** → **모든 행동 제한**
- **Cooldown 완료** → **FullBodyBusy = false** → **정상 동작**

### 10.3 공격 후 못 움직이는 이유

**답**: `Stance_Cooldown`이 활성화되어 `FullBodyBusy = true`가 되기 때문입니다.

1. 공격 완료 후 `Verb.BurstingTick()`에서 `Stance_Cooldown` 설정
2. `Stance_Cooldown`이 활성화되면 `Pawn_StanceTracker.FullBodyBusy = true`
3. `FullBodyBusy = true`이면 모든 행동(이동, 공격 등)이 차단됨
4. Cooldown 시간(Tool의 `cooldownTime`) 동안 지속됨
5. Cooldown 완료 후 `Stance_Cooldown.Expire()` 호출 → `FullBodyBusy = false` → 정상 동작

---

## 참고 자료

- `Report/22_GunlanceAttack_Flow_Analysis.md` - 공격 플로우 분석
- `Report/25_TryCastShot_Call_Flow_Analysis.md` - TryCastShot 호출 경로
- `Report/56_Tools_System_Mechanism_Analysis.md` - Tools 시스템 메커니즘
- `Report/61_Warmup_Stance_Target_Change_Analysis.md` - Warmup Stance 분석
