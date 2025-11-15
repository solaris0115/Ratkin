# WyvernFire 버너 커스터마이징 - 2025-11-14

## 작업 개요
- 요청 내용: @Weapon_HighTech.xml (237-247) WyvernFire 전용 `CompProperties_AbilityBurner`/`CompAbilityEffect_Burner` 새로 작성 및 커스터마이징
- 목표: WyvernFire 무기를 위한 전용 버너 컴프/이펙트 정의 및 적용

## 계획 (AI가 결정한 계획)
1. 기존 버너 컴프/이펙트 구조와 사용처 파악
2. WyvernFire 전용 클래스/Def 설계 및 구현
3. WyvernFire 무기에 새로운 컴프/이펙트 연결 및 검증

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. 기존 버너 컴프/이펙트 구조와 사용처 파악
2. WyvernFire 전용 클래스/Def 설계 및 구현
3. WyvernFire 무기에 새로운 컴프/이펙트 연결 및 검증

## 작업 세부 진행
1. 구조 파악 [v]
2. 전용 클래스/Def 구현 [v]
3. 무기 연결 및 검증 [v]
4. 시각 효과/오프셋 보정 작업 [v]

## 진행 상황
### 1. 구조 파악
- 내용: RimWorld 기본 `CompProperties_AbilityBurner`/`CompAbilityEffect_Burner` 구조와 동작 방식 확인
- 결과: 코드 파악 완료, 커스터마이징 포인트 정리

### 2. 전용 클래스/Def 구현
- 내용: `CompAbilityEffect_WyvernFireBurner` 로직을 새로 작성해 시야 검사/스트림 생성 로직을 이해 가능한 형태로 구현, `CompProperties_AbilityWyvernFireBurner` 정리
- 결과: WyvernFire 전용 코드/프로퍼티 구현 완료

### 3. 무기 연결 및 검증
- 내용: `Weapon_HighTech.xml`에서 WyvernFire 능력이 신규 컴프를 사용하도록 갱신
- 결과: Def 갱신 완료

### 4. 시각 효과/오프셋 보정
- 내용: `lineWidthEnd` 기반 콘 각도 계산을 버너 시각효과에 도입, `lineWidthEnd`/`minConeAngleDegrees` 프로퍼티 추가 후 Def와 코드에 반영
- 결과: WyvernFire 판정 폭과 시각효과 일치하도록 보정 완료

## 최종 작업 결과/ 중단 사유
WyvernFire 전용 버너 컴프/이펙트 구현 및 Def 적용 완료

## 관련 파일 목록
- `Project/1.6/Source`
- `Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml`

## 참고사항
- 없음

