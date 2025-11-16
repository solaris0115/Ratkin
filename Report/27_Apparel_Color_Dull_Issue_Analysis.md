# 랫킨 의상 색상 칙칙함 문제 분석 보고서

## 문제 상황
모든 랫킨 의상이 칙칙하게 나타나는 문제가 발생했습니다.

## 원인 분석

### 1. RimWorld 기본 의상과의 비교

#### RimWorld 기본 의상 (Apparel_Various.xml)
```xml
<ThingDef Name="ApparelBase" ParentName="ApparelNoQualityBase" Abstract="True">
  <!-- ignoreThingDrawColor 설정 없음 (기본값: false) -->
  <comps>
    <li>
      <compClass>CompColorable</compClass>
    </li>
  </comps>
</ThingDef>

<!-- 기본 의상 예시 -->
<ThingDef ParentName="ApparelMakeableBase">
  <defName>Apparel_Pants</defName>
  <!-- ... -->
  <colorGenerator Class="ColorGenerator_StandardApparel" />
</ThingDef>
```

**특징:**
- `ignoreThingDrawColor` 설정이 없음 (기본값 `false`)
- 대부분의 의상에 `colorGenerator`가 설정됨
  - `ColorGenerator_StandardApparel`: 표준 의상 색상 생성기
  - `ColorGenerator_Options`: 특정 색상 옵션 제공

#### Ratkin 의상 (Apparel_Various.xml)
```xml
<ThingDef Name="RK_ApparelBase" Abstract="True">
  <graphicData>
    <!--소재로 인한 컬러링 무시를 디폴트로.-->	
    <ignoreThingDrawColor>true</ignoreThingDrawColor>
  </graphicData>
  <comps>
    <li>
      <compClass>CompColorable</compClass>
    </li>
  </comps>
</ThingDef>

<!-- 대부분의 Ratkin 의상 -->
<ThingDef ParentName="RK_ApparelMakeableBase">
  <defName>RK_ApronSkirt</defName>
  <!-- colorGenerator 없음 -->
</ThingDef>
```

**특징:**
- `ignoreThingDrawColor`가 `true`로 설정됨
- 대부분의 의상에 `colorGenerator`가 없음
- 일부 의상만 `colorGenerator` 보유 (RK_Muffler, RK_Cardigan, RK_WoolenHat, RK_StrawHat 등)

### 2. 문제의 근본 원인

#### `ignoreThingDrawColor`와 `colorGenerator`의 상호작용

1. **`ignoreThingDrawColor = true`의 의미:**
   - 텍스처의 원본 색상이 그대로 표시됨
   - 소재(Stuff)의 색상이 텍스처에 적용되지 않음
   - 하지만 `CompColorable`이 있으면 색상 변경 기능은 여전히 활성화됨

2. **`colorGenerator`가 없을 때의 문제:**
   - 게임이 의상을 생성할 때 기본 색상을 결정할 수 없음
   - `CompColorable`이 있지만 색상 생성기가 없으면 기본값이 명확하지 않음
   - 결과적으로 텍스처의 원본 색상이 그대로 표시되거나, 기본 회색/칙칙한 색상으로 표시됨

3. **RimWorld의 기본 동작:**
   - `ignoreThingDrawColor = false` (기본값)일 때: 소재의 색상이 텍스처에 적용됨
   - `colorGenerator`가 있으면: 생성기가 지정한 색상이 적용됨
   - 둘 다 없으면: 기본적으로 밝고 선명한 색상이 적용됨

### 3. 현재 Ratkin 의상의 설정 상태

#### `colorGenerator`가 있는 의상 (정상 작동 예상)
- `RK_StrawHat`: `ColorGenerator_Options` 사용
- `RK_Muffler`: `ColorGenerator_Options` 사용  
- `RK_Cardigan`: `ColorGenerator_Options` 사용
- `RK_WoolenHat`: `ColorGenerator_Options` 사용

#### `colorGenerator`가 없는 의상 (칙칙함 문제 발생)
- `RK_ApronSkirt`
- `RK_SummerDress`
- `RK_WorkerWear`
- `RK_ExplorerWear`
- `RK_ChefSuit`
- 기타 대부분의 의상들

## 해결 방안

### 옵션 1: 기본 색상 생성기 추가 (권장)
모든 Ratkin 의상에 `ColorGenerator_StandardApparel` 추가:

```xml
<ThingDef Name="RK_ApparelMakeableBase" ParentName="RK_ApparelBase" Abstract="True">
  <!-- ... 기존 설정 ... -->
  <colorGenerator Class="ColorGenerator_StandardApparel" />
</ThingDef>
```

**장점:**
- RimWorld 기본 의상과 동일한 색상 시스템 사용
- 밝고 선명한 색상 자동 생성
- 일관성 있는 색상 적용

### 옵션 2: ignoreThingDrawColor 제거
`RK_ApparelBase`에서 `ignoreThingDrawColor` 설정 제거:

```xml
<ThingDef Name="RK_ApparelBase" Abstract="True">
  <graphicData>
    <onGroundRandomRotateAngle>35</onGroundRandomRotateAngle>
    <!-- ignoreThingDrawColor 제거 -->
  </graphicData>
  <!-- ... -->
</ThingDef>
```

**장점:**
- 소재의 색상이 텍스처에 자연스럽게 적용됨
- 추가 설정 없이도 색상이 적용됨

**단점:**
- 소재 색상이 텍스처를 덮어쓸 수 있음 (원하지 않는 결과 가능)

### 옵션 3: 개별 의상에 colorGenerator 추가
각 의상에 적절한 색상 생성기 개별 설정:

```xml
<ThingDef ParentName="RK_ApparelMakeableBase">
  <defName>RK_ApronSkirt</defName>
  <!-- ... -->
  <colorGenerator Class="ColorGenerator_StandardApparel" />
</ThingDef>
```

**장점:**
- 세밀한 색상 제어 가능
- 의상별로 다른 색상 생성기 사용 가능

**단점:**
- 작업량이 많음
- 일관성 유지가 어려움

## 권장 해결책

**옵션 1 (기본 색상 생성기 추가)**을 권장합니다:

1. `RK_ApparelMakeableBase`에 `colorGenerator` 추가
2. 필요시 개별 의상에서 오버라이드 가능
3. RimWorld 기본 동작과 일치하여 예측 가능한 결과

## 참고 파일

- **RimWorld 기본 의상**: `RimWorldData/Core/Defs/ThingDefs_Misc/Apparel_Various.xml`
- **Ratkin 의상**: `Project/1.6/Defs/ThingsDefs/Apparel_Various.xml`
- **색상 생성기 사용 예시**: `RK_StrawHat`, `RK_Muffler`, `RK_Cardigan`, `RK_WoolenHat`

## 결론

랫킨 의상이 칙칙하게 나타나는 원인은:
1. `ignoreThingDrawColor = true` 설정으로 인해 소재 색상이 적용되지 않음
2. 대부분의 의상에 `colorGenerator`가 없어 기본 색상이 생성되지 않음
3. 결과적으로 텍스처의 원본 색상이 그대로 표시되거나 칙칙한 색상으로 표시됨

**해결책**: `RK_ApparelMakeableBase`에 `<colorGenerator Class="ColorGenerator_StandardApparel" />` 추가하여 RimWorld 기본 의상과 동일한 색상 시스템을 사용하도록 수정













