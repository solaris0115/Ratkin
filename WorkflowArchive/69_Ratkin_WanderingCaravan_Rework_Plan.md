# 랫킨 캐러반 시스템 개선 계획서

> **태그**: WanderingTrader Caravan Leader Interaction Dialog Rework  
> **상태**: 초안 - 사용자 검토 대기

---

## 1. 현재 시스템 요약

| 항목 | 현재 구현 |
|------|-----------|
| 트리거 | `IncidentWorker_RatkinWanderingTrader` (DevMode 수동 발동, baseChance=0) |
| 팩션 | `RK_Faction_Caravan` (hidden) |
| 구성원 | 상인 1 + 호위병 N + 판매용 pawn 3~6명 |
| 상호작용 | 상인(TraderKind 보유자)에게 우클릭 → FloatMenu → Dialog 창 |
| 기능 | 정착 희망 유랑민 보기 (은화로 구매), 용병 물색 (플레이스홀더) |
| LordJob | `LordJob_TradeWithColony` (바닐라 캐러밴 교역) |

### 현재 문제점
- 일반 교역 캐러밴과 구분이 안 됨 (같은 LordJob 사용)
- "리더"라는 개념이 없음 — 상인 = 교역 담당자일 뿐
- 상호작용이 FloatMenu 우클릭에 의존 → 유저가 기능 존재를 인지하기 어려움
- 용병 물색 기능 미구현

---

## 2. 개선 캐러반 개요

독립 히든 팩션. 오직 **상인, 유랑민, 떠돌이**로 구성된 캐러반.

### 2.1 캐러반 구성

| 역할 | PawnKind | 설명 |
|------|----------|------|
| **캐러반 리더** | `RK_PawnKind_CaravanLeader` (신규) | 상호작용 대상. 거래·의뢰 등 핵심 인물 |
| **캐러반 호위** | `RK_PawnKind_CaravanGuard` (신규) | 경무장 근접 위주 호위병 |
| **유랑민/떠돌이** | `RK_PawnKind_Nomad` / `RK_PawnKind_Wanderer` (기존) | 정착 제안 대상 |

- 리더와 호위는 PawnKind를 별도 구분

### 2.2 이벤트 등장 방식

- 스토리텔러에 의한 **주기적 방문**
- **매년 봄 (1분기 1~15일)** 이내에 확정 트리거
- 여름으로 넘어갈 수 있으나 트리거 확률이 점차 상승
- 최소 대기시간: 1년

### 2.3 이벤트 발생 절차

1. 캐러반이 마을까지 이동 (트레이더 진입 위치와 동일)
2. 일정 시간 동안 머묾
3. 시간이 되면 마을을 떠남

### 2.4 적대 조건

- 적대 행위를 당한 경우 반격 (일반 트레이더와 동일)
- 적대해도 **다음 해에 캐러반 재등장**
- 단, 진행 중이던 **"물건 의뢰"는 소멸**

---

## 3. 상호작용 설계

### 3.1 FloatMenu (리더 우클릭)

리더를 우클릭하면 다음 2가지만 표시:

| 옵션 | 동작 |
|------|------|
| **대화하기** | 메인 대화 Dialog 열기 |
| **돌려보내기** | 캐러반 즉시 퇴장 |

**구현 방식**: `LordToil.ExtraFloatMenuOptions` 사용 (보고서 §4.3, §6.3 참고)

- `LordToil_WanderingCaravanIdle`에서 `ExtraFloatMenuOptions(clickedPawn, forPawn)` 오버라이드
- `clickedPawn == leader`일 때만 옵션 반환
- `FloatMenuOptionProvider_FromLord`가 자동으로 Lord 소속 pawn의 `ExtraFloatMenuOptions`를 수집

```csharp
// LordToil_WanderingCaravanIdle.ExtraFloatMenuOptions 의사코드
if (requester != this.leader) yield break;

// 대화하기
yield return new FloatMenuOption("대화하기", () => {
    // Dialog_NodeTree로 메인 대화 창 오픈
    Find.WindowStack.Add(CreateMainDialog(leader, colonist));
}, MenuOptionPriority.InitiateSocial, ...);

// 돌려보내기
yield return new FloatMenuOption("돌려보내기", () => {
    this.lord.ReceiveMemo("CaravanDismissed");
}, MenuOptionPriority.Default, ...);
```

### 3.2 메인 대화 Dialog

**`Dialog_NodeTree` 활용** (보고서 §5.1 참고) — DiaNode 트리 기반 대화 창.

"대화하기" 선택 시 열리는 창. 아래 선택지 제공:

| 선택지 | 기능 | 상세 |
|--------|------|------|
| **1. 물자 거래** | 일반 거래 창 오픈 | 바닐라 TradeUI 활용. 희귀 물자 포함 |
| **2. 물건 의뢰** | 의뢰 전용 창 오픈 | 은화를 주고 원하는 물건을 확정 구매. 시장가 +10% |
| **3. 정착 제안하기** | 유랑민/떠돌이 목록 창 오픈 | 캐릭터 선택 → 몸값 절반 지불 → 정착민 합류 |
| **4. 대화 마치기** | Dialog 닫기 | — |

