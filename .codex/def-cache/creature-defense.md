---
category: creature-defense
last_updated: 2026-03-24
sources:
  - RimworldData/Core/Defs/ThingDefs_Races/
  - RimworldData/Ideology/Defs/ThingDefs_Races/
  - RimworldData/Biotech/Defs/ThingDefs_Races/
  - RimworldData/Anomaly/Defs/ThingDefs_Races/
  - RimworldData/Odyssey/Defs/ThingDefs_Races/
scope: 림월드 바닐라(Core+DLC) 모든 생명체의 방어/피격 관련 데이터
fields: defName, ArmorRating_Sharp, ArmorRating_Blunt, ArmorRating_Heat, Flammability, MeleeDodgeChance, baseBodySize, baseHealthScale, MoveSpeed, PsychicSensitivity
---

# 생명체 방어 데이터 (Creature Defense)

## 수집 기준

- **ArmorRating**: ThingDef statBases 값. 상속 체인 완전 해석. StatDef 기본값=0
- **Flammability**: 발화성. StatDef 기본값=0, BasePawn=0.7, BaseMechanoid=0, BaseFleshbeast=1.25
- **MeleeDodgeChance**: StatDef 기본값=0. 대부분 미지정 → 스킬 기반 계산 (Melee스킬x1 + Movingx18 + Sightx8 → postProcessCurve)
- **baseBodySize**: 기본값=1. 피격 면적·근접 피해 보정에 영향
- **baseHealthScale**: 기본값=1. 전체 HP 배율
- **MoveSpeed**: 이동속도. StatDef 기본값=0
- **PsychicSensitivity**: StatDef 기본값=1

## Core

