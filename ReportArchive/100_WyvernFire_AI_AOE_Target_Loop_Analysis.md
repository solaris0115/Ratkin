# WyvernFire AI AOE 타겟 무한 루프 분석 보고서

**작성일**: 2026-01-24  
**분석 목적**: 적대 AI Pawn이 `RK_WyvernFire_Ability`를 사용할 때 `ai_SearchAOEForTargets`가 true일 경우 사거리 밖 대상을 조준하고 취소를 반복하는 문제 분석

**태그**: WyvernFire Ability AI AOE Targeting Range Check Loop Issue

---

## 1. 문제 현상

적대 AI Pawn이 `RK_WyvernFire_Ability`를 가지고 있고 `ai_SearchAOEForTargets`가 `true`일 때:
- 사거리 밖의 대상을 조준하려고 시도
- Warmup 중에 타겟이 사거리 밖임을 감지하여 취소
- 다시 같은 타겟을 찾아 조준 시도
- **무한 반복 발생**

---

## 2. AI 어빌리티 사용 플로우 분석

### 2.1 전체 플로우 다이어그램

```
[매 틱마다 실행]
JobGiver_AIFightEnemy.TryGiveJob()
    ↓
GetAbilityJob(pawn, enemyTarget)
    ↓
AICastableAbilities() - 사용 가능한 어빌리티 목록 수집
    ↓
[1단계] 직접 타겟 사거리 확인 (223줄)
    for (int i = 0; i < list.Count; i++)
        if (list[i].verb.CanHitTarget(enemyTarget))
            return list[i].GetJob(enemyTarget, enemyTarget);
    ↓
[2단계] AOE 타겟 검색 (228-234줄) ⚠️ 문제 발생 지점
    for (int j = 0; j < list.Count; j++)
        LocalTargetInfo localTargetInfo = list[j].AIGetAOETarget();
        if (localTargetInfo.IsValid)
            return list[j].GetJob(localTargetInfo, localTargetInfo);
    ↓
[3단계] Self 타겟 검색 (236-242줄)
    for (int k = 0; k < list.Count; k++)
        if (list[k].verb.targetParams.canTargetSelf)
            return list[k].GetJob(pawn, pawn);
```

### 2.2 핵심 코드 위치

**파일**: `RimworldSource/RimWorld/JobGiver_AIFightEnemy.cs:208-245`

```csharp
public static Job GetAbilityJob(Pawn pawn, Thing enemyTarget)
{
    // ... 생략 ...
    List<Ability> list = pawn.abilities.AICastableAbilities(enemyTarget, true);
    
    // 1단계: 직접 타겟이 사거리 내인지 확인
    for (int i = 0; i < list.Count; i++)
    {
        if (list[i].verb.CanHitTarget(enemyTarget))
        {
            return list[i].GetJob(enemyTarget, enemyTarget);
        }
    }
    
    // 2단계: AOE 타겟 검색 ⚠️ 여기서 문제 발생
    for (int j = 0; j < list.Count; j++)
    {
        LocalTargetInfo localTargetInfo = list[j].AIGetAOETarget();
        if (localTargetInfo.IsValid)
        {
            return list[j].GetJob(localTargetInfo, localTargetInfo);
        }
    }
    
    // 3단계: Self 타겟 검색
    // ... 생략 ...
}
```

---

## 3. Ability.AIGetAOETarget() 기본 구현 분석

### 3.1 기본 구현 코드

**파일**: `RimworldSource/RimWorld/Ability.cs:471-499`

```csharp
public virtual LocalTargetInfo AIGetAOETarget()
{
    if (this.def.ai_SearchAOEForTargets)
    {
        // 사거리 내의 모든 Thing을 순회
        foreach (Thing thing in GenRadial.RadialDistinctThingsAround(
            this.pawn.Position, 
            this.pawn.Map, 
            this.verb.EffectiveRange,  // ⚠️ 사거리 기준으로 검색
            true))
        {
            if (this.ValidAOEAffectedTarget(thing))
            {
                bool flag = true;
                // EffectComp들이 타겟 가능한지 확인
                foreach (CompAbilityEffect effect in this.EffectComps)
                {
                    if (!effect.AICanTargetNow(thing))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    return thing;  // ⚠️ 사거리 검증 없이 Thing 반환
                }
            }
        }
    }
    return LocalTargetInfo.Invalid;
}
```

