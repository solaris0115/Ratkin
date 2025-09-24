# FactionDef pawnGroupMakers 선택 과정 분석 보고서

## 개요
이 보고서는 RimWorld에서 FactionDef의 pawnGroupMakers가 레이드 포인트와 레이드 규모에 따라 어떻게 개체를 선택하는지에 대한 상세한 분석을 제공합니다.

## 1. 기지 가치에 의한 레이드 포인트 결정

### 1.1 기본 레이드 포인트 계산
레이드 포인트는 `StorytellerUtility.DefaultThreatPointsNow()` 메서드에서 계산됩니다:

```csharp
// StorytellerUtility.cs:210-268
public static float DefaultThreatPointsNow(IIncidentTarget target)
{
    float playerWealthForStoryteller = target.PlayerWealthForStoryteller;
    float num = StorytellerUtility.PointsPerWealthCurve.Evaluate(playerWealthForStoryteller);
    
    // 추가 계산...
    return Mathf.Clamp(num4 * num5 * Find.Storyteller.difficulty.threatScale * 
           Find.Storyteller.def.pointsFactorFromDaysPassed.Evaluate((float)GenDate.DaysPassedSinceSettle), 
           StorytellerUtility.GlobalPointsMin(), 10000f);
}
```

### 1.2 주요 계산 요소
1. **기지 가치 (PlayerWealthForStoryteller)**: 건물, 아이템, 동물 등의 총 가치
2. **PointsPerWealthCurve**: 기지 가치를 레이드 포인트로 변환하는 곡선
   - 0~14,000 가치: 0 포인트
   - 400,000 가치: 2,400 포인트  
   - 700,000 가치: 3,600 포인트
   - 1,000,000 가치: 4,200 포인트

3. **추가 보정 요소**:
   - 콜로니스트 수와 나이
   - 전투 훈련 가능한 동물
   - 메크 유닛
   - 서브휴먼 종족
   - 난이도 설정
   - 시간 경과 배율

## 2. 레이드 포인트와 레이드 규모 책정 원리

### 2.1 레이드 포인트 조정
레이드 포인트는 여러 요소에 의해 추가 조정됩니다:

```csharp
// IncidentWorker_Raid.cs:196-216
public static float AdjustedRaidPoints(float points, PawnsArrivalModeDef raidArrivalMode, 
    RaidStrategyDef raidStrategy, Faction faction, PawnGroupKindDef groupKind, 
    IIncidentTarget target, RaidAgeRestrictionDef ageRestriction = null)
{
    // 도착 방식 보정
    if (raidArrivalMode.pointsFactorCurve != null)
        points *= raidArrivalMode.pointsFactorCurve.Evaluate(points);
    
    // 레이드 전략 보정  
    if (raidStrategy.pointsFactorCurve != null)
        points *= raidStrategy.pointsFactorCurve.Evaluate(points);
    
    // 나이 제한 보정
    if (ageRestriction != null)
        points *= ageRestriction.threatPointsFactor;
    
    // 타일 레이어 보정
    if (target.Tile.Valid)
        points *= target.Tile.LayerDef.raidPointsFactor;
    
    // 최소 포인트 보장
    points = Mathf.Max(points, raidStrategy.Worker.MinimumPoints(faction, groupKind) * 1.05f);
    
    return points;
}
```

### 2.2 FactionDef의 maxPawnCostPerTotalPointsCurve
각 FactionDef는 `maxPawnCostPerTotalPointsCurve`를 통해 개별 파운의 최대 비용을 제한합니다:

```xml
<!-- Ratkin FactionDef 예시 -->
<maxPawnCostPerTotalPointsCurve>
    <points>
        <li>(0,35)</li>
        <li>(70, 50)</li>
        <li>(700, 100)</li>
        <li>(1300, 150)</li>
        <li>(100000, 10000)</li>
    </points>
</maxPawnCostPerTotalPointsCurve>
```

이는 레이드 포인트에 따라 개별 파운의 최대 비용을 제한하여, 너무 강력한 단일 유닛이 등장하는 것을 방지합니다.

## 3. pawnGroupMaker 개체 선택 방식

### 3.1 pawnGroupMaker 선택 과정

#### 3.1.1 기본 선택 로직
```csharp
// PawnGroupMakerUtility.cs:96-119
public static bool TryGetRandomPawnGroupMaker(PawnGroupMakerParms parms, out PawnGroupMaker pawnGroupMaker, bool ignoreCommonality = false)
{
    IEnumerable<PawnGroupMaker> source = from gm in parms.faction.def.pawnGroupMakers
    where gm.kindDef == parms.groupKind && gm.CanGenerateFrom(parms)
    select gm;
    
    if (ignoreCommonality)
        result = source.TryRandomElement(out pawnGroupMaker);
    else
        result = source.TryRandomElementByWeight((PawnGroupMaker gm) => gm.commonality, out pawnGroupMaker);
}
```

#### 3.1.2 CanGenerateFrom 조건 검사
```csharp
// PawnGroupMaker.cs:40-63
public bool CanGenerateFrom(PawnGroupMakerParms parms)
{
    // 1. 포인트 한계 검사
    if (parms.points > this.maxTotalPoints)
        return false;
    
    // 2. 금지된 전략 검사
    if (this.disallowedStrategies != null && this.disallowedStrategies.Contains(parms.raidStrategy))
        return false;
    
    // 3. 최소 포인트 검사
    if (parms.points < this.MinPointsToGenerateAnything(parms.faction.def, parms))
        return false;
    
    // 4. 레이드 전략 호환성 검사
    // 5. PawnGroupKindDef Worker 검사
    return this.kindDef.Worker.CanGenerateFrom(parms, this);
}
```