| defName | Sharp | Blunt | Heat | Flamm | Dodge | BodySize | HpScale | MoveSpd | PsySens |
|---------|-------|-------|------|-------|-------|----------|---------|---------|---------|
| Alpaca | 0 | 0 | 0 | 0.7 | 0 | 1.0 | 1.0 | 4.1 | 1 |
| Alphabeaver | 0 | 0 | 0 | 0.7 | 0 | 0.45 | 0.7 | 3.7 | 1 |
| Bear_Grizzly | 0 | 0 | 0 | 0.7 | 0 | 2.15 | 2.5 | 4.6 | 1 |
| Bear_Polar | 0 | 0 | 0 | 0.7 | 0 | 2.15 | 2.5 | 4.6 | 1 |
| Bison | 0 | 0 | 0 | 0.7 | 0 | 2.4 | 1.75 | 4.7 | 1 |
| Boomalope | 0 | 0 | 0 | 0.7 | 0 | 2.0 | 0.65 | 3.4 | 1 |
| Boomrat | 0 | 0 | 0 | 0.7 | 0 | 0.2 | 0.4 | 4.6 | 1 |
| Capybara | 0 | 0 | 0 | 0.7 | 0 | 0.75 | 0.7 | 3.9 | 1 |
| Caribou | 0 | 0 | 0 | 0.7 | 0 | 1.0 | 2.0 | 5 | 1 |
| Cassowary | 0 | 0 | 0 | 0.7 | 0 | 0.6 | 0.8 | 4.6 | 1 |
| Cat | 0 | 0 | 0 | 0.7 | 0 | 0.32 | 0.42 | 4.4 | 1 |
| Chicken | 0 | 0 | 0 | 0.7 | 0 | 0.3 | 0.35 | 2.1 | 1 |
| Chinchilla | 0 | 0 | 0 | 0.7 | 0 | 0.35 | 0.4 | 5.0 | 1 |
| Cobra | 0 | 0 | 0 | 0.7 | 0 | 0.25 | 0.5 | 3.5 | 1 |
| Cougar | 0 | 0 | 0 | 0.7 | 0 | 1.0 | 1.3 | 5.0 | 1 |
| Cow | 0 | 0 | 0 | 0.7 | 0 | 2.4 | 1.5 | 3.2 | 1 |
| Deer | 0 | 0 | 0 | 0.7 | 0 | 1.2 | 0.9 | 5.5 | 1 |
| Donkey | 0 | 0 | 0 | 0.7 | 0 | 1.4 | 1.45 | 5.0 | 1 |
| Dromedary | 0 | 0 | 0 | 0.7 | 0 | 2.1 | 1.6 | 4.3 | 1 |
| Duck | 0 | 0 | 0 | 0.7 | 0 | 0.3 | 0.35 | 2.1 | 1 |
| Elephant | 0 | 0 | 0 | 0.7 | 0 | 4.0 | 3.6 | 4.8 | 1 |
| Elk | 0 | 0 | 0 | 0.7 | 0 | 2.1 | 1.9 | 5 | 1 |
| Emu | 0 | 0 | 0 | 0.7 | 0 | 0.6 | 0.9 | 5.5 | 1 |
| Fox_Arctic | 0 | 0 | 0 | 0.7 | 0 | 0.55 | 0.70 | 4.6 | 1 |
| Fox_Fennec | 0 | 0 | 0 | 0.7 | 0 | 0.55 | 0.70 | 4.6 | 1 |
| Fox_Red | 0 | 0 | 0 | 0.7 | 0 | 0.55 | 0.70 | 4.6 | 1 |
| Gazelle | 0 | 0 | 0 | 0.7 | 0 | 0.7 | 0.7 | 6.0 | 1 |
| Goat | 0 | 0 | 0 | 0.7 | 0 | 0.75 | 0.7 | 3.9 | 1 |
| Goose | 0 | 0 | 0 | 0.7 | 0 | 0.60 | 0.40 | 2.3 | 1 |
| GuineaPig | 0 | 0 | 0 | 0.7 | 0 | 0.2 | 0.4 | 5.0 | 1 |
| Hare | 0 | 0 | 0 | 0.7 | 0 | 0.2 | 0.4 | 6.0 | 1 |
| Horse | 0 | 0 | 0 | 0.7 | 0 | 2.4 | 1.75 | 5.8 | 1 |
| Human | 0 | 0 | 0 | 0.7 | 0 | 1 | 1 | 4.6 | 1 |
| Husky | 0 | 0 | 0 | 0.7 | 0 | 0.86 | 1.05 | 5.0 | 1 |
| Ibex | 0 | 0 | 0 | 0.7 | 0 | 1.00 | 0.85 | 4.6 | 1 |
| Iguana | 0 | 0 | 0 | 0.7 | 0 | 0.4 | 0.5 | 3.0 | 1 |
| LabradorRetriever | 0 | 0 | 0 | 0.7 | 0 | 0.75 | 1.0 | 5.0 | 1 |
| Lynx | 0 | 0 | 0 | 0.7 | 0 | 0.6 | 0.8 | 5.0 | 1 |
| Mech_Lancer | 0.40 | 0.20 | 2.00 | 0 | 0 | 1.0 | 0.72 | 4.7 | 0.5 |
| Mech_Pikeman | 0.40 | 0.20 | 2.00 | 0 | 0 | 1.0 | 0.85 | 2.5 | 0.5 |
| Mech_Scyther | 0.40 | 0.20 | 2.00 | 0 | 0 | 1.0 | 1.32 | 4.7 | 0.5 |
| Mech_Termite | 0.56 | 0.22 | 2.00 | 0 | 0 | 1.6 | 2.16 | 2.1 | 0.75 |
| Megascarab | 0.72 | 0.18 | 0 | 0.7 | 0 | 0.2 | 0.4 | 3.75 | 1 |
| Megasloth | 0 | 0 | 0 | 0.7 | 0 | 4.0 | 3.6 | 4.8 | 1 |
| Megaspider | 0.27 | 0.18 | 0 | 0.7 | 0 | 1.2 | 2.5 | 3.60 | 1 |
| Monkey | 0 | 0 | 0 | 0.7 | 0 | 0.35 | 0.45 | 4.3 | 1 |
| Muffalo | 0 | 0 | 0 | 0.7 | 0 | 2.4 | 1.75 | 4.5 | 1 |
| Ostrich | 0 | 0 | 0 | 0.7 | 0 | 1.0 | 1.0 | 6.0 | 1 |
| Pig | 0 | 0 | 0 | 0.7 | 0 | 1.7 | 0.7 | 3.9 | 1 |
| Raccoon | 0 | 0 | 0 | 0.7 | 0 | 0.4 | 0.4 | 4.1 | 1 |
| Rat | 0 | 0 | 0 | 0.7 | 0 | 0.2 | 0.29 | 4.0 | 1 |
| Rhinoceros | 0 | 0 | 0 | 0.7 | 0 | 3.0 | 3.5 | 5.0 | 1 |
| Sheep | 0 | 0 | 0 | 0.7 | 0 | 0.75 | 0.7 | 4.8 | 1 |
| Snowhare | 0 | 0 | 0 | 0.7 | 0 | 0.2 | 0.4 | 6.0 | 1 |
| Spelopede | 0.18 | 0.18 | 0 | 0.7 | 0 | 0.8 | 1.7 | 3.65 | 1 |
| Squirrel | 0 | 0 | 0 | 0.7 | 0 | 0.2 | 0.25 | 5.1 | 1 |
| Thrumbo | 0.60 | 0.40 | 0.30 | 0.7 | 0 | 4 | 8.0 | 5.5 | 1 |
| Tortoise | 0.50 | 0.35 | 0 | 0.7 | 0 | 0.5 | 0.6 | 1.0 | 1 |
| Turkey | 0 | 0 | 0 | 0.7 | 0 | 0.6 | 0.6 | 3.6 | 1 |
| Warg | 0 | 0 | 0 | 0.7 | 0 | 1.4 | 1.4 | 5.0 | 1 |
| WildBoar | 0 | 0 | 0 | 0.7 | 0 | 0.85 | 0.7 | 4.6 | 1 |
| Wolf_Arctic | 0 | 0 | 0 | 0.7 | 0 | 0.85 | 0.99 | 5.0 | 1 |
| Wolf_Timber | 0 | 0 | 0 | 0.7 | 0 | 0.85 | 0.99 | 5.0 | 1 |
| Yak | 0 | 0 | 0 | 0.7 | 0 | 2.1 | 1.5 | 3.2 | 1 |
| YorkshireTerrier | 0 | 0 | 0 | 0.7 | 0 | 0.32 | 0.4 | 3.1 | 1 |

