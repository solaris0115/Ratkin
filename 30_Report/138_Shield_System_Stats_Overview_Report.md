# Shield System Stats DeflectAngle BlockChance ShieldArmor Sharp Blunt Heat TowerShield HeavyShield WoodenShield

# 138 — 방패 시스템 스탯 개요 보고서

생성일: 2026-04-08  
참조 파일:
- `Project/1.6/Defs/Stats/Stats_ShieldDeflect.xml`
- `Project/1.6/Defs/ThingsDefs/Apparel_Shield.xml`

---

## 판정 2단계 구조

```
1단계: RK_Stat_ShieldBlockChance (폰 스탯)
  → 블락 시도 성공 여부만 판정 (아머와 무관)
  → 영향: 근접 스킬 (+1/레벨), Manipulation (scale 12, max 1.5), Sight (scale 12, max 1.5)
  → postProcessCurve:
      스킬 -20 → 5%
      스킬   0 → 50%
      스킬  10 → 80%
      스킬  20 → 90%
      스킬  40 → 96%
      스킬  60 → 98%

2단계: Shield Armor vs 공격자 AP (아머 롤)
  → 방패의 Sharp / Blunt / Heat 방어도로 최종 데미지 감소 결정
```

---

## 커스텀 StatDef 목록

| DefName | 카테고리 | 설명 | 영향 요소 |
|---|---|---|---|
| `RK_Stat_ShieldBlockChance` | PawnCombat | 블락 시도 확률 | 근접 스킬, Manipulation, Sight |
| `RK_Stat_DeflectAngle` | PawnCombat | 방어 유효 정면각 (도) | 근접 스킬 (base -40, +4/레벨), postProcessCurve |
| `RK_Stat_Shield_Sharp` | Apparel | 방패 방어도 - 날카로움 | Stuff 재질 × StuffBase, 품질 계수 |
| `RK_Stat_Shield_Blunt` | Apparel | 방패 방어도 - 둔탁 | 동일 |
| `RK_Stat_Shield_Heat` | Apparel | 방패 방어도 - 열기 | 동일 |
| `RK_Stat_ShieldStuffBase` | Apparel (hidden) | Stuff 배율 (StuffEffectMultiplierArmor 대응) | 방패 타입별 고정값 |

### DeflectAngle postProcessCurve (로그형 감소)
| 입력(raw) | 출력(실제 각도) |
|---|---|
| 0 | 0° |
| 50 | 48° |
| 90 | 90° |
| 140 | 138° |
| 180 | 162° |
| 220 | 180° (상한) |

### 품질 계수 (Shield Armor 공통)
| 품질 | 계수 |
|---|---|
| Awful | 0.70 |
| Poor | 0.85 |
| Normal | 1.00 |
| Good | 1.10 |
| Excellent | 1.20 |
| Masterwork | 1.35 |
| Legendary | 1.60 |

---

## 방패 3종 스탯 비교

| | 소방패 (RK_WoodenShield) | 중방패 (RK_HeavyShield) | 타워방패 (RK_TowerShield) |
|---|---|---|---|
| **HP** | 150 | 250 | 400 |
| **무게** | 3 | 6 | 10 |
| **Flammability** | 0.8 | 0.2 | 0.2 |
| **Sharp (base)** | 0.35 (fixed) | 0.15 + Stuff×0.45 | 0.20 + Stuff×0.65 |
| **Blunt (base)** | 0.25 (fixed) | 0.30 + Stuff×0.45 | 0.45 + Stuff×0.65 |
| **Heat (base)** | 0.10 (fixed) | 0.15 + Stuff×0.45 | 0.15 + Stuff×0.65 |
| **StuffBase** | 없음 (재질 고정) | 0.45 | 0.65 |
| **DeflectAngle 보정** | +140° | +100° | +90° |
| **BlockChance 보정** | 없음 | +2 (raw offset) | +4 (raw offset) |
| **MoveSpeed 패널티** | 없음 | -0.2 | -0.5 |
| **방향전환 쿨다운** | 180 ticks (3s) | 240 ticks (4s) | 300 ticks (5s) |
| **무기 호환** | 한손 + ShieldCompatible | 한손 + ShieldCompatible | 한손만 |
| **제작 연구** | RK_Research_Carpentry | RK_Research_SwordAndShield | SwordAndShield + Metallurgy |
| **제작처** | (기본) | FueledSmithy / ElectricSmithy | FueledSmithy / ElectricSmithy |
| **재료** | Steel 25 + Wood 120 | Steel 50 + Plasteel 15 + Metallic 120 | Steel 70 + Plasteel 30 + Metallic 160 |

---

## Comp 구성 (전 방패 공통)

| Comp | 역할 |
|---|---|
| `CompProperties_ShieldDeflect` | 방패 방어 핵심 로직 |
| `CompProperties_ExtraDrawer` | 무장/비무장 드로우 분리 렌더링 |
| `CompProperties_ShieldWeaponIncompatible` | 허용 무기 태그 필터 |
| `CompProperties_ShieldFaceDirection` | 방향 전환 커맨드 및 쿨다운 |

---

## 확인 필요 사항

1. **소방패 BlockChance 보정 없음** — 중/타워는 raw +2/+4인데 소방패만 없음. 의도된 설계인지 확인.
2. **DeflectAngle 역전** — 소방패(140°) > 중(100°) > 타워(90°). 타워는 각도 좁은 대신 BlockChance로 보완하는 구조. 의도된 트레이드오프인지 확인.
3. **BlockChance raw +2/+4 의미** — StatWorker_ShieldBlockChance에서 이 raw offset이 실제 확률에 어떻게 반영되는지 코드 확인 필요 (postProcessCurve 적용 전/후 여부).
4. **소방패 Stuff 없음** — `stuffCategories Inherit="False"` 처리로 재질 영향 없음. 고정 방어도만 사용.
