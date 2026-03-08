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
fields: defName, craftable, stuff, power, cooldown, damageType
note: damageType = capacity(armorCategory). stuff O = steel/plasteel 등 재료에 따라 데미지 변동
---

# 근접 무기 (Melee Weapons)

## damageType 매핑 (capacity → armorCategory)

| capacity | damageType |
|----------|------------|
| Poke | blunt |
| Cut | sharp |
| Stab | sharp |
| Blunt | blunt |
| Scratch | sharp |
| Demolish | blunt |
| Bite | sharp |
| Flame | heat |
| Burn | heat |
| RK_HalberdCleave | sharp |
| RK_ToolCapacity_PickaxeStab | sharp |
| RK_ToolCapacity_ChainSword | sharp |
| RK_ToolCapacity_MeleeExplosion | sharp (Bomb) |
| GunlanceShell_Normal | sharp (Bomb, 원거리) |

## 림월드 (Core + DLC)

### Core (8개)

| defName | craftable | stuff | power | cooldown | damageType |
|---------|-----------|-------|-------|----------|------------|
| MeleeWeapon_Club | O | O | 9 | 2 | Poke(blunt) |
| MeleeWeapon_Club | O | O | 14 | 2 | Blunt(blunt) |
| MeleeWeapon_Knife | O | O | 9 | 2 | Blunt(blunt) |
| MeleeWeapon_Knife | O | O | 12 | 1.5 | Cut(sharp) |
| MeleeWeapon_Knife | O | O | 13 | 2 | Stab(sharp) |
| MeleeWeapon_Ikwa | O | O | 9 | 2 | Blunt(blunt); Poke(blunt) |
| MeleeWeapon_Ikwa | O | O | 15 | 2 | Stab(sharp) |
| MeleeWeapon_Ikwa | O | O | 15 | 2 | Cut(sharp) |
| MeleeWeapon_Spear | O | O | 13 | 2.6 | Blunt(blunt); Poke(blunt) |
| MeleeWeapon_Spear | O | O | 23 | 2.6 | Stab(sharp) |
| MeleeWeapon_Mace | O | O | 9 | 2 | Poke(blunt) |
| MeleeWeapon_Mace | O | O | 15.7 | 2 | Blunt(blunt) |
| MeleeWeapon_Gladius | O | O | 9 | 2 | Blunt(blunt) |
| MeleeWeapon_Gladius | O | O | 16 | 2 | Stab(sharp) |
| MeleeWeapon_Gladius | O | O | 16 | 2 | Cut(sharp) |
| MeleeWeapon_LongSword | O | O | 9 | 2 | Blunt(blunt) |
| MeleeWeapon_LongSword | O | O | 23 | 2.6 | Stab(sharp) |
| MeleeWeapon_LongSword | O | O | 23 | 2.6 | Cut(sharp) |
| MeleeWeapon_BreachAxe | O | O | 9 | 2 | Blunt(blunt); Poke(blunt) |
| MeleeWeapon_BreachAxe | O | O | 7.5 | 1 | Demolish(blunt) |

### Royalty (9개)

| defName | craftable | stuff | power | cooldown | damageType |
|---------|-----------|-------|-------|----------|------------|
| MeleeWeapon_Axe | O | O | 9 | 2 | Poke(blunt) |
| MeleeWeapon_Axe | O | O | 15 | 2 | Cut(sharp) |
| MeleeWeapon_Warhammer | O | O | 11 | 2.6 | Poke(blunt) |
| MeleeWeapon_Warhammer | O | O | 20 | 2.6 | Blunt(blunt) |
| MeleeWeapon_MonoSwordBladelink | X | X | 12 | 1.6 | Blunt(blunt) |
| MeleeWeapon_MonoSwordBladelink | X | X | 27 | 1.6 | Stab(sharp) |
| MeleeWeapon_MonoSwordBladelink | X | X | 27 | 1.6 | Cut(sharp) |
| MeleeWeapon_ZeusHammerBladelink | X | X | 15 | 1.6 | Poke(blunt) |
| MeleeWeapon_ZeusHammerBladelink | X | X | 31 | 2.2 | Blunt(blunt)+EMP |
| MeleeWeapon_PlasmaSwordBladelink | X | X | 12 | 1.6 | Blunt(blunt) |
| MeleeWeapon_PlasmaSwordBladelink | X | X | 23 | 2 | Stab(sharp)+Flame(heat) |
| MeleeWeapon_PlasmaSwordBladelink | X | X | 23 | 2 | Cut(sharp)+Flame(heat) |
| MeleeWeapon_MonoSword | X | X | 12 | 1.6 | Blunt(blunt) |
| MeleeWeapon_MonoSword | X | X | 25 | 2 | Stab(sharp) |
| MeleeWeapon_MonoSword | X | X | 25 | 2 | Cut(sharp) |
| MeleeWeapon_Zeushammer | X | X | 15 | 2 | Poke(blunt) |
| MeleeWeapon_Zeushammer | X | X | 31 | 3 | Blunt(blunt)+EMP |
| MeleeWeapon_PlasmaSword | X | X | 12 | 2 | Blunt(blunt) |
| MeleeWeapon_PlasmaSword | X | X | 21 | 2.6 | Stab(sharp)+Flame(heat) |
| MeleeWeapon_PlasmaSword | X | X | 21 | 2.6 | Cut(sharp)+Flame(heat) |
| MeleeWeapon_PsyfocusStaff | X | X | 12 | 2.6 | Blunt(blunt) |

