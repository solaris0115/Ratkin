# Execute Raid With Specifics 종합 분석 보고서

**태그**: Execute Raid With Specifics Debug Menu UI Flow Selection Process  
**작성일**: 2025-01-24  
**분석 목적**: RimWorld의 "Execute raid with specifics" 디버그 기능의 선택 플로우, 진행 과정, 정보 표시 방식 종합 분석

---

## 목차

1. [개요](#1-개요)
2. [선택 플로우](#2-선택-플로우)
3. [진행 과정](#3-진행-과정)
4. [정보 표시 방식](#4-정보-표시-방식)
5. [UI 구조 및 상호작용](#5-ui-구조-및-상호작용)
6. [유효성 검사 및 제약사항](#6-유효성-검사-및-제약사항)
7. [실제 실행 과정](#7-실제-실행-과정)

---

## 1. 개요

**기능 위치**: `RimworldSource/Verse/DebugActionsIncidents.cs`  
**메서드**: `ExecuteRaidWithSpecifics()` (165-273줄)  
**디버그 액션 경로**: 개발자 모드 → `Incidents` → `Execute raid with specifics...`

개발자 모드에서 레이드를 세부 설정으로 테스트할 수 있는 디버깅 기능으로, 단계별 메뉴를 통해 Faction, Points, RaidStrategy, PawnsArrivalMode, RaidAgeRestriction을 순차적으로 선택하여 레이드를 실행합니다.

---

## 2. 선택 플로우

### 2.1 전체 선택 흐름도

```
[개발자 모드 활성화]
    ↓
[디버그 메뉴 열기]
    ↓
[Incidents → Execute raid with specifics... 클릭]
    ↓
┌─────────────────────────────────────────┐
│ [1단계] Faction 선택 메뉴              │
│ - 모든 Faction 목록 표시                │
│ - 사용 불가능한 Faction: "[NO]" 표시   │
└─────────────────────────────────────────┘
    ↓ (Faction 선택)
┌─────────────────────────────────────────┐
│ [2단계] Points 선택 메뉴                │
│ - 20~10000 포인트 옵션 표시             │
│ - Extended 모드: 세밀한 간격            │
└─────────────────────────────────────────┘
    ↓ (Points 선택)
┌─────────────────────────────────────────┐
│ [3단계] RaidStrategy 선택 메뉴         │
│ - 모든 RaidStrategyDef 목록 표시        │
│ - 사용 불가능한 전략: "[NO]" 표시       │
└─────────────────────────────────────────┘
    ↓ (RaidStrategy 선택)
┌─────────────────────────────────────────┐
│ [4단계] PawnsArrivalMode 선택 메뉴     │
│ - "-Random-" 옵션 포함                  │
│ - 선택한 RaidStrategy와 호환성 체크     │
│ - 사용 불가능한 방식: "[NO]" 표시       │
└─────────────────────────────────────────┘
    ↓ (PawnsArrivalMode 선택)
┌─────────────────────────────────────────┐
│ [5단계] RaidAgeRestriction 선택 메뉴    │
│ (Biotech DLC 활성 시에만 표시)          │
│ - "-Random-" 옵션 포함                  │
│ - 사용 불가능한 제한: "[NO]" 표시       │
└─────────────────────────────────────────┘
    ↓ (RaidAgeRestriction 선택 또는 건너뛰기)
┌─────────────────────────────────────────┐
│ [최종 실행] DoRaid(parms) 호출         │
│ - IncidentDefOf.RaidEnemy 또는          │
│   IncidentDefOf.RaidFriendly 선택       │
│ - IncidentWorker.TryExecute() 실행      │
└─────────────────────────────────────────┘
```

### 2.2 각 단계별 선택 항목

#### 2.2.1 1단계: Faction 선택

**표시 내용**:
- 모든 Faction 목록 (`Find.FactionManager.AllFactions`)
- 표시 형식: `"{Faction.Name} ({Faction.def.defName})"`
- 예시: `"Rakinia (RatkinFaction)"`
- 사용 불가능한 경우: `"{Faction.Name} ({Faction.def.defName}) [NO]"`

**검증 조건**:
```csharp
IncidentWorker_PawnsArrive.FactionCanBeGroupSource(faction, parms, false)
```

**선택 가능한 Faction 조건**:
- `FactionCanBeGroupSource()` == true
- deactivated == false
- 적절한 레이드 전략이 존재

#### 2.2.2 2단계: Points 선택

**표시 내용**:
- Extended 모드 (true) 사용
- 포인트 범위: 20 ~ 10000

**포인트 옵션 상세**:
| 범위 | 간격 | 항목 수 |
|------|------|---------|
| 20 ~ 90 | 10 | 8개 |
| 100 ~ 475 | 25 | 16개 |
| 500 ~ 1450 | 50 | 20개 |
| 1500 ~ 5000 | 100 | 36개 |
| 6000 ~ 10000 | 1000 | 5개 |
| **총계** | - | **85개** |

**표시 형식**: `"{points} points"` (예: "500 points")

#### 2.2.3 3단계: RaidStrategy 선택

**표시 내용**:
- 모든 `RaidStrategyDef` 목록 (`DefDatabase<RaidStrategyDef>.AllDefs`)
- 표시 형식: `"{RaidStrategyDef.defName}"`
- 사용 불가능한 경우: `"{RaidStrategyDef.defName} [NO]"`

**주요 RaidStrategy 종류**:
- `ImmediateAttack`: 즉시 공격
- `ImmediateAttackFriendly`: 즉시 도움 제공 (우호 세력)
- `ImmediateAttackSmart`: 즉시 공격 (똑똑한 전술)
- `StageThenAttack`: 준비 후 공격
- `Siege`: 포격 공성전
- `ImmediateAttackSappers`: 공병을 이용한 터널 공격
- `ImmediateAttackBreaching`: 벽 돌파 공격
- `ImmediateAttackBreachingSmart`: 벽 돌파 공격 (똑똑한 전술)
- `EmergeFromWater`: 물에서 등장
- `PsychicRitualSiege` (Anomaly DLC): 사이킥 의식 공성전
- `ShamblerAssault` (Anomaly DLC): 좀비 시체 무리 공격

**검증 조건**:
```csharp
raidStrategy.Worker.CanUseWith(parms, PawnGroupKindDefOf.Combat)
```

#### 2.2.4 4단계: PawnsArrivalMode 선택

**표시 내용**:
- 모든 `PawnsArrivalModeDef` 목록 (`DefDatabase<PawnsArrivalModeDef>.AllDefs`)
- **"-Random-"** 옵션 (항상 첫 번째 항목)
- 표시 형식: `"{PawnsArrivalModeDef.defName}"`
- 사용 불가능한 경우: `"{PawnsArrivalModeDef.defName} [NO]"`

**주요 PawnsArrivalMode 종류**:
- `EdgeWalkIn`: 가장자리에서 걸어서 등장
- `EdgeDrop`: 가장자리에 드롭 포드
- `CenterDrop`: 중앙에 드롭 포드
- `RandomDrop`: 랜덤 위치에 드롭 포드
- `EdgeDropGroups`: 가장자리 그룹 드롭
- `EdgeWalkInGroups`: 가장자리 그룹 걸어서 등장
- `EdgeWalkInDarkness` (Anomaly DLC): 어둠 속 가장자리 등장
- `EmergeFromWater`: 물에서 등장
- `EdgeWalkInDistributedGroups` (Anomaly DLC): 분산 그룹 등장

**검증 조건**:
1. `arrivalMode.Worker.CanUseWith(parms)` == true
2. `selectedRaidStrategy.arriveModes.Contains(arrivalMode)` == true

**"-Random-" 옵션**:
- 선택 시 `parms.raidArrivalMode`를 설정하지 않음
- 이후 `ResolveRaidArriveMode()`에서 자동 선택

#### 2.2.5 5단계: RaidAgeRestriction 선택 (Biotech DLC)

**표시 조건**:
- `ModsConfig.BiotechActive == true`일 때만 표시

**표시 내용**:
- 모든 `RaidAgeRestrictionDef` 목록 (`DefDatabase<RaidAgeRestrictionDef>.AllDefs`)
- **"-Random-"** 옵션 (항상 첫 번째 항목)
- 표시 형식: `"{RaidAgeRestrictionDef.defName}"`
- 사용 불가능한 경우: `"{RaidAgeRestrictionDef.defName} [NO]"`

**주요 RaidAgeRestriction 종류**:
- `Children`: 어린이만
- `Adults`: 성인만
- `None`: 제한 없음

**검증 조건**:
```csharp
raidAgeRestriction.Worker.CanUseWith(parms)
```

---

## 3. 진행 과정

### 3.1 초기화 단계

**위치**: `ExecuteRaidWithSpecifics()` 시작 부분

**처리 내용**:
1. StorytellerComp에서 기본 `IncidentParms` 생성
   ```csharp
   StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First(...);
   IncidentParms parms = storytellerComp.GenerateParms(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap);
   ```
2. 강제 실행 플래그 설정
   ```csharp
   parms.forced = true;
   ```
3. 첫 번째 메뉴 (Faction 선택) 준비 및 표시

### 3.2 메뉴 표시 및 선택 단계

**UI 시스템**: `Dialog_DebugOptionListLister`

**각 단계의 처리**:
1. `List<DebugMenuOption>` 생성
2. 각 옵션에 대해:
   - 라벨 생성 (검증 실패 시 "[NO]" 추가)
   - `DebugMenuOption` 생성 (`DebugMenuOptionMode.Action`)
   - 선택 시 다음 단계로 진행하는 액션 등록
3. `Dialog_DebugOptionListLister` 생성 및 표시
   ```csharp
   Find.WindowStack.Add(new Dialog_DebugOptionListLister(list, null));
   ```

**중첩 메뉴 구조**:
- 각 선택은 새로운 `Dialog_DebugOptionListLister` 창을 엽니다
- 이전 창은 자동으로 닫히지 않지만, 새 창이 위에 표시됩니다
- 사용자는 ESC 키로 이전 단계로 돌아갈 수 있습니다

### 3.3 최종 실행 단계

**위치**: `DoRaid(parms)` 메서드

**처리 내용**:
1. Faction의 적대 여부 확인
   ```csharp
   bool isEnemy = faction.HostileTo(Faction.OfPlayer);
   ```
2. 적절한 IncidentDef 선택
   - 적대: `IncidentDefOf.RaidEnemy`
   - 우호: `IncidentDefOf.RaidFriendly`
3. IncidentWorker 실행
   ```csharp
   IncidentWorker worker = incidentDef.Worker;
   worker.TryExecute(parms);
   ```

---

## 4. 정보 표시 방식

### 4.1 UI 창 구조

**클래스**: `Dialog_DebugOptionListLister`  
**상속**: `Dialog_DebugOptionLister`

**주요 구성 요소**:
- **옵션 리스트**: `List<DebugMenuOption>`
- **헤더**: 선택적 문자열 (대부분 null)
- **필터 기능**: 텍스트 입력으로 옵션 필터링 가능
- **키보드 네비게이션**: 화살표 키로 옵션 이동, Enter로 선택

### 4.2 옵션 표시 형식

**기본 표시**:
- 각 옵션은 한 줄로 표시
- 라벨 텍스트만 표시
- 마우스 호버 시 하이라이트

**"[NO]" 표시**:
- 검증 실패 시 옵션 라벨 끝에 `" [NO]"` 추가
- 예: `"Siege [NO]"`, `"Rakinia (RatkinFaction) [NO]"`
- 클릭 가능하지만 실행 시 오류 발생 가능

**하이라이트 표시**:
- 현재 선택된 옵션은 하이라이트됨
- 키보드 네비게이션으로 이동 가능
- `HighlightedIndex` 속성으로 관리

### 4.3 필터 기능

**위치**: `Dialog_DebugOptionLister` (부모 클래스)

**기능**:
- 텍스트 입력 필드로 옵션 필터링
- 라벨 텍스트에 포함된 옵션만 표시
- 실시간 필터링 (입력 즉시 반영)

**사용 예시**:
- "Rak" 입력 → "Rakinia" 관련 옵션만 표시
- "Siege" 입력 → "Siege" 관련 옵션만 표시

### 4.4 키보드 단축키

| 키 | 기능 |
|----|------|
| `↑` / `↓` | 옵션 이동 |
| `Enter` | 선택된 옵션 실행 |
| `ESC` | 창 닫기 (이전 단계로 돌아가기) |
| `Tab` | 필터 입력 필드 포커스 |

---

## 5. UI 구조 및 상호작용

### 5.1 Dialog_DebugOptionListLister 구조

**파일**: `RimworldSource/LudeonTK/Dialog_DebugOptionListLister.cs`

**주요 메서드**:

#### 5.1.1 생성자
```csharp
public Dialog_DebugOptionListLister(IEnumerable<DebugMenuOption> options, string header = null)
{
    this.options = options.ToList<DebugMenuOption>();
    this.header = header;
}
```

#### 5.1.2 옵션 렌더링
```csharp
protected override void DoListingItems(Rect inRect, float columnWidth)
{
    // 필터 입력 필드 처리
    if (KeyBindingDefOf.Dev_ChangeSelectedDebugAction.KeyDownEvent)
    {
        this.ChangeHighlightedOption();
    }
    
    // 헤더 표시 (있는 경우)
    if (!string.IsNullOrEmpty(this.header))
    {
        base.DebugLabel(this.header, columnWidth);
    }
    
    // 각 옵션 렌더링
    int highlightedIndex = this.HighlightedIndex;
    for (int i = 0; i < this.options.Count; i++)
    {
        DebugMenuOption debugMenuOption = this.options[i];
        bool highlight = highlightedIndex == i;
        
        // 모드에 따라 다른 렌더링
        if (debugMenuOption.mode == DebugMenuOptionMode.Action)
        {
            base.DebugAction(debugMenuOption.label, columnWidth, debugMenuOption.method, highlight);
        }
        else if (debugMenuOption.mode == DebugMenuOptionMode.Tool)
        {
            base.DebugToolMap(debugMenuOption.label, columnWidth, debugMenuOption.method, highlight);
        }
    }
}
```

#### 5.1.3 Enter 키 처리
```csharp
public override void OnAcceptKeyPressed()
{
    if (GUI.GetNameOfFocusedControl() == "DebugFilter")
    {
        int highlightedIndex = this.HighlightedIndex;
        if (highlightedIndex >= 0)
        {
            this.Close(true);
            if (this.options[highlightedIndex].mode == DebugMenuOptionMode.Action)
            {
                this.options[highlightedIndex].method(); // 액션 실행
            }
        }
    }
}
```

### 5.2 DebugMenuOption 구조

**구성 요소**:
- `label`: 표시될 텍스트
- `mode`: `DebugMenuOptionMode.Action` 또는 `DebugMenuOptionMode.Tool`
- `method`: 선택 시 실행될 `Action` 델리게이트

**생성 예시**:
```csharp
new DebugMenuOption(
    "500 points",                    // label
    DebugMenuOptionMode.Action,      // mode
    delegate() {                     // method
        parms.points = 500f;
        // 다음 단계로 진행
    }
)
```

### 5.3 메뉴 중첩 구조

**특징**:
- 각 선택은 새로운 창을 엽니다
- 이전 창은 닫히지 않고 스택에 쌓입니다
- ESC 키로 이전 창으로 돌아갈 수 있습니다

**창 스택 관리**:
```csharp
Find.WindowStack.Add(new Dialog_DebugOptionListLister(list, null));
```

---

## 6. 유효성 검사 및 제약사항

### 6.1 Faction 검증

**검증 메서드**: `IncidentWorker_PawnsArrive.FactionCanBeGroupSource()`

**검증 항목**:
- Faction이 비활성화되지 않음 (`deactivated == false`)
- Faction이 적절한 레이드 전략을 가지고 있음
- Faction이 레이드 소스로 사용 가능함

**실패 시**: `"[NO]"` 표시

### 6.2 RaidStrategy 검증

**검증 메서드**: `RaidStrategyWorker.CanUseWith(parms, PawnGroupKindDefOf.Combat)`

**검증 항목**:
- Faction의 `disallowedRaidStrategies`에 포함되지 않음
- `SelectionWeightForFaction` > 0
- `MinimumPoints(faction, groupKind)` 조건 충족
- 타일의 `blacklistedRaidStrategies`에 포함되지 않음
- `layerWhitelist/Blacklist` 조건 충족

**실패 시**: `"[NO]"` 표시

### 6.3 PawnsArrivalMode 검증

**검증 항목**:
1. `arrivalMode.Worker.CanUseWith(parms)` == true
2. `selectedRaidStrategy.arriveModes.Contains(arrivalMode)` == true

**실패 시**: `"[NO]"` 표시

**특수 케이스**:
- `"-Random-"` 옵션은 항상 사용 가능
- 선택한 RaidStrategy의 `arriveModes`에 포함된 항목만 표시

### 6.4 RaidAgeRestriction 검증

**검증 메서드**: `RaidAgeRestrictionDef.Worker.CanUseWith(parms)`

**검증 항목**:
- Biotech DLC 활성화 여부
- Faction이 해당 연령 제한을 지원하는지

**실패 시**: `"[NO]"` 표시

### 6.5 Points 제약사항

**범위**: 20 ~ 10000

**제약사항**:
- 일부 RaidStrategy는 최소 포인트 요구사항이 있음
  - `ImmediateAttackSmart`: 최소 1000 포인트
  - `Siege`: 최소 500 포인트
  - `ImmediateAttackSappers`: 최소 700 포인트
  - `ImmediateAttackBreaching`: 최소 700 포인트

---

## 7. 실제 실행 과정

### 7.1 DoRaid() 메서드 호출

**위치**: `DebugActionsIncidents.DoRaid(parms)`

**처리 내용**:
1. Faction의 적대 여부 확인
2. 적절한 IncidentDef 선택
3. `IncidentWorker.TryExecute(parms)` 호출

### 7.2 IncidentWorker 실행

**흐름**:
```
DoRaid(parms)
    ↓
IncidentDefOf.RaidEnemy/RaidFriendly.Worker.TryExecute(parms)
    ↓
IncidentWorker_Raid.TryExecuteWorker(parms)
    ↓
TryGenerateRaidInfo(parms)
    ├─ ResolveRaidPoints() - 포인트 결정 (이미 설정됨)
    ├─ TryResolveRaidFaction() - 세력 결정 (이미 설정됨)
    ├─ ResolveRaidStrategy() - 전략 결정 (이미 설정됨)
    ├─ ResolveRaidArriveMode() - 도착 방식 결정 (설정되지 않았으면 자동 선택)
    ├─ ResolveRaidAgeRestriction() - 연령 제한 결정 (Biotech DLC)
    ├─ TryGenerateThreats() - 특수 위협 생성 (선택적)
    ├─ TryResolveRaidSpawnCenter() - 스폰 위치 결정
    ├─ AdjustedRaidPoints() - 포인트 보정
    └─ SpawnThreats() 또는 PawnGroupMakerUtility.GeneratePawns() - Pawn 생성
    ↓
Pawn 생성 및 맵에 스폰
    ↓
PostProcessSpawnedPawns() - 생성 후 처리
    ↓
Letter 전송 (플레이어에게 알림)
    ↓
RaidStrategyWorker.MakeLords() - Lord 생성
    ↓
Lord.StateGraph 실행 시작
    ↓
레이드 진행
```

### 7.3 포인트 보정

**위치**: `AdjustedRaidPoints()`

**보정 요소**:
1. **도착 방식 보정**: DropPod 방식은 포인트 감소
2. **전략 보정**: 각 전략별 포인트 배율 적용
   - `ImmediateAttack`: 1.0 배율
   - `ImmediateAttackSmart`: 0.95 배율
   - `Siege`: 0.80~0.65 배율 (포인트에 따라 감소)
   - `ImmediateAttackSappers`: 0.85~0.60 배율
   - `ImmediateAttackBreaching`: 1.0~0.5 배율
3. **연령 제한 보정**: Children 제한 시 포인트 감소
4. **레이어 보정**: 지하 레이어 등 특수 레이어 보정

### 7.4 Pawn 생성

**과정**:
1. `PawnGroupMaker` 선택 (Faction의 `pawnGroupMakers` 중 선택)
2. `PawnGenOption` 선택 (포인트 기반 반복 선택)
3. `PawnGenerator.GeneratePawn()` 호출
4. `PawnsArrivalModeWorker.Arrive()` 호출하여 맵에 스폰

### 7.5 Lord 생성 및 상태 그래프 실행

**과정**:
1. `RaidStrategyWorker.MakeLords()` 호출
2. `LordJob` 생성 (전략별로 다른 Job)
3. `LordMaker.MakeNewLord()` 호출
4. `Lord.AddPawn()`으로 Pawn 할당
5. `Lord.StateGraph` 실행 시작
6. `LordToil` 실행 및 `Trigger` 체크
7. 조건 충족 시 `Transition` 발생
8. 레이드 종료 조건 충족 시 `LordToil_ExitMap`으로 전환

---

## 8. 사용 예시

### 8.1 기본 사용법

1. **개발자 모드 활성화**
   - 옵션 → 개발자 모드 활성화
   - 또는 `Prefs.DevMode = true`

2. **게임 내 맵에서 플레이 중**

3. **디버그 메뉴 열기**
   - 화면 상단의 디버그 메뉴 클릭

4. **Execute raid with specifics 실행**
   - `Incidents` → `Execute raid with specifics...` 선택

5. **단계별 선택**
   - Faction 선택 (예: "Rakinia (RatkinFaction)")
   - Points 선택 (예: "500 points")
   - RaidStrategy 선택 (예: "ImmediateAttack")
   - PawnsArrivalMode 선택 (예: "EdgeWalkIn")
   - (Biotech DLC) RaidAgeRestriction 선택 (예: "-Random-")

6. **레이드 실행**
   - 모든 선택 완료 시 자동으로 레이드 실행

### 8.2 테스트 시나리오 예시

**시나리오 1: Ratkin 세력 소규모 레이드**
- Faction: Rakinia
- Points: 200
- RaidStrategy: ImmediateAttack
- PawnsArrivalMode: EdgeWalkIn

**시나리오 2: 공성전 테스트**
- Faction: Rakinia
- Points: 1000
- RaidStrategy: Siege
- PawnsArrivalMode: EdgeDrop

**시나리오 3: 공병 공격 테스트**
- Faction: Rakinia
- Points: 800
- RaidStrategy: ImmediateAttackSappers
- PawnsArrivalMode: EdgeWalkInGroups

---

## 9. 관련 파일 및 클래스

### 9.1 핵심 파일

| 파일 | 용도 |
|------|------|
| `RimworldSource/Verse/DebugActionsIncidents.cs` | ExecuteRaidWithSpecifics() 메서드 정의 |
| `RimworldSource/LudeonTK/Dialog_DebugOptionListLister.cs` | 디버그 메뉴 UI 창 |
| `RimworldSource/Verse/DebugActionsUtility.cs` | PointsOptions() 등 유틸리티 메서드 |
| `RimworldSource/RimWorld/IncidentWorker_Raid.cs` | 레이드 실행 로직 |
| `RimworldSource/RimWorld/RaidStrategyWorker.cs` | 레이드 전략 처리 |

### 9.2 주요 클래스

| 클래스 | 용도 |
|--------|------|
| `DebugActionsIncidents` | 디버그 액션 정의 |
| `Dialog_DebugOptionListLister` | 디버그 메뉴 UI 창 |
| `DebugMenuOption` | 메뉴 옵션 데이터 구조 |
| `IncidentParms` | 이벤트 파라미터 |
| `IncidentWorker_Raid` | 레이드 실행 로직 |
| `RaidStrategyWorker` | 레이드 전략 처리 |
| `PawnsArrivalModeWorker` | 도착 방식 처리 |

---

## 10. 주의사항 및 팁

### 10.1 주의사항

1. **개발자 모드 필수**: `Prefs.DevMode` 활성화 필요
2. **맵 플레이 중**: `AllowedGameStates.PlayingOnMap` 상태에서만 사용 가능
3. **유효성 검사**: `[NO]` 표시된 옵션은 선택 불가능하거나 오류 발생 가능
4. **강제 실행**: `parms.forced = true`로 설정되어 일반 발생 조건 무시
5. **창 스택**: 여러 창이 쌓일 수 있으므로 ESC 키로 정리

### 10.2 디버깅 팁

1. **Points를 낮게 설정**: 소규모 레이드로 빠른 테스트
2. **`[NO]` 표시 원인 파악**: 관련 Def 설정 확인
3. **로그 확인**: `Log.Message()`로 `IncidentParms` 내용 출력
4. **필터 기능 활용**: 긴 목록에서 특정 항목 빠르게 찾기
5. **키보드 단축키 활용**: 마우스 없이 빠른 선택

### 10.3 Ratkin 프로젝트 활용 방안

1. **커스텀 RaidStrategy 테스트**: Ratkin 전용 전략 검증
2. **Faction별 레이드 밸런스 테스트**: Rakinia 세력 레이드 포인트 조정
3. **PawnKindDef 검증**: 특정 PawnKindDef가 레이드에 포함되는지 확인
4. **ArrivalMode 테스트**: 커스텀 등장 방식 검증
5. **연령 제한 테스트**: Biotech DLC 연령 제한 기능 검증

---

## 참고 자료

- [Report/62_Execute_Raid_With_Specifics_Debug_Analysis.md](Report/62_Execute_Raid_With_Specifics_Debug_Analysis.md)
- [Report/63_Raid_Execution_Flow_Analysis.md](Report/63_Raid_Execution_Flow_Analysis.md)
- [Report/62_Raid_Incident_Flow_Analysis.md](Report/62_Raid_Incident_Flow_Analysis.md)
