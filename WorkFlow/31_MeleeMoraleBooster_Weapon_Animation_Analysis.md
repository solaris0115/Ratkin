# RK_Ability_MeleeMoraleBooster 무기 애니메이션 분석 - 2025-01-27

## 작업 개요
- 요청 내용: RK_Ability_MeleeMoraleBooster 사용 시 무기를 위로 치켜드는 애니메이션 설정 방법 조사
- 목표: 부모 abilityDef 조사 및 RimWorld 소스에서 관련 로직 확인 후 해결 방안 제시

## 계획 (AI가 결정한 계획)
1. RK_Ability_MeleeMoraleBooster의 부모 RoleAuraBuffBase abilityDef 조사
2. RimWorld 소스에서 Verb_CastAbility와 무기 애니메이션 관련 로직 조사
3. 무기를 위로 치켜드는 동작 구현 방법 확인 (AimAngleOverride 등)
4. 조사 결과 리포트 작성 및 해결 방안 제시

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
[사용자 승인 완료 - 작업 진행]

## 작업 세부 진행
1. RK_Ability_MeleeMoraleBooster의 부모 RoleAuraBuffBase abilityDef 조사 [v]
2. RimWorld 소스에서 Verb_CastAbility와 무기 애니메이션 관련 로직 조사 [v]
3. 무기를 위로 치켜드는 동작 구현 방법 확인 (AimAngleOverride 등) [v]
4. 조사 결과 리포트 작성 및 해결 방안 제시 [v]

## 진행 상황

### 1. RK_Ability_MeleeMoraleBooster 정의 확인
- 내용: 현재 AbilityDefs.xml에서 RK_Ability_MeleeMoraleBooster 확인
- 결과:
  - ParentName: "RoleAuraBuffBase"
  - verbClass: Verb_CastAbility (부모에서 상속)
  - warmupTime: 0.5초 (부모에서 상속)

### 2. RoleAuraBuffBase 부모 정의 조사
- 내용: RimWorldData/Ideology/Defs/AbilityDefs/Abilities.xml에서 RoleAuraBuffBase 확인
- 결과:
  - 위치: `RimWorldData/Ideology/Defs/AbilityDefs/Abilities.xml:390-421`
  - verbClass: Verb_CastAbility
  - warmupTime: 0.5초
  - drawAimPie: False
  - requireLineOfSight: False
  - targetRequired: False

### 3. Verb_CastAbility 소스 코드 분석
- 내용: RimworldSource/RimWorld/Verb_CastAbility.cs 확인
- 결과:
  - Verb_CastAbility는 Verb를 상속받음
  - AimAngleOverride 속성은 상속받아 사용 가능
  - TryCastShot()에서 ability.Activate() 호출

### 4. 무기 애니메이션 메커니즘 분석
- 내용: PawnRenderUtility.cs에서 무기 각도 계산 로직 확인
- 결과:
  - 위치: `RimworldSource/Verse/PawnRenderUtility.cs:254-268`
  - 조준 각도 계산 순서:
    1. `stance_Busy.focusTarg`를 향한 각도 계산 (기본)
    2. `Verb.AimAngleOverride` 속성이 있으면 해당 값으로 오버라이드
  - `neverAimWeapon` 속성으로 무기 조준 비활성화 가능

### 5. 해결 방안 도출
- 내용: 무기를 위로 치켜드는 동작 구현 방법 확인
- 결과:
  - **방법 1**: 커스텀 Verb_CastAbility를 만들어 `AimAngleOverride` 오버라이드
  - **방법 2**: AbilityDef의 verbProperties에서 커스텀 verbClass 지정
  - 위쪽 각도: 약 90도 (수직 위) 또는 45도 (대각선 위)

## 최종 작업 결과/ 중단 사유
✅ **작업 완료**: RK_Ability_MeleeMoraleBooster 무기 애니메이션 분석 완료

**주요 발견사항:**
1. RoleAuraBuffBase는 Verb_CastAbility를 사용
2. 무기 각도는 `Verb.AimAngleOverride`로 제어 가능
3. 커스텀 Verb 클래스를 만들어 위쪽 각도(90도)를 반환하면 무기를 위로 치켜들 수 있음

## 관련 파일 목록
- `Project/1.6/Defs/AbilityDefs/AbilityDefs.xml` - RK_Ability_MeleeMoraleBooster 정의
- `RimWorldData/Ideology/Defs/AbilityDefs/Abilities.xml` - RoleAuraBuffBase 정의
- `RimworldSource/RimWorld/Verb_CastAbility.cs` - Verb_CastAbility 소스 코드
- `RimworldSource/Verse/PawnRenderUtility.cs` - 무기 드로잉 로직
- `RimworldSource/Verse/Verb.cs` - Verb 클래스 (AimAngleOverride)
- `Report/28_Weapon_Drawing_Angle_Analysis_Report.md` - 무기 각도 분석 리포트

## 참고사항
- 무기를 위로 치켜들려면 커스텀 Verb_CastAbility를 만들어 `AimAngleOverride`를 90도로 설정
- 각도는 0도(동쪽)부터 시계방향으로 증가 (90도 = 북쪽, 위쪽)
- AbilityDef에서 verbProperties.verbClass를 커스텀 클래스로 지정하면 적용됨











