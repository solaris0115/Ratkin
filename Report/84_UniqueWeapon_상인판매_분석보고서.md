# UniqueWeapon 상인 판매 분석 보고서

## 태그
UniqueWeapon CompUniqueWeapon 상인 판매 StockGenerator ThingSetMaker

---

## 개요

83 보고서를 참고하여 유니크 무기 생성 로직을 분석하고, 상인이 판매할 수 있도록 하기 위한 요구사항을 정리합니다.

---

## 1. 유니크 무기 생성 메커니즘

### 1.1 ThingSetMaker_UniqueWeapon 동작 방식

**파일**: `RimworldSource/RimWorld/ThingSetMaker_UniqueWeapon.cs`

```csharp
// ThingSetMaker_UniqueWeapon.Generate()
Thing thing = ThingMaker.MakeThing(
    (from x in DefDatabase<ThingDef>.AllDefs
     where x.HasComp<CompUniqueWeapon>()  // 핵심 조건
     select x).RandomElement<ThingDef>(), 
    null
);
```

**선택 조건**:
- `HasComp<CompUniqueWeapon>()`이 `true`인 ThingDef만 선택
- Odyssey DLC가 활성화되어 있어야 함 (`ModsConfig.OdysseyActive`)

### 1.2 CompUniqueWeapon 컴포넌트

**필수 컴포넌트**: `CompProperties_UniqueWeapon`

**파일**: `RimworldSource/RimWorld/CompProperties_UniqueWeapon.cs`

```csharp
public class CompProperties_UniqueWeapon : CompProperties
{
    public List<WeaponCategoryDef> weaponCategories = new List<WeaponCategoryDef>();
    [MustTranslate]
    public List<string> namerLabels = new List<string>();
}
```

**필수 속성**:
- `weaponCategories`: 무기 카테고리 목록 (WeaponTraitDef와 매칭용)
- `namerLabels`: 이름 생성 시 사용할 라벨 목록

---

## 2. 유니크 무기 ThingDef 요구사항

### 2.1 필수 컴포넌트

```xml
<comps Inherit="False">
    <!-- CompUniqueWeapon 필수 -->
    <li Class="CompProperties_UniqueWeapon">
        <weaponCategories>
            <li>Ranged</li>
            <li>BulletFiring</li>
            <li>Gun</li>
            <!-- 무기 타입에 맞는 카테고리 추가 -->
        </weaponCategories>
        <namerLabels>
            <li>rifle</li>
            <li>gun</li>
            <!-- 이름 생성에 사용될 라벨들 -->
        </namerLabels>
    </li>
    
    <!-- CompQuality (품질 시스템) -->
    <li>
        <compClass>CompQuality</compClass>
    </li>
    
    <!-- CompEquippableAbilityReloadable (원거리 무기인 경우) -->
    <li Class="CompProperties_EquippableAbilityReloadable" />
    
    <!-- 기타 필수 컴포넌트들 -->
    <li Class="CompProperties_Forbiddable"/>
    <li Class="CompProperties_Styleable"/>
    <li Class="CompProperties_Biocodable"/>  <!-- 원거리 무기인 경우 -->
</comps>
```

### 2.2 상인 판매를 위한 필수 속성

```xml
<!-- 거래 가능 설정 -->
<tradeability>Sellable</tradeability>
<!-- 또는 -->
<tradeability>All</tradeability>

<!-- 플레이어가 획득 가능해야 함 -->
<!-- (기본값이 true이지만 명시적으로 확인 필요) -->

<!-- ThingSetMaker 태그 (선택사항) -->
<thingSetMakerTags Inherit="False">
    <li>UniqueWeapon</li>
</thingSetMakerTags>

<!-- 제작 불가 설정 (유니크 무기는 제작 불가) -->
<recipeMaker Inherit="False" IsNull="True" />

<!-- 생성 확률 0 (자동 생성 방지) -->
<generateAllowChance>0</generateAllowChance>
<possessionCount>0</possessionCount>
```

### 2.3 Odyssey 유니크 무기 예시

**파일**: `RimworldData/Odyssey/Defs/ThingDefs_Items/Weapons_Unique.xml`

**주요 특징**:
- 모든 유니크 무기가 `<tradeability>Sellable</tradeability>` 설정
- `CompProperties_UniqueWeapon` 컴포넌트 포함
- `CompQuality` 컴포넌트 포함
- `recipeMaker`가 `IsNull="True"`로 설정 (제작 불가)

---

