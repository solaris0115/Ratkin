# Tool 선택 확률 분석 보고서

## 개요
RimWorld에서 근접 공격 시 Tool 선택 로직을 분석하고, Weapon_HighTech.xml과 Ratkin RaceDef의 각 Tool 발동 확률을 계산합니다.

## 1. Tool 선택 로직 분석

### 1.1 RimWorld 소스코드 분석 결과

**파일**: `RimworldSource/Verse/VerbProperties.cs`

#### AdjustedMeleeSelectionWeight 메서드

```csharp
public float AdjustedMeleeSelectionWeight(Tool tool, Pawn attacker, Thing equipment, HediffComp_VerbGiver hediffCompSource, bool comesFromPawnNativeVerbs)
{
    float num = 1f;
    float num2 = this.AdjustedExpectedDamageForVerbUsableInMelee(tool, attacker, equipment, hediffCompSource);
    
    // 피해량의 제곱을 가중치에 곱함
    if (num2 >= 0.001f || !typeof(Verb_MeleeApplyHediff).IsAssignableFrom(this.verbClass))
    {
        num *= num2 * num2;  // ★ 피해량의 제곱
    }
    
    // ManeuverDef의 commonality 적용
    num *= this.commonality;
    
    // Tool의 chanceFactor 적용
    if (tool != null)
    {
        num *= tool.chanceFactor;  // ★ Tool 선택 확률
    }
    
    // Pawn의 기본 Tool인 경우 0.3 배율 적용
    if (comesFromPawnNativeVerbs && (tool == null || !tool.alwaysTreatAsWeapon))
    {
        num *= 0.3f;  // ★ Pawn 기본 Tool 감소
    }
    
    return num;
}
```

### 1.2 Tool 선택 가중치 공식

```
선택 가중치 = (예상 피해량²) × ManeuverDef.commonality × Tool.chanceFactor × (Pawn 기본 Tool인 경우 0.3)
```

**요소 설명:**
1. **예상 피해량²**: Tool의 power와 무기 스탯을 기반으로 계산된 피해량의 제곱
2. **ManeuverDef.commonality**: ManeuverDef에서 정의된 가중치 (기본값 1.0)
3. **Tool.chanceFactor**: Tool에서 정의된 가중치 (기본값 1.0)
4. **Pawn 기본 Tool 배율**: RaceDef의 Tool인 경우 0.3 배율 적용

## 2. Tool 정의 확인

### 2.1 Weapon_HighTech.xml의 Tools

```xml
<tools>
    <li>
        <label>point</label>
        <capacities>
            <li>Stab</li>
        </capacities>
        <power>16</power>
        <cooldownTime>3</cooldownTime>
        <!-- chanceFactor 미지정 → 기본값 1.0 -->
    </li>
    <li>
        <label>edge</label>
        <capacities>
            <li>GunlanceShell_Normal</li>
        </capacities>
        <power>16</power>
        <cooldownTime>1</cooldownTime>
        <!-- chanceFactor 미지정 → 기본값 1.0 -->
    </li>
</tools>
```

**특징:**
- 두 tool 모두 `chanceFactor` 미지정 → **기본값 1.0**
- 두 tool 모두 `power` 16 (동일)
- 다른 `capacities`를 사용하므로 서로 다른 ManeuverDef와 연결됨

### 2.2 Ratkin RaceDef의 Tools

```xml
<tools>
    <li>
        <label>teeth</label>
        <capacities>
            <li>Bite</li>
        </capacities>
        <power>10</power>
        <cooldownTime>1.4</cooldownTime>
        <!-- chanceFactor 미지정 → 기본값 1.0 -->
    </li>
    <li>
        <label>left fist</label>
        <capacities>
            <li>Scratch</li>
        </capacities>
        <power>5</power>
        <cooldownTime>1.5</cooldownTime>
        <!-- chanceFactor 미지정 → 기본값 1.0 -->
    </li>
    <li>
        <label>right fist</label>
        <capacities>
            <li>Scratch</li>
        </capacities>
        <power>5</power>
        <cooldownTime>1.5</cooldownTime>
        <!-- chanceFactor 미지정 → 기본값 1.0 -->
    </li>
</tools>
```

**특징:**
- 모든 tool의 `chanceFactor` 미지정 → **기본값 1.0**
- **Pawn 기본 Tool**이므로 **0.3 배율 적용**
- left fist와 right fist는 동일한 power와 capacity를 가짐

## 3. 확률 계산

### 3.1 가중치 계산 전제 조건

**공통 가정:**
- ManeuverDef의 `commonality` = 1.0 (기본값)
- Tool의 `chanceFactor` = 1.0 (기본값)
- 예상 피해량 = Tool의 `power` 값 (실제로는 무기 스탯 등이 적용되지만, 간단화를 위해 power만 사용)
- 무기 스탯 배율 = 1.0 (기본값)

### 3.2 Weapon_HighTech.xml Tool 확률

