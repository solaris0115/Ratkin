# CataphractArmor 테크프린트 구현 분석 보고서

## 개요

이 보고서는 RimWorld Royalty DLC의 `CataphractArmor` 연구와 관련된 테크프린트 시스템의 구현 방식을 분석하고, 새로운 아이템에 동일한 시스템을 적용하는 방법을 제시합니다.

## 1. CataphractArmor 제작을 위한 연구 연결

### 1.1 ThingDef의 recipeMaker 설정

CataphractArmor를 제작하기 위해서는 해당 ThingDef의 `recipeMaker` 섹션에 `researchPrerequisite`가 설정되어 있어야 합니다.

**위치**: `RimworldData/Royalty/Defs/ThingDefs_Misc/Apparel_Various.xml`

```xml
<ThingDef Name="ApparelArmorCataphractBase" ParentName="ArmorMachineableBase" Abstract="True">
  <techLevel>Spacer</techLevel>
  <recipeMaker>
    <unfinishedThingDef>UnfinishedTechArmor</unfinishedThingDef>
    <researchPrerequisite>CataphractArmor</researchPrerequisite>
    <skillRequirements>
      <Crafting>8</Crafting>
    </skillRequirements>
    <recipeUsers Inherit="False">
      <li>FabricationBench</li>
    </recipeUsers>
  </recipeMaker>
  <!-- ... -->
</ThingDef>
```

**필수 요소**:
- `<researchPrerequisite>CataphractArmor</researchPrerequisite>`: 연구 프로젝트의 `defName`을 참조

### 1.2 ResearchProjectDef 정의

**위치**: `RimworldData/Royalty/Defs/ResearchProjectDefs/ResearchProjects_Apparel.xml`

```xml
<ResearchProjectDef>
  <defName>CataphractArmor</defName>
  <label>cataphract armor</label>
  <description>Craft cataphract armor, heavy powered armor that slows the user but which can absorb extreme punishment. Note that these also require advanced components.</description>
  <baseCost>6000</baseCost>
  <techLevel>Spacer</techLevel>
  <prerequisites>
    <li>PoweredArmor</li>
  </prerequisites>
  <requiredResearchBuilding>HiTechResearchBench</requiredResearchBuilding>
  <requiredResearchFacilities>
    <li>MultiAnalyzer</li>
  </requiredResearchFacilities>
  <researchViewX>19.00</researchViewX>
  <researchViewY>1.10</researchViewY>
  <techprintCount>2</techprintCount>
  <techprintCommonality>3</techprintCommonality>
  <techprintMarketValue>3000</techprintMarketValue>
  <heldByFactionCategoryTags>
    <li>Empire</li>
  </heldByFactionCategoryTags>
  <!-- ... -->
</ResearchProjectDef>
```

## 2. 테크프린트 시스템 분석

### 2.1 테크프린트란?

테크프린트(Techprint)는 특정 연구 프로젝트를 진행하기 위해 필요한 도면 아이템입니다. 연구를 완료하기 위해서는 지정된 수만큼의 테크프린트를 연구 작업대에 적용해야 합니다.

### 2.2 테크프린트 생성 메커니즘

테크프린트는 **동적으로 생성**되는 아이템입니다. `ResearchProjectDef`에 `techprintCount`가 0보다 큰 값으로 설정되어 있으면, 게임이 자동으로 해당 연구 프로젝트에 대한 테크프린트 ThingDef를 생성합니다.

**소스 코드 위치**: `RimworldSource/RimWorld/ThingDefGenerator_Techprints.cs`

```csharp
public static IEnumerable<ThingDef> ImpliedTechprintDefs(bool hotReload = false)
{
    foreach (ResearchProjectDef researchProjectDef in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
    {
        if (researchProjectDef.TechprintCount > 0)
        {
            string defName = "Techprint_" + researchProjectDef.defName;
            ThingDef thingDef = new ThingDef();
            // ... 테크프린트 ThingDef 설정 ...
            thingDef.comps.Add(new CompProperties_Techprint
            {
                project = researchProjectDef
            });
            // ...
        }
    }
}
```

**생성 규칙**:
- 테크프린트의 `defName`은 `"Techprint_" + ResearchProjectDef.defName` 형식으로 자동 생성됩니다.
- 예: `CataphractArmor` 연구 → `Techprint_CataphractArmor` 아이템 생성

### 2.3 ResearchProjectDef의 테크프린트 관련 필드

| 필드명 | 타입 | 설명 | CataphractArmor 예시 |
|--------|------|------|---------------------|
| `techprintCount` | int | 필요한 테크프린트 개수 | `2` |
| `techprintCommonality` | float | 테크프린트 생성 확률 가중치 (높을수록 더 자주 생성됨) | `3` |
| `techprintMarketValue` | float | 테크프린트의 시장 가치 | `3000` |
| `heldByFactionCategoryTags` | List<string> | 테크프린트를 보유할 수 있는 팩션 카테고리 태그 | `["Empire"]` |

