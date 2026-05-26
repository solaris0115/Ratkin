---
category: ranged-weapons
last_updated: 2026-03-19
sources:
  - Project/1.6/Defs/ThingsDefs/Weapon_Range.xml
  - RimworldData/Anomaly/Defs/ThingDefs_Misc/Weapons/Weapons_Ranged.xml
  - RimworldData/Anomaly/Defs/ThingDefs_Misc/Weapons/Weapons_RangedHoraxian.xml
  - RimworldData/Biotech/Defs/ThingDefs_Misc/Weapons/RangedIndustrial.xml
  - RimworldData/Biotech/Defs/ThingDefs_Misc/Weapons/RangedMechanoid_Light.xml
  - RimworldData/Biotech/Defs/ThingDefs_Misc/Weapons/RangedMechanoid_Medium.xml
  - RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedIndustrial.xml
  - RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedMechanoid.xml
  - RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedNeolithic.xml
  - RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedSpacer.xml
  - RimworldData/Core/Defs/ThingDefs_Misc/Weapons/Weapons_Breach.xml
  - RimworldData/Odyssey/Defs/ThingDefs_Misc/Weapons/Ranged_Spacer.xml
scope: Verb_Shoot/Verb_LaunchProjectile 원거리 무기, Equippable/Equipable 컴포넌트 보유(착용 가능)
fields: defName, burstShotCount, burstSec, cooldown, range, damage, AP, DPS, DPS_AVG, accTouch(DPS), accShort(DPS), accMedium(DPS), accLong(DPS)
---

# 원거리 무기 (Ranged Weapons)

## DPS 공식

`totalCycleSec = warmupTime + cooldown + (burstShotCount - 1) * (ticksBetweenBurstShots / 60)`
`DPS = (damageAmountBase * burstShotCount) / totalCycleSec`
`DPS_AVG = DPS * acc̄` (거리 가중 적분 평균 명중률)
`acc̄ = (1/R) * ∫₀ᴿ acc(d) dd` — [0, range] 구간에서 선형 보간 명중률을 적분 (사다리꼴 공식)
고정 구간: Touch=3, Short=12, Medium=25, Long=40 타일 (ShootTuning.cs)
AP: armorPenetrationBase 미지정 시 `damage * 0.015` (ProjectileProperties.GetArmorPenetration)
상세: 30_Report/125_DPS_AVG_Weighted_Accuracy_Formula_Report.md

## 림월드 (Core + DLC)

### Core (20개)

