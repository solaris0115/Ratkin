---
# DPS_AVG Weighted Accuracy Formula RangedWeapon HitChance Curve Interpolation Gun_AssaultRifle
category: report
last_updated: 2026-03-10
sources:
  - RimworldSource/Verse/VerbProperties.cs (GetHitChanceFactor, AdjustedAccuracy)
  - RimworldSource/Verse/ShotReport.cs (HitFactorFromShooter, HitReportFor)
  - RimworldSource/Verse/ShootTuning.cs (DistTouch/Short/Medium/Long)
  - RimworldSource/Verse/GenTicks.cs (TicksPerRealSecond)
  - RimworldSource/Verse/DebugOutputsGeneral.cs (dpsMissless, fullcycle)
  - RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedIndustrial.xml
scope: 원거리 무기 거리별 명중률 가중 평균 DPS 공식 설계 및 검증
---

# DPS_AVG: 거리별 명중률 가중 평균 DPS 공식

## 1. 목적

림월드 원거리 무기의 **실전 기대 DPS**를 단일 수치로 표현하기 위한 공식을 설계한다.
기존 단순 4점 평균 `(accT+accS+accM+accL)/4` 방식의 한계를 극복하고,
**실제 명중률 커브 보간 로직**과 **무기별 사거리**를 정확히 반영한다.

---

## 2. 림월드 명중률 커브 원본 로직

### 2.1 고정 거리 구간 (ShootTuning.cs)

| 구간명 | 거리(타일) | 상수 |
|--------|-----------|------|
| Touch  | 0 ~ 3    | `DistTouch = 3f` |
| Short  | 3 ~ 12   | `DistShort = 12f` |
| Medium | 12 ~ 25  | `DistMedium = 25f` |
| Long   | 25 ~ 40  | `DistLong = 40f` |

**핵심**: 이 구간은 무기 최대 사거리(range)와 **무관하게 고정**이다.

### 2.2 보간 방식 (VerbProperties.GetHitChanceFactor)

```
acc(d) =
  d ∈ [0, 3]   → accTouch
  d ∈ (3, 12]  → Lerp(accTouch, accShort,  (d-3)/9)
  d ∈ (12, 25] → Lerp(accShort, accMedium, (d-12)/13)
  d ∈ (25, 40] → Lerp(accMedium, accLong,  (d-25)/15)
  d > 40       → accLong
```

- **선형 보간**(Mathf.Lerp) 사용
- 결과값은 `Clamp(0.01, 1.0)` 적용

### 2.3 사수 측 명중률 (ShotReport.HitFactorFromShooter)

```
shooterFactor = Pow(ShootingAccuracyPawn, distance) × ShootingAccuracyFactor_[구간]
```

> **본 공식에서는 사수 보정을 제외**하고, 순수 무기 성능만으로 DPS_AVG를 산출한다.
> 사수 보정은 폰의 ShootingAccuracy 스탯에 따라 달라지므로 무기 밸런싱 비교에는 무기 자체 명중률만 사용한다.

---

## 3. 기본 DPS 공식 (DPS_raw)

### 3.1 시간 단위 통일

| 파라미터 | 원 단위 | 치환 |
|----------|---------|------|
| warmupTime | 초 | 그대로 |
| RangedWeapon_Cooldown | 초 | 그대로 |
| ticksBetweenBurstShots | 틱 | ÷ 60 |
| burstShotCount | 무차원 | 그대로 |

### 3.2 Full Cycle Time

```
T_cycle = warmup + cooldown + (burstCount - 1) × (ticksBetweenBurstShots / 60)
```

### 3.3 DPS_raw

```
DPS_raw = (damage × burstCount) / T_cycle
```

---

## 4. DPS_AVG 공식 (가중 평균 명중률 기반)

### 4.1 핵심 아이디어

무기가 사격 가능한 전체 거리 `[0, R]` (R = 최대 사거리)에서
명중률 함수 `acc(d)`를 **적분**하여 **거리 가중 평균 명중률**을 구한다.

```
         1   R
acc̄ = ─── ∫  acc(d) dd
         R   0
```

### 4.2 구간별 적분 (선형 보간 → 사다리꼴 공식)

선형 보간 구간의 적분은 **사다리꼴 면적**과 동일하다:

