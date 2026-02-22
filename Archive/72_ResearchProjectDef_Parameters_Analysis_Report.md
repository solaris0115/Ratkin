# ResearchProjectDef 파라미터 완전 분석 보고서

## 개요

이 문서는 RimWorld의 `ResearchProjectDef` 클래스에 정의된 모든 XML 파라미터를 소스코드 분석을 통해 상세히 정리한 참고 자료입니다.

**분석 기준**: `RimworldSource/Verse/ResearchProjectDef.cs` 소스코드 분석  
**작성일**: 2024년

---
<!-- 여기 -->
```xml
<ResearchProjectDef>
	<defName>RK_Research_Sample</defName>
	<label>sample research</label>
	<description>연구 설명입니다.</description>
	<tab>RK_ResearchTab_Default</tab>
	<baseCost>600</baseCost>
	<techLevel>Medieval</techLevel>
	<researchViewX>0</researchViewX>
	<researchViewY>1</researchViewY>
	<prerequisites>
		<li>RK_Research_Smithing</li>
		<li>RK_Research_Metallurgy</li>
	</prerequisites>
	<hiddenPrerequisites>   <!--UI에서 감추고 싶을때-->
		<li>RK_Research_Carpentry</li>
	</hiddenPrerequisites>
	<requiredResearchBuilding>HiTechResearchBench</requiredResearchBuilding>

	<requiredResearchFacilities>
		<li>MultiAnalyzer</li>
	</requiredResearchFacilities>
	<tags>
		<li>RK_ResearchTag_Default</li>
	</tags>
	<discoveredLetterTitle>연구 완료: {0}</discoveredLetterTitle>
	<discoveredLetterText>연구가 완료되었습니다.</discoveredLetterText>
	<discoveredLetterDisabledWhen>
		<difficulty>Peaceful</difficulty>
	</discoveredLetterDisabledWhen>
	<techprintCount>1</techprintCount>
	<techprintCommonality>1.0</techprintCommonality>
	<techprintMarketValue>1000</techprintMarketValue>
	<heldByFactionCategoryTags>
		<li>Empire</li>
	</heldByFactionCategoryTags>
	<requiresMechanitor>false</requiresMechanitor>
	<requiredAnalyzed>
		<li>MechanoidRemains</li>
	</requiredAnalyzed>
	<recalculatePower>false</recalculatePower>
	<knowledgeCategory>RK_KnowledgeCategory</knowledgeCategory>
	<knowledgeCost>100</knowledgeCost>
	<teachConcept>RK_Concept</teachConcept>
	<requireGravEngineInspected>false</requireGravEngineInspected>
	<customUnlockTexts>
		<li>언락 텍스트 1</li>
		<li>언락 텍스트 2</li>
	</customUnlockTexts>
</ResearchProjectDef>
```

---
## 1. 기본 필드 (Def에서 상속)

### 1.1 defName
- **타입**: `string` (Def 기본 필드)
- **필수**: 예
- **설명**: 연구 프로젝트의 고유 식별자
- **예시**: `RK_Research_Smithing`

### 1.2 label
- **타입**: `string` (Def 기본 필드)
- **필수**: 예
- **설명**: 연구 프로젝트의 표시 이름
- **예시**: `단조`

### 1.3 description
- **타입**: `string` (Def 기본 필드)
- **필수**: 아니오
- **설명**: 연구 프로젝트의 설명 텍스트
- **예시**: `금속을 가공하여 무기와 도구를 만드는 기술을 연구합니다.`

---

## 2. 연구 기본 설정

### 2.1 tab
- **타입**: `ResearchTabDef`
- **필수**: 아니오 (기본값: `ResearchTabDefOf.Main`)
- **설명**: 연구가 표시될 연구 탭
- **ResolveReferences**: `tab`이 null이면 자동으로 `ResearchTabDefOf.Main`으로 설정됨
- **예시**: `<tab>RK_ResearchTab_Default</tab>`

### 2.2 baseCost
- **타입**: `float`
- **필수**: 아니오 (기본값: 0)
- **설명**: 연구 완료에 필요한 기본 비용 (연구 포인트)
- **주의사항**: `knowledgeCost`와 동시 사용 불가 (ConfigErrors에서 검증)
- **예시**: `<baseCost>600</baseCost>`

