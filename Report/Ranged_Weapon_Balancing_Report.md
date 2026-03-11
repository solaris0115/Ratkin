<!-- Ranged Weapon DPS Balancing Report Rimworld Ratkin -->

# Ranged Weapon Balancing Report

## 개요

- **생성일**: 2026-03-11
- **데이터 소스**: RimworldData (Core, Royalty, Ideology, Biotech, Anomaly, Odyssey), Project/1.6/Defs
- **포함 조건**: Verb_Shoot 또는 Verb_LaunchProjectile, Equippable/Equipable 컴포넌트 보유(부모 체인 포함), weaponClasses/thingCategories/weaponTags 보유, damageAmountBase > 0
- **제외**: Verb_ShootBeam, Verb_Spray, Verb_SpewFire, 소모품(음료 등), 터렛 건물(turretGunDef 참조), 착용 불가(Equippable 미보유)

## DPS 계산 공식

```
totalCycleSec = warmupTime + cooldown + (burstShotCount - 1) * (ticksBetweenBurstShots / 60)
DPS = (damageAmountBase * burstShotCount) / totalCycleSec
DPS_AVG = DPS × acc̄
acc̄ = (1/R) × ∫₀ᴿ acc(d) dd   (R = weapon range)
```

- **명중률 보간**: 고정 구간 Touch=3, Short=12, Medium=25, Long=40 타일에서 선형 보간 (VerbProperties.GetHitChanceFactor)
- **acc̄**: 사격 가능 거리 [0, R] 전체에서 acc(d)를 적분한 가중 평균 (사다리꼴 공식)
- **ticksBetweenBurstShots**: 미지정 시 15 (VerbProperties.cs 기본값)
- **burstShotCount**: 미지정 시 1
- **AP**: armorPenetrationBase 미지정 시 damage × 0.015 (ProjectileProperties.GetArmorPenetration)
- 상세: Report/125_DPS_AVG_Weighted_Accuracy_Formula_Report.md

## 림월드 원거리 무기 요약

### 림월드 원거리 무기 (읽기 순서)

