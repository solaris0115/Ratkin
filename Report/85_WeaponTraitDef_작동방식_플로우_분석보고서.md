# WeaponTraitDef 작동 방식 플로우 분석 보고서

## 개요
WeaponTraitDef의 부착 플로우와 스탯 적용 메커니즘을 간결하게 정리한 참고용 보고서입니다.

---

## 1. WeaponTraitDef 부착 플로우

### 1.1 호출 트리거
**이벤트**: 유니크 무기 생성 시점
- **트리거**: `ThingSetMaker_UniqueWeapon.Generate()` (보상/상인/퀘스트 등)
- **조건**: `ModsConfig.OdysseyActive == true` && `HasComp<CompUniqueWeapon>()`

### 1.2 부착 플로우
```
ThingSetMaker_UniqueWeapon.Generate()
    ↓
ThingMaker.MakeThing(ThingDef, StuffDef)
    ↓
Thing.PostPostMake()
    ↓
CompUniqueWeapon.PostPostMake()
    ↓
CompUniqueWeapon.InitializeTraits()
    ↓
1~3개 WeaponTraitDef 랜덤 선택
- weaponCategories 호환성 체크
- exclusionTags 충돌 체크
- commonality 가중치 적용
```

### 1.3 주요 함수
- **초기화**: `CompUniqueWeapon.PostPostMake()` → `InitializeTraits()`
- **선택 로직**: `CanAddTrait()` - 카테고리/제외태그/중복 체크
- **부착**: `AddTrait()` - 리스트에 추가

---

## 2. 스탯 적용 메커니즘

### 2.1 적용 방식
**statPart 검사 방식이 아닌 ThingComp 오버라이드 방식**

```
StatWorker.GetValue(StatRequest req)
    ↓
req.Thing이 ThingWithComps인 경우
    ↓
모든 ThingComp 순회:
    num += comp.GetStatOffset(stat)  // 오프셋 합산
    num *= comp.GetStatFactor(stat)   // 배수 곱셈
    ↓
CompUniqueWeapon.GetStatOffset() / GetStatFactor() 호출
    ↓
모든 WeaponTraitDef 순회:
    statOffsets 합산 / statFactors 곱셈
```

### 2.2 주요 함수
- **스탯 계산**: `StatWorker.GetValue()` (라인 360-368)
- **오프셋 적용**: `CompUniqueWeapon.GetStatOffset()` (라인 243-251)
- **배수 적용**: `CompUniqueWeapon.GetStatFactor()` (라인 253-261)

### 2.3 적용 위치
- **파일**: `RimworldSource/RimWorld/StatWorker.cs`
- **라인**: 360-368 (ThingComp 순회 부분)
- **특징**: ThingComp의 가상 메서드를 통한 확장 가능한 구조

---

## 3. 근접 무기 및 다른 무기 타입 적용 가능성

### 3.1 현재 지원 여부
**이미 지원됨**
- `CompUniqueWeapon.SpecialDisplayStats()` (라인 348)에서 `IsMeleeWeapon` 체크 존재
- 근접/원거리 무기 모두 동일한 메커니즘으로 작동

### 3.2 구현 필요사항
**statOffsets/statFactors만으로 충분**

1. **ThingDef에 CompUniqueWeapon 추가**
   ```xml
   <comps>
     <li Class="RimWorld.CompProperties_UniqueWeapon">
       <weaponCategories>
         <li>Melee</li>
       </weaponCategories>
     </li>
   </comps>
   ```

2. **WeaponTraitDef에 weaponCategory 지정**
   ```xml
   <WeaponTraitDef>
     <weaponCategory>Melee</weaponCategory>
     <statOffsets>
       <MeleeWeapon_AverageDPS>10</MeleeWeapon_AverageDPS>
     </statOffsets>
   </WeaponTraitDef>
   ```

### 3.3 추가 구현 불필요
- **statPart 추가 불필요**: ThingComp 오버라이드로 이미 처리됨
- **별도 컴포넌트 불필요**: CompUniqueWeapon이 범용적으로 작동
- **Worker 클래스 확장**: 필요시 `WeaponTraitWorker` 상속하여 특수 로직 추가 가능

---

## 4. 핵심 요약

### 4.1 부착 플로우
- **트리거**: 유니크 무기 생성 시 (`ThingSetMaker_UniqueWeapon`)
- **시점**: `PostPostMake()` 단계
- **선택**: `InitializeTraits()`에서 랜덤 선택

### 4.2 스탯 적용
- **방식**: ThingComp 가상 메서드 오버라이드
- **위치**: `StatWorker.GetValue()`에서 자동 호출
- **검사**: statPart 검사 없이 컴포넌트 존재 시 자동 적용

### 4.3 확장성
- **근접 무기**: 이미 지원 (weaponCategory만 지정)
- **구현 난이도**: 낮음 (Def만 수정)
- **추가 작업**: statPart 불필요, CompUniqueWeapon 범용 사용

---

## 관련 파일
- `RimworldSource/RimWorld/CompUniqueWeapon.cs` - 컴포넌트 구현
- `RimworldSource/RimWorld/StatWorker.cs` - 스탯 계산 (라인 360-368)
- `RimworldSource/RimWorld/WeaponTraitDef.cs` - 트레이트 정의 클래스
- `RimworldData/Odyssey/Defs/WeaponTraitDefs/WeaponTraitDefs.xml` - 트레이트 정의
