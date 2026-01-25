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

## 무기 Relic의 부가 효과

Relic으로 생성된 무기는 여러 부가 효과를 받습니다. 이는 Relic이 자동으로 **Legendary 품질**로 생성되기 때문입니다.

### 1. 품질 기반 스탯 보너스

#### 1.1 근접 무기 (Melee Weapon)
**위치**: `RimworldData/Core/Defs/Stats/Stats_Weapons_Melee.xml`

- **MeleeWeapon_DamageMultiplier**: Legendary 품질 시 **1.65배** 피해량 증가
  - Awful: 0.8배
  - Poor: 0.9배
  - Normal: 1.0배
  - Good: 1.1배
  - Excellent: 1.2배
  - Masterwork: 1.45배
  - **Legendary: 1.65배** ⭐

**적용 메커니즘**:
- `StatPart_Quality` 컴포넌트를 통해 자동 적용
- 무기의 `MeleeWeapon_DamageMultiplier` 스탯에 품질 배율이 곱해짐

#### 1.2 원거리 무기 (Ranged Weapon)
**위치**: `RimworldData/Core/Defs/Stats/Stats_Weapons_Ranged.xml`

- **RangedWeapon_DamageMultiplier**: Legendary 품질 시 **1.5배** 피해량 증가
  - Awful: 0.9배
  - Poor: 1.0배
  - Normal: 1.0배
  - Good: 1.0배
  - Excellent: 1.0배
  - Masterwork: 1.25배
  - **Legendary: 1.5배** ⭐

**적용 메커니즘**:
- `StatPart_Quality` 컴포넌트를 통해 자동 적용
- 무기의 `RangedWeapon_DamageMultiplier` 스탯에 품질 배율이 곱해짐

### 2. 품질 보너스 적용 범위

`StatPart_Quality`는 다양한 스탯에 적용될 수 있습니다:

**적용 가능한 스탯 예시**:
- 무기 피해량 배율 (MeleeWeapon_DamageMultiplier, RangedWeapon_DamageMultiplier)
- 무기 정확도 (Accuracy 관련 스탯)
- 무기 쿨다운 (Cooldown 관련 스탯)
- 방어구 방어력 (ArmorRating 관련 스탯)
- 기타 `StatPart_Quality`가 포함된 모든 스탯

**코드 위치**: `RimworldSource/RimWorld/StatPart_Quality.cs`

```csharp
public override void TransformValue(StatRequest req, ref float val)
{
    if (val <= 0f && !this.applyToNegativeValues)
        return;
    
    float num = val * this.QualityMultiplier(req.QualityCategory) - val;
    num = Mathf.Min(num, this.MaxGain(req.QualityCategory));
    val += num;
}
```

### 3. 이데올로기 관련 효과

Relic 무기는 특정 이데올로기와 연관되어 있습니다:

- **이데올로기 식별**: `Precept_Relic.ideo` 속성으로 연결된 이데올로기 확인 가능
- **스타일링**: 이데올로기의 스타일 시스템과 연동되어 시각적 차별화 가능
- **이데올로기 특수 효과**: 일부 이데올로기 특성(Precept)이 Relic 무기에 추가 효과를 부여할 수 있음

### 4. 컴포넌트 기반 추가 효과

무기에 특정 컴포넌트가 있을 경우, Relic일 때 추가 효과를 받을 수 있습니다:

**예시**:
- `CompBladelinkWeapon`: Royalty DLC의 블레이드링크 무기 컴포넌트
- `CompEquippable`: 장착 시 특수 효과를 부여하는 컴포넌트
- 커스텀 컴포넌트: `IsRelic()` 체크를 통해 Relic일 때만 활성화되는 효과

**확인 방법**:
```csharp
if (weapon.IsRelic())
{
    // Relic일 때만 적용되는 특수 효과
}
```

### 5. 부가 효과 요약

| 효과 유형 | 적용 범위 | 보너스 수치 |
|---------|---------|-----------|
| **근접 무기 피해량** | MeleeWeapon_DamageMultiplier | **+65%** (1.65배) |
| **원거리 무기 피해량** | RangedWeapon_DamageMultiplier | **+50%** (1.5배) |
| **기타 품질 스탯** | StatPart_Quality가 포함된 모든 스탯 | 품질별 배율 적용 |
| **이데올로기 연동** | 이데올로기 시스템 | 이데올로기별 특수 효과 |
| **컴포넌트 효과** | 특정 컴포넌트 보유 시 | 컴포넌트별 특수 효과 |

### 6. 구현 시 고려사항

Relic 무기에 추가 효과를 구현하려면:

1. **품질 보너스 활용**: `StatPart_Quality`를 통해 자동으로 적용되므로 별도 구현 불필요
2. **커스텀 효과 추가**: `CompStyleable.SourcePrecept`가 `Precept_Relic`인지 확인하여 추가 효과 부여
3. **이데올로기 연동**: `Precept_Relic.ideo`를 통해 이데올로기별 특수 효과 구현 가능

**예시 코드**:
```csharp
// Relic 무기 확인
if (weapon.IsRelic())
{
    // Relic일 때만 적용되는 특수 효과
    // 예: 추가 능력 부여, 스탯 보너스 등
}

// 또는 CompStyleable을 통한 확인
CompStyleable compStyleable = weapon.TryGetComp<CompStyleable>();
if (compStyleable?.SourcePrecept is Precept_Relic relicPrecept)
{
    // Relic Precept에 접근하여 추가 효과 구현
}
```

## 참고 파일

- **구현 코드**: `Project/1.6/Source/ShieldOfRatkinia/StockGenerator_Relic.cs`
- **핵심 클래스**: `RimworldSource/RimWorld/Precept_Relic.cs`
- **유틸리티**: `RimworldSource/RimWorld/ReliquaryUtility.cs`
- **상세 분석**: `Report/61_Relic_Generation_System_Analysis_Report.md`
