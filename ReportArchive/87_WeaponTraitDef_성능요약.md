# WeaponTraitDef 성능 요약 보고서

## 개요
WeaponTraitDef의 각 defName별 성능 관련 정보만 정리 (MarketValue 제외)

---

## Gun 카테고리

### QuickReload
- **statFactors**:
  - `RangedWeapon_Cooldown`: 0.8 (쿨다운 20% 감소)

### AimAssistance
- **statFactors**:
  - `AccuracyTouch`: 1.2 (근거리 명중률 20% 증가)
  - `AccuracyShort`: 1.2 (단거리 명중률 20% 증가)
  - `AccuracyMedium`: 1.2 (중거리 명중률 20% 증가)
  - `AccuracyLong`: 1.2 (장거리 명중률 20% 증가)
- **특수 효과**: `ignoresAccuracyMaluses: true` (날씨/연기 명중률 페널티 무시)

### CustomGrip
- **statFactors**:
  - `AccuracyTouch`: 1.1 (근거리 명중률 10% 증가)
  - `AccuracyShort`: 1.1 (단거리 명중률 10% 증가)
- **statOffsets**:
  - `Beauty`: +2 (미관 증가)

---

## Pistol 카테고리

### StabilizerBrace
- **statFactors**:
  - `AccuracyTouch`: 1.2 (근거리 명중률 20% 증가)
  - `AccuracyShort`: 1.2 (단거리 명중률 20% 증가)
  - `AccuracyMedium`: 1.2 (중거리 명중률 20% 증가)
  - `AccuracyLong`: 1.2 (장거리 명중률 20% 증가)

### PulseCharger
- **statFactors**:
  - `AccuracyTouch`: 0.8 (근거리 명중률 20% 감소)
  - `AccuracyShort`: 0.8 (단거리 명중률 20% 감소)
  - `AccuracyMedium`: 0.8 (중거리 명중률 20% 감소)
  - `AccuracyLong`: 0.8 (장거리 명중률 20% 감소)
- **statOffsets**:
  - `RangedWeapon_DamageMultiplier`: +0.6 (데미지 60% 증가)
  - `RangedWeapon_ArmorPenetrationMultiplier`: +0.3 (관통력 30% 증가)

---

## BulletFiring 카테고리

### ToxRounds
- **특수 효과**: 
  - `damageDefOverride: Bullet_TraitTox` (독 데미지 타입으로 변경)
  - 독성 축적 효과

### PiercingRounds
- **statOffsets**:
  - `RangedWeapon_ArmorPenetrationMultiplier`: +0.2 (관통력 20% 증가)

### IncendiaryRounds
- **특수 효과**: 
  - `damageDefOverride: Bullet_TraitIncendiary` (화염 데미지 타입으로 변경)
  - 발화 효과

### EMPRounds
- **extraDamages**:
  - `EMP`: +4 (EMP 데미지 추가)

### HollowPointRounds
- **statOffsets**:
  - `RangedWeapon_DamageMultiplier`: +0.2 (데미지 20% 증가)

### HighPowerRounds
- **statOffsets**:
  - `RangedWeapon_DamageMultiplier`: +0.3 (데미지 30% 증가)
  - `RangedWeapon_ArmorPenetrationMultiplier`: +0.15 (관통력 15% 증가)
- **statFactors**:
  - `AccuracyTouch`: 0.9 (근거리 명중률 10% 감소)
  - `AccuracyShort`: 0.9 (단거리 명중률 10% 감소)
  - `AccuracyMedium`: 0.9 (중거리 명중률 10% 감소)
  - `AccuracyLong`: 0.9 (장거리 명중률 10% 감소)

---

## PelletFiring 카테고리

### ToxPellets
- **특수 효과**: 
  - `damageDefOverride: Bullet_TraitTox` (독 데미지 타입으로 변경)
  - 독성 축적 효과

### IncendiaryPellets
- **특수 효과**: 
  - `damageDefOverride: Bullet_TraitIncendiary` (화염 데미지 타입으로 변경)
  - 발화 효과

