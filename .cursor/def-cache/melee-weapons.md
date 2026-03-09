---
category: melee-weapons
last_updated: 2026-03-08
sources:
  - RimworldData/Core/Defs/ThingDefs_Misc/Weapons/MeleeNeolithic.xml
  - RimworldData/Core/Defs/ThingDefs_Misc/Weapons/MeleeMedieval.xml
  - RimworldData/Core/Defs/ThingDefs_Misc/Weapons/Breach.xml
  - RimworldData/Royalty/Defs/ThingDefs_Misc/Weapons/MeleeMedieval.xml
  - RimworldData/Royalty/Defs/ThingDefs_Misc/Weapons/MeleeBladelink.xml
  - RimworldData/Royalty/Defs/ThingDefs_Misc/Weapons/MeleeUltratech.xml
  - RimworldData/Royalty/Defs/ThingDefs_Misc/Weapons/PsychicWeapons.xml
  - RimworldData/Odyssey/Defs/ThingDefs_Items/Items_Exotic.xml
  - Project/1.6/Defs/ThingsDefs/Weapon_Melee.xml
  - Project/1.6/Defs/ThingsDefs/Weapon_Util.xml
  - Project/1.6/Defs/ThingsDefs/Weapon_DropOnly.xml
  - Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml
scope: 순수 근접 무기만 (원거리 무기의 근접 tools 제외)
fields: defName, 생산, power, cooldown, damageType
note: damageType = capacity(armorCategory). 생산 = stuff(소재선택, 등급/소재 계수) | Fixed Cost(고정재료, 등급만) | 생산 불가
---

# 근접 무기 (Melee Weapons)

## damageType 매핑 및 계수

- **[damage-type-mapping.md](damage-type-mapping.md)**: capacity → damageType, damageType → armorCategory
- **[combat-coefficients.md](combat-coefficients.md)**: 전투 관련 계수 (등급별 피해/방어, 소재별 계수)

**생산**: stuff = 소재 선택 (등급/소재 계수) | Fixed Cost = 고정 재료 (등급만) | 생산 불가

## 림월드 (Core + DLC)

### Core (8개)

| defName | 생산 | power | cooldown | damageType |
|---------|------|-------|----------|------------|
| MeleeWeapon_Club | stuff | 9 | 2 | Poke(blunt) |
| MeleeWeapon_Club | stuff | 14 | 2 | Blunt(blunt) |
| MeleeWeapon_Knife | stuff | 9 | 2 | Blunt(blunt) |
| MeleeWeapon_Knife | stuff | 12 | 1.5 | Cut(sharp) |
| MeleeWeapon_Knife | stuff | 13 | 2 | Stab(sharp) |
| MeleeWeapon_Ikwa | stuff | 9 | 2 | Blunt(blunt); Poke(blunt) |
| MeleeWeapon_Ikwa | stuff | 15 | 2 | Stab(sharp) |
| MeleeWeapon_Ikwa | stuff | 15 | 2 | Cut(sharp) |
| MeleeWeapon_Spear | stuff | 13 | 2.6 | Blunt(blunt); Poke(blunt) |
| MeleeWeapon_Spear | stuff | 23 | 2.6 | Stab(sharp) |
| MeleeWeapon_Mace | stuff | 9 | 2 | Poke(blunt) |
| MeleeWeapon_Mace | stuff | 15.7 | 2 | Blunt(blunt) |
| MeleeWeapon_Gladius | stuff | 9 | 2 | Blunt(blunt) |
| MeleeWeapon_Gladius | stuff | 16 | 2 | Stab(sharp) |
| MeleeWeapon_Gladius | stuff | 16 | 2 | Cut(sharp) |
| MeleeWeapon_LongSword | stuff | 9 | 2 | Blunt(blunt) |
| MeleeWeapon_LongSword | stuff | 23 | 2.6 | Stab(sharp) |
| MeleeWeapon_LongSword | stuff | 23 | 2.6 | Cut(sharp) |
| MeleeWeapon_BreachAxe | stuff | 9 | 2 | Blunt(blunt); Poke(blunt) |
| MeleeWeapon_BreachAxe | stuff | 7.5 | 1 | Demolish(blunt) |

