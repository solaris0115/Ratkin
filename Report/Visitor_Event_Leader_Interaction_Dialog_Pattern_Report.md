# 방문 이벤트 및 리더 상호작용·대화창 패턴 분석 보고서

> **태그**: Visitor Incident Trader Beggar Leader Interaction Dialog FloatMenu LordJob Quest  
> **목적**: 이벤트로 방문한 그룹의 리더에게 말을 걸어 특정 창을 띄우고, 선택/교환/의뢰 등을 수행하는 공통 폼 분석

---

## 1. 개요

림월드에서 **거지, 방문객, 상인** 등 그룹이 방문 후 머물다 가는 이벤트는 다음 공통 구조를 가집니다:

1. **Incident** → 그룹 스폰 및 Letter 발송
2. **LordJob** → 그룹의 행동(이동, 대기, 퇴장) 제어
3. **대상(리더/상인)** → 플레이어가 우클릭으로 상호작용
4. **FloatMenu** → 우클릭 시 나타나는 메뉴
5. **Job** → 정착민이 대상에게 이동 후 실행
6. **Dialog/Window** → 선택, 교환, 의뢰 등을 처리하는 창

---

## 2. Incident 시스템: 방문 그룹 발생

### 2.1 주요 Incident 유형

| Incident | Worker | LordJob | 특징 |
|----------|--------|---------|------|
| `VisitorGroup` | `IncidentWorker_VisitorGroup` | `LordJob_VisitColony` | 방문객, 75% 확률로 1명 상인화 |
| `TraderCaravanArrival` | `IncidentWorker_TraderCaravanArrival` | `LordJob_TradeWithColony` | 상인 캐러밴 |
| `GiveQuest_Beggars` | `IncidentWorker_GiveQuest` | `LordJob_BegForItems` (QuestPart) | 거지 퀘스트 (Ideology) |
| `TravelerGroup` | `IncidentWorker_TravelerGroup` | `LordJob_VisitColony` | 여행자 그룹 |

### 2.2 Incident 실행 흐름 (VisitorGroup 예시)

```
IncidentWorker_VisitorGroup.TryExecuteWorker()
  ├─ TryResolveParms()           // 팩션, 포인트 등 해석
  ├─ SpawnPawns()                // PawnGroupMaker로 pawn 생성
  ├─ TryConvertOnePawnToSmallTrader()  // 75% 확률로 1명 상인화
  │   └─ pawn.mindState.wantsToTradeWithColony = true
  │   └─ pawn.trader.traderKind = ...
  │   └─ TradeUtility.CheckGiveTraderQuest(
  ├─ LordMaker.MakeNewLord(..., CreateLordJob(), ...)
  └─ SendLetter()
```

### 2.3 Incident Def 구조 (Incidents_Map_Special.xml)

```xml
<IncidentDef>
  <defName>VisitorGroup</defName>
  <workerClass>IncidentWorker_VisitorGroup</workerClass>
  <category>FactionArrival</category>
  <targetTags><li>Map_PlayerHome</li></targetTags>
  <requireColonistsPresent>True</requireColonistsPresent>
</IncidentDef>
```

---

## 3. Lord 그룹 관리: 방문·머무름·퇴장

### 3.1 LordJob_VisitColony (방문객)

- **chillSpot**: 대기 위치 (식민지 외곽)
- **durationTicks**: 머무는 시간 (null이면 8000~22000 틱 랜덤)
- **StateGraph**: Travel → DefendPoint → (시간 경과) → TravelAndExit

```
Travel(chillSpot) → DefendPoint(대기)
  ├─ Trigger_TicksPassed → 퇴장 (선물 체크 후)
  ├─ Trigger_PawnExperiencingDangerousTemperatures → 즉시 퇴장
  └─ Trigger_BecamePlayerEnemy → 적대 전환
```

### 3.2 LordJob_TradeWithColony (상인 캐러밴)

- **chillSpot**: 대기 위치
- **durationTicks**: 27000~45000 틱 (약 4.5~7.5일)
- **traderDismissed**: 플레이어가 "돌려보내기" 시 즉시 퇴장

```
Travel → DefendTraderCaravan2(대기)
  ├─ Trigger_TicksPassed → 퇴장
  ├─ Trigger_Custom(traderDismissed) → 즉시 퇴장
  └─ Trigger_PawnHarmed → 방어 모드
```

### 3.3 LordJob_BegForItems (거지 퀘스트)

- **target**: 아이템 수령 대상 pawn (리더)
- **thingDef, amount**: 요청 아이템
- **outSignalItemsReceived**: 수령 시 Quest에 전달할 시그널

```
LordToil_TravelAndWaitForItems → LordToil_WaitForItems
  ├─ Trigger_Custom(HasAllRequestedItems) → 퇴장 + 시그널
  └─ Trigger_BecamePlayerEnemy / Trigger_PawnKilled → 적대/퇴장
```

---

## 4. 대상(리더)와의 상호작용

### 4.1 FloatMenu 흐름