| 순위 | defName | DPS | DPS_AVG | range | accT | accS | accM | accL | damage | AP |
|------|---------|-----|--------|-------|------|------|------|------|--------|-----|
| 1 | Gun_Revolver | 6.32 | 4.47 | 25.9 | 0.80 | 0.75 | 0.55 | 0.40 | 12.0 | 0.18 |
| 2 | Gun_Autopistol | 7.69 | 4.95 | 25.9 | 0.80 | 0.70 | 0.40 | 0.30 | 10.0 | 0.15 |
| 3 | Gun_MachinePistol | 11.02 | 7.8 | 19.9 | 0.90 | 0.65 | 0.35 | 0.15 | 6.0 | 0.09 |
| 4 | Gun_BoltActionRifle | 5.62 | 4.54 | 36.9 | 0.65 | 0.80 | 0.90 | 0.80 | 18.0 | 0.27 |
| 5 | Gun_PumpShotgun | 8.37 | 6.98 | 15.9 | 0.80 | 0.87 | 0.77 | 0.64 | 18.0 | 0.14 |
| 6 | Gun_ChainShotgun | 18.73 | 11.22 | 12.9 | 0.57 | 0.64 | 0.55 | 0.45 | 18.0 | 0.14 |
| 7 | Gun_HeavySMG | 12.34 | 8.09 | 22.9 | 0.85 | 0.65 | 0.35 | 0.20 | 12.0 | 0.18 |
| 8 | Gun_LMG | 18.08 | 7.58 | 25.9 | 0.40 | 0.48 | 0.35 | 0.26 | 12.0 | 0.18 |
| 9 | Gun_AssaultRifle | 10.88 | 7.09 | 30.9 | 0.60 | 0.70 | 0.65 | 0.55 | 11.0 | 0.165 |
| 10 | Gun_SniperRifle | 5.0 | 3.89 | 44.9 | 0.50 | 0.70 | 0.88 | 0.90 | 25.0 | 0.375 |
| 11 | Gun_Minigun | 41.67 | 9.8 | 30.9 | 0.20 | 0.25 | 0.25 | 0.18 | 10.0 | 0.15 |
| 12 | Gun_ChargeBlasterHeavy | 34.07 | 8.09 | 26.9 | 0.18 | 0.26 | 0.26 | 0.18 | 15.0 | 0.225 |
| 13 | Gun_Needle | 3.26 | 2.65 | 44.9 | 0.60 | 0.80 | 0.90 | 0.85 | 15.0 | 0.35 |
| 14 | Bow_Short | 3.67 | 2.36 | 22.9 | 0.75 | 0.65 | 0.45 | 0.25 | 11.0 | 0.165 |
| 15 | Pila | 3.85 | 2.79 | 18.9 | 0.80 | 0.71 | 0.50 | 0.32 | 25.0 | 0.1 |
| 16 | Bow_Recurve | 4.52 | 3.25 | 25.9 | 0.70 | 0.78 | 0.65 | 0.35 | 14.0 | 0.21 |
| 17 | Bow_Great | 4.86 | 3.67 | 29.9 | 0.65 | 0.85 | 0.75 | 0.50 | 17.0 | 0.15 |
| 18 | Gun_ChargeRifle | 14.12 | 8.25 | 27.9 | 0.55 | 0.64 | 0.55 | 0.45 | 16.0 | 0.35 |
| 19 | Gun_ChargeLance | 6.82 | 5.44 | 32.9 | 0.65 | 0.85 | 0.85 | 0.75 | 30.0 | 0.45 |
| 20 | Gun_ThumpCannon | 1.8 | 1.48 | 24.9 | 0.80 | 0.87 | 0.77 | 0.64 | 9.0 | 0.0 |

### DLC별 무기 수

- **Core**: 20개
- **Royalty**: 0개
- **Ideology**: 0개
- **Biotech**: 6개
- **Anomaly**: 2개
- **Odyssey**: 2개

## 랫킨 원거리 무기

| defName | DPS | DPS_AVG | range | accT | accS | accM | accL | damage | AP |
|---------|-----|--------|-------|------|------|------|------|--------|-----|
| RK_Crossbow | 4.23 | 2.65 | 22.9 | 0.70 | 0.65 | 0.45 | 0.25 | 11.0 | 0.21 |
| RK_AutoCrossBow | 9.0 | 6.28 | 27.0 | 0.75 | 0.70 | 0.65 | 0.50 | 10.0 | 0.2 |
| RK_Weapon_Arbalest | 6.25 | 4.44 | 35.0 | 0.70 | 0.75 | 0.70 | 0.65 | 25.0 | 0.5 |
| RK_Rifle | 5.71 | 4.3 | 33.9 | 0.65 | 0.85 | 0.75 | 0.65 | 16.0 | 0.28 |
| RK_SniperRifle | 5.62 | 4.31 | 42.0 | 0.60 | 0.70 | 0.85 | 0.85 | 18.0 | 0.32 |
| RK_FlechetteRifle | 5.81 | 4.4 | 31.0 | 0.65 | 0.85 | 0.75 | 0.65 | 18.0 | 0.38 |
| RK_FlechetteSniperRifle | 5.71 | 4.11 | 39.0 | 0.60 | 0.65 | 0.80 | 0.80 | 20.0 | 0.42 |
| RK_Rifle_line | 14.4 | 12.21 | 13.9 | 0.85 | 0.85 | 0.65 | 0.55 | 5.0 | 0.45 |
| RK_Weapon_Bolter | 8.0 | 5.52 | 23.9 | 0.85 | 0.65 | 0.55 | 0.40 | 20.0 | 0.45 |
| RK_PrototypePulseRifle | 7.58 | 5.59 | 31.0 | 0.75 | 0.85 | 0.65 | 0.45 | 12.0 | 0.6 |
| RK_Weapon_BFR | 5.6 | 3.54 | 44.9 | 0.65 | 0.35 | 0.65 | 0.85 | 28.0 | 0.6 |
| RK_Weapon_RatHolicGun | 2.33 | 0.55 | 30.9 | 0.20 | 0.25 | 0.25 | 0.18 | 7.0 | 0.3 |

