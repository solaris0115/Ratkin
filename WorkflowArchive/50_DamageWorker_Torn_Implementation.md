# DamageWorker_Torn 구현 - 2024-12-19

## 작업 개요
- 요청 내용: `DamageWorker_Cut`를 커스텀 `DamageWorker_Torn`으로 변경하여 구현
- 목표: Cut과 동일하게 구현하되, 현재 파라미터들(`cutExtraTargetsCurve`, `cutCleaveBonus` 등)을 모두 활용할 수 있도록 구현

## 계획 (AI가 결정한 계획)
1. `DamageWorker_Cut` 클래스 구현 확인
2. `DamageWorker_Torn` 클래스 생성 (Cut과 동일한 구현)
3. XML 파일에서 `workerClass`를 `DamageWorker_Torn`으로 변경

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. `DamageWorker_Torn` 클래스 생성 (Cut과 동일한 구현)
2. XML 파일에서 `workerClass`를 `DamageWorker_Torn`으로 변경

## 작업 세부 진행
1. [DamageWorker_Torn 클래스 생성] [v]
2. [XML 파일 workerClass 변경] [v]
3. [컬렉션 수정 버그 수정] [v]

## 진행 상황
### 1. DamageWorker_Torn 클래스 생성
- 내용: `DamageWorker_Cut`와 동일한 구현으로 `DamageWorker_Torn` 클래스 생성
- 결과: `Project/1.6/Source/DamageWorker_Torn.cs` 파일 생성 완료
- 구현 내용:
  - `DamageWorker_AddInjury`를 상속
  - `ChooseHitPart` 메서드: Cut과 동일하게 구현
  - `ApplySpecialEffectsToPart` 메서드: Cut과 동일하게 구현
  - `cutExtraTargetsCurve`와 `cutCleaveBonus` 파라미터 활용

### 2. XML 파일 workerClass 변경
- 내용: `Weapon_HighTech.xml`의 `RK_Damage_Torn`에서 `workerClass`를 `DamageWorker_Torn`으로 변경
- 결과: 변경 완료 (560번째 줄)

### 3. 컬렉션 수정 버그 수정
- 내용: `System.InvalidOperationException: Collection was modified` 에러 수정
- 문제: `IEnumerable` 지연 평가로 인해 순회 중 컬렉션이 수정될 수 있음
- 해결: 모든 컬렉션을 먼저 `List`로 변환하여 즉시 평가하도록 수정
- 변경 사항:
  - `IEnumerable.Concat()` 대신 `List.AddRange()` 사용
  - `Except()` 대신 `where x != dinfo.HitPart` 사용
  - 모든 부위 컬렉션을 순회 전에 리스트로 변환
- 결과: 버그 수정 완료

## 최종 작업 결과/ 중단 사유
완료: `DamageWorker_Torn` 클래스를 생성하고 XML 파일의 `workerClass`를 변경했습니다. 현재 Cut과 동일하게 구현되어 있으며, XML에 정의된 모든 파라미터(`cutExtraTargetsCurve`, `cutCleaveBonus` 등)를 활용할 수 있습니다.

추가로 `Collection was modified` 에러를 수정하여 공격 시 안정적으로 동작하도록 개선했습니다.

## 관련 파일 목록
- [DamageWorker_Torn.cs](Project/1.6/Source/DamageWorker_Torn.cs)
- [Weapon_HighTech.xml](Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml)

## 참고사항
- `DamageWorker_Cut`는 `cutExtraTargetsCurve`와 `cutCleaveBonus` 파라미터를 사용
- 현재 XML에 정의된 파라미터들이 모두 동작하도록 구현 필요

