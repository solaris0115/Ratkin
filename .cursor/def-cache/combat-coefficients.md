---
category: combat-coefficients
last_updated: 2026-03-08
sources:
  - RimworldData/Core/Defs/Misc/StuffCategoryDefs/StuffCategories.xml
  - RimworldData/Core/Defs/Stats/Stats_Stuff.xml
  - RimworldData/Core/Defs/Stats/Stats_Weapons_Melee.xml
  - RimworldData/Core/Defs/Stats/Stats_Weapons_Ranged.xml
  - RimworldData/Core/Defs/Stats/Stats_Apparel.xml
  - RimworldData/Core/Defs/ThingDefs_Items/Items_Resource_Stuff.xml
  - RimworldData/Core/Defs/ThingDefs_Items/Items_Resource_Stuff_Leather.xml
  - RimworldData/Core/Defs/ThingDefs_Misc/Various_Stone.xml
  - RimworldData/Anomaly/Defs/ThingDefs_Items/Items_Resource_Stuff.xml
  - RimworldData/Odyssey/Defs/ThingDefs_Items/Items_Resource_Stuff.xml
scope: 소재별 계수 + 등급별 전투 관련 계수
fields: stuffCategory, SharpDamageMultiplier, BluntDamageMultiplier, MeleeWeapon_CooldownMultiplier, StuffPower_Armor, qualityDamage, qualityArmor
note: 전투 관련 계수 = 소재(stuff) 계수 + 등급(quality) 계수
---
# 전투 관련 계수 정보 (Combat Coefficients)

소재(stuff)와 등급(quality)에 따른 공격/방어 계수를 정리한다.

---

## 0. 근접 무기(Melee) StatDef 요약

| StatDef | 카테고리 | 설명 | 소재/등급 적용 |
|---------|----------|------|----------------|
| MeleeWeapon_AverageDPS | Weapon_Melee | 평균 DPS (Worker 계산) | - |
| MeleeWeapon_AverageArmorPenetration | Weapon_Melee | 평균 관통력 (Worker 계산) | - |
| MeleeWeapon_DamageMultiplier | Weapon_Melee | 피해 배율 | **등급(quality)** |
| MeleeWeapon_CooldownMultiplier | StuffStatFactors | 공격 쿨다운 배율 | **소재(stuff)** |
| SharpDamageMultiplier | StuffStatFactors | 날카로운 공격 피해 배율 | **소재(stuff)** |
| BluntDamageMultiplier | StuffStatFactors | 둔한 공격 피해 배율 | **소재(stuff)** |

※ stuff O 무기: Sharp/Blunt/Cooldown는 소재에 따라, DamageMultiplier는 등급에 따라 적용

---

## 1. 등급별 계수 (Quality Tier Coefficients)

`StatPart_Quality`로 적용. CompQuality 보유 아이템에만 적용됨.

### 1.1 공격 계수

| StatDef | 설명 | Awful | Poor | Normal | Good | Excellent | Masterwork | Legendary |
|---------|------|-------|------|--------|------|-----------|------------|-----------|
| MeleeWeapon_DamageMultiplier | 근접 피해 배율 | 0.80 | 0.90 | 1.00 | 1.10 | 1.20 | 1.45 | 1.65 |
| RangedWeapon_DamageMultiplier | 원거리 피해 배율 | 0.90 | 1.00 | 1.00 | 1.00 | 1.00 | 1.25 | 1.50 |
| RangedWeapon_ArmorPenetrationMultiplier | 원거리 관통 배율 | 0.90 | 1.00 | 1.00 | 1.00 | 1.00 | 1.25 | 1.50 |
| AccuracyBase | 원거리 정확도 (Touch/Short/Medium/Long) | 0.80 | 0.90 | 1.00 | 1.10 | 1.20 | 1.35 | 1.50 |

※ 한글 등급: Awful=형편없음, Poor=하급, Normal=평범, Good=상급, Excellent=완벽, Masterwork=대장장이, Legendary=전설

### 1.2 방어 계수