## 전체 상세 데이터

### 림월드 전체

| defName | burst | cooldown | range | damage | AP | DPS | DPS_AVG | accAvg | accT | accS | accM | accL | defaultProjectile |
|---------|-------|----------|-------|--------|-----|-----|--------|--------|------|------|------|------|-------------------|
| Gun_Revolver | 1 | 1.6 | 25.9 | 12.0 | 0.18 | 6.32 | 4.47 | 0.7072 | 0.80 | 0.75 | 0.55 | 0.40 | Bullet_Revolver |
| Gun_Autopistol | 1 | 1.0 | 25.9 | 10.0 | 0.15 | 7.69 | 4.95 | 0.6431 | 0.80 | 0.70 | 0.40 | 0.30 | Bullet_Autopistol |
| Gun_MachinePistol | 3 | 0.9 | 19.9 | 6.0 | 0.09 | 11.02 | 7.8 | 0.708 | 0.90 | 0.65 | 0.35 | 0.15 | Bullet_MachinePistol |
| Gun_BoltActionRifle | 1 | 1.5 | 36.9 | 18.0 | 0.27 | 5.62 | 4.54 | 0.8066 | 0.65 | 0.80 | 0.90 | 0.80 | Bullet_BoltActionRifle |
| Gun_PumpShotgun | 1 | 1.25 | 15.9 | 18.0 | 0.14 | 8.37 | 6.98 | 0.8333 | 0.80 | 0.87 | 0.77 | 0.64 | Bullet_Shotgun |
| Gun_ChainShotgun | 3 | 1.35 | 12.9 | 18.0 | 0.14 | 18.73 | 11.22 | 0.5991 | 0.57 | 0.64 | 0.55 | 0.45 | Bullet_Shotgun |
| Gun_HeavySMG | 3 | 1.65 | 22.9 | 12.0 | 0.18 | 12.34 | 8.09 | 0.6556 | 0.85 | 0.65 | 0.35 | 0.20 | Bullet_HeavySMG |
| Gun_LMG | 6 | 1.6 | 25.9 | 12.0 | 0.18 | 18.08 | 7.58 | 0.4196 | 0.40 | 0.48 | 0.35 | 0.26 | Bullet_LMG |
| Gun_AssaultRifle | 3 | 1.7 | 30.9 | 11.0 | 0.165 | 10.88 | 7.09 | 0.6519 | 0.60 | 0.70 | 0.65 | 0.55 | Bullet_AssaultRifle |
| Gun_SniperRifle | 1 | 1.5 | 44.9 | 25.0 | 0.375 | 5.0 | 3.89 | 0.778 | 0.50 | 0.70 | 0.88 | 0.90 | Bullet_SniperRifle |
| Gun_Minigun | 25 | 1.5 | 30.9 | 10.0 | 0.15 | 41.67 | 9.8 | 0.2352 | 0.20 | 0.25 | 0.25 | 0.18 | Bullet_Minigun |
| Gun_ChargeBlasterHeavy | 24 | 7.4 | 26.9 | 15.0 | 0.225 | 34.07 | 8.09 | 0.2373 | 0.18 | 0.26 | 0.26 | 0.18 | Bullet_ChargeBlasterHeavy |
| Gun_Needle | 1 | 2.1 | 44.9 | 15.0 | 0.35 | 3.26 | 2.65 | 0.8116 | 0.60 | 0.80 | 0.90 | 0.85 | Bullet_NeedleGun |
| Bow_Short | 1 | 1.65 | 22.9 | 11.0 | 0.165 | 3.67 | 2.36 | 0.6428 | 0.75 | 0.65 | 0.45 | 0.25 | Arrow_Short |
| Pila | 1 | 2.5 | 18.9 | 25.0 | 0.1 | 3.85 | 2.79 | 0.7254 | 0.80 | 0.71 | 0.50 | 0.32 | Pilum_Thrown |
| Bow_Recurve | 1 | 1.65 | 25.9 | 14.0 | 0.21 | 4.52 | 3.25 | 0.7194 | 0.70 | 0.78 | 0.65 | 0.35 | Arrow_Recurve |
| Bow_Great | 1 | 1.5 | 29.9 | 17.0 | 0.15 | 4.86 | 3.67 | 0.755 | 0.65 | 0.85 | 0.75 | 0.50 | Arrow_Great |
| Gun_ChargeRifle | 3 | 2.0 | 27.9 | 16.0 | 0.35 | 14.12 | 8.25 | 0.5845 | 0.55 | 0.64 | 0.55 | 0.45 | Bullet_ChargeRifle |
| Gun_ChargeLance | 1 | 2.7 | 32.9 | 30.0 | 0.45 | 6.82 | 5.44 | 0.7981 | 0.65 | 0.85 | 0.85 | 0.75 | Bullet_ChargeLance |
| Gun_ThumpCannon | 1 | 4.0 | 24.9 | 9.0 | 0.0 | 1.8 | 1.48 | 0.8232 | 0.80 | 0.87 | 0.77 | 0.64 | Bullet_ThumpCannon |
| Flamebow | 1 | 1.65 | 22.9 | 6.0 | 0.09 | 2.0 | 1.29 | 0.6428 | 0.75 | 0.65 | 0.45 | 0.25 | Proj_FireArrow |
| Gun_MiniShotgun | 1 | 1.7 | 12.9 | 10.0 | 0.12 | 3.45 | 2.86 | 0.8289 | 0.80 | 0.87 | 0.70 | 0.55 | Bullet_MiniShotgun |
| Gun_Slugthrower | 1 | 4.0 | 19.9 | 12.0 | 0.18 | 2.79 | 0.77 | 0.2744 | 0.20 | 0.30 | 0.40 | 0.95 | Bullet_Slugthrower |
| Gun_Spiner | 1 | 1.8 | 6.9 | 12.0 | 0.18 | 5.71 | 1.21 | 0.2122 | 0.20 | 0.30 | 0.40 | 0.95 | Bullet_Spiner |
| Gun_ToxicNeedle | 1 | 2.1 | 44.9 | 25.0 | 0.35 | 5.62 | 4.67 | 0.8309 | 0.60 | 0.80 | 0.90 | 0.92 | Bullet_ToxicNeedleGun |
| Gun_NeedleLauncher | 1 | 2.1 | 24.9 | 15.0 | 0.35 | 3.26 | 2.5 | 0.7655 | 0.60 | 0.80 | 0.90 | 0.85 | Bullet_NeedleGun |
| Gun_HellcatRifle | 3 | 1.7 | 26.9 | 10.0 | 0.15 | 9.57 | 6.28 | 0.6561 | 0.60 | 0.70 | 0.65 | 0.55 | Bullet_HellcatRifle |
| NerveSpiker | 1 | 1.65 | 29.9 | 11.0 | 0.165 | 3.55 | 2.49 | 0.7023 | 0.70 | 0.78 | 0.65 | 0.35 | NerveSpiker_Spike |
| Gun_BeamRepeater | 30 | 3.0 | 21.9 | 5.0 | 0.5 | 12.68 | 8.67 | 0.684 | 0.65 | 0.72 | 0.65 | 0.60 | Bullet_BeamRepeater |
| Gun_Scattergun | 1 | 2.0 | 19.9 | 13.0 | 0.15 | 5.1 | 4.21 | 0.8255 | 0.80 | 0.87 | 0.72 | 0.60 | Bullet_Scattergun |