### 3.2 문제점

1. **사거리 검증 부재**: `GenRadial.RadialDistinctThingsAround`는 `EffectiveRange`를 기준으로 검색하지만, 이는 **원형 범위**입니다.
2. **Thing 반환**: Thing 자체를 반환하는데, Thing의 **위치**가 사거리 내인지는 확인하지 않습니다.
3. **Verb 사거리와 불일치**: `EffectiveRange`는 원형이지만, 실제 Verb의 사거리는 **원뿔형**일 수 있습니다 (WyvernFire의 경우).

---

## 4. Ability_WyvernFire.AIGetAOETarget() 오버라이드 분석

### 4.1 현재 구현 코드

**파일**: `Project/1.6/Source/WyvernFire/Ability_WyvernFire.cs:217-249`

```csharp
public override LocalTargetInfo AIGetAOETarget()
{
    if (this.def.ai_SearchAOEForTargets)
    {
        // 기본 AOE 타겟 검색
        LocalTargetInfo baseTarget = base.AIGetAOETarget();
        
        // 타겟을 찾았지만 사거리 밖에 있는 경우 무한 루프 방지
        if (baseTarget.IsValid)
        {
            // Thing 타겟인 경우 해당 Thing의 위치가 사거리 내인지 확인
            if (baseTarget.HasThing)
            {
                LocalTargetInfo thingLocation = baseTarget.Thing.Position;
                if (this.verb.CanHitTarget(thingLocation))
                {
                    return thingLocation; // 위치를 타겟으로 반환 (canTargetLocations: true)
                }
            }
            // Cell 타겟인 경우 사거리 확인
            else if (baseTarget.Cell.IsValid)
            {
                if (this.verb.CanHitTarget(baseTarget))
                {
                    return baseTarget;
                }
            }
            // 사거리 밖이면 Invalid 반환하여 무한 루프 방지
            return LocalTargetInfo.Invalid;
        }
    }
    return LocalTargetInfo.Invalid;
}
```

### 4.2 문제점 분석

1. **로직은 올바름**: 사거리 검증을 추가하여 무한 루프를 방지하려고 시도함
2. **하지만 여전히 문제 발생**: 왜 여전히 무한 루프가 발생하는가?

---

## 5. Job 생성 및 Warmup 플로우 분석

### 5.1 Ability.GetJob() 호출

**파일**: `RimworldSource/RimWorld/Ability.cs:698-707`

```csharp
public virtual Job GetJob(LocalTargetInfo target, LocalTargetInfo destination)
{
    Job job = JobMaker.MakeJob(this.def.jobDef ?? JobDefOf.CastAbilityOnThing);
    job.verbToUse = this.verb;
    job.targetA = target;  // ⚠️ 타겟 설정
    job.targetB = destination;
    job.ability = this;
    this.needToRecacheWarmupMotes = true;
    return job;
}
```

### 5.2 JobDriver_CastAbility 실행

Job이 생성되면 `JobDriver_CastAbility`가 실행됩니다. 이 JobDriver는:
1. 타겟 위치로 이동 (필요시)
2. `Verb.TryStartCastOn()` 호출
3. Warmup 시작

### 5.3 Verb.TryStartCastOn() 호출

**파일**: `RimworldSource/Verse/Verb.cs:481-532`

