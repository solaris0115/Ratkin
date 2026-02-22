# DisplayPriority 의류 레시피 분석 보고서

**태그**: DisplayPriority RecipeMaker Apparel Order 정렬 순서 의류 레시피

**작성일**: 2026-01-29

**분석 대상**: 림월드 코어 및 Ratkin 프로젝트 의류 레시피의 `displayPriority` 속성

---

## 1. 개요

`displayPriority`는 림월드에서 레시피의 제작 메뉴 표시 순서를 제어하는 속성입니다. 이 보고서는 의류 레시피에 한정하여 `displayPriority`의 사용 현황과 패턴을 분석합니다.

---

## 2. displayPriority 기본 개념

### 2.1 위치 및 구조

`displayPriority`는 `ThingDef`의 `recipeMaker` 섹션 내에 위치합니다:

```xml
<ThingDef ParentName="ApparelMakeableBase">
    <defName>Apparel_TribalA</defName>
    <!-- ... -->
    <recipeMaker>
        <recipeUsers>
            <li>ElectricTailoringBench</li>
            <li>HandTailoringBench</li>
            <li>CraftingSpot</li>
        </recipeUsers>
        <displayPriority>200</displayPriority>
    </recipeMaker>
</ThingDef>
```

### 2.2 값의 의미

- **숫자 값**: 정수로 표현되며, **낮을수록 우선순위가 높습니다**
- **정렬 순서**: 제작 메뉴에서 낮은 값이 먼저 표시됨
- **기본값**: `displayPriority`가 없으면 기본값(보통 0 또는 매우 높은 값)이 적용됨

---

## 3. 림월드 코어 의류 displayPriority 분석

### 3.1 Core 의류 displayPriority 값 분포

**파일**: `RimworldData/Core/Defs/ThingDefs_Misc/Apparel_Various.xml`

| 의류 | displayPriority | 티어/카테고리 | 비고 |
|------|----------------|--------------|------|
| Apparel_TribalA | 200 | Neolithic | 기본 의류 |
| Apparel_Parka | 220 | Neolithic | 외투 |
| Apparel_BasicShirt | 205 | Medieval | 기본 셔츠 |
| Apparel_CollarShirt | 208 | Medieval | 단추 셔츠 |
| Apparel_Duster | 215 | Medieval | 더스터 코트 |
| Apparel_Jacket | 210 | Medieval | 재킷 |
| Apparel_PlateArmor | 250 | Medieval | 판금 갑옷 |
| Apparel_FlakVest | 220 | Industrial | 플랙 조끼 |
| Apparel_FlakPants | 230 | Industrial | 플랙 바지 |
| Apparel_FlakJacket | 225 | Industrial | 플랙 재킷 |
| Apparel_PowerArmor | 105 | Spacer | 파워 아머 |
| Apparel_ArmorRecon | 115 | Spacer | 정찰 아머 |
| Apparel_Cape | 235 | Spacer | 망토 |
| Apparel_Robe | 240 | Spacer | 로브 |

### 3.2 패턴 분석

#### 3.2.1 티어별 우선순위

1. **Spacer 티어 (105-115)**: 최고급 장비로 가장 높은 우선순위
2. **Medieval 티어 (205-265)**: 기본 의류들이 중간 우선순위
3. **Neolithic 티어 (200-220)**: 초기 의류들
4. **Industrial 티어 (220-230)**: 산업 시대 의류들

#### 3.2.2 카테고리별 그룹화

- **기본 의류**: 200-215 범위
- **방어구**: 220-250 범위
- **고급 방어구**: 105-115 범위
- **특수 의류**: 235-240 범위

---

## 4. Ratkin 프로젝트 displayPriority 현황

### 4.1 현재 사용 현황

#### 4.1.1 displayPriority가 있는 파일

1. **Apparel_Spacer.xml**
   - `RK_SpacerArmor`: 237
   - `RK_SpacerHelmet`: 235
   - `RK_SpacerHelmet_Biotech`: 235

2. **Weapon_Range.xml**
   - 일부 무기 레시피: 450

#### 4.1.2 displayPriority가 없는 파일

다음 파일들에는 `displayPriority`가 **없습니다**:

- `Apparel_Util.xml` (배너, 가방, 방패 등)
- `Apparel_Various.xml` (모든 기본 의류)
- `Apparel_Layer.xml` (레이어 정의만 포함)
- `Apparel_DropOnly.xml`
- `Apparel_Royal.xml`

### 4.2 누락된 displayPriority 분석

#### 4.2.1 Apparel_Util.xml

**배너류**:
- `RK_Apparel_Banner`: 없음

**가방류**:
- `RK_CrossBack`: 없음
- `RK_Backpack`: 없음
- `RK_OutdoorBackpack`: 없음

**방패류**:
- `RK_WoodenShield`: 없음
- `RK_HeavyShield`: 없음
- `RK_TowerShield`: 없음

#### 4.2.2 Apparel_Various.xml

**1티어 의류** (RK_Research_Tailoring):
- `RK_ApronSkirt`: 없음
- `RK_SummerDress`: 없음
- `RK_StrawHat`: 없음
- `RK_Muffler`: 없음
- `RK_Cardigan`: 없음
- `RK_WoolenHat`: 없음

**2티어 의류** (RK_Research_FunctionalApparel):
- `RK_WorkerWear`: 없음
- `RK_Coif`: 없음
- `RK_ResearchGown`: 없음
- `RK_ResearchGlasses`: 없음
- `RK_ExplorerWear`: 없음
- `RK_ExplorerHat`: 없음
- `RK_ChefSuit`: 없음
- `RK_ChefHat`: 없음
- `RK_GaurdenUniform`: 없음
- `RK_WinterRobe`: 없음