### Royalty (9개)

| defName | 생산 | power | cooldown | damageType |
|---------|------|-------|----------|------------|
| MeleeWeapon_Axe | stuff | 9 | 2 | Poke(blunt) |
| MeleeWeapon_Axe | stuff | 15 | 2 | Cut(sharp) |
| MeleeWeapon_Warhammer | stuff | 11 | 2.6 | Poke(blunt) |
| MeleeWeapon_Warhammer | stuff | 20 | 2.6 | Blunt(blunt) |
| MeleeWeapon_MonoSwordBladelink | 생산 불가 | 12 | 1.6 | Blunt(blunt) |
| MeleeWeapon_MonoSwordBladelink | 생산 불가 | 27 | 1.6 | Stab(sharp) |
| MeleeWeapon_MonoSwordBladelink | 생산 불가 | 27 | 1.6 | Cut(sharp) |
| MeleeWeapon_ZeusHammerBladelink | 생산 불가 | 15 | 1.6 | Poke(blunt) |
| MeleeWeapon_ZeusHammerBladelink | 생산 불가 | 31 | 2.2 | Blunt(blunt)+EMP |
| MeleeWeapon_PlasmaSwordBladelink | 생산 불가 | 12 | 1.6 | Blunt(blunt) |
| MeleeWeapon_PlasmaSwordBladelink | 생산 불가 | 23 | 2 | Stab(sharp)+Flame(heat) |
| MeleeWeapon_PlasmaSwordBladelink | 생산 불가 | 23 | 2 | Cut(sharp)+Flame(heat) |
| MeleeWeapon_MonoSword | 생산 불가 | 12 | 1.6 | Blunt(blunt) |
| MeleeWeapon_MonoSword | 생산 불가 | 25 | 2 | Stab(sharp) |
| MeleeWeapon_MonoSword | 생산 불가 | 25 | 2 | Cut(sharp) |
| MeleeWeapon_Zeushammer | 생산 불가 | 15 | 2 | Poke(blunt) |
| MeleeWeapon_Zeushammer | 생산 불가 | 31 | 3 | Blunt(blunt)+EMP |
| MeleeWeapon_PlasmaSword | 생산 불가 | 12 | 2 | Blunt(blunt) |
| MeleeWeapon_PlasmaSword | 생산 불가 | 21 | 2.6 | Stab(sharp)+Flame(heat) |
| MeleeWeapon_PlasmaSword | 생산 불가 | 21 | 2.6 | Cut(sharp)+Flame(heat) |
| MeleeWeapon_PsyfocusStaff | 생산 불가 | 12 | 2.6 | Blunt(blunt) |

### Odyssey (2개)

| defName | 생산 | power | cooldown | damageType |
|---------|------|-------|----------|------------|
| MastodonTusk | 생산 불가 | 20 | 2.8 | Scratch(sharp); Stab(sharp) |
| MastodonTusk | 생산 불가 | 10 | 2.2 | Blunt(blunt) |
| AlphaThrumboHorn | 생산 불가 | 32 | 1.6 | Scratch(sharp); Stab(sharp) |
| AlphaThrumboHorn | 생산 불가 | 9 | 1.4 | Blunt(blunt) |

## 랫킨

### Weapon_Melee.xml (9개)

