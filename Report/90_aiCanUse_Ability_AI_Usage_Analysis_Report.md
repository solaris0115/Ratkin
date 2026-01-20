# aiCanUse 플래그 AI 어빌리티 사용 분석 보고서

**작성일**: 2025-01-27  
**분석 목적**: `aiCanUse` 플래그가 설정된 어빌리티가 AI에 의해 언제, 어떻게 사용되는지 분석

## 태그
aiCanUse Ability AI Usage JobGiver ThinkTree AICanTargetNow

---

## 1. 개요

`aiCanUse`는 `AbilityDef`에 정의된 boolean 필드로, AI가 해당 어빌리티를 사용할 수 있는지 여부를 결정합니다.

### 1.1 정의 위치
- **파일**: `RimworldSource/RimWorld/AbilityDef.cs` (61번째 줄)
- **타입**: `public bool aiCanUse;`
- **기본값**: `false` (명시적으로 설정하지 않으면 AI가 사용하지 않음)

---

## 2. aiCanUse 체크 메커니즘

### 2.1 핵심 체크 지점: `Ability.AICanTargetNow()`

**파일**: `RimworldSource/RimWorld/Ability.cs` (444-469번째 줄)

```csharp
public virtual bool AICanTargetNow(LocalTargetInfo target)
{
    // ✅ 첫 번째 체크: aiCanUse 플래그 확인
    if (!this.def.aiCanUse || !this.CanCast)
    {
        return false;
    }
    
    // 타겟에 적용 가능한지 확인
    if (!this.CanApplyOn(target))
    {
        return false;
    }
    
    // 각 EffectComp의 AICanTargetNow 확인
    if (this.EffectComps != null)
    {
        foreach (CompAbilityEffect comp in this.EffectComps)
        {
            if (!comp.AICanTargetNow(target))
            {
                return false;
            }
        }
        return true;
    }
    return true;
}
```

**체크 순서**:
1. ✅ **`def.aiCanUse`가 `true`인지 확인** (필수)
2. ✅ `CanCast` 확인 (쿨다운, 차지 등)
3. ✅ `CanApplyOn(target)` 확인
4. ✅ 각 `CompAbilityEffect`의 `AICanTargetNow()` 확인

---

## 3. AI 어빌리티 사용 경로

### 3.1 전투 중 어빌리티 사용: `JobGiver_AIFightEnemy`

**파일**: `RimworldSource/RimWorld/JobGiver_AIFightEnemy.cs` (214번째 줄)

```csharp
// 전투 중 적을 타겟으로 하는 공격형 어빌리티 사용
List<Ability> list = pawn.abilities.AICastableAbilities(enemyTarget, true);
```

**작동 조건**:
- `offensive = true`: 공격형 어빌리티만 선택
- `ability.def.ai_IsOffensive == true`인 어빌리티만 포함
- `AICanTargetNow(target)` 또는 `AICanTargetNow(pawn)`이 `true`여야 함

**사용 시점**:
- Pawn이 적과 전투 중일 때
- `JobGiver_AIFightEnemy.TryGiveJob()` 호출 시
- 적을 직접 타겟으로 할 수 있는 어빌리티 우선 사용
- AOE 어빌리티는 두 번째로 고려
- 자기 자신에게 시전 가능한 어빌리티는 세 번째로 고려

---

### 3.2 비전투 어빌리티 사용: `JobGiver_NPCCastNonCombatAbilities`

**파일**: `RimworldSource/RimWorld/JobGiver_NPCCastNonCombatAbilities.cs`

```csharp
// 비전투 상황에서 자기 자신에게 시전하는 어빌리티 사용
List<Ability> list = pawn.abilities.AICastableAbilities(pawn, false);
```

**작동 조건**:
- `offensive = false`: 비공격형 어빌리티만 선택
- `ability.def.ai_IsOffensive == false`인 어빌리티만 포함
- `AICanTargetNow(pawn)`이 `true`여야 함

**사용 시점**:
- 전투 중이 아닐 때
- ThinkTree에서 `JobGiver_NPCCastNonCombatAbilities`가 호출될 때
- 랜덤하게 하나의 어빌리티 선택하여 사용

---

### 3.3 특정 어빌리티 강제 사용: `JobGiver_AICastAbility`

**파일**: `RimworldSource/RimWorld/JobGiver_AICastAbility.cs`

```csharp
public abstract class JobGiver_AICastAbility : ThinkNode_JobGiver
{
    protected AbilityDef ability;  // 특정 어빌리티 지정
    
    protected override Job TryGiveJob(Pawn pawn)
    {
        Ability ability2 = pawn.abilities.GetAbility(this.ability, false);
        if (ability2 == null || !ability2.CanCast)
        {
            return null;
        }
        LocalTargetInfo target = this.GetTarget(pawn, ability2);
        if (!target.IsValid)
        {
            return null;
        }
        return ability2.GetJob(target, target);
    }
}
```