### 2.4 테크프린트 컴포넌트

테크프린트 아이템은 `CompProperties_Techprint` 컴포넌트를 가지고 있으며, 이 컴포넌트는 특정 `ResearchProjectDef`를 참조합니다.

**소스 코드 위치**: `RimworldSource/RimWorld/CompProperties_Techprint.cs`

```csharp
public class CompProperties_Techprint : CompProperties
{
    public ResearchProjectDef project;
    // ...
}
```

## 3. 구현 방법

### 3.1 새로운 아이템에 테크프린트 시스템 적용하기

#### 단계 1: ResearchProjectDef 생성

`Project/1.6/Defs/ResearchDefs/ResearchProjects.xml` 또는 새로운 파일에 연구 프로젝트를 정의합니다.

```xml
<ResearchProjectDef>
  <defName>RK_MyAdvancedWeapon</defName>
  <label>advanced weapon</label>
  <description>고급 무기를 연구합니다.</description>
  <baseCost>4000</baseCost>
  <techLevel>Spacer</techLevel>
  <prerequisites>
    <li>MicroelectronicsBasics</li>
  </prerequisites>
  <requiredResearchBuilding>HiTechResearchBench</requiredResearchBuilding>
  <requiredResearchFacilities>
    <li>MultiAnalyzer</li>
  </requiredResearchFacilities>
  
  <!-- 테크프린트 설정 -->
  <techprintCount>2</techprintCount>
  <techprintCommonality>3</techprintCommonality>
  <techprintMarketValue>2500</techprintMarketValue>
  <heldByFactionCategoryTags>
    <li>Empire</li>
    <li>Outlander</li>
  </heldByFactionCategoryTags>
  
  <researchViewX>15.00</researchViewX>
  <researchViewY>2.00</researchViewY>
</ResearchProjectDef>
```

**필수 요소**:
- `techprintCount`: 필요한 테크프린트 개수 (0보다 큰 값)
- `techprintCommonality`: 테크프린트 생성 확률 (권장: 1~5)
- `techprintMarketValue`: 테크프린트 시장 가치
- `heldByFactionCategoryTags`: 테크프린트를 보유할 수 있는 팩션 카테고리

#### 단계 2: ThingDef의 recipeMaker에 연구 연결

`Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml` 또는 해당 아이템 파일에 `recipeMaker` 섹션을 추가합니다.

```xml
<ThingDef ParentName="RK_WeaponAttr_UltratechBase">
  <defName>RK_Weapon_MyAdvancedWeapon</defName>
  <label>advanced weapon</label>
  <!-- ... -->
  <recipeMaker>
    <researchPrerequisite>RK_MyAdvancedWeapon</researchPrerequisite>
    <skillRequirements>
      <Crafting>10</Crafting>
    </skillRequirements>
    <recipeUsers Inherit="False">
      <li>FabricationBench</li>
    </recipeUsers>
  </recipeMaker>
</ThingDef>
```

**필수 요소**:
- `<researchPrerequisite>RK_MyAdvancedWeapon</researchPrerequisite>`: ResearchProjectDef의 `defName`과 일치해야 함

### 3.2 테크프린트 생성 확인

게임이 시작되면 `techprintCount > 0`인 모든 `ResearchProjectDef`에 대해 자동으로 테크프린트 ThingDef가 생성됩니다.

**생성된 테크프린트 확인 방법**:
- 게임 내에서 상인(특히 Empire 팩션)의 거래 목록에서 확인
- 퀘스트 보상으로 제공될 수 있음
- 연구 화면에서 해당 연구 프로젝트에 "기술청사진 필요 (0 / 2)" 표시 확인

## 4. 테크프린트 시스템 동작 흐름

### 4.1 연구 진행 흐름

1. **연구 시작**: 플레이어가 연구 프로젝트를 선택하여 연구 시작
2. **테크프린트 필요 확인**: 게임이 `techprintCount`를 확인
3. **테크프린트 적용**: 플레이어가 테크프린트 아이템을 연구 작업대에 적용
   - 테크프린트 적용 시 남은 연구 진행률의 50%가 채워짐
   - 연구자의 지적 능력(Intellectual) 스킬 경험치 증가
4. **연구 완료**: 필요한 모든 테크프린트를 적용하고 연구를 완료하면 아이템 제작 가능

### 4.2 테크프린트 획득 방법

1. **상인 거래**: Empire, Outlander 등 지정된 팩션 카테고리의 상인에게서 구매
2. **퀘스트 보상**: 퀘스트 완료 시 보상으로 제공
3. **이벤트**: 특정 이벤트에서 획득 가능

### 4.3 테크프린트 생성 조건