### 2.3 techLevel
- **타입**: `TechLevel` (enum)
- **필수**: 아니오 (단, `knowledgeCategory`가 없으면 `Undefined` 불가)
- **설명**: 연구의 기술 수준
- **가능한 값**: `Undefined`, `Neolithic`, `Medieval`, `Industrial`, `Spacer`, `Ultra`, `Archotech`
- **비용 계산**: 연구자의 기술 수준이 낮으면 비용이 증가 (레벨당 0.5배 증가)
- **예시**: `<techLevel>Medieval</techLevel>`

---

## 5. 필수 건물 및 시설

### 5.1 requiredResearchBuilding
- **타입**: `ThingDef`
- **필수**: 아니오
- **설명**: 연구를 수행하기 위해 필요한 특정 연구 벤치
- **검증 로직**: `CanBeResearchedAt()` 메서드에서 벤치 타입 확인
- **예시**: `<requiredResearchBuilding>HiTechResearchBench</requiredResearchBuilding>`

### 5.2 requiredResearchFacilities
- **타입**: `List<ThingDef>`
- **필수**: 아니오
- **설명**: 연구 벤치에 연결되어 있어야 하는 필수 시설 목록
- **검증 로직**: `CanBeResearchedAt()` 메서드에서 `CompAffectedByFacilities`를 통해 연결된 시설 확인
- **예시**:
```xml
<requiredResearchFacilities>
    <li>MultiAnalyzer</li>
</requiredResearchFacilities>
```

---

## 6. 연구 태그

### 6.1 tags
- **타입**: `List<ResearchProjectTagDef>`
- **필수**: 아니오
- **설명**: 연구 프로젝트에 부여할 태그 목록
- **용도**: 연구를 분류하거나 필터링하는 데 사용
- **예시**:
```xml
<tags>
    <li>RK_ResearchTag_Default</li>
</tags>
```

---

## 7. 발견 편지 (Discovery Letter)

### 7.1 discoveredLetterTitle
- **타입**: `string` (`[MustTranslate]`)
- **필수**: 아니오
- **설명**: 연구 완료 시 표시될 편지의 제목
- **번역**: 번역 가능한 문자열
- **예시**: `<discoveredLetterTitle>연구 완료: {0}</discoveredLetterTitle>`

### 7.2 discoveredLetterText
- **타입**: `string` (`[MustTranslate]`)
- **필수**: 아니오
- **설명**: 연구 완료 시 표시될 편지의 내용
- **번역**: 번역 가능한 문자열
- **예시**: `<discoveredLetterText>연구가 완료되었습니다.</discoveredLetterText>`

### 7.3 discoveredLetterDisabledWhen
- **타입**: `DifficultyConditionConfig`
- **필수**: 아니오 (기본값: 빈 객체)
- **설명**: 특정 난이도에서 발견 편지를 비활성화
- **검증 로직**: `ResearchManager.FinishProject()`에서 `Find.Storyteller.difficulty.AllowedBy()` 확인
- **파라미터**:
  - `bigThreatsDisabled` (bool): 대규모 위협 비활성화 시 편지 비활성화
  - `trapsDisabled` (bool): 함정 비활성화 시 편지 비활성화
  - `turretsDisabled` (bool): 포탑 비활성화 시 편지 비활성화
  - `mortarsDisabled` (bool): 박격포 비활성화 시 편지 비활성화
  - `extremeWeatherIncidentsDisabled` (bool): 극한 날씨 사건 비활성화 시 편지 비활성화
- **예시**:
```xml
<discoveredLetterDisabledWhen>
    <turretsDisabled>true</turretsDisabled>
</discoveredLetterDisabledWhen>
```

---

## 8. 테크프린트 (Royalty DLC)

### 8.1 techprintCount
- **타입**: `int`
- **필수**: 아니오 (기본값: 0)
- **설명**: 연구를 시작하기 위해 필요한 테크프린트 개수
- **DLC 요구사항**: Royalty DLC 필요
- **검증**: Royalty가 설치되지 않으면 `PostLoad()`에서 0으로 설정됨
- **주의사항**: `techprintCount > 0`이면 `heldByFactionCategoryTags` 필수
- **예시**: `<techprintCount>1</techprintCount>`

### 8.2 techprintCommonality
- **타입**: `float`
- **필수**: 아니오 (기본값: 1.0)
- **설명**: 테크프린트 생성 확률 가중치
- **DLC 요구사항**: Royalty DLC
- **예시**: `<techprintCommonality>1.0</techprintCommonality>`

