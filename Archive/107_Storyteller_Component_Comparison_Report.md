# 스토리텔러 컴포넌트 구성 비교 보고서

<!-- StorytellerDef Cassandra Phoebe Randy RottyMilden Ratkin 컴포넌트 비교 분석 -->

## 개요

본 보고서는 림월드 기본 스토리텔러(카산드라, 피비, 랜디)의 컴포넌트 구성을 분석하고, 랫킨 전용 스토리텔러(Rotty Milden)와의 비교를 위한 기초 자료를 제공합니다.

---

# Part 1: 기본 스토리텔러 컴포넌트 구성 분석

## 1.1 상속 구조

모든 기본 스토리텔러는 `BaseStoryteller`를 상속받습니다.

```
BaseStoryteller (공통)
├── Cassandra Classic
├── Phoebe Chillax
└── Randy Random
```

## 1.2 컴포넌트 타입 개요

RimWorld 스토리텔러는 `<comps>` 내부에 여러 `StorytellerCompProperties_*` 클래스로 구성됩니다.

| 컴포넌트 클래스 | 용도 | 사용 스토리텔러 |
|----------------|------|-----------------|
| **StorytellerCompProperties_OnOffCycle** | On/Off 주기로 이벤트 발생 (예측 가능) | 카산드라, 피비 |
| **StorytellerCompProperties_RandomMain** | 랜덤 기반 메인 이벤트 시스템 | 랜디 |
| **StorytellerCompProperties_ThreatsGenerator** | RaidBeacon 대상 습격 전용 | 모두 |
| **StorytellerCompProperties_CategoryMTB** | MTB(Mean Time Between) 기반 카테고리 이벤트 | 카산드라, 피비, 랜디 |
| **StorytellerCompProperties_Disease** | 인간/동물 질병 | 모두 |
| **StorytellerCompProperties_FactionInteraction** | 세력 상호작용 (상인, 방문자, 습격 등) | 카산드라, 피비 |
| **StorytellerCompProperties_CategoryIndividualMTBByBiome** | 캐러밴/임시 맵 이벤트 (생물군계별) | 모두 |
| **StorytellerCompProperties_RandomQuest** | 퀘스트 제공 | 모두 |
| **StorytellerCompProperties_Triggered** | 특정 조건 시 트리거 이벤트 | 모두 |
| **StorytellerCompProperties_ClassicIntro** | 시작 인트로 이벤트 | 카산드라, 피비 |
| **StorytellerCompProperties_RK_PriestJoin** | 랫킨 사제 합류 (커스텀) | 랫킨 전용 |

## 1.3 카산드라 클래식 (Cassandra Classic) 컴포넌트 구성

### 시스템: OnOffCycle 기반

| # | 컴포넌트 | category | 주요 파라미터 |
|---|----------|----------|---------------|
| 1 | OnOffCycle | ThreatBig | minDaysPassed=11, onDays=4.6, offDays=6.0 |
| 2 | OnOffCycle | ThreatSmall | minDaysPassed=11, onDays=4.6, offDays=6.0 |
| 3 | CategoryMTB | Misc | minDaysPassed=5, mtbDays=4.8 |
| 4 | Disease | DiseaseHuman | minDaysPassed=9 |
| 5 | Disease | DiseaseAnimal | minDaysPassed=9 |
| 6 | ThreatsGenerator | - | RaidBeacon 대상 |
| 7 | FactionInteraction | RaidFriendly | baseIncidentsPerYear=15 |
| 8 | FactionInteraction | TraderCaravanArrival | baseIncidentsPerYear=5, minSpacingDays=6 |
| 9 | FactionInteraction | VisitorGroup | baseIncidentsPerYear=4, minSpacingDays=5 |
| 10 | FactionInteraction | TravelerGroup | baseIncidentsPerYear=6, minSpacingDays=1 |
| 11 | ClassicIntro | - | 시작 인트로 |
| 12 | ShipChunkDrop | - | 우주선 파편 (OnOffCycle) |
| 13 | OrbitalTraderArrival | - | onDays=7, offDays=8 |
| 14 | CategoryIndividualMTBByBiome | Misc | Caravan, Map_TempIncident |
| 15 | CategoryIndividualMTBByBiome | ThreatSmall | Caravan, Map_TempIncident |
| 16 | CategoryIndividualMTBByBiome | ThreatBig | Caravan, Map_TempIncident |
| 17 | RandomQuest | GiveQuest | minSpacingDays=3 (Non-Royalty) |
| 18 | RandomQuest | GiveQuest | minSpacingDays=3 (Royalty) |
| 19 | CategoryMTB | Misc (World) | minDaysPassed=15, mtbDays=15 |
| 20 | Triggered | StrangerInBlackJoin | delayTicks |

