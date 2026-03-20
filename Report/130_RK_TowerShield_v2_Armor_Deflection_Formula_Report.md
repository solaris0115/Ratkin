# 랫킨 대방패 v2 방어력 및 도탄 확률 계산식 보고서

> **태그**: RK_TowerShield_Second v2 방패 방어력 도탄 확률 1.5 플라스틸 평범 전설 Deflection Formula  
> **작성일**: 2026-03-20  
> **목적**: RK_TowerShield_Second 방어력(평범/전설, 플라스틸) 및 1.5/v2 도탄 확률 계산식 비교

---

## 1. 개요

본 보고서는 다음을 담습니다.

1. **랫킨 대방패 v2 (RK_TowerShield_Second)** 방어력 – 평범/전설 품질, 플라스틸 소재
2. **1.5 기준** 방패 도탄 확률 계산식
3. **v2 개선안** 도탄 확률 계산식 (계획)

---

## 2. RK_TowerShield_Second 방어력

### 2.1 Def 정보

| 항목 | 값 |
|------|-----|
| defName | RK_TowerShield_Second |
| thingClass | NewRatkin.ApparelShieldTowerSecond |
| ArmorRating_Sharp (Base) | 0.1 |
| ArmorRating_Blunt (Base) | 0.15 |
| StuffEffectMultiplierArmor | 0.4 |
| costList | Steel 200, Plasteel 30, Uranium 20 (고정 제작) |

※ 현재 Def는 **고정 제작(costList)** 방식이라 소재(stuff) 미사용. 아래는 **플라스틸 소재 가정** 시 계산입니다.

### 2.2 방어력 계산식

```
ArmorRating_XXX = (Base + StuffEffectMultiplierArmor × StuffPower_Armor_XXX) × QualityFactor
```

- **Plasteel** StuffPower: Sharp 1.14, Blunt 0.55
- **QualityFactor**: 평범 1.0, 전설 1.80

### 2.3 플라스틸 소재 기준 방어력

| 품질 | Sharp | Blunt |
|------|-------|-------|
| **평범 (Normal)** | (0.1 + 0.4×1.14) × 1.0 = **0.556** | (0.15 + 0.4×0.55) × 1.0 = **0.37** |
| **전설 (Legendary)** | (0.1 + 0.4×1.14) × 1.80 = **1.001** | (0.15 + 0.4×0.55) × 1.80 = **0.666** |

### 2.4 현재 Def (고정 제작, 소재 없음) 기준

소재 미사용 시 `StuffPower = 0`:

| 품질 | Sharp | Blunt |
|------|-------|-------|
| **평범** | 0.1 × 1.0 = **0.10** | 0.15 × 1.0 = **0.15** |
| **전설** | 0.1 × 1.80 = **0.18** | 0.15 × 1.80 = **0.27** |

---

## 3. 도탄 확률 계산식 비교

### 3.1 1.5 기준 도탄 확률 (Shield.CheckPreAbsorbDamage)

**출처**: [128_Ratkin_Shield_Deflection_Logic_1.5_Analysis_Report.md](128_Ratkin_Shield_Deflection_Logic_1.5_Analysis_Report.md)

```
totalDeflectChance = blockRateBySkill + clampedBlockRateFromStuff
```

#### (1) 스킬 기반 (blockRateBySkill)

```
blockRateBySkill = MeleeSkillLevel × 0.02
```

| Melee 스킬 | blockRateBySkill |
|------------|------------------|
| 0 | 0% |
| 5 | 10% |
| 10 | 20% |
| 15 | 30% |
| 20 | 40% |

#### (2) 소재 기반 (clampedBlockRateFromStuff)

```
armorRate = ArmorRating_Sharp | ArmorRating_Blunt | ArmorRating_Heat  (공격 타입별)
clampedBlockRateFromStuff = Clamp01(armorRate / 2) / 2
```

- **최대**: 50% (armorRate ≥ 2.0)
- **예시**: ArmorRating 0.5 → 12.5%, 1.0 → 25%

#### (3) 판정

```
if (Rand.Value <= totalDeflectChance) → 도탄 성공 (데미지 0)
```

#### (4) 1.5 공식 요약

```
도탄 확률 = (Melee스킬 × 2%) + min(armorRate/2, 1) × 50%
```

---

### 3.2 v2 개선안 도탄 확률 (계획)

**출처**: [v2_방패_개선_계획](.cursor/plans/v2_방패_개선_계획_ba191d9d.plan.md), [129_Shield_Deflection_Chance_LogCurve_Report](129_Shield_Deflection_Chance_LogCurve_Report.md)

#### (1) 원거리 방호력 (melee 레벨별)

| Melee 스킬 | 원거리방호력 |
|------------|--------------|
| 0 | 130% |
| 5 | 157% |
| 10 | 170% |
| 20 | 175% |

#### (2) effectiveArmor 기반 판정

```
effectiveArmor = max(방패 방어력 × [원거리방호력] - 원거리 공격 관통력, 0)
rand = 0~1 랜덤

- rand < effectiveArmor × 0.5  → 완전 무효화 (데미지 0)
- rand < effectiveArmor        → 피해 50% 감소 + Sharp → Blunt
- 그 외                        → 방어 무효 (데미지 그대로)
```

#### (3) [스탯] 튕겨낼 확률 (별도 로그형 커브)

```
y = 30 + 45 × ln(1 + meleeSkill) / ln(21)
```

| Melee 스킬 | 도탄 확률 |
|------------|-----------|
| 0 | 30% |
| 5 | 57% |
| 10 | 66% |
| 20 | 75% |

#### (4) v2 공식 요약

```
effectiveArmor = max(armorRating × rangedProtectionCurve(melee) - armorPenetration, 0)

판정:
  rand < effectiveArmor×0.5 → 완전 무효화
  rand < effectiveArmor     → 50% 감소 + Blunt
  else                      → 무효
```

---

## 4. 두 공식 비교표

| 구분 | 1.5 | v2 개선안 |
|------|-----|-----------|
| **구조** | 가산 (스킬 + 소재) | effectiveArmor vs rand |
| **스킬 영향** | 선형 (레벨×2%) | 원거리방호력 곱 (130%~175%) |
| **소재/방어력** | Clamp01(armor/2)/2, 최대 50% | armor × 방호력 - 관통력 |
| **관통력** | 미반영 | 명시적 반영 |
| **결과** | 이진 (도탄/미도탄) | 3단계 (무효화/50%감소/무효) |
| **각도** | ±70° | ±45° (deflectAngleHalf) |

---

## 5. 참조 파일

| 파일 | 역할 |
|------|------|
| [Apparel_Shield.xml](Project/1.6/Defs/ThingsDefs/Apparel_Shield.xml) | RK_TowerShield_Second Def |
| [ApparelShieldTowerSecond.cs](Project/1.6/Source/ShieldOfRatkinia/ApparelShieldTowerSecond.cs) | v2 방패 소스 |
| [128_Ratkin_Shield_Deflection_Logic_1.5_Analysis_Report.md](128_Ratkin_Shield_Deflection_Logic_1.5_Analysis_Report.md) | 1.5 도탄 로직 |
| [129_Shield_Deflection_Chance_LogCurve_Report.md](129_Shield_Deflection_Chance_LogCurve_Report.md) | v2 로그형 커브 |
| [v2_방패_개선_계획](.cursor/plans/v2_방패_개선_계획_ba191d9d.plan.md) | v2 개선 계획 |
| [armor-rating-formula](.cursor/def-cache/armor-rating-formula.md) | 방어력 계산식 |