| StatDef | 설명 | Awful | Poor | Normal | Good | Excellent | Masterwork | Legendary |
|---------|------|-------|------|--------|------|-----------|------------|-----------|
| ArmorRatingBase | 방어력 (Sharp/Blunt/Heat) | 0.60 | 0.80 | 1.00 | 1.15 | 1.30 | 1.45 | 1.80 |
| EnergyShieldEnergyMax | 방패 최대 에너지 | 0.60 | 0.80 | 1.00 | 1.20 | 1.40 | 1.70 | 2.10 |
| EnergyShieldRechargeRate | 방패 충전 속도 | 0.90 | 0.95 | 1.00 | 1.05 | 1.10 | 1.20 | 1.30 |

### 1.3 기타 전투 관련 (등급 적용)

| StatDef | Awful | Poor | Normal | Good | Excellent | Masterwork | Legendary |
|---------|-------|------|--------|------|-----------|------------|-----------|
| InsulationBase (냉/열 단열) | 0.80 | 0.90 | 1.00 | 1.10 | 1.20 | 1.50 | 1.80 |
| PackRadius (폭발팩 반경) | 0.84 | 0.92 | 1.00 | 1.08 | 1.16 | 1.30 | 1.50 |

---

## 2. 소재별 계수 (Stuff/Material Coefficients)

소재 ThingDef의 `statBases`에 정의. **stuff O** 무기/방어구에만 적용됨.

### 2.1 소재 카테고리 (StuffCategories)

`stuffProps.categories`에 정의. 제작 레시피의 `stuffCategories`로 소재 제한 시 사용됨.

| 카테고리 | 설명 | 대표 소재 |
|----------|------|-----------|
| Metallic | 금속 | Steel, Silver, Gold, Plasteel, Uranium |
| Woody | 나무 | Wood |
| Stony | 돌/석재 | Jade, Stone blocks, Obsidian |
| Fabric | 직물/천 | Cloth, Synthread, Devilstrand, Hyperweave, Wool |
| Leathery | 가죽 | Leather_Plain, Leather_Leathery, Thrumbo 등 모든 가죽 |

※ DLC: Bioferrite = Metallic+Bioferrite, Obsidian = Stony+Metallic

### 2.2 공격 관련 계수 (무기 소재)

| StatDef | 설명 | 기본값 |
|---------|------|--------|
| SharpDamageMultiplier | 날카로운 공격(Cut, Stab, Scratch, Bite 등) 피해 배율 | 1 |
| BluntDamageMultiplier | 둔한 공격(Blunt, Poke, Demolish 등) 피해 배율 | 1 |
| MeleeWeapon_CooldownMultiplier | 근접 공격 재사용 대기시간 배율 (높을수록 느림) | 1 |

**소재별 값 (무기 제작용)**:

| 소재 | 카테고리 | SharpDamageMultiplier | BluntDamageMultiplier | MeleeWeapon_CooldownMultiplier |
|------|----------|----------------------|------------------------|--------------------------------|
| Steel | Metallic | 1.0 | 1.0 | (1.0) |
| Silver | Metallic | 0.85 | 1.0 | (1.0) |
| Gold | Metallic | 0.75 | 1.0 | 1.0 |
| Plasteel | Metallic | 1.1 | 0.9 | **0.8** |
| Uranium | Metallic | 1.1 | 1.5 | 1.10 |
| Wood | Woody | 0.4 | 0.9 | (1.0) |
| Jade | Stony | (1.0) | 1.5 | 1.30 |
| Stone (기본) | Stony | 0.6 | 1.0 | 1.30 |
| Sandstone | Stony | 0.5 | 1.0 | 1.30 |
| Granite | Stony | 0.65 | 1.0 | 1.30 |
| Limestone/Slate/Marble | Stony | 0.6 | 1.0 | 1.30 |
| Obsidian (Odyssey) | Stony, Metallic | 1.4 | (1.0) | 1.0 |
| Bioferrite (Anomaly) | Metallic, Bioferrite | 1.3 | 0.9 | 1.0 |
| LabyrinthMatter | - | 0 | 0 | - |

※ 괄호: 명시 미정의 시 기본값 또는 부모(StoneBlocksBase 등) 상속값 적용  
※ MeleeWeapon_CooldownMultiplier: **낮을수록** 공격 속도 빠름 (Plasteel 0.8 = 20% 빠름)

### 2.3 방어력 관련 계수 (방어구 소재)

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
