# RimWorld Culture/Ideology 시스템 구조 가이드

## 개요

RimWorld의 Ideology DLC에서 Culture(문화)와 Ideology(이데올로기) 시스템의 구성 요소와 관계를 정리한 문서입니다.

---

## 1. 핵심 구조 관계도

```
Ideo (이데올로기)
├── CultureDef (문화)
├── IdeoFoundation (기초: Deity/Non-Deity)
├── List<MemeDef> (밈 - 핵심 사상)
└── List<Precept> (교리 - 구체적 규칙)
    └── PreceptDef (교리 정의)
        ├── IssueDef (주제)
        ├── requiredMemes (필수 밈)
        ├── associatedMemes (연관 밈)
        ├── conflictingMemes (충돌 밈)
        └── List<PreceptComp> (구현 컴포넌트)
```

---

## 2. 주요 Def 타입

### 2.1 CultureDef (문화 정의)

**역할**: 문화적 배경과 스타일 설정

**주요 필드**:
- `pawnNameMaker` / `pawnNameMakerFemale`: 인물 이름 생성기
- `ideoNameMaker`: 이데올로기 이름 생성기
- `deityNameMaker`: 신 이름 생성기
- `deityTypeMaker`: 신 타입 생성기
- `leaderTitleMaker`: 지도자 칭호 생성기
- `festivalNameMaker`: 축제 이름 생성기
- `thingStyleCategories`: 스타일 카테고리 우선순위
- `styleItemTags`: 스타일 아이템 태그 (수염, 문신 등)
- `preferredWeaponClasses`: 선호 무기 클래스
- `allowedPlaceTags`: 허용된 장소 태그 (OriginTechnoFeudal, OriginSpacer, OriginTribal)

**위치**: `RimworldData/Ideology/Defs/` 또는 `Project/1.6/Defs/CultureDefs/`

---

### 2.2 MemeDef (밈 정의)

**역할**: 이데올로기의 핵심 사상/이념

**주요 필드**:
- `category`: MemeCategory (Normal, Structure)
- `impact`: 영향도 (1-3, Structure는 0)
- `groupDef`: MemeGroupDef (그룹 분류)
- `exclusionTags`: 배제 태그 (다른 밈과 충돌)
- `requireOne`: 필수 PreceptDef 목록 (하나 이상 필요)
- `selectOneOrNone`: 선택적 PreceptDef 목록
- `requiredRituals`: 필수 의식 목록
- `preferredWeaponClasses`: 선호 무기 클래스
- `thingStyleCategories`: 스타일 카테고리
- `styleItemTags`: 스타일 아이템 태그
- `worshipRoomLabel`: 예배실 라벨
- `startingResearchProjects`: 시작 연구 프로젝트
- `apparelRequirements`: 의복 요구사항
- `agreeableTraits` / `disagreeableTraits`: 선호/비선호 특성
- `xenotypeSet`: 선호 제노타입 세트
- `descriptionMaker`: 이데올로기 설명 생성기

**MemeCategory**:
- `Normal`: 일반 밈 (impact 1-3)
- `Structure`: 구조 밈 (impact 0, 필수)

**위치**: `RimworldData/Ideology/Defs/MemeDefs/`

---

### 2.3 PreceptDef (교리 정의)

**역할**: 구체적인 규칙과 행동 지침

**주요 필드**:
- `issue`: IssueDef (어떤 주제인지)
- `preceptClass`: Precept 클래스 타입
  - `Precept_Role`: 역할 (Moralist, Warlord 등)
  - `Precept_Ritual`: 의식
  - `Precept_Apparel`: 의복 선호
  - `Precept_ThingStyle`: 아이템 스타일
  - `Precept_Relic`: 유물
  - `Precept_Weapon`: 무기 선호
  - `Precept_Building`: 건물 요구
  - `Precept_Animal`: 동물 관련
  - 기타...
- `comps`: PreceptComp 리스트 (실제 기능 구현)
- `requiredMemes`: 필수 밈 목록
- `associatedMemes`: 연관 밈 목록
- `conflictingMemes`: 충돌 밈 목록
- `exclusionTags`: 배제 태그
- `maxCount`: 최대 개수
- `impact`: PreceptImpact (Low, Medium, High)
- `selectionWeight`: 선택 가중치
- `classic` / `classicExtra`: 클래식 모드 전용

**PreceptComp 타입 예시**:
- `PreceptComp_SituationalThought`: 상황적 사고
- `PreceptComp_Apparel`: 의복 관련
- `PreceptComp_UnwillingToDo`: 금지 행동
- `PreceptComp_MentalBreak`: 정신 붕괴
- `PreceptComp_DevelopmentPoints`: 발전 포인트
- 기타...

**위치**: `RimworldData/Ideology/Defs/PreceptDefs/`

---

### 2.4 IssueDef (주제 정의)

**역할**: Precept들을 주제별로 그룹화

**주요 필드**:
- `allowMultiplePrecepts`: 여러 Precept 허용 여부
- `iconPath`: 아이콘 경로

**예시 IssueDef**:
- `ApparelDesire`: 의복 선호
- `IdeoRole`: 이데올로기 역할
- `Charity`: 자선
- `Cannibalism`: 식인
- `DrugUse`: 약물 사용
- `Slavery`: 노예제
- `MeatEating`: 육식
- 기타...

**위치**: `PreceptDefs` 파일 내에 함께 정의

---

