# 무기 드로잉 및 각도 분석 - 2025-01-27

## 작업 개요
- 요청 내용: 무기 드로잉과 드로잉 각도 관련된 내용을 RimWorld 소스코드에서 찾기. 일반적으로 손에 들고 있는 각도와 발사시 조준하는 각도를 커스터마이징 가능한지 확인
- 목표: 무기 드로잉 메커니즘 분석 및 커스터마이징 가능 여부 확인

## 계획 (AI가 결정한 계획)
1. RimWorld 소스코드에서 무기 드로잉 관련 코드 검색
2. 손에 들고 있을 때의 각도 처리 로직 분석
3. 발사 시 조준 각도 처리 로직 분석
4. 커스터마이징 가능한 속성 확인
5. 분석 결과 리포트 작성

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
[사용자 승인 완료 - 작업 진행]

## 작업 세부 진행
1. RimWorld 소스코드에서 무기 드로잉 관련 코드 검색 [v]
2. 손에 들고 있을 때의 각도 처리 로직 분석 [v]
3. 발사 시 조준 각도 처리 로직 분석 [v]
4. 커스터마이징 가능한 속성 확인 [v]
5. 분석 결과 리포트 작성 [v]

## 진행 상황

### 1. RimWorld 소스코드 검색
- 내용: `PawnRenderUtility.cs`, `PawnRenderer.cs`, `EquipmentUtility.cs` 등 무기 드로잉 관련 코드 발견
- 결과: 주요 클래스 및 메서드 식별 완료

### 2. 손에 들고 있을 때의 각도 처리 분석
- 내용: `PawnRenderUtility.DrawCarriedWeapon` 메서드 분석
- 결과:
  - 위치: `RimworldSource/Verse/PawnRenderUtility.cs:49-69`
  - 동작: Pawn의 facing 방향에 따라 고정된 각도 사용
    - North, East, South: 143도
    - West: 217도
  - 각도는 `DrawEquipmentAiming` 메서드에 전달됨

### 3. 발사 시 조준 각도 처리 분석
- 내용: `PawnRenderUtility.DrawEquipmentAiming` 및 `DrawEquipmentAndApparelExtras` 메서드 분석
- 결과:
  - 위치: `RimworldSource/Verse/PawnRenderUtility.cs:71-114`, `230-273`
  - 조준 각도 계산:
    1. `stance_Busy.focusTarg`를 향한 각도 계산 (기본)
    2. `Verb.AimAngleOverride` 속성으로 오버라이드 가능
  - `equippedAngleOffset` 속성으로 각도 보정 가능

### 4. 커스터마이징 가능한 속성 확인
- 내용: ThingDef 및 Verb 클래스의 속성 확인
- 결과:
  - **ThingDef 속성** (XML에서 설정 가능):
    - `equippedAngleOffset` (float): 무기 장착 시 각도 오프셋
    - `equippedDistanceOffset` (float): 무기 장착 시 거리 오프셋
  - **Verb 속성** (코드에서 오버라이드 가능):
    - `AimAngleOverride` (virtual float?): 조준 각도 오버라이드 (기본값 null)

### 5. 리코일 처리 분석
- 내용: `EquipmentUtility.Recoil` 메서드 분석
- 결과:
  - 위치: `RimworldSource/RimWorld/EquipmentUtility.cs:168-193`
  - 무기 리코일이 각도에 영향을 줌
  - `weaponDef.recoilPower`, `weaponDef.recoilRelaxation` 속성으로 제어 가능

## 최종 작업 결과/ 중단 사유
✅ **작업 완료**: 무기 드로잉 및 각도 메커니즘 분석 완료

**주요 발견사항:**
1. 손에 들고 있을 때는 facing 방향에 따라 고정 각도 사용 (143도 또는 217도)
2. 조준 시에는 타겟을 향한 각도 계산 + `equippedAngleOffset` 보정
3. `Verb.AimAngleOverride`로 조준 각도 커스터마이징 가능
4. `ThingDef.equippedAngleOffset` 및 `equippedDistanceOffset`으로 XML에서 설정 가능

## 관련 파일 목록
- `RimworldSource/Verse/PawnRenderUtility.cs` - 무기 드로잉 메서드
- `RimworldSource/Verse/PawnRenderer.cs` - Pawn 렌더링 처리
- `RimworldSource/RimWorld/EquipmentUtility.cs` - 리코일 처리
- `RimworldSource/Verse/Verb.cs` - Verb 클래스 (AimAngleOverride)
- `RimworldSource/Verse/ThingDef.cs` - ThingDef 속성 정의

## 참고사항
- 무기 각도는 기본적으로 계산되지만, `equippedAngleOffset`으로 미세 조정 가능
- 커스텀 Verb를 만들면 `AimAngleOverride`를 오버라이드하여 조준 각도를 완전히 제어 가능