### 카산드라 핵심 수치

| 항목 | 값 |
|------|-----|
| ThreatBig 시작 |
| minDaysPassed | 11 |
| onDays | 4.6 |
| offDays | 6.0 |
| ThreatSmall | 동일 |
| Misc (Home) | mtbDays=4.8, minDaysPassed=5 |
| 질병 | Human/Animal 9일 후 |
| RaidFriendly | 15회/년 |
| TraderCaravan | 5회/년, minSpacingDays=6 |
| 퀘스트 | minSpacingDays=3 |

## 1.4 피비 칼락스 (Phoebe Chillax) 컴포넌트 구성

### 시스템: OnOffCycle 기반 (카산드라와 동일, 수치만 완화)

| # | 컴포넌트 | category | 주요 파라미터 |
|---|----------|----------|---------------|
| 1 | OnOffCycle | ThreatBig | minDaysPassed=13, onDays=8.0, offDays=8.0 |
| 2 | OnOffCycle | ThreatSmall | minDaysPassed=13, onDays=8.0, offDays=8.0 |
| 3 | CategoryMTB | Misc | minDaysPassed=5, mtbDays=4.8 |
| 4 | Disease | DiseaseHuman | minDaysPassed=12 |
| 5 | Disease | DiseaseAnimal | minDaysPassed=12 |
| 6 | ThreatsGenerator | - | RaidBeacon 대상 |
| 7 | FactionInteraction | RaidFriendly | baseIncidentsPerYear=15 |
| 8 | FactionInteraction | TraderCaravanArrival | baseIncidentsPerYear=5, minSpacingDays=6 |
| 9 | FactionInteraction | VisitorGroup | baseIncidentsPerYear=4, minSpacingDays=5 |
| 10 | FactionInteraction | TravelerGroup | baseIncidentsPerYear=6, minSpacingDays=1 |
| 11 | ClassicIntro | - | 시작 인트로 |
| 12 | ShipChunkDrop | - | 우주선 파편 |
| 13 | OrbitalTraderArrival | - | onDays=7, offDays=8 |
| 14~16 | CategoryIndividualMTBByBiome | Misc, ThreatSmall, ThreatBig | Caravan, Map_TempIncident |
| 17~18 | RandomQuest | GiveQuest | minSpacingDays=3 |
| 19 | CategoryMTB | Misc (World) | minDaysPassed=15, mtbDays=15 |
| 20 | Triggered | StrangerInBlackJoin | delayTicks |

### 피비 핵심 수치

| 항목 | 값 |
|------|-----|
| ThreatBig 시작 | minDaysPassed=13 |
| onDays | 8.0 |
| offDays | 8.0 |
| ThreatSmall | 동일 |
| Misc (Home) | mtbDays=4.8, minDaysPassed=5 |
| 질병 | Human/Animal 12일 후 |
| RaidFriendly | 15회/년 |
| TraderCaravan | 5회/년, minSpacingDays=6 |
| 퀘스트 | minSpacingDays=3 |

## 1.5 랜디 랜덤 (Randy Random) 컴포넌트 구성

### 시스템: RandomMain 기반 (완전 랜덤)

| # | 컴포넌트 | 용도 |
|---|----------|------|
| 1 | **RandomMain** | 메인 이벤트 시스템 (mtbDays=1.35, maxThreatBigIntervalDays=13) |
| 2 | Disease | DiseaseHuman (minDaysPassed=0) |
| 3 | Disease | DiseaseAnimal (minDaysPassed=0) |
| 4 | ThreatsGenerator | RaidBeacon 대상 |
| 5 | FactionInteraction | RaidFriendly (baseIncidentsPerYear=10) |
| 6~8 | CategoryIndividualMTBByBiome | Caravan, Map_TempIncident |
| 9~10 | RandomQuest | GiveQuest (minSpacingDays=0.2) |
| 11 | CategoryMTB | Misc (World) (minDaysPassed=1, mtbDays=15) |
| 12 | Triggered | StrangerInBlackJoin |

### 랜디 특이사항

- **TraderCaravanArrival, VisitorGroup, TravelerGroup, OrbitalTraderArrival**: 개별 컴포넌트 없음 → RandomMain의 FactionArrival 가중치(2.4)로 대체
- **ClassicIntro, ShipChunkDrop**: 없음
- **질병**: 즉시(0일) 가능
- **퀘스트**: minSpacingDays=0.2 (매우 짧음)

### 랜디 RandomMain 카테고리 가중치