```
∫[a,b] Lerp(v₁, v₂, (d-a)/(b-a)) dd = (b-a) × (v₁+v₂)/2
```

무기 사거리 R에 따라 구간을 자른다:

```
breakpoints = [0, min(3,R), min(12,R), min(25,R), min(40,R)]  (중복 제거)
```

각 구간 [bₖ, bₖ₊₁]에서:
```
integral_k = (bₖ₊₁ - bₖ) × (acc(bₖ) + acc(bₖ₊₁)) / 2
```

### 4.3 최종 공식

```
         Σ integral_k
acc̄ = ─────────────
             R

DPS_AVG = DPS_raw × acc̄
```

### 4.4 단순 4점 평균과의 차이

| 방식 | 장점 | 단점 |
|------|------|------|
| 단순 4점 평균 `(T+S+M+L)/4` | 계산 간단 | 사거리 무시, 구간 폭 무시 |
| **가중 적분 평균** (본 공식) | 사거리 반영, 구간 폭 가중 | 계산 약간 복잡 |

---

## 5. Gun_AssaultRifle 적용 예시

### 5.1 입력 데이터

| 항목 | 값 |
|------|-----|
| damage | 11 |
| burstShotCount | 3 |
| ticksBetweenBurstShots | 10 (= 0.1667초) |
| warmupTime | 1.0초 |
| cooldownTime | 1.70초 |
| range | 30.9 |
| accTouch | 0.60 |
| accShort | 0.70 |
| accMedium | 0.65 |
| accLong | 0.55 |

### 5.2 DPS_raw 계산

```
T_cycle = 1.0 + 1.70 + (3-1) × (10/60) = 3.0333초
DPS_raw = (11 × 3) / 3.0333 = 10.8791
```

### 5.3 구간별 적분

| 구간 | 폭 | acc(시작) | acc(끝) | 구간 평균 | 적분값 |
|------|-----|-----------|---------|-----------|--------|
| [0, 3] | 3.0 | 0.6000 | 0.6000 | 0.6000 | 1.8000 |
| [3, 12] | 9.0 | 0.6000 | 0.7000 | 0.6500 | 5.8500 |
| [12, 25] | 13.0 | 0.7000 | 0.6500 | 0.6750 | 8.7750 |
| [25, 30.9] | 5.9 | 0.6500 | 0.6107 | 0.6303 | 3.7190 |

- `acc(30.9) = Lerp(0.65, 0.55, (30.9-25)/15) = Lerp(0.65, 0.55, 0.3933) = 0.6107`

### 5.4 가중 평균 명중률

```
acc̄ = (1.8000 + 5.8500 + 8.7750 + 3.7190) / 30.9 = 20.1440 / 30.9 = 0.6519
```

### 5.5 DPS_AVG

```
DPS_AVG = 10.8791 × 0.6519 = 7.0922
```

### 5.6 비교

| 방식 | 평균 명중률 | DPS_AVG |
|------|------------|---------|
| 단순 4점 평균 | 0.6250 | 6.7995 |
| **가중 적분 평균** | **0.6519** | **7.0922** |

차이 원인: AssaultRifle은 accShort(0.70)가 가장 높은데,
Short 구간(3~12)에서 많은 거리를 커버하므로 가중 평균이 더 높아진다.

---

## 6. 거리별 상세 명중률 및 실효 DPS

| 거리(타일) | 명중률 | DPS_effective |
|-----------|--------|---------------|
| 1.0 | 0.6000 | 6.5275 |
| 3.0 | 0.6000 | 6.5275 |
| 5.0 | 0.6222 | 6.7692 |
| 7.5 | 0.6500 | 7.0714 |
| 10.0 | 0.6778 | 7.3736 |
| 12.0 | 0.7000 | 7.6154 |
| 15.0 | 0.6885 | 7.4899 |
| 18.0 | 0.6769 | 7.3643 |
| 20.0 | 0.6692 | 7.2806 |
| 25.0 | 0.6500 | 7.0714 |
| 28.0 | 0.6300 | 6.8538 |
| 30.0 | 0.6167 | 6.7088 |
| 30.9 | 0.6107 | 6.6435 |

**피크 DPS**: 12 타일 (Short 경계)에서 7.6154 — accShort가 최고치이므로

---

