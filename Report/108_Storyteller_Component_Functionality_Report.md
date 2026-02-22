# 스토리텔러 컴포넌트 기능 분석 보고서

<!-- StorytellerComp OnOffCycle RandomMain ThreatsGenerator CategoryMTB FactionInteraction Disease Triggered 기능 분석 -->

## 개요

본 보고서는 RimWorld 스토리텔러의 각 `StorytellerComp` 컴포넌트가 어떤 기능을 수행하는지 상세 분석합니다.  
스토리텔러는 `MakeIntervalIncidents()`를 주기적으로 호출하여 `FiringIncident` 목록을 생성하고, 게임이 이를 평가하여 실제 이벤트를 발생시킵니다.

---

## 1. StorytellerComp 기본 동작

### 1.1 공통 메커니즘

- **Storyteller**: 게임 내 이벤트 발생을 제어하는 시스템
- **StorytellerComp**: 스토리텔러의 하위 컴포넌트로, 특정 유형의 이벤트 발생 로직 담당
- **MakeIntervalIncidents(IIncidentTarget target)**: 각 컴포넌트가 이 메서드를 오버라이드하여 `FiringIncident` 시퀀스를 반환
- **GenerateParms(category, target)**: 이벤트 파라미터(위협 포인트, 팩션 등) 생성

### 1.2 MTB (Mean Time Between)

- **Rand.MTBEventOccurs(mtbDays, daysPerTick, checkDuration)**: 확률적 이벤트 발생 판정
- `mtbDays`가 작을수록 이벤트 발생 빈도 증가
- 예: `mtbDays=6` → 평균 6일마다 1회 발생 가능성

### 1.3 targetTags / allowedTargetTags

- **Map_PlayerHome**: 플레이어 정착지 맵
- **World**: 월드맵 (퀘스트, 캐러밴 등)
- **Caravan**: 플레이어 캐러밴
- **Map_TempIncident**: 임시 맵 (전투, 던전 등)
- **Map_RaidBeacon**: 레이드 비콘 설치 맵

---

## 2. 컴포넌트별 기능 상세

### 2.1 StorytellerCompProperties_OnOffCycle

#### 기능
On/Off 주기로 이벤트를 발생시키는 **예측 가능한** 시스템. 카산드라·피비 스토리텔러의 핵심.

#### 동작 방식
- `onDays` 동안 "이벤트 가능" 상태, `offDays` 동안 "이벤트 불가" 상태를 반복
- `minDaysPassed` 이후부터 동작 시작
- `minSpacingDays`: 동일 카테고리 이벤트 간 최소 간격
- `category`에 해당하는 IncidentDef 풀에서 이벤트 선택 (ThreatBig, ThreatSmall 등)

#### 주요 파라미터
| 파라미터 | 설명 |
|----------|------|
| category | IncidentCategoryDef (ThreatBig, ThreatSmall 등) |
| minDaysPassed | 게임 시작 후 이 일수까지는 비활성 |
| onDays | 이벤트가 발생할 수 있는 기간(일) |
| offDays | 이벤트가 발생하지 않는 기간(일) |
| minSpacingDays | 동일 카테고리 이벤트 간 최소 간격(일) |
| numIncidentsRange | 한 번에 발생 가능한 이벤트 수 (예: 1~2) |
| forceRaidEnemyBeforeDaysPassed | 이 일수까지 적 습격 강제 1회 |
| disallowedTargetTags | 제외할 타겟 태그 (예: Map_RaidBeacon) |
| acceptPercentFactorPerThreatPointsCurve | 위협 포인트에 따른 수락 확률 곡선 |

#### 사용 예
- **ThreatBig**: 적 습격, 메카노이드 클러스터 등 큰 위협
- **ThreatSmall**: 작은 동물 무리, 드러퍼 등 작은 위협

---

### 2.2 StorytellerCompProperties_RandomMain

