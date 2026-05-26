# AlienRace 체형별 머리/바디 Offset 커스터마이징 보고서

<!-- AlienRace headOffset bodyOffset Female Thin bodyTypes headOffsetDirectional 커스터마이즈 -->

## 1. 적용 위치

| 대상 | 정의 위치 |
|------|-----------|
| 머리 offset | `ThingDef_AlienRace` → `alienRace.generalSettings.alienPartGenerator` |
| 바디 애드온 offset | `AlienRace.RaceSettings` → `universalBodyAddons` |

---

## 2. 머리 Offset (alienPartGenerator)

### 2-1. 기본값 (성별 분리)

```xml
<alienPartGenerator>
  <headOffset>(0.0, -0.09)</headOffset>
  <headFemaleOffset>(0.0, -0.08)</headFemaleOffset>
</alienPartGenerator>
```

- `headOffset`: Male/None
- `headFemaleOffset`: Female (미지정 시 headOffset 상속)

### 2-2. 방향·체형·헤드타입별 (headOffsetDirectional)

```xml
<headOffsetDirectional>
  <south>
    <offset>(0, 0)</offset>
    <bodyTypes>
      <Female>(0.02, 0.01)</Female>
      <Thin>(0.01, -0.02)</Thin>
    </bodyTypes>
    <headTypes>
      <Male_AverageNormal>(0, 0.01)</Male_AverageNormal>
    </headTypes>
  </south>
  <north>
    <offset>(0, 0)</offset>
    <bodyTypes>
      <Female>(0.01, 0)</Female>
      <Thin>(-0.01, -0.01)</Thin>
    </bodyTypes>
  </north>
  <east>
    <offset>(0, 0)</offset>
    <bodyTypes>
      <Female>(0.02, 0)</Female>
      <Thin>(0.01, -0.01)</Thin>
    </bodyTypes>
  </east>
</headOffsetDirectional>
```

- `south` / `north` / `east` / `west` (west 미지정 시 east 사용)
- `bodyTypes`: BodyTypeDef defName → `(x, y)` Vector2
- `headTypes`: HeadTypeDef defName → `(x, y)` Vector2
- Female 전용: `headFemaleOffsetDirectional` (구조 동일)

### 2-3. LifeStageAge별 오버라이드

`lifeStageAges` 내 `LifeStageAgeAlien`에서 동일 필드로 오버라이드 가능.

```xml
<lifeStageAges>
  <li>
    <minAge>0</minAge>
    <body>Human</body>
    <headOffset>(0, -0.09)</headOffset>
    <headFemaleOffset>(0, -0.08)</headFemaleOffset>
    <headOffsetDirectional>
      <south>
        <bodyTypes>
          <Female>(0.02, 0.01)</Female>
          <Thin>(0.01, -0.02)</Thin>
        </bodyTypes>
      </south>
    </headOffsetDirectional>
  </li>
</lifeStageAges>
```

---

## 3. 바디 애드온 Offset (universalBodyAddons)

꼬리, 귀 등 `universalBodyAddons`의 `offsets`에서 체형별 보정.

```xml
<universalBodyAddons>
  <li>
    <path>Things/Ratkin/BodyAddon/RK_Texture_Tail</path>
    <defaultOffset>Tail</defaultOffset>
    <offsets>
      <north>
        <offset>(-0.01, 0.1)</offset>
        <bodyTypes>
          <Female>(0, 0.02)</Female>
          <Thin>(0, -0.05)</Thin>
        </bodyTypes>
      </north>
      <south>
        <offset>(0, 0)</offset>
        <bodyTypes>
          <Female>(0.01, 0.01)</Female>
          <Thin>(0, -0.03)</Thin>
        </bodyTypes>
      </south>
      <east>
        <offset>(-0.1, -0.05)</offset>
        <bodyTypes>
          <Female>(-0.02, 0)</Female>
          <Thin>(-0.01, -0.02)</Thin>
        </bodyTypes>
      </east>
    </offsets>
  </li>
</universalBodyAddons>
```

- `bodyTypes`: `<BodyTypeDefName>(x,y)</BodyTypeDefName>`
- `portraitBodyTypes`: 초상화 전용 (선택)

---

## 4. 샘플: Ratkin에 Female/Thin 머리·바디 보정 적용

### ThingDef (Races_Rakinlike.xml)

```xml
<alienPartGenerator>
  <headOffset>(0.0, -0.09)</headOffset>
  <headFemaleOffset>(0.0, -0.07)</headFemaleOffset>
  <headOffsetDirectional>
    <south>
      <offset>(0, 0)</offset>
      <bodyTypes>
        <Female>(0.02, 0.02)</Female>
        <Thin>(0.01, -0.01)</Thin>
      </bodyTypes>
    </south>
    <north>
      <offset>(0, 0)</offset>
      <bodyTypes>
        <Female>(0.01, 0.01)</Female>
        <Thin>(0, -0.01)</Thin>
      </bodyTypes>
    </north>
    <east>
      <offset>(0, 0)</offset>
      <bodyTypes>
        <Female>(0.02, 0)</Female>
        <Thin>(0.01, -0.01)</Thin>
      </bodyTypes>
    </east>
  </headOffsetDirectional>
  <headFemaleOffsetDirectional>
    <south>
      <bodyTypes>
        <Female>(0.01, 0.02)</Female>
        <Thin>(0.01, -0.01)</Thin>
      </bodyTypes>
    </south>
  </headFemaleOffsetDirectional>
</alienPartGenerator>
```

### AlienRaceSettings (universalBodyAddons)

```xml
<offsets>
  <south>
    <offset>(-0.03, -0.35)</offset>
    <bodyTypes>
      <Female>(-0.01, -0.02)</Female>
      <Thin>(0, -0.01)</Thin>
    </bodyTypes>
  </south>
  <north>
    <offset>(0.03, -0.23)</offset>
    <bodyTypes>
      <Female>(0.01, -0.01)</Female>
      <Thin>(0, -0.02)</Thin>
    </bodyTypes>
  </north>
</offsets>
```

---

## 5. 참고

| 항목 | 설명 |
|------|------|
| offset 좌표 | (x, y) — x: 좌우, y: 상하 |
| bodyTypes 태그 | BodyTypeDef defName (Female, Thin, Male, Hulk, Fat, Baby, Child 등) |
| headTypes 태그 | HeadTypeDef defName (예: Male_AverageNormal) |
| west | 생략 시 east 값 사용 |