1. **FloatMenuMakerMap** → `GetOptions()` 호출
2. **FloatMenuOptionProvider** 목록 순회
3. `TargetPawnValid(clickedPawn)` → true인 Provider만
4. `GetOptionsFor(clickedPawn, context)` → FloatMenuOption 반환

### 4.2 상인과 거래 (FloatMenuOptionProvider_Trade)

- **조건**: `(ITrader)clickedPawn).CanTradeNow`
- **CanTradeNow** (Pawn_TraderTracker):  
  `wantsToTradeWithColony && CanCasuallyInteractNow && !Downed && ...`
- **동작**: `JobDefOf.TradeWithPawn` → `JobDriver_TradeWithPawn` → `Dialog_Trade` 오픈

```csharp
// FloatMenuOptionProvider_Trade.cs
Action action = () => {
    Job job = JobMaker.MakeJob(JobDefOf.TradeWithPawn, clickedPawn);
    context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, ...);
};
yield return new FloatMenuOption("TradeWith".Translate(...), action, ...);
```

### 4.3 거지에게 아이템 주기 (LordToil → ExtraFloatMenuOptions)

- **FloatMenuOptionProvider_FromLord**: Lord 소속 pawn 우클릭 시
- **Lord.CurLordToil.ExtraFloatMenuOptions(clickedPawn, context.FirstSelectedPawn)** 호출
- **LordToil_WaitForItems**: `target == requester`일 때 `GiveItemsToPawnUtility.GetFloatMenuOptionsForPawn()` 반환

```csharp
// LordToil_WaitForItems.ExtraFloatMenuOptions
if (this.target == requester) {
    foreach (var opt in GiveItemsToPawnUtility.GetFloatMenuOptionsForPawn(
        requester, current, requestedThingDef, requestedThingCount))
        yield return opt;
}
```

- **GiveItemsToPawnUtility**: "Give X to {pawn}" 메뉴 → `JobDefOf.GiveToPawn` 발급

### 4.4 패턴 정리

| 상호작용 유형 | FloatMenu 출처 | Job | 결과 창 |
|---------------|----------------|-----|---------|
| 상인 거래 | FloatMenuOptionProvider_Trade | TradeWithPawn | Dialog_Trade |
| 거지에게 주기 | FloatMenuOptionProvider_FromLord (LordToil) | GiveToPawn | (Job 완료 시 시그널) |
| 캐러밴 만남 (월드맵) | CaravanMeeting Incident | - | Dialog_NodeTreeWithFactionInfo |
| 통신기 (Comms) | Building_CommsConsole | UseCommsConsole | Dialog_Negotiation |

---

## 5. 대화/창 패턴

### 5.1 Dialog_NodeTree 기반

- **DiaNode**: 텍스트 + `List<DiaOption>` 옵션
- **DiaOption**: 텍스트, `link`(다음 노드), `action`, `resolveTree`(창 닫기)
- **Dialog_NodeTree**: `DiaNode` 트리를 표시하는 Window

```csharp
DiaNode root = new DiaNode("인사 메시지");

DiaOption opt1 = new DiaOption("거래하기");
opt1.action = () => Find.WindowStack.Add(new Dialog_Trade(negotiator, trader, false));
root.options.Add(opt1);

DiaOption opt2 = new DiaOption("의뢰하기");
opt2.link = new DiaNode("의뢰 내용...");
opt2.linkLateBind = () => CreateRequestNode();  // 동적 노드
root.options.Add(opt2);

DiaOption opt3 = new DiaOption("끝내기");
opt3.resolveTree = true;  // 창 닫기
root.options.Add(opt3);

Find.WindowStack.Add(new Dialog_NodeTree(root, delayInteractivity: true, radioMode: false, title: "대화"));
```

### 5.2 Dialog 변형

| 클래스 | 용도 |
|--------|------|
| `Dialog_NodeTree` | 기본 대화 트리 |
| `Dialog_NodeTreeWithFactionInfo` | 팩션 정보 표시 (캐러밴 만남 등) |
| `Dialog_Negotiation` | 통신기 대화 (negotiator, commTarget 표시) |
| `Dialog_Trade` | 거래 창 (ITrader, Pawn negotiator) |

### 5.3 IncidentWorker_CaravanMeeting 예시 (월드맵 캐러밴 만남)

```csharp
DiaNode diaNode = new DiaNode("CaravanMeeting".Translate(...));

// 거래 옵션
DiaOption tradeOpt = new DiaOption("CaravanMeeting_Trade".Translate());
tradeOpt.action = () => Find.WindowStack.Add(new Dialog_Trade(bestNegotiator, metCaravan, false));
diaNode.options.Add(tradeOpt);

// 공격 옵션
DiaOption attackOpt = new DiaOption("CaravanMeeting_Attack".Translate());
attackOpt.action = () => { /* 전투 시작 */ };
attackOpt.resolveTree = true;
diaNode.options.Add(attackOpt);

// 떠나기
DiaOption leaveOpt = new DiaOption("CaravanMeeting_MoveOn".Translate());
leaveOpt.resolveTree = true;
diaNode.options.Add(leaveOpt);

Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, faction, true, false, title));
```

---

