---
category: damage-type-mapping armorCategory capacity
last_updated: 2026-03-08
sources:
  - RimworldData/Core/Defs/DamageDefs/Damages_MeleeWeapon.xml
  - RimworldData/Core/Defs/DamageDefs/Damages_Misc.xml
  - RimworldData/Core/Defs/Misc/DamageArmorCategoryDef/DamageArmorCategoryDefs.xml
note: capacity = Tool의 capacities, damageType = DamageDef.defName, armorCategory = DamageDef.armorCategory
---

# damageType 매핑

## 1. capacity → damageType 매핑

| capacity | damageType | 비고 |
|----------|------------|------|
| Poke | Blunt | blunt 계열, Stab 워커 |
| Cut | Cut | sharp |
| Stab | Stab | sharp |
| Blunt | Blunt | blunt |
| Scratch | Scratch | sharp |
| Demolish | Demolish | blunt, 건물 피해 강화 |
| Bite | Bite | sharp |
| Flame | Flame | heat |
| Burn | Burn | heat |
| RK_HalberdCleave | Cut | sharp (랫킨) |
| RK_ToolCapacity_PickaxeStab | Stab | sharp (랫킨) |
| RK_ToolCapacity_ChainSword | Cut | sharp (랫킨) |
| RK_ToolCapacity_MeleeExplosion | Bomb | sharp (랫킨) |
| GunlanceShell_Normal | Bomb | sharp, 원거리 (랫킨) |

## 2. damageType별 아머 카테고리

### 2.1 DamageArmorCategoryDef (핵심 정의)

`RimworldData/Core/Defs/Misc/DamageArmorCategoryDef/DamageArmorCategoryDefs.xml`

| armorCategory | multStat (공격) | armorRatingStat (방어) |
|---------------|-----------------|------------------------|
| Sharp | SharpDamageMultiplier | ArmorRating_Sharp |
| Blunt | BluntDamageMultiplier | ArmorRating_Blunt |
| Heat | (없음) | ArmorRating_Heat |

※ Heat는 무기 소재 배율 없음. Flame/Burn은 고정 피해.

### 2.2 DamageDef → armorCategory 매핑

| damageType | armorCategory | 출처 |
|------------|---------------|------|
| Cut | Sharp | Damages_MeleeWeapon |
| Stab | Sharp | Damages_MeleeWeapon |
| Scratch | Sharp | Damages_MeleeWeapon |
| Bite | Sharp | Damages_MeleeWeapon |
| Blunt | Blunt | Damages_MeleeWeapon |
| Crush | Blunt | Damages_MeleeWeapon |
| Poke | Blunt | Damages_MeleeWeapon (BluntBase 상속) |
| Demolish | Blunt | Damages_MeleeWeapon (BluntBase 상속) |
| Flame | Heat | Damages_Misc 등 |
| Burn | Heat | Damages_Misc 등 |
| Bomb | Sharp | Damages_Misc |
| Thump | Sharp | Damages_Misc |
| Vaporize | Heat | Damages_Misc |
| Beam | Heat | Damages_Misc |
| AcidBurn | Sharp | Damages_Misc |