**특징**:
- 특정 `AbilityDef`를 직접 지정하여 사용
- `aiCanUse` 체크는 `GetJob()` 내부에서 `AICanTargetNow()`를 통해 간접적으로 확인됨
- ThinkTree에서 특정 어빌리티를 강제로 사용하고 싶을 때 사용

**예시**: `JobGiver_AICastAbilityOnSelf`
- 자기 자신에게 시전하는 어빌리티 전용 JobGiver
- ThinkTree에서 특정 어빌리티를 지정하여 사용 (예: `SmokepopMech`)

---

## 4. AICastableAbilities 메서드 분석

**파일**: `RimworldSource/RimWorld/Pawn_AbilityTracker.cs` (155-186번째 줄)

```csharp
public List<Ability> AICastableAbilities(LocalTargetInfo target, bool offensive)
{
    this.tmpAbilities.Clear();
    foreach (Ability ability in this.AllAbilitiesForReading)
    {
        // 동물의 경우 훈련된 어빌리티는 제외
        bool flag = true;
        if (this.pawn.IsAnimal)
        {
            // ... 동물 훈련 체크 로직 ...
        }
        
        // ✅ 핵심 필터링 조건
        if (flag && 
            ability.def.ai_IsOffensive == offensive && 
            (ability.AICanTargetNow(target) || ability.AICanTargetNow(this.pawn)))
        {
            this.tmpAbilities.Add(ability);
        }
    }
    return this.tmpAbilities;
}
```

**필터링 조건**:
1. ✅ `ability.def.ai_IsOffensive == offensive`: 공격형/비공격형 일치
2. ✅ `ability.AICanTargetNow(target)`: 타겟에 시전 가능
3. ✅ 또는 `ability.AICanTargetNow(this.pawn)`: 자기 자신에게 시전 가능

**중요**: `AICanTargetNow()` 내부에서 `def.aiCanUse`를 체크하므로, **`aiCanUse=false`인 어빌리티는 이 리스트에 포함되지 않음**

---

## 5. 실제 사용 예시

### 5.1 RimWorld 기본 어빌리티 예시

**Biotech DLC - Gene Abilities**:
```xml
<AbilityDef>
    <defName>FireSpew</defName>
    <aiCanUse>true</aiCanUse>
    <ai_IsOffensive>true</ai_IsOffensive>
    <!-- ... -->
</AbilityDef>
```

**Odyssey DLC - Weapon Trait Abilities**:
```xml
<AbilityDef>
    <defName>SmokepopMech</defName>
    <aiCanUse>true</aiCanUse>
    <!-- ThinkTree에서 JobGiver_AICastAbilityOnSelf로 직접 사용 -->
</AbilityDef>
```

### 5.2 ThinkTree에서의 사용

**Mech ThinkTree 예시** (`RimworldData/Biotech/Defs/ThinkTreeDefs/SubTrees_Mech.xml`):
```xml
<li Class="ThinkNode_ConditionalShotRecently">
    <thresholdTicks>600</thresholdTicks>
    <subNodes>
        <li Class="ThinkNode_ConditionalTotalDamage">
            <thresholdPercent>0.25</thresholdPercent>
            <subNodes>
                <!-- 특정 어빌리티를 직접 지정하여 사용 -->
                <li Class="JobGiver_AICastAbilityOnSelf">
                    <ability>SmokepopMech</ability>
                </li>
            </subNodes>
        </li>
    </subNodes>
</li>
```

**조건**:
- 최근 600틱 이내에 공격받았고
- 총 피해가 25% 이상일 때
- `SmokepopMech` 어빌리티를 자기 자신에게 시전

---

## 6. RK_Ability_MeleeMoraleBooster 적용 분석

### 6.1 현재 설정

```xml
<AbilityDef ParentName="RoleAuraBuffBase">
    <defName>RK_Ability_MeleeMoraleBooster</defName>
    <aiCanUse>true</aiCanUse>
    <casterMustBeCapableOfViolence>false</casterMustBeCapableOfViolence>
    <!-- ai_IsOffensive는 부모 클래스에서 상속됨 -->
</AbilityDef>
```

### 6.2 예상 동작

#### ✅ 자동 사용 가능한 경로

1. **전투 중 자동 사용** (`JobGiver_AIFightEnemy`):
   - `ai_IsOffensive`가 `true`인 경우: 전투 중 적을 타겟으로 사용 시도
   - `ai_IsOffensive`가 `false`인 경우: 전투 중에는 사용하지 않음