| 카테고리 | 가중치 |
|----------|--------|
| Misc | 3.5 |
| ThreatBig | 1.4 |
| ThreatSmall | 0.6 |
| FactionArrival | 2.4 |
| OrbitalVisitor | 1.1 |
| ShipChunkDrop | 0.22 |

## 1.6 기본 스토리텔러 컴포넌트 구성 요약표

| 컴포넌트 | 카산드라 | 피비 | 랜디 |
|----------|:--------:|:----:|:----:|
| OnOffCycle (ThreatBig) | ✓ | ✓ | - |
| OnOffCycle (ThreatSmall) | ✓ | ✓ | - |
| RandomMain | - | - | ✓ |
| CategoryMTB (Home Misc) | ✓ | ✓ | - |
| Disease (Human/Animal) | ✓ | ✓ | ✓ |
| ThreatsGenerator | ✓ | ✓ | ✓ |
| FactionInteraction (RaidFriendly) | ✓ | ✓ | ✓ |
| FactionInteraction (Trader) | ✓ | ✓ | - |
| FactionInteraction (Visitor) | ✓ | ✓ | - |
| FactionInteraction (Traveler) | ✓ | ✓ | - |
| ClassicIntro | ✓ | ✓ | - |
| ShipChunkDrop | ✓ | ✓ | - |
| OrbitalTraderArrival | ✓ | ✓ | - |
| CategoryIndividualMTBByBiome | ✓ | ✓ | ✓ |
| RandomQuest | ✓ | ✓ | ✓ |
| CategoryMTB (World) | ✓ | ✓ | ✓ |
| Triggered | ✓ | ✓ | ✓ |

---

# Part 2: 랫킨 스토리텔러 (Rotty Milden) 컴포넌트 구성

## 2.1 상속 구조

```
BaseStoryteller
└── RK_Storyteller_RottyMilden (Rotty Milden)
```

## 2.2 컴포넌트 목록

| # | 컴포넌트 | category | 주요 파라미터 |
|---|----------|----------|---------------|
| 1 | OnOffCycle | ThreatBig | minDaysPassed=15, onDays=6, offDays=10, minSpacingDays=2.5 |
| 2 | ThreatsGenerator | RaidBeacon | onDays=2, offDays=0.5, minSpacingDays=0.6 |
| 3 | OnOffCycle | ThreatSmall | minDaysPassed=6, onDays=5, offDays=8 |
| 4 | CategoryMTB | Misc | minDaysPassed=5, mtbDays=6 |
| 5 | Disease | DiseaseHuman | minDaysPassed=6 |
| 6 | Disease | DiseaseAnimal | minDaysPassed=6 |
| 7 | FactionInteraction | RaidFriendly | baseIncidentsPerYear=12, minDanger=High |
| 8 | FactionInteraction | TraderCaravanArrival | baseIncidentsPerYear=6, minSpacingDays=4 |
| 9 | FactionInteraction | VisitorGroup | baseIncidentsPerYear=4, minSpacingDays=5 |
| 10 | FactionInteraction | TravelerGroup | baseIncidentsPerYear=6, minSpacingDays=1 |
| 11~13 | CategoryIndividualMTBByBiome | Misc, ThreatSmall, ThreatBig | Caravan, Map_TempIncident |
| 14 | RandomQuest | GiveQuest | onDays=10, minSpacingDays=1.6 (Non-Royalty) |
| 15 | RandomQuest | GiveQuest | onDays=12, minSpacingDays=1.6 (Royalty) |
| 16 | CategoryMTB | Misc (World) | minDaysPassed=8, mtbDays=15 |
| 17 | Triggered | RK_Incident_VagabondJoin | delayTicks=300 |
| 18 | **RK_PriestJoin** | - | fireAfterDaysPassed=60, mtbDays=3 |

## 2.3 랫킨 전용 컴포넌트

| 컴포넌트 | 설명 |
|----------|------|
| **StorytellerCompProperties_RK_PriestJoin** | 60일 이후 1회만 트리거되는 랫킨 사제 합류 이벤트 |
| **Triggered → RK_Incident_VagabondJoin** | 방랑자 2명 합류 (StrangerInBlack 대체) |

## 2.4 랫킨에 없는 기본 컴포넌트

- **ClassicIntro**: 없음
- **ShipChunkDrop**: 없음
- **OrbitalTraderArrival**: 없음

---

# Part 3: 수치 비교 (기본 vs 랫킨)

## 3.1 ThreatBig / ThreatSmall 비교

