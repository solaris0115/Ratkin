---
category: material-coefficient stuff-category SharpDamageMultiplier BluntDamageMultiplier StuffPower_Armor
last_updated: 2026-03-08
sources:
  - RimworldData/Core/Defs/Misc/StuffCategoryDefs/StuffCategories.xml
  - RimworldData/Core/Defs/Stats/Stats_Stuff.xml
  - RimworldData/Core/Defs/Stats/Stats_Weapons_Melee.xml
  - RimworldData/Core/Defs/ThingDefs_Items/Items_Resource_Stuff.xml
  - RimworldData/Core/Defs/ThingDefs_Items/Items_Resource_Stuff_Leather.xml
  - RimworldData/Core/Defs/ThingDefs_Misc/Various_Stone.xml
  - RimworldData/Anomaly/Defs/ThingDefs_Items/Items_Resource_Stuff.xml
  - RimworldData/Odyssey/Defs/ThingDefs_Items/Items_Resource_Stuff.xml
note: stuffProps.categories = 제작 가능 아이템의 소재 제한
---

# 소재별 계수 (Material Coefficients)

## 1. 소재 카테고리 (StuffCategories)

`stuffProps.categories`에 정의. 제작 레시피의 `stuffCategories`로 소재 제한 시 사용됨.

| 카테고리 | 설명 | 대표 소재 |
|----------|------|-----------|
| Metallic | 금속 | Steel, Silver, Gold, Plasteel, Uranium |
| Woody | 나무 | Wood |
| Stony | 돌/석재 | Jade, Stone blocks, Obsidian |
| Fabric | 직물/천 | Cloth, Synthread, Devilstrand, Hyperweave, Wool |
| Leathery | 가죽 | Leather_Plain, Leather_Leathery, Thrumbo 등 모든 가죽 |

※ DLC: Bioferrite = Metallic+Bioferrite, Obsidian = Stony+Metallic

## 2. 공격 관련 계수 (무기 소재)

소재 ThingDef의 `statBases`에 정의. **stuff O** 무기에만 적용됨.

| StatDef | 설명 | 기본값 |
|---------|------|--------|
| SharpDamageMultiplier | 날카로운 공격(Cut, Stab, Scratch, Bite 등) 피해 배율 | 1 |
| BluntDamageMultiplier | 둔한 공격(Blunt, Poke, Demolish 등) 피해 배율 | 1 |

**소재별 값 (무기 제작용)**:

| 소재 | 카테고리 | SharpDamageMultiplier | BluntDamageMultiplier |
|------|----------|----------------------|------------------------|
| Steel | Metallic | 1.0 | 1.0 |
| Silver | Metallic | 0.85 | 1.0 |
| Gold | Metallic | 0.75 | 1.0 |
| Plasteel | Metallic | 1.1 | 0.9 |
| Uranium | Metallic | 1.1 | 1.5 |
| Wood | Woody | 0.4 | 0.9 |
| Jade | Stony | (1.0) | 1.5 |
| Stone (기본) | Stony | 0.6 | 1.0 |
| Sandstone | Stony | 0.5 | 1.0 |
| Granite | Stony | 0.65 | 1.0 |
| Obsidian (Odyssey) | Stony, Metallic | 1.4 | (1.0) |
| Bioferrite (Anomaly) | Metallic, Bioferrite | 1.3 | 0.9 |
| LabyrinthMatter | - | 0 | 0 |

※ 괄호: 명시 미정의 시 기본값 1.0 적용

## 3. 방어력 관련 계수 (방어구 소재)

소재 ThingDef의 `statBases`에 정의. **의류/방어구**의 ArmorRating 계산에 사용됨.

| StatDef | 설명 | 적용 대상 |
|---------|------|-----------|
| StuffPower_Armor_Sharp | 날카로운 공격 방어 계수 | ArmorRating_Sharp |
| StuffPower_Armor_Blunt | 둔한 공격 방어 계수 | ArmorRating_Blunt |
| StuffPower_Armor_Heat | 열 공격 방어 계수 | ArmorRating_Heat |

**주요 소재별 값**:

| 소재 | 카테고리 | StuffPower_Armor_Sharp | StuffPower_Armor_Blunt | StuffPower_Armor_Heat |
|------|----------|------------------------|------------------------|------------------------|
| Cloth | Fabric | 0.36 | 0 | 0.18 |
| Synthread | Fabric | 0.94 | 0.26 | 0.90 |
| Devilstrand | Fabric | 1.40 | 0.36 | 3.00 |
| Hyperweave | Fabric | 2.00 | 0.54 | 2.88 |
| Wool (기본) | Fabric | 0.36 | 0 | 1.10 |
| Leather (기본) | Leathery | 0.81 | 0.24 | 1.5 |
| Thrumbo | Leathery | 2.08 | 0.36 | - |
| Steel | Metallic | 0.9 | 0.45 | 0.60 |
| Plasteel | Metallic | 1.14 | 0.55 | 0.65 |
| Uranium | Metallic | 1.08 | 0.54 | 0.65 |
| Wood | Woody | 0.54 | 0.54 | 0.40 |
| Bioferrite | Metallic, Bioferrite | 1.1 | 0.5 | 0.5 |