#### 기능
**랜덤 기반** 메인 이벤트 시스템. 랜디 스토리텔러 전용. OnOffCycle 대신 사용.

#### 동작 방식
- `mtbDays`마다 이벤트 발생 판정 (매우 짧음, 예: 1.35일)
- `randomPointsFactorRange`로 위협 포인트 변동 (예: 0.5~1.5)
- `maxThreatBigIntervalDays`: ThreatBig 이벤트 최대 간격 제한
- 카테고리별 **가중치**로 이벤트 종류 결정:
  - Misc, ThreatBig, ThreatSmall, FactionArrival, OrbitalVisitor, ShipChunkDrop 등

#### 주요 파라미터
| 파라미터 | 설명 |
|----------|------|
| mtbDays | 평균 이벤트 발생 간격(일) |
| randomPointsFactorRange | 위협 포인트 배율 범위 |
| maxThreatBigIntervalDays | ThreatBig 최대 간격(일) |
| (가중치) | Misc, ThreatBig, ThreatSmall 등 카테고리별 선택 확률 |

#### 특이사항
- TraderCaravan, VisitorGroup 등 개별 FactionInteraction 컴포넌트 없음
- FactionArrival 가중치로 상인·방문자·여행자 등이 랜덤하게 발생

---

### 2.3 StorytellerCompProperties_ThreatsGenerator

#### 기능
**RaidBeacon**이 설치된 맵에만 적용되는 습격 전용 컴포넌트.

#### 동작 방식
- `allowedTargetTags`에 `Map_RaidBeacon`만 지정
- 플레이어가 레이드 비콘을 사용해 적을 유인할 때, 이 컴포넌트가 해당 맵에 습격을 생성
- OnOffCycle 또는 자체 parms로 빈도 제어

#### 주요 파라미터
| 파라미터 | 설명 |
|----------|------|
| allowedTargetTags | Map_RaidBeacon (필수) |
| parms | allowedThreats(Raids), onDays, offDays, minSpacingDays, numIncidentsRange, minThreatPoints 등 |

#### 사용 맥락
- 레이드 비콘은 "적을 불러오는" 아이템
- 이 컴포넌트가 없으면 비콘 사용 시 습격이 발생하지 않음

---

### 2.4 StorytellerCompProperties_CategoryMTB

#### 기능
**MTB(Mean Time Between)** 방식으로 특정 **카테고리**의 이벤트를 발생시킴.

#### 동작 방식
- `mtbDays`마다 `Rand.MTBEventOccurs()`로 발생 판정
- `category`에 해당하는 IncidentDef 중 하나를 랜덤 선택
- `allowedTargetTags`로 적용 대상 제한 (Map_PlayerHome, World 등)

#### 주요 파라미터
| 파라미터 | 설명 |
|----------|------|
| category | Misc, ThreatBig, ThreatSmall 등 |
| mtbDays | 평균 발생 간격(일) |
| minDaysPassed | 게임 시작 후 이 일수까지 비활성 |
| allowedTargetTags | Map_PlayerHome, World 등 |

#### 사용 예
- **Home Misc**: 정착지의 기타 이벤트 (동물 무리, 난파선, 유물 등)
- **World Misc**: 월드맵 이벤트 (캐러밴 공격, 퀘스트 등)

---

### 2.5 StorytellerCompProperties_CategoryIndividualMTBByBiome

#### 기능
**캐러밴** 및 **임시 맵**에서 발생하는 이벤트를 **생물군계별**로 MTB 방식 제어.

#### 동작 방식
- `allowedTargetTags`: Caravan, Map_TempIncident
- 플레이어가 캐러밴을 이끌 때, 또는 전투/던전 등 임시 맵에 있을 때 적용
- 생물군계(Biome)마다 다른 MTB 곡선 적용 가능
- `applyCaravanVisibility`: 캐러밴이 보이는지에 따라 확률 조정