```csharp
public virtual bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, ...)
{
    // 검증
    if (this.caster == null || !this.caster.Spawned)
        return false;
    
    if (this.state == VerbState.Bursting || !this.CanHitTarget(castTarg))  // ⚠️ 여기서 검증
        return false;
    
    this.currentTarget = castTarg;
    this.currentDestination = destTarg;
    
    // Warmup 시작
    if (this.CasterIsPawn && this.WarmupTime > 0f)
    {
        int ticks = (this.WarmupTime * statValue).SecondsToTicks();
        this.CasterPawn.stances.SetStance(new Stance_Warmup(ticks, castTarg, this));
    }
    // ...
}
```

**문제**: `TryStartCastOn`에서 `CanHitTarget`을 체크하지만, **Job 생성 시점**과 **실제 Warmup 시작 시점** 사이에 타겟이 이동할 수 있습니다.

### 5.4 Stance_Warmup 틱마다 검증

**파일**: `RimworldSource/Verse/Stance_Warmup.cs:220-231`

```csharp
public override void StanceTick()
{
    if (!this.stanceTracker.stunner.Stunned)
    {
        // 타겟이 다운되었는지 확인
        if (!this.targetStartedDowned && this.focusTarg.HasThing && 
            this.focusTarg.Thing is Pawn && ((Pawn)this.focusTarg.Thing).Downed)
        {
            this.stanceTracker.SetStance(new Stance_Mobile());
            return;
        }
        
        // ⚠️ 매 틱마다 사거리 검증
        if (this.focusTarg.HasThing && 
            (!this.focusTarg.Thing.Spawned || 
             this.verb == null || 
             !this.verb.CanHitTargetFrom(base.Pawn.Position, this.focusTarg)))
        {
            this.stanceTracker.SetStance(new Stance_Mobile());  // 취소
            return;
        }
        // ...
    }
    base.StanceTick();
}
```

**핵심**: `Stance_Warmup`는 **매 틱마다** `CanHitTargetFrom`을 체크하여 타겟이 사거리 밖이면 Warmup을 취소합니다.

---

## 6. 무한 루프 발생 원인 분석

### 6.1 시나리오 재현

1. **AI가 AOE 타겟 검색**
   - `base.AIGetAOETarget()` 호출
   - `GenRadial.RadialDistinctThingsAround(EffectiveRange)`로 검색
   - Thing을 찾아 반환

2. **Ability_WyvernFire.AIGetAOETarget()에서 사거리 검증**
   - `this.verb.CanHitTarget(thingLocation)` 호출
   - **문제**: `CanHitTarget`은 Thing의 위치를 확인하지만, **원뿔형 사거리**를 제대로 고려하지 않을 수 있음

3. **Job 생성 및 Warmup 시작**
   - `GetJob(thingLocation, thingLocation)` 호출
   - `JobDriver_CastAbility` 시작
   - `Verb.TryStartCastOn()` 호출 → Warmup 시작

4. **Warmup 중 타겟 검증 실패**
   - `Stance_Warmup.StanceTick()`에서 매 틱마다 `CanHitTargetFrom` 체크
   - 타겟이 사거리 밖이면 Warmup 취소

5. **다시 AOE 타겟 검색**
   - `JobGiver_AIFightEnemy.TryGiveJob()`이 다시 호출됨
   - `AIGetAOETarget()`이 **같은 타겟**을 다시 찾음
   - **무한 루프 발생**

### 6.2 근본 원인

1. **AIGetAOETarget()의 검색 범위와 Verb의 실제 사거리 불일치**
   - `GenRadial.RadialDistinctThingsAround(EffectiveRange)`는 **원형 범위**
   - WyvernFire는 **원뿔형 범위** (range: 5.5, 원뿔 각도)
   - 원형 범위 내의 Thing이지만 원뿔 범위 밖에 있을 수 있음

2. **CanHitTarget 검증 타이밍 문제**
   - `AIGetAOETarget()`에서 `CanHitTarget`을 체크하지만, **Thing의 위치**만 확인
   - Thing이 이동하거나, 원뿔의 방향이 맞지 않으면 사거리 밖이 될 수 있음

