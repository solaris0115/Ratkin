---
category: armor-rating-formula
last_updated: 2026-03-12
sources:
  - RimworldData/Core/Defs/Stats/Stats_Apparel.xml
  - RimworldSource/RimWorld/StatPart_Stuff.cs
  - RimworldSource/RimWorld/StatPart_Quality.cs
  - RimworldSource/RimWorld/StatDef.cs
  - RimworldSource/Verse/ArmorUtility.cs
scope: 방어구 방어력(ArmorRating) 계산식 및 관련 계수
fields: baseArmor, StuffEffectMultiplierArmor, StuffPower_Armor_Sharp/Blunt/Heat, QualityFactor
note: 품질·소재·고정방어력이 모두 반영된 최종 계산식
keywords: ArmorRating 방어력 계산 품질 소재 고정방어력 StuffEffectMultiplierArmor StuffPower
---

# 방어구 방어력 계산식 (Armor Rating Formula)

## 1. 최종 계산식

```
최종 방어력 = (고정방어력 + 소재방어력) × 품질계수
```

```
ArmorRating_XXX = (BaseArmor_XXX + StuffEffectMultiplierArmor × StuffPower_Armor_XXX) × QualityFactor
```

### 변수 설명

| 변수 | 출처 | 설명 |
|------|------|------|
| **BaseArmor_XXX** | 방어구 ThingDef `statBases` | 고정방어력. `ArmorRating_Sharp`, `ArmorRating_Blunt`, `ArmorRating_Heat` 중 해당 값 |
| **StuffEffectMultiplierArmor** | 방어구 ThingDef `statBases` | 소재 반영 배율. 0~1. 소재 없으면 0 |
| **StuffPower_Armor_XXX** | 소재 ThingDef `statBases` | 소재별 방어 계수. Sharp/Blunt/Heat 각각 |
| **QualityFactor** | StatPart_Quality | 품질 배율. Normal=1.0 |

---

## 2. StatPart 처리 순서

`StatDef.PostLoad`에서 `parts.SortBy(-priority)`로 정렬 → **높은 priority가 먼저** 실행.

| 순서 | StatPart | priority | 동작 |
|------|----------|----------|------|
| 1 | StatPart_Stuff | 100 | `value += StuffEffectMultiplierArmor × StuffPower_Armor_XXX` |
| 2 | StatPart_Quality | 0 | `value *= QualityFactor` |

**코드 근거**: `StatPart_Stuff.TransformValue` → `value += GetMultiplier(req) * num`  
**코드 근거**: `StatPart_Quality.TransformValue` → `val += (val * QualityMultiplier - val)` = 곱셈

---

## 3. 품질 계수 (QualityFactor)

`ArmorRatingBase`의 StatPart_Quality. **생산 가능 + CompQuality** 보유 시에만 적용.

| 품질 | factorAwful | factorPoor | factorNormal | factorGood | factorExcellent | factorMasterwork | factorLegendary |
|------|-------------|------------|--------------|------------|-----------------|------------------|-----------------|
| **계수** | 0.60 | 0.80 | 1.00 | 1.15 | 1.30 | 1.45 | 1.80 |

※ 한글: 형편없음/하급/평범/상급/완벽/대장장이/전설

---

## 4. 소재 계수 (StuffPower_Armor_XXX)

소재 ThingDef의 `statBases`에 정의. **stuff 방식** 방어구(`costStuffCount` + `stuffCategories`)에만 적용.

### 4.1 StatDef 매핑

| ArmorRating Stat | StuffPower Stat |
|------------------|-----------------|
| ArmorRating_Sharp | StuffPower_Armor_Sharp |
| ArmorRating_Blunt | StuffPower_Armor_Blunt |
| ArmorRating_Heat | StuffPower_Armor_Heat |

### 4.2 주요 소재별 값 (Core + DLC)

| 소재 | 카테고리 | Sharp | Blunt | Heat |
|------|----------|-------|-------|------|
| Cloth | Fabric | 0.36 | 0 | 0.18 |
| Synthread | Fabric | 0.94 | 0.26 | 0.90 |
| Devilstrand | Fabric | 1.40 | 0.36 | 3.00 |
| Hyperweave | Fabric | 2.00 | 0.54 | 2.88 |
| Leather (기본) | Leathery | 0.81 | 0.24 | 1.5 |
| Thrumbo | Leathery | 2.08 | 0.36 | (상속) |
| Steel | Metallic | 0.90 | 0.45 | 0.60 |
| Plasteel | Metallic | 1.14 | 0.55 | 0.65 |
| Uranium | Metallic | 1.08 | 0.54 | 0.65 |
| Wood | Woody | 0.54 | 0.54 | 0.40 |
| Bioferrite (Anomaly) | Metallic, Bioferrite | 1.10 | 0.50 | 0.50 |

---

## 5. StuffEffectMultiplierArmor

방어구 ThingDef `statBases`에 정의. **소재 방어력을 얼마나 반영할지** 배율.

- **0**: 소재 무시 (고정방어력만)
- **1.0**: 소재 계수 100% 반영
- **0.9**: PlateArmor (원본) – 소재 90% 반영

### 랫킨 방어구 예시

| defName | StuffEffectMultiplierArmor | Base Sharp | Base Blunt |
|---------|---------------------------|------------|------------|
| RK_Plate | 0.63 | 0.25 | 0.30 |
| RK_PlateHelm | 0.35 | 0.40 | 0.20 |

---

## 6. 계산 예시

### RK_Plate (강철, Normal 품질)

- Base: Sharp 0.25, Blunt 0.30
- StuffEffectMultiplierArmor: 0.63
- Steel: Sharp 0.90, Blunt 0.45
- Quality: 1.0

**Sharp** = (0.25 + 0.63×0.90) × 1.0 = **0.817**  
**Blunt** = (0.30 + 0.63×0.45) × 1.0 = **0.584**

### RK_Plate (플라스틸, Masterwork 품질)

- Base: Sharp 0.25, Blunt 0.30
- StuffEffectMultiplierArmor: 0.63
- Plasteel: Sharp 1.14, Blunt 0.55
- Quality: 1.45

**Sharp** = (0.25 + 0.63×1.14) × 1.45 = **1.185**  
**Blunt** = (0.30 + 0.63×0.55) × 1.45 = **0.877**

---

## 7. 특수 케이스

| 조건 | 적용 |
|------|------|
| **소재 없음** (Fixed Cost) | StuffPower = 0 → `Base × Quality` |
| **품질 없음** (비제작) | QualityFactor = 1.0 |
| **ArmorRating_Heat** | StuffPower_Armor_Heat 사용. 일부 방어구는 Heat 미정의 |

---

## 8. 방어력 적용 (참고)

`ArmorUtility.ApplyArmor`:
1. `effectiveArmor = max(armorRating - armorPenetration, 0)`
2. rand(0~1) < effectiveArmor×0.5 → **완전 무효화**
3. rand(0~1) < effectiveArmor → **피해 50% + Blunt 전환**
4. 그 외 → **방어 무효**

---

## 9. 관련 캐시

- [combat-coefficients.md](combat-coefficients.md): 품질/소재 계수 상세
- [apparel.md](apparel.md): 랫킨 방어구 defName 목록
