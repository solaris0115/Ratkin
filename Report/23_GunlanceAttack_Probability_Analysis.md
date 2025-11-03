# 건랜스 포격 공격 확률 분석 보고서

## 분석 목적
건랜스를 장착한 랫킨이 `GunlanceShell_Normal` 포격 공격을 사용할 확률을 계산.

## 데이터 수집

### 랫킨 기본 tools
| Tool | Capacity | Power | Cooldown | chanceFactor |
|------|----------|-------|----------|--------------|
| teeth | Bite | 10 | 1.4 | 1.0 (기본값) |
| left fist | Scratch | 5 | 1.5 | 1.0 (기본값) |
| right fist | Scratch | 5 | 1.5 | 1.0 (기본값) |

### 건랜스 tools
| Tool | Capacity | Power | Cooldown | chanceFactor |
|------|----------|-------|----------|--------------|
| point | Stab | 16 | 3.0 | 1.0 (기본값) |
| edge | GunlanceShell_Normal | 16 | 1.0 | 1.0 (기본값) |

### ManeuverDef
- **Stab**: meleeDamageBaseAmount = 1, meleeArmorPenetrationBase = -1
- **GunlanceShell_Normal**: damageAmount = 15, damageDef = Bomb
- **Bite**: meleeDamageBaseAmount = 1, meleeArmorPenetrationBase = -1, commonalityVsEdificeFactor = 0.01
- **Scratch**: meleeDamageBaseAmount = 1, meleeArmorPenetrationBase = -1

## 선택 메커니즘 분석

### 림월드 근접 공격 선택 알고리즘

**위치**: `RimworldSource/Verse/VerbUtility.cs`

#### 1단계: InitialVerbWeight 계산
```csharp
InitialVerbWeight = DPS × AdditionalSelectionFactor
```

**DPS 계산**:
```csharp
DPS = damage × (1 + armorPenetration) × accuracyTouch / cycleTime
```

**AdditionalSelectionFactor**:
```csharp
factor = tool.chanceFactor (기본 1.0)
+ 0.1 per additionalHediff (추가 효과가 있는 경우)
```

#### 2단계: VerbSelectionCategory 분류
```csharp
highestWeight = max(모든 verb의 InitialVerbWeight)
if (weight >= highestWeight * 0.95f)  → Best
else if (weight < highestWeight * 0.25f) → Worst
else → Mid
```

#### 3단계: FinalSelectionWeight
```csharp
if (category == Worst) return 0
else return 1 / category내verb개수 * (category == Mid ? 0.25 : 0.75)
```

## 계산 수행

### 공통 설정 가정
- accuracyTouch = 1.0 (기본값)
- armorPenetration = 0 (meleeArmorPenetrationBase가 -1이므로 조정값으로 계산됨)
- chanceFactor = 1.0 (모든 tool에 명시되지 않음)

### 각 Tool의 InitialVerbWeight 계산

#### 1. teeth (Bite)
- damage = 10 (power)
- AP = 0 (추정)
- accuracy = 1.0
- cooldown = 1.4
- **DPS = 10 × 1 × 1 / 1.4 = 7.14**
- chanceFactor = 1.0
- **InitialVerbWeight = 7.14**

#### 2. left/right fist (Scratch)
- damage = 5 (power)
- AP = 0
- accuracy = 1.0
- cooldown = 1.5
- **DPS = 5 × 1 × 1 / 1.5 = 3.33**
- chanceFactor = 1.0
- **InitialVerbWeight = 3.33**

#### 3. point (Stab)
- damage = 16 (power)
- AP = 0
- accuracy = 1.0
- cooldown = 3.0
- **DPS = 16 × 1 × 1 / 3.0 = 5.33**
- chanceFactor = 1.0
- **InitialVerbWeight = 5.33**

#### 4. edge (GunlanceShell_Normal)
- damage = 16 (power) - **주의**: ManeuverDef의 damageAmount는 별도로 사용될 가능성
- AP = 0
- accuracy = 1.0
- cooldown = 1.0
- **DPS = 16 × 1 × 1 / 1.0 = 16.0**
- chanceFactor = 1.0
- **InitialVerbWeight = 16.0**

### 카테고리 분류
**highestWeight = 16.0** (edge)

| Tool | InitialWeight | Ratio to Highest | Category |
|------|---------------|------------------|----------|
| edge | 16.0 | 100% | **Best** |
| teeth | 7.14 | 44.6% | Mid |
| point | 5.33 | 33.3% | Mid |
| left fist | 3.33 | 20.8% | Mid |
| right fist | 3.33 | 20.8% | Mid |

**주의**: Best 분류 기준은 >= 95%인데, edge만 100%이므로 Best에 edge만 존재.
Mid 분류에는 teeth, point, left fist, right fist가 포함됨.

### FinalSelectionWeight 계산
- **Best (edge)**: 1개 → weight = 1 × 0.75 = **0.75**
- **Mid (teeth, point, left, right)**: 4개 → 각 weight = (1/4) × 0.25 = **0.0625**

### 최종 확률 계산

**Total weight = 0.75 + (0.0625 × 4) = 1.0**

| Attack | Weight | Probability |
|--------|--------|-------------|
| **GunlanceShell_Normal (edge)** | 0.75 | **75%** |
| Stab (point) | 0.0625 | 6.25% |
| Bite (teeth) | 0.0625 | 6.25% |
| Scratch (left fist) | 0.0625 | 6.25% |
| Scratch (right fist) | 0.0625 | 6.25% |

## 결론

**건랜스를 장착한 랫킨이 `GunlanceShell_Normal` 포격 공격을 사용할 확률: 75%**

### 근거
1. **DPS 비교**: edge (16.0) > teeth (7.14) > point (5.33) > fist (3.33)
2. **카테고리 분류**: edge만 Best 카테고리(>= 95% threshold)
3. **최종 가중치**: Best 0.75, Mid 0.25

### 추가 고려사항
- **Bite의 commonalityVsEdificeFactor = 0.01**: 건물 타겟 시 Bite 확률이 100배 감소
- **건물 타겟 전용**: 다른 공격들도 영향받을 수 있으나, GunlanceShell_Normal의 dominance는 유지됨

## 참고: 소스코드 위치
- **Verb 선택 로직**: `RimworldSource/Verse/VerbUtility.cs` (InitialVerbWeight, FinalSelectionWeight)
- **카테고리 분류**: `RimworldSource/Verse/VerbUtility.cs:217-229` (GetSelectionCategory)
- **가중치 계산**: `RimworldSource/Verse/VerbUtility.cs:254-273` (FinalSelectionWeight)
- **Verb Entry**: `RimworldSource/RimWorld/VerbEntry.cs:27-50` (GetSelectionWeight)