3. **Job 생성 후 타겟 변경 불가**
   - Job이 생성되면 타겟이 고정됨
   - Warmup 중에 타겟이 사거리 밖이 되면 취소만 가능
   - AI는 다시 같은 타겟을 찾으려고 시도

---

## 7. Verb.CanHitTarget() 및 CanHitTargetFrom() 분석

### 7.1 CanHitTarget() 구현

**파일**: `RimworldSource/Verse/Verb.cs:802-805`

```csharp
public virtual bool CanHitTarget(LocalTargetInfo targ)
{
    return this.caster != null && this.caster.Spawned && 
           (targ == this.caster || this.CanHitTargetFrom(this.caster.Position, targ));
}
```

### 7.2 CanHitTargetFrom() 구현

**파일**: `RimworldSource/Verse/Verb.cs:885-893`

```csharp
public virtual bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
{
    if (targ.Thing != null && targ.Thing == this.caster)
    {
        return this.targetParams.canTargetSelf;
    }
    ShootLine shootLine;
    return (targ.Pawn == null || !targ.Pawn.IsPsychologicallyInvisible() || 
            !this.caster.HostileTo(targ.Pawn)) && 
           !this.ApparelPreventsShooting() && 
           this.TryFindShootLineFromTo(root, targ, out shootLine, false);
}
```

**핵심**: `TryFindShootLineFromTo`가 실제 사거리 및 LOS를 확인합니다.

### 7.3 TryFindShootLineFromTo()의 역할

이 메서드는:
1. 사거리 내인지 확인
2. LOS (Line of Sight) 확인
3. 원뿔형 범위인 경우 방향 확인

**문제**: `AIGetAOETarget()`에서 `CanHitTarget`을 호출할 때, **Pawn의 현재 방향**을 기준으로 검증하지만, 실제로는 **타겟을 향한 방향**이어야 합니다.

---

## 8. 문제 요약

### 8.1 핵심 문제

1. **검색 범위 불일치**
   - `AIGetAOETarget()`: 원형 범위 (`EffectiveRange`)
   - 실제 Verb: 원뿔형 범위 (WyvernFire)

2. **방향 고려 부재**
   - `AIGetAOETarget()`에서 `CanHitTarget`을 호출할 때, Pawn의 **현재 방향** 기준
   - 실제로는 **타겟을 향한 방향**이어야 원뿔 범위 내에 들어옴

3. **타겟 이동 고려 부재**
   - Job 생성 시점과 Warmup 시작 시점 사이에 타겟이 이동할 수 있음
   - Warmup 중에도 타겟이 이동할 수 있음

### 8.2 현재 Ability_WyvernFire.AIGetAOETarget()의 한계

현재 구현은:
- Thing의 위치를 확인하여 사거리 검증을 시도
- 하지만 **원뿔형 범위의 방향**을 고려하지 않음
- Pawn의 현재 방향 기준으로만 검증

---

## 9. 다음 단계 분석 계획

다음 단계에서는 다음을 분석해야 합니다:

1. **Verb_CastAbility의 TryFindShootLineFromTo 구현**
   - 원뿔형 범위를 어떻게 처리하는지
   - 방향 계산 로직

2. **JobDriver_CastAbility의 동작**
   - 타겟으로 이동하는 로직
   - Warmup 시작 전 타겟 재검증

3. **해결 방안 제시**
   - AIGetAOETarget()에서 방향을 고려한 검증
   - 또는 AOE 타겟 검색 로직 개선

---

## 10. 참고 파일

- `RimworldSource/RimWorld/Ability.cs` - Ability 기본 클래스
- `RimworldSource/RimWorld/JobGiver_AIFightEnemy.cs` - AI 어빌리티 사용 로직
- `RimworldSource/Verse/Stance_Warmup.cs` - Warmup 중 타겟 검증
- `RimworldSource/Verse/Verb.cs` - Verb 기본 클래스
- `Project/1.6/Source/WyvernFire/Ability_WyvernFire.cs` - WyvernFire 어빌리티 구현
- `Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml` - RK_WyvernFire_Ability 정의
