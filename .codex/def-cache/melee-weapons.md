---
category: melee-weapons
last_updated: 2026-03-13
sources:
  - Project/1.6/Defs/ThingsDefs/Weapon_DropOnly.xml
  - Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml
  - Project/1.6/Defs/ThingsDefs/Weapon_Melee.xml
  - Project/1.6/Defs/ThingsDefs/Weapon_Util.xml
  - RimworldData/Core/Defs/ThingDefs_Misc/Weapons/Breach.xml
  - RimworldData/Core/Defs/ThingDefs_Misc/Weapons/MeleeMedieval.xml
  - RimworldData/Core/Defs/ThingDefs_Misc/Weapons/MeleeNeolithic.xml
  - RimworldData/Odyssey/Defs/ThingDefs_Items/Items_Exotic.xml
  - RimworldData/Royalty/Defs/ThingDefs_Misc/Weapons/MeleeBladelink.xml
  - RimworldData/Royalty/Defs/ThingDefs_Misc/Weapons/MeleeMedieval.xml
  - RimworldData/Royalty/Defs/ThingDefs_Misc/Weapons/MeleeUltratech.xml
  - RimworldData/Royalty/Defs/ThingDefs_Misc/Weapons/PsychicWeapons.xml
scope: 순수 근접 무기만 (원거리 무기의 근접 tools 제외)
fields: defName, 생산, damageType, power, piercing, cooldown, DPS
note: DPS/관통력 = 전설등급. 고정관통력(armorPenetration 지정)은 품질만, 미지정은 effective_damage×0.015
---

# 근접 무기 (Melee Weapons)

## damageType 매핑 및 계수

- **[damage-type-mapping.md](damage-type-mapping.md)**: capacity → damageType, damageType → armorCategory
- **[combat-coefficients.md](combat-coefficients.md)**: 전투 관련 계수 (등급별 피해/소재별 계수)

**생산**: stuff = 소재 선택 (등급/소재 계수) | Fixed Cost = 고정 재료 (등급만) | 생산 불가

## 림월드 (Core + DLC)

### Core (20 tool행)

| defName | 생산 | damageType | power | piercing | cooldown | DPS |
|---|---|---|---|---|---|---|
| MeleeWeapon_BreachAxe | stuff | Blunt(blunt); Poke(blunt) | 9.0 | 0.33 | 2.0 | 10.12 |
| MeleeWeapon_BreachAxe | stuff | Demolish(blunt) | 7.5 | 0.28 | 1.0 | 16.87 |
| MeleeWeapon_Mace | stuff | Poke(blunt) | 9.0 | 0.33 | 2.0 | 10.12 |
| MeleeWeapon_Mace | stuff | Blunt(blunt) | 15.7 | 0.58 | 2.0 | 17.66 |
| MeleeWeapon_Gladius | stuff | Blunt(blunt) | 9.0 | 0.33 | 2.0 | 10.12 |
| MeleeWeapon_Gladius | stuff | Stab(sharp) | 16.0 | 0.44 | 2.0 | 18.15 |
| MeleeWeapon_Gladius | stuff | Cut(sharp) | 16.0 | 0.44 | 2.0 | 18.15 |
| MeleeWeapon_LongSword | stuff | Blunt(blunt) | 9.0 | 0.33 | 2.0 | 10.12 |
| MeleeWeapon_LongSword | stuff | Stab(sharp) | 23.0 | 0.63 | 2.6 | 20.07 |
| MeleeWeapon_LongSword | stuff | Cut(sharp) | 23.0 | 0.63 | 2.6 | 20.07 |
| MeleeWeapon_Club | stuff | Poke(blunt) | 9.0 | 0.33 | 2.0 | 10.12 |
| MeleeWeapon_Club | stuff | Blunt(blunt) | 14.0 | 0.52 | 2.0 | 15.75 |
| MeleeWeapon_Knife | stuff | Blunt(blunt) | 9.0 | 0.33 | 2.0 | 10.12 |
| MeleeWeapon_Knife | stuff | Cut(sharp) | 12.0 | 0.33 | 1.5 | 18.15 |
| MeleeWeapon_Knife | stuff | Stab(sharp) | 13.0 | 0.35 | 2.0 | 14.75 |
| MeleeWeapon_Ikwa | stuff | Blunt(blunt); Poke(blunt) | 9.0 | 0.33 | 2.0 | 10.12 |
| MeleeWeapon_Ikwa | stuff | Stab(sharp) | 15.0 | 0.41 | 2.0 | 17.02 |
| MeleeWeapon_Ikwa | stuff | Cut(sharp) | 15.0 | 0.41 | 2.0 | 17.02 |
| MeleeWeapon_Spear | stuff | Blunt(blunt); Poke(blunt) | 13.0 | 0.48 | 2.6 | 11.25 |
| MeleeWeapon_Spear | stuff | Stab(sharp) | 23.0 | 0.82 | 2.6 | 20.07 |


### Royalty (21 tool행)