### BirdshotPellets
- **statOffsets**:
  - `RangedWeapon_ArmorPenetrationMultiplier`: -0.25 (관통력 25% 감소)
- **statFactors**:
  - `AccuracyTouch`: 1.3 (근거리 명중률 30% 증가)
  - `AccuracyShort`: 1.2 (단거리 명중률 20% 증가)
  - `AccuracyMedium`: 0.95 (중거리 명중률 5% 감소)
  - `AccuracyLong`: 0.9 (장거리 명중률 10% 감소)

---

## Bow 카테고리

### PiercingArrows
- **statOffsets**:
  - `RangedWeapon_ArmorPenetrationMultiplier`: +0.3 (관통력 30% 증가)

### ParalyticArrows
- **특수 효과**: 
  - `damageDefOverride: Nerve` (신경 데미지 타입으로 변경)
  - 기절 효과

### BroadheadArrows
- **statOffsets**:
  - `RangedWeapon_DamageMultiplier`: +0.3 (데미지 30% 증가)

### LightweightArrows
- **statOffsets**:
  - `RangedWeapon_RangeMultiplier`: +0.3 (사거리 30% 증가)
  - `RangedWeapon_DamageMultiplier`: -0.1 (데미지 10% 감소)

### ReinforcedLimbs
- **statOffsets**:
  - `RangedWeapon_RangeMultiplier`: +0.15 (사거리 15% 증가)

---

## Rifle 카테고리

### ExtendedBarrel
- **statOffsets**:
  - `RangedWeapon_RangeMultiplier`: +0.2 (사거리 20% 증가)

---

## Sighted 카테고리

### ImprovedSights
- **statFactors**:
  - `AccuracyMedium`: 1.2 (중거리 명중률 20% 증가)
  - `AccuracyLong`: 1.2 (장거리 명중률 20% 증가)

### ShoddySights
- **statFactors**:
  - `AccuracyMedium`: 0.8 (중거리 명중률 20% 감소)
  - `AccuracyLong`: 0.8 (장거리 명중률 20% 감소)

---

## Scoped 카테고리

### PrecisionScope
- **statOffsets**:
  - `RangedWeapon_WarmupMultiplier`: -0.25 (준비 시간 25% 감소)

### RangefinderScope
- **statOffsets**:
  - `RangedWeapon_RangeMultiplier`: +0.1 (사거리 10% 증가)

---

## Shotgun 카테고리

### ShortenedBarrel
- **statOffsets**:
  - `RangedWeapon_WarmupMultiplier`: -0.2 (준비 시간 20% 감소)
  - `RangedWeapon_RangeMultiplier`: -0.2 (사거리 20% 감소)

---

## BurstFire 카테고리

### RapidFire
- **burstShotSpeedMultiplier**: 2.0 (버스트 발사 속도 2배 증가)

### ExtendedMagazine
- **burstShotCountMultiplier**: 1.5 (버스트 발사 수 50% 증가)

---

## PulseCharge 카테고리

### ChargeCapacitor
- **statOffsets**:
  - `RangedWeapon_ArmorPenetrationMultiplier`: +0.2 (관통력 20% 증가)
  - `RangedWeapon_DamageMultiplier`: +0.35 (데미지 35% 증가)

### EMPPulser
- **특수 효과**: EMP 펄스 능력 추가 (능력 사용)

---

## BeamWeapon 카테고리

### FrequencyAmplifier
- **statOffsets**:
  - `RangedWeapon_RangeMultiplier`: +0.3 (사거리 30% 증가)
  - `RangedWeapon_DamageMultiplier`: +0.5 (데미지 50% 증가)
- **statFactors**:
  - `RangedWeapon_Cooldown`: 1.5 (쿨다운 50% 증가)

---

## LowStoppingPower 카테고리

### OversizedRounds
- **statOffsets**:
  - `RangedWeapon_DamageMultiplier`: +0.1 (데미지 10% 증가)