### 3.2 개체 선택 알고리즘

#### 3.2.1 포인트 기반 선택
```csharp
// PawnGroupMakerUtility.cs:234-295
public static IEnumerable<PawnGenOptionWithXenotype> ChoosePawnGenOptionsByPoints(float pointsTotal, List<PawnGenOption> options, PawnGroupMakerParms groupParms)
{
    float num = pointsTotal;
    float highestCost = -1f;
    
    for (;;)
    {
        // 1. 사용 가능한 옵션들 수집
        foreach (PawnGenOptionWithXenotype item in GetOptions(groupParms, groupParms.faction.def, options, pointsTotal, num, null, chosenOptions, leaderChosen))
        {
            if (item.Cost <= num)
            {
                if (item.Cost > highestCost)
                    highestCost = item.Cost;
                list.Add(item);
            }
        }
        
        // 2. 가중치 기반 선택
        Func<PawnGenOptionWithXenotype, float> weightSelector = delegate(PawnGenOptionWithXenotype gr)
        {
            if (!PawnGenOptionValid(gr.Option, groupParms, chosenOptions))
                return 0f;
            return gr.SelectionWeight * PawnWeightFactorByMostExpensivePawnCostFractionCurve.Evaluate(gr.Cost / highestCost);
        };
        
        // 3. 랜덤 선택
        if (!list.TryRandomElementByWeight(weightSelector, out item2))
            break;
        
        // 4. 포인트 차감 및 반복
        chosenOptions.Add(item2);
        num -= item2.Cost;
    }
}
```

#### 3.2.2 가중치 계산 시스템
개체 선택 시 다음 가중치들이 적용됩니다:

1. **selectionWeight**: PawnKindDef에서 정의된 기본 가중치
2. **PawnWeightFactorByMostExpensivePawnCostFractionCurve**: 가장 비싼 유닛 대비 비용 비율에 따른 가중치
   - 20% 이하: 0.01 (매우 낮음)
   - 30%: 0.3 (낮음)  
   - 50% 이상: 1.0 (정상)

### 3.3 Ratkin FactionDef의 pawnGroupMaker 구조

```xml
<pawnGroupMakers>
    <!-- 초반 기본 전투 -->
    <li>
        <kindDef>Combat</kindDef>
        <commonality>100</commonality>
        <maxTotalPoints>1000</maxTotalPoints>
        <options>
            <RatkinCombatant>20</RatkinCombatant>
            <RatkinSoldier>5</RatkinSoldier>
        </options>
    </li>
    
    <!-- 초반 혼합 전투 -->
    <li>
        <kindDef>Combat</kindDef>
        <commonality>100</commonality>
        <maxTotalPoints>1000</maxTotalPoints>
        <options>
            <RatkinDefender>5</RatkinDefender>
            <RatkinVanguard>10</RatkinVanguard>
            <RatkinCombatant>15</RatkinCombatant>
        </options>
    </li>
    
    <!-- 중후반 군인 -->
    <li>
        <kindDef>Combat</kindDef>
        <commonality>50</commonality>
        <options>
            <RatkinSoldier>30</RatkinSoldier>
            <RatkinEliteSoldier>5</RatkinEliteSoldier>
            <RatkinDemonMan>5</RatkinDemonMan>
            <RatkinDefender>15</RatkinDefender>
            <RatkinEliteGuardener>15</RatkinEliteGuardener>
        </options>
    </li>
    
    <!-- 기타 전투 조합들... -->
</pawnGroupMakers>
```

## 4. 선택 과정 요약

### 4.1 전체 워크플로우
1. **기지 가치 계산** → 레이드 포인트 결정
2. **레이드 포인트 조정** → 전략/도착방식/나이제한 등 적용
3. **Faction 선택** → RaidCommonalityFromPoints 기반 가중치 선택
4. **pawnGroupMaker 선택** → commonality 기반 가중치 선택
5. **개체 선택** → 포인트 기반 반복 선택 알고리즘

### 4.2 핵심 제약 조건
- **maxTotalPoints**: pawnGroupMaker별 최대 포인트 한계
- **maxPawnCostPerTotalPointsCurve**: 개별 파운 최대 비용 제한
- **disallowedStrategies**: 특정 전략에서 제외되는 pawnGroupMaker
- **commonality**: pawnGroupMaker 선택 확률 가중치

### 4.3 밸런싱 요소
- **초반 vs 후반**: maxTotalPoints로 단계별 제한
- **다양성**: 여러 pawnGroupMaker 조합으로 변화 제공
- **난이도 조절**: 가중치 시스템으로 확률적 선택
- **전략별 차별화**: disallowedStrategies로 전략별 특화

## 결론

FactionDef의 pawnGroupMaker 시스템은 복잡하지만 체계적인 다층 선택 구조를 가지고 있습니다. 기지 가치에서 시작하여 최종 개체 선택까지 여러 단계의 필터링과 가중치 시스템을 통해 게임의 밸런스를 유지하면서도 다양한 조합을 제공합니다. 이 시스템은 모드 개발자들이 새로운 종족이나 팩션을 설계할 때 참고할 수 있는 중요한 설계 패턴을 제공합니다.
