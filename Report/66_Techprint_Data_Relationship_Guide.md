# RimWorld 테크프린트 데이터 관계 가이드

## 개요

이 문서는 RimWorld에서 아이템(ThingDef), 연구(ResearchProjectDef), 테크프린트(Techprint) 간의 연결 관계를 간결하게 정리한 참고 자료입니다.

---

## 1. 핵심 데이터 관계도

```
ThingDef (아이템)
    └─ recipeMaker
        └─ researchPrerequisite: "ResearchProjectDef.defName" 참조
            │
            ├─ ResearchProjectDef (연구)
            │   ├─ techprintCount: 테크프린트 필요 개수 (> 0)
            │   ├─ techprintCommonality: 생성 확률 가중치
            │   ├─ techprintMarketValue: 시장 가치
            │   └─ heldByFactionCategoryTags: 보유 가능 팩션
            │
            └─ Techprint ThingDef (자동 생성)
                └─ defName: "Techprint_" + ResearchProjectDef.defName
                └─ CompProperties_Techprint
                    └─ project: ResearchProjectDef 참조
```

---

## 2. 데이터 연결 구조

### 2.1 ThingDef → ResearchProjectDef 연결

**위치**: `ThingDef/recipeMaker/researchPrerequisite`

```xml
<ThingDef>
  <recipeMaker>
    <researchPrerequisite>CataphractArmor</researchPrerequisite>
    <!-- researchPrerequisite 값은 ResearchProjectDef.defName과 정확히 일치해야 함 -->
  </recipeMaker>
</ThingDef>
```

**연결 규칙**:
- `researchPrerequisite`의 값은 반드시 `ResearchProjectDef.defName`과 **정확히 일치**해야 함
- 대소문자 구분됨
- 연구가 완료되어야 아이템 제작 가능

### 2.2 ResearchProjectDef → Techprint 생성 조건

**위치**: `ResearchProjectDef` 필드들

```xml
<ResearchProjectDef>
  <defName>CataphractArmor</defName>
  
  <!-- 테크프린트 생성 조건 -->
  <techprintCount>2</techprintCount>                    <!-- 필수: 0보다 큰 값 -->
  <techprintCommonality>3</techprintCommonality>         <!-- 선택: 생성 확률 (기본값 1) -->
  <techprintMarketValue>3000</techprintMarketValue>      <!-- 선택: 시장 가치 -->
  <heldByFactionCategoryTags>                            <!-- 선택: 팩션 카테고리 -->
    <li>Empire</li>
  </heldByFactionCategoryTags>
</ResearchProjectDef>
```

**생성 조건**:
- `techprintCount > 0`이면 자동으로 테크프린트 ThingDef 생성
- 게임 시작 시 `ThingDefGenerator_Techprints.ImpliedTechprintDefs()` 실행

### 2.3 Techprint ThingDef 자동 생성 규칙

**생성 위치**: 런타임 (게임 시작 시)

**생성 규칙**:
- **defName**: `"Techprint_" + ResearchProjectDef.defName`
- **예시**: `CataphractArmor` → `Techprint_CataphractArmor`
- **컴포넌트**: `CompProperties_Techprint` 자동 추가
  - `project`: 원본 `ResearchProjectDef` 참조

**소스 코드**: `RimworldSource/RimWorld/ThingDefGenerator_Techprints.cs`

---

## 3. 필수 연결 정보 체크리스트

### 3.1 아이템에 테크프린트 시스템 적용 시

#### ✅ Step 1: ResearchProjectDef 생성
- [ ] `defName` 정의 (고유한 이름)
- [ ] `techprintCount` 설정 (1~3 권장)
- [ ] `techprintCommonality` 설정 (1~5 권장)
- [ ] `techprintMarketValue` 설정 (선택)
- [ ] `heldByFactionCategoryTags` 설정 (선택)

#### ✅ Step 2: ThingDef에 연구 연결
- [ ] `recipeMaker/researchPrerequisite` 추가
- [ ] `researchPrerequisite` 값이 `ResearchProjectDef.defName`과 **정확히 일치** 확인

#### ✅ Step 3: 테크프린트 확인
- [ ] 게임 시작 시 자동 생성 확인
- [ ] 생성된 테크프린트 defName: `"Techprint_" + ResearchProjectDef.defName`

---

## 4. 데이터 필드 상세

### 4.1 ResearchProjectDef 테크프린트 관련 필드

| 필드명 | 타입 | 필수 | 기본값 | 설명 |
|--------|------|------|--------|------|
| `techprintCount` | int | ✅ | 0 | 필요한 테크프린트 개수 (0보다 큰 값이면 테크프린트 생성) |
| `techprintCommonality` | float | ❌ | 1 | 테크프린트 생성 확률 가중치 (높을수록 더 자주 생성) |
| `techprintMarketValue` | float | ❌ | - | 테크프린트의 시장 가치 |
| `heldByFactionCategoryTags` | List\<string\> | ❌ | - | 테크프린트를 보유할 수 있는 팩션 카테고리 태그 |