**소스 코드 위치**: `RimworldSource/RimWorld/TechprintUtility.cs`

테크프린트가 생성되려면 다음 조건을 모두 만족해야 합니다:

1. `ResearchProjectDef.TechprintCount > 0`
2. 연구가 아직 완료되지 않음 (`!p.IsFinished`)
3. 필요한 테크프린트가 모두 적용되지 않음 (`!p.TechprintRequirementMet`)
4. 팩션 카테고리 태그가 일치 (`p.heldByFactionCategoryTags`에 해당 팩션의 `categoryTag` 포함)

## 5. CataphractArmor 예시 요약

### 5.1 연구 프로젝트 설정

```xml
<ResearchProjectDef>
  <defName>CataphractArmor</defName>
  <!-- 기본 연구 설정 -->
  <baseCost>6000</baseCost>
  <techLevel>Spacer</techLevel>
  <prerequisites>
    <li>PoweredArmor</li>
  </prerequisites>
  
  <!-- 테크프린트 설정 -->
  <techprintCount>2</techprintCount>           <!-- 2개 필요 -->
  <techprintCommonality>3</techprintCommonality> <!-- 생성 확률 가중치 -->
  <techprintMarketValue>3000</techprintMarketValue> <!-- 시장 가치 -->
  <heldByFactionCategoryTags>
    <li>Empire</li>  <!-- Empire 팩션에서만 획득 가능 -->
  </heldByFactionCategoryTags>
</ResearchProjectDef>
```

### 5.2 아이템 제작 설정

```xml
<ThingDef Name="ApparelArmorCataphractBase" ParentName="ArmorMachineableBase" Abstract="True">
  <recipeMaker>
    <researchPrerequisite>CataphractArmor</researchPrerequisite> <!-- 연구 연결 -->
    <skillRequirements>
      <Crafting>8</Crafting>
    </skillRequirements>
    <recipeUsers Inherit="False">
      <li>FabricationBench</li>
    </recipeUsers>
  </recipeMaker>
</ThingDef>
```

### 5.3 자동 생성되는 테크프린트

게임이 자동으로 다음 ThingDef를 생성합니다:

- **defName**: `Techprint_CataphractArmor`
- **label**: "기술청사진 (근위대 갑옷)"
- **description**: 연구 프로젝트 설명 + 해제되는 아이템 목록
- **MarketValue**: 3000
- **CompProperties_Techprint**: `project = CataphractArmor`

## 6. 주의사항

### 6.1 defName 일치

`recipeMaker`의 `researchPrerequisite`는 반드시 `ResearchProjectDef`의 `defName`과 정확히 일치해야 합니다.

### 6.2 테크프린트 개수

- `techprintCount`는 0보다 큰 정수여야 합니다.
- 일반적으로 1~3개가 사용됩니다.
- 너무 많으면 플레이어가 연구를 완료하기 어려워질 수 있습니다.

### 6.3 팩션 카테고리 태그

`heldByFactionCategoryTags`에 지정된 팩션 카테고리만 해당 테크프린트를 보유할 수 있습니다. 일반적으로:
- `Empire`: 제국 팩션 (Royalty DLC)
- `Outlander`: 외지인 팩션
- `Tribal`: 부족 팩션

### 6.4 연구 선행 조건

테크프린트가 생성되려면 연구의 선행 조건(`prerequisites`)이 완료되어 있어야 합니다. (`TechprintUtility.GetSelectionWeight` 참고)

## 7. 참고 자료

- **ResearchProjectDef 소스**: `RimworldSource/RimWorld/ResearchProjectDef.cs`
- **Techprint 생성기**: `RimworldSource/RimWorld/ThingDefGenerator_Techprints.cs`
- **Techprint 유틸리티**: `RimworldSource/RimWorld/TechprintUtility.cs`
- **Techprint 컴포넌트**: `RimworldSource/RimWorld/CompProperties_Techprint.cs`
- **CataphractArmor 연구**: `RimworldData/Royalty/Defs/ResearchProjectDefs/ResearchProjects_Apparel.xml`
- **CataphractArmor 아이템**: `RimworldData/Royalty/Defs/ThingDefs_Misc/Apparel_Various.xml`

## 8. 결론

CataphractArmor와 같은 고급 아이템을 제작하기 위해서는:

1. **ResearchProjectDef**에 `techprintCount`, `techprintCommonality`, `techprintMarketValue`, `heldByFactionCategoryTags` 설정
2. **ThingDef**의 `recipeMaker`에 `researchPrerequisite` 설정하여 연구 프로젝트와 연결
3. 게임이 자동으로 테크프린트 ThingDef를 생성하므로 별도의 ThingDef 정의 불필요

이 시스템을 통해 플레이어는 상인이나 퀘스트를 통해 테크프린트를 획득하고, 이를 연구 작업대에 적용하여 고급 기술을 해제할 수 있습니다.

