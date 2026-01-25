# Food Merchant Stock Lineup Analysis
# 식자재 상인 상품 라인업 분석 보고서

## 개요
랫킨 식자재 상인(RK_TraderKind_KingdomFoodMerchant)의 상품 라인업을 카테고리별로 정리한 보고서입니다. 고기, 야채, 동물 제품, 조리된 음식 등 모든 먹을 수 있는 식자재를 종류별로 분류하여 정리했습니다.

---

## 1. 식자재 상인 기본 정보
**DefName**: `RK_TraderKind_KingdomFoodMerchant`  
**Label**: `food merchant`

---

## 2. 상품 라인업

### 2.1 자원 (Resources)
- **Silver**: 750~1200개
  - 거래용 기본 화폐

### 2.2 물고기 (Fish 카테고리) [Odyssey]
- **카테고리**: `Fish`
- **종류 수**: 2~4종류
- **총 가격 범위**: 150~400
- **설명**: Odyssey DLC에서 추가되는 모든 종류의 물고기

**포함 가능한 물고기 종류**:
- 연어 (Fish_Salmon)
- 배스 (Fish_Bass)
- 틸라피아 (Fish_Tilapia)
- 대구 (Fish_Cod)
- 파란농어 (Fish_Bluefish)
- 메기 (Fish_Catfish)
- 돔발상어 (Fish_Dogfish)
- 가자미 (Fish_Flounder)
- 얼음갈치 (Fish_Frostfish)
- 구피 (Fish_Guppy)
- 돛새치 (Fish_Marlin)
- 기타 물고기 종류

### 2.3 일반 동물 고기 (MeatRaw 카테고리, Fish 제외)
- **카테고리**: `MeatRaw`
- **종류 수**: 4~6종류
- **총 가격 범위**: 300~600
- **제외 카테고리**: `Fish` (물고기 제외)
- **설명**: 물고기를 제외한 모든 종류의 생고기

**포함 가능한 고기 종류**:
- 일반 동물 고기 (소, 돼지, 양, 사슴, 말, 엘크, 거북이, 거북, 이구아나 등)
- 인간형 고기 (HumanlikeMeat)
- 곤충 고기 (InsectMeat)
- 기타 특수 고기

### 2.4 야채 (PlantFoodRaw 카테고리)
- **카테고리**: `PlantFoodRaw`
- **종류 수**: 4~6종류
- **총 가격 범위**: 200~600
- **설명**: 모든 종류의 식물성 식자재

**포함 가능한 야채 종류**:
- 옥수수 (Corn)
- 감자 (Potato)
- 쌀 (Rice)
- 딸기 (Strawberry)
- 토마토 (Tomato)
- 당근 (Carrot)
- 아가베 (Agave)
- 건초 (Hay)
- 기타 식물성 식자재

### 2.5 비수정란 (EggsUnfertilized 카테고리)
- **카테고리**: `EggsUnfertilized`
- **종류 수**: 2~3종류
- **총 가격 범위**: 100~250
- **설명**: 동물에서 얻을 수 있는 비수정란 계란

**포함 가능한 계란 종류**:
- 닭 계란 (EggChickenUnfertilized)
- 오리 계란 (EggDuckUnfertilized)
- 거위 계란 (EggGooseUnfertilized)
- 칠면조 계란 (EggTurkeyUnfertilized)
- 타조 계란 (EggOstrichUnfertilized)
- 에뮤 계란 (EggEmuUnfertilized)
- 화식조 계란 (EggCassowaryUnfertilized)
- 코브라 계란 (EggCobraUnfertilized)
- 이구아나 계란 (EggIguanaUnfertilized)
- 거북 계란 (EggTortoiseUnfertilized)
- 바다거북 계란 (EggSeaTurtleUnfertilized) [Odyssey]
- 악어 계란 (EggAlligatorUnfertilized) [Odyssey]
- 모니터 도마뱀 계란 (EggMonitorLizardUnfertilized) [Odyssey]
- 플라밍고 계란 (EggFlamingoUnfertilized) [Odyssey]
- 왜가리 계란 (EggHeronUnfertilized) [Odyssey]
- 백조 계란 (EggSwanUnfertilized) [Odyssey]
- 참새 계란 (EggSparrowUnfertilized) [Odyssey]
- 까마귀 계란 (EggCrowUnfertilized) [Odyssey]
- 파랑새 계란 (EggBluebirdUnfertilized) [Odyssey]
- 메추라기 계란 (EggQuailUnfertilized) [Odyssey]
- 마코앵무새 계란 (EggMacawUnfertilized) [Odyssey]
- 공작새 계란 (EggPeacockUnfertilized) [Odyssey]
- 독수리 계란 (EggVultureUnfertilized) [Odyssey]
- 펭귄 계란 (EggPenguinUnfertilized) [Odyssey]
- 기타 조류 및 파충류 계란