**팩션 카테고리 태그 예시**:
- `Empire`: 제국 팩션 (Royalty DLC)
- `Outlander`: 외지인 팩션
- `Tribal`: 부족 팩션

### 4.2 ThingDef recipeMaker 필드

| 필드명 | 타입 | 필수 | 설명 |
|--------|------|------|------|
| `researchPrerequisite` | string | ✅ | 연구 프로젝트의 `defName` (정확히 일치해야 함) |

---

## 5. 실제 예시: CataphractArmor

### 5.1 ResearchProjectDef

```xml
<!-- RimworldData/Royalty/Defs/ResearchProjectDefs/ResearchProjects_Apparel.xml -->
<ResearchProjectDef>
  <defName>CataphractArmor</defName>
  <baseCost>6000</baseCost>
  <techLevel>Spacer</techLevel>
  <prerequisites>
    <li>PoweredArmor</li>
  </prerequisites>
  
  <!-- 테크프린트 설정 -->
  <techprintCount>2</techprintCount>
  <techprintCommonality>3</techprintCommonality>
  <techprintMarketValue>3000</techprintMarketValue>
  <heldByFactionCategoryTags>
    <li>Empire</li>
  </heldByFactionCategoryTags>
</ResearchProjectDef>
```

### 5.2 ThingDef 연결

```xml
<!-- RimworldData/Royalty/Defs/ThingDefs_Misc/Apparel_Various.xml -->
<ThingDef Name="ApparelArmorCataphractBase" ParentName="ArmorMachineableBase" Abstract="True">
  <recipeMaker>
    <researchPrerequisite>CataphractArmor</researchPrerequisite>
    <!-- ResearchProjectDef.defName과 정확히 일치 -->
    <skillRequirements>
      <Crafting>8</Crafting>
    </skillRequirements>
    <recipeUsers Inherit="False">
      <li>FabricationBench</li>
    </recipeUsers>
  </recipeMaker>
</ThingDef>
```

### 5.3 자동 생성되는 Techprint

**생성된 ThingDef**:
- **defName**: `Techprint_CataphractArmor`
- **label**: "기술청사진 (근위대 갑옷)"
- **CompProperties_Techprint.project**: `CataphractArmor` (ResearchProjectDef 참조)

---

## 6. 주의사항

### 6.1 defName 일치 필수
- `ThingDef.recipeMaker.researchPrerequisite`는 반드시 `ResearchProjectDef.defName`과 **정확히 일치**해야 함
- 대소문자 구분됨
- 오타 시 연구 연결 실패

### 6.2 테크프린트 생성 조건
- `techprintCount > 0`이어야 테크프린트 생성
- 연구의 선행 조건(`prerequisites`)이 완료되어 있어야 생성됨
- 연구가 이미 완료되면 생성되지 않음

### 6.3 테크프린트 ThingDef 수동 생성 불필요
- 게임이 자동으로 생성하므로 별도의 ThingDef 정의 **불필요**
- `ThingDefGenerator_Techprints`가 런타임에 생성

### 6.4 팩션 카테고리 태그
- `heldByFactionCategoryTags`에 지정된 팩션만 테크프린트 보유 가능
- 지정하지 않으면 모든 팩션에서 획득 가능

---

## 7. 참고 소스 코드 위치

| 항목 | 경로 |
|------|------|
| ResearchProjectDef 정의 | `RimworldSource/RimWorld/ResearchProjectDef.cs` |
| 테크프린트 생성기 | `RimworldSource/RimWorld/ThingDefGenerator_Techprints.cs` |
| 테크프린트 유틸리티 | `RimworldSource/RimWorld/TechprintUtility.cs` |
| 테크프린트 컴포넌트 | `RimworldSource/RimWorld/CompProperties_Techprint.cs` |

---

## 8. 요약: 구현 체크리스트

새로운 아이템에 테크프린트 시스템을 적용할 때:

1. ✅ **ResearchProjectDef 생성**
   - `techprintCount > 0` 설정
   - `techprintCommonality`, `techprintMarketValue`, `heldByFactionCategoryTags` 선택 설정

2. ✅ **ThingDef에 연구 연결**
   - `recipeMaker/researchPrerequisite`에 `ResearchProjectDef.defName` 정확히 입력

3. ✅ **자동 생성 확인**
   - 게임 시작 시 `Techprint_[ResearchProjectDef.defName]` 자동 생성됨
   - 별도 ThingDef 정의 불필요

---

## 9. 데이터 흐름도

```
[게임 시작]
    │
    ├─ ResearchProjectDef 로드
    │   └─ techprintCount > 0 확인
    │       └─ ThingDefGenerator_Techprints 실행
    │           └─ Techprint ThingDef 자동 생성
    │               └─ defName: "Techprint_" + ResearchProjectDef.defName
    │
    └─ ThingDef 로드
        └─ recipeMaker.researchPrerequisite 확인
            └─ ResearchProjectDef.defName과 매칭
                └─ 연구 완료 시 아이템 제작 가능
```

---

**마지막 업데이트**: 2025-01-XX  
**참고 문서**: `Report/65_CataphractArmor_Techprint_Implementation_Analysis.md`