| defName | burst | burstSec | cooldown | range | damage | AP | DPS | DPS_AVG | Touch | Short | Med | Long |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Gun_Revolver | 1 | 0.25 | 1.6 | 25.9 | 12.0 | 0.18 | 6.32 | 4.47 | 0.80 (5.05) | 0.75 (4.74) | 0.55 (3.47) | 0.40 (2.53) |
| Gun_Autopistol | 1 | 0.25 | 1.0 | 25.9 | 10.0 | 0.15 | 7.69 | 4.95 | 0.80 (6.15) | 0.70 (5.38) | 0.40 (3.08) | 0.30 (2.31) |
| Gun_MachinePistol | 3 | 0.117 | 0.9 | 19.9 | 6.0 | 0.09 | 11.02 | 7.8 | 0.90 (9.92) | 0.65 (7.16) | 0.35 (3.86) | 0.15 (1.65) |
| Gun_BoltActionRifle | 1 | 0.25 | 1.5 | 36.9 | 18.0 | 0.27 | 5.62 | 4.54 | 0.65 (3.66) | 0.80 (4.5) | 0.90 (5.06) | 0.80 (4.5) |
| Gun_PumpShotgun | 1 | 0.25 | 1.25 | 15.9 | 18.0 | 0.14 | 8.37 | 6.98 | 0.80 (6.7) | 0.87 (7.28) | 0.77 (6.45) | 0.64 (5.36) |
| Gun_ChainShotgun | 3 | 0.167 | 1.35 | 12.9 | 18.0 | 0.14 | 18.73 | 11.22 | 0.57 (10.68) | 0.64 (11.99) | 0.55 (10.3) | 0.45 (8.43) |
| Gun_HeavySMG | 3 | 0.183 | 1.65 | 22.9 | 12.0 | 0.18 | 12.34 | 8.09 | 0.85 (10.49) | 0.65 (8.02) | 0.35 (4.32) | 0.20 (2.47) |
| Gun_LMG | 6 | 0.117 | 1.6 | 25.9 | 12.0 | 0.18 | 18.08 | 7.58 | 0.40 (7.23) | 0.48 (8.68) | 0.35 (6.33) | 0.26 (4.7) |
| Gun_AssaultRifle | 3 | 0.167 | 1.7 | 30.9 | 11.0 | 0.165 | 10.88 | 7.09 | 0.60 (6.53) | 0.70 (7.62) | 0.65 (7.07) | 0.55 (5.98) |
| Gun_SniperRifle | 1 | 0.25 | 1.5 | 44.9 | 25.0 | 0.375 | 5.0 | 3.89 | 0.50 (2.5) | 0.70 (3.5) | 0.88 (4.4) | 0.90 (4.5) |
| Gun_Minigun | 25 | 0.083 | 1.5 | 30.9 | 10.0 | 0.15 | 41.67 | 9.8 | 0.20 (8.33) | 0.25 (10.42) | 0.25 (10.42) | 0.18 (7.5) |
| Gun_ChargeBlasterHeavy | 24 | 0.083 | 7.4 | 26.9 | 15.0 | 0.225 | 34.07 | 8.09 | 0.18 (6.13) | 0.26 (8.86) | 0.26 (8.86) | 0.18 (6.13) |
| Gun_Needle | 1 | 0.25 | 2.1 | 44.9 | 15.0 | 0.35 | 3.26 | 2.65 | 0.60 (1.96) | 0.80 (2.61) | 0.90 (2.93) | 0.85 (2.77) |
| Bow_Short | 1 | 0.25 | 1.65 | 22.9 | 11.0 | 0.165 | 3.67 | 2.36 | 0.75 (2.75) | 0.65 (2.38) | 0.45 (1.65) | 0.25 (0.92) |
| Pila | 1 | 0.25 | 2.5 | 18.9 | 25.0 | 0.1 | 3.85 | 2.79 | 0.80 (3.08) | 0.71 (2.73) | 0.50 (1.92) | 0.32 (1.23) |
| Bow_Recurve | 1 | 0.25 | 1.65 | 25.9 | 14.0 | 0.21 | 4.52 | 3.25 | 0.70 (3.16) | 0.78 (3.52) | 0.65 (2.94) | 0.35 (1.58) |
| Bow_Great | 1 | 0.25 | 1.5 | 29.9 | 17.0 | 0.15 | 4.86 | 3.67 | 0.65 (3.16) | 0.85 (4.13) | 0.75 (3.64) | 0.50 (2.43) |
| Gun_ChargeRifle | 3 | 0.2 | 2.0 | 27.9 | 16.0 | 0.35 | 14.12 | 8.25 | 0.55 (7.76) | 0.64 (9.04) | 0.55 (7.76) | 0.45 (6.35) |
| Gun_ChargeLance | 1 | 0.25 | 2.7 | 32.9 | 30.0 | 0.45 | 6.82 | 5.44 | 0.65 (4.43) | 0.85 (5.8) | 0.85 (5.8) | 0.75 (5.11) |
| Gun_ThumpCannon | 1 | 0.25 | 4.0 | 24.9 | 9.0 | 0.0 | 1.8 | 1.48 | 0.80 (1.44) | 0.87 (1.57) | 0.77 (1.39) | 0.64 (1.15) |


### Biotech (6개)

| defName | burst | burstSec | cooldown | range | damage | AP | DPS | DPS_AVG | Touch | Short | Med | Long |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Flamebow | 1 | 0.25 | 1.65 | 22.9 | 6.0 | 0.09 | 2.0 | 1.29 | 0.75 (1.5) | 0.65 (1.3) | 0.45 (0.9) | 0.25 (0.5) |
| Gun_MiniShotgun | 1 | 0.167 | 1.7 | 12.9 | 10.0 | 0.12 | 3.45 | 2.86 | 0.80 (2.76) | 0.87 (3.0) | 0.70 (2.41) | 0.55 (1.9) |
| Gun_Slugthrower | 1 | 0.133 | 4.0 | 19.9 | 12.0 | 0.18 | 2.79 | 0.77 | 0.20 (0.56) | 0.30 (0.84) | 0.40 (1.12) | 0.95 (2.65) |
| Gun_Spiner | 1 | 0.133 | 1.8 | 6.9 | 12.0 | 0.18 | 5.71 | 1.21 | 0.20 (1.14) | 0.30 (1.71) | 0.40 (2.29) | 0.95 (5.43) |
| Gun_ToxicNeedle | 1 | 0.25 | 2.1 | 44.9 | 25.0 | 0.35 | 5.62 | 4.67 | 0.60 (3.37) | 0.80 (4.49) | 0.90 (5.06) | 0.92 (5.17) |
| Gun_NeedleLauncher | 1 | 0.25 | 2.1 | 24.9 | 15.0 | 0.35 | 3.26 | 2.5 | 0.60 (1.96) | 0.80 (2.61) | 0.90 (2.93) | 0.85 (2.77) |


