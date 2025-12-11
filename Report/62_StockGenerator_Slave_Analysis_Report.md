# StockGenerator_Slave 기능 분석 보고서

## 개요

RimWorld에서 상인에게서 Slave와 같은 Pawn을 거래할 때의 처리 흐름과 구현 방식에 대한 분석 보고서입니다.

## 1. StockGenerator_Slaves 구조

### 1.1 클래스 위치 및 상속
- **파일**: `RimworldSource/RimWorld/StockGenerator_Slaves.cs`
- **상속**: `StockGenerator` 클래스를 상속
- **네임스페이스**: `RimWorld`

### 1.2 주요 필드
```csharp
private bool respectPopulationIntent;  // 인구 의도 존중 여부
public PawnKindDef slaveKindDef;       // 노예 종류 정의 (null이면 기본 Slave 사용)
```

### 1.3 GenerateThings 메서드
- **목적**: 상인 재고에 노예 Pawn 생성
- **생성 조건**:
  - `respectPopulationIntent`가 true면 인구 의도 확인
  - Ideology DLC 활성화 시 팩션의 이데올로기가 노예제를 승인하는지 확인
  - 랜덤한 비플레이어 팩션 선택 (humanlikeFaction, temporary 아님)
- **생성 방식**: `PawnGenerator.GeneratePawn()` 사용
- **반환**: `IEnumerable<Thing>` (Pawn 객체들)

### 1.4 HandlesThingDef 메서드
- **조건**: `ThingCategory.Pawn`이고 `Humanlike`이며 `tradeability > Tradeability.None`

## 2. 거래 처리 흐름

### 2.1 거래 시작
1. **TradeSession.SetupWith()**: 거래 세션 초기화
2. **TradeDeal 생성**: 거래 가능한 아이템 목록 생성
3. **Tradeable_Pawn 생성**: Pawn 거래 항목 생성 (`TradeDeal.AddToTradeables()`)

### 2.2 거래 실행
**TradeDeal.TryExecute()** (172-248줄):
- 거래 실행 전 검증 (화폐, 이데올로기 등)
- 각 Tradeable의 `ResolveTrade()` 호출

### 2.3 Pawn 거래 처리
**Tradeable_Pawn.ResolveTrade()** (60-84줄):
- **PlayerBuys 액션**:
  ```csharp
  List<Pawn> list2 = this.thingsTrader.Take(base.CountToTransferToSource).Cast<Pawn>().ToList<Pawn>();
  for (int j = 0; j < list2.Count; j++)
  {
      TradeSession.trader.GiveSoldThingToPlayer(list2[j], 1, TradeSession.playerNegotiator);
  }
  ```

### 2.4 플레이어에게 전달
**Pawn_TraderTracker.GiveSoldThingToPlayer()** (192-235줄):
- Pawn인 경우:
  1. `pawn.PreTraded(TradeAction.PlayerBuys, ...)` 호출
  2. Lord에서 제거 (`lord.Notify_PawnLost()`)
  3. Prisoner인 경우 `soldPrisoners`에서 제거

### 2.5 Pawn 전처리 및 팩션 변경
**Pawn.PreTraded()** (2769-2809줄):
- **PlayerBuys 액션 처리**:
  ```csharp
  if (action == TradeAction.PlayerBuys)
  {
      if (this.guest != null && this.guest.joinStatus == JoinStatus.JoinAsSlave)
      {
          this.guest.SetGuestStatus(Faction.OfPlayer, RimWorld.GuestStatus.Slave);
      }
      else
      {
          Need_Mood mood = this.needs.mood;
          if (mood != null)
          {
              mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FreedFromSlavery, null, null);
          }
          this.SetFaction(Faction.OfPlayer, null);  // ⭐ 즉시 플레이어 팩션으로 변경
      }
  }
  ```

## 3. 핵심 발견 사항

### 3.1 즉시 구성원화
- **현재 동작**: `Pawn.PreTraded()`에서 `SetFaction(Faction.OfPlayer)` 호출로 **즉시 식민지 구성원**이 됨
- **위치**: `RimworldSource/Verse/Pawn.cs` 2807줄

### 3.2 일시적 구성원 기능 부재
- 현재 시스템에는 **일시적으로 머물다가 이탈하는 기능이 없음**
- 모든 구매한 Pawn은 즉시 영구 구성원이 됨

## 4. 구현 필요 사항

### 4.1 일시적 구성원 시스템 구현 필요
사용자 요구사항:
- Pawn이 15~30일 동안 머물면서 일시적 구성원이 되었다가 이탈

### 4.2 구현 방안
1. **Pawn.PreTraded() 패치**:
   - `SetFaction(Faction.OfPlayer)` 호출 전에 일시적 구성원 여부 확인
   - 일시적 구성원인 경우 별도 처리

2. **일시적 구성원 컴포넌트 생성**:
   - `CompTemporaryColonist` 같은 ThingComp 추가
   - 남은 일수 추적 (15~30일 랜덤)
   - 매일 체크하여 일수 경과 시 이탈 처리

3. **이탈 처리**:
   - 일수 경과 시 `SetFaction(null)` 또는 원래 팩션으로 복귀
   - 맵에서 제거 또는 떠나는 행동 시작

## 5. 관련 파일 목록

- `RimworldSource/RimWorld/StockGenerator_Slaves.cs` - 노예 생성기
- `RimworldSource/RimWorld/Tradeable_Pawn.cs` - Pawn 거래 처리
- `RimworldSource/RimWorld/TradeDeal.cs` - 거래 실행
- `RimworldSource/RimWorld/Pawn_TraderTracker.cs` - 상인 Pawn의 거래 처리
- `RimworldSource/Verse/Pawn.cs` - Pawn의 PreTraded 메서드 (2807줄에서 SetFaction 호출)

## 6. 참고사항

- 현재 프로젝트에는 `StockGenerator_Mercenary`가 있어 참고 가능
- `Project/1.6/Source/ShieldOfRatkinia/StockGen_Mercenary.cs` 참조