## 3. 상인 판매 구현 방법

### 3.1 방법 1: ThingSetMaker_UniqueWeapon 사용

**TraderKindDef에 추가**:

```xml
<TraderKindDef>
    <defName>RK_TraderKind_UniqueWeapons</defName>
    <label>unique weapons trader</label>
    <stockGenerators>
        <li Class="ThingSetMaker_UniqueWeapon">
            <countRange>1~2</countRange>
            <totalMarketValueRange>5000~15000</totalMarketValueRange>
        </li>
    </stockGenerators>
</TraderKindDef>
```

**장점**:
- RimWorld 기본 시스템 활용
- 자동으로 유니크 무기 생성 및 이름 부여
- 무기 트레이트 자동 적용

**단점**:
- Odyssey DLC 필수
- 모든 `CompUniqueWeapon`을 가진 무기 중 랜덤 선택

### 3.2 방법 2: Custom StockGenerator 구현

**예시**: `StockGenerator_Relic.cs` 참고

```csharp
public class StockGenerator_UniqueWeapon : StockGenerator
{
    public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
    {
        if (!ModsConfig.OdysseyActive)
        {
            yield break;
        }

        // CompUniqueWeapon을 가진 ThingDef 찾기
        List<ThingDef> candidates = DefDatabase<ThingDef>.AllDefs
            .Where(def => def.HasComp<CompUniqueWeapon>() 
                       && def.tradeability.TraderCanSell() 
                       && def.PlayerAcquirable)
            .ToList();

        if (candidates.Count == 0)
        {
            yield break;
        }

        // 유니크 무기 생성
        ThingDef chosenDef = candidates.RandomElement();
        Thing uniqueWeapon = ThingMaker.MakeThing(chosenDef, null);
        
        yield return uniqueWeapon;
    }

    public override bool HandlesThingDef(ThingDef thingDef)
    {
        return thingDef.HasComp<CompUniqueWeapon>();
    }
}
```

**장점**:
- 더 세밀한 제어 가능
- 특정 무기만 필터링 가능
- 추가 조건 추가 가능

**단점**:
- 직접 구현 필요
- ThingSetMaker_UniqueWeapon의 로직을 재구현해야 함

### 3.3 방법 3: StockGenerator_Tag 활용

**특정 태그를 가진 무기만 선택**:

```xml
<TraderKindDef>
    <stockGenerators>
        <li Class="StockGenerator_Tag">
            <tradeTag>UniqueWeapon</tradeTag>
            <countRange>1~2</countRange>
        </li>
    </stockGenerators>
</TraderKindDef>
```

**ThingDef에 태그 추가**:

```xml
<ThingDef>
    <tradeTags>
        <li>UniqueWeapon</li>
    </tradeTags>
</ThingDef>
```

**주의사항**:
- `StockGenerator_Tag`는 일반 무기도 선택할 수 있음
- `CompUniqueWeapon` 체크가 없으므로 필터링 필요

---

## 4. 상인 판매 필수 조건 요약

### 4.1 ThingDef 필수 조건

1. **CompUniqueWeapon 컴포넌트**
   ```xml
   <li Class="CompProperties_UniqueWeapon">
       <weaponCategories>...</weaponCategories>
       <namerLabels>...</namerLabels>
   </li>
   ```

2. **거래 가능 설정**
   ```xml
   <tradeability>Sellable</tradeability>
   <!-- 또는 -->
   <tradeability>All</tradeability>
   ```

3. **플레이어 획득 가능**
   - `PlayerAcquirable` 속성 (기본값 true, 명시적 확인 권장)

4. **CompQuality 컴포넌트** (권장)
   - 유니크 무기는 Super 품질로 생성됨

### 4.2 StockGenerator 필수 조건

1. **TraderCanSell() 체크**
   ```csharp
   if (!def.tradeability.TraderCanSell() || !def.PlayerAcquirable)
   {
       continue;
   }
   ```

2. **Odyssey DLC 활성화 체크**
   ```csharp
   if (!ModsConfig.OdysseyActive)
   {
       yield break;
   }
   ```

---

## 5. 구현 예시

### 5.1 ThingDef 예시

