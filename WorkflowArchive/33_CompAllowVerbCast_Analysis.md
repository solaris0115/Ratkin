# CompAllowVerbCast 분석 - 2025-11-07

## 작업 개요
- 요청 내용: @CompShieldWeaponIncompatible.cs (43) 다른 액션의 verb도 실행 불가임?
- 목표: `CompShieldWeaponIncompatible`의 `CompAllowVerbCast` 로직을 분석하여 다른 액션 verb 차단 여부를 확인한다.

## 계획 (AI가 결정한 계획)
1. `CompShieldWeaponIncompatible`의 `CompAllowVerbCast` 구현을 검토한다.
2. 방패 착용 조건에서 허용/차단되는 verb 범위를 분석한다.
3. 분석 결과와 영향 범위를 정리한다.

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. `CompShieldWeaponIncompatible`의 `CompAllowVerbCast` 구현을 검토한다.
2. 방패 착용 조건에서 허용/차단되는 verb 범위를 분석한다.
3. 분석 결과와 영향 범위를 정리한다.

## 작업 세부 진행
1. `CompAllowVerbCast` 코드 구조 파악 [v]
2. 차단 로직이 적용되는 verb 범위 분석 [v]
3. 분석 결과 정리 초안 작성 [v]
4. Primary 무기 verb만 태그 검사하도록 `CompAllowVerbCast` 수정 [v]
5. 착용 시 허용 태그 검증 로직 추가 [v]
6. Verb/착용 실패 원인 로그 추가 및 공유 [v]

## 진행 상황
### 1. `CompAllowVerbCast` 코드 구조 파악
- 내용: 컴포넌트 속성과 방패 착용/무기 보유 여부에 따른 조기 탈출 조건을 확인했다.
- 결과: Props, Apparel, Wearer, Primary 무기, 허용 태그 존재 여부 등에 대한 조건부 true 반환을 파악했다.

### 2. 차단 로직이 적용되는 verb 범위 분석
- 내용: verb 파라미터 미사용, Primary 무기 태그 검사 후 미허용 시 false 반환 흐름을 확인했다.
- 결과: 허용 태그 미보유 무기의 경우 모든 verb 호출이 차단됨을 파악했다.

### 3. 분석 결과 정리 초안 작성
- 내용: verb 파라미터 미사용으로 인해 일괄 차단됨을 정리하고, 허용 태그 미존재 시 모든 verb가 막히는 점을 도출했다.
- 결과: 사용자 전달용 결론 및 주의 사항 초안 작성 완료.

### 4. Primary 무기 verb만 태그 검사하도록 `CompAllowVerbCast` 수정
- 내용: verb의 `EquipmentSource`를 확인하도록 업데이트하여 Primary 무기 verb만 태그 검증을 수행한다.
- 결과: Ability·Gene verb는 그대로 허용되고 Primary 무기 verb는 허용 태그 조건을 따르도록 수정 완료.

### 5. 착용 시 허용 태그 검증 로직 추가
- 내용: `Apparel.PawnCanWear`에 대한 Harmony Postfix 패치를 추가해 컴포넌트의 `IsWeaponAllowed` 결과에 따라 착용 가능 여부를 판정하도록 구현했다.
- 결과: 허용 태그 미보유 무기를 든 상태에는 착용 메뉴가 비활성화된다.

### 6. Verb/착용 실패 원인 로그 추가 및 공유
- 내용: 허용 여부와 이유를 반환하는 메서드를 도입하고, Verb 차단·착용 허용/거부 시 로그를 남기도록 구현했다.
- 결과: 허용 태그 미충족 원인을 로그를 통해 즉시 확인 가능하다.

## 최종 작업 결과/ 중단 사유
- 완료: Primary 무기 verb만 차단 로직을 적용하고, 착용 시에도 허용 태그 검증이 이루어지도록 구현을 마쳤다.

## 관련 파일 목록
- [CompShieldWeaponIncompatible.cs](Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs)

## 참고사항
- RimWorld의 `ThingComp.CompAllowVerbCast` 반환값이 false인 경우 해당 컴프가 장착된 아이템이 verb 시전을 차단한다.