```csharp
// CreateMainDialog 의사코드
DiaNode root = new DiaNode("캐러반 리더 인사 메시지");

// 1. 물자 거래
DiaOption trade = new DiaOption("물자 거래");
trade.action = () => Find.WindowStack.Add(new Dialog_Trade(colonist, leader, false));
trade.resolveTree = true;
root.options.Add(trade);

// 2. 물건 의뢰
DiaOption commission = new DiaOption("물건 의뢰");
commission.action = () => Find.WindowStack.Add(new Dialog_CaravanCommission(leader));
commission.resolveTree = true;
root.options.Add(commission);

// 3. 정착 제안하기
DiaOption settle = new DiaOption("정착 제안하기");
settle.action = () => Find.WindowStack.Add(new Dialog_CaravanSettlers(leader, settlers));
settle.resolveTree = true;
root.options.Add(settle);

// 4. 대화 마치기
DiaOption close = new DiaOption("대화 마치기");
close.resolveTree = true;
root.options.Add(close);

return new Dialog_NodeTree(root, delayInteractivity: true, radioMode: false, title: "유랑단 리더");
```

---

## 4. 기능별 상세

### 4.1 물자 거래

- 바닐라 `Dialog_Trade` 활용 — 리더를 `ITrader`로 설정
- **희귀 물자 포함**: 평소 얻기 힘든 유니크 무기, 유전자, 진귀한 오락거리 등
- 희귀 물자 구성: 우선 `misc_unique` / `rare` 태그 아이템 5개로 임시 구성 (추후 조율)
- 리더 pawn에 `TraderKind` 부여 필요 (`pawn.mindState.wantsToTradeWithColony = true`)

### 4.2 물건 의뢰

- `Dialog_CaravanCommission` — 별도 Window 클래스
- 시장가격 대비 **+10%** 비용으로 확정 획득
- **초기 구현**: 빈 창 + 취소/완료 버튼만 (추후 내용 채움)

### 4.3 정착 제안하기

- `Dialog_CaravanSettlers` — 기존 `Dialog_WanderingTraderSettlers` 리팩터링
- 유랑민/떠돌이 목록을 별도 창으로 표시
- **표시 정보**:
  - 전신 포트레이트
  - 성인기 Backstory만 공개
  - 가장 높은 스킬 2가지 (열정 포함)
- **비용**: 몸값(MarketValue)의 **절반** 지불
- 선택 시 즉시 정착민으로 합류

---

## 5. LordJob 설계

### 5.1 상태 그래프

`LordJob_TradeWithColony` (보고서 §3.2) 기반으로 커스텀:

```
LordJob_WanderingCaravan
│
│  필드: Pawn leader, IntVec3 chillSpot, bool dismissed
│
├── LordToil_Travel(chillSpot)          ← 맵 진입 후 대기 지점으로 이동
│     │
│     ├─ Trigger_Memo("TravelArrived")
│     ▼
├── LordToil_WanderingCaravanIdle       ← 대기 (리더 상호작용 가능)
│     │  - ExtraFloatMenuOptions: 대화하기, 돌려보내기
│     │
│     ├─ Trigger_TicksPassed(27000~45000) → 시간 초과 퇴장
│     ├─ Trigger_Memo("CaravanDismissed") → 돌려보내기 퇴장
│     ├─ Trigger_BecamePlayerEnemy       → 적대 전환
│     ├─ Trigger_PawnHarmed              → 방어 모드
│     └─ Trigger_PawnExperiencingDangerousTemperatures → 즉시 퇴장
│     ▼
├── LordToil_ExitMap                    ← 맵 이탈
│
└── (적대 시) LordToil_ExitMapAndDefendSelf
```

### 5.2 ExposeData (저장/로드)

```csharp
public override void ExposeData() {
    base.ExposeData();
    Scribe_References.Look(ref leader, "leader");
    Scribe_Values.Look(ref chillSpot, "chillSpot");
    Scribe_Values.Look(ref dismissed, "dismissed");
}
```

---

## 6. IncidentWorker 수정 설계

### 6.1 기존 → 변경 비교

| 항목 | 기존 | 변경 |
|------|------|------|
| 리더 생성 | 없음 (상인이 자동 지정) | `RK_PawnKind_CaravanLeader`로 명시적 생성 |
| 호위 생성 | PawnGroupMaker 자동 | `RK_PawnKind_CaravanGuard`로 명시적 생성 |
| LordJob | `LordJob_TradeWithColony` | `LordJob_WanderingCaravan(leader, chillSpot)` |
| 판매용 pawn | `SalePawnKinds` 배열에서 랜덤 | 기존 유지 (`Nomad`, `Wanderer`) |
| Letter | 일반 캐러밴 도착 문구 | "유랑단 도착, 리더에게 말을 걸어보세요" |