### 2.6 우유
- **ThingDef**: `Milk`
- **개수 범위**: 20~50개
- **설명**: 동물에서 얻을 수 있는 우유

### 2.7 조리된 음식 (3개 중 하나)
- **종류**: 
  - MealSimple (간단한 식사)
  - MealFine (정교한 식사)
  - MealLavish (호화로운 식사)
- **개수 범위**: 30~60개
- **설명**: 3가지 조리된 음식 중 하나만 판매

### 2.8 기타 음식류 (MultiDef)
- **종류**: 
  - Pemmican (펨미컨)
  - Chocolate (초콜릿)
  - Kibble (사료)
- **개수 범위**: 각 20~50개
- **설명**: 보존 식품 및 특수 음식류

**상세 설명**:
- **Pemmican**: 지방과 식물을 섞어 만든 보존 식품, 긴 보관 기간
- **Chocolate**: 초콜릿, 사치품
- **Kibble**: 동물 사료, 인간도 먹을 수 있지만 선호도 낮음

### 2.9 랫킨 고유 식자재 (MultiDef)
- **종류**:
  - RK_Food_Hardtack (하드택)
  - RK_StrawberryBeer (딸기 맥주)
- **개수 범위**: 각 15~40개
- **설명**: 랫킨 종족 고유의 식자재

**상세 설명**:
- **RK_Food_Hardtack**: 밀가루와 물로 만든 장기 보관 식품, 여행용 식량
- **RK_StrawberryBeer**: 랫킨 고유의 알코올 음료

### 2.10 식자재 보관 관련
- **WoodLog**: 200~400개
  - 식자재 보관 및 연료용 나무

---

## 3. 변경 사항 요약

### 변경 전
- `FoodRaw` 카테고리만 사용 (4~6종류)
- `FoodMeals` 카테고리 (2~3종류, 20~50개)
- 랫킨 고유 식자재만 별도 지정

### 변경 후
- **물고기 (Fish)**: 2~4종류 [Odyssey]
- **일반 동물 고기 (MeatRaw, Fish 제외)**: 4~6종류 별도 분류
- **야채 (PlantFoodRaw)**: 4~6종류 별도 분류
- **비수정란 (EggsUnfertilized)**: 2~3종류 별도 분류
- **우유 (Milk)**: 20~50개 별도 지정
- **조리된 음식**: 3개 중 하나 (MealSimple, MealFine, MealLavish), 30~60개
- **기타 음식류**: Pemmican, Chocolate, Kibble 추가
- **랫킨 고유 식자재**: 하드택, 딸기 맥주 포함

---

## 4. 카테고리 구조 및 상세 항목

### 4.1 전체 카테고리 구조

