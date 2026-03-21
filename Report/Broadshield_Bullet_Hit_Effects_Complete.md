# Broadshield 총탄 충돌 이펙트 전체 목록

**태그:** Broadshield ProjectileInterceptor Shield Effect Fleck Sound Arc Glow

---

## 개요

일반 총알(Projectile)이 Broadshield에 부딪힐 때 발생하는 모든 시청각 이펙트를 정리한 보고서.

---

## 1. 쉴드 원호 빛남 (원의 호가 번쩍)

**출처:** `CompProjectileInterceptor.PostDraw` → `ForceFieldConeMat`

| 항목 | 내용 |
|------|------|
| **텍스처** | `Other/ForceFieldCone` |
| **셰이더** | MoteGlow |
| **트리거** | `CheckIntercept` 성공 시 `drawInterceptCone = true`, `lastInterceptAngle` 갱신 |
| **표시** | 충돌 위치 각도로 원 위에 부채꼴(cone) 형태로 오버레이 |
| **지속** | 40틱 동안 페이드아웃 (`GetCurrentConeAlpha_RecentlyIntercepted`) |
| **알파** | 최대 0.82 → 40틱 후 0 |

여러 발 맞을 때마다 `lastInterceptAngle`이 업데이트되므로, 발사 위치에 따라 원호가 해당 각도로 빛나고, 연속 사격 시 원을 따라 번쩍이는 것처럼 보인다.

---

## 2. Effecter (Interceptor_BlockedProjectile)

**출처:** `CompProjectileInterceptor.TriggerEffecter` → `EffecterDefOf.Interceptor_BlockedProjectile`

Broadshield는 별도 `interceptEffect`를 지정하지 않으므로 기본값 사용.

### 2-1. 사운드

| Def | 경로 | 비고 |
|-----|------|------|
| **Interceptor_BlockProjectile** | `Misc/EnergyShield/Absorb` | 차단 시 재생 |
| **maxSimultaneous** | 1 | 동시 1발만 (연사 시 다수 미재생 가능) |

### 2-2. Fleck – DustPuff (연기/먼지)

| 항목 | 값 |
|------|-----|
| **FleckDef** | DustPuff |
| **텍스처** | Things/Mote/DustPuff |
| **burstCount** | 3~5 |
| **speed** | 0.6~0.75 |
| **scale** | 1.5~2.3 |
| **positionRadius** | 0.1 |

### 2-3. Fleck – ExplosionFlash (번쩍임)

| 항목 | 값 |
|------|-----|
| **FleckDef** | ExplosionFlash |
| **텍스처** | Things/Mote/ExplosionFlash |
| **셰이더** | MoteGlow |
| **burstCount** | 1 |
| **scale** | 3.0 |
| **solidTime** | 0.05초 |
| **fadeOutTime** | 0.1초 |

---

## 3. 기본 쉴드 원형

**출처:** `CompProjectileInterceptor.PostDraw` → `ForceFieldMat`

| 항목 | 내용 |
|------|------|
| **텍스처** | Other/ForceField |
| **셰이더** | MoteGlow |
| **색상** | Broadshield: (0.6, 0.6, 0.8) |
| **알파** | `GetCurrentAlpha_RecentlyIntercepted`로 최근 차단 시 0.09 정도 추가 발광 |
| **지속** | 40틱 동안 유지 |

총탄이 맞을 때 원 전체가 약간 더 밝아지는 효과.

---

## 4. 기타 (총탄이 맞지 않을 때)

- **blockedByShield = true** → Projectile의 `landedEffecter`(착탄 이펙트)는 **실행되지 않음** (쉴드에서 소멸)
- Broadshield에는 별도 `interceptEffect` 지정 없음 → `Interceptor_BlockedProjectile`만 사용
- Psychic 방패(에테르 종족 등)는 `Interceptor_BlockedProjectilePsychic` 사용 (별도 Def)

---

## 5. 이펙트 요약 (일반 총탄 기준)

| 카테고리 | 이펙트 | 설명 |
|----------|--------|------|
| **비주얼** | 원호 빛남 | ForceFieldCone – 충돌 각도 부채꼴, 40틱 페이드 |
| | 원형 미세 발광 | ForceField – 최근 차단 시 약간 밝아짐 |
| | ExplosionFlash | 순간 플래시 (scale 3.0) |
| | DustPuff | 연기/먼지 3~5개 (scale 1.5~2.3) |
| **사운드** | Interceptor_BlockProjectile | 에너지 쉴드 흡수음 (동시 1회 제한) |

---

## 6. Def 참조 경로

- **EffecterDef:** `RimworldData/Core/Defs/Effects/Effecter_Damage.xml` → Interceptor_BlockedProjectile
- **SoundDef:** `RimworldData/Core/Defs/SoundDefs/World_Oneshots_ProjectileImpacts.xml` → Interceptor_BlockProjectile
- **FleckDef:** `RimworldData/Core/Defs/Effects/Fleck_Visual.xml` → DustPuff, ExplosionFlash
- **소스:** `RimworldSource/RimWorld/CompProjectileInterceptor.cs` (PostDraw, CheckIntercept, TriggerEffecter)
