# 부채꼴(Sector) 셀 유틸리티 분석 보고서

> **태그**: Sector 부채꼴 Fan Cone TeleUtils circularSector GetSectorCells LOS LineOfSight 폭발 Explosion Gunlance BFR  
> **목적**: 위치·방향·길이·각도로 부채꼴 셀을 반환하는 유틸 함수 정리 및 LOS 적용 여부 검토  
> **분석 대상**: TeleUtils, Projectile_BFRHE, CompAbilityEffect_FireSpew, 림월드 원본 Explosion/DamageWorker

---

## 1. 요구사항 4가지 정리

| 항목 | 설명 |
|------|------|
| **위치** | 부채꼴 중심 셀 (IntVec3 center) |
| **방향** | 부채꼴 중심선 방향 (각도 또는 두 점으로 유도) |
| **길이** | 반지름/radius (셀 단위) |
| **각도** | 부채꼴 전체 각도 (합산각) 또는 좌우 반각 |

---

## 2. 기존 유틸 함수 비교

| 함수 | 위치 | 중심 | 방향 | 길이 | 각도 | LOS |
|------|------|------|------|------|------|-----|
| **TeleUtils.circularSectorCellsStartedCaster** | Verb_GunlanceFiring.cs | center | center→target | radius | angle (합산각) | ✅ center↔cell |
| **TeleUtils.circularSectorCellsStartedTarget** | Verb_GunlanceFiring.cs | target | center→target | radius | angle (합산각) | ✅ target↔cell |
| **BFR GetSectorCells** | Projectile_BFRHE.cs | center | origin→destination | sectorRadius | sectorAngle (합산각) | ⚠️ wallBreachRadius 이내 무시, 초과 시 적용 |
| **CompAbilityEffect_FireSpew.AffectedCells** | RimworldSource | Pawn.Position | Pawn→target | range | lineWidthEnd | ✅ TryFindShootLineFromTo |

---

## 3. TeleUtils (Gunlance)

**위치**: `Project/1.6/Source/Gunlance/Verb_GunlanceFiring.cs` (304~361행)

### 3.1 시그니처

```csharp
public static IEnumerable<IntVec3> circularSectorCellsStartedTarget(
    IntVec3 center, Map map, IntVec3 target, float radius, float angle, bool useCenter = false)

public static IEnumerable<IntVec3> circularSectorCellsStartedCaster(
    IntVec3 center, Map map, IntVec3 target, float radius, float angle, bool useCenter = false)
```

### 3.2 파라미터

| 파라미터 | 설명 |
|----------|------|
| center | 발사자(캐스터) 위치 |
| target | 목표 지점 (방향 유도용) |
| radius | 부채꼴 반지름 |
| angle | 合算각 (중심선 좌우로 angle/2씩) |
| useCenter | 0번째 셀(중심) 포함 여부 |

### 3.3 차이점

| 함수 | 부채꼴 중심 | LOS 기준 |
|------|-------------|----------|
| **StartedCaster** | center | center ↔ cell |
| **StartedTarget** | target | target ↔ cell |

### 3.4 사용처

- `CreateGunlanceExplosion()` (76행): `circularSectorCellsStartedTarget` → `GenExplosion.DoExplosion(..., overrideCells)`

### 3.5 한계

- 방향을 **두 점(center, target)**으로만 지정 가능
- **각도(angle)를 직접** 받는 형태가 아님

---

## 4. BFR GetSectorCells (Projectile_BFRHE)

**위치**: `Project/1.6/Source/BFR/Projectile_BFRHE.cs`

### 4.1 특징

- **방향**: `destination - origin` (탄이 날아가는 방향의 **후면**)
- **LOS**: `wallBreachRadius` 이내는 벽 무시, 초과 시 `GenSight.LineOfSight(center, cell, map, true)` 적용

### 4.2 핵심 로직

```csharp
// wallBreachRadius 이내: 벽 무시. 초과: 벽에 막히면 제외.
float distSq = (cell - center).LengthHorizontalSquared;
if (distSq <= wallBreachRadiusSq)
    result.Add(cell);
else if (GenSight.LineOfSight(center, cell, map, true, null, 0, 0))
    result.Add(cell);
```

### 4.3 한계

- 방향이 `origin`/`destination` 프로젝타일 내부 데이터에 의존
- 범용 유틸로 분리되어 있지 않음

---

## 5. 림월드 원본

### 5.1 GenExplosion.DoExplosion

- `affectedAngle`: `FloatRange?`로 각도 범위 지정 → 부채꼴 폭발
- `overrideCells`: 셀 목록 직접 지정
- `Verb_SpewFire`에서 `affectedAngle` 사용

### 5.2 DamageWorker.ExplosionCellsToHit

- `affectedAngle`가 있으면 각도 필터링
- `GenSight.LineOfSight(center, intVec, map, true)` 적용

### 5.3 CompAbilityEffect_FireSpew.AffectedCells

- 내부 전용, FireSpew 능력 전용
- `TryFindShootLineFromTo`로 시야 체크 (LOS 유사)

---

## 6. 결론

### 6.1 범용 유틸 부재

**위치·방향·길이·각도를 직접 파라미터로 받는 범용 유틸 함수는 없음.**

| 구분 | 유틸 | 비고 |
|------|------|------|
| LOS 적용 | TeleUtils.circularSectorCellsStartedCaster / StartedTarget | 방향은 두 점으로만 지정 |
| LOS 부분 적용 | BFR GetSectorCells | wallBreachRadius 이내 무시, 초과 시 적용 |
| LOS 미적용 | - | BFR은 wallBreachRadius=0으로 설정 시 가능 |

### 6.2 제안: 범용 유틸 추가 시그니처

```csharp
/// <summary>
/// 부채꼴 내 셀들 반환.
/// </summary>
/// <param name="center">부채꼴 중심</param>
/// <param name="map">맵</param>
/// <param name="directionAngle">중심선 방향 각도 (도)</param>
/// <param name="radius">반지름</param>
/// <param name="totalAngle">합산 각도 (좌우 totalAngle/2씩)</param>
/// <param name="requireLOS">true면 LOS 체크, false면 벽 무시</param>
/// <param name="useCenter">중심 셀 포함 여부</param>
IEnumerable<IntVec3> GetSectorCells(
    IntVec3 center, Map map, float directionAngle, float radius, float totalAngle,
    bool requireLOS = true, bool useCenter = false)
```

---

## 7. 참조 파일

| 파일 | 용도 |
|------|------|
| `Project/1.6/Source/Gunlance/Verb_GunlanceFiring.cs` | TeleUtils |
| `Project/1.6/Source/BFR/Projectile_BFRHE.cs` | BFR GetSectorCells |
| `RimworldSource/Verse/GenRadial.cs` | RadialPattern, NumCellsInRadius |
| `RimworldSource/Verse/GenSight.cs` | LineOfSight |
| `RimworldSource/RimWorld/CompAbilityEffect_FireSpew.cs` | AffectedCells |
| `RimworldSource/Verse/DamageWorker.cs` | ExplosionCellsToHit (affectedAngle) |
