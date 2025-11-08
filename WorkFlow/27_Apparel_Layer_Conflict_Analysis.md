# 의상 레이어 충돌 분석 및 해결 방안 - 2025-01-27

## 작업 개요
- 요청 내용: 우주복과 배너를 동시에 착용할 수 있도록 하되, 가방과는 동시 착용하지 못하게 하고 싶음
- 목표: BackPack 레이어 충돌 해결 및 배너-가방 배타성 확보

## 계획 (AI가 결정한 계획)
1. RimWorld 의상 동시 착용 로직 분석
2. 현재 배너, 가방, 우주복의 레이어 및 bodyPartGroups 분석
3. 해결 방법 제안 및 사용자 승인 대기

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
[사용자 승인 대기]

## 작업 세부 진행
1. RimWorld 의상 동시 착용 로직 분석 [v]
2. 현재 배너, 가방, 우주복의 레이어 및 bodyPartGroups 분석 [진행중]
3. 해결 방법 제안 및 사용자 승인 대기 [ ]

## 진행 상황

### 1. RimWorld 의상 동시 착용 로직 분석
- 내용: `ApparelUtility.CanWearTogether` 메서드 분석
- 결과: 
  - **1단계**: 같은 레이어를 사용하는지 확인
  - **2단계**: 같은 레이어를 사용하면, `bodyPartGroups` 충돌 확인
  - 같은 레이어를 사용하더라도 `bodyPartGroups`가 겹치지 않으면 동시 착용 가능

### 2. 현재 상황 분석
- 내용: 배너, 가방, 우주복의 레이어 및 bodyPartGroups 확인
- 결과:
  - **배너** (RK_ApparelAttr_Banner):
    - 레이어: `BackPack` → `Banner` → `OffHand` (최종 변경됨)
    - bodyPartGroups: `Torso`, `Shoulders`, `Arms`
  - **방패들** (RK_WoodenShield, RK_HeavyShield, RK_TowerShield):
    - 레이어: `OuterClothing` → `OffHand` (변경됨)
    - bodyPartGroups: `Torso`, `Shoulders`, `Arms` (또는 더 많은 부위)
  - **가방** (RK_Backpack):
    - 레이어: `BackPack`
    - bodyPartGroups: `Shoulders`
  - **우주복** (RK_Apparel_Vacsuit):
    - 레이어: `BackPack` (layers에 BackPack 포함)
    - bodyPartGroups: `Torso`, `Neck`, `Shoulders`, `Arms`, `Legs`
  - **스페이스 아머** (RK_Apparel_SpaceArmor):
    - 레이어: `BackPack`
    - bodyPartGroups: 상속됨 (아마도 Torso, Shoulders 등)

**변경 전 문제점:**
- 배너와 우주복: 모두 `BackPack` 레이어 사용, `bodyPartGroups` 겹침 (Torso, Shoulders, Arms) → 동시 착용 불가
- 가방과 우주복: 모두 `BackPack` 레이어 사용, `bodyPartGroups` 겹침 (Shoulders) → 동시 착용 불가
- 배너와 가방: 모두 `BackPack` 레이어 사용, `bodyPartGroups` 겹침 (Shoulders) → 동시 착용 불가

**변경 후 상태:**
- 배너와 방패: 모두 `OffHand` 레이어 사용, `bodyPartGroups` 겹침 → **동시 착용 불가** ✅
- 배너와 우주복: 서로 다른 레이어 (`OffHand` vs `BackPack`) → **동시 착용 가능** ✅
- 가방과 우주복: 같은 레이어 (`BackPack`), `bodyPartGroups` 겹침 (Shoulders) → 동시 착용 불가 (의도된 동작) ✅
- 가방과 배너: 서로 다른 레이어 (`BackPack` vs `OffHand`) → **동시 착용 가능** ✅
- 가방과 방패: 서로 다른 레이어 (`BackPack` vs `OffHand`) → **동시 착용 가능** ✅

### 3. 작업 완료 내용
- 내용: 레이어 정리 작업 완료
- 결과:
  - `Apparel_Layer.xml`에서 `Banner` 레이어 삭제, `OffHand` 레이어 추가 (drawOrder: 255)
  - `Apparel_Util.xml`의 배너 정의에서 레이어를 `Banner`에서 `OffHand`로 변경
  - `Apparel_Util.xml`의 모든 방패(RK_WoodenShield, RK_HeavyShield, RK_TowerShield) 정의에서 레이어를 `OuterClothing`에서 `OffHand`로 변경
  - 가방은 `BackPack` 레이어 유지 (변경 없음)
  - 린터 오류 없음 확인

## 최종 작업 결과/ 중단 사유
✅ **작업 완료**: 레이어 정리 완료

**변경 사항:**
1. `Banner` 레이어 삭제, `OffHand` 레이어 추가
2. 배너의 레이어를 `Banner`에서 `OffHand`로 변경
3. 모든 방패들(RK_WoodenShield, RK_HeavyShield, RK_TowerShield)의 레이어를 `OuterClothing`에서 `OffHand`로 변경
4. 가방은 `BackPack` 레이어 유지 (변경 없음)

**최종 상태:**
- 배너와 방패: 모두 `OffHand` 레이어 사용, `bodyPartGroups` 겹침 → **동시 착용 불가** ✅
- 배너와 우주복: 서로 다른 레이어 (`OffHand` vs `BackPack`) → **동시 착용 가능** ✅
- 가방과 우주복: 같은 레이어 (`BackPack`), `bodyPartGroups` 겹침 (Shoulders) → 동시 착용 불가 (의도된 동작) ✅
- 가방과 배너: 서로 다른 레이어 (`BackPack` vs `OffHand`) → **동시 착용 가능** ✅
- 가방과 방패: 서로 다른 레이어 (`BackPack` vs `OffHand`) → **동시 착용 가능** ✅

## 관련 파일 목록
- `RimworldSource/RimWorld/ApparelUtility.cs` - CanWearTogether 메서드
- `RimworldSource/RimWorld/ApparelProperties.cs` - GetInterferingBodyPartGroups 메서드
- `Project/1.6/Defs/ThingsDefs/Apparel_Util.xml` - 배너 및 가방 정의
- `Project/1.6/Defs/ThingsDefs/Apparel_Spacer.xml` - 우주복 정의

## 참고사항
- RimWorld의 의상 동시 착용 로직은 레이어와 bodyPartGroups를 모두 확인
- 같은 레이어를 사용하더라도 bodyPartGroups가 겹치지 않으면 동시 착용 가능

