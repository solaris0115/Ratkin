# 근접 공격 관련 스탯 리스트

## 개요
근접 공격과 관련된 모든 StatDef의 defName을 중심으로 간결하게 정리한 보고서입니다.

## Pawn 전투 스탯 (PawnCombat)

### 1. MeleeDPS
- **defName**: `MeleeDPS`
- **설명**: 근접 전투에서 초당 평균 데미지

### 2. MeleeDamageFactor
- **defName**: `MeleeDamageFactor`
- **설명**: 근접 데미지 배율 (곱셈)

### 3. MeleeCooldownFactor
- **defName**: `MeleeCooldownFactor`
- **설명**: 근접 공격 후 쿨다운 시간 배율

### 4. MeleeArmorPenetration
- **defName**: `MeleeArmorPenetration`
- **설명**: 근접 전투에서 평균 방어관통률

### 5. MeleeHitChance
- **defName**: `MeleeHitChance`
- **설명**: 근접 공격 명중률

### 6. MeleeDodgeChance
- **defName**: `MeleeDodgeChance`
- **설명**: 근접 공격 회피 확률

## 무기 스탯 (Weapon_Melee)

### 7. MeleeWeapon_AverageDPS
- **defName**: `MeleeWeapon_AverageDPS`
- **설명**: 근접 무기 초당 평균 데미지

### 8. MeleeWeapon_AverageArmorPenetration
- **defName**: `MeleeWeapon_AverageArmorPenetration`
- **설명**: 근접 무기 평균 방어관통률

### 9. MeleeWeapon_DamageMultiplier
- **defName**: `MeleeWeapon_DamageMultiplier`
- **설명**: 근접 무기 데미지 배율

### 10. MeleeWeapon_CooldownMultiplier
- **defName**: `MeleeWeapon_CooldownMultiplier`
- **설명**: 무기 재료에 따른 공격 딜레이 배율

## 재료 데미지 배율 (StuffStatFactors)

### 11. SharpDamageMultiplier
- **defName**: `SharpDamageMultiplier`
- **설명**: 날카로운 공격 타입 데미지 배율 (재료별)

### 12. BluntDamageMultiplier
- **defName**: `BluntDamageMultiplier`
- **설명**: 둔기 공격 타입 데미지 배율 (재료별)

## 프로젝트 내 사용 현황

### MeleeHitChance
- `RK_LightLance`, `RK_HeavyLance`, `RK_Halberd`, `RK_HighTech_Weapon`, `RK_Apparel_Util`

### MeleeDodgeChance
- `Ratkin` 종족, `RK_Ability_MeleeMoraleBooster`, 각종 방어구

## 요약

**총 12개 스탯**
- Pawn 전투 스탯: 6개
- 무기 스탯: 4개
- 재료 스탯: 2개