**3티어 의류** (RK_Research_AdvancedApparel):
- `RK_OrderUniform`: 없음
- `RK_BulletProofHelmet`: 없음
- `RK_FlatColorCoat`: 없음
- `RK_FrillOnepiece`: 없음
- `RK_HairCorsage`: 없음
- `RK_RibbonHairBand`: 없음
- `RK_SistersDerss`: 없음
- `RK_SistersVeil`: 없음

**특수 의류**:
- `RK_BattleSuit`: 없음
- `RK_Mask`: 없음
- `RK_MaskB`: 없음
- `RK_HeadBand`: 없음
- `RK_Plate`: 없음
- `RK_PlateHelmA/B/C`: 없음

---

## 5. displayPriority 권장 값 제안

### 5.1 티어별 권장 범위

림월드 코어 패턴을 참고하여 다음과 같이 제안합니다:

| 티어 | 권장 범위 | 설명 |
|------|----------|------|
| 1티어 (RK_Research_Tailoring) | 200-220 | 기본 의류, 초기 단계 |
| 2티어 (RK_Research_FunctionalApparel) | 220-240 | 기능성 의류, 중간 단계 |
| 3티어 (RK_Research_AdvancedApparel) | 240-260 | 고급 의류, 후반 단계 |
| 특수/방어구 | 250-280 | 방어구, 특수 의류 |
| Spacer 티어 | 100-120 | 최고급 장비 (이미 적용됨) |

### 5.2 카테고리별 세부 제안

#### 5.2.1 기본 의류 (1티어)

```
RK_ApronSkirt: 200
RK_SummerDress: 205
RK_StrawHat: 210
RK_Muffler: 215
RK_Cardigan: 220
RK_WoolenHat: 225
```

#### 5.2.2 기능성 의류 (2티어)

```
RK_WorkerWear: 230
RK_Coif: 235
RK_ResearchGown: 240
RK_ResearchGlasses: 245
RK_ExplorerWear: 250
RK_ExplorerHat: 255
RK_ChefSuit: 260
RK_ChefHat: 265
RK_GaurdenUniform: 270
RK_WinterRobe: 275
```

#### 5.2.3 고급 의류 (3티어)

```
RK_OrderUniform: 280
RK_BulletProofHelmet: 285
RK_FlatColorCoat: 290
RK_FrillOnepiece: 295
RK_HairCorsage: 300
RK_RibbonHairBand: 305
RK_SistersDerss: 310
RK_SistersVeil: 315
```

#### 5.2.4 유틸리티 아이템

```
RK_CrossBack: 320
RK_Backpack: 325
RK_OutdoorBackpack: 330
RK_Apparel_Banner: 335
```

#### 5.2.5 방패류

```
RK_WoodenShield: 340
RK_HeavyShield: 345
RK_TowerShield: 350
```

#### 5.2.6 방어구

```
RK_Plate: 360
RK_PlateHelmA/B/C: 365
RK_BattleSuit: 370
RK_Mask/MaskB: 375
```

---

## 6. RecipeDef에서의 displayPriority

### 6.1 RecipeDef 사용 예시

프로젝트 내 `RecipeDef`에서도 `displayPriority`가 사용됩니다:

**파일**: `Project/1.6/Defs/Things_ItemDefs/Thing_Food.xml`

```xml
<RecipeDef>
    <defName>RK_Recipe_Hardtack</defName>
    <!-- ... -->
    <displayPriority>1430</displayPriority>
</RecipeDef>

<RecipeDef>
    <label>make hardtack x4</label>
    <!-- ... -->
    <displayPriority>1440</displayPriority>
</RecipeDef>
```

### 6.2 RecipeDef vs ThingDef.recipeMaker

- **RecipeDef**: 독립적인 레시피 정의, 직접 `displayPriority` 사용
- **ThingDef.recipeMaker**: 아이템 정의 내 레시피 정보, `recipeMaker` 섹션 내 `displayPriority` 사용

---

## 7. 결론 및 권장사항

### 7.1 현재 상태

- **림월드 코어**: 대부분의 의류에 `displayPriority`가 잘 정의되어 있음
- **Ratkin 프로젝트**: 대부분의 의류에 `displayPriority`가 **누락**되어 있음
- **영향**: 제작 메뉴에서 의류 순서가 일관되지 않을 수 있음

### 7.2 권장 조치사항

1. **1단계**: 모든 의류에 `displayPriority` 추가
   - 티어별로 적절한 범위 할당
   - 카테고리별로 논리적인 순서 유지

2. **2단계**: 테스트 및 조정
   - 게임 내 제작 메뉴 확인
   - 사용자 경험에 맞게 값 조정

3. **3단계**: 문서화
   - `displayPriority` 값 범위 가이드라인 문서화
   - 향후 의류 추가 시 참고용

### 7.3 참고사항

- `displayPriority`는 **낮을수록 우선순위가 높음**
- 값 사이에 여유를 두어 향후 추가/조정이 용이하도록 함
- 림월드 코어 패턴을 참고하되, 프로젝트 특성에 맞게 조정

---

## 8. 참고 자료

- **림월드 코어 의류**: `RimworldData/Core/Defs/ThingDefs_Misc/Apparel_Various.xml`
- **프로젝트 의류**: `Project/1.6/Defs/ThingsDefs/Apparel_*.xml`
- **RecipeDef 예시**: `Project/1.6/Defs/Things_ItemDefs/Thing_Food.xml`

---

**작성자**: AI Assistant  
**검토 필요**: displayPriority 값 범위 최종 확인 필요