```
Foods
├── FoodRaw (원재료)
│   ├── MeatRaw (고기)
│   │   ├── 일반 동물 고기 (소, 돼지, 양, 사슴, 말, 엘크, 거북이, 거북, 이구아나 등)
│   │   ├── 인간형 고기 (HumanlikeMeat)
│   │   ├── 곤충 고기 (InsectMeat)
│   │   └── Fish (물고기) [Odyssey] - 별도 카테고리로 분리됨
│   │       ├── Fish_Salmon (연어)
│   │       ├── Fish_Bass (배스)
│   │       ├── Fish_Tilapia (틸라피아)
│   │       ├── Fish_Cod (대구)
│   │       ├── Fish_Bluefish (파란농어)
│   │       ├── Fish_Catfish (메기)
│   │       ├── Fish_Dogfish (돔발상어)
│   │       ├── Fish_Flounder (가자미)
│   │       ├── Fish_Frostfish (얼음갈치)
│   │       ├── Fish_Guppy (구피)
│   │       ├── Fish_Marlin (돛새치)
│   │       └── 기타 물고기 종류
│   ├── PlantFoodRaw (야채)
│   │   ├── RawCorn (옥수수)
│   │   ├── RawPotatoes (감자)
│   │   ├── RawRice (쌀)
│   │   ├── RawBerries (딸기/베리)
│   │   ├── RawAgave (아가베)
│   │   ├── RawFungus (버섯) [Ideology - Fungus 태그]
│   │   └── RawToxipotato (독감자) [Biotech]
│   └── AnimalProductRaw (동물 제품)
│       ├── EggsUnfertilized (비수정란) - 별도 카테고리로 분리됨
│       │   ├── EggChickenUnfertilized (닭 계란)
│       │   ├── EggDuckUnfertilized (오리 계란)
│       │   ├── EggGooseUnfertilized (거위 계란)
│       │   ├── EggTurkeyUnfertilized (칠면조 계란)
│       │   ├── EggOstrichUnfertilized (타조 계란)
│       │   ├── EggEmuUnfertilized (에뮤 계란)
│       │   ├── EggCassowaryUnfertilized (화식조 계란)
│       │   ├── EggCobraUnfertilized (코브라 계란)
│       │   ├── EggIguanaUnfertilized (이구아나 계란)
│       │   ├── EggTortoiseUnfertilized (거북 계란)
│       │   ├── EggSeaTurtleUnfertilized (바다거북 계란) [Odyssey]
│       │   ├── EggAlligatorUnfertilized (악어 계란) [Odyssey]
│       │   ├── EggMonitorLizardUnfertilized (모니터 도마뱀 계란) [Odyssey]
│       │   ├── EggFlamingoUnfertilized (플라밍고 계란) [Odyssey]
│       │   ├── EggHeronUnfertilized (왜가리 계란) [Odyssey]
│       │   ├── EggSwanUnfertilized (백조 계란) [Odyssey]
│       │   ├── EggSparrowUnfertilized (참새 계란) [Odyssey]
│       │   ├── EggCrowUnfertilized (까마귀 계란) [Odyssey]
│       │   ├── EggBluebirdUnfertilized (파랑새 계란) [Odyssey]
│       │   ├── EggQuailUnfertilized (메추라기 계란) [Odyssey]
│       │   ├── EggMacawUnfertilized (마코앵무새 계란) [Odyssey]
│       │   ├── EggPeacockUnfertilized (공작새 계란) [Odyssey]
│       │   ├── EggVultureUnfertilized (독수리 계란) [Odyssey]
│       │   └── EggPenguinUnfertilized (펭귄 계란) [Odyssey]
│       ├── Milk (우유) - 별도 지정
│       ├── InsectJelly (곤충 젤리)
│       └── EggsFertilized (수정란) - 제외됨
└── FoodMeals (조리된 음식)
    ├── MealSimple (간단한 식사) - 3개 중 하나만 선택
    ├── MealFine (정교한 식사) - 3개 중 하나만 선택
    └── MealLavish (호화로운 식사) - 3개 중 하나만 선택
```

### 4.2 DLC별 추가 항목

#### Core (기본 게임)
- 모든 기본 고기, 야채, 계란, 조리된 음식

#### Ideology DLC
- **RawFungus (버섯)**: Fungus 태그를 가진 버섯류 식자재

