# 레이드 실행부터 종료까지 전체 플로우 분석

**작성일**: 2024-12-30  
**분석 목적**: RaidStrategy 선택 후 실제 레이드 실행, Pawn 생성, 전투, 이탈/종료까지의 전체 흐름 및 관련 컴포넌트 분석

---

## 1. 큰 플로우

```
RaidStrategy 선택 완료
    ↓
IncidentWorker_Raid.TryExecuteWorker()
    ↓
TryGenerateRaidInfo()
    ├─ ResolveRaidPoints() - 포인트 결정
    ├─ TryResolveRaidFaction() - 세력 결정
    ├─ ResolveRaidStrategy() - 전략 결정 (이미 선택됨)
    ├─ ResolveRaidArriveMode() - 도착 방식 결정
    ├─ ResolveRaidAgeRestriction() - 연령 제한 결정
    ├─ TryGenerateThreats() - 특수 위협 생성 (선택적)
    ├─ TryResolveRaidSpawnCenter() - 스폰 위치 결정
    ├─ AdjustedRaidPoints() - 포인트 보정
    └─ SpawnThreats() 또는 PawnGroupMakerUtility.GeneratePawns() - Pawn 생성
    ↓
Pawn 생성 및 맵에 스폰
    ├─ PawnGenerator.GeneratePawn() - 개별 Pawn 생성
    └─ PawnsArrivalModeWorker.Arrive() - 맵에 도착 처리
    ↓
PostProcessSpawnedPawns() - 생성 후 처리
    ↓
Letter 전송 (플레이어에게 알림)
    ↓
RaidStrategyWorker.MakeLords() - Lord 생성
    ├─ MakeLordJob() - LordJob 생성
    ├─ LordMaker.MakeNewLord() - Lord 인스턴스 생성
    └─ Lord.AddPawn() - Pawn들을 Lord에 할당
    ↓
Lord.StateGraph 실행 시작
    ├─ CreateGraph() - 상태 그래프 생성
    ├─ StartingToil 설정
    └─ 첫 번째 LordToil 시작
    ↓
[레이드 진행 단계]
    ├─ LordToil 실행 (각 전략별 동작)
    ├─ Trigger 체크 (조건 확인)
    ├─ Transition 발생 (상태 전환)
    └─ 새로운 LordToil로 이동
    ↓
[종료 조건 충족]
    ├─ Trigger_FractionColonyDamageTaken - 일정 피해 입힘
    ├─ Trigger_TicksPassed - 시간 초과
    ├─ Trigger_BecameNonHostileToPlayer - 비적대화
    ├─ Trigger_FractionPawnsLost - 일정 비율 손실
    └─ Trigger_GameEnding - 게임 종료 조건
    ↓
LordToil_ExitMap - 맵 이탈
    ↓
모든 Pawn이 맵을 벗어남
    ↓
Lord 정리 및 제거
```

---

## 2. 상세 단계별 분석

### 2.1 Pawn 생성 단계

#### 2.1.1 TryGenerateRaidInfo()

**위치**: `IncidentWorker_Raid.TryGenerateRaidInfo()`

**주요 단계**:
1. **ResolveRaidPoints()**: 레이드 포인트 결정
   - `StorytellerUtility.DefaultThreatPointsNow()` 사용
   - 기지 가치 기반 계산

2. **TryResolveRaidFaction()**: 세력 결정
   - `PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup()` 사용
   - 포인트 기반 가중치 선택

3. **ResolveRaidStrategy()**: 전략 결정 (이미 선택됨)
   - `RaidStrategyDef` 선택
   - `CanUseWith()` 체크

4. **ResolveRaidArriveMode()**: 도착 방식 결정
   - 선택된 전략의 `arriveModes` 중 선택
   - `GetSelectionWeight()` 기반 가중치 선택

5. **TryGenerateThreats()**: 특수 위협 생성 (선택적)
   - 일부 전략에서 추가 위협 생성 (예: Siege의 모르타르)

