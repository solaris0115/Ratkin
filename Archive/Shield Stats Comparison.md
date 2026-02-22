# Shield Stats Comparison
# 방패 스탯 비교표

## 방패 기본 스탯

| 방패 종류 | Shield Stamina | Shield Stamina Recharge Rate |
|----------|----------------|------------------------------|
| RK_WoodenShield (목제 방패) | 1.1 | 0.25 |
| RK_HeavyShield (철제 방패) | 1.5 | 0.16 |
| RK_TowerShield (타워 쉴드) | 1.9 | 0.13 |

## StaminaShield 컴포넌트 스탯

| 방패 종류 | Starting Ticks To Reset | Stun Duration Ticks | Durability Damage % | Stamina On Reset |
|----------|------------------------|---------------------|---------------------|------------------|
| RK_WoodenShield | 180 (3초) | 180 | 0.1 | 0.2 |
| RK_HeavyShield | 180 (3초) | 180 | 0.1 | 0.2 |
| RK_TowerShield | 180 (3초) | 180 | 0.1 | 0.2 |

## 스태미나 손실 (Stamina Loss Per Damage)

| 방패 종류 | Melee | Ranged | Explosive |
|----------|-------|--------|-----------|
| RK_WoodenShield | 0.066 | 0.033 | 0.1 |
| RK_HeavyShield | 0.066| 0.033 | 0.1 |
| RK_TowerShield | 0.033 | 0.033 | 0.066 |

## 피해 감소율 (Damage Reduction %)

| 방패 종류 | Melee | Ranged | Explosive |
|----------|-------|--------|-----------|
| RK_WoodenShield | 0.4 (40%) | 0.5 (50%) | 0.25 (25%) |
| RK_HeavyShield | 0.7 (70%) | 0.8 (80%) | 0.4 (40%) |
| RK_TowerShield | 0.9 (90%) | 1.0 (100%) | 0.5 (50%) |

## 등급(Quality) 영향 받는 스탯

### 1. RK_Stat_ShieldStamina (방패 스태미나 최대량)
| 등급 | 배율 | Normal 대비 |
|------|------|-------------|
| Awful | 0.6 | -40% |
| Poor | 0.8 | -20% |
| Normal | 1.0 | 기준 |
| Good | 1.2 | +20% |
| Excellent | 1.4 | +40% |
| Masterwork | 1.7 | +70% |
| Legendary | 2.1 | +110% |

**예시**: Normal 방패 스태미나 1.1 → Legendary 방패 스태미나 2.31

### 2. RK_Stat_ShieldStaminaRechargeRate (방패 스태미나 재충전 속도)
| 등급 | 배율 | Normal 대비 |
|------|------|-------------|
| Awful | 0.9 | -10% |
| Poor | 0.95 | -5% |
| Normal | 1.0 | 기준 |
| Good | 1.05 | +5% |
| Excellent | 1.1 | +10% |
| Masterwork | 1.2 | +20% |
| Legendary | 1.3 | +30% |

**예시**: Normal 방패 재충전률 0.13/s → Legendary 방패 재충전률 0.169/s

### 3. MaxHitPoints (내구도)
- 일반적으로 RimWorld에서 quality 영향을 받음
- 등급이 높을수록 내구도 증가

### 등급 영향 안 받는 스탯
- **StaminaShield 컴포넌트 스탯**: startingTicksToReset, stunDurationTicks, durabilityDamagePercent, staminaOnReset
- **스태미나 손실**: staminaLossPerDamageMelee, staminaLossPerDamageRanged, staminaLossPerDamageExplosive
- **피해 감소율**: damageReductionPercentMelee, damageReductionPercentRanged, damageReductionPercentExplosive

## 요약

### 공통 사항
- 모든 방패의 기본 스태미나와 재충전률이 동일 (1.1 / 0.13)
- 모든 방패의 리셋 시간, 스턴 지속시간, 리셋 시 스태미나가 동일
- 모든 방패의 스태미나 손실 값이 동일 (Melee: 0.066, Ranged: 0.033, Explosive: 0.166)

### 차이점
- **피해 감소율**: 방패 등급에 따라 다름
  - 목제 방패: Melee 40%, Ranged 50%, Explosive 25%
  - 철제 방패: Melee 70%, Ranged 80%, Explosive 40%
  - 타워 쉴드: Melee 90%, Ranged 100%, Explosive 50%