#### 주요 파라미터
| 파라미터 | 설명 |
|----------|------|
| category | Misc, ThreatSmall, ThreatBig |
| allowedTargetTags | Caravan, Map_TempIncident |
| applyCaravanVisibility | 캐러밴 가시성 반영 여부 |

#### 사용 예
- 캐러밴 이동 중 습격, 동물 무리, 난파선 등
- 던전/전투 맵에서의 추가 이벤트

---

### 2.6 StorytellerCompProperties_Disease

#### 기능
**인간** 및 **동물** 질병 발생을 제어.

#### 동작 방식
- `category`: DiseaseHuman, DiseaseAnimal
- `minDaysPassed` 이후부터 질병 발생 가능
- 게임 내 질병 IncidentDef 풀에서 선택 (플루, 말라리아, 동물 전염병 등)

#### 주요 파라미터
| 파라미터 | 설명 |
|----------|------|
| category | DiseaseHuman, DiseaseAnimal |
| minDaysPassed | 게임 시작 후 이 일수까지 비활성 |

---

### 2.7 StorytellerCompProperties_FactionInteraction

#### 기능
**세력과의 상호작용** 이벤트를 연간 빈도 기반으로 발생시킴.

#### 동작 방식
- `incident`: 특정 IncidentDef 지정 (TraderCaravanArrival, VisitorGroup, TravelerGroup, RaidFriendly 등)
- `baseIncidentsPerYear`: 1년당 목표 발생 횟수
- `minSpacingDays`: 동일 이벤트 간 최소 간격
- `minDanger`: (선택) 이 위험도 이상일 때만 발생 (예: RaidFriendly의 minDanger=High)
- `fullAlliesOnly`: (RaidFriendly) 완전 동맹만 도움 제공

#### 주요 파라미터
| 파라미터 | 설명 |
|----------|------|
| incident | IncidentDef (TraderCaravanArrival, VisitorGroup 등) |
| baseIncidentsPerYear | 연간 목표 발생 횟수 |
| minSpacingDays | 최소 간격(일) |
| minDaysPassed | 게임 시작 후 이 일수까지 비활성 |
| minDanger | 최소 위험도 (None, Low, Medium, High, Extreme) |
| fullAlliesOnly | 완전 동맹만 (RaidFriendly) |
| allowedTargetTags | Map_PlayerHome 등 |

#### 사용 예
- **TraderCaravanArrival**: 상인 캐러밴 도착
- **VisitorGroup**: 방문자 그룹 도착
- **TravelerGroup**: 여행자 그룹 도착
- **RaidFriendly**: 우호 세력이 위기 시 도움 제공

---

### 2.8 StorytellerCompProperties_RandomQuest

#### 기능
**퀘스트** 제공. 로열티 DLC 유무에 따라 다른 설정 적용 가능.

#### 동작 방식
- `category`: GiveQuest
- `onDays`, `numIncidentsRange`로 퀘스트 발생 빈도 제어
- `acceptFractionByDaysPassedCurve`: 게임 진행 일수에 따른 수락 확률 곡선
- `disableIfAnyModActive` / `enableIfAnyModActive`: DLC 조건부 활성화

#### 주요 파라미터
| 파라미터 | 설명 |
|----------|------|
| category | GiveQuest |
| onDays | 퀘스트 발생 주기(일) |
| numIncidentsRange | 한 번에 제공할 퀘스트 수 |
| minSpacingDays | 퀘스트 간 최소 간격 |
| acceptFractionByDaysPassedCurve | (8일, 0%) → (15일, 100%) 등 |
| disableIfAnyModActive | 이 모드 활성 시 비활성 (예: Royalty 없음) |
| enableIfAnyModActive | 이 모드 활성 시만 활성 (예: Royalty 있음) |

---

### 2.9 StorytellerCompProperties_Triggered

