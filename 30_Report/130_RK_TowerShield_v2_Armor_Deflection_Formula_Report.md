# 랫킨 대방패 v2 방어력 및 도탄 로직 (AI 참조용)

> **태그**: RK_TowerShield_Second v2 방패 방어력 도탄 1.5 Deflection Formula

## RK_TowerShield_Second Def
| 항목 | 값 |
|------|-----|
| thingClass | NewRatkin.ApparelShieldTowerSecond |
| ArmorRating_Sharp (Base) | 0.1 |
| ArmorRating_Blunt (Base) | 0.15 |
| StuffEffectMultiplierArmor | 0.4 |
| costList | Steel 200, Plasteel 30, Uranium 20 (고정 제작, stuff 미사용) |
| deflectAngleHalf | 45° (XML) |
| deflectChance | 1.0 |

## 방어력 계산식
```
ArmorRating_XXX = (Base + StuffEffectMultiplierArmor × StuffPower_Armor_XXX) × QualityFactor
```
- Plasteel StuffPower: Sharp 1.14, Blunt 0.55
- QualityFactor: 평범 1.0, 전설 1.80

### 현재 Def (stuff 없음)
| 품질 | Sharp | Blunt |
|------|-------|-------|
| 평범 | 0.10 | 0.15 |
| 전설 | 0.18 | 0.27 |

### 플라스틸 가정 시
| 품질 | Sharp | Blunt |
|------|-------|-------|
| 평범 | 0.556 | 0.37 |
| 전설 | 1.001 | 0.666 |

## 1.5 도탄 공식 (128 참조)
```
totalDeflectChance = MeleeSkill×0.02 + Clamp01(armorRate/2)/2
Rand.Value <= totalDeflectChance → 차단
```

## v2 개선안 (계획, 미구현)
```
effectiveArmor = max(armorRating × rangedProtection(melee) - armorPenetration, 0)
rand < effectiveArmor×0.5 → 완전 무효화
rand < effectiveArmor     → 50% 감소 + Sharp→Blunt
else                       → 무효
```

### 원거리방호력 (melee 레벨별)
| Melee | 원거리방호력 |
|-------|--------------|
| 0 | 130% |
| 10 | 170% |
| 20 | 175% |

### 로그형 도탄 확률 (129 참조)
```
y = 30 + 45 × ln(1 + meleeSkill) / ln(21)
```
| Melee | y |
|-------|---|
| 0 | 30% |
| 10 | ~65% |
| 20 | 75% |

## 1.5 vs v2 개선안
| 구분 | 1.5 | v2 계획 |
|------|-----|---------|
| 구조 | 스킬+소재 가산 | effectiveArmor vs rand |
| 관통력 | 미반영 | 반영 |
| 결과 | 이진 (차단/통과) | 3단계 |
| 각도 | ±70° | ±45° |