#### Biotech DLC
- **RawToxipotato (독감자)**: 독성 감자, 식중독 확률 증가
- **BabyFood (유아용 식사)**: 유아 전용 식사
- **HemogenPack (헤모겐 팩)**: 헤모겐 인간용 혈액 팩

#### Odyssey DLC
- **물고기 (Fish)**: 다양한 종류의 물고기 (연어, 배스, 틸라피아, 대구, 파란농어, 메기, 돔발상어, 가자미, 얼음갈치, 구피, 돛새치 등)
- **해안 동물 계란**: 바다거북, 악어, 모니터 도마뱀 계란
- **해안 조류 계란**: 플라밍고, 왜가리, 백조, 참새, 까마귀, 파랑새, 메추라기, 마코앵무새, 공작새, 독수리, 펭귄 계란

### 4.3 카테고리별 특징

#### Fish (물고기) [Odyssey]
- **포함**: Odyssey DLC에서 추가되는 모든 물고기 종류
- **종류 수**: 2~4종류
- **특징**: 생고기로 먹을 수 있지만 선호도 낮음 (RawBad)
- **별도 분리**: 일반 동물 고기와 별도로 분리되어 판매됨

#### MeatRaw (일반 동물 고기)
- **포함**: 물고기를 제외한 모든 동물 고기, 인간형 고기, 곤충 고기
- **제외**: Fish 카테고리 제외
- **종류 수**: 4~6종류
- **특징**: 생고기로 먹을 수 있지만 선호도 낮음 (RawBad)

#### PlantFoodRaw (야채)
- **포함**: 모든 식물성 식자재
- **종류 수**: 4~6종류
- **버섯**: Ideology DLC의 Fungus 태그를 가진 버섯류
- **독감자**: Biotech DLC의 독성 감자

#### EggsUnfertilized (비수정란)
- **포함**: 모든 비수정란 계란
- **종류 수**: 2~3종류
- **계란 종류**: Core + Odyssey DLC의 다양한 조류 및 파충류 계란
- **별도 분리**: 동물 제품에서 별도로 분리되어 판매됨

#### Milk (우유)
- **별도 지정**: 동물 제품 카테고리가 아닌 단일 아이템으로 별도 지정
- **개수**: 20~50개

#### FoodMeals (조리된 음식)
- **포함**: MealSimple, MealFine, MealLavish 중 하나만 선택
- **개수**: 선택된 종류당 30~60개
- **특징**: 3가지 중 랜덤하게 하나만 판매됨

---

## 5. 특징

### 5.1 종류별 분류
- 고기, 야채, 동물 제품을 각각 별도 카테고리로 분리하여 더 다양한 식자재 제공
- 각 카테고리별로 적절한 가격 범위 설정

### 5.2 조리된 음식 다양화
- 조리된 음식 종류를 2~3종류에서 3~5종류로 증가
- 각 종류당 개수도 20~50개에서 30~60개로 증가

### 5.3 보존 식품 추가
- Pemmican, 하드택 등 장기 보관 가능한 식자재 포함
- 여행 및 장기 보관에 유용

### 5.4 랫킨 고유 아이템
- 하드택과 딸기 맥주 등 랫킨 종족만의 고유 식자재 제공

---

## 6. 참고 사항

- 수정란(EggsFertilized)은 동물 제품에서 제외되어 판매되지 않음
- 모든 식자재는 게임 내 실제 존재하는 ThingDef를 기반으로 자동 생성됨
- DLC에 따라 추가되는 식자재도 자동으로 포함될 수 있음

---

## 7. 관련 파일

- **TraderKindDef**: `Project/1.6/Defs/TraderDefs/TraderKinds_Caravan_Ratkin.xml`
- **ThingCategoryDef**: `RimworldData/Core/Defs/ThingCategoryDefs/ThingCategories.xml`
- **Food ThingDefs**: `RimworldData/Core/Defs/ThingDefs_Items/Items_Food.xml`

---

**작성일**: 2024년  
**작성자**: AI Assistant (Composer)