```xml
<ThingDef ParentName="RK_Rifle">
    <defName>RK_Rifle_Unique</defName>
    <label>unique ratkin rifle</label>
    <description>A ratkin rifle with customized parts.</description>
    
    <comps Inherit="False">
        <li Class="CompProperties_EquippableAbilityReloadable" />
        <li Class="CompProperties_Forbiddable"/>
        <li Class="CompProperties_Styleable"/>
        <li Class="CompProperties_Biocodable"/>
        <li>
            <compClass>CompQuality</compClass>
        </li>
        <li Class="CompProperties_Art">
            <nameMaker>NamerArtWeaponGun</nameMaker>
            <descriptionMaker>ArtDescription_WeaponGun</descriptionMaker>
            <minQualityForArtistic>Excellent</minQualityForArtistic>
        </li>
        <li Class="CompProperties_UniqueWeapon">
            <weaponCategories>
                <li>Ranged</li>
                <li>BulletFiring</li>
                <li>Gun</li>
                <li>Rifle</li>
                <li>Sighted</li>
            </weaponCategories>
            <namerLabels>
                <li>ratkin rifle</li>
                <li>rifle</li>
                <li>gun</li>
            </namerLabels>
        </li>
    </comps>
    
    <generateAllowChance>0</generateAllowChance>
    <possessionCount>0</possessionCount>
    <tradeability>Sellable</tradeability>
    <thingSetMakerTags Inherit="False">
        <li>UniqueWeapon</li>
    </thingSetMakerTags>
    <recipeMaker Inherit="False" IsNull="True" />
</ThingDef>
```

### 5.2 TraderKindDef 예시

```xml
<TraderKindDef>
    <defName>RK_TraderKind_UniqueWeapons</defName>
    <label>unique weapons trader</label>
    <stockGenerators>
        <li Class="ThingSetMaker_UniqueWeapon">
            <countRange>1~2</countRange>
            <totalMarketValueRange>5000~15000</totalMarketValueRange>
        </li>
    </stockGenerators>
</TraderKindDef>
```

---

## 6. 참고사항

### 6.1 CompUniqueWeapon 동작 흐름

1. **생성 시점**: `ThingMaker.MakeThing()` 호출 시
2. **초기화**: `CompUniqueWeapon.PostPostMake()` 실행
3. **트레이트 생성**: 1~3개의 WeaponTraitDef 랜덤 선택
4. **품질 설정**: Super 등급으로 고정
5. **이름 생성**: `NamerUniqueWeapon` RulePack 사용
6. **색상 설정**: 무기 색상 랜덤 선택

### 6.2 WeaponCategoryDef 매칭

- `CompProperties_UniqueWeapon.weaponCategories`에 포함된 카테고리만 WeaponTraitDef와 매칭 가능
- 예: `Ranged`, `BulletFiring`, `Gun`, `Rifle` 등

### 6.3 이름 생성

- `namerLabels`에서 랜덤 선택된 라벨이 이름 생성에 사용됨
- 무기 트레이트, 색상, 랜덤 Pawn 이름 등이 조합됨

---

## 7. 결론

### 7.1 상인이 유니크 무기를 판매하려면:

1. **ThingDef에 CompUniqueWeapon 컴포넌트 추가**
   - `weaponCategories` 설정
   - `namerLabels` 설정

2. **거래 가능 설정**
   - `<tradeability>Sellable</tradeability>` 또는 `All`

3. **TraderKindDef에 StockGenerator 추가**
   - `ThingSetMaker_UniqueWeapon` 사용 (권장)
   - 또는 Custom StockGenerator 구현

4. **Odyssey DLC 활성화 확인**
   - `ModsConfig.OdysseyActive` 체크

### 7.2 핵심 포인트

- **CompUniqueWeapon 컴포넌트가 핵심**: 이 컴포넌트가 없으면 유니크 무기로 인식되지 않음
- **tradeability 설정 필수**: `Sellable` 또는 `All`이어야 상인이 판매 가능
- **ThingSetMaker_UniqueWeapon 활용**: RimWorld 기본 시스템을 사용하면 자동으로 모든 처리가 됨

---

## 참고 파일

- `Report/83_NamerUniqueWeapon_사용처_플로우_분석보고서.md` - NamerUniqueWeapon 플로우 분석
- `RimworldSource/RimWorld/CompUniqueWeapon.cs` - 유니크 무기 컴포넌트
- `RimworldSource/RimWorld/ThingSetMaker_UniqueWeapon.cs` - 유니크 무기 생성기
- `RimworldData/Odyssey/Defs/ThingDefs_Items/Weapons_Unique.xml` - Odyssey 유니크 무기 예시
- `Project/1.6/Source/ShieldOfRatkinia/StockGenerator_Relic.cs` - Custom StockGenerator 예시