- **additionalStoppingPower**: +2 (스톱핑 파워 +2)

---

## Ranged 카테고리 (범용)

### Ornamental
- **statOffsets**:
  - `RangedWeapon_DamageMultiplier`: -0.15 (데미지 15% 감소)
  - `Beauty`: +5 (미관 증가)

### Ugly
- **statOffsets**:
  - `Beauty`: -4 (미관 감소)

### Lightweight
- **statOffsets**:
  - `RangedWeapon_WarmupMultiplier`: -0.2 (준비 시간 20% 감소)
- **statFactors**:
  - `Mass`: 0.75 (무게 25% 감소)

### Cumbersome
- **statOffsets**:
  - `RangedWeapon_WarmupMultiplier`: +0.2 (준비 시간 20% 증가)

### GoldInlay
- **statOffsets**:
  - `Beauty`: +20 (미관 증가)

### JadeInlay
- **statOffsets**:
  - `Beauty`: +10 (미관 증가)

---

## Attachable 카테고리

### GrenadeLauncher
- **특수 효과**: 수류탄 발사 능력 추가 (능력 사용)

### EMPLauncher
- **특수 효과**: EMP 포탄 발사 능력 추가 (능력 사용)

### SmokeLauncher
- **특수 효과**: 연막 포탄 발사 능력 추가 (능력 사용)

### IncendiaryLauncher
- **특수 효과**: 소이 포탄 발사 능력 추가 (능력 사용)

### BioferriteBurner
- **특수 효과**: 바이오페라이트 버너 능력 추가 (능력 사용)

---

## 성능 지표 요약

### 명중률 (Accuracy)
- **증가**: AimAssistance (+20% 전거리), StabilizerBrace (+20% 전거리), CustomGrip (+10% 근/단거리), ImprovedSights (+20% 중/장거리), BirdshotPellets (+30% 근거리, +20% 단거리)
- **감소**: PulseCharger (-20% 전거리), HighPowerRounds (-10% 전거리), ShoddySights (-20% 중/장거리), BirdshotPellets (-5% 중거리, -10% 장거리)

### 데미지 (Damage)
- **증가**: PulseCharger (+60%), HighPowerRounds (+30%), HollowPointRounds (+20%), BroadheadArrows (+30%), ChargeCapacitor (+35%), FrequencyAmplifier (+50%), OversizedRounds (+10%)
- **감소**: Ornamental (-15%), LightweightArrows (-10%)

### 관통력 (Armor Penetration)
- **증가**: PulseCharger (+30%), PiercingRounds (+20%), HighPowerRounds (+15%), PiercingArrows (+30%), ChargeCapacitor (+20%)
- **감소**: BirdshotPellets (-25%)

### 사거리 (Range)
- **증가**: ExtendedBarrel (+20%), RangefinderScope (+10%), LightweightArrows (+30%), ReinforcedLimbs (+15%), FrequencyAmplifier (+30%)
- **감소**: ShortenedBarrel (-20%)

### 쿨다운/준비시간 (Cooldown/Warmup)
- **쿨다운 감소**: QuickReload (쿨다운 20% 감소)
- **쿨다운 증가**: FrequencyAmplifier (쿨다운 50% 증가)
- **준비시간 감소**: PrecisionScope (-25%), ShortenedBarrel (-20%), Lightweight (-20%)
- **준비시간 증가**: Cumbersome (+20%)

### 발사 속도/수량
- **버스트 속도**: RapidFire (2배 증가)
- **버스트 수량**: ExtendedMagazine (50% 증가)

### 기타
- **스톱핑 파워**: OversizedRounds (+2)
- **추가 데미지**: EMPRounds (EMP +4)
- **데미지 타입 변경**: ToxRounds/ToxPellets (독), IncendiaryRounds/IncendiaryPellets (화염), ParalyticArrows (신경)
- **특수 효과**: AimAssistance (명중률 페널티 무시), EMPPulser/EMPLauncher (EMP 능력), 각종 런처 (발사 능력)
