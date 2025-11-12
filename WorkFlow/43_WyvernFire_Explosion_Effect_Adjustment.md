# WyvernFire Explosion Effect Adjustment - 2025-11-11

## 작업 개요
- 요청 내용: 지면에 화염을 남기거나 하는 그런 부분 제거해줘. 오로지 폭발만 부채꼴로.
- 목표: `CompAbilityEffect_WyvernFire`가 부채꼴 폭발만 발생시키도록 하며 잔류 화염/필스/점화 등 2차 효과를 제거한다.

## 계획 (AI가 결정한 계획)
1. 기존 코드에서 잔류 화염 및 필스를 생성하는 로직 위치 파악
2. 제거 대상 로직 및 파라미터 정리, 변경 후 영향 검토
3. 코드 수정 및 기본값 정리 후 검증

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. 기존 코드에서 잔류 화염 및 필스를 생성하는 로직 위치 파악
2. 제거 대상 로직 및 파라미터 정리, 변경 후 영향 검토
3. 코드 수정 및 기본값 정리 후 검증

## 작업 세부 진행
1. 로직 분석 및 영향 범위 파악 [v]
2. 코드 수정 및 정리 [v]
3. 확인 및 마무리 [v]

## 진행 상황
### 1. 로직 분석 및 영향 범위 파악
- 내용: `CompAbilityEffect_WyvernFire.Apply`에서 `postExplosionSpawnThingDef`와 `flammabilityAttachFireChanceCurve`가 지면 잔류 효과를 담당함을 확인
- 결과: 잔류 효과 제거 시 해당 파라미터 정리 필요

### 2. 코드 수정 및 정리
- 내용: 지면 화염/필스 생성 파라미터를 제거하고 `chanceToStartFire`를 0으로 설정하여 폭발만 발생하도록 수정
- 이슈: 없음
- 결과: 완료

### 3. 확인 및 마무리
- 내용: 코드 검토로 잔류 효과 관련 파라미터가 모두 제거되었음을 확인
- 결과: 완료

## 최종 작업 결과/ 중단 사유
완료

## 관련 파일 목록
- `Project/1.6/Source/WyvernFire/CompAbilityEffect_WyvernFire.cs`
- `Project/1.6/Source/WyvernFire/CompProperties_AbilityWyvernFire.cs`

## 참고사항
- 없음

