# NamerUniqueWeapon 사용처 및 플로우 분석 보고서

## 개요
`NamerUniqueWeapon`은 Odyssey DLC의 유니크 무기가 생성될 때 고유한 이름을 부여하기 위해 사용되는 RulePackDef입니다.

---

## 1. 요청하는 곳

### 1.1 직접 사용 위치
- **파일**: `RimworldSource/RimWorld/CompUniqueWeapon.cs`
- **메서드**: `CompUniqueWeapon.PostPostMake()`
- **라인**: 138
- **코드**:
```csharp
request.Includes.Add(RulePackDefOf.NamerUniqueWeapon);
this.name = NameGenerator.GenerateName(request, null, false, "r_weapon_name", null).StripTags();
```

### 1.2 RulePackDef 정의
- **파일**: `RimworldSource/RimWorld/RulePackDefOf.cs`
- **라인**: 179
- **정의**: `public static RulePackDef NamerUniqueWeapon;`

---

## 2. 각각의 이벤트 플로우

### 2.1 유니크 무기 생성 플로우

```
ThingSetMaker_UniqueWeapon.Generate()
    ↓
ThingMaker.MakeThing(ThingDef, StuffDef)
    ↓
ThingDef.MakeThing()
    ↓
ThingWithComps 생성
    ↓
CompUniqueWeapon 컴포넌트 초기화
    ↓
Thing.PostPostMake() 호출
    ↓
CompUniqueWeapon.PostPostMake() 실행
    ↓
NamerUniqueWeapon 사용하여 이름 생성
```

### 2.2 이름 생성 세부 플로우

```
CompUniqueWeapon.PostPostMake()
    ↓
InitializeTraits() - 무기 트레이트 초기화 (1~3개)
    ↓
품질 설정 (Super 등급)
    ↓
무기 색상 랜덤 선택
    ↓
GrammarRequest 구성:
    - weapon_type: Props.namerLabels에서 랜덤 선택
    - color: 선택된 색상 라벨
    - trait_adjective: 트레이트 형용사
    - ANYPAWN: 랜덤 Pawn 이름
    ↓
request.Includes.Add(RulePackDefOf.NamerUniqueWeapon)
    ↓
NameGenerator.GenerateName(request, ..., "r_weapon_name", ...)
    ↓
GrammarResolver.Resolve("r_weapon_name", request)
    ↓
NamerUniqueWeapon RulePack 규칙 적용
    ↓
최종 이름 문자열 생성
    ↓
CompArt.Title에 이름 저장 (있는 경우)
```

---

## 3. 유니크 무기 생성 시 전체 플로우

### 3.1 생성 트리거
- **ThingSetMaker_UniqueWeapon**: 보상/상인/퀘스트 등에서 유니크 무기 생성
- **조건**: `ModsConfig.OdysseyActive == true`
- **필터**: `HasComp<CompUniqueWeapon>()`인 ThingDef만 선택

### 3.2 생성 단계별 상세

#### Stage 1: ThingSetMaker에서 선택
```csharp
// ThingSetMaker_UniqueWeapon.Generate()
ThingDef 선택: CompUniqueWeapon을 가진 모든 ThingDef 중 랜덤
```

#### Stage 2: Thing 생성
```csharp
// ThingMaker.MakeThing()
ThingWithComps 인스턴스 생성
컴포넌트 초기화 (CompUniqueWeapon 포함)
```

#### Stage 3: PostPostMake 호출
```csharp
// Thing.PostPostMake()
→ CompUniqueWeapon.PostPostMake()
```

#### Stage 4: 트레이트 초기화
```csharp
InitializeTraits()
- 1~3개의 WeaponTraitDef 랜덤 선택
- weaponCategories와 호환되는 트레이트만 선택
- 중복/충돌 체크
```

#### Stage 5: 속성 설정
```csharp
- 품질: Super 등급 고정
- 색상: ColorType.Weapon 중 랜덤 선택
- 트레이트 강제 색상이 있으면 덮어쓰기
```

#### Stage 6: 이름 생성 준비
```csharp
GrammarRequest 구성:
- weapon_type: Props.namerLabels 랜덤 (예: "revolver", "rifle")
- color: 색상 라벨
- trait_adjective: 트레이트 형용사 리스트에서 랜덤
- ANYPAWN: TaleData_Pawn.GenerateRandom()로 생성
```

#### Stage 7: NamerUniqueWeapon 적용
```csharp
request.Includes.Add(RulePackDefOf.NamerUniqueWeapon)
NameGenerator.GenerateName(request, ..., "r_weapon_name", ...)
```

#### Stage 8: 최종 이름 저장
```csharp
this.name = 생성된 이름 문자열
CompArt.Title = this.name (CompArt 있는 경우)
```

---

## 4. 이름 생성 규칙 (NamerUniqueWeapon)

### 4.1 주요 규칙 패턴
- `r_weapon_name(p=2) -> [weapon_adjective] [weapon_noun]`
- `r_weapon_name(p=0.5) -> [badass_concept]의 [weapon_type]`
- `r_weapon_name(p=0.5) -> [weapon_adjective] [weapon_type]`
- `r_weapon_name(p=0.3) -> [ANYPAWN_nameIndef]의 [weapon_noun]`

### 4.2 사용되는 변수
- `weapon_type`: 무기 타입 (revolver, rifle 등)
- `weapon_adjective`: 트레이트 형용사 또는 badass_adjective
- `weapon_noun`: 무기 타입 또는 badass_noun
- `badass_concept`: 정의, 복수, 고통 등
- `ANYPAWN_nameIndef`: 랜덤 Pawn 이름

---

## 5. 관련 파일

### 5.1 소스코드
- `RimworldSource/RimWorld/CompUniqueWeapon.cs` - 유니크 무기 컴포넌트
- `RimworldSource/RimWorld/CompProperties_UniqueWeapon.cs` - 컴포넌트 속성
- `RimworldSource/RimWorld/ThingSetMaker_UniqueWeapon.cs` - 유니크 무기 생성기
- `RimworldSource/RimWorld/RulePackDefOf.cs` - RulePackDef 참조

### 5.2 Def 파일
- `RimworldData/Odyssey/Defs/RulePackDefs/RulePacks_Namers_UniqueWeapons.xml` - RulePackDef 정의
- `RimworldData/Odyssey/Defs/ThingDefs_Items/Weapons_Unique.xml` - 유니크 무기 정의 (13개)

### 5.3 번역 파일
- `RimworldData/Odyssey/Languages/Korean (한국어)/DefInjected/RulePackDef/RulePacks_Namers_UniqueWeapons.xml` - 한국어 번역

---

## 6. 요약

### 6.1 사용 시점
- 유니크 무기가 생성될 때 (`PostPostMake()`)
- 게임 내에서 한 번만 실행됨 (생성 시점)

### 6.2 생성 경로
1. ThingSetMaker_UniqueWeapon → 보상/상인/퀘스트
2. ThingMaker.MakeThing() → 무기 인스턴스 생성
3. CompUniqueWeapon.PostPostMake() → 이름 생성

### 6.3 이름 생성 요소
- 무기 타입 (namerLabels)
- 무기 색상
- 무기 트레이트 (1~3개)
- 랜덤 Pawn 이름 (선택적)

---

## 참고사항
- Odyssey DLC가 활성화되어 있어야 동작함
- 유니크 무기는 `thingSetMakerTags`에 `UniqueWeapon` 태그를 가짐
- 생성된 이름은 저장/로드 시 유지됨 (`PostExposeData()`)