| defName | 생산 | power | cooldown | damageType |
|---------|------|-------|----------|------------|
| RK_Dagger | stuff | 15 | 2 | Stab(sharp) |
| RK_Dagger | stuff | 12 | 1.5 | Cut(sharp) |
| RK_OneHanded | stuff | 21 | 2.4 | Stab(sharp) |
| RK_OneHanded | stuff | 21 | 2.4 | Cut(sharp) |
| RK_OneHanded | stuff | 10 | 1.8 | Blunt(blunt) |
| RK_Mace | stuff | 19 | 2.4 | Blunt(blunt) |
| RK_Mace | stuff | 10 | 2.2 | Blunt(blunt) |
| RK_LightLance | Fixed Cost | 27 | 2.5 | Stab(sharp) |
| RK_LightLance | Fixed Cost | 21 | 2.8 | Cut(sharp) |
| RK_LightLance | Fixed Cost | 13 | 2.8 | Blunt(blunt) |
| RK_TwoHanded | stuff | 12 | 2 | Blunt(blunt) |
| RK_TwoHanded | stuff | 27 | 2.9 | Stab(sharp) |
| RK_TwoHanded | stuff | 27 | 2.9 | Cut(sharp) |
| RK_HeavyLance | stuff | 32 | 3 | Stab(sharp) |
| RK_HeavyLance | stuff | 27 | 3.4 | Cut(sharp) |
| RK_HeavyLance | stuff | 16 | 3.4 | Blunt(blunt) |
| RK_LongSword | stuff | 23 | 2.1 | Stab(sharp) |
| RK_LongSword | stuff | 23 | 2.1 | Cut(sharp) |
| RK_LongSword | stuff | 12 | 1.8 | Blunt(blunt) |
| RK_Spear | stuff | 6.5 | 1.3 | Blunt(blunt); Poke(blunt) |
| RK_Spear | stuff | 11.5 | 1.3 | Stab(sharp) |
| RK_Halberd | stuff | 15 | 1.6 | Stab(sharp) |
| RK_Halberd | stuff | 23 | 3.2 | RK_HalberdCleave(sharp) |

### Weapon_Util.xml (5개)

| defName | 생산 | power | cooldown | damageType |
|---------|------|-------|----------|------------|
| RK_Axe | stuff | 8 | 2 | Blunt(blunt) |
| RK_Axe | stuff | 20 | 2.5 | Cut(sharp) |
| RK_Cleaver | stuff | 5 | 2 | Blunt(blunt) |
| RK_Cleaver | stuff | 12 | 1.8 | Cut(sharp) |
| RK_Hockey | stuff | 14 | 2 | Stab(sharp) |
| RK_Hockey | stuff | 14 | 2 | Cut(sharp) |
| RK_Fork | stuff | 14 | 2 | Stab(sharp) |
| RK_Fork | stuff | 14 | 2 | Cut(sharp) |
| RK_Pickaxe | stuff | 15 | 2.5 | Blunt(blunt) |
| RK_Pickaxe | stuff | 10 | 3 | RK_ToolCapacity_PickaxeStab(sharp) +Crush(blunt) |

### Weapon_DropOnly.xml (1개)

| defName | 생산 | power | cooldown | damageType |
|---------|------|-------|----------|------------|
| RK_MagicWand | 생산 불가 | 5 | 1.5 | RK_ToolCapacity_MeleeExplosion(sharp) |
| RK_MagicWand | 생산 불가 | 5 | 1.5 | RK_ToolCapacity_MeleeExplosion(sharp) |

### Weapon_HighTech.xml (3개)

| defName | 생산 | power | cooldown | damageType |
|---------|------|-------|----------|------------|
| RK_Weapon_Gunlance | stuff | 20 | 3 | Stab(sharp) |
| RK_Weapon_Gunlance | stuff | 25 | 3 | GunlanceShell_Normal(sharp) |
| RK_Weapon_ProtoChainSword | Fixed Cost | 20 | 3 | RK_ToolCapacity_ChainSword(sharp) |
| RK_Weapon_ProtoChainSword | Fixed Cost | 20 | 3 | RK_ToolCapacity_ChainSword(sharp) |
| RK_Weapon_ProtoFlameChainSword | Fixed Cost | 20 | 3 | RK_ToolCapacity_ChainSword(sharp)+Burn(heat) |
| RK_Weapon_ProtoFlameChainSword | Fixed Cost | 20 | 3 | RK_ToolCapacity_ChainSword(sharp)+Burn(heat) |