| defName | 생산 | damageType | power | piercing | cooldown | DPS |
|---|---|---|---|---|---|---|
| MeleeWeapon_MonoSwordBladelink | 생산 불가 | Blunt(blunt) | 12.0 | 0.3 | 1.6 | 12.37 |
| MeleeWeapon_MonoSwordBladelink | 생산 불가 | Stab(sharp) | 27.0 | 1.48 | 1.6 | 27.84 |
| MeleeWeapon_MonoSwordBladelink | 생산 불가 | Cut(sharp) | 27.0 | 1.48 | 1.6 | 27.84 |
| MeleeWeapon_ZeusHammerBladelink | 생산 불가 | Poke(blunt) | 15.0 | 0.37 | 1.6 | 15.47 |
| MeleeWeapon_ZeusHammerBladelink | 생산 불가 | Blunt(blunt)+EMP(sharp) | 31.0 | 0.77 | 2.2 | 23.25 |
| MeleeWeapon_PlasmaSwordBladelink | 생산 불가 | Blunt(blunt) | 12.0 | 0.3 | 1.6 | 12.37 |
| MeleeWeapon_PlasmaSwordBladelink | 생산 불가 | Stab(sharp)+Flame(heat) | 23.0 | 0.57 | 2.0 | 18.97 |
| MeleeWeapon_PlasmaSwordBladelink | 생산 불가 | Cut(sharp)+Flame(heat) | 23.0 | 0.57 | 2.0 | 18.97 |
| MeleeWeapon_Axe | stuff | Poke(blunt) | 9.0 | 0.33 | 2.0 | 10.12 |
| MeleeWeapon_Axe | stuff | Cut(sharp) | 15.0 | 0.41 | 2.0 | 17.02 |
| MeleeWeapon_Warhammer | stuff | Poke(blunt) | 11.0 | 0.41 | 2.6 | 9.52 |
| MeleeWeapon_Warhammer | stuff | Blunt(blunt) | 20.0 | 0.74 | 2.6 | 17.31 |
| MeleeWeapon_MonoSword | 생산 불가 | Blunt(blunt) | 12.0 | 0.3 | 1.6 | 12.37 |
| MeleeWeapon_MonoSword | 생산 불가 | Stab(sharp) | 25.0 | 1.48 | 2.0 | 20.62 |
| MeleeWeapon_MonoSword | 생산 불가 | Cut(sharp) | 25.0 | 1.48 | 2.0 | 20.62 |
| MeleeWeapon_Zeushammer | 생산 불가 | Poke(blunt) | 15.0 | 0.37 | 2.0 | 12.38 |
| MeleeWeapon_Zeushammer | 생산 불가 | Blunt(blunt)+EMP(sharp) | 31.0 | 0.77 | 3.0 | 17.05 |
| MeleeWeapon_PlasmaSword | 생산 불가 | Blunt(blunt) | 12.0 | 0.3 | 2.0 | 9.9 |
| MeleeWeapon_PlasmaSword | 생산 불가 | Stab(sharp)+Flame(heat) | 21.0 | 0.52 | 2.6 | 13.33 |
| MeleeWeapon_PlasmaSword | 생산 불가 | Cut(sharp)+Flame(heat) | 21.0 | 0.52 | 2.6 | 13.33 |
| MeleeWeapon_PsyfocusStaff | 생산 불가 | Blunt(blunt) | 12.0 | 0.3 | 2.6 | 7.62 |


### Odyssey (4 tool행)

| defName | 생산 | damageType | power | piercing | cooldown | DPS |
|---|---|---|---|---|---|---|
| MastodonTusk | 생산 불가 | Scratch(sharp); Stab(sharp) | 20.0 | 0.49 | 2.8 | 11.79 |
| MastodonTusk | 생산 불가 | Blunt(blunt) | 10.0 | 0.25 | 2.2 | 7.5 |
| AlphaThrumboHorn | 생산 불가 | Scratch(sharp); Stab(sharp) | 32.0 | 0.79 | 1.6 | 33.0 |
| AlphaThrumboHorn | 생산 불가 | Blunt(blunt) | 9.0 | 0.22 | 1.4 | 10.61 |


## 랫킨 (Ratkin)

### Weapon_Melee (23 tool행)