#### A. Point Tool (Stab capacity)
- **Power**: 16
- **chanceFactor**: 1.0
- **Pawn 기본 Tool**: 아니오 (무기 Tool)
- **배율**: 1.0

**가중치 계산:**
```
가중치 = (16²) × 1.0 × 1.0 × 1.0 = 256
```

#### B. Edge Tool (GunlanceShell_Normal capacity)
- **Power**: 16
- **chanceFactor**: 1.0
- **Pawn 기본 Tool**: 아니오 (무기 Tool)
- **배율**: 1.0

**가중치 계산:**
```
가중치 = (16²) × 1.0 × 1.0 × 1.0 = 256
```

**결과:**
- **총 가중치**: 256 + 256 = 512
- **Point 발동 확률**: 256 / 512 = **50%**
- **Edge 발동 확률**: 256 / 512 = **50%**

**⚠️ 중요**: 
1. **Pawn 고유 Tools도 포함**: 무기를 들고 있어도 Ratkin의 teeth, left fist, right fist도 함께 선택 대상입니다! (실제 시나리오는 7장 참고)
2. **ManeuverDef 연결**: 
   - Point는 Stab capacity의 ManeuverDef와 연결
   - Edge는 GunlanceShell_Normal capacity의 ManeuverDef와 연결
   - 각 ManeuverDef마다 다른 commonality를 가질 수 있어 실제 확률은 다를 수 있습니다.

### 3.3 Ratkin RaceDef Tool 확률

**Pawn 기본 Tool**이므로 모든 Tool에 **0.3 배율** 적용됩니다.

#### A. Teeth Tool (Bite capacity)
- **Power**: 10
- **chanceFactor**: 1.0
- **Pawn 기본 Tool**: 예
- **배율**: 0.3

**가중치 계산:**
```
가중치 = (10²) × 1.0 × 1.0 × 0.3 = 100 × 0.3 = 30
```

#### B. Left Fist Tool (Scratch capacity)
- **Power**: 5
- **chanceFactor**: 1.0
- **Pawn 기본 Tool**: 예
- **배율**: 0.3

**가중치 계산:**
```
가중치 = (5²) × 1.0 × 1.0 × 0.3 = 25 × 0.3 = 7.5
```

#### C. Right Fist Tool (Scratch capacity)
- **Power**: 5
- **chanceFactor**: 1.0
- **Pawn 기본 Tool**: 예
- **배율**: 0.3

**가중치 계산:**
```
가중치 = (5²) × 1.0 × 1.0 × 0.3 = 25 × 0.3 = 7.5
```

**결과:**
- **총 가중치**: 30 + 7.5 + 7.5 = 45
- **Teeth 발동 확률**: 30 / 45 = **66.67%** (약 66.7%)
- **Left Fist 발동 확률**: 7.5 / 45 = **16.67%** (약 16.7%)
- **Right Fist 발동 확률**: 7.5 / 45 = **16.67%** (약 16.7%)

## 4. 종합 결과표

### 4.1 Weapon_HighTech.xml Tools

| Tool | Capacity | Power | chanceFactor | 가중치 | 확률 |
|------|----------|-------|--------------|--------|------|
| point | Stab | 16 | 1.0 | 256 | 50% |
| edge | GunlanceShell_Normal | 16 | 1.0 | 256 | 50% |

### 4.2 Ratkin RaceDef Tools

| Tool | Capacity | Power | chanceFactor | Pawn 기본 Tool 배율 | 가중치 | 확률 |
|------|----------|-------|--------------|-------------------|--------|------|
| teeth | Bite | 10 | 1.0 | 0.3 | 30 | **66.67%** |
| left fist | Scratch | 5 | 1.0 | 0.3 | 7.5 | **16.67%** |
| right fist | Scratch | 5 | 1.0 | 0.3 | 7.5 | **16.67%** |

## 5. 참고사항

### 5.1 실제 계산 시 고려사항

1. **ManeuverDef의 commonality**: 각 ManeuverDef마다 다른 commonality 값을 가질 수 있음
2. **예상 피해량 계산**: 실제로는 무기의 MeleeWeapon_DamageMultiplier 스탯 등이 적용됨
3. **Tool 선택 조건**: 
   - ManeuverDef 선택 → 해당 ManeuverDef의 requiredCapacity를 가진 Tool 중 선택
   - 따라서 동일한 ManeuverDef를 사용하는 Tool들 간에만 확률 계산이 의미있음

### 5.2 Tool 선택 흐름

**파일**: `RimworldSource/RimWorld/Pawn_MeleeVerbs.cs`

실제 근접 공격 시 `GetUpdatedAvailableVerbsList()`가 호출되어 **모든 근접 공격 Verb를 수집**합니다:

```
1. 근접 공격 시작 (TryGetMeleeVerb)
2. GetUpdatedAvailableVerbsList() 호출하여 모든 근접 공격 Verb 수집:
   a) Pawn.verbTracker.AllVerbs (Pawn 고유 Tools - RaceDef의 tools)
   b) Equipment.AllVerbs (무기의 Tools) ← 무기를 들고 있으면 추가
   c) Apparel.AllVerbs (장비의 Tools) ← 장비가 있으면 추가
   d) Hediffs.Verbs (상태이상의 Verbs) ← 상태이상이 있으면 추가
   e) Terrain.Verbs (지형 기반 Verbs, 4% 확률)
3. 각 VerbEntry의 GetSelectionWeight() 계산 (VerbUtility.InitialVerbWeight 사용)
4. 가중치 기반 랜덤 선택 (TryRandomElementByWeight)
5. 선택된 Verb의 Tool 사용
```

**핵심 발견**: 무기를 들고 있어도 **Pawn 고유 Tools(RaceDef)도 함께 선택 대상에 포함**됩니다!

### 5.3 chanceFactor 조정 방법

Tool의 발동 확률을 조정하려면 XML에서 `chanceFactor` 값을 설정하면 됩니다:

```xml
<li>
    <label>point</label>
    <capacities>
        <li>Stab</li>
    </capacities>
    <power>16</power>
    <cooldownTime>3</cooldownTime>
    <chanceFactor>0.5</chanceFactor>  <!-- 발동 확률 50%로 감소 -->
</li>
```

## 6. 결론

### Weapon_HighTech.xml
- Point와 Edge는 **동일한 power와 chanceFactor**를 가지므로, 동일한 조건에서 선택될 때 **각각 50% 확률**로 발동됩니다.

### Ratkin RaceDef
- **Teeth**: **66.67%** (가장 높은 확률, power가 높음)
- **Left Fist**: **16.67%**
- **Right Fist**: **16.67%**
- Pawn 기본 Tool로 인한 0.3 배율이 모든 Tool에 적용되지만, Teeth가 상대적으로 높은 power를 가져 더 높은 확률을 가집니다.

## 7. 실제 게임 시나리오 분석

### 7.1 무기를 들고 있는 Ratkin Pawn

**상황**: Ratkin이 Weapon_HighTech 무기를 들고 근접 공격

**선택 가능한 Tools:**
1. **무기 Tools** (Equipment):
   - point (Stab, power 16)
   - edge (GunlanceShell_Normal, power 16)

2. **Pawn 고유 Tools** (RaceDef):
   - teeth (Bite, power 10) ← **0.3 배율 적용**
   - left fist (Scratch, power 5) ← **0.3 배율 적용**
   - right fist (Scratch, power 5) ← **0.3 배율 적용**

**가중치 계산** (ManeuverDef.commonality = 1.0 가정):
- point: (16²) × 1.0 × 1.0 × 1.0 = **256**
- edge: (16²) × 1.0 × 1.0 × 1.0 = **256**
- teeth: (10²) × 1.0 × 1.0 × 0.3 = **30**
- left fist: (5²) × 1.0 × 1.0 × 0.3 = **7.5**
- right fist: (5²) × 1.0 × 1.0 × 0.3 = **7.5**

**총 가중치**: 256 + 256 + 30 + 7.5 + 7.5 = **557**

**실제 발동 확률:**
- **point**: 256 / 557 = **45.96%** (약 46%)
- **edge**: 256 / 557 = **45.96%** (약 46%)
- **teeth**: 30 / 557 = **5.39%** (약 5.4%)
- **left fist**: 7.5 / 557 = **1.35%** (약 1.4%)
- **right fist**: 7.5 / 557 = **1.35%** (약 1.4%)

**결론**: 무기를 들고 있어도 Pawn 고유 Tools가 약 **8.1%** 확률로 발동됩니다.

### 7.2 맨손 Ratkin Pawn

**상황**: Ratkin이 무기 없이 근접 공격

**선택 가능한 Tools:**
1. **Pawn 고유 Tools** (RaceDef):
   - teeth (Bite, power 10) ← **0.3 배율 적용**
   - left fist (Scratch, power 5) ← **0.3 배율 적용**
   - right fist (Scratch, power 5) ← **0.3 배율 적용**

**가중치 계산**:
- teeth: (10²) × 1.0 × 1.0 × 0.3 = **30**
- left fist: (5²) × 1.0 × 1.0 × 0.3 = **7.5**
- right fist: (5²) × 1.0 × 1.0 × 0.3 = **7.5**

**총 가중치**: 30 + 7.5 + 7.5 = **45**

**실제 발동 확률:**
- **teeth**: 30 / 45 = **66.67%** (약 66.7%)
- **left fist**: 7.5 / 45 = **16.67%** (약 16.7%)
- **right fist**: 7.5 / 45 = **16.67%** (약 16.7%)

이것이 이전에 계산한 값입니다.

## 관련 파일
- RimworldSource/Verse/Tool.cs
- RimworldSource/Verse/VerbProperties.cs
- RimworldSource/Verse/VerbUtility.cs
- RimworldSource/RimWorld/Pawn_MeleeVerbs.cs ← **핵심: 실제 Tool 선택 로직**
- RimworldSource/RimWorld/SocialInteractionUtility.cs
- Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml
- Project/1.6/Defs/ThingDefs_Races/Races_Rakinlike.xml