| 항목 | 카산드라 | 피비 | 랫킨 (Rotty) |
|------|----------|------|---------------|
| ThreatBig minDaysPassed | 11 | 13 | **15** |
| ThreatBig onDays | 4.6 | 8.0 | **6.0** |
| ThreatBig offDays | 6.0 | 8.0 | **10.0** |
| ThreatBig minSpacingDays | - | - | **2.5** |
| ThreatSmall minDaysPassed | 11 | 13 | **6** |
| ThreatSmall onDays | 4.6 | 8.0 | **5.0** |
| ThreatSmall offDays | 6.0 | 8.0 | **8.0** |

**랫킨 특징**: ThreatBig는 시작이 늦고(15일) offDays가 길어(10일) 위협 빈도가 낮음. ThreatSmall은 6일부터 시작.

## 3.2 Misc / 질병 비교

| 항목 | 카산드라 | 피비 | 랫킨 |
|------|----------|------|------|
| Misc (Home) mtbDays | 4.8 | 4.8 | **6.0** |
| Misc (Home) minDaysPassed | 5 | 5 | 5 |
| Misc (World) mtbDays | 15 | 15 | 15 |
| Misc (World) minDaysPassed | 15 | 15 | **8** |
| Disease Human minDaysPassed | 9 | 12 | **6** |
| Disease Animal minDaysPassed | 9 | 12 | **6** |

**랫킨 특징**: Home Misc는 6일로 더 느림. 질병은 6일부터 (카산드라보다 빠름).

## 3.3 FactionInteraction 비교

| 항목 | 카산드라 | 피비 | 랫킨 |
|------|----------|------|------|
| RaidFriendly baseIncidentsPerYear | 15 | 15 | **12** |
| RaidFriendly minDanger | - | - | **High** |
| TraderCaravan baseIncidentsPerYear | 5 | 5 | **6** |
| TraderCaravan minSpacingDays | 6 | 6 | **4** |
| TraderCaravan minDaysPassed | - | - | **12** |
| VisitorGroup baseIncidentsPerYear | 4 | 4 | 4 |
| VisitorGroup minSpacingDays | 5 | 5 | 5 |
| TravelerGroup baseIncidentsPerYear | 6 | 6 | 6 |
| TravelerGroup minSpacingDays | 1 | 1 | 1 |

**랫킨 특징**: RaidFriendly 12회/년 (낮음), TraderCaravan 6회/년 (높음), minDanger=High로 RaidFriendly는 위험도 높을 때만.

## 3.4 퀘스트 비교

| 항목 | 카산드라 | 피비 | 랫킨 |
|------|----------|------|------|
| Non-Royalty minSpacingDays | 3 | 3 | **1.6** |
| Non-Royalty onDays | - | - | **10** |
| Royalty minSpacingDays | 3 | 3 | **1.6** |
| Royalty onDays | - | - | **12** |
| Royalty numIncidentsRange | 2 | 2 | **1~2** |

**랫킨 특징**: 퀘스트 간격 1.6일로 더 짧음 (퀘스트가 더 자주 발생 가능).

## 3.5 ThreatsGenerator (RaidBeacon) 비교

| 항목 | 기본 (공통) | 랫킨 |
|------|-------------|------|
| onDays | 1.0 | **2.0** |
| offDays | 0.5 | 0.5 |
| minSpacingDays | 0.04 | **0.6** |
| numIncidentsRange | - | **2~3** |
| minThreatPoints | - | **500** |

**랫킨 특징**: RaidBeacon 습격은 onDays=2, minSpacingDays=0.6로 기본보다 완화.

---

# 요약

## 기본 스토리텔러 컴포넌트 구성

- **카산드라/피비**: OnOffCycle 기반, 규칙적 패턴. FactionInteraction 개별 컴포넌트 다수.
- **랜디**: RandomMain 기반, FactionInteraction 개별 컴포넌트 없음 → RandomMain 가중치로 대체.

## 랫킨 전용 스토리텔러 (Rotty Milden)

- **시스템**: OnOffCycle 기반 (카산드라/피비와 유사)
- **특징**: ThreatBig는 늦고 느슨, ThreatSmall은 6일부터. Misc/질병은 중간값. TraderCaravan 빈도 증가. RK_PriestJoin, RK_VagabondJoin 전용 이벤트.
- **없는 컴포넌트**: ClassicIntro, ShipChunkDrop, OrbitalTraderArrival

---

**분석 일자**: 2025-02-22  
**참조**: ReportArchive/63_RimWorld_Core_Storytellers_Comparison_Report.md
**참조 파일**: Project/1.6/Defs/Storytellers/Storytellers.xml
**원본 Defs**: RimworldData/Core/Defs/Storyteller/Storytellers.xml (기본)
