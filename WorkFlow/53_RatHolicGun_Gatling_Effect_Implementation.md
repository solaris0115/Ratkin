# RatHolic Gun 게틀링 효과 구현 - 2025-01-XX

## 작업 개요
- 요청 내용: RatHolic Gun을 게틀링 총처럼 작동하도록 구현. 매 발사마다 사용자의 원거리 무기 재장전 속도를 줄여서 발사 간격이 점차 줄어드는 효과
- 목표: 게틀링 총 효과 구현 (발사마다 재장전 속도 감소, 최대 5중첩)

## 계획 (AI가 결정한 계획)
1. **HediffDef 생성**: 재장전 속도 감소 Hediff 정의
   - RangedWeapon_Cooldown을 -0.3초씩 감소시키는 Hediff
   - 최대 5중첩 가능 (총 -1.5초 감소)
   - 중첩 가능한 Hediff로 설정

2. **Verb_RatHolicGun 생성**: 커스텀 Verb 클래스 구현
   - Verb_Shoot을 상속받아 동일한 기능 유지
   - TryCastShot() 성공 시 사용자에게 Hediff 부여
   - Hediff 중첩 관리 (최대 5중첩)

3. **Comp_RatHolicGun 생성**: 장착/해제 시 Hediff 관리 Comp
   - Notify_Equipped: 무기 장착 시 기존 Hediff 제거
   - Notify_Unequipped: 무기 해제 시 Hediff 제거

4. **Weapon_Range.xml 수정**: RatHolic Gun에 Comp 및 Verb 설정 추가

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
[사용자 승인 대기]

## 작업 세부 진행
1. HediffDef 파일 생성 [v]
2. Verb_RatHolicGun.cs 생성 [v]
3. Comp_RatHolicGun.cs 생성 [v]
4. Weapon_Range.xml 수정 [v]
5. StatPart_RatHolicGunSpooling.cs 생성 [v]
6. RangedCooldownFactor StatDef Patch 파일 생성 [v]
7. 빌드 및 테스트 [-]

## 진행 상황
### 1. HediffDef 파일 생성
- 내용: `RK_Hediff_RatHolicGunSpooling` HediffDef 생성
- RangedWeapon_Cooldown에 -0.3 오프셋 적용
- 파일: `Project/1.6/Defs/HediffDefs/Hediffs_RatHolicGun.xml`
- 결과: 완료

### 2. Verb_RatHolicGun.cs 생성
- 내용: `Verb_Shoot`을 상속받아 `TryCastShot()` 오버라이드
- 발사 성공 시 Hediff 추가 로직 구현
- 최대 5중첩 제한 구현
- 파일: `Project/1.6/Source/RatHolicGun/Verb_RatHolicGun.cs`
- 결과: 완료

### 3. Comp_RatHolicGun.cs 생성
- 내용: `ThingComp`를 상속받아 `Notify_Equipped`/`Notify_Unequipped` 구현
- 장착/해제 시 모든 Spooling Hediff 제거
- 파일: `Project/1.6/Source/RatHolicGun/Comp_RatHolicGun.cs`
- 결과: 완료

### 4. Weapon_Range.xml 수정
- 내용: RatHolic Gun의 `verbClass`를 `Verb_RatHolicGun`으로 변경
- `comps`에 `CompProperties_RatHolicGun` 추가
- 파일: `Project/1.6/Defs/ThingsDefs/Weapon_Range.xml`
- 결과: 완료

### 5. StatPart_RatHolicGunSpooling.cs 생성
- 내용: `StatPart`를 상속받아 `TransformValue()` 오버라이드
- Hediff의 Severity를 읽어서 정확히 0.3초씩 cooldown 감소
- 무기 cooldown을 직접 읽어서 factor 감소량 계산
- 최적화: Hediff가 없으면 즉시 반환
- 파일: `Project/1.6/Source/RatHolicGun/StatPart_RatHolicGunSpooling.cs`
- 결과: 완료

### 6. RangedCooldownFactor StatDef Patch 파일 생성
- 내용: `RangedCooldownFactor` StatDef에 `StatPart_RatHolicGunSpooling` 추가
- 파일: `Project/1.6/Defs/Stats/Stats_Pawns_Combat_Patch.xml`
- 결과: 완료

### 7. HediffDef 수정
- 내용: `statFactors` 제거 (StatPart에서 처리하므로 불필요)
- 표시용으로만 사용
- 파일: `Project/1.6/Defs/HediffDefs/Hediffs_RatHolicGun.xml`
- 결과: 완료

### 8. Verb_RatHolicGun.cs 수정
- 내용: 여러 Hediff 인스턴스 대신 하나의 Hediff에 Severity로 중첩 관리
- Severity: 0.2 = 1스택, 0.4 = 2스택, ..., 1.0 = 5스택
- 파일: `Project/1.6/Source/RatHolicGun/Verb_RatHolicGun.cs`
- 결과: 완료

### 9. Comp_RatHolicGun.cs 수정
- 내용: 하나의 Hediff만 제거하도록 간소화
- 파일: `Project/1.6/Source/RatHolicGun/Comp_RatHolicGun.cs`
- 결과: 완료

## 최종 작업 결과/ 중단 사유
구현 완료. StatPart를 사용하여 정확히 0.3초씩 cooldown 감소 구현.
다음 작업:
- 빌드하여 컴파일 오류 확인
- 게임 내 테스트하여 게틀링 효과 확인

## 관련 파일 목록
- `Project/1.6/Defs/HediffDefs/Hediffs_RatHolicGun.xml` (신규 생성, 수정)
- `Project/1.6/Source/RatHolicGun/Verb_RatHolicGun.cs` (신규 생성, 수정)
- `Project/1.6/Source/RatHolicGun/Comp_RatHolicGun.cs` (신규 생성, 수정)
- `Project/1.6/Source/RatHolicGun/StatPart_RatHolicGunSpooling.cs` (신규 생성)
- `Project/1.6/Defs/Stats/Stats_Pawns_Combat_Patch.xml` (신규 생성)
- `Project/1.6/Defs/ThingsDefs/Weapon_Range.xml` (수정)

## 참고사항
- StatPart를 사용하여 무기 cooldown과 무관하게 정확히 0.3초씩 감소 구현
- Hediff의 Severity로 중첩 관리 (0.2 = 1스택, 최대 1.0 = 5스택)
- 최적화: Hediff가 없으면 StatPart에서 즉시 반환하여 성능 향상
- RangedCooldownFactor는 무기 cooldown에 곱해지는 배율이므로, 실제 감소 시간을 정확히 제어하려면 StatPart 필요

