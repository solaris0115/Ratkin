# Def 데이터 캐시 레퍼런스

## 캐시 파일 포맷

모든 캐시 파일은 `.cursor/def-cache/{category}.md` 경로에 저장하며 아래 구조를 따른다.

### 메타데이터 블록 (YAML frontmatter)

```yaml
---
category: {kebab-case 카테고리명}
last_updated: {YYYY-MM-DD}
sources:
  - {탐색한 원본 경로 1}
  - {탐색한 원본 경로 2}
scope: {수집 범위 설명}
fields: {수집된 필드 목록, 쉼표 구분}
---
```

### 본문 구조

```markdown
# {카테고리 제목}

## 림월드 (Core + DLC)

### Core

| defName | field1 | field2 | ... |
|---------|--------|--------|-----|

### Royalty (해당 시)

| defName | field1 | field2 | ... |
|---------|--------|--------|-----|

## 랫킨

| defName | source | field1 | field2 | ... |
|---------|--------|--------|--------|-----|
```

- 림월드는 DLC별로 하위 섹션 분리 (데이터가 있는 DLC만)
- 랫킨은 `source` 컬럼으로 원본 파일 표기
- 테이블 셀 내 리스트는 `;` 구분자 사용

## 카테고리별 수집 가이드

### melee-weapons (근접 무기)

- **탐색 경로**: `ThingDefs_Misc/Weapons/Melee*.xml`, `Weapons/Breach.xml`, `Weapons/PsychicWeapons.xml`, `Odyssey/Defs/ThingDefs_Items/Items_Exotic.xml` (MastodonTusk, AlphaThrumboHorn)
- **필터**: 순수 근접 무기만 (원거리 무기의 근접 tools 제외)
- **기본 필드**: defName, tool.label, tool.capacities, tool.power, tool.cooldownTime
- **선택 필드**: armorPenetration, extraMeleeDamages, surpriseAttack

### ranged-weapons (원거리 무기)

- **탐색 경로**: `ThingDefs_Misc/Weapons/Ranged*.xml`, `Weapons/Grenades.xml`
- **필터**: Verb_Shoot 또는 Verb_LaunchProjectile 보유
- **기본 필드**: defName, statBases(MarketValue, Mass, AccuracyTouch/Short/Medium/Long, RangedWeapon_Cooldown)
- **선택 필드**: costList, costStuffCount, verbs(range, warmupTime, burstShotCount)

### apparel (방어구/의류)

- **탐색 경로**: `ThingDefs_Misc/Apparel/`
- **기본 필드**: defName, statBases(ArmorRating_Sharp, ArmorRating_Blunt, ArmorRating_Heat, Mass, MarketValue)
- **선택 필드**: apparel layers, bodyPartGroups, equippedStatOffsets

### buildings (건물)

- **탐색 경로**: `ThingDefs_Misc/Buildings/`
- **기본 필드**: defName, statBases(MaxHitPoints, WorkToBuild, Mass, MarketValue)
- **선택 필드**: costList, costStuffCount, size, passability

### research (연구)

- **탐색 경로**: `ResearchProjectDefs/`
- **기본 필드**: defName, baseCost, techLevel, prerequisites
- **선택 필드**: requiredResearchBuilding, requiredResearchFacilities

## Capacity -> DamageType 매핑

ToolCapacityDef는 ManeuverDef를 통해 DamageDef에 연결된다.

| Capacity | ManeuverDef | DamageDef | armorCategory |
|----------|-------------|-----------|---------------|
| Cut | Slash | Cut | Sharp |
| Stab | Stab | Stab | Sharp |
| Blunt | Smash | Blunt | Blunt |
| Poke | Poke | Poke | Blunt (BluntBase 상속) |
| Scratch | Scratch | Scratch | Sharp |
| Bite | Bite | Bite | Sharp |
| Demolish | Demolish | Demolish | Blunt |

### 랫킨 커스텀 Capacity

| Capacity | DamageDef | 비고 |
|----------|-----------|------|
| RK_HalberdCleave | RK_Damage_BonusCut | CutBase 상속, 30% 확률 추가 대상 |
| RK_ToolCapacity_PickaxeStab | RK_Damage_PickaxeStab | Stab 계열, 건물 추가 피해 |
| RK_ToolCapacity_MeleeExplosion | DemoBomb | 폭발 데미지, Sharp |
| RK_ToolCapacity_ChainSword | (커스텀) | 체인소드 전용 |
| GunlanceShell_Normal | Bomb | 건랜스 포격, 범위 폭발 |

## combat-coefficients (전투 관련 계수)

- **탐색 경로**: `Stats_Weapons_Melee.xml`, `Stats_Weapons_Ranged.xml`, `Stats_Apparel.xml`, `Stats_Stuff.xml`, Stuff ThingDefs
- **내용**: 등급별(quality) 피해/방어 계수 + 소재별(stuff) 공격/방어 계수
- **기본 필드**: StatDef별 factorAwful~factorLegendary, StuffPower_Armor_*, SharpDamageMultiplier, BluntDamageMultiplier

## armor-rating-formula (방어력 계산식)

- **탐색 경로**: `Stats_Apparel.xml`, `StatPart_Stuff.cs`, `StatPart_Quality.cs`, `StatDef.cs`
- **내용**: 품질·소재·고정방어력이 반영된 방어력 최종 계산식
- **기본 필드**: BaseArmor, StuffEffectMultiplierArmor, StuffPower_Armor_Sharp/Blunt/Heat, QualityFactor

## 네이밍 규칙

- 파일명: kebab-case (`melee-weapons.md`, `ranged-weapons.md`, `combat-coefficients.md`)
- 카테고리명: 영문 소문자, 하이픈 구분
- 복합 카테고리 가능: `melee-weapons-tools.md` (특정 필드 집합)
