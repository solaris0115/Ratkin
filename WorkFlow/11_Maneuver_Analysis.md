# Maneuver 분석 - 2024-12-19

## 작업 개요
- 요청 내용: Maneuver가 림월드 소스코드/def에서 무엇을 의미하는지 분석
- 목표: Maneuver의 정의, 구조, 역할을 명확히 파악

## 계획 (AI가 결정한 계획)
1. 소스코드에서 Maneuver 관련 클래스 분석
2. Def 파일에서 ManeuverDef 구조 확인
3. ToolCapacityDef와의 관계 파악
4. 실제 사용 예시 확인
5. 전체적인 시스템 구조 정리

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. 소스코드에서 Maneuver 관련 클래스 분석
2. Def 파일에서 ManeuverDef 구조 확인
3. ToolCapacityDef와의 관계 파악
4. 실제 사용 예시 확인
5. 전체적인 시스템 구조 정리

## 작업 세부 진행
1. 소스코드 분석 [v]
2. Def 파일 분석 [v]
3. 관계 파악 [v]
4. 사용 예시 확인 [v]
5. 구조 정리 [v]

## 진행 상황
### 1. 소스코드 분석
- 내용: ManeuverDef.cs 파일 분석
- 결과: ManeuverDef는 Def를 상속받는 클래스로, 근접 전투 행동을 정의

### 2. Def 파일 분석
- 내용: Maneuvers.xml 파일들 분석
- 결과: 다양한 근접 공격 타입들(Slash, Stab, Smash, Scratch, Bite 등) 정의

### 3. 관계 파악
- 내용: ToolCapacityDef와 Tool의 관계 분석
- 결과: Tool이 capacities를 가지고, ManeuverDef가 requiredCapacity를 요구하는 구조

### 4. 사용 예시 확인
- 내용: 실제 동물과 무기에서의 사용 예시 확인
- 결과: 동물의 발톱, 이빨, 머리 등이 다양한 capacities를 가지고 해당 Maneuver 사용

### 5. 구조 정리
- 내용: 전체 시스템 구조 정리
- 결과: 완료

## 최종 작업 결과/ 중단 사유
완료 - Maneuver의 전체적인 구조와 역할을 명확히 파악

## 관련 파일 목록
- RimworldSource/Verse/ManeuverDef.cs
- RimWorldData/Core/Defs/Maneuvers/Maneuvers.xml
- RimWorldData/Core/Defs/ToolCapacityDefs/ToolCapacity.xml
- RimWorldData/Core/Defs/RulePackDefs/RulePacks_Maneuvers.xml

## 참고사항
- Maneuver는 근접 전투에서 사용되는 특정한 공격 행동을 정의
- ToolCapacityDef와 연결되어 어떤 도구가 어떤 공격을 할 수 있는지 결정
- 각 Maneuver는 Hit, Miss, Deflect, Dodge 상황에 대한 전투 로그 규칙을 가짐

