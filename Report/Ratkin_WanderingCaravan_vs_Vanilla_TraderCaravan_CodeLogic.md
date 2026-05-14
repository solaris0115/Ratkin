# Ratkin WanderingCaravan Vanilla TraderCaravan CodeLogic Hostile Trade FloatMenu LordJob

**태그:** 랫킨 유랑단, RK_Faction_Caravan, LordJob_WanderingCaravan, LordJob_TradeWithColony, 적대 교역, FloatMenuOptionProvider_Trade, Pawn_TraderTracker.CanTradeNow, IncidentWorker_TraderCaravanArrival, TryResetCaravanFactionToNeutralIfCleared

**목적:** Def가 아니라 **실제 C# 동작** 기준으로 랫킨 유랑 상인(유랑단 캐러반)과 림월드 바닐라 상인 캐러반의 차이를 정리한다. (세부 라인 나열보다 **무엇이 다른지** 중심)

**참고 소스:** `Project/1.6/Source/WanderingTrader/`, `RimworldSource/RimWorld/` (LordJob_TradeWithColony, IncidentWorker_TraderCaravanArrival, FloatMenuOptionProvider_Trade, Pawn_TraderTracker 등)

---

## 1. 한 줄 요약

- **바닐라:** “상인 캐러반”은 **적대 팩션이면 인시던트 자체가 실패**하고, 맵에 있어도 **표준 교역 메뉴는 `CanTradeNow`(적대 제외) 등으로 막힌다.**
- **랫킨 유랑단:** **전용 숨김 팩션 + 커스텀 LordJob**으로 돌아가며, **인시던트가 적대 여부로 스폰을 막지 않고**, 대화/거래는 **커스텀 플로우로 `Dialog_Trade`를 직접 열어** 바닐라의 교역 가능 조건 검사를 **그대로 타지 않는다.** 또한 **맵에서 유랑단이 비면 팩션 관계를 중립으로 되돌리는** 전용 로직이 있어, 바닐라 상인과 **외교·재방문 체감**이 달라진다.

---

## 2. 인시던트(방문) 발생 조건

| 구분 | 바닐라 `IncidentWorker_TraderCaravanArrival` | 랫킨 `IncidentWorker_RatkinWanderingTrader` |
|------|-----------------------------------------------|---------------------------------------------|
| 팩션 후보 | `IncidentWorker_NeutralGroup` 계열: **Hidden이거나 플레이어에게 적대인 팩션은 그룹 소스에서 제외** | **항상 `RK_Faction_Caravan` 하나** (없으면 생성). 일반 세력 후보 랜덤이 아님 |
| 실행 시 적대 검사 | `TryExecuteWorker` 초반에 **`parms.faction.HostileTo(Faction.OfPlayer)`이면 즉시 false** (상인 안 옴) | **`HostileTo`로 스폰을 막는 분기 없음** |
| 그 외 게이트 | `preventNeutralVisitors`, 차단 Lord 등 | 동일 계열(`preventNeutralVisitors`), **공격 패널티 틱**, **다른 맵 포함 “이미 유랑단 활성”** 등 **모드 전용 조건** |

**차이의 의미:** 바닐라에서는 “적대면 상인 이벤트가 아예 안 뜬다”가 기본이고, 랫킨에서는 **같은 숨김 상인 팩션이 적대 상태로 남아 있어도** (다른 조건만 맞으면) **방문 로직이 돌아갈 수 있는 구조**다.

---

## 3. 팩션 관계·재방문 (플레이어가 말한 “다음 해 적대인데 교류”와 연결)

랫킨 전용 동작:

1. **`IncidentWorker_RatkinWanderingTrader.TryExecuteWorker`**  
   - `GameComponent_WanderingCaravan.WasAttackedByPlayer`가 true인 상태로 **새 방문이 시작될 때**, **`RK_Faction_Caravan`이 플레이어에게 적대면 `SetRelationDirect`로 중립 복구**하는 경로가 있다.  
   - 즉 “공격 패널티 이후 첫 방문” 루틴과 **팩션 관계 강제 조정**이 묶여 있다.

2. **`WanderingCaravanUtility.TryResetCaravanFactionToNeutralIfCleared`**  
   - **맵 어디에도 유랑단이 더 이상 활성으로 없을 때**, `RK_Faction_Caravan`이 플레이어에게 적대면 **중립으로 되돌린다.**  
   - `LordJob_WanderingCaravan.Notify_PawnLost` 등에서 호출된다.

**바닐라:** 이런 “숨김 상인 전용 팩션” 자동 중립화는 없고, **상인은 원칙적으로 적대가 아닌 세력**에서만 온다.

**차이의 의미:** 플레이어 UI/외교 패널의 “적대”가 **`RK_Faction_Caravan`인지, 랫킨 본 세력인지**, 혹은 **퇴장 직후 자동 중립화 타이밍**과 겹치는지에 따라 **“적대인데 말 걸린다 / 거래된다”** 같은 체감 불일치가 나기 쉽다. 코드상으로는 **유랑단 경로가 바닐라 상인보다 관계를 적극적으로 덮어쓴다.**

---

## 4. Lord 행동 그래프 (이벤트 후 맵에서의 생애주기)

### 바닐라 `LordJob_TradeWithColony`

- Travel → `LordToil_DefendTraderCaravan`(chill) 대기 → 시간/돌려보내기 → **`LordToil_ExitMapAndEscortCarriers`** 중심 퇴장.
- **온도/이상 기상/특정 GameCondition**, **트레이더 중요 인물 손실**, **`Trigger_BecamePlayerEnemy`** 등 **다양한 트리거로 조기 퇴장·전투 퇴장**이 연결됨.
- 캐러반 구성원 역할은 **`TraderCaravanRole`(상인/호위/짐꾼/노예 등)**로 duty가 갈림.