| defName | 생산 | damageType | power | piercing | cooldown | DPS |
|---|---|---|---|---|---|---|
| RK_Dagger | stuff | Stab(sharp) | 14.0 | 0.38 | 1.5 | 21.17 |
| RK_Dagger | stuff | Cut(sharp)+Stun(sharp)+Cut(sharp) | 12.0 | 0.33 | 1.5 | 18.15 |
| RK_OneHanded | stuff | Stab(sharp) | 21.0 | 0.57 | 2.4 | 19.85 |
| RK_OneHanded | stuff | Cut(sharp) | 21.0 | 0.57 | 2.4 | 19.85 |
| RK_OneHanded | stuff | Blunt(blunt) | 10.0 | 0.37 | 1.8 | 12.5 |
| RK_Mace | stuff | Blunt(blunt) | 20.0 | 0.74 | 2.4 | 18.75 |
| RK_Mace | stuff | Blunt(blunt) | 10.0 | 0.37 | 2.2 | 10.23 |
| RK_LightLance | Fixed Cost | Stab(sharp) | 27.0 | 0.67 | 2.5 | 17.82 |
| RK_LightLance | Fixed Cost | Cut(sharp) | 21.0 | 0.52 | 2.8 | 12.38 |
| RK_LightLance | Fixed Cost | Blunt(blunt) | 13.0 | 0.32 | 2.8 | 7.66 |
| RK_TwoHanded | stuff | Blunt(blunt) | 12.0 | 0.45 | 2.0 | 13.5 |
| RK_TwoHanded | stuff | Stab(sharp) | 24.0 | 0.65 | 2.5 | 21.78 |
| RK_TwoHanded | stuff | Cut(sharp) | 27.0 | 0.74 | 2.8 | 21.88 |
| RK_HeavyLance | stuff | Stab(sharp) | 32.0 | 0.87 | 3.0 | 24.2 |
| RK_HeavyLance | stuff | Cut(sharp) | 27.0 | 0.74 | 3.4 | 18.02 |
| RK_HeavyLance | stuff | Blunt(blunt) | 16.0 | 0.59 | 3.4 | 10.59 |
| RK_LongSword | stuff | Stab(sharp) | 23.0 | 0.63 | 2.1 | 24.85 |
| RK_LongSword | stuff | Cut(sharp) | 23.0 | 0.63 | 2.1 | 24.85 |
| RK_LongSword | stuff | Blunt(blunt) | 12.0 | 0.45 | 1.8 | 15.0 |
| RK_Spear | stuff | Blunt(blunt); Poke(blunt) | 6.5 | 0.24 | 1.3 | 11.25 |
| RK_Spear | stuff | Stab(sharp) | 11.5 | 0.82 | 1.3 | 20.07 |
| RK_Halberd | stuff | Stab(sharp) | 15.0 | 0.41 | 1.8 | 18.91 |
| RK_Halberd | stuff | RK_HalberdCleave(sharp) | 23.0 | 1.07 | 2.4 | 21.74 |


### Weapon_Util (10 tool행)

| defName | 생산 | damageType | power | piercing | cooldown | DPS |
|---|---|---|---|---|---|---|
| RK_Axe | stuff | Blunt(blunt) | 8.0 | 0.3 | 2.0 | 9.0 |
| RK_Axe | stuff | Cut(sharp) | 20.0 | 0.54 | 2.5 | 18.15 |
| RK_Cleaver | stuff | Blunt(blunt) | 5.0 | 0.19 | 2.0 | 5.62 |
| RK_Cleaver | stuff | Cut(sharp) | 12.0 | 0.33 | 1.8 | 15.12 |
| RK_Hockey | stuff | Stab(sharp) | 17.0 | 0.46 | 2.4 | 16.07 |
| RK_Hockey | stuff | Cut(sharp) | 14.0 | 0.38 | 2.0 | 15.88 |
| RK_Fork | stuff | Stab(sharp) | 17.0 | 0.46 | 2.4 | 16.07 |
| RK_Fork | stuff | Cut(sharp) | 14.0 | 0.38 | 2.0 | 15.88 |
| RK_Pickaxe | stuff | Blunt(blunt) | 20.0 | 0.74 | 3.0 | 15.0 |
| RK_Pickaxe | stuff | RK_ToolCapacity_PickaxeStab(sharp)+Crush(blunt) | 10.0 | 0.82 | 3.0 | 7.56 |


### Weapon_DropOnly (2 tool행)

| defName | 생산 | damageType | power | piercing | cooldown | DPS |
|---|---|---|---|---|---|---|
| RK_MagicWand | Fixed Cost | RK_ToolCapacity_MeleeExplosion(sharp) | 5.0 | 0.58 | 1.5 | 5.5 |
| RK_MagicWand | Fixed Cost | RK_ToolCapacity_MeleeExplosion(sharp) | 5.0 | 0.58 | 1.5 | 5.5 |


### Weapon_HighTech (6 tool행)

| defName | 생산 | damageType | power | piercing | cooldown | DPS |
|---|---|---|---|---|---|---|
| RK_Weapon_Gunlance | stuff | Stab(sharp) | 20.0 | 0.54 | 3.0 | 15.12 |
| RK_Weapon_Gunlance | stuff | GunlanceShell_Normal(sharp) | 25.0 | 0.68 | 3.0 | 18.91 |
| RK_Weapon_ProtoChainSword | Fixed Cost | RK_ToolCapacity_ChainSword(sharp) | 20.0 | 1.15 | 2.0 | 16.5 |
| RK_Weapon_ProtoChainSword | Fixed Cost | RK_ToolCapacity_ChainSword(sharp) | 20.0 | 1.15 | 2.0 | 16.5 |
| RK_Weapon_ProtoFlameChainSword | Fixed Cost | RK_ToolCapacity_ChainSword(sharp)+Burn(heat) | 20.0 | 1.15 | 2.0 | 16.5 |
| RK_Weapon_ProtoFlameChainSword | Fixed Cost | RK_ToolCapacity_ChainSword(sharp)+Burn(heat) | 20.0 | 1.15 | 2.0 | 16.5 |