6. **TryResolveRaidSpawnCenter()**: 스폰 위치 결정
   - `PawnsArrivalModeWorker.TryResolveRaidSpawnCenter()` 호출
   - EdgeWalkIn: 맵 가장자리
   - DropPod: 드롭 포드 위치
   - CenterDrop: 맵 중앙

7. **AdjustedRaidPoints()**: 포인트 보정
   - 도착 방식 보정
   - 전략 보정
   - 연령 제한 보정
   - 레이어 보정

8. **SpawnThreats() 또는 GeneratePawns()**: Pawn 생성
   - `RaidStrategyWorker.SpawnThreats()` 우선 시도
   - 실패 시 `PawnGroupMakerUtility.GeneratePawns()` 사용

#### 2.1.2 Pawn 생성 과정

**PawnGroupMakerUtility.GeneratePawns()**:
1. `PawnGroupMaker` 선택
   - Faction의 `pawnGroupMakers` 중 선택
   - `commonality` 기반 가중치
   - `CanGenerateFrom()` 체크

2. `PawnGenOption` 선택
   - 포인트 기반 반복 선택 알고리즘
   - `maxPawnCostPerTotalPointsCurve` 제한
   - `PawnKindDef` 선택

3. `PawnGenerator.GeneratePawn()` 호출
   - `PawnGenerationRequest` 생성
   - 종족, 장비, 특성 등 설정
   - Pawn 인스턴스 생성

4. `PawnsArrivalModeWorker.Arrive()` 호출
   - 맵에 스폰
   - 도착 방식별 처리:
     - EdgeWalkIn: 가장자리에서 걸어서 등장
     - EdgeDrop: 가장자리에 드롭 포드
     - CenterDrop: 중앙에 드롭 포드
     - RandomDrop: 랜덤 위치에 드롭 포드

### 2.2 Lord 생성 단계

#### 2.2.1 MakeLords()

**위치**: `RaidStrategyWorker.MakeLords()`

**주요 단계**:
1. **그룹 분할**: `IncidentParmsUtility.SplitIntoGroups()`
   - 여러 그룹으로 나눌 경우 분할
   - 각 그룹마다 별도 Lord 생성

2. **MakeLordJob()**: LordJob 생성
   - 전략별로 다른 LordJob 생성:
     - `ImmediateAttack` → `LordJob_AssaultColony`
     - `Siege` → `LordJob_Siege`
     - `StageThenAttack` → `LordJob_StageThenAttack`
     - `ImmediateAttackSappers` → `LordJob_AssaultColony` (sappers=true)
     - `ImmediateAttackBreaching` → `LordJob_AssaultColony` (breachers=true)

3. **LordMaker.MakeNewLord()**: Lord 인스턴스 생성
   - Faction, LordJob, Map, Pawns 전달
   - Lord 인스턴스 생성 및 초기화

4. **Lord.AddPawn()**: Pawn 할당
   - 각 Pawn을 Lord에 추가
   - `pawn.lord` 설정

### 2.3 상태 그래프 실행 단계

#### 2.3.1 StateGraph 생성

**위치**: `LordJob.CreateGraph()`

각 LordJob은 `CreateGraph()`를 구현하여 상태 그래프를 생성합니다.

**주요 구성 요소**:
- **LordToil**: 상태 (할 일)
- **Transition**: 상태 전환
- **Trigger**: 전환 조건
- **TransitionAction**: 전환 시 실행할 액션

#### 2.3.2 ImmediateAttack 전략 예시

**LordJob_AssaultColony.CreateGraph()**:

```
[시작]
    ↓
LordToil_AssaultColony (공격 상태)
    ├─ Trigger_TicksPassed (시간 초과) → LordToil_ExitMap
    ├─ Trigger_FractionColonyDamageTaken (피해 입힘) → LordToil_ExitMap
    ├─ Trigger_KidnapVictimPresent (납치 대상 발견) → LordJob_Kidnap
    ├─ Trigger_HighValueThingsAround (고가치 아이템 발견) → LordJob_Steal
    └─ Trigger_BecameNonHostileToPlayer (비적대화) → LordToil_ExitMap
    ↓
LordToil_ExitMap (맵 이탈)
    ↓
[종료]
```

