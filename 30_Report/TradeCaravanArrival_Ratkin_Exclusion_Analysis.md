# Trade Caravan Arrival 이벤트 분석 및 랫킨 캐러반 호출 방지

> **태그**: TradeCaravanArrival Incident Ratkin WanderingCaravan Faction Exclusion

## 1. TraderCaravanArrival 이벤트 구조

### 1.1 호출 흐름

```
Storyteller (FactionArrival 등)
    → IncidentDef: TraderCaravanArrival
    → IncidentWorker_TraderCaravanArrival
        → IncidentWorker_NeutralGroup (base)
        → IncidentWorker_PawnsArrive (base)
```

### 1.2 팩션 선택 조건 (FactionCanBeGroupSource)

| 단계 | 클래스 | 조건 |
|------|--------|------|
| 1 | IncidentWorker_PawnsArrive | `!f.IsPlayer`, `!f.defeated`, `!f.temporary`, 온도/레이어 등 |
| 2 | IncidentWorker_NeutralGroup | **`!f.Hidden`**, `!f.HostileTo(Player)`, `pawnGroupMakers`에 Trader 포함 |
| 3 | IncidentWorker_TraderCaravanArrival | `caravanTraderKinds.Count > 0`, TraderKindCommonality > 0 |

### 1.3 실행 시 동작

- `LordJob_TradeWithColony` 부여 (일반 상인 캐러반)
- `PawnGroupMakerUtility.GeneratePawns`로 Pawn 생성
- 맵 가장자리 스폰 → 식민지 근처 chillSpot으로 이동

---

## 2. 랫킨 관련 팩션 현황

| 팩션 | defName | hidden | caravanTraderKinds | TraderCaravanArrival 선택 |
|------|---------|--------|--------------------|---------------------------|
| 랫킨 왕국 | Rakinia | false | O (5종) | **가능** |
| 랫킨 캐러반 | RK_Faction_Caravan | **true** | O (1종) | **불가** |

### 2.1 RK_Faction_Caravan (유랑 상인)

- `hidden=true` → **IncidentWorker_NeutralGroup에서 이미 제외됨**
- TraderCaravanArrival로 호출되지 않음
- **별도 조치 불필요**

### 2.2 Rakinia (랫킨 왕국)

- `hidden=false`, `caravanTraderKinds` 5종 보유
- TraderCaravanArrival로 **선택 가능**
- LordJob_TradeWithColony로 일반 상인 캐러반 스폰 (유랑단 아님)

---

## 3. 문제 정의

- **RK_Incident_WanderingTrader**: 유랑단 전용 (LordJob_WanderingCaravan, GameComponent 연동)
- **TraderCaravanArrival**: 일반 상인 캐러반 (LordJob_TradeWithColony)

랫킨 왕국(Rakinia)이 TraderCaravanArrival로 선택되면, 유랑단과 별개의 일반 상인 캐러반이 도착함.  
유랑단만 사용하려면 Rakinia를 TraderCaravanArrival 후보에서 제외해야 함.

---

## 4. 해결 방안

### 방안 A: Rakinia에서 caravanTraderKinds 제거 (권장)

**효과**: Rakinia가 TraderCaravanArrival 후보에서 제외됨.

**영향**:
- TraderCaravanArrival로 랫킨 왕국 캐러반 도착 안 함
- `baseTraderKinds`는 유지 → **정착지 방문 시 거래는 그대로**
- `visitorTraderKinds`는 유지 → 방문자 거래는 그대로

**수정**: `Project/1.6/Defs/FactionDefs/Factions_Misc.xml` RatkinFactionBase

```xml
<!-- caravanTraderKinds 제거 또는 빈 리스트 -->
<caravanTraderKinds>
  <!-- 비움: TraderCaravanArrival로 호출 안 됨 -->
</caravanTraderKinds>
```

### 방안 B: Harmony Patch로 FactionCanBeGroupSource에서 Rakinia 제외

**효과**: Def 수정 없이 C#에서 Rakinia만 제외.

```csharp
[HarmonyPatch(typeof(IncidentWorker_TraderCaravanArrival), "FactionCanBeGroupSource")]
static class Patch_TraderCaravanArrival_ExcludeRatkin
{
    static void Postfix(Faction f, ref bool __result)
    {
        if (__result && f?.def?.defName == "Rakinia")
            __result = false;
    }
}
```

### 방안 C: FactionDef ModExtension + Worker 패치

- FactionDef에 `excludeFromTraderCaravanArrival` 같은 extension 추가
- IncidentWorker_TraderCaravanArrival를 상속한 커스텀 Worker에서 해당 extension 체크
- IncidentDef에서 workerClass를 커스텀 Worker로 교체

→ Def/Worker 수정이 많아져 비권장.

---

## 5. 권장 사항

| 목표 | 권장 방안 |
|------|-----------|
| 유랑단만 랫킨 상인으로 사용 | **방안 A**: Rakinia의 caravanTraderKinds 비우기 |
| Def 수정 없이 C#만 수정 | **방안 B**: Harmony Patch |
| RK_Faction_Caravan만 제외 | **조치 불필요** (이미 hidden으로 제외됨) |

---

## 6. 참고: Incident 비교

| Incident | Worker | LordJob | 팩션 |
|----------|--------|---------|------|
| TraderCaravanArrival | IncidentWorker_TraderCaravanArrival | LordJob_TradeWithColony | caravanTraderKinds 보유, !Hidden |
| RK_Incident_WanderingTrader | IncidentWorker_RatkinWanderingTrader | LordJob_WanderingCaravan | RK_Faction_Caravan (고정) |