## 7. 일반화 공식 (임의 무기 적용)

### 7.1 Python 의사코드

```python
def calc_dps_avg(damage, burst, warmup, cooldown, ticks_between,
                 acc_t, acc_s, acc_m, acc_l, weapon_range):
    # DPS_raw
    cycle = warmup + cooldown + (burst - 1) * (ticks_between / 60.0)
    dps_raw = (damage * burst) / cycle

    # 명중률 함수
    def acc(d):
        if d <= 3:    return acc_t
        if d <= 12:   return acc_t + (acc_s - acc_t) * (d - 3) / 9
        if d <= 25:   return acc_s + (acc_m - acc_s) * (d - 12) / 13
        if d <= 40:   return acc_m + (acc_l - acc_m) * (d - 25) / 15
        return acc_l

    # 구간별 적분
    R = weapon_range
    bps = sorted(set([0, min(3,R), min(12,R), min(25,R), min(40,R), R]))
    total = sum((bps[i+1]-bps[i]) * (acc(bps[i])+acc(bps[i+1])) / 2
               for i in range(len(bps)-1))

    acc_avg = total / R
    return dps_raw * acc_avg
```

### 7.2 수학적 닫힌 형태

무기 사거리 R이 구간 경계 사이에 위치할 때, 구간을 R에서 잘라서 계산:

```
acc̄(R) = (1/R) × [
    min(3,R) × accT                                           (Touch 구간)
  + clamp_width(3,12,R) × (accT + acc(min(12,R))) / 2        (Touch→Short)
  + clamp_width(12,25,R) × (accS + acc(min(25,R))) / 2       (Short→Medium)
  + clamp_width(25,40,R) × (accM + acc(min(40,R))) / 2       (Medium→Long)
]

where clamp_width(a,b,R) = max(0, min(b,R) - a)  (R > a일 때만)
```

---

## 8. 기존 보고서 DPS_AVG와의 차이

| 무기 | 기존 DPS_AVG (4점평균) | 새 DPS_AVG (가중적분) | 차이 |
|------|----------------------|---------------------|------|
| Gun_AssaultRifle | 6.80 | **7.09** | +0.29 |

기존 `Ranged_Weapon_Balancing_Report.md`의 DPS_AVG는 `DPS × (T+S+M+L)/4` 방식이었으며,
본 보고서의 가중 적분 방식이 **실제 게임 내 명중률 로직을 더 정확히 반영**한다.

---

## 9. 주의사항

1. **사수 보정 미포함**: 본 공식은 무기 자체 명중률만 반영. 실전에서는 `Pow(ShootingAccuracyPawn, dist)` 등 사수 능력이 추가로 곱해짐
2. **엄폐/날씨/어둠 미포함**: factorFromWeather, factorFromCoveringGas, offsetFromDarkness 등은 상황 의존적이므로 제외
3. **거리 균등 분포 가정**: 모든 교전 거리가 [0, R]에서 균등하게 발생한다고 가정. 실전에서는 특정 거리 교전이 더 잦을 수 있음
4. **품질 보정 미포함**: 무기 품질(Normal 기준)에 따른 stat 변화 미반영

---

## 10. 소스코드 레퍼런스

| 파일 | 핵심 내용 |
|------|-----------|
| `RimworldSource/Verse/ShootTuning.cs:7-14` | DistTouch=3, DistShort=12, DistMedium=25, DistLong=40 |
| `RimworldSource/Verse/VerbProperties.cs:721-745` | GetHitChanceFactor — 선형 보간 명중률 계산 |
| `RimworldSource/Verse/VerbProperties.cs:609-643` | AdjustedAccuracy — 무기 Stat 기반 명중률 |
| `RimworldSource/Verse/VerbProperties.cs:646-649` | AdjustedFullCycleTime — 전체 사이클 시간 |
| `RimworldSource/Verse/ShotReport.cs:205-250` | HitFactorFromShooter — 사수+거리 보정 |
| `RimworldSource/Verse/ShotReport.cs:66-94` | 최종 명중률 합산 |
| `RimworldSource/Verse/GenTicks.cs:11` | TicksPerRealSecond = 60 |
| `RimworldSource/Verse/DebugOutputsGeneral.cs:63-80` | fullcycle, dpsMissless 디버그 함수 |