#### 2.3.3 Siege 전략 예시

**LordJob_Siege.CreateGraph()**:

```
[시작]
    ↓
LordToil_Travel (공성 위치로 이동)
    ├─ Trigger_Memo("TravelArrived") → LordToil_Siege
    └─ Trigger_TicksPassed(5000) → LordToil_Siege
    ↓
LordToil_Siege (공성 상태)
    ├─ 모르타르 설치 및 포격
    ├─ Trigger_Memo("NoBuilders") → LordJob_AssaultColony
    ├─ Trigger_Memo("NoArtillery") → LordJob_AssaultColony
    ├─ Trigger_PawnHarmed(0.08f) → LordJob_AssaultColony
    ├─ Trigger_FractionPawnsLost(0.3f) → LordJob_AssaultColony
    ├─ Trigger_TicksPassed(1.5~3일) → LordJob_AssaultColony
    └─ Trigger_BecameNonHostileToPlayer → LordToil_ExitMap
    ↓
LordJob_AssaultColony (공격 상태)
    ↓
LordToil_ExitMap (맵 이탈)
    ↓
[종료]
```

#### 2.3.4 StageThenAttack 전략 예시

**LordJob_StageThenAttack.CreateGraph()**:

```
[시작]
    ↓
LordToil_Stage (준비 상태)
    ├─ Trigger_TicksPassed(5~15초) → LordJob_AssaultColony
    └─ Trigger_FractionPawnsLost(0.3f) → LordJob_AssaultColony
    ↓
LordJob_AssaultColony (공격 상태)
    ↓
LordToil_ExitMap (맵 이탈)
    ↓
[종료]
```

### 2.4 전투 및 이탈 단계

#### 2.4.1 LordToil 실행

**LordToil_AssaultColony**:
- Pawn들이 플레이어 콜로니를 공격
- AI 작업 할당:
  - `JobGiver_AISapper` (공병인 경우)
  - `JobGiver_AIFightEnemy` (전투)
  - `JobGiver_AIAttackEnemy` (공격)

**LordToil_Siege**:
- 공성 위치에서 모르타르 설치 및 포격
- 일부 Pawn은 건설 작업 수행

**LordToil_Stage**:
- 지정된 위치에서 대기
- 공격 준비

#### 2.4.2 Trigger 체크

매 틱마다 각 Transition의 Trigger를 체크합니다:

**주요 Trigger 종류**:
- **Trigger_TicksPassed**: 시간 경과
- **Trigger_FractionColonyDamageTaken**: 콜로니 피해 비율
- **Trigger_FractionPawnsLost**: Pawn 손실 비율
- **Trigger_PawnHarmed**: Pawn 피해
- **Trigger_BecameNonHostileToPlayer**: 비적대화
- **Trigger_KidnapVictimPresent**: 납치 대상 발견
- **Trigger_HighValueThingsAround**: 고가치 아이템 발견
- **Trigger_Memo**: 메모 트리거

#### 2.4.3 Transition 실행

조건 충족 시:
1. **PreAction 실행**: `TransitionAction_Message` 등
2. **PostAction 실행**: `TransitionAction_WakeAll` 등
3. **새로운 LordToil로 전환**

#### 2.4.4 맵 이탈

**LordToil_ExitMap**:
- Pawn들이 맵 가장자리로 이동
- `JobGiver_ExitMap` 작업 할당
- 맵을 벗어나면 Pawn 제거

### 2.5 종료 단계

#### 2.5.1 Lord 정리

**조건**:
- 모든 Pawn이 맵을 벗어남
- 모든 Pawn이 죽음
- `ShouldExist`가 false가 됨

**처리**:
- `LordManager.RemoveLord()` 호출
- Lord 인스턴스 제거
- 관련 리소스 정리

---

## 3. 관련 컴포넌트

### 3.1 핵심 클래스

