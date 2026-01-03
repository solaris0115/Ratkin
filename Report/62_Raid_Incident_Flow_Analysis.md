# Raid Incident 흐름 분석 보고서

## 개요
이 보고서는 RimWorld에서 Incident가 발생하여 Raid가 생성될 때, pawnGroupMaker와 전략(RaidStrategy)이 어떻게 선택되는지 전체 흐름을 분석합니다.

## 전체 흐름 개요

```
Incident 발생
    ↓
레이드 포인트 결정 (StorytellerUtility.DefaultThreatPointsNow)
    ↓
Faction 선택 (RaidCommonalityFromPoints 기반 가중치)
    ↓
PawnGroupKindDef 결정 (기본값: Combat)
    ↓
RaidStrategy 선택 (SelectionWeightForFaction 기반 가중치)
    ↓
PawnsArrivalMode 선택 (RaidStrategy의 arriveModes 중 선택)
    ↓
RaidAgeRestriction 결정 (선택적)
    ↓
레이드 포인트 조정 (AdjustedRaidPoints)
    ↓
PawnGroupMaker 선택 (commonality 기반 가중치)
    ↓
PawnKind 선택 (포인트 기반 반복 선택)
    ↓
Pawn 생성 및 배치
```

## 단계별 상세 분석

### 1. Incident 발생 및 레이드 포인트 결정

**위치**: `IncidentWorker_Raid.TryGenerateRaidInfo()`

**프로세스**:
1. Storyteller가 Incident를 발생시킴
2. `ResolveRaidPoints()` 호출하여 레이드 포인트 결정
   - 기본값: `StorytellerUtility.DefaultThreatPointsNow()`
   - 기지 가치, 콜로니스트 수, 동물, 메크 등을 고려하여 계산

**결과**: `IncidentParms.points`에 레이드 포인트 저장

---

### 2. Faction 선택

**위치**: `IncidentWorker_RaidEnemy.TryResolveRaidFaction()`

**프로세스**:
1. `parms.faction`이 이미 설정되어 있으면 그대로 사용
2. 그렇지 않으면 `PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroupWeighted()` 호출
3. 사용 가능한 Faction 필터링:
   - `FactionCanBeGroupSource()` 조건 통과
   - HostileTo(Faction.OfPlayer) == true
   - deactivated == false
   - earliestRaidDays 조건 충족
   - raidArrivalLayerWhitelist/Blacklist 조건 충족
   - 사용 가능한 RaidStrategy가 존재
4. 가중치 기반 선택:
   - 가중치 = `faction.def.RaidCommonalityFromPoints(points)`
   - 최근 레이드 Faction은 0.4배 페널티 적용

**FactionDef의 RaidCommonalityFromPoints**:
- `raidCommonalityFromPointsCurve` 곡선을 사용하여 포인트에 따른 가중치 계산
- 곡선이 없으면 기본값 1.0 반환

**결과**: `parms.faction`에 선택된 Faction 저장

---

### 3. PawnGroupKindDef 결정

**위치**: `IncidentWorker_Raid.TryGenerateRaidInfo()`

**프로세스**:
1. `parms.pawnGroupKind`이 설정되어 있으면 그대로 사용
2. 그렇지 않으면 기본값 `PawnGroupKindDefOf.Combat` 사용

**`parms.pawnGroupKind`이 설정되는 경우**:

1. **Quest 생성 시** (`QuestGen_Threat.Raid()`):
   - Quest 시스템에서 레이드를 생성할 때 파라미터로 전달
   - 파라미터가 null이면 기본값 `PawnGroupKindDefOf.Combat` 사용
   - Quest에서 특정 그룹 종류를 요구하는 경우 명시적으로 설정

2. **특수 Incident Worker들**:
   - **`IncidentWorker_ShamblerAssault`**: 
     - `PawnGroupKindDefOf.Shamblers`로 설정
     - Anomaly DLC의 Shambler 공격 이벤트
   - **`IncidentWorker_PsychicRitualSiege`**: 
     - `PawnGroupKindDefOf.PsychicRitualSiege`로 설정
     - Anomaly DLC의 사이킥 의식 공성 이벤트

3. **일반적인 경우**:
   - 대부분의 일반 레이드는 `parms.pawnGroupKind`이 null이므로 기본값 `Combat` 사용

**PawnGroupKindDef의 역할**:
- 그룹 생성 방식을 결정하는 Worker 클래스 지정
- Combat: 일반 전투 그룹 생성
- Trader: 상인 그룹 생성
- Shamblers: Shambler 특수 그룹 생성
- PsychicRitualSiege: 사이킥 의식 공성 그룹 생성
- 기타 특수 그룹 타입들

**결과**: `groupKind` 변수에 저장

---

### 4. RaidStrategy 선택

**위치**: `IncidentWorker_RaidEnemy.ResolveRaidStrategy()`

**프로세스**:
1. `parms.raidStrategy`가 이미 설정되어 있으면 그대로 사용
2. 그렇지 않으면:
   - 모든 `RaidStrategyDef` 중에서 필터링:
     - `CanUseWith(parms, groupKind)` == true
     - `arriveModes`가 있거나 `parms.raidArrivalMode`가 설정되어 있음
   - 가중치 기반 선택:
     - 가중치 = `strategy.Worker.SelectionWeightForFaction(map, faction, points)`

