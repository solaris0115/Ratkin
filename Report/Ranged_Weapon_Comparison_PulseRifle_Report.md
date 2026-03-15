# 원거리 무기 비교 데이터 보고서
<!-- Tags: PulseRifle AssaultRifle ChargeRifle ChargeLance BeamRepeater SniperRifle Weapon Range Comparison DPS DPS_AVG ArmorPenetration Accuracy FullCycleTime -->

## 개요

림월드 원본 주요 소총급 무기(Assault Rifle, Charge Rifle, Charge Lance, Beam Repeater, Sniper Rifle)와 랫킨 프로토타입 펄스 라이플(RK_PrototypePulseRifle)의 핵심 전투 수치를 비교합니다.

## 계산 기준

### ArmorPenetration (명시 없는 경우)

```
armorPenetration = damageAmount × 0.015
```

- 출처: `RimworldSource/Verse/ProjectileProperties.cs` → `GetArmorPenetration()`
- 상수: `VerbProperties.DefaultArmorPenetrationPerDamage = 0.015f`

### DPS 공식

```
T_cycle = warmup + cooldown + (burstCount - 1) × (ticksBetweenBurstShots / 60)
DPS = (damage × burstCount) / T_cycle
DPS_AVG = DPS × acc̄
acc̄ = (1/R) × ∫₀ᴿ acc(d) dd   (가중 적분 평균 명중률)
```

- 60 ticks = 1초 (`GenTicks.TicksPerRealSecond = 60`)
- 명중률 보간: Touch=3, Short=12, Medium=25, Long=40 타일 고정 구간에서 선형 보간
- 상세: `Report/124_Ranged_Weapon_DPS_Time_Unit_Conversion_Report.md`, `Report/125_DPS_AVG_Weighted_Accuracy_Formula_Report.md`

## 무기 비교 테이블

| 무기 | Power | Piercing | Range | Burst | Warmup | Cooldown | Touch | Short | Medium | Long | T_cycle | DPS | acc̄ | DPS_AVG |
|------|-------|----------|-------|-------|--------|----------|-------|-------|--------|------|---------|-----|------|---------|
| Assault Rifle | 11 | 0.165* | 30.9 | 3 | 1.0 | 1.7 | 60% | 70% | 65% | 55% | 3.033 | 10.88 | 0.652 | 7.09 |
| Charge Rifle | 16 | 0.35 | 27.9 | 3 | 1.0 | 2.0 | 55% | 64% | 55% | 45% | 3.400 | 14.12 | 0.585 | 8.25 |
| Charge Lance | 30 | 0.45* | 32.9 | 1 | 1.7 | 2.7 | 65% | 85% | 85% | 75% | 4.400 | 6.82 | 0.798 | 5.44 |
| Sniper Rifle | 25 | 0.375* | 44.9 | 1 | 3.5 | 1.5 | 50% | 70% | 88% | 90% | 5.000 | 5.00 | 0.778 | 3.89 |
| Beam Repeater | 5 | 0.50 | 21.9 | 30 | 4.0 | 3.0 | 65% | 72% | 65% | 60% | 11.833 | 12.68 | 0.684 | 8.67 |
| **RK_PulseRifle (Light)** | **12** | **0.60** | **27** | **4** | **1.0** | **1.5** | **75%** | **85%** | **65%** | **45%** | **3.000** | **16.00** | **0.758** | **12.13** |
| **RK_PulseRifle (Heavy)** | **25** | **0.70** | **35** | **2** | **1.7** | **2.0** | **65%** | **75%** | **80%** | **70%** | **3.700** | **13.51** | **0.743** | **10.04** |

> `*` = 명시적 armorPenetrationBase 없음, `damage × 0.015` 공식으로 계산된 값

## DPS 계산 상세

### Assault Rifle
```
T_cycle = 1.0 + 1.7 + (3-1) × (10/60) = 2.7 + 0.333 = 3.033초
DPS = (11 × 3) / 3.033 = 33 / 3.033 = 10.88
acc̄ = 0.652  →  DPS_AVG = 10.88 × 0.652 = 7.09
```

### Charge Rifle
```
T_cycle = 1.0 + 2.0 + (3-1) × (12/60) = 3.0 + 0.400 = 3.400초
DPS = (16 × 3) / 3.400 = 48 / 3.400 = 14.12
acc̄ = 0.585  →  DPS_AVG = 14.12 × 0.585 = 8.25
```

### Charge Lance
```
T_cycle = 1.7 + 2.7 + (1-1) × 0 = 4.400초
DPS = 30 / 4.400 = 6.82
acc̄ = 0.798  →  DPS_AVG = 6.82 × 0.798 = 5.44
```

### Sniper Rifle
```
T_cycle = 3.5 + 1.5 + (1-1) × 0 = 5.000초
DPS = 25 / 5.000 = 5.00
acc̄ = 0.778  →  DPS_AVG = 5.00 × 0.778 = 3.89
```

acc̄ 상세 (R=44.9):
- [0, 3]: 3 × 0.50 = 1.500
- [3, 12]: 9 × (0.50+0.70)/2 = 5.400
- [12, 25]: 13 × (0.70+0.88)/2 = 10.270
- [25, 40]: 15 × (0.88+0.90)/2 = 13.350
- [40, 44.9]: 4.9 × 0.90 = 4.410
- 합계 = 34.930, acc̄ = 34.930 / 44.9 = 0.778

### Beam Repeater
```
T_cycle = 4.0 + 3.0 + (30-1) × (10/60) = 7.0 + 4.833 = 11.833초
DPS = (5 × 30) / 11.833 = 150 / 11.833 = 12.68
acc̄ = 0.684  →  DPS_AVG = 12.68 × 0.684 = 8.67
```