### 2.5 IdeoFoundation (이데올로기 기초)

**역할**: 이데올로기의 기초 구조

**타입**:
- `IdeoFoundation_Deity`: 신앙 기반
- `IdeoFoundation_NonDeity`: 비신앙 기반

**주요 기능**:
- Meme 선택 및 생성
- Precept 생성 및 배치
- 이데올로기 설명 생성
- 스타일 카테고리 선택

**위치**: `RimworldSource/RimWorld/IdeoFoundation.cs`

---

## 3. 관계 구조

### 3.1 Ideo → Meme 관계

- **Ideo**: `List<MemeDef> memes`
- **Meme**: 이데올로기의 핵심 사상
- **제약**:
  - Meme의 `exclusionTags`로 다른 Meme과 충돌 방지
  - Meme의 `requireOne`으로 필수 Precept 강제

### 3.2 Meme → Precept 관계

- **Meme.requireOne**: Meme이 활성화되면 반드시 하나 이상의 Precept 필요
- **Precept.requiredMemes**: 특정 Meme이 있어야만 선택 가능
- **Precept.associatedMemes**: Meme과 연관됨 (선택 가중치 증가)
- **Precept.conflictingMemes**: Meme과 충돌 (선택 불가)

### 3.3 Precept → Issue 관계

- **Precept.issue**: Precept가 속한 주제
- **Issue.allowMultiplePrecepts**: 같은 Issue에 여러 Precept 허용 여부

### 3.4 Culture → Ideo 관계

- **Ideo.culture**: 이데올로기가 사용하는 문화
- **Culture**: 이름 생성기, 스타일 설정 제공

---

## 4. 생성 흐름

```
1. Ideo 생성
   ↓
2. CultureDef 선택 (또는 랜덤)
   ↓
3. IdeoFoundation 선택 (Deity/NonDeity)
   ↓
4. MemeDef 선택 (1-4개, Structure 1개 필수)
   ↓
5. Meme의 requireOne에 따라 PreceptDef 자동 선택
   ↓
6. IssueDef별로 PreceptDef 추가 선택
   ↓
7. Precept 인스턴스 생성 (PreceptMaker.MakePrecept)
   ↓
8. Ideo 완성
```

---

## 5. 주요 키워드 및 태그

### 5.1 Meme exclusionTags
- `AnimalTreatment`: 동물 대우 관련
- `GenderSupremacy`: 성별 우월주의
- `GroupRelation`: 집단 관계
- 기타...

### 5.2 Precept exclusionTags
- Precept 간 충돌 방지용 태그

### 5.3 Place Tags
- `OriginTechnoFeudal`: 기술-봉건주의 기원
- `OriginSpacer`: 우주 시대 기원
- `OriginTribal`: 부족 기원

### 5.4 Style Tags
- `NoBeard`: 수염 없음
- `NoTattoo`: 문신 없음
- `RK_Style`: 랫킨 전용 스타일

---

## 6. 랫킨 전용 구현 시 고려사항

### 6.1 CultureDef
- `RK_Culture_Kingdom` 이미 존재
- RulePackDef 파일에 이름 생성기들 필요:
  - `NamerIdeo_RatkinKingdom`
  - `NamerDeity_RatkinKingdom`
  - `DeityTypeMaker_RatkinKingdom`
  - `LeaderTitleMaker_RatkinKingdom`
  - `NamerFestival_RatkinKingdom`

### 6.2 MemeDef
- 랫킨 특성에 맞는 밈 생성
- 예: 봉건주의, 기사도, 상업 등

### 6.3 PreceptDef
- 랫킨 문화에 맞는 교리 생성
- 예: 특정 의복 선호, 특정 무기 선호 등

### 6.4 IssueDef
- 필요시 새로운 주제 정의

---

## 7. 파일 구조 예시

```
Project/1.6/Defs/
├── CultureDefs/
│   └── Cultures.xml (CultureDef)
├── MemeDefs/
│   └── Memes_Ratkin.xml (MemeDef)
├── PreceptDefs/
│   ├── Precepts_Ratkin_Role.xml (PreceptDef)
│   ├── Precepts_Ratkin_Apparel.xml (PreceptDef)
│   └── Precepts_Ratkin_Ritual.xml (PreceptDef)
└── RulePackDefs/
    └── Names_Ratkin.xml (RulePackDef - 이름 생성기)
```

---

## 8. 참고 소스코드 위치

- **Ideo**: `RimworldSource/RimWorld/Ideo.cs`
- **CultureDef**: `RimworldSource/RimWorld/CultureDef.cs`
- **MemeDef**: `RimworldSource/RimWorld/MemeDef.cs`
- **PreceptDef**: `RimworldSource/RimWorld/PreceptDef.cs`
- **IssueDef**: `RimworldSource/RimWorld/IssueDef.cs`
- **IdeoFoundation**: `RimworldSource/RimWorld/IdeoFoundation.cs`
- **IdeoGenerator**: `RimworldSource/RimWorld/IdeoGenerator.cs`

---

## 9. 핵심 요약

1. **Ideo** = Culture + Foundation + Memes + Precepts
2. **Meme** = 핵심 사상 (1-4개, Structure 1개 필수)
3. **Precept** = 구체적 규칙 (Issue별로 그룹화)
4. **Issue** = 주제 (Precept들을 묶는 카테고리)
5. **Culture** = 문화적 배경 (이름, 스타일 등)