## 6. 맵 내 리더와의 "말 걸기" 구현 패턴

### 6.1 접근 방식 비교

| 방식 | 트리거 | Job 필요 | 창 오픈 시점 |
|------|--------|----------|-------------|
| **A. Trade 패턴** | pawn이 ITrader + wantsToTradeWithColony | TradeWithPawn (이동 후) | Job Toil에서 |
| **B. Lord ExtraFloatMenu** | LordToil.ExtraFloatMenuOptions | GiveToPawn 등 | Job 완료 시 시그널 |
| **C. 직접 창 오픈** | FloatMenuOptionProvider 커스텀 | 없음 또는 단순 Goto | 우클릭 즉시 |

### 6.2 권장: "리더에게 말 걸기" 공통 폼

1. **FloatMenuOptionProvider** 또는 **LordToil.ExtraFloatMenuOptions**로 "말 걸기" 메뉴 추가
2. **Job**으로 정착민이 리더에게 이동 (선택적)
3. 도착 시 또는 **즉시** `Dialog_NodeTree`(또는 커스텀 Dialog) 오픈
4. **DiaNode** 트리에서:
   - 선택지 (의뢰, 교환, 정보 등)
   - `action`에서 추가 창(Dialog_Trade, 커스텀 창) 오픈
   - `resolveTree = true`로 대화 종료

### 6.3 커스텀 LordJob으로 "리더" 지정

- `LordJob`에 `Pawn leader` 또는 `Pawn interactionTarget` 필드 추가
- `LordToil`에서 `ExtraFloatMenuOptions(clickedPawn, forPawn)` 구현
- `clickedPawn == leader`일 때만 "말 걸기" 옵션 반환

```csharp
// 예: LordToil_VisitColonyWithLeader
public override IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Pawn requester, Pawn current) {
    if (requester != this.leader) yield break;
    yield return new FloatMenuOption("TalkToLeader".Translate(), () => {
        Find.WindowStack.Add(new Dialog_VisitorLeader(this.leader, current));
    }, MenuOptionPriority.InitiateSocial, ...);
}
```

---

## 7. 구현 체크리스트 (리더 대화 창)

| 단계 | 항목 | 참고 |
|------|------|------|
| 1 | Incident 또는 Quest로 방문 그룹 스폰 | IncidentWorker_VisitorGroup, QuestNode_Root_Beggars |
| 2 | LordJob에 리더/대상 pawn 지정 | LordJob_VisitColony, LordJob_BegForItems |
| 3 | LordToil에서 ExtraFloatMenuOptions 구현 | LordToil_WaitForItems, FloatMenuOptionProvider_FromLord |
| 4 | "말 걸기" FloatMenuOption 추가 | Job 또는 즉시 창 오픈 |
| 5 | Dialog_NodeTree 또는 커스텀 Window 작성 | DiaNode, DiaOption |
| 6 | 선택지별 action (의뢰, 교환 등) | FactionDialogMaker, IncidentWorker_CaravanMeeting |
| 7 | Quest 연동 (필요 시) | QuestPart, QuestUtility.SendQuestTargetSignals |

---

## 8. 관련 소스 파일

| 역할 | 경로 |
|------|------|
| Incident Def | RimworldData/Core/Defs/Storyteller/Incidents_Map_Special.xml |
| Visitor Worker | RimworldSource/RimWorld/IncidentWorker_VisitorGroup.cs |
| Trader Worker | RimworldSource/RimWorld/IncidentWorker_TraderCaravanArrival.cs |
| LordJob Visit | RimworldSource/RimWorld/LordJob_VisitColony.cs |
| LordJob Trade | RimworldSource/RimWorld/LordJob_TradeWithColony.cs |
| LordJob Beg | RimworldSource/RimWorld/LordJob_BegForItems.cs |
| FloatMenu Trade | RimworldSource/RimWorld/FloatMenuOptionProvider_Trade.cs |
| FloatMenu Lord | RimworldSource/RimWorld/FloatMenuOptionProvider_FromLord.cs |
| GiveToPawn | RimworldSource/RimWorld/GiveItemsToPawnUtility.cs |
| Dialog NodeTree | RimworldSource/Verse/Dialog_NodeTree.cs |
| DiaNode/Option | RimworldSource/Verse/DiaNode.cs, DiaOption.cs |
| Caravan Meeting | RimworldSource/RimWorld/IncidentWorker_CaravanMeeting.cs |
| Faction Dialog | RimworldSource/RimWorld/FactionDialogMaker.cs |

---

## 9. 랫킨 유랑단 적용 시 참고

- **WorkFlow/69_Ratkin_WanderingCaravan_Rework_Plan.md**와 연계
- 리더에게 "말 걸기" → `Dialog_NodeTree` 또는 `Dialog_NodeTreeWithFactionInfo`로 통합 창
- 선택지: 정착 희망 유랑민 보기, 용병 물색, (추가 의뢰/교환 등)
- `LordJob` 커스텀 시 `LordJob_TradeWithColony` 대신 `LordJob_VisitColony` 기반 + 리더 필드 추가 검토
