# Relic Generation Flow Report
# 유물 생성 플로우 보고서

## 개요

림월드에서 장비(무기, 방어구 등)가 "유물"(Relic)로 생성될 수 있는지와 생성 플로우에 대한 간결한 정리 보고서입니다.

## 결론: 유물 생성 가능 여부

**가능합니다.** 림월드의 장비는 유물로 생성될 수 있으며, 이미 프로젝트에 구현되어 있습니다.

## 유물 생성 플로우

### 1. 생성 요청 지점

#### 1.1 상인 재고 생성 (StockGenerator)
- **위치**: `TraderKindDef.stockGenerators` → `StockGenerator.GenerateThings()`
- **호출 시점**: 상인 방문 시 재고 생성
- **구현**: `Project/1.6/Source/ShieldOfRatkinia/StockGenerator_Relic.cs`

#### 1.2 퀘스트 보상 생성
- **위치**: `QuestNode_Root_RelicHunt`, `QuestNode_Root_ReliquaryPilgrims` 등
- **호출 시점**: 퀘스트 보상으로 유물 생성
- **예시**: `QuestNode_Root_RelicHunt.cs:92` - `precept_Relic.GenerateRelic()`

#### 1.3 고대 복합체/사이트 생성
- **위치**: `SitePart`, `QuestNode_Root_Mission_AncientComplex` 등
- **호출 시점**: 고대 유적에서 유물 발견

### 2. 핵심 생성 메서드

#### 2.1 `Precept_Relic.GenerateRelic()`
**위치**: `RimworldSource/RimWorld/Precept_Relic.cs:138-159`

```csharp
public Thing GenerateRelic()
{
    // 1. Stuff 확인 및 설정 (MadeFromStuff인 경우)
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
    
    // 3. 유물 속성 설정 (핵심)
    thing.StyleSourcePrecept = this;  // Precept_Relic 연결
    this.relicGenerated = true;
    this.generatedRelic = thing;
    
    return thing;
}
```

**핵심 포인트**:
- `thing.StyleSourcePrecept = this` 설정이 유물로 인식되는 핵심
- 품질 시스템이 있으면 자동으로 `Legendary` 품질로 생성
- `MadeFromStuff`인 경우 Stuff 자동 선택

### 3. StockGenerator에서의 유물 생성 플로우

#### 3.1 전체 플로우

```
TraderKindDef.stockGenerators
    ↓
StockGenerator_Relic.GenerateThings()
    ↓
1. Ideology DLC 체크 (ModsConfig.IdeologyActive)
    ↓
2. ThingDef 후보 수집 (tradeTag/weaponTag/apparelTag 기반)
    ↓
3. ThingDef 랜덤 선택
    ↓
4. 임시 Ideo 생성 (한 번만, static 변수로 캐싱)
    ↓
5. PreceptDef "Relic" 로드
    ↓
6. Precept_Relic 생성 및 설정
    - relicPrecept.ideo = tempIdeo
    - relicPrecept.ThingDef = chosenDef
    - relicPrecept.SetRandomStuff()
    ↓
7. relicPrecept.GenerateRelic() 호출
    ↓
8. 생성된 유물 반환
```

#### 3.2 구현 코드 위치

**파일**: `Project/1.6/Source/ShieldOfRatkinia/StockGenerator_Relic.cs`

**주요 단계**:
1. **후보 수집** (29-72줄): 태그 기반으로 ThingDef 필터링
2. **ThingDef 선택** (80줄): `candidates.RandomElement()`
3. **Ideo 생성** (83-102줄): `IdeoGenerator.GenerateIdeo()`로 임시 이데올로기 생성
4. **Precept_Relic 생성** (104-114줄):
   - `PreceptMaker.MakePrecept(relicPreceptDef)`로 Precept_Relic 생성
   - ThingDef 및 Stuff 설정
5. **유물 생성** (117줄): `relicPrecept.GenerateRelic()` 호출

### 4. 유물 식별 방법

#### 4.1 `ReliquaryUtility.IsRelic()`
**위치**: `RimworldSource/RimWorld/ReliquaryUtility.cs`

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

**핵심**: `Thing.StyleSourcePrecept`가 `Precept_Relic` 타입인지 확인

### 5. 생성 요구사항

#### 5.1 필수 조건
1. **Ideology DLC 활성화**: `ModsConfig.IdeologyActive == true`
2. **Precept_Relic 객체**: ThingDef와 Stuff가 설정된 Precept_Relic 필요
3. **ThingDef 요구사항**:
   - `ThingWithComps` 타입 (CompStyleable 컴포넌트 필요)
   - `tradeability.TraderCanSell() == true`
   - `PlayerAcquirable == true`

#### 5.2 Stuff 선택 로직
**위치**: `Precept_Relic.GenerateStuffFor()` (92-110줄)

- `MadeFromStuff`인 경우에만 Stuff 생성
- 같은 이데올로기에서 이미 사용된 Stuff 제외
- `BaseMarketValue`를 가중치로 랜덤 선택

### 6. 생성 플로우 다이어그램

```
[상인 방문]
    ↓
[TraderKindDef.stockGenerators 순회]
    ↓
[StockGenerator_Relic.GenerateThings() 호출]
    ↓
┌─────────────────────────────────────┐
│ 1. Ideology DLC 체크                │
│    ModsConfig.IdeologyActive?       │
└─────────────────────────────────────┘
    ↓ (true)
┌─────────────────────────────────────┐
│ 2. ThingDef 후보 수집               │
│    - tradeTag/weaponTag/apparelTag  │
│    - tradeability 체크              │
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│ 3. ThingDef 랜덤 선택              │
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│ 4. 임시 Ideo 생성 (캐싱)           │
│    IdeoGenerator.GenerateIdeo()    │
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│ 5. Precept_Relic 생성              │
│    - PreceptDef "Relic" 로드        │
│    - PreceptMaker.MakePrecept()     │
│    - ThingDef 설정                  │
│    - SetRandomStuff()               │
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│ 6. GenerateRelic() 호출            │
│    - Stuff 확인/설정                │
│    - Thing 생성 (Legendary 품질)    │
│    - StyleSourcePrecept 설정        │
└─────────────────────────────────────┘
    ↓
[유물 반환]
```

## 요약

### 생성 가능 여부
✅ **가능**: 림월드의 장비는 유물로 생성 가능

### 생성 요청 지점
1. **상인 재고**: `StockGenerator_Relic.GenerateThings()`
2. **퀘스트 보상**: `QuestNode_Root_RelicHunt` 등
3. **고대 유적**: `SitePart`, `QuestNode_Root_Mission_AncientComplex` 등

### 핵심 생성 메서드
- `Precept_Relic.GenerateRelic()`: 유물 생성의 핵심 메서드
- `thing.StyleSourcePrecept = this`: 유물로 인식되는 핵심 속성 설정

### 필수 조건
- Ideology DLC 활성화 필수
- Precept_Relic 객체 필요
- ThingDef는 ThingWithComps 타입이어야 함

## 참고 파일

- **구현 코드**: `Project/1.6/Source/ShieldOfRatkinia/StockGenerator_Relic.cs`
- **핵심 클래스**: `RimworldSource/RimWorld/Precept_Relic.cs`
- **유틸리티**: `RimworldSource/RimWorld/ReliquaryUtility.cs`
- **상세 분석**: `Report/61_Relic_Generation_System_Analysis_Report.md`