#### 3.1.1 IncidentWorker 계층

**IncidentWorker_Raid** (추상 클래스):
- `TryExecuteWorker()`: 레이드 실행 메인 메서드
- `TryGenerateRaidInfo()`: 레이드 정보 생성
- `ResolveRaidPoints()`: 포인트 결정
- `ResolveRaidStrategy()`: 전략 결정
- `ResolveRaidArriveMode()`: 도착 방식 결정
- `AdjustedRaidPoints()`: 포인트 보정

**IncidentWorker_RaidEnemy**:
- 적대 레이드 구현
- `GetLetterLabel()`, `GetLetterText()` 구현

**IncidentWorker_RaidFriendly**:
- 우호 레이드 구현

#### 3.1.2 RaidStrategyWorker 계층

**RaidStrategyWorker** (추상 클래스):
- `MakeLords()`: Lord 생성
- `MakeLordJob()`: LordJob 생성 (추상)
- `SpawnThreats()`: 특수 위협 생성
- `TryGenerateThreats()`: 위협 생성 시도
- `CanUseWith()`: 사용 가능 여부 체크
- `SelectionWeight()`: 선택 가중치 계산

**구현 클래스**:
- `RaidStrategyWorker_ImmediateAttack`
- `RaidStrategyWorker_ImmediateAttackSmart`
- `RaidStrategyWorker_ImmediateAttackSappers`
- `RaidStrategyWorker_ImmediateAttackBreaching`
- `RaidStrategyWorker_Siege`
- `RaidStrategyWorker_StageThenAttack`

#### 3.1.3 LordJob 계층

**LordJob** (추상 클래스):
- `CreateGraph()`: 상태 그래프 생성 (추상)
- `LordJobTick()`: 틱마다 호출
- `Map`: 맵 참조
- `lord`: Lord 참조

**레이드 관련 구현**:
- `LordJob_AssaultColony`: 즉시 공격
- `LordJob_Siege`: 공성전
- `LordJob_StageThenAttack`: 준비 후 공격
- `LordJob_Kidnap`: 납치
- `LordJob_Steal`: 도둑질
- `LordJob_AssistColony`: 도움 제공

#### 3.1.4 LordToil 계층

**LordToil** (추상 클래스):
- `UpdateAllDuties()`: 모든 Pawn의 Duty 업데이트
- `CreateDuty()`: Duty 생성

**레이드 관련 구현**:
- `LordToil_AssaultColony`: 공격 상태
- `LordToil_AssaultColonySappers`: 공병 공격 상태
- `LordToil_AssaultColonyBreaching`: 돌파 공격 상태
- `LordToil_Siege`: 공성 상태
- `LordToil_Stage`: 준비 상태
- `LordToil_ExitMap`: 맵 이탈 상태
- `LordToil_Travel`: 이동 상태

#### 3.1.5 Trigger 계층

**Trigger** (추상 클래스):
- `ActivateOn()`: 활성화 시점
- `SourceToil()`: 소스 Toil

**주요 구현**:
- `Trigger_TicksPassed`: 시간 경과
- `Trigger_FractionColonyDamageTaken`: 콜로니 피해 비율
- `Trigger_FractionPawnsLost`: Pawn 손실 비율
- `Trigger_PawnHarmed`: Pawn 피해
- `Trigger_BecameNonHostileToPlayer`: 비적대화
- `Trigger_KidnapVictimPresent`: 납치 대상 발견
- `Trigger_HighValueThingsAround`: 고가치 아이템 발견
- `Trigger_Memo`: 메모 트리거
- `Trigger_NoFightingSappers`: 공병 전투 종료
- `Trigger_GameEnding`: 게임 종료 조건

#### 3.1.6 PawnsArrivalModeWorker 계층

**PawnsArrivalModeWorker** (추상 클래스):
- `Arrive()`: Pawn 도착 처리
- `TryResolveRaidSpawnCenter()`: 스폰 위치 결정
- `CanUseWith()`: 사용 가능 여부 체크
- `GetSelectionWeight()`: 선택 가중치 계산

