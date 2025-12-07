# Range Cooldown 관련 StatOffsets 참조 문서

**작성일**: 2025-01-XX  
**목적**: HediffDef의 statOffsets에서 사용 가능한 원거리 무기 cooldown 관련 Stat 정리

---

## HediffDef statOffsets에서 사용 가능한 Range Cooldown 관련 Stat

### ✅ 사용 가능한 Stat (Pawn의 Stat)

#### 1. **RangedCooldownFactor** ⭐
- **카테고리**: `PawnCombat`
- **타입**: Multiplier (기본값: 1.0)
- **용도**: 무기 cooldown에 곱해지는 배율
- **계산식**: 최종 cooldown = 무기의 RangedWeapon_Cooldown × Pawn의 RangedCooldownFactor
- **사용 예시**:
  ```xml
  <statOffsets>
    <RangedCooldownFactor>-0.2</RangedCooldownFactor>  <!-- 20% 감소 -->
  </statOffsets>
  ```
- **참고**: Apparel에서 `equippedStatOffsets`로 사용됨 (Biotech Apparel_Various.xml)

#### 2. **AimingDelayFactor**
- **카테고리**: `PawnCombat`
- **타입**: Factor (기본값: 1.0)
- **용도**: 조준 시간(warmup time) 배율
- **사용 예시**:
  ```xml
  <statOffsets>
    <AimingDelayFactor>-0.4</AimingDelayFactor>  <!-- 조준 시간 40% 감소 -->
  </statOffsets>
  ```
- **참고**: Ideology RoleEffect에서 사용됨

#### 3. **ShootingAccuracyPawn**
- **카테고리**: `PawnCombat`
- **타입**: Offset (기본값: 0)
- **용도**: 사격 정확도 보정
- **사용 예시**:
  ```xml
  <statOffsets>
    <ShootingAccuracyPawn>3</ShootingAccuracyPawn>  <!-- 정확도 +3 -->
  </statOffsets>
  ```
- **참고**: Apparel, RoleEffect에서 사용됨

#### 4. **ShootingAccuracyFactor_Long**
- **카테고리**: `PawnCombat`
- **타입**: Factor (기본값: 1.0)
- **용도**: 장거리 사격 정확도 배율
- **사용 예시**:
  ```xml
  <statFactors>
    <ShootingAccuracyFactor_Long>0.25</ShootingAccuracyFactor_Long>  <!-- 장거리 정확도 75% 감소 -->
  </statFactors>
  ```
- **참고**: GeneDef에서 사용됨 (Nearsighted gene)

#### 5. **ShootingAccuracyFactor_Medium**
- **카테고리**: `PawnCombat`
- **타입**: Factor (기본값: 1.0)
- **용도**: 중거리 사격 정확도 배율
- **사용 예시**:
  ```xml
  <statFactors>
    <ShootingAccuracyFactor_Medium>0.5</ShootingAccuracyFactor_Medium>  <!-- 중거리 정확도 50% 감소 -->
  </statFactors>
  ```

---

### ❌ 사용 불가능한 Stat (무기의 Stat)

#### **RangedWeapon_Cooldown**
- **카테고리**: `Weapon_Ranged`
- **타입**: 무기 자체의 Stat
- **용도**: 무기의 기본 재장전 시간 정의
- **HediffDef에서 사용**: ❌ 불가능 (Pawn의 Stat이 아님)
- **참고**: ThingDef의 `statBases`에서만 사용 가능

---

## 실제 사용 예시

### 예시 1: RangedCooldownFactor 사용 (statOffsets)
```xml
<HediffDef>
  <defName>RK_Hediff_RatHolicGunSpooling</defName>
  <stages>
    <li>
      <statOffsets>
        <RangedCooldownFactor>-0.2</RangedCooldownFactor>  <!-- 20% 감소 -->
      </statOffsets>
    </li>
  </stages>
</HediffDef>
```

**효과 계산**:
- 무기 cooldown: 1.6초
- 기본 factor: 1.0
- 적용 후 factor: 1.0 - 0.2 = 0.8
- 최종 cooldown: 1.6 × 0.8 = **1.28초** (0.32초 감소)

### 예시 2: RangedCooldownFactor 사용 (statFactors)
```xml
<HediffDef>
  <defName>RK_Hediff_RatHolicGunSpooling</defName>
  <stages>
    <li>
      <statFactors>
        <RangedCooldownFactor>0.8125</RangedCooldownFactor>  <!-- 18.75% 감소 -->
      </statFactors>
    </li>
  </stages>
</HediffDef>
```

**효과 계산**:
- 무기 cooldown: 1.6초
- 기본 factor: 1.0
- 적용 후 factor: 1.0 × 0.8125 = 0.8125
- 최종 cooldown: 1.6 × 0.8125 = **1.3초** (0.3초 감소)

**주의**: 무기 cooldown이 다르면 효과가 달라짐!

### 예시 3: 복합 효과 (RangedCooldownFactor + AimingDelayFactor)
```xml
<HediffDef>
  <defName>RK_Hediff_RapidFire</defName>
  <stages>
    <li>
      <statOffsets>
        <RangedCooldownFactor>-0.15</RangedCooldownFactor>  <!-- 재장전 15% 감소 -->
        <AimingDelayFactor>-0.2</AimingDelayFactor>        <!-- 조준 시간 20% 감소 -->
      </statOffsets>
    </li>
  </stages>
</HediffDef>
```

---

## statOffsets vs statFactors 비교

### statOffsets (더하기/빼기)
- **용도**: 기본값에 더하거나 빼기
- **RangedCooldownFactor 기본값**: 1.0
- **예시**: `<RangedCooldownFactor>-0.2</RangedCooldownFactor>`
- **결과**: 1.0 - 0.2 = 0.8 (20% 감소)

### statFactors (곱하기)
- **용도**: 기본값에 곱하기
- **RangedCooldownFactor 기본값**: 1.0
- **예시**: `<RangedCooldownFactor>0.8</RangedCooldownFactor>`
- **결과**: 1.0 × 0.8 = 0.8 (20% 감소)

**차이점**: 
- `statOffsets`: 절대값 감소 (항상 동일한 효과)
- `statFactors`: 상대값 감소 (기본값에 따라 효과가 달라질 수 있음)

---

## RatHolic Gun에 적용 가능한 옵션

### 옵션 1: statOffsets 사용 (권장)
```xml
<statOffsets>
  <RangedCooldownFactor>-0.3</RangedCooldownFactor>  <!-- 중첩당 0.3 감소 -->
</statOffsets>
```
- **장점**: 무기 cooldown과 무관하게 항상 factor를 0.3 감소
- **단점**: factor가 0.3 미만이면 음수가 될 수 있음 (최소값 0.01로 제한됨)

### 옵션 2: statFactors 사용 (현재 방식)
```xml
<statFactors>
  <RangedCooldownFactor>0.8125</RangedCooldownFactor>  <!-- 18.75% 감소 -->
</statFactors>
```
- **장점**: 비율 기반으로 안정적
- **단점**: 무기 cooldown에 따라 실제 감소 시간이 달라짐

---

## 관련 파일 목록

- `RimworldData/Core/Defs/Stats/Stats_Pawns_Combat.xml` - RangedCooldownFactor 정의
- `RimworldData/Core/Defs/Stats/Stats_Weapons_Ranged.xml` - RangedWeapon_Cooldown 정의
- `RimworldData/Biotech/Defs/ThingDefs_Misc/Apparel_Various.xml` - RangedCooldownFactor 사용 예시

