# Relic 생성 시스템 분석 보고서

## 개요

RimWorld에서 유물(Relic)을 생성하여 상인에게 판매하는 기능을 구현하기 위한 선행 조건과 생성 방식에 대한 분석 보고서입니다.

## 1. 유물(Relic) 시스템 개요

### 1.1 유물의 정의

- **유물(Relic)**: Ideology DLC에서 도입된 특수 아이템 시스템
- **식별 방법**: `Thing.StyleSourcePrecept`가 `Precept_Relic` 타입인지 확인
- **특징**: 특정 이데올로기와 연관된 고유한 아이템으로, 품질이 높고 특별한 가치를 가짐

### 1.2 유물 생성의 핵심 클래스

**Precept_Relic** (`RimworldSource/RimWorld/Precept_Relic.cs`)
- Ideology DLC의 Precept 시스템에서 유물을 관리하는 클래스
- `Precept_ThingStyle`을 상속받아 아이템 스타일과 연동

## 2. 유물 생성 선행 조건

### 2.1 필수 DLC

**Ideology DLC 필수**
- `ModsConfig.IdeologyActive`가 `true`여야 함
- 유물 시스템은 Ideology DLC 없이는 작동하지 않음

### 2.2 Precept_Relic 객체 필요

유물을 생성하려면 다음 조건을 만족하는 `Precept_Relic` 객체가 필요합니다:

1. **ThingDef 설정**: 유물로 만들 아이템의 `ThingDef`가 설정되어 있어야 함
2. **Stuff 설정**: 아이템이 `MadeFromStuff`인 경우, `stuff` 속성이 설정되어 있어야 함
3. **CanGenerateRelic**: `relicGenerated` 플래그가 `false`여야 함 (한 번만 생성 가능)

### 2.3 ThingDef 요구사항

유물로 만들 수 있는 아이템은 다음 조건을 만족해야 합니다:

- **ThingWithComps 타입**: `CompStyleable` 컴포넌트를 가질 수 있어야 함
- **일반 아이템**: 무기, 방어구, 장식품 등 대부분의 아이템 가능
- **제한사항**: 
  - 유물은 용해 불가 (`IsRelic()` 체크로 용해 방지)
  - 유물은 스택 불가 (`CanStackWith()` 체크로 스택 방지)

## 3. 유물 생성 방식

### 3.1 GenerateRelic() 메서드

**위치**: `Precept_Relic.GenerateRelic()`

**생성 과정**:

```csharp
public Thing GenerateRelic()
{
    // 1. Stuff 확인 및 설정
    if (this.stuff == null && base.ThingDef.MadeFromStuff)
    {
        this.stuff = Precept_Relic.GenerateStuffFor(base.ThingDef, this.ideo);
    }
    
    // 2. 아이템 생성
    Thing thing;
    if (base.ThingDef.CompDefFor<CompQuality>() != null)
    {
        // 품질 시스템이 있는 경우: Legendary 품질로 생성
        ThingStuffPairWithQuality pair = new ThingStuffPairWithQuality(
            base.ThingDef, 
            this.stuff, 
            QualityCategory.Legendary
        );
        thing = pair.MakeThing(true);
    }
    else
    {
        // 품질 시스템이 없는 경우: 일반 생성
        thing = ThingMaker.MakeThing(base.ThingDef, this.stuff);
    }
    
    // 3. 유물 속성 설정
    thing.StyleSourcePrecept = this;  // 핵심: Precept_Relic 연결
    this.relicGenerated = true;
    this.generatedRelic = thing;
    
    return thing;
}
```

### 3.2 Stuff 생성 로직

**위치**: `Precept_Relic.GenerateStuffFor()`

**Stuff 선택 규칙**:
1. 아이템이 `MadeFromStuff`인 경우에만 Stuff 생성
2. 같은 이데올로기에서 이미 사용된 Stuff는 제외
3. `BaseMarketValue`를 가중치로 사용하여 랜덤 선택 (높은 가치일수록 선택 확률 증가)

### 3.3 유물 식별 메서드

**위치**: `ReliquaryUtility.IsRelic()`

```csharp
public static bool IsRelic(this Thing thing)
{
    if (!ModsConfig.IdeologyActive)
        return false;
    
    ThingWithComps thingWithComps = thing as ThingWithComps;
    if (thingWithComps == null)
        return false;
    
    CompStyleable compStyleable = thingWithComps.compStyleable;
    Precept_ThingStyle precept = (compStyleable != null) 
        ? compStyleable.SourcePrecept 
        : null;
    
    return precept != null 
        && typeof(Precept_Relic).IsAssignableFrom(precept.GetType());
}
```

## 4. StockGenerator에서 유물 생성 구현 방안

### 4.1 StockGenerator 기본 구조

**위치**: `RimworldSource/RimWorld/StockGenerator.cs`

