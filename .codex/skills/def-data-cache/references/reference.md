# Def Data Cache Reference

## 캐시 파일 포맷

캐시는 `.codex/def-cache/{category}.md`에 둔다.

```yaml
---
category: kebab-case-name
last_updated: YYYY-MM-DD
sources:
  - source/path
scope: collected scope
fields: field1, field2, field3
---
```

본문은 가능한 한 RimWorld 원본과 Ratkin 데이터를 분리한다.

```markdown
# Category Title

## RimWorld

### Core

| defName | field1 | field2 |
| --- | --- | --- |

## Ratkin

| defName | source | field1 | field2 |
| --- | --- | --- | --- |
```

테이블 셀 안의 리스트는 `;`로 구분한다.

## 카테고리별 수집 가이드

### melee-weapons

- 탐색: `ThingDefs_Misc/Weapons/Melee*.xml`, `Weapons/Breach.xml`, `Weapons/PsychicWeapons.xml`, `Odyssey/Defs/ThingDefs_Items/Items_Exotic.xml`
- 필터: 순수 근접 무기
- 기본 필드: `defName`, `tool.label`, `tool.capacities`, `tool.power`, `tool.cooldownTime`
- 선택 필드: `armorPenetration`, `extraMeleeDamages`, `surpriseAttack`

### ranged-weapons

- 탐색: `ThingDefs_Misc/Weapons/Ranged*.xml`, `Weapons/Grenades.xml`
- 필터: `Verb_Shoot` 또는 `Verb_LaunchProjectile`
- 기본 필드: `defName`, `MarketValue`, `Mass`, `AccuracyTouch`, `AccuracyShort`, `AccuracyMedium`, `AccuracyLong`, `RangedWeapon_Cooldown`
- 선택 필드: `costList`, `costStuffCount`, `verbs(range, warmupTime, burstShotCount)`

### apparel

- 탐색: `ThingDefs_Misc/Apparel/`
- 기본 필드: `defName`, `ArmorRating_Sharp`, `ArmorRating_Blunt`, `ArmorRating_Heat`, `Mass`, `MarketValue`
- 선택 필드: `apparel.layers`, `bodyPartGroups`, `equippedStatOffsets`

### buildings

- 탐색: `ThingDefs_Misc/Buildings/`
- 기본 필드: `defName`, `MaxHitPoints`, `WorkToBuild`, `Mass`, `MarketValue`
- 선택 필드: `costList`, `costStuffCount`, `size`, `passability`

### research

- 탐색: `ResearchProjectDefs/`
- 기본 필드: `defName`, `baseCost`, `techLevel`, `prerequisites`
- 선택 필드: `requiredResearchBuilding`, `requiredResearchFacilities`

## Capacity to DamageDef

ToolCapacityDef는 ManeuverDef를 거쳐 DamageDef에 연결된다.

| Capacity | ManeuverDef | DamageDef | armorCategory |
| --- | --- | --- | --- |
| Cut | Slash | Cut | Sharp |
| Stab | Stab | Stab | Sharp |
| Blunt | Smash | Blunt | Blunt |
| Poke | Poke | Poke | Blunt |
| Scratch | Scratch | Scratch | Sharp |
| Bite | Bite | Bite | Sharp |
| Demolish | Demolish | Demolish | Blunt |

## 네이밍

- 파일명: kebab-case
- 카테고리명: 영문 소문자와 하이픈
- 복합 카테고리 허용: `melee-weapons-tools.md`
