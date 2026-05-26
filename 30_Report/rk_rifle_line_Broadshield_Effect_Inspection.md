# rk_rifle_line Broadshield 이펙트 검사 현황 보고서

**태그:** rk_rifle_line Broadshield SectorShot Shield Effect Inspection

---

## 개요

`RK_Rifle_line`(랫킨 샷건)은 **Verb_SectorShot**을 사용하는 부채꼴 즉시 피해 무기이며, **실제 Projectile을 발사하지 않는다**.

---

## 검사 경로 비교

| 구분 | 일반 총알(Projectile) | RK_Rifle_line(SectorShot) |
|------|------------------------|---------------------------|
| **진행 방식** | 매 틱 이동, `Projectile.TickInterval` | 발사 시 한 번 `TryCastShot` 실행 |
| **실제 투사체** | 있음 (날아가는 Bullet) | 없음 |
| **차단 검사 시점** | 투사체가 쉴드 원 둘레와 교차할 때 | 부채꼴 셀 중 쉴드 내부에 있는 셀 판별 |

---

## SectorShot의 Broadshield 검사 흐름

1. **FilterShieldedCells**
   - `map.listerThings.ThingsInGroup(ProjectileInterceptor)`로 활성 쉴드 수집
   - 사수가 쉴드 내부에 있거나, 사수와 쉴드가 비적대 관계이면 해당 쉴드 제외
   - `interceptGroundProjectiles`가 true인 것만 대상
   - 각 부채꼴 셀에 대해 `GetBlockingShieldIndex`로 쉴드 반경 내 여부 판별
   - 쉴드 내부 셀 → `shieldedCells`, `hitShields`에 기록

2. **SpawnShieldBlockEffects**
   - `shieldedCells`가 비어 있으면 호출 생략(이펙트 없음)
   - 각 hitShields마다 `comp.Props.interceptEffect` 또는 `Interceptor_BlockedProjectile` 사용
   - **이펙트 배치 조건**: 셀 중심이 쉴드 반경의 **외곽 링**(내부 반경 ~ 반경) 안에 있어야 함  
     - `innerThreshold = (radius - 1.5)²`  
     - `radiusSq = radius²`  
     - 조건: `innerThreshold ≤ 거리² ≤ radiusSq`
   - 조건을 만족하는 셀에 Effecter 트리거
   - 위 조건을 만족하는 셀이 하나도 없으면 `shieldedCells[0]`에 폴백 이펙트 1회만 생성

---

## 이펙트가 안 나는 가능 요인

1. **shieldedCells가 비어 있음**  
   - 부채꼴과 쉴드가 겹치는 셀이 없거나, 모든 쉴드가 사수·적대 조건으로 필터링됨

2. **외곽 링 조건 미충족**  
   - shieldedCells는 있지만, 모두 `innerThreshold` 안쪽(쉴드 중심 근처)이라 이펙트용 조건에서 탈락

3. **Effecter 스폰 위치**  
   - 일반 투사체는 기하학적 교차점에서 트리거, SectorShot은 셀 중심 기준이라 위치·감각이 다를 수 있음

---

## 요약

- 일반 투사체: `CompProjectileInterceptor.CheckIntercept` + `GenGeo.IntersectLineCircleOutline`로 경로·쉴드 둘레 교차 검사 후 즉시 이펙트 트리거
- RK_Rifle_line: 부채꼴 셀 단위로 쉴드 내부 여부만 검사하고, 쉴드 **외곽 링** 내부 셀에만 이펙트를 배치 → 셀 단위 판정 + 링 조건 때문에 이펙트가 생략될 수 있음