## Ideology

| defName | Sharp | Blunt | Heat | Flamm | Dodge | BodySize | HpScale | MoveSpd | PsySens |
|---------|-------|-------|------|-------|-------|----------|---------|---------|---------|
| Dryad_Barkskin | 0.70 | 0.40 | 0 | 0.7 | 0 | 0.65 | 0.9 | 3.2 | 1 |
| Dryad_Basic | 0 | 0 | 0 | 0.7 | 0 | 0.667 | 0.8 | 3 | 1 |
| Dryad_Berrymaker | 0 | 0 | 0 | 0.7 | 0 | 0.667 | 0.8 | 3 | 1 |
| Dryad_Carrier | 0 | 0 | 0 | 0.7 | 0 | 0.667 | 0.8 | 2.4 | 1 |
| Dryad_Clawer | 0 | 0 | 0 | 0.7 | 0 | 0.667 | 0.7 | 4 | 1 |
| Dryad_Gaumaker | 0 | 0 | 0 | 0.7 | 0 | 0.667 | 0.8 | 3 | 1 |
| Dryad_Medicinemaker | 0 | 0 | 0 | 0.7 | 0 | 0.667 | 0.8 | 3 | 1 |
| Dryad_Woodmaker | 0 | 0 | 0 | 0.7 | 0 | 0.667 | 0.8 | 3 | 1 |

## Biotech