### 6.2 스토리텔러 연동

IncidentDef에 스토리텔러 주기 설정 추가:

```xml
<IncidentDef>
  <defName>RK_Incident_WanderingTrader</defName>
  <workerClass>NewRatkin.IncidentWorker_RatkinWanderingTrader</workerClass>
  <category>Misc</category>
  <baseChance>1</baseChance>
  <earliestDay>0</earliestDay>
  <minRefireDays>45</minRefireDays>
  <!-- 스토리텔러 Comp에서 봄 1분기 트리거 제어 -->
</IncidentDef>
```

Storyteller Def에 OnOffCycle 추가:

```xml
<li Class="StorytellerCompProperties_OnOffCycle">
  <incident>RK_Incident_WanderingTrader</incident>
  <onDays>15</onDays>
  <offDays>45</offDays>
  <minSpacingDays>45</minSpacingDays>
  <numIncidentsRange>1~1</numIncidentsRange>
  <!-- 봄 1분기(0~15일) 집중, 이후 점차 확률 상승 -->
</li>
```

---

## 7. 작업 목록

### 7.1 Def 작업
- [ ] `RK_PawnKind_CaravanLeader` PawnKindDef 추가
- [ ] `RK_PawnKind_CaravanGuard` PawnKindDef 추가
- [ ] `RK_Backstory_CaravanLeader` BackstoryDef 추가
- [ ] IncidentDef 수정 — baseChance, 스토리텔러 주기 설정
- [ ] Storyteller Def에 OnOffCycle 추가 (봄 1분기 트리거)
- [ ] TraderKindDef 수정 — 희귀 물자 StockGenerator 추가
- [ ] 언어 키 추가/수정

### 7.2 C# 작업
- [ ] `LordJob_WanderingCaravan` — 전용 LordJob (이동 → 대기 → 퇴장), leader 필드, ExposeData
- [ ] `LordToil_WanderingCaravanIdle` — 대기 상태 + ExtraFloatMenuOptions (대화하기, 돌려보내기)
- [ ] 메인 대화 — `Dialog_NodeTree` + `DiaNode/DiaOption`으로 구성 (별도 클래스 불필요)
- [ ] `Dialog_CaravanSettlers` — 정착 제안 창 (기존 Dialog_WanderingTraderSettlers 리팩터링)
- [ ] `Dialog_CaravanCommission` — 물건 의뢰 창 (빈 창 + 취소/완료)
- [ ] `IncidentWorker_RatkinWanderingTrader` 수정 — 리더/호위 생성, 새 LordJob 사용, Letter 문구
- [ ] `FloatMenuOptionProvider_WanderingTrader` 제거 또는 축소 — LordToil에서 처리하므로

### 7.3 후순위 (추후 확장)
- [ ] 물건 의뢰 내용 구현
- [ ] 희귀 물자 목록 확정
- [ ] 유랑단 호감도 시스템 (반복 방문 시 할인 등)

---

## 8. 참고

### 8.1 바닐라 패턴 대응표

| 우리 시스템 | 바닐라 참고 | 보고서 섹션 |
|-------------|-------------|-------------|
| `LordJob_WanderingCaravan` | `LordJob_TradeWithColony` | §3.2 |
| `LordToil_WanderingCaravanIdle` + ExtraFloatMenuOptions | `LordToil_WaitForItems` | §4.3, §6.3 |
| 메인 대화 (Dialog_NodeTree) | `IncidentWorker_CaravanMeeting` | §5.1, §5.3 |
| 물자 거래 (Dialog_Trade) | `FloatMenuOptionProvider_Trade` | §4.2 |
| 돌려보내기 (Memo → 퇴장) | `LordJob_TradeWithColony.traderDismissed` | §3.2 |

### 8.2 관련 파일

| 역할 | 경로 |
|------|------|
| 현재 IncidentWorker | `Project/1.6/Source/WanderingTrader/IncidentWorker_RatkinWanderingTrader.cs` |
| 현재 FloatMenu | `Project/1.6/Source/WanderingTrader/FloatMenuOptionProvider_WanderingTrader.cs` |
| 현재 정착민 Dialog | `Project/1.6/Source/WanderingTrader/Dialog_WanderingTraderSettlers.cs` |
| 현재 가격 설정 | `Project/1.6/Source/WanderingTrader/WanderingTraderSettings.cs` |
| PawnKind Def | `Project/1.6/Defs/WanderingTrader/PawnKinds_WanderingTrader.xml` |
| Trader/Incident Def | `Project/1.6/Defs/WanderingTrader/RK_WanderingTrader.xml` |
| Faction Def | `Project/1.6/Defs/FactionDefs/Factions_Misc.xml` |
| 분석 보고서 | `Report/Visitor_Event_Leader_Interaction_Dialog_Pattern_Report.md` |