### Odyssey (2개)

| defName | craftable | stuff | power | cooldown | damageType |
|---------|-----------|-------|-------|----------|------------|
| MastodonTusk | X | X | 20 | 2.8 | Scratch(sharp); Stab(sharp) |
| MastodonTusk | X | X | 10 | 2.2 | Blunt(blunt) |
| AlphaThrumboHorn | X | X | 32 | 1.6 | Scratch(sharp); Stab(sharp) |
| AlphaThrumboHorn | X | X | 9 | 1.4 | Blunt(blunt) |

## 랫킨

### Weapon_Melee.xml (9개)

| defName | craftable | stuff | power | cooldown | damageType |
|---------|-----------|-------|-------|----------|------------|
| RK_Dagger | O | O | 15 | 2 | Stab(sharp) |
| RK_Dagger | O | O | 12 | 1.5 | Cut(sharp) |
| RK_OneHanded | O | O | 21 | 2.4 | Stab(sharp) |
| RK_OneHanded | O | O | 21 | 2.4 | Cut(sharp) |
| RK_OneHanded | O | O | 10 | 1.8 | Blunt(blunt) |
| RK_Mace | O | O | 19 | 2.4 | Blunt(blunt) |
| RK_Mace | O | O | 10 | 2.2 | Blunt(blunt) |
| RK_LightLance | O | O | 27 | 2.5 | Stab(sharp) |
| RK_LightLance | O | O | 21 | 2.8 | Cut(sharp) |
| RK_LightLance | O | O | 13 | 2.8 | Blunt(blunt) |
| RK_TwoHanded | O | O | 12 | 2 | Blunt(blunt) |
| RK_TwoHanded | O | O | 27 | 2.9 | Stab(sharp) |
| RK_TwoHanded | O | O | 27 | 2.9 | Cut(sharp) |
| RK_HeavyLance | O | O | 32 | 3 | Stab(sharp) |
| RK_HeavyLance | O | O | 27 | 3.4 | Cut(sharp) |
| RK_HeavyLance | O | O | 16 | 3.4 | Blunt(blunt) |
| RK_LongSword | O | O | 23 | 2.1 | Stab(sharp) |
| RK_LongSword | O | O | 23 | 2.1 | Cut(sharp) |
| RK_LongSword | O | O | 12 | 1.8 | Blunt(blunt) |
| RK_Spear | O | O | 6.5 | 1.3 | Blunt(blunt); Poke(blunt) |
| RK_Spear | O | O | 11.5 | 1.3 | Stab(sharp) |
| RK_Halberd | O | O | 15 | 1.6 | Stab(sharp) |
| RK_Halberd | O | O | 23 | 3.2 | RK_HalberdCleave(sharp) |

### Weapon_Util.xml (5개)

| defName | craftable | stuff | power | cooldown | damageType |
|---------|-----------|-------|-------|----------|------------|
| RK_Axe | O | O | 8 | 2 | Blunt(blunt) |
| RK_Axe | O | O | 20 | 2.5 | Cut(sharp) |
| RK_Cleaver | O | O | 5 | 2 | Blunt(blunt) |
| RK_Cleaver | O | O | 12 | 1.8 | Cut(sharp) |
| RK_Hockey | O | O | 14 | 2 | Stab(sharp) |
| RK_Hockey | O | O | 14 | 2 | Cut(sharp) |
| RK_Fork | O | O | 14 | 2 | Stab(sharp) |
| RK_Fork | O | O | 14 | 2 | Cut(sharp) |
| RK_Pickaxe | O | O | 15 | 2.5 | Blunt(blunt) |
| RK_Pickaxe | O | O | 10 | 3 | RK_ToolCapacity_PickaxeStab(sharp) +Crush(blunt) |

### Weapon_DropOnly.xml (1개)

| defName | craftable | stuff | power | cooldown | damageType |
|---------|-----------|-------|-------|----------|------------|
| RK_MagicWand | X | X | 5 | 1.5 | RK_ToolCapacity_MeleeExplosion(sharp) |
| RK_MagicWand | X | X | 5 | 1.5 | RK_ToolCapacity_MeleeExplosion(sharp) |

### Weapon_HighTech.xml (3개)

| defName | craftable | stuff | power | cooldown | damageType |
|---------|-----------|-------|-------|----------|------------|
| RK_Weapon_Gunlance | O | O | 20 | 3 | Stab(sharp) |
| RK_Weapon_Gunlance | O | O | 25 | 3 | GunlanceShell_Normal(sharp) |
| RK_Weapon_ProtoChainSword | O | O | 20 | 3 | RK_ToolCapacity_ChainSword(sharp) |
| RK_Weapon_ProtoChainSword | O | O | 20 | 3 | RK_ToolCapacity_ChainSword(sharp) |
| RK_Weapon_ProtoFlameChainSword | O | O | 20 | 3 | RK_ToolCapacity_ChainSword(sharp)+Burn(heat) |
| RK_Weapon_ProtoFlameChainSword | O | O | 20 | 3 | RK_ToolCapacity_ChainSword(sharp)+Burn(heat) |