| defName | Sharp | Blunt | Heat | Flamm | Dodge | BodySize | HpScale | MoveSpd | PsySens |
|---------|-------|-------|------|-------|-------|----------|---------|---------|---------|
| Mech_Agrihand | 0.20 | 0.10 | 2.00 | 0 | 0 | 0.7 | 1 | 3.4 | 0.5 |
| Mech_Apocriton | 0.75 | 0.40 | 2.00 | 0 | 0 | 1.0 | 5.2 | 3.2 | 0.5 |
| Mech_Centurion | 0.75 | 0.25 | 2.00 | 0 | 0 | 3.6 | 3 | 1.6 | 0.75 |
| Mech_Cleansweeper | 0.20 | 0.10 | 2.00 | 0 | 0 | 0.3 | 1 | 3.4 | 0.5 |
| Mech_Constructoid | 0.20 | 0.10 | 2.00 | 0 | 0 | 0.7 | 1 | 3.4 | 0.5 |
| Mech_Diabolus | 0.75 | 0.25 | 2.00 | 0 | 0 | 4 | 4.5 | 2.4 | 0.75 |
| Mech_Fabricor | 0.20 | 0.10 | 2.00 | 0 | 0 | 0.7 | 1 | 3.4 | 0.5 |
| Mech_Legionary | 0.40 | 0.20 | 2.00 | 0 | 0 | 1.0 | 0.72 | 4.3 | 0.5 |
| Mech_Lifter | 0.20 | 0.10 | 2.00 | 0 | 0 | 0.7 | 1 | 2.8 | 0.5 |
| Mech_Militor | 0.20 | 0.10 | 2.00 | 0 | 0 | 0.7 | 1 | 3.8 | 0.5 |
| Mech_Paramedic | 0.20 | 0.10 | 2.00 | 0 | 0 | 0.7 | 1 | 3.8 | 0.5 |
| Mech_Scorcher | 0.40 | 0.20 | 2.00 | 0 | 0 | 1.0 | 0.7 | 4.5 | 0.5 |
| Mech_Tesseron | 0.40 | 0.20 | 2.00 | 0 | 0 | 1.0 | 0.72 | 4.7 | 0.5 |
| Mech_Tunneler | 0.80 | 0.40 | 2.00 | 0 | 0 | 3.5 | 1.5 | 1.9 | 0.5 |
| Mech_WarUrchin | 0.20 | 0.10 | 2.00 | 0 | 0 | 0.7 | 1.3 | 4.2 | 0.5 |
| Mech_Warqueen | 0.75 | 0.25 | 2.00 | 0 | 0 | 4 | 5.2 | 1.6 | 0.75 |
| Toxalope | 0 | 0 | 0 | 0.7 | 0 | 1.4 | 0.65 | 3.4 | 1 |
| WasteRat | 0 | 0 | 0 | 0.7 | 0 | 0.3 | 0.29 | 4.0 | 1 |

## Anomaly

| defName | Sharp | Blunt | Heat | Flamm | Dodge | BodySize | HpScale | MoveSpd | PsySens |
|---------|-------|-------|------|-------|-------|----------|---------|---------|---------|
| Bulbfreak | 0 | 0 | 0 | 1.25 | 0 | 3.5 | 0.3 | 3.8 | 1 |
| Chimera | 0 | 0 | 0 | 0.7 | 0 | 2.15 | 3 | 3.6 | 1 |
| CreepJoiner | 0 | 0 | 0 | 0.7 | 0 | 1 | 1 | 4.6 | 1 |
| Devourer | 0.5 | 0.3 | 0.2 | 0.7 | 0 | 4 | 7.2 | 3.5 | 2 |
| Dreadmeld | 0 | 0 | 0 | 1.25 | 0 | 5 | 10 | 1.5 | 0.5 |
| Fingerspike | 0 | 0 | 0 | 1.25 | 0 | 0.6 | 0.5 | 5.1 | 1 |
| FleshmassNucleus | 1.00 | 1.00 | 2.00 | 0 | 0 | 1 | 1 | 0 | 1 |
| Gorehulk | 0 | 0 | 0 | 0.7 | 0 | 2 | 1.25 | 3.25 | 2 |
| Metalhorror | 0.5 | 0.5 | 0 | 2 | 0 | 1 | 0.6 | 5.5 | 1.5 |
| Nociosphere | 1.00 | 1.00 | 2.00 | 0 | 0 | 3.6 | 10 | 0 | 1 |
| Noctol | 0.28 | 0.18 | 0 | 0.7 | 0 | 1 | 1.5 | 8 | 1 |
| Revenant | 0 | 0 | 0 | 0 | 0 | 1 | 10 | 3.2 | 2 |
| Sightstealer | 0 | 0 | 0 | 0.7 | 0 | 0.8 | 0.75 | 4.83 | 2 |
| Toughspike | 0.2 | 0.16 | 0 | 1.25 | 0 | 1 | 1 | 4.3 | 1 |
| Trispike | 0 | 0 | 0 | 1.25 | 0 | 1 | 0.3 | 4.3 | 1 |