### RK_PulseRifle (Light) — 갱신됨
```
T_cycle = 1.0 + 1.5 + (4-1) × (10/60) = 2.5 + 0.500 = 3.000초
DPS = (12 × 4) / 3.000 = 48 / 3.000 = 16.00
acc̄ = 0.758  →  DPS_AVG = 16.00 × 0.758 = 12.13
```

acc̄ 상세 (R=27):
- [0, 3]: 3 × 0.75 = 2.250
- [3, 12]: 9 × (0.75+0.85)/2 = 7.200
- [12, 25]: 13 × (0.85+0.65)/2 = 9.750
- [25, 27]: acc(27) = Lerp(0.65, 0.45, 2/15) = 0.6233 → 2 × (0.65+0.6233)/2 = 1.273
- 합계 = 20.473, acc̄ = 20.473 / 27 = 0.758

### RK_PulseRifle (Heavy) — 갱신됨
```
T_cycle = 1.7 + 2.0 + (2-1) × (0/60) = 3.700초
DPS = (25 × 2) / 3.700 = 50 / 3.700 = 13.51
acc̄ = 0.743  →  DPS_AVG = 13.51 × 0.743 = 10.04
```

acc̄ 상세 (R=35):
- [0, 3]: 3 × 0.65 = 1.950
- [3, 12]: 9 × (0.65+0.75)/2 = 6.300
- [12, 25]: 13 × (0.75+0.80)/2 = 10.075
- [25, 35]: acc(35) = Lerp(0.80, 0.70, 10/15) = 0.7333 → 10 × (0.80+0.7333)/2 = 7.667
- 합계 = 25.992, acc̄ = 25.992 / 35 = 0.743

## 상세 데이터

### Assault Rifle (Gun_AssaultRifle)
- **출처**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedIndustrial.xml`
- **투사체**: `Bullet_AssaultRifle` — damage 11, AP 미명시 (11 × 0.015 = 0.165)
- **ticksBetweenBurstShots**: 10

### Charge Rifle (Gun_ChargeRifle)
- **출처**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedSpacer.xml`
- **투사체**: `Bullet_ChargeRifle` — damage 16, AP 0.35 (명시)
- **ticksBetweenBurstShots**: 12

### Charge Lance (Gun_ChargeLance)
- **출처**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedSpacer.xml`
- **투사체**: `Bullet_ChargeLance` — damage 30, AP 미명시 (30 × 0.015 = 0.45)
- **burstShotCount**: 1 (단발, verb에 명시 없으면 기본 1)
- **stoppingPower**: 1.5

### Sniper Rifle (Gun_SniperRifle)
- **출처**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedIndustrial.xml`
- **투사체**: `Bullet_SniperRifle` — damage 25, AP 미명시 (25 × 0.015 = 0.375)
- **burstShotCount**: 1 (단발)
- **warmupTime**: 3.5 (매우 긴 조준 시간)

### Beam Repeater (Gun_BeamRepeater)
- **출처**: `RimworldData/Odyssey/Defs/ThingDefs_Misc/Weapons/Ranged_Spacer.xml`
- **투사체**: `Bullet_BeamRepeater` — damage 5, AP 0.50 (명시)
- **damageDef**: BeamBypassShields (실드 관통)
- **ticksBetweenBurstShots**: 10
- **총 버스트 데미지**: 5 × 30 = 150

### RK_PrototypePulseRifle (Light / Burst 모드) — 갱신됨
- **출처**: `Project/1.6/Defs/ThingsDefs/Weapon_Range.xml`
- **투사체**: `Bullet_RK_PrototypePulseRifleLight` — damage 12, AP 0.60 (명시)
- **burstShotCount**: 4 (CompProperties 기준)
- **ticksBetweenBurstShots**: 10
- **range**: 27 (CompProperties rangeBurst)
- **warmupTime**: 1.0 (CompProperties warmupTimeBurst)
- **총 버스트 데미지**: 12 × 4 = 48
- **정확도**: 무기 기본 statBases 그대로 적용

### RK_PrototypePulseRifle (Heavy / Single 모드) — 갱신됨
- **출처**: `Project/1.6/Defs/ThingsDefs/Weapon_Range.xml`
- **투사체**: `Bullet_RK_PrototypePulseRifleHeavy` — damage 25, AP 0.70 (명시)
- **burstShotCount**: 2 (CompProperties 기준)
- **ticksBetweenBurstShots**: 0
- **range**: 35 (CompProperties rangeSingle)
- **warmupTime**: 1.7 (CompProperties warmupTimeSingle)
- **총 버스트 데미지**: 25 × 2 = 50
- **정확도**: 기본 statBases + statOffsetsSingle 적용
  - Touch: 0.75 + (-0.10) = 0.65
  - Short: 0.85 + (-0.10) = 0.75
  - Medium: 0.65 + 0.15 = 0.80
  - Long: 0.45 + 0.25 = 0.70
- **Cooldown**: 1.5 + 0.5 = 2.0

## 발사 모드 전환 구조 (RK_PrototypePulseRifle)

`CompProperties_PulseRifleFireMode`로 Burst/Single 모드 전환:

| 속성 | Light (Burst) | Heavy (Single) |
|------|---------------|----------------|
| projectile | Bullet_RK_PrototypePulseRifleLight | Bullet_RK_PrototypePulseRifleHeavy |
| range | 27 | 35 |
| burstShotCount | 4 | 2 |
| ticksBetweenBurstShots | 10 | 0 |
| warmupTime | 1.0 | 1.7 |
| statOffsets | — | Touch -0.10, Short -0.10, Medium +0.15, Long +0.25, Cooldown +0.5 |