### 8.3 techprintMarketVal
- **타입**: `float`
- **필수**: 아니오 (기본값: 1000.0)
- **설명**: 테크프린트의 시장 가치
- **DLC 요구사항**: Royalty DLC
- **예시**: `<techprintMarketValue>1000</techprintMarketValue>`

### 8.4 heldByFactionCategoryTags
- **타입**: `List<string>` (`[NoTranslate]`)
- **필수**: 아니오 (단, `techprintCount > 0`이면 필수)
- **설명**: 테크프린트를 보유할 수 있는 세력 카테고리 태그 목록
- **검증**: `techprintCount == 0`이면 비어있어야 하고, `techprintCount > 0`이면 비어있으면 안 됨
- **예시**:
```xml
<heldByFactionCategoryTags>
    <li>Empire</li>
</heldByFactionCategoryTags>
```

---

## 9. 숨김 조건

### 9.1 hideWhen
- **타입**: `DifficultyConditionConfig`
- **필수**: 아니오 (기본값: 빈 객체)
- **설명**: 특정 조건에서 연구를 숨김
- **파라미터**:
  - `bigThreatsDisabled` (bool): 대규모 위협 비활성화 시 숨김
  - `trapsDisabled` (bool): 함정 비활성화 시 숨김
  - `turretsDisabled` (bool): 포탑 비활성화 시 숨김
  - `mortarsDisabled` (bool): 박격포 비활성화 시 숨김
  - `extremeWeatherIncidentsDisabled` (bool): 극한 날씨 사건 비활성화 시 숨김
- **예시**:
```xml
<hideWhen>
    <turretsDisabled>true</turretsDisabled>
</hideWhen>
```

---

## 10. 메카니터 요구사항 (Biotech DLC)

### 10.1 requiresMechanitor
- **타입**: `bool`
- **필수**: 아니오 (기본값: false)
- **설명**: 연구를 시작하기 위해 메카니터가 필요한지 여부
- **DLC 요구사항**: Biotech DLC
- **검증 로직**: `PlayerMechanitorRequirementMet` 속성에서 확인
- **예시**: `<requiresMechanitor>false</requiresMechanitor>`

---

## 11. 분석 요구사항 (Biotech DLC)

### 11.1 requiredAnalyzed
- **타입**: `List<ThingDef>`
- **필수**: 아니오
- **설명**: 연구를 시작하기 위해 분석해야 하는 아이템 목록
- **DLC 요구사항**: Biotech DLC
- **검증**: Biotech가 설치되지 않으면 `PostLoad()`에서 null로 설정됨
- **검증 로직**: 
  - 각 ThingDef는 `CompAnalyzable` 컴포넌트를 가져야 함
  - `AnalyzedThingsRequirementsMet` 속성에서 분석 완료 여부 확인
- **예시**:
```xml
<requiredAnalyzed>
    <li>MechanoidRemains</li>
</requiredAnalyzed>
```

---

## 12. 전력 재계산

### 12.1 recalculatePower
- **타입**: `bool`
- **필수**: 아니오 (기본값: false)
- **설명**: 전력 재계산 여부 (구체적 용도 불명)
- **예시**: `<recalculatePower>false</recalculatePower>`

---

## 13. 지식 시스템 (Anomaly DLC)

### 13.1 knowledgeCategory
- **타입**: `KnowledgeCategoryDef`
- **필수**: 아니오 (단, `knowledgeCost > 0`이면 필수)
- **설명**: 지식 카테고리 (Anomaly DLC의 지식 시스템)
- **DLC 요구사항**: Anomaly DLC
- **검증**: `knowledgeCost > 0`이면 반드시 설정해야 함
- **예시**: `<knowledgeCategory>RK_KnowledgeCategory</knowledgeCategory>`

### 13.2 knowledgeCost
- **타입**: `float`
- **필수**: 아니오 (기본값: 0)
- **설명**: 지식 비용 (Anomaly DLC의 지식 시스템)
- **DLC 요구사항**: Anomaly DLC
- **주의사항**: 
  - `baseCost`와 동시 사용 불가 (ConfigErrors에서 검증)
  - `knowledgeCategory`가 설정되어 있어야 함
- **비용 계산**: `Cost` 속성에서 `baseCost <= 0`이면 `knowledgeCost` 사용
- **예시**: `<knowledgeCost>100</knowledgeCost>`

---

## 14. 튜토리얼 개념

