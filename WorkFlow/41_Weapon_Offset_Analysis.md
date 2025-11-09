# Weapon Offset Analysis - 2025-11-09

## 작업 개요
- 요청 내용: @Weapon_Melee.xml (827-830) 드로잉 길이 관련해서 조정 가능해? 아니면 포지션 offset 무기로 공격시 equippedAngleOffset 이걸 쓰는 것 같은데, 오프셋 조정이 가능했으면 좋겠어. 로직에서 확인좀. 림월드 소스코드에서
- 목표: RimWorld 및 모드 소스에서 무기 장착 각도/포지션 오프셋 조정 가능 여부를 파악하고 적용 가능한 방법을 정리

## 계획 (AI가 결정한 계획)
1. 관련 가이드 및 기존 분석 참고하여 규칙 준수 여부 확인
2. `Weapon_Melee.xml` 및 연관 C# 로직에서 `equippedAngleOffset` 처리 방식 조사
3. 적용 가능한 조정 방법 및 필요 조건 정리

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. 관련 규칙 재확인
2. RimWorld/모드 코드에서 `equippedAngleOffset` 및 위치 오프셋 처리 로직 조사
3. 적용 가능성 및 방법 정리

## 작업 세부 진행
1. 규칙 및 레퍼런스 확인 [v]
2. 소스 코드 조사 [v]
3. 결과 정리 [v]

## 진행 상황
### 1. 규칙 및 레퍼런스 확인
- 내용: 워크플로 규칙 및 기존 `28_Weapon_Drawing_Angle_Analysis` 문서 재확인
- 결과: 완료

### 2. 소스 코드 조사
- 내용: `PawnRenderUtility.DrawEquipmentAiming`, `ThingDef.equippedAngleOffset`, `equippedDistanceOffset` 로직 확인
- 결과: 완료

### 3. 결과 정리
- 내용: 각도/거리 오프셋 조정 가능 속성 및 적용 로직 정리
- 결과: 완료

## 최종 작업 결과/ 중단 사유
- RimWorld 소스 분석 완료 및 적용 방법 정리

## 관련 파일 목록
- `WorkFlow/41_Weapon_Offset_Analysis.md`

## 참고사항
- 없음