**RaidStrategyWorker.SelectionWeightForFaction**:
1. `def.selectionWeightCurvesPerFaction`에 해당 Faction의 곡선이 있으면 사용
2. 그렇지 않으면 `SelectionWeight(map, basePoints)` 호출
   - `def.selectionWeightPerPointsCurve.Evaluate(basePoints)` 사용

**RaidStrategyWorker.CanUseWith 검사 항목**:
- Faction의 `disallowedRaidStrategies`에 포함되어 있지 않음
- `SelectionWeightForFaction` > 0
- `MinimumPoints(faction, groupKind)` 조건 충족
- 타일의 `blacklistedRaidStrategies`에 포함되어 있지 않음
- `layerWhitelist/Blacklist` 조건 충족

**결과**: `parms.raidStrategy`에 선택된 RaidStrategy 저장

---

### 5. PawnsArrivalMode 선택

**위치**: `IncidentWorker_Raid.ResolveRaidArriveMode()`

**프로세스**:
1. `parms.raidArrivalMode`가 이미 설정되어 있으면 그대로 사용
2. `forQuickMilitaryAid` 플래그가 있으면 특수 처리
3. 그렇지 않으면:
   - `raidStrategy.arriveModes` 중에서 필터링:
     - `CanUseWith(parms)` == true
   - 가중치 기반 선택:
     - 가중치 = `arrivalMode.Worker.GetSelectionWeight(parms)`

**결과**: `parms.raidArrivalMode`에 선택된 도착 방식 저장

---

### 6. RaidAgeRestriction 결정

**위치**: `IncidentWorker_RaidEnemy.ResolveRaidAgeRestriction()`

**프로세스**:
1. Biotech DLC 활성화 여부 확인
2. `RaidAgeRestrictionDefOf.Children`의 `CanUseWith()` 확인
3. 확률(`chance`)에 따라 적용

**결과**: `parms.raidAgeRestriction`에 선택적으로 저장

---

### 7. 레이드 포인트 조정

**위치**: `IncidentWorker_Raid.AdjustedRaidPoints()`

**프로세스**:
1. PawnsArrivalMode 보정:
   - `raidArrivalMode.pointsFactorCurve`가 있으면 적용
2. RaidStrategy 보정:
   - `raidStrategy.pointsFactorCurve`가 있으면 적용
3. RaidAgeRestriction 보정:
   - `ageRestriction.threatPointsFactor` 적용
4. 타일 레이어 보정:
   - `target.Tile.LayerDef.raidPointsFactor` 적용
5. 최소 포인트 보장:
   - `raidStrategy.Worker.MinimumPoints(faction, groupKind) * 1.05` 이상 보장

**결과**: 조정된 포인트가 `parms.points`에 저장

---

### 8. PawnGroupMaker 선택

**위치**: `PawnGroupMakerUtility.TryGetRandomPawnGroupMaker()`

**프로세스**:
1. `faction.def.pawnGroupMakers` 중에서 필터링:
   - `gm.kindDef == parms.groupKind`
   - `gm.CanGenerateFrom(parms)` == true

**PawnGroupMaker.CanGenerateFrom 검사 항목**:
- `parms.points <= maxTotalPoints`
- `disallowedStrategies`에 `parms.raidStrategy`가 포함되어 있지 않음
- `parms.points >= MinPointsToGenerateAnything()`
- RaidStrategy의 `CanUseWithGroupMaker()` 통과 (필요시)
- `kindDef.Worker.CanGenerateFrom()` 통과

2. 가중치 기반 선택:
   - `ignoreCommonality`가 false이면: `commonality` 값으로 가중치 선택
   - `ignoreCommonality`가 true이면: 동일 확률로 랜덤 선택

**결과**: 선택된 `PawnGroupMaker` 반환

---

### 9. PawnKind 선택

**위치**: `PawnGroupKindWorker_Normal.GeneratePawns()`

**프로세스**:
1. `PawnGroupMakerUtility.ChoosePawnGenOptionsByPoints()` 호출
2. 포인트 기반 반복 선택:
   - 사용 가능한 옵션 수집 (`GetOptions()`)
     - `PawnGenOptionValid()` 통과
     - `CanUseOption()` 통과 (포인트, 비용 제한 등)
     - Xenotype 조합 생성 (Biotech 활성화 시)
   - 가중치 계산:
     - 기본 가중치 = `option.selectionWeight`
     - Xenotype 가중치 적용 (Biotech 활성화 시)
     - 비용 비율 가중치 적용:
       - `PawnWeightFactorByMostExpensivePawnCostFractionCurve.Evaluate(cost / highestCost)`
   - 랜덤 선택
   - 포인트 차감 후 반복
3. 선택된 PawnGenOption으로 Pawn 생성:
   - `PawnGenerator.GeneratePawn()` 호출
   - RaidAgeRestriction 적용 (나이 범위 제한)
   - RaidStrategy의 `CanUsePawn()` 검사