### 14.1 teachConcept
- **타입**: `ConceptDef`
- **필수**: 아니오
- **설명**: 연구 완료 시 자동으로 가르칠 튜토리얼 개념
- **동작**: `ResearchManager.FinishProject()`에서 `LessonAutoActivator.TeachOpportunity()` 호출
- **용도**: 연구 완료 시 새로운 기능 사용법을 자동으로 안내
- **예시**: `<teachConcept>RK_Concept</teachConcept>`

---

## 15. 일반 규칙 팩

### 15.1 generalRules
- **타입**: `RulePack`
- **필수**: 아니오
- **설명**: 일반 규칙 팩 (구체적 용도 불명)
- **예시**:
```xml
<generalRules>
    <rulesStrings>
        <li>rule1</li>
        <li>rule2</li>
    </rulesStrings>
</generalRules>
```

---

## 16. 중력 엔진 검사 (Odyssey DLC)

### 16.1 requireGravEngineInspected
- **타입**: `bool`
- **필수**: 아니오 (기본값: false)
- **설명**: 연구를 시작하기 위해 중력 엔진을 검사해야 하는지 여부
- **DLC 요구사항**: Odyssey DLC
- **검증 로직**: `InspectionRequirementsMet` 속성에서 `Find.ResearchManager.gravEngineInspected` 확인
- **예시**: `<requireGravEngineInspected>false</requireGravEngineInspected>`

---

## 17. 커스텀 언락 텍스트

### 17.1 customUnlockTexts
- **타입**: `List<string>` (`[MustTranslate]`)
- **필수**: 아니오
- **설명**: 커스텀 언락 텍스트 목록
- **번역**: 번역 가능한 문자열
- **예시**:
```xml
<customUnlockTexts>
    <li>언락 텍스트 1</li>
    <li>언락 텍스트 2</li>
</customUnlockTexts>
```

---

## 18. 주요 속성 및 메서드

### 18.1 계산 속성

#### Cost
- **타입**: `float` (속성)
- **설명**: 실제 연구 비용
- **로직**: `baseCost > 0`이면 `baseCost`, 아니면 `knowledgeCost` 반환

#### CostApparent
- **타입**: `float` (속성)
- **설명**: 플레이어에게 표시되는 비용 (기술 수준 차이 반영)
- **로직**: `Cost * CostFactor(Faction.OfPlayer.def.techLevel)`

#### CostFactor(TechLevel researcherTechLevel)
- **타입**: `float` (메서드)
- **설명**: 기술 수준 차이에 따른 비용 배율 계산
- **로직**: 연구자 기술 수준이 낮으면 레벨당 0.5배씩 증가

#### IsFinished
- **타입**: `bool` (속성)
- **설명**: 연구 완료 여부
- **로직**: `ProgressReal >= Cost`

#### CanStartNow
- **타입**: `bool` (속성)
- **설명**: 연구를 지금 시작할 수 있는지 여부
- **검증 항목**:
  - 연구 미완료
  - 선행 연구 완료
  - 테크프린트 요구사항 충족
  - 필수 연구 벤치 보유
  - 메카니터 요구사항 충족
  - 분석 요구사항 충족
  - 숨김 상태 아님
  - 검사 요구사항 충족

#### PrerequisitesCompleted
- **타입**: `bool` (속성)
- **설명**: 모든 선행 연구 완료 여부
- **검증**: `prerequisites`와 `hiddenPrerequisites` 모두 확인

### 18.2 검증 메서드

#### CanBeResearchedAt(Building_ResearchBench bench, bool ignoreResearchBenchPowerStatus)
- **설명**: 특정 연구 벤치에서 연구할 수 있는지 확인
- **검증 항목**:
  - 필수 연구 벤치 타입 확인
  - 전력 상태 확인 (옵션)
  - 필수 연구 시설 연결 확인

---

## 19. ConfigErrors 검증 항목

다음 항목들이 `ConfigErrors()` 메서드에서 검증됩니다:

1. **techLevel이 Undefined이고 knowledgeCategory가 없음**
2. **researchViewX 또는 researchViewY가 0 미만**
3. **techprintCount == 0인데 heldByFactionCategoryTags가 있음**
4. **techprintCount > 0인데 heldByFactionCategoryTags가 없음**
5. **같은 탭에서 동일 좌표 사용**
6. **Royalty 미설치 시 techprintCount > 0**
7. **requiredAnalyzed의 ThingDef가 분석 불가능**
8. **knowledgeCost > 0인데 knowledgeCategory가 없음**
9. **baseCost와 knowledgeCost 동시 사용**

