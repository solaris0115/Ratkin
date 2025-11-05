# TryCastShot 호출 경로 및 Tool 선택 로직 분석

**작성일**: 2024-12-29  
**분석 목적**: `TryCastShot`가 상위 단계에서 어떻게 호출되는지 열거하고, Tool 선택 부분 마킹

## TryCastShot 호출 경로 전체 흐름

```
사용자 공격 명령 클릭
    ↓
Job 생성 (JobDefOf.AttackMelee)
    ↓
JobDriver_AttackMelee.MakeNewToils()
    ↓
Toils_Combat.FollowAndMeleeAttack() 생성
    ↓ (매 틱마다 실행되는 tickIntervalAction)
Pawn_MeleeVerbs.TryMeleeAttack()
    ↓
[🔧 Tool 선택 로직] ChooseMeleeVerb() → VerbEntry.GetSelectionWeight()
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
- `VerbTracker.cs` - `GetVerbsCommands()`
- UI에서 공격 버튼 클릭 → Job 생성
- `pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.AttackMelee, target))`

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

### 4. TryMeleeAttack - Verb 선택 시작점

**Pawn_MeleeVerbs.cs** - `TryMeleeAttack` (83-117):
```csharp
public bool TryMeleeAttack(Thing target, Verb verbToUse = null, bool surpriseAttack = false)
{
    if (pawn.stances.FullBodyBusy)
    {
        return false;
    }

    if (!pawn.kindDef.canMeleeAttack)
    {
        return false;
    }

    if (verbToUse != null)
    {
        if (!verbToUse.IsStillUsableBy(pawn))
        {
            return false;
        }

        if (!verbToUse.IsMeleeAttack)
        {
            Log.Warning("Pawn " + pawn?.ToString() + " tried to melee attack " + target?.ToString() + " with non melee-attack verb " + verbToUse?.ToString() + ".");
            return false;
        }
    }

    Verb verb = verbToUse ?? TryGetMeleeVerb(target);  // ← Tool 선택 로직 호출
    if (verb == null)
    {
        return false;
    }

    verb.TryStartCastOn(target, surpriseAttack);
    return true;
}
```

### 5. 🔧 Tool 선택 로직 (TryGetMeleeVerb)

**Pawn_MeleeVerbs.cs** - `TryGetMeleeVerb` (48-56):
```csharp
public Verb TryGetMeleeVerb(Thing target)
{
    if (curMeleeVerb == null || curMeleeVerbTarget != target || 
        Find.TickManager.TicksGame >= curMeleeVerbUpdateTick + 60 || 
        !curMeleeVerb.IsStillUsableBy(pawn) || 
        !curMeleeVerb.IsUsableOn(target))
    {
        ChooseMeleeVerb(target);  // ← 🔧 Tool 선택 로직 진입점
    }

    return curMeleeVerb;
}
```

### 6. 🔧 Tool 선택 로직 (ChooseMeleeVerb) ⭐ 핵심

**Pawn_MeleeVerbs.cs** - `ChooseMeleeVerb` (58-81):
```csharp
private void ChooseMeleeVerb(Thing target)
{
    bool flag = Rand.Chance(0.04f);  // 4% 확률로 지형 기반 Tool 고려
    List<VerbEntry> updatedAvailableVerbsList = GetUpdatedAvailableVerbsList(flag);
    bool flag2 = false;
    
    // 🔧 가중치 기반 Tool 선택 ⭐
    if (updatedAvailableVerbsList.TryRandomElementByWeight((VerbEntry ve) => ve.GetSelectionWeight(target), out var result))
    {
        flag2 = true;
    }
    else if (flag)
    {
        updatedAvailableVerbsList = GetUpdatedAvailableVerbsList(terrainTools: false);
        flag2 = updatedAvailableVerbsList.TryRandomElementByWeight((VerbEntry ve) => ve.GetSelectionWeight(target), out result);
    }

    if (flag2)
    {
        SetCurMeleeVerb(result.verb, target);  // ← 선택된 Verb 저장
        return;
    }

    Log.ErrorOnce(pawn.ToStringSafe() + " has no available melee attack...");
    SetCurMeleeVerb(null, null);
}
```

**핵심**: `TryRandomElementByWeight`에서 `VerbEntry.GetSelectionWeight()`를 사용하여 Tool을 선택합니다!

### 7. 🔧 Tool 선택 로직 (GetUpdatedAvailableVerbsList) - Tool 수집

**Pawn_MeleeVerbs.cs** - `GetUpdatedAvailableVerbsList` (119-239):
```csharp
public List<VerbEntry> GetUpdatedAvailableVerbsList(bool terrainTools)
{
    meleeVerbs.Clear();
    verbsToAdd.Clear();
    
    if (!terrainTools)
    {
        // 🔧 1. Pawn 고유 Verbs (RaceDef의 tools)
        List<Verb> allVerbs = pawn.verbTracker.AllVerbs;
        for (int i = 0; i < allVerbs.Count; i++)
        {
            if (IsUsableMeleeVerb(allVerbs[i]))
            {
                verbsToAdd.Add(allVerbs[i]);
            }
        }

        // 🔧 2. Equipment Verbs (무기의 tools) ⭐
        if (pawn.equipment != null)
        {
            List<ThingWithComps> allEquipmentListForReading = pawn.equipment.AllEquipmentListForReading;
            for (int j = 0; j < allEquipmentListForReading.Count; j++)
            {
                List<Verb> list = allEquipmentListForReading[j].GetComp<CompEquippable>()?.AllVerbs;
                if (list == null)
                {
                    continue;
                }

                for (int k = 0; k < list.Count; k++)
                {
                    if (IsUsableMeleeVerb(list[k]))
                    {
                        verbsToAdd.Add(list[k]);  // ← 무기의 Tool들 추가
                    }
                }
            }
        }

        // 🔧 3. Apparel Verbs (장비의 tools)
        if (pawn.apparel != null)
        {
            List<Apparel> wornApparel = pawn.apparel.WornApparel;
            for (int l = 0; l < wornApparel.Count; l++)
            {
                List<Verb> list2 = wornApparel[l].GetComp<CompEquippable>()?.AllVerbs;
                if (list2 == null)
                {
                    continue;
                }

                for (int m = 0; m < list2.Count; m++)
                {
                    if (IsUsableMeleeVerb(list2[m]))
                    {
                        verbsToAdd.Add(list2[m]);
                    }
                }
            }
        }

        // 🔧 4. Hediffs Verbs (상태이상의 Verbs)
        foreach (Verb hediffsVerb in pawn.health.hediffSet.GetHediffsVerbs())
        {
            if (IsUsableMeleeVerb(hediffsVerb))
            {
                verbsToAdd.Add(hediffsVerb);
            }
        }

        // 🔧 5. Mutant Verbs (Anomaly 모드)
        if (ModsConfig.AnomalyActive && pawn.IsMutant)
        {
            foreach (Verb allVerb in pawn.mutant.AllVerbs)
            {
                if (IsUsableMeleeVerb(allVerb))
                {
                    verbsToAdd.Add(allVerb);
                }
            }
        }
    }
    else if (pawn.Spawned && !pawn.IsSubhuman)
    {
        // 🔧 6. 지형 기반 Verbs (4% 확률)
        TerrainDef terrain = pawn.Position.GetTerrain(pawn.Map);
        if (terrainVerbs == null || terrainVerbs.def != terrain)
        {
            terrainVerbs = Pawn_MeleeVerbs_TerrainSource.Create(this, terrain);
        }

        List<Verb> allVerbs2 = terrainVerbs.tracker.AllVerbs;
        for (int n = 0; n < allVerbs2.Count; n++)
        {
            Verb verb = allVerbs2[n];
            if (IsUsableMeleeVerb(verb))
            {
                verbsToAdd.Add(verb);
            }
        }
    }

    // 🔧 최대 가중치 계산
    float num = 0f;
    foreach (Verb item in verbsToAdd)
    {
        float num2 = VerbUtility.InitialVerbWeight(item, pawn);
        if (num2 > num)
        {
            num = num2;
        }
    }

    // 🔧 VerbEntry 생성 (각 Verb에 대해)
    foreach (Verb item2 in verbsToAdd)
    {
        meleeVerbs.Add(new VerbEntry(item2, pawn, verbsToAdd, num));
    }

    return meleeVerbs;
}
```

**핵심**: 무기를 들고 있어도 **Pawn 고유 Tools(RaceDef)도 함께 선택 대상에 포함**됩니다!

### 8. 🔧 Tool 선택 로직 (VerbEntry.GetSelectionWeight)

**VerbEntry.cs** - `GetSelectionWeight`:
```csharp
public float GetSelectionWeight(Thing target)
{
    // VerbProperties.AdjustedMeleeSelectionWeight() 호출 ⭐
    return verbProperties.AdjustedMeleeSelectionWeight(
        tool, 
        pawn, 
        equipment, 
        hediffCompSource, 
        comesFromPawnNativeVerbs
    );
}
```

### 9. 🔧 Tool 선택 로직 (VerbProperties.AdjustedMeleeSelectionWeight) ⭐ 최종 선택 가중치 계산

**VerbProperties.cs** - `AdjustedMeleeSelectionWeight`:
```csharp
public float AdjustedMeleeSelectionWeight(Tool tool, Pawn attacker, Thing equipment, HediffComp_VerbGiver hediffCompSource, bool comesFromPawnNativeVerbs)
{
    float num = 1f;
    float num2 = this.AdjustedExpectedDamageForVerbUsableInMelee(tool, attacker, equipment, hediffCompSource);
    
    // 🔧 피해량의 제곱을 가중치에 곱함
    if (num2 >= 0.001f || !typeof(Verb_MeleeApplyHediff).IsAssignableFrom(this.verbClass))
    {
        num *= num2 * num2;  // ★ 피해량의 제곱
    }
    
    // 🔧 ManeuverDef의 commonality 적용
    num *= this.commonality;
    
    // 🔧 Tool의 chanceFactor 적용 ⭐
    if (tool != null)
    {
        num *= tool.chanceFactor;  // ★ Tool 선택 확률
    }
    
    // 🔧 Pawn의 기본 Tool인 경우 0.3 배율 적용 ⭐
    if (comesFromPawnNativeVerbs && (tool == null || !tool.alwaysTreatAsWeapon))
    {
        num *= 0.3f;  // ★ Pawn 기본 Tool 감소
    }
    
    return num;
}
```

**가중치 공식**:
```
선택 가중치 = (예상 피해량²) × ManeuverDef.commonality × Tool.chanceFactor × (Pawn 기본 Tool인 경우 0.3)
```

### 10. TryStartCastOn - 캐스팅 시작

**Verb.cs** - `TryStartCastOn`:
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

### 11. WarmupComplete - Burst 시작

**Verb.cs** - `WarmupComplete`:
```csharp
public virtual void WarmupComplete()
{
    this.burstShotsLeft = this.ShotsPerBurst;
    this.state = VerbState.Bursting;
    this.TryCastNextBurstShot();  // ← 첫 발사
}
```

**Stance_Warmup**에서 완료 시 호출:
- `Stance_Warmup.WarmupStanceTick()` → 0이 되면 `WarmupComplete()` 호출

### 12. TryCastNextBurstShot - 발사 처리

**Verb.cs** - `TryCastNextBurstShot`:
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

### 13. TryCastShot - 실제 공격 실행 ⭐

**Verb_GunlanceFiring.cs** - `TryCastShot` (14-68):
```csharp
protected override bool TryCastShot()
{
    // 시전자(공격자) 상태 확인 - 스폰되어 있고 전신이 바쁘지 않은지 체크
    Pawn casterPawn = this.CasterPawn;
    if (!casterPawn.Spawned || casterPawn.stances.FullBodyBusy)
    {
        return false;
    }

    // 타겟 확인 및 근접 사거리 내에 있는지 검증
    Thing targetThing = this.currentTarget.Thing;
    if (!this.CanHitTarget(targetThing))
        Log.Warning($"{casterPawn} meleed {targetThing} from out of melee position.");

    // 공격자가 타겟을 바라보도록 회전
    casterPawn.rotationTracker.Face(targetThing.DrawPos);

    // 타겟이 움직일 수 있고, 시전자에게 스킬 시스템이 있으며, 타겟이 콜로니 메크가 아닌 경우 근접 전투 스킬 경험치 획득
    if (!this.IsTargetImmobile(this.currentTarget) &&
        casterPawn.skills != null &&
        (this.currentTarget.Pawn == null || !this.currentTarget.Pawn.IsColonyMech))
    {
        casterPawn.skills.Learn(SkillDefOf.Melee, 200f * this.verbProps.AdjustedFullCycleTime(this, casterPawn), false, false);
    }

    // 타겟이 생물(Pawn)인 경우, 적대 관계 설정 및 근접 위협으로 인식
    Pawn targetPawn = targetThing as Pawn;
    if (targetPawn != null &&
    !targetPawn.Dead &&
    (casterPawn.MentalStateDef != MentalStateDefOf.SocialFighting || targetPawn.MentalStateDef != MentalStateDefOf.SocialFighting) &&
    (casterPawn.story == null || !casterPawn.story.traits.DisableHostilityFrom(targetPawn)))
    {
        targetPawn.mindState.meleeThreat = casterPawn;
        targetPawn.mindState.lastMeleeThreatHarmTick = Find.TickManager.TicksGame;
    }
    
    // 건랜스 포격은 적중 여부와 관계없이 그쪽으로 발사만 시도함. 적중 판단은 나중에 폭발에서 별도 처리
    verbProperties = verbProps as VerbProperties_Gunlance;
    CreateGunlanceExplosion(targetThing);
    CreateCombatLog((ManeuverDef maneuver) => maneuver.combatLogRulesHit, true);

    // 포격음은 따로없음. 폭발 사운드로 대체됨
    // 공격 애니메이션 알림
    if (casterPawn.Spawned)
    {
        casterPawn.Drawer.Notify_MeleeAttackOn(targetThing);
        casterPawn.rotationTracker.FaceCell(targetThing.Position);
    }

    // 호출자(이벤트 리스너)에게 근접 공격 완료 알림
    if (casterPawn.caller != null) casterPawn.caller.Notify_DidMeleeAttack();
    
    return true;
}
```

## Tool 선택 로직 요약

### 🔧 Tool 선택이 일어나는 위치

1. **Pawn_MeleeVerbs.TryMeleeAttack()** (line 109)
   - `verbToUse ?? TryGetMeleeVerb(target)` 호출

2. **Pawn_MeleeVerbs.TryGetMeleeVerb()** (line 48-56)
   - 조건 불만족 시 `ChooseMeleeVerb(target)` 호출

3. **Pawn_MeleeVerbs.ChooseMeleeVerb()** (line 58-81) ⭐ **핵심 선택 로직**
   - `GetUpdatedAvailableVerbsList()` 호출하여 모든 Tool 수집
   - `TryRandomElementByWeight((VerbEntry ve) => ve.GetSelectionWeight(target), out result)` 호출
   - 가중치 기반 랜덤 선택

4. **Pawn_MeleeVerbs.GetUpdatedAvailableVerbsList()** (line 119-239) ⭐ **Tool 수집**
   - Pawn 고유 Verbs (RaceDef의 tools)
   - Equipment Verbs (무기의 tools) ← **무기 Tool 포함**
   - Apparel Verbs (장비의 tools)
   - Hediffs Verbs (상태이상의 Verbs)
   - Mutant Verbs (Anomaly 모드)
   - 지형 기반 Verbs (4% 확률)

5. **VerbEntry.GetSelectionWeight()** ⭐ **가중치 계산**
   - `VerbProperties.AdjustedMeleeSelectionWeight()` 호출

6. **VerbProperties.AdjustedMeleeSelectionWeight()** ⭐ **최종 가중치 계산**
   - `(예상 피해량²) × ManeuverDef.commonality × Tool.chanceFactor × (Pawn 기본 Tool인 경우 0.3)` 계산

### Tool 선택 타이밍

- **60틱마다** 재선택 (또는 조건 불만족 시 즉시)
- `TryGetMeleeVerb`에서 캐싱된 Verb 사용
- **같은 대상, 60틱 내**면 동일한 Verb 사용

```csharp
if (Find.TickManager.TicksGame >= this.curMeleeVerbUpdateTick + 60)
{
    this.ChooseMeleeVerb(target);  // 재선택
}
```

## 핵심 발견 사항

### 1. Tool 선택은 TryCastShot 이전에 발생

- `TryCastShot` 호출 전에 이미 Tool이 선택되어 `curMeleeVerb`에 저장됨
- `TryCastShot` 내부에서는 선택된 Tool을 사용함 (`this.tool`)

### 2. 무기를 들고 있어도 Pawn 고유 Tools 포함

- 무기 Tool (예: point, edge)
- Pawn 고유 Tool (예: teeth, left fist, right fist) ← **0.3 배율 적용**

### 3. 가중치 기반 랜덤 선택

- `TryRandomElementByWeight`를 사용하여 가중치 기반으로 선택
- 가중치가 높을수록 선택 확률이 높음

### 4. Tool 선택 위치 요약

```
TryMeleeAttack() 
  → TryGetMeleeVerb() 
    → ChooseMeleeVerb() ⭐ Tool 선택 핵심
      → GetUpdatedAvailableVerbsList() ⭐ Tool 수집
        → VerbEntry.GetSelectionWeight() 
          → VerbProperties.AdjustedMeleeSelectionWeight() ⭐ 가중치 계산
```

## 관련 파일

- `Project/1.6/Source/Gunlance/Verb_GunlanceFiring.cs` - TryCastShot 구현
- `Project/1.6/Source/Gunlance/ToolSelectionPatch.cs` - Tool 선택 로깅 패치
- `RimworldSource/RimWorld/Pawn_MeleeVerbs.cs` - Tool 선택 로직
- `RimworldSource/Verse/VerbProperties.cs` - 가중치 계산
- `RimworldSource/Verse/Verb.cs` - TryCastNextBurstShot, WarmupComplete
- `Report/22_GunlanceAttack_Flow_Analysis.md` - 전체 플로우 분석
- `Report/24_Tool_Selection_Probability_Report.md` - Tool 선택 확률 분석

