# MoveSpeed Stat 산정 및 이동 플로우

> **태그**: MoveSpeed Stat StatPart StatWorker Pathfinding TicksPerMove  
> **목적**: MoveSpeed 산정 흐름 및 이동 명령 시 사용 방식 파악

---

## 1. 전체 흐름

```
Stat 산정 (MoveSpeed)  →  TicksPerMove  →  경로 탐색 / 셀당 비용  →  실제 이동
```

---

## 2. Stat 산정 — StatPart 및 영향 요소

**기본값**: 3.0 c/s (ThingDef.statBases 또는 defaultBaseValue)

**capacityFactors**: Moving (weight 1) — PawnCapacity Moving 수준이 그대로 반영됨

**StatPart (순서대로 TransformValue 적용)**:

| StatPart | 조건 | 영향 |
|----------|------|------|
| **StatPart_Glow** | 인간형, 시야 가능, 어둠 비선호 | 조명 0% → 80%, 30% 이상 → 100% (곡선) |
| **StatPart_RevenantSpeed** | Anomaly, Revenant 종류, 가시 상태 | 노출 시간에 따른 속도 곡선 |
| **StatPart_TerrainMoveSpeed** | Odyssey | TransformValue 비어 있음. 지형은 **CostToMoveIntoCell**에서 적용 |

**그 외 StatWorker에서 적용되는 것들**: traits, hediffs, genes, apparel, lifeStage, skillNeed, capacityOffsets 등 (offset/factor)

---

## 3. Stat → TicksPerMove → 셀당 비용

**TicksPerMove** = 60 / MoveSpeed (직선), 대각선은 ×√2

**추가 적용 (TicksPerMove 계산 시)**:
- 날씨: `CurMoveSpeedMultiplier`로 나눔
- 구속: ×0.35
- Pawn 운반: ×0.6
- Downed + CanCrawl: CrawlSpeed 사용

**CostToMoveIntoCell (셀당 비용)**:
- 기본: TicksPerMoveCardinal / TicksPerMoveDiagonal
- + pathGrid 비용, edifice 비용
- terrain.tags + `moveSpeedFactorByTerrainTag`: 비용 ÷ factor (물/진흙 등)
- locomotionUrgency: Amble ×3, Walk ×2, Sprint ×0.75

---

## 4. 사용 흐름

1. **경로 탐색**: PathFinder가 `pawn.TicksPerMoveCardinal/Diagonal`을 job에 넣어 셀 비용 계산
2. **셀 이동**: Pawn_PathFollower가 `CostToMoveIntoCell`로 다음 셀 비용 산정
3. **틱 진행**: `CostToPayThisTick`만큼 차감, 0 이하 시 다음 셀로 이동 (Stagger/Flying 등 추가 적용)
