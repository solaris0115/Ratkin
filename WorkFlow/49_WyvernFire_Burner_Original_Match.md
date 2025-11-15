# WyvernFire Burner 오리지널 코드 매칭 - 2025-11-14

## 작업 개요
- 요청 내용: WyvernFire 커스텀 코드가 RimWorld 오리지널 CompAbilityEffect_Burner와 동작이 다름 (특히 오프셋)
- 목표: 오리지널 소스코드와 동일하게 동작하도록 수정

## 계획 (AI가 결정한 계획)
1. 오리지널과 커스텀 코드의 차이점 상세 분석
2. CompProperties_AbilityWyvernFireBurner 수정 (coneSizeDegrees 추가, 불필요한 속성 제거)
3. CompAbilityEffect_WyvernFireBurner 로직 수정 (동적 각도 계산 제거, effecter 로직 수정)
4. Weapon_HighTech.xml 설정 조정
5. 빌드 테스트

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. 오리지널과 커스텀 코드의 차이점 상세 분석
2. CompProperties_AbilityWyvernFireBurner 수정
3. CompAbilityEffect_WyvernFireBurner 수정
4. Weapon_HighTech.xml 설정 조정
5. 빌드 테스트

## 작업 세부 진행
1. 차이점 분석 [v]
2. CompProperties 수정 [v]
3. CompAbilityEffect 수정 [v]
4. XML 설정 수정 [v]
5. 빌드 테스트 [v]

## 진행 상황

### 1. 차이점 분석
- 내용: 오리지널과 커스텀 코드 비교 분석
- 결과: 

#### 주요 차이점 발견:

**1. 콘 각도 계산 방식**
- 오리지널: `Props.coneSizeDegrees` 고정값을 `-coneSizeDegrees ~ +coneSizeDegrees` 범위로 랜덤 사용
- 커스텀: `CalculateConeHalfAngle()` 메서드로 거리 기반 동적 계산

**2. CompProperties 속성 차이**
- 오리지널: `coneSizeDegrees` (float)
- 커스텀: `lineWidthEnd` (float), `minConeAngleDegrees` (float, 기본값 5f)
- 커스텀에 불필요한 속성이 추가됨

**3. Effecter 유지 시간**
- 오리지널: Line 64에서 100 틱
- 커스텀: Line 82에서 50 + 깨진 문자들 (컴파일 에러 가능성)

**4. 코드 구조**
- 오리지널: Line 50-65에서 if 조건문 안에 spray.Add()와 effecter 추가를 함께 처리
- 커스텀: Line 68-78에서 spray.Add() 후 Line 80-83에서 별도 if문으로 effecter 추가

**5. MoteDef 사용**
- 오리지널: ThingDefOf.Mote_IncineratorBurst 하드코딩
- 커스텀: props.moteDef ?? ThingDefOf.Mote_IncineratorBurst (커스터마이징 가능)

**6. Effecter null 체크**
- 오리지널: effecterDef가 항상 존재한다고 가정
- 커스텀: `if (props.effecterDef != null)` 체크 추가

### 2. CompProperties_AbilityWyvernFireBurner 수정
- 내용: 속성을 오리지널과 동일하게 수정
- 작업:
  - `lineWidthEnd`, `minConeAngleDegrees` 제거
  - `coneSizeDegrees` 추가
- 결과: 완료

### 3. CompAbilityEffect_WyvernFireBurner 수정
- 내용: 로직을 오리지널과 완전히 동일하게 수정
- 작업:
  - `BurnerProps` 속성을 `Props`로 변경 (new 키워드 사용)
  - `SpawnWyvernFireSpray` 메서드를 inline delegate로 변경
  - `CalculateConeHalfAngle()` 메서드 제거
  - 고정된 `coneSizeDegrees` 범위로 랜덤 각도 생성
  - effecter 추가 로직을 if 블록 안으로 이동
  - effecter 유지 시간을 100 틱으로 수정
  - 깨진 문자 제거
  - moteDef는 커스터마이징 가능하도록 유지 (null coalescence 사용)
- 결과: 완료

### 4. Weapon_HighTech.xml 설정 수정
- 내용: XML 설정을 새로운 속성에 맞게 조정
- 작업:
  - `<lineWidthEnd>5</lineWidthEnd>` 제거
  - `<minConeAngleDegrees>5</minConeAngleDegrees>` 제거
  - `<coneSizeDegrees>15</coneSizeDegrees>` 추가
- 결과: 완료

### 5. 빌드 테스트
- 내용: 수정된 코드 컴파일 확인
- 결과: 빌드 성공 (0 Warning, 0 Error)

## 최종 작업 결과

✅ **작업 완료**

WyvernFire 관련 커스텀 코드를 RimWorld 오리지널 `CompAbilityEffect_Burner`와 완전히 동일하게 수정했습니다.

**주요 변경사항:**
1. 동적 콘 각도 계산 제거 → 고정 `coneSizeDegrees` 사용
2. Effecter 유지 시간 100 틱으로 통일
3. 코드 구조를 오리지널과 동일하게 변경
4. 깨진 문자 수정 및 컴파일 에러 해결
5. moteDef 커스터마이징 기능은 유지

**오프셋 문제 해결:**
- 오리지널과 동일한 `barrelOffsetDistance` 계산 방식 적용
- `worldSource = drawPos + vector * this.Props.barrelOffsetDistance`로 통일

## 관련 파일 목록
- [CompProperties_AbilityWyvernFireBurner.cs](../Project/1.6/Source/WyvernFire/CompProperties_AbilityWyvernFireBurner.cs)
- [CompAbilityEffect_WyvernFireBurner.cs](../Project/1.6/Source/WyvernFire/CompAbilityEffect_WyvernFireBurner.cs)
- [Weapon_HighTech.xml](../Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml)

## 참고사항
- 오리지널 소스: [CompAbilityEffect_Burner.cs](../RimworldSource/RimWorld/CompAbilityEffect_Burner.cs)
- 오리지널 소스: [CompProperties_AbilityBurner.cs](../RimworldSource/RimWorld/CompProperties_AbilityBurner.cs)
- moteDef 커스터마이징 기능은 오리지널에 없던 기능이지만 유용하므로 유지함