### 랫킨 `LordJob_WanderingCaravan`

- Travel → **`LordToil_WanderingCaravanIdle`**(chill) → **`LordToil_ExitMapWanderingCaravan`** 퇴장 등 **모듈 전용 Toil** 체인.
- **피해·방어·`Trigger_BecamePlayerEnemy`** 등은 있으나, **바닐라만큼 분기 종류(이상 기상 전용, ImportantTraderCaravanPeopleLost 등)는 단순화**되어 있다.
- 구성은 **고정 PawnKind(리더/호위/유랑민 풀/햄스터 짐꾼)** 중심이며 **`GetTraderCaravanRole` 기반 분기가 아니다.**

**차이의 의미:** “위험하면 도망, 공격당하면 반격 후 이탈” 큰 그림은 비슷하게 맞췄지만, **세부 이탈·집결 조건·역할 duty**는 바닐라와 **동일 구현이 아니다.**

---

## 5. 교역·말풍선·우클릭 — **가장 큰 로직 차이 (적대 시에도 교류 가능해 보이는 이유)**

### 바닐라

- 맵에서 상인에게 거는 **기본 교역 UI**는 `FloatMenuOptionProvider_Trade` 쪽으로 간다.
- 여기서 먼저 **`(ITrader)clickedPawn.CanTradeNow`**를 본다.
- **`Pawn_TraderTracker.CanTradeNow`**는  
  `wantsToTradeWithColony` + 상태 체크 + **`Faction == null || !Faction.HostileTo(Faction.OfPlayer)`** 등을 요구한다.  
  → **적대면 표준 “거래하기” 자체가 안 뜬다.**
- 추가로 **`Pawn.CanTradeWith(상인 팩션, …)`**에서 **`faction.HostileTo(pawn.Faction)`이면 거절**한다.
- 상인 Lord 타입이 **`LordJob_TradeWithColony`**일 때만 **바닐라 “상인 돌려보내기(DismissTrader)”** 등 일부 옵션이 붙는다.

### 랫킨

- 리더 우클릭은 **`LordToil_WanderingCaravanIdle.ExtraFloatMenuOptions`** → **“대화하기 / 돌려보내기”** 커스텀 옵션.
- 검사는 주로 **사교 스탯 비활성 여부**, **경로 가능**, **정착민 여부** 수준이고, **`HostileTo` / `CanTradeNow` / `CanTradeWith`를 거치지 않는다.**
- 대화 도착 후 **`LordToil_WanderingCaravanIdle.CreateMainDialog`**에서 곧바로 **`new Dialog_Trade(colonist, leader, false)`**를 연다.  
  → **바닐라 교역 플로트 메뉴에서 막히는 “적대” 조건을 우회**한다.

**차이의 의미:** 질문에 나온 **“적대 상태면 말풍선이 안 보이고 교류 불가여야 하는데 유랑단은 된다”**는 현상은, 코드 관점에서 **랫킨이 의도적으로 별도 UX를 탔고, 그 경로에 적대 차단이 없기 때문**으로 설명되는 것이 자연스럽다. (말풍선 표시 자체는 별 모듈이지만, **교역 가능성 게이트**는 바닐라와 확실히 다르다.)

---

## 6. `Pawn_TraderTracker.Goods`와 Lord 타입 (부가 차이)

- `Goods` getter는 **Lord가 `LordJob_TradeWithColony`가 아닐 때** 리더 인벤토리만 먼저 훑는 분기와, **Lord가 있으면** 소속 폰들(짐꾼·노예 등)을 이어서 훑는 구조다.
- 랫킨 유랑단 Lord는 **`LordJob_WanderingCaravan`**이라 **첫 분기 조건은 바닐라 상인과 다르게 평가**된다. (뒤쪽 `lord != null` 블록으로 캐러반 물품은 이어짐 — **완전 끊김은 아님**)

실질 플레이어 체감에 더 큰 것은 위 **5절의 UI 진입 차이**다.

---

## 7. 정리 표

| 항목 | 바닐라 상인 캐러반 | 랫킨 유랑단 |
|------|-------------------|------------|
| 팩션 | 일반 세력 (비적대 후보만) | 숨김 `RK_Faction_Caravan` + 명부/풀 |
| 적대 시 스폰 | **불가** (`HostileTo` 검사) | **차단 안 함** |
| 적대 시 표준 교역 메뉴 | **`CanTradeNow`로 불가** | 해당 경로 **미사용** |
| 거래 창 열기 | `TradeWithPawn` 잡 등 표준 흐름 | **커스텀 대화 → `Dialog_Trade` 직접** |
| 관계 보정 | 없음(세력 그대로) | **활성 유랑단 없음 / 패널티 후 방문 등에서 중립 복구** |
| LordJob | `LordJob_TradeWithColony` | `LordJob_WanderingCaravan` |
| 퇴장·위기 분기 | 세부 트리거 다수 | 유사 큰 줄기 + 일부 단순화 |

---

## 8. 결론 (원인 가설을 코드와 연결)

- **“적대인데도 다음 해에 교류된다”**는 체감은  
  (A) **`RK_Faction_Caravan`만 중립으로 돌아가고 플레이어가 보는 다른 팩션은 여전히 적대**이거나,  
  (B) **방문은 되는데 UI가 바닐라 상인과 다른 길**이라 **적대 차단이 안 걸리는**  
  두 축이 겹칠 수 있다.
- 코드상 확실한 차이는: **랫킨 유랑단은 바닐라 상인의 `HostileTo` 스폰 차단 + `CanTradeNow`/`CanTradeWith` 게이트를 동일하게 쓰지 않는다.**

---

*작성: 코드 비교 기준. 밸런스·Def 수치는 범위 밖.*