## Odyssey

| defName | Sharp | Blunt | Heat | Flamm | Dodge | BodySize | HpScale | MoveSpd | PsySens |
|---------|-------|-------|------|-------|-------|----------|---------|---------|---------|
| Alligator | 0 | 0 | 0 | 0.7 | 0 | 1.5 | 1.2 | 2.8 | 1 |
| AlphaThrumbo | 0.80 | 0.60 | 0.30 | 0.7 | 0 | 5 | 12.0 | 6.0 | 1 |
| Armadillo | 0.35 | 0.15 | 0 | 0.7 | 0 | 0.4 | 0.5 | 4.0 | 1 |
| Badger | 0 | 0 | 0 | 0.7 | 0 | 0.6 | 0.7 | 3.8 | 1 |
| Bluebird | 0 | 0 | 0 | 0.7 | 0 | 0.15 | 0.1 | 3.1 | 1 |
| BogHound | 0 | 0 | 0 | 0.7 | 0 | 0.85 | 1.0 | 5.0 | 1 |
| Bullfrog | 0 | 0 | 0 | 0.7 | 0 | 0.2 | 0.29 | 2.8 | 1 |
| Cat_Scimitar | 0 | 0 | 0 | 0.7 | 0 | 1.3 | 1.5 | 6.0 | 1 |
| ColossusToad | 0 | 0 | 0 | 0.7 | 0 | 0.8 | 0.8 | 3.2 | 1 |
| Crow | 0 | 0 | 0 | 0.7 | 0 | 0.15 | 0.1 | 3.1 | 1 |
| Drone_Hunter | 0.25 | 0.05 | 1.50 | 0 | 0 | 0.7 | 0.35 | 3.4 | 0 |
| Drone_Sentry | 0.45 | 0.15 | 1.50 | 0 | 0 | 1 | 1.3 | 2.9 | 0 |
| Drone_Wasp | 0.25 | 0.05 | 1.50 | 0 | 0 | 0.25 | 0.35 | 5.8 | 0 |
| Flamingo | 0 | 0 | 0 | 0.7 | 0 | 0.6 | 0.6 | 3.4 | 1 |
| Gorilla | 0 | 0 | 0 | 0.7 | 0 | 2 | 2 | 4.7 | 1 |
| HermitCrab | 0.6 | 0.2 | 0 | 0.7 | 0 | 0.2 | 0.4 | 2 | 1 |
| Heron | 0 | 0 | 0 | 0.7 | 0 | 0.6 | 0.6 | 3.4 | 1 |
| Hippo | 0 | 0 | 0 | 0.7 | 0 | 2.8 | 3.4 | 4.2 | 1 |
| HiveQueen | 0.22 | 0.27 | 0 | 0.7 | 0 | 4.5 | 9.8 | 3.4 | 1 |
| Larva | 0 | 0 | 0 | 0.7 | 0 | 0.2 | 0.25 | 2.0 | 1 |
| LavaSnail | 0.55 | 0.25 | 0.65 | 0.7 | 0 | 0.9 | 0.8 | 0.85 | 1 |
| Locust | 0 | 0 | 0 | 0.7 | 0 | 0.6 | 0.7 | 3.0 | 1 |
| Macaw | 0 | 0 | 0 | 0.7 | 0 | 0.35 | 0.2 | 3.1 | 1 |
| Mastodon | 0 | 0 | 0 | 0.7 | 0 | 4.5 | 4 | 4.7 | 1 |
| Mech_Cyclops | 0.40 | 0.20 | 2.00 | 0 | 0 | 1.0 | 0.9 | 4.5 | 0.5 |
| Megavole | 0 | 0 | 0 | 0.7 | 0 | 0.8 | 1 | 3.2 | 1 |
| Mink | 0 | 0 | 0 | 0.7 | 0 | 0.35 | 0.35 | 4.6 | 1 |
| MonitorLizard | 0 | 0 | 0 | 0.7 | 0 | 0.6 | 1 | 3.6 | 1 |
| Moose | 0 | 0 | 0 | 0.7 | 0 | 2.5 | 2.1 | 4.7 | 1 |
| Muskox | 0 | 0 | 0 | 0.7 | 0 | 2.5 | 1.8 | 4.3 | 1 |
| Otter | 0 | 0 | 0 | 0.7 | 0 | 0.4 | 0.5 | 3.5 | 1 |
| Panda | 0 | 0 | 0 | 0.7 | 0 | 1.5 | 1.2 | 3.2 | 1 |
| Peacock | 0 | 0 | 0 | 0.7 | 0 | 0.6 | 0.6 | 3 | 1 |
| Penguin | 0 | 0 | 0 | 0.7 | 0 | 0.6 | 0.6 | 2 | 1 |
| Porcupine | 0 | 0 | 0 | 0.7 | 0 | 0.35 | 0.4 | 3.0 | 1 |
| PrairieDog | 0 | 0 | 0 | 0.7 | 0 | 0.2 | 0.35 | 3.8 | 1 |
| Quail | 0 | 0 | 0 | 0.7 | 0 | 0.25 | 0.1 | 3.1 | 1 |
| SeaLion | 0 | 0 | 0 | 0.7 | 0 | 1.2 | 0.8 | 2.8 | 1 |
| SeaTurtle | 0.60 | 0.25 | 0 | 0.7 | 0 | 0.5 | 0.6 | 1.5 | 1 |
| Seal | 0 | 0 | 0 | 0.7 | 0 | 0.8 | 0.7 | 2.6 | 1 |
| Sparrow | 0 | 0 | 0 | 0.7 | 0 | 0.12 | 0.075 | 3.1 | 1 |
| StoneCrab | 0.55 | 0.25 | 0 | 0.7 | 0 | 0.3 | 0.6 | 2.2 | 1 |
| Swan | 0 | 0 | 0 | 0.7 | 0 | 0.6 | 0.6 | 3.4 | 1 |
| Tiger | 0 | 0 | 0 | 0.7 | 0 | 1.4 | 1.5 | 5.2 | 1 |
| Vulture | 0 | 0 | 0 | 0.7 | 0 | 0.6 | 0.6 | 2.8 | 1 |
| Walrus | 0 | 0 | 0 | 0.7 | 0 | 2.0 | 1.2 | 2.3 | 1 |
| Wolf_Great | 0 | 0 | 0 | 0.7 | 0 | 1.5 | 1.5 | 5.5 | 1 |
| Wolverine | 0 | 0 | 0 | 0.7 | 0 | 0.7 | 0.6 | 4.6 | 1 |

## StatDef 기본값 참고

| StatDef | defaultBaseValue | 비고 |
|---------|-----------------|------|
| ArmorRating_Sharp | 0 | 장갑 미지정 생명체는 0 |
| ArmorRating_Blunt | 0 | 장갑 미지정 생명체는 0 |
| ArmorRating_Heat | 0 | BaseMechanoid에서 2.0 상속 |
| Flammability | 0 (StatDef) / 0.7 (BasePawn) | BasePawn 상속. 메카노이드=0, Fleshbeast=1.25 |
| MeleeDodgeChance | 0 | Melee스킬x1 + Moving x18 + Sight x8 → postProcessCurve |
| PsychicSensitivity | 1.0 | StatDef 기본값 |

## MeleeDodgeChance PostProcessCurve

| 입력값 | 최종 회피율 |
|--------|-----------|
| 5 | 0% |
| 20 | 30% |
| 60 | 50% |