#### 기능
**특정 조건**이 충족되면 `delayTicks` 후 지정된 이벤트를 1회 트리거.

#### 동작 방식
- `incident`: 트리거할 IncidentDef
- `delayTicks`: 조건 충족 후 대기 틱
- 일반적으로 게임 시작 직후 또는 특정 이벤트 직후 1회성으로 사용

#### 주요 파라미터
| 파라미터 | 설명 |
|----------|------|
| incident | IncidentDef (StrangerInBlackJoin, RK_Incident_VagabondJoin 등) |
| delayTicks | 트리거 전 대기 틱 |

#### 사용 예
- **StrangerInBlackJoin**: 게임 시작 후 일정 시간 뒤 검은 옷의 낯선인 합류
- **RK_Incident_VagabondJoin**: 랫킨 방랑자 2명 합류 (delayTicks=300)

---

### 2.10 StorytellerCompProperties_ClassicIntro

#### 기능
**게임 시작 인트로** 이벤트. 카산드라·피비 전용.

#### 동작 방식
- 새 게임 시작 시 1회만 실행
- 튜토리얼성 이벤트 또는 초기 상황 설정

---

### 2.11 ShipChunkDrop / OrbitalTraderArrival

#### 기능
- **ShipChunkDrop**: 우주선 파편 낙하 (OnOffCycle 기반)
- **OrbitalTraderArrival**: 궤도 상인 도착 (OnOffCycle: onDays=7, offDays=8)

#### 동작 방식
- OnOffCycle과 유사한 주기적 발생
- 랜디에는 개별 컴포넌트 없음 (RandomMain 가중치로 대체)

---

### 2.12 StorytellerCompProperties_RK_PriestJoin (랫킨 전용)

#### 기능
**랫킨 사제 합류** 이벤트. 게임당 **1회만** 발생.

#### 동작 방식
1. `fireAfterDaysPassed`(60일) 이후부터 트리거 가능
2. `Rand.MTBEventOccurs(mtbDays, 60000f, 1000f)`로 발생 판정
3. `target.StoryState.lastFireTicks`로 이미 발생했으면 재발생 방지
4. `IncidentWorker_GiveQuest` + `RK_QuestScript_PriestJoin`으로 퀘스트 형태 실행

#### 주요 파라미터
| 파라미터 | 설명 |
|----------|------|
| incident | RK_Incident_PriestJoin |
| fireAfterDaysPassed | 60일 이후부터 가능 |
| mtbDays | 조건 충족 후 평균 3일마다 판정 |

#### 소스 참조
`Project/1.6/Source/Priest/StorytellerComp_RK_PriestJoin.cs`

---

## 3. 컴포넌트 호출 흐름 요약

```
[게임 틱마다 / 일정 간격]
    ↓
[Storyteller.StorytellerTick() 또는 유사 메서드]
    ↓
[각 StorytellerComp.MakeIntervalIncidents(target) 호출]
    ↓
[FiringIncident 목록 수집]
    ↓
[게임이 각 FiringIncident 평가]
    ├─ TargetAllowed?
    ├─ CanFireNow(parms)?
    └─ 수락 확률 등
    ↓
[선택된 IncidentWorker.TryExecute(parms) 실행]
```

---

## 4. IncidentCategoryDef 참조

| category | 대표 이벤트 |
|----------|-------------|
| ThreatBig | RaidEnemy, MechCluster, Infestation |
| ThreatSmall | ManhunterPack, DroneShipPart |
| Misc | WandererJoin, AnimalOrbiting, ShipChunkDrop |
| DiseaseHuman | Flu, Plague, Malaria |
| DiseaseAnimal | Animal disease |
| GiveQuest | 퀘스트 제공 |

---

**분석 일자**: 2025-02-22  
**참조**: ReportArchive/107_Storyteller_Component_Comparison_Report.md  
**참조**: Project/1.6/Source/Priest/StorytellerComp_RK_PriestJoin.cs
