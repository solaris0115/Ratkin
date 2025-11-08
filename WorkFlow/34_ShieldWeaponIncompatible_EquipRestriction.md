# 방패-무기 착용 제한 로직 설계 - 2025-11-07

## 작업 개요
- 요청 내용: @CompShieldWeaponIncompatible.cs allowedWeaponTags 이 태그가 없다면 동시 착용이 불가능하게 해줘. 이중 하나라도 있다면 착용 가능하지만 하나도 없다면 착용이 불가능해야되.
- 목표: `CompShieldWeaponIncompatible`에서 허용 태그가 없는 무기와는 방패 동시 착용을 막고, 허용 태그가 하나라도 있으면 착용을 허용하도록 로직 구현

## 계획 (AI가 결정한 계획)
1. RimWorld 원본 `Apparel.PawnCanWear` 비롯 착용 검증 흐름 재분석
2. 기본 `ThingComp`가 착용 가능 여부를 제어하는 사례 및 인터페이스 조사
3. 분석 결과 정리 및 모드 적용 전략 초안 작성

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. RimWorld 원본 `Apparel.PawnCanWear` 비롯 착용 검증 흐름 재분석
2. 기본 `ThingComp`가 착용 가능 여부를 제어하는 사례 및 인터페이스 조사
3. 분석 결과 정리 및 모드 적용 전략 초안 작성

## 작업 세부 진행
1. RimWorld 착용 검증 흐름 재분석 [v]
2. `ThingComp` 착용 제어 사례 조사 [v]
3. 분석 결과 및 적용 전략 정리 [ ]

## 진행 상황
### 1. RimWorld 착용 검증 흐름 재분석
- 내용: 원본 `Apparel.PawnCanWear` 및 `ApparelProperties.PawnCanWear` 구현 확인, 착용 가능한지 여부가 성별·발달단계 조건만 검사함을 확인
- 결과: 기본 게임에서는 무기/방패 호환성 같은 추가 조건이 착용 단계에서 평가되지 않음

### 2. `ThingComp` 착용 제어 사례 조사
- 내용: `ThingComp` 가상 메서드 검토, RimWorld 내 대표적인 컴프(`CompShield`) 동작 확인
- 이슈: 착용 차단을 직접 제공하는 콜백 없음, 대신 `CompAllowVerbCast` 등 착용 이후 동작 제한 방식 사용
- 결과: 방패/무기 동시 착용 불가는 기본 제공되지 않으며, 기존 Shield Belt 는 착용 가능하되 발사 시 `CompAllowVerbCast` 로 차단

### 3. 분석 결과 및 적용 전략 정리
- 내용: 착용 제어를 구현하려면 `Pawn_ApparelTracker.Wear` 흐름에 패치하거나 `Apparel.PawnCanWear` 반환값을 변경하는 별도 Harmony 패치 필요 여부 정리
- 결과: `Comp` 단독으로 착용 허용 여부를 제어하는 구조가 없어, 모드 측에서 Postfix/Prefix 패치로 무기 태그 검사 로직을 삽입해야 함

## 최종 작업 결과/ 중단 사유


## 관련 파일 목록
- 

## 참고사항
- 