---

## 20. DLC별 파라미터 요약

### Royalty DLC
- `techprintCount`
- `techprintCommonality`
- `techprintMarketValue`
- `heldByFactionCategoryTags`

### Biotech DLC
- `requiresMechanitor`
- `requiredAnalyzed`

### Anomaly DLC
- `knowledgeCategory`
- `knowledgeCost`

### Odyssey DLC
- `requireGravEngineInspected`

---

## 21. 사용 예시

### 기본 연구
```xml
<ResearchProjectDef>
    <defName>RK_Research_Smithing</defName>
    <label>단조</label>
    <description>금속을 가공하여 무기와 도구를 만드는 기술을 연구합니다.</description>
    <tab>RK_ResearchTab_Default</tab>
    <baseCost>600</baseCost>
    <techLevel>Medieval</techLevel>
    <researchViewX>1</researchViewX>
    <researchViewY>0</researchViewY>
</ResearchProjectDef>
```

### 선행 연구가 있는 연구
```xml
<ResearchProjectDef>
    <defName>RK_Research_AdvancedSmithing</defName>
    <label>고급 단조</label>
    <tab>RK_ResearchTab_Default</tab>
    <baseCost>1200</baseCost>
    <techLevel>Medieval</techLevel>
    <prerequisites>
        <li>RK_Research_Smithing</li>
    </prerequisites>
    <researchViewX>1</researchViewX>
    <researchViewY>1</researchViewY>
</ResearchProjectDef>
```

### 테크프린트가 필요한 연구 (Royalty DLC)
```xml
<ResearchProjectDef>
    <defName>RK_Research_Advanced</defName>
    <label>고급 연구</label>
    <tab>RK_ResearchTab_Default</tab>
    <baseCost>2400</baseCost>
    <techLevel>Industrial</techLevel>
    <techprintCount>1</techprintCount>
    <techprintCommonality>1.0</techprintCommonality>
    <techprintMarketValue>1000</techprintMarketValue>
    <heldByFactionCategoryTags>
        <li>Empire</li>
    </heldByFactionCategoryTags>
    <requiredResearchBuilding>HiTechResearchBench</requiredResearchBuilding>
    <researchViewX>2</researchViewX>
    <researchViewY>2</researchViewY>
</ResearchProjectDef>
```

### 분석이 필요한 연구 (Biotech DLC)
```xml
<ResearchProjectDef>
    <defName>RK_Research_MechAnalysis</defName>
    <label>메카 분석</label>
    <tab>RK_ResearchTab_Default</tab>
    <baseCost>1800</baseCost>
    <techLevel>Industrial</techLevel>
    <requiredAnalyzed>
        <li>MechanoidRemains</li>
    </requiredAnalyzed>
    <researchViewX>3</researchViewX>
    <researchViewY>2</researchViewY>
</ResearchProjectDef>
```

---

## 22. 참고사항

### 상속 가능
- `ResearchProjectDef`는 `Abstract="True"`로 설정하여 부모 Def로 사용 가능
- 자식 Def는 `ParentName`을 통해 상속 가능

### 좌표 자동 조정
- `GenerateNonOverlappingCoordinates()` 메서드가 중복 좌표를 자동으로 조정
- X축: 0.5 단위, Y축: 0.1 단위로 스냅
- Y축 최대값: 6.5

### 비용 계산
- 연구자의 기술 수준이 연구의 기술 수준보다 낮으면 비용 증가
- 레벨 차이당 0.5배씩 증가 (최대 4레벨 차이까지)

### 언락된 Def 자동 계산
- `UnlockedDefs` 속성이 자동으로 다음을 수집:
  - RecipeDef의 products
  - ThingDef (건물, 아이템)
  - ThingDef.plant.sowResearchPrerequisites
  - TerrainDef
  - RecipeDef (수술)
  - PsychicRitualDef

---

## 23. 결론

`ResearchProjectDef`는 RimWorld의 연구 시스템을 정의하는 핵심 Def 클래스입니다. 다양한 DLC와 연동되어 있으며, 복잡한 선행 조건과 요구사항을 설정할 수 있습니다. 각 파라미터의 용도와 제약사항을 이해하고 올바르게 사용하면 효과적인 연구 트리를 구성할 수 있습니다.