### 랫킨 전체

| defName | burst | cooldown | range | damage | AP | DPS | DPS_AVG | accAvg | accT | accS | accM | accL | defaultProjectile |
|---------|-------|----------|-------|--------|-----|-----|--------|--------|------|------|------|------|-------------------|
| RK_Crossbow | 1 | 1.2 | 22.9 | 11.0 | 0.21 | 4.23 | 2.65 | 0.6265 | 0.70 | 0.65 | 0.45 | 0.25 | Bolt_RK_Crossbow |
| RK_AutoCrossBow | 3 | 1.7 | 27.0 | 10.0 | 0.2 | 9.0 | 6.28 | 0.6974 | 0.75 | 0.70 | 0.65 | 0.50 | Bolt_RK_AutoCrossBow |
| RK_Weapon_Arbalest | 1 | 2.5 | 35.0 | 25.0 | 0.5 | 6.25 | 4.44 | 0.711 | 0.70 | 0.75 | 0.70 | 0.65 | RK_Projectile_Arbalest |
| RK_Rifle | 1 | 1.2 | 33.9 | 16.0 | 0.28 | 5.71 | 4.3 | 0.7525 | 0.65 | 0.85 | 0.75 | 0.65 | Bullet_RK_Rifle |
| RK_SniperRifle | 1 | 1.4 | 42.0 | 18.0 | 0.32 | 5.62 | 4.31 | 0.7661 | 0.60 | 0.70 | 0.85 | 0.85 | Bullet_RK_SniperRifle |
| RK_FlechetteRifle | 1 | 1.5 | 31.0 | 18.0 | 0.38 | 5.81 | 4.4 | 0.7574 | 0.65 | 0.85 | 0.75 | 0.65 | Bullet_RK_FlechetteRifle |
| RK_FlechetteSniperRifle | 1 | 1.7 | 39.0 | 20.0 | 0.42 | 5.71 | 4.11 | 0.7192 | 0.60 | 0.65 | 0.80 | 0.80 | Bullet_RK_FlechetteSniperRifle |
| RK_Rifle_line | 6 | 1.1 | 13.9 | 5.0 | 0.45 | 14.4 | 12.21 | 0.848 | 0.85 | 0.85 | 0.65 | 0.55 | Bullet_RK_Buck |
| RK_Weapon_Bolter | 1 | 2.0 | 23.9 | 20.0 | 0.45 | 8.0 | 5.52 | 0.69 | 0.85 | 0.65 | 0.55 | 0.40 | RK_Bullet_Bolter |
| RK_PrototypePulseRifle | 2 | 1.5 | 31.0 | 12.0 | 0.6 | 7.58 | 5.59 | 0.7374 | 0.75 | 0.85 | 0.65 | 0.45 | Bullet_RK_PrototypePulseRifleLight |
| RK_Weapon_BFR | 1 | 2.5 | 44.9 | 28.0 | 0.6 | 5.6 | 3.54 | 0.6317 | 0.65 | 0.35 | 0.65 | 0.85 | Bullet_BFR_AP |
| RK_Weapon_RatHolicGun | 1 | 1.0 | 30.9 | 7.0 | 0.3 | 2.33 | 0.55 | 0.2352 | 0.20 | 0.25 | 0.25 | 0.18 | RK_Bullet_RatHolicGun |

## 밸런싱 참고

- **림월드 최고 DPS_AVG**: Gun_ChainShotgun (11.22)
- **랫킨 최고 DPS_AVG**: RK_Rifle_line (12.21)
- **제외된 무기**: 폭발물(로켓/박격포 등 damageAmountBase 없음), 빔/스프레이/화염(Verb_ShootBeam 등), 수류탄(explosion 기반)