2. **비전투 중 자동 사용** (`JobGiver_NPCCastNonCombatAbilities`):
   - `ai_IsOffensive`가 `false`인 경우: 비전투 상황에서 자기 자신에게 시전
   - ThinkTree에 `JobGiver_NPCCastNonCombatAbilities`가 포함되어 있어야 함

#### ⚠️ 제한사항

1. **ThinkTree 의존성**:
   - `JobGiver_AIFightEnemy`는 기본 Humanlike ThinkTree에 포함됨
   - `JobGiver_NPCCastNonCombatAbilities`는 기본 ThinkTree에 포함되어 있지 않을 수 있음

2. **ai_IsOffensive 설정 필요**:
   - `RoleAuraBuffBase`에서 상속되는 값 확인 필요
   - 사기 진작은 버프이므로 `ai_IsOffensive=false`가 적절할 수 있음

3. **타겟팅 조건**:
   - `targetRequired=false` 또는 `canTargetSelf=true`여야 자기 자신에게 시전 가능
   - 현재 설정: `onlyApplyToSelf=True`이므로 자기 자신에게 시전 가능

---

## 7. 작동 조건 요약

### 7.1 aiCanUse=true만으로는 부족함

`aiCanUse=true`만 설정하면:
- ✅ `AICanTargetNow()` 체크를 통과할 수 있음
- ❌ **하지만 자동으로 사용되지는 않음**
- ❌ ThinkTree에 적절한 JobGiver가 있어야 함

### 7.2 완전한 자동 사용을 위한 조건

1. ✅ `aiCanUse=true` 설정
2. ✅ `ai_IsOffensive` 적절히 설정 (전투/비전투 구분)
3. ✅ ThinkTree에 적절한 JobGiver 포함:
   - 전투용: `JobGiver_AIFightEnemy` (기본 포함)
   - 비전투용: `JobGiver_NPCCastNonCombatAbilities` (기본 포함 여부 확인 필요)
   - 특정 어빌리티: `JobGiver_AICastAbilityOnSelf` + ThinkTree에 추가 필요

---

## 8. 권장 사항

### 8.1 RK_Ability_MeleeMoraleBooster의 경우

**옵션 1: 기본 AI 시스템 활용**
- `aiCanUse=true` ✅ (이미 설정됨)
- `ai_IsOffensive=false` 확인/설정 (버프이므로 비공격형)
- `JobGiver_NPCCastNonCombatAbilities`가 기본 ThinkTree에 포함되어 있는지 확인
- 전투 중 사용을 원하면 `JobGiver_AIFightEnemy`가 자기 시전 어빌리티도 고려하는지 확인

**옵션 2: 커스텀 JobGiver 사용 (이전 구현)**
- `JobGiver_AICastMeleeMoraleBooster` 생성
- ThinkTree에 직접 추가
- 전투 중 사용 조건 명시적 제어 가능

**옵션 3: ThinkTree에 직접 추가**
- `JobGiver_AICastAbilityOnSelf` 사용
- 특정 조건(전투 중, 피해 받았을 때 등)에 사용하도록 ThinkTree에 추가

---

## 9. 참고 파일

- `RimworldSource/RimWorld/AbilityDef.cs`: `aiCanUse` 필드 정의
- `RimworldSource/RimWorld/Ability.cs`: `AICanTargetNow()` 메서드
- `RimworldSource/RimWorld/Pawn_AbilityTracker.cs`: `AICastableAbilities()` 메서드
- `RimworldSource/RimWorld/JobGiver_AIFightEnemy.cs`: 전투 중 어빌리티 사용
- `RimworldSource/RimWorld/JobGiver_NPCCastNonCombatAbilities.cs`: 비전투 어빌리티 사용
- `RimworldSource/RimWorld/JobGiver_AICastAbility.cs`: 특정 어빌리티 강제 사용
- `RimworldSource/RimWorld/JobGiver_AICastAbilityOnSelf.cs`: 자기 시전 어빌리티

---

## 10. 결론

`aiCanUse=true`는 AI가 어빌리티를 사용할 수 있는 **필수 조건**이지만, **충분 조건은 아닙니다**.

**실제 사용을 위해서는**:
1. ✅ `aiCanUse=true` 설정
2. ✅ `ai_IsOffensive` 적절히 설정
3. ✅ ThinkTree에 적절한 JobGiver 포함
4. ✅ `AICanTargetNow()` 조건 충족 (쿨다운, 타겟팅 등)

**RK_Ability_MeleeMoraleBooster의 경우**, `aiCanUse=true`만으로는 자동 사용이 보장되지 않을 수 있으므로, 테스트 후 필요시 커스텀 JobGiver나 ThinkTree 수정이 필요할 수 있습니다.