**PawnGenOptionValid 검사 항목**:
- `generateFightersOnly`이면 `isFighter` 확인
- `dontUseSingleUseRocketLaunchers`이면 무기 태그 확인
- RaidStrategy의 `CanUsePawnGenOption()` 통과
- RaidAgeRestriction의 `CanUseKind()` 통과
- Bossgroup 예약 여부 확인
- `maxPerGroup` 제한 확인

**결과**: 생성된 Pawn 리스트 반환

---

### 10. Pawn 생성 및 배치

**위치**: `IncidentWorker_Raid.TryGenerateRaidInfo()`

**프로세스**:
1. RaidStrategy의 `TryGenerateThreats()` 호출 (특수 위협 생성, 선택적)
2. RaidStrategy의 `SpawnThreats()` 호출 (특수 스폰, 선택적)
3. 그렇지 않으면 일반 Pawn 생성:
   - `PawnGroupMakerUtility.GeneratePawns()` 호출
   - `PawnGroupMaker.GeneratePawns()` 호출
   - `PawnGroupKindWorker.GeneratePawns()` 호출
4. 도착 방식 적용:
   - `raidArrivalMode.Worker.Arrive(pawns, parms)` 호출
5. Lord 생성:
   - `raidStrategy.Worker.MakeLords(parms, pawns)` 호출

**결과**: 맵에 Pawn들이 배치되고 Lord가 생성됨

---

## 주요 엔티티 및 역할

### IncidentParms
레이드 생성에 필요한 모든 파라미터를 담는 구조체
- `points`: 레이드 포인트
- `faction`: 선택된 Faction
- `raidStrategy`: 선택된 RaidStrategy
- `raidArrivalMode`: 선택된 도착 방식
- `raidAgeRestriction`: 선택된 나이 제한
- `groupKind`: PawnGroupKindDef
- `target`: 대상 맵

### FactionDef
세력 정의
- `pawnGroupMakers`: 사용 가능한 PawnGroupMaker 리스트
- `raidCommonalityFromPointsCurve`: 포인트에 따른 선택 가중치 곡선
- `maxPawnCostPerTotalPointsCurve`: 개별 Pawn 최대 비용 제한 곡선
- `disallowedRaidStrategies`: 사용 불가능한 전략 리스트
- `earliestRaidDays`: 최소 레이드 발생 일수

### PawnGroupKindDef
그룹 종류 정의
- `workerClass`: 그룹 생성 로직을 담당하는 Worker 클래스 타입
- Combat: 일반 전투 그룹
- Trader: 상인 그룹
- 기타 특수 그룹 타입들

### RaidStrategyDef
레이드 전략 정의
- `workerClass`: 전략 로직을 담당하는 Worker 클래스 타입
- `selectionWeightPerPointsCurve`: 포인트에 따른 선택 가중치 곡선
- `selectionWeightCurvesPerFaction`: Faction별 선택 가중치 곡선
- `arriveModes`: 사용 가능한 도착 방식 리스트
- `pointsFactorCurve`: 포인트 보정 곡선
- `minPawns`: 최소 Pawn 수

### PawnGroupMaker
Pawn 그룹 생성기 정의
- `kindDef`: PawnGroupKindDef 참조
- `commonality`: 선택 가중치
- `maxTotalPoints`: 최대 포인트 제한
- `disallowedStrategies`: 사용 불가능한 전략 리스트
- `options`: 사용 가능한 PawnGenOption 리스트

### PawnGenOption
Pawn 생성 옵션
- `kind`: PawnKindDef 참조
- `selectionWeight`: 선택 가중치
- `Cost`: 포인트 비용

---

## 선택 기준 요약

### Faction 선택
- **가중치**: `RaidCommonalityFromPoints(points)`
- **페널티**: 최근 레이드 Faction은 0.4배
- **조건**: Hostile, 활성화됨, earliestRaidDays 충족, 사용 가능한 전략 존재

### RaidStrategy 선택
- **가중치**: `SelectionWeightForFaction(map, faction, points)`
  - Faction별 곡선 우선, 없으면 일반 곡선 사용
- **조건**: CanUseWith 통과, arriveModes 존재

### PawnGroupMaker 선택
- **가중치**: `commonality` 값
- **조건**: kindDef 일치, CanGenerateFrom 통과

### PawnKind 선택
- **가중치**: `selectionWeight * XenotypeWeight * CostRatioWeight`
- **조건**: PawnGenOptionValid 통과, 포인트/비용 제한 충족

---

## 참고사항

1. **포인트 조정 순서**: 원본 포인트 → 도착방식 보정 → 전략 보정 → 나이제한 보정 → 타일 보정 → 최소값 보장
2. **선택 우선순위**: 이미 설정된 값이 있으면 재선택하지 않음
3. **가중치 시스템**: 모든 선택은 가중치 기반 랜덤 선택을 사용
4. **조건 필터링**: 각 단계마다 엄격한 조건 검사를 통해 유효한 옵션만 선택 대상이 됨
5. **페널티 시스템**: 최근 레이드 Faction에 페널티를 적용하여 다양성 확보