**주요 구현**:
- `PawnsArrivalModeWorker_EdgeWalkIn`: 가장자리 걸어서 등장
- `PawnsArrivalModeWorker_EdgeDrop`: 가장자리 드롭 포드
- `PawnsArrivalModeWorker_CenterDrop`: 중앙 드롭 포드
- `PawnsArrivalModeWorker_RandomDrop`: 랜덤 드롭 포드
- `PawnsArrivalModeWorker_EmergeFromWater`: 물에서 등장

### 3.2 유틸리티 클래스

#### 3.2.1 PawnGroupMakerUtility

**주요 메서드**:
- `GeneratePawns()`: Pawn 그룹 생성
- `TryGetRandomPawnGroupMaker()`: PawnGroupMaker 선택
- `ChoosePawnGenOptionsByPoints()`: 포인트 기반 Pawn 선택
- `TryGetRandomFactionForCombatPawnGroup()`: 전투용 세력 선택

#### 3.2.2 PawnGenerator

**주요 메서드**:
- `GeneratePawn()`: 개별 Pawn 생성
- `GeneratePawn()`: PawnGenerationRequest 기반 생성

#### 3.2.3 LordMaker

**주요 메서드**:
- `MakeNewLord()`: 새 Lord 생성
- `MakeNewLord()`: Faction, LordJob, Map, Pawns 전달

#### 3.2.4 IncidentParmsUtility

**주요 메서드**:
- `GetDefaultPawnGroupMakerParms()`: 기본 파라미터 생성
- `SplitIntoGroups()`: Pawn 그룹 분할

### 3.3 데이터 구조

#### 3.3.1 IncidentParms

**주요 필드**:
- `target`: 대상 (Map)
- `faction`: 세력
- `points`: 포인트
- `raidStrategy`: 레이드 전략
- `raidArrivalMode`: 도착 방식
- `raidAgeRestriction`: 연령 제한
- `spawnCenter`: 스폰 중심 위치
- `pawnGroups`: Pawn 그룹 설정
- `canKidnap`: 납치 가능 여부
- `canSteal`: 도둑질 가능 여부
- `canTimeoutOrFlee`: 시간 초과/도주 가능 여부

#### 3.3.2 Lord

**주요 필드**:
- `lordManager`: LordManager 참조
- `curJob`: 현재 LordJob
- `curLordToil`: 현재 LordToil
- `graph`: StateGraph
- `faction`: 세력
- `ownedPawns`: 소유 Pawn 목록
- `ownedBuildings`: 소유 건물 목록
- `ticksInToil`: 현재 Toil에서 경과 틱
- `numPawnsLostViolently`: 폭력적으로 잃은 Pawn 수
- `initialColonyHealthTotal`: 초기 콜로니 건강 총합

#### 3.3.3 StateGraph

**주요 필드**:
- `toils`: LordToil 목록
- `transitions`: Transition 목록
- `startingToil`: 시작 Toil

---

## 4. 상태 전환 예시

### 4.1 ImmediateAttack 전략

```
[Pawn 생성 및 스폰]
    ↓
[Lord 생성]
    ├─ LordJob_AssaultColony 생성
    └─ StateGraph 생성
    ↓
[LordToil_AssaultColony 시작]
    ├─ Pawn들이 콜로니 공격
    ├─ AI 작업 할당 (JobGiver_AIFightEnemy)
    └─ 매 틱 Trigger 체크
    ↓
[조건 충족 시]
    ├─ 시간 초과 (26~38초) → LordToil_ExitMap
    ├─ 콜로니 피해 25~35% → LordToil_ExitMap
    ├─ 납치 대상 발견 → LordJob_Kidnap
    ├─ 고가치 아이템 발견 → LordJob_Steal
    └─ 비적대화 → LordToil_ExitMap
    ↓
[LordToil_ExitMap]
    ├─ Pawn들이 맵 가장자리로 이동
    └─ 맵을 벗어나면 제거
    ↓
[Lord 정리]
```