**필수 구현 메서드**:
- `GenerateThings(PlanetTile forTile, Faction faction = null)`: 아이템 생성
- `HandlesThingDef(ThingDef thingDef)`: 아이템 처리 여부 확인

### 4.2 StockGenerator_Relic 구현 전략

**방법 1: 기존 Precept_Relic 활용**
- 게임 내 존재하는 모든 이데올로기의 `Precept_Relic`을 수집
- `CanGenerateRelic == true`인 것들 중 랜덤 선택
- `GenerateRelic()` 호출하여 유물 생성

**방법 2: 임시 Precept_Relic 생성**
- 새로운 `Precept_Relic` 객체를 동적으로 생성
- ThingDef와 Stuff를 설정
- `GenerateRelic()` 호출

**방법 3: 직접 Thing 생성 후 속성 설정**
- `ThingMaker.MakeThing()`으로 아이템 생성
- 품질이 있으면 `Legendary`로 설정
- `StyleSourcePrecept`를 임시 `Precept_Relic`으로 설정

### 4.3 구현 시 주의사항

1. **Ideology DLC 체크**: `ModsConfig.IdeologyActive` 확인 필수
2. **Precept_Relic 생성**: Ideology 시스템과 연동 필요
3. **Stuff 선택**: `MadeFromStuff` 아이템의 경우 적절한 Stuff 선택
4. **품질 설정**: `CompQuality`가 있으면 `Legendary` 품질로 설정
5. **중복 생성 방지**: 같은 Precept_Relic은 한 번만 생성 가능

## 5. 구현 예시 코드 구조

### 5.1 StockGenerator_Relic 기본 구조

```csharp
public class StockGenerator_Relic : StockGenerator
{
    public List<ThingDef> allowedThingDefs;  // 유물로 만들 수 있는 ThingDef 목록
    public IntRange countRange = new IntRange(1, 1);
    
    public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
    {
        if (!ModsConfig.IdeologyActive)
            yield break;
        
        // Precept_Relic 수집 또는 생성 로직
        // GenerateRelic() 호출
        // yield return relic;
    }
    
    public override bool HandlesThingDef(ThingDef thingDef)
    {
        // 유물로 만들 수 있는 ThingDef인지 확인
        return false;  // 일반 거래 불가
    }
}
```

### 5.2 Precept_Relic 수집 방법

```csharp
// 방법 1: 모든 이데올로기에서 수집
var allRelics = Find.IdeoManager.IdeosInViewOrder
    .SelectMany(ideo => ideo.PreceptsListForReading
        .OfType<Precept_Relic>()
        .Where(relic => relic.CanGenerateRelic))
    .ToList();

// 방법 2: 특정 팩션의 이데올로기에서 수집
if (faction != null && faction.ideos != null)
{
    var factionRelics = faction.ideos.PrimaryIdeo
        .PreceptsListForReading
        .OfType<Precept_Relic>()
        .Where(relic => relic.CanGenerateRelic)
        .ToList();
}
```

## 6. 제약사항 및 고려사항

### 6.1 제약사항

1. **Ideology DLC 필수**: DLC가 없으면 유물 생성 불가
2. **이데올로기 의존성**: Precept_Relic은 특정 이데올로기에 속함
3. **중복 생성 제한**: 같은 Precept_Relic은 한 번만 생성 가능
4. **게임 상태 의존**: 이데올로기가 생성되어 있어야 함

### 6.2 대안 방안

**Ideology DLC 없이 유물처럼 보이는 아이템 생성**:
- `QualityCategory.Legendary` 품질로 생성
- 특별한 이름 부여
- 높은 가격 설정
- 단, 실제 유물 시스템의 이점(이데올로기 보너스 등)은 없음

## 7. 결론

### 7.1 핵심 요약

1. **유물 생성은 Ideology DLC 필수**
2. **Precept_Relic 객체가 필요하며, `GenerateRelic()` 메서드로 생성**
3. **생성된 아이템의 `StyleSourcePrecept`가 `Precept_Relic`으로 설정되어야 유물로 인식**
4. **품질 시스템이 있으면 자동으로 `Legendary` 품질로 생성**

### 7.2 구현 권장사항

1. **Ideology DLC 체크**: DLC 없을 때의 대안 제공
2. **다양한 이데올로기 지원**: 여러 이데올로기의 유물을 랜덤하게 생성
3. **ThingDef 필터링**: 유물로 만들 수 있는 아이템만 선택
4. **Stuff 자동 선택**: `MadeFromStuff` 아이템의 경우 적절한 Stuff 선택

### 7.3 참고 소스코드

- `RimworldSource/RimWorld/Precept_Relic.cs`: 유물 생성 핵심 로직
- `RimworldSource/RimWorld/ReliquaryUtility.cs`: 유물 유틸리티
- `RimworldSource/RimWorld/StockGenerator.cs`: StockGenerator 기본 클래스
- `RimworldSource/RimWorld/StockGeneratorUtility.cs`: StockGenerator 유틸리티