### Anomaly (2개)

| defName | burst | burstSec | cooldown | range | damage | AP | DPS | DPS_AVG | Touch | Short | Med | Long |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Gun_HellcatRifle | 3 | 0.167 | 1.7 | 26.9 | 10.0 | 0.15 | 9.57 | 6.28 | 0.60 (5.74) | 0.70 (6.7) | 0.65 (6.22) | 0.55 (5.27) |
| NerveSpiker | 1 | 0.25 | 1.65 | 29.9 | 11.0 | 0.165 | 3.55 | 2.49 | 0.70 (2.48) | 0.78 (2.77) | 0.65 (2.31) | 0.35 (1.24) |


### Odyssey (2개)

| defName | burst | burstSec | cooldown | range | damage | AP | DPS | DPS_AVG | Touch | Short | Med | Long |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Gun_BeamRepeater | 30 | 0.167 | 3.0 | 21.9 | 5.0 | 0.5 | 12.68 | 8.67 | 0.65 (8.24) | 0.72 (9.13) | 0.65 (8.24) | 0.60 (7.61) |
| Gun_Scattergun | 1 | 0.167 | 2.0 | 19.9 | 13.0 | 0.15 | 5.1 | 4.21 | 0.80 (4.08) | 0.87 (4.44) | 0.72 (3.67) | 0.60 (3.06) |


## 랫킨 (Ratkin)

| defName | burst | burstSec | cooldown | range | damage | AP | DPS | DPS_AVG | Touch | Short | Med | Long |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| RK_Crossbow | 1 | 0.25 | 1.2 | 22.9 | 11.0 | 0.21 | 4.23 | 2.65 | 0.70 (2.96) | 0.65 (2.75) | 0.45 (1.9) | 0.25 (1.06) |
| RK_AutoCrossBow | 3 | 0.217 | 1.7 | 27.0 | 10.0 | 0.2 | 9.0 | 6.28 | 0.75 (6.75) | 0.70 (6.3) | 0.65 (5.85) | 0.50 (4.5) |
| RK_Weapon_Arbalest | 1 | 0.25 | 2.5 | 35.0 | 25.0 | 0.5 | 6.25 | 4.44 | 0.70 (4.38) | 0.75 (4.69) | 0.70 (4.38) | 0.65 (4.06) |
| RK_Rifle | 1 | 0.25 | 1.2 | 33.9 | 16.0 | 0.28 | 5.71 | 4.3 | 0.65 (3.71) | 0.85 (4.86) | 0.75 (4.29) | 0.65 (3.71) |
| RK_SniperRifle | 1 | 0.25 | 1.4 | 42.0 | 18.0 | 0.32 | 5.62 | 4.31 | 0.60 (3.38) | 0.70 (3.94) | 0.85 (4.78) | 0.85 (4.78) |
| RK_FlechetteRifle | 1 | 0.25 | 1.5 | 31.0 | 18.0 | 0.38 | 5.81 | 4.4 | 0.65 (3.77) | 0.85 (4.94) | 0.75 (4.35) | 0.65 (3.77) |
| RK_FlechetteSniperRifle | 1 | 0.25 | 1.7 | 39.0 | 20.0 | 0.42 | 5.71 | 4.11 | 0.60 (3.43) | 0.65 (3.71) | 0.80 (4.57) | 0.80 (4.57) |
| RK_Rifle_line | 6 | 0.017 | 1.3 | 10.9 | 5.0 | 0.45 | 13.14 | 11.17 | 0.85 (11.17) | 0.85 (11.17) | 0.65 (8.54) | 0.55 (7.23) |
| RK_Weapon_Bolter | 1 | 0.25 | 2.0 | 23.9 | 20.0 | 0.45 | 8.0 | 5.52 | 0.85 (6.8) | 0.65 (5.2) | 0.55 (4.4) | 0.40 (3.2) |
| RK_Weapon_BFR | 1 | 0.25 | 2.5 | 44.9 | 25.0 | 0.5 | 5.0 | 3.16 | 0.65 (3.25) | 0.35 (1.75) | 0.65 (3.25) | 0.85 (4.25) |
| RK_Weapon_RatHolicGun | 1 | 0.25 | 1.0 | 30.9 | 7.0 | 0.3 | 2.33 | 0.55 | 0.20 (0.47) | 0.25 (0.58) | 0.25 (0.58) | 0.18 (0.42) |