### 4.2 Siege 전략

```
[Pawn 생성 및 스폰]
    ↓
[Lord 생성]
    ├─ LordJob_Siege 생성
    └─ StateGraph 생성
    ↓
[LordToil_Travel 시작]
    ├─ 공성 위치로 이동
    └─ 도착 시 → LordToil_Siege
    ↓
[LordToil_Siege 시작]
    ├─ 모르타르 설치 및 포격
    ├─ 일부 Pawn은 건설 작업
    └─ 매 틱 Trigger 체크
    ↓
[조건 충족 시]
    ├─ 건설자 없음 → LordJob_AssaultColony
    ├─ 포격 무기 없음 → LordJob_AssaultColony
    ├─ Pawn 피해 8% → LordJob_AssaultColony
    ├─ Pawn 손실 30% → LordJob_AssaultColony
    ├─ 1.5~3일 경과 → LordJob_AssaultColony
    └─ 비적대화 → LordToil_ExitMap
    ↓
[LordJob_AssaultColony]
    ├─ 즉시 공격으로 전환
    └─ 공격 로직 실행
    ↓
[LordToil_ExitMap]
    ↓
[Lord 정리]
```

---

## 5. 주요 메커니즘

### 5.1 Duty 시스템

**Duty**: Pawn에게 할당되는 작업 지시

**주요 Duty 종류**:
- `DutyDefOf.AssaultColony`: 콜로니 공격
- `DutyDefOf.Sapper`: 공병 작업
- `DutyDefOf.Breaching`: 벽 돌파
- `DutyDefOf.Siege`: 공성전
- `DutyDefOf.ExitMap`: 맵 이탈

**할당 과정**:
1. `LordToil.UpdateAllDuties()` 호출
2. 각 Pawn에 대해 `CreateDuty()` 호출
3. `Pawn.mindState.duty` 설정
4. AI가 Duty에 따라 작업 선택

### 5.2 JobGiver 시스템

**JobGiver**: Pawn의 작업 할당자

**주요 JobGiver**:
- `JobGiver_AIFightEnemy`: 적과 전투
- `JobGiver_AIAttackEnemy`: 적 공격
- `JobGiver_AISapper`: 공병 작업
- `JobGiver_ExitMap`: 맵 이탈

**작동 방식**:
1. `Pawn.mindState.duty` 확인
2. Duty에 맞는 JobGiver 선택
3. `JobGiver.TryGiveJob()` 호출
4. 적절한 Job 반환

### 5.3 Trigger 시스템

**Trigger**: 상태 전환 조건

**체크 주기**:
- 매 틱마다 모든 Transition의 Trigger 체크
- 조건 충족 시 Transition 실행

**주요 Trigger**:
- 시간 기반: `Trigger_TicksPassed`
- 피해 기반: `Trigger_FractionColonyDamageTaken`, `Trigger_PawnHarmed`
- 손실 기반: `Trigger_FractionPawnsLost`
- 관계 기반: `Trigger_BecameNonHostileToPlayer`
- 상황 기반: `Trigger_KidnapVictimPresent`, `Trigger_HighValueThingsAround`

---

## 6. 참고 파일

- `RimworldSource/RimWorld/IncidentWorker_Raid.cs`
- `RimworldSource/RimWorld/RaidStrategyWorker.cs`
- `RimworldSource/RimWorld/RaidStrategyWorker_ImmediateAttack.cs`
- `RimworldSource/RimWorld/RaidStrategyWorker_Siege.cs`
- `RimworldSource/RimWorld/LordJob_AssaultColony.cs`
- `RimworldSource/RimWorld/LordJob_Siege.cs`
- `RimworldSource/RimWorld/LordJob_StageThenAttack.cs`
- `RimworldSource/Verse/AI/Group/LordJob.cs`
- `RimworldSource/Verse/AI/Group/Lord.cs`
- `RimworldSource/Verse/AI/Group/LordToil.cs`
- `RimworldSource/Verse/AI/Group/StateGraph.cs`

