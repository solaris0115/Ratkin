---
# DPS Ranged Weapon Time Unit ticks seconds 치환 변환 원거리무기
category: report
last_updated: 2026-03-10
sources:
  - RimworldSource/Verse/GenTicks.cs
  - RimworldSource/Verse/VerbProperties.cs
  - RimworldSource/Verse/DebugOutputsGeneral.cs
scope: 원거리 무기 DPS 산출 시 시간 단위 통일

# 원거리 무기 DPS 시간 단위 치환 가이드

## 1. 개요

원거리 무기 DPS를 정확히 산출하려면 다음 파라미터들의 **시간 단위가 서로 다르므로** 초(seconds)로 통일해야 한다.

| 파라미터 | Def/Stat 위치 | 원 단위 | 치환 후 |
|----------|---------------|---------|---------|
| warmupTime | verbs → warmupTime | **초** | 그대로 사용 |
| RangedWeapon_Cooldown | statBases | **초** | 그대로 사용 |
| ticksBetweenBurstShots | verbs → ticksBetweenBurstShots | **틱** | ÷ 60 |
| burstShotCount | verbs → burstShotCount | 무차원 | 그대로 사용 |

---

## 2. 핵심 변환식 (GenTicks.cs)

```
60 ticks = 1초
```

| 방향 | 공식 |
|------|------|
| 틱 → 초 | `ticks / 60` |
| 초 → 틱 | `seconds × 60` (반올림) |

---

## 3. 치환 방법

### 3.1 warmupTime

- **위치**: `verbs` → `warmupTime`
- **원 단위**: 초(seconds)
- **치환**: **그대로 사용**

```xml
<warmupTime>1.0</warmupTime>  →  1.0초
```

---

### 3.2 RangedWeapon_Cooldown

- **위치**: `statBases` → `RangedWeapon_Cooldown`
- **원 단위**: 초(seconds) — StatDef formatString `{0} s`
- **치환**: **그대로 사용**

```xml
<RangedWeapon_Cooldown>1.70</RangedWeapon_Cooldown>  →  1.70초
```

---

### 3.3 ticksBetweenBurstShots (연발 간격)

- **위치**: `verbs` → `ticksBetweenBurstShots`
- **원 단위**: 틱(ticks)
- **치환**: **÷ 60** 하여 초로 변환

```
ticksBetweenBurstShots = 10  →  10 / 60 = 0.1667초
ticksBetweenBurstShots = 15  →  15 / 60 = 0.25초
```

---

### 3.4 burstShotCount

- **위치**: `verbs` → `burstShotCount`
- **원 단위**: 무차원 (연발 횟수)
- **치환**: **그대로 사용**

---

## 4. Full Cycle Time (1회 발사 사이클) 공식

모든 값을 **초**로 통일한 후:

```
fullCycleTime_sec = warmupTime + RangedWeapon_Cooldown + (burstShotCount - 1) × (ticksBetweenBurstShots / 60)
```

| 항목 | 치환식 |
|------|--------|
| warmupTime | `warmupTime` (그대로) |
| cooldown | `RangedWeapon_Cooldown` (그대로) |
| 연발 간격 합 | `(burstShotCount - 1) × ticksBetweenBurstShots / 60` |

---

## 5. DPS 공식

```
DPS = (damage × burstShotCount) / fullCycleTime_sec
```

- `damage`: projectile의 `damageAmountBase` (탄환당 피해량)

---

## 6. Assault Rifle 예시

| 항목 | Def 값 | 치환 후 (초) |
|------|--------|--------------|
| warmupTime | 1.0 | 1.0 |
| RangedWeapon_Cooldown | 1.70 | 1.70 |
| burstShotCount | 3 | 3 |
| ticksBetweenBurstShots | 10 | 10/60 = 0.1667 |
| damage (Bullet_AssaultRifle) | 11 | 11 |

**계산**
```
fullCycleTime = 1.0 + 1.70 + (3-1) × (10/60) = 2.70 + 0.333 = 3.033초
DPS = (11 × 3) / 3.033 ≈ 10.87
```

---

## 7. 단발 무기 (burstShotCount = 1)

연발 간격 항이 0이 됨:

```
fullCycleTime_sec = warmupTime + RangedWeapon_Cooldown
DPS = damage / fullCycleTime_sec
```

---

## 8. 요약 치환표

| 파라미터 | 치환 | 비고 |
|----------|------|------|
| warmupTime | `warmupTime` | 초 |
| RangedWeapon_Cooldown | `RangedWeapon_Cooldown` | 초 |
| ticksBetweenBurstShots | `ticksBetweenBurstShots / 60` | 틱→초 |
| burstShotCount | `burstShotCount` | 변환 없음 |
