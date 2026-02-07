# 방패 데미지 타입 분류 분석

## RimWorld DamageDef 속성 기준 분류

### 원거리 공격 (`isRanged = true`)

**RimWorld Core DamageDef:**
- Bullet - `isRanged: true`
- Arrow - `isRanged: true`
- ArrowHighVelocity - `isRanged: true` (부모: Arrow)
- RangedStab - `isRanged: true`
- Beam - `isRanged: true`

**특징:**
- `isRanged: true` 속성 설정
- 투사체 무기에서 사용
- 총알, 화살, 빔 등

### 근접 공격

**RimWorld Core DamageDef:**
- Cut - `isRanged: false` (설정 없음), `isExplosive: false` (설정 없음)
- Blunt - `isRanged: false`, `isExplosive: false`
- Stab - `isRanged: false`, `isExplosive: false`
- Scratch - `isRanged: false`, `isExplosive: false`
- Bite - `isRanged: false`, `isExplosive: false`
- Crush - `isRanged: false`, `isExplosive: false`
- Poke - `isRanged: false`, `isExplosive: false`
- Demolish - `isRanged: false`, `isExplosive: false`

**특징:**
- `isRanged: false` (또는 설정 없음, 기본값 false)
- `isExplosive: false` (또는 설정 없음, 기본값 false)
- Verb가 `IsMeleeAttack = true`인 경우

### 폭발 공격 (`isExplosive = true`)

**RimWorld Core DamageDef:**
- Bomb - `isExplosive: true`, `isRanged: false` (설정 없음)
- BombSuper - `isExplosive: true` (부모: Bomb), `isRanged: false`
- Thump - `isExplosive: true`, `isRanged: false`
- Vaporize - `isExplosive: false` (설정 없음), `isRanged: false` (설정 없음) - 특수 케이스

**특징:**
- `isExplosive: true` 속성 설정
- 폭발 무기, 폭탄 등에서 사용
- 범위 피해를 가짐

### 기타/특수 데미지

**RimWorld Core DamageDef:**
- Flame - `isRanged: false`, `isExplosive: false` (설정 없음) - 화염 데미지
- Burn - `isRanged: false`, `isExplosive: false` (부모: Flame)
- Frostbite - `isRanged: false`, `isExplosive: false`
- TornadoScratch - `isRanged: false`, `isExplosive: false`
- EMP - 특수 (실드 무시)
- Stun - 특수
- Smoke - `harmsHealth: false`
- Extinguish - `harmsHealth: false`

## 현재 코드의 분류 로직

```csharp
// 1순위: 원거리 공격
if (dinfo.Def.isRanged)
{
    // 원거리 처리
    damageReductionPercent = this.Props.damageReductionPercentRanged;
    staminaLossPerDamage = this.Props.staminaLossPerDamageRanged;
}

// 2순위: 근접 공격
else if (isMeleeAttack || (!dinfo.Def.isRanged && !dinfo.Def.isExplosive))
{
    // 근접 처리
    // 조건: Verb가 근접이거나, (isRanged = false && isExplosive = false)
    damageReductionPercent = this.Props.damageReductionPercentMelee;
    staminaLossPerDamage = this.Props.staminaLossPerDamageMelee;
}

// 3순위: 폭발/기타
else
{
    // 폭발 처리
    // 조건: isExplosive = true이거나 기타 모든 경우
    damageReductionPercent = this.Props.damageReductionPercentExplosive;
    staminaLossPerDamage = this.Props.staminaLossPerDamageExplosive;
}
```

## 각 DamageDef 타입별 처리 결과

### Bomb 데미지

**RimWorld Bomb DamageDef 속성:**
```xml
<DamageDef Name="Bomb">
    <isExplosive>true</isExplosive>
    <!-- isRanged 속성 없음 (기본값 false) -->
</DamageDef>
```

**현재 코드 처리:**
1. `isRanged = false` → 첫 번째 조건 false
2. `isExplosive = true` → 두 번째 조건도 false (`!isExplosive = false`)
3. → **else 분기로 폭발 처리됨** ✅

**결론:** Bomb은 올바르게 폭발로 처리됩니다.

### Bomb이 투사체로 처리될 수 있는 경우

만약 Bomb 데미지가 "투사체로 처리"된다면, 다음 경우일 수 있습니다:

1. **Bomb을 투사하는 무기가 원거리 무기인 경우**
   - 무기 자체는 원거리 (`Verb_Shoot` 등)
   - 하지만 데미지 타입은 Bomb (`isExplosive: true`)
   - 이 경우에도 Bomb DamageDef의 `isRanged = false`이므로 폭발로 처리되어야 함

2. **Bomb 데미지가 아닌 다른 데미지 타입을 사용하는 경우**
   - 원거리 무기에서 다른 DamageDef (예: Bullet)를 사용
   - 이 경우 `isRanged = true`이므로 원거리로 처리됨

## 주요 DamageDef 분류 표

| DamageDef | isRanged | isExplosive | 현재 처리 | 예상 처리 |
|-----------|----------|-------------|-----------|-----------|
| Bullet | true | false | 원거리 ✅ | 원거리 |
| Arrow | true | false | 원거리 ✅ | 원거리 |
| Beam | true | false | 원거리 ✅ | 원거리 |
| Cut | false | false | 근접 ✅ | 근접 |
| Blunt | false | false | 근접 ✅ | 근접 |
| Stab | false | false | 근접 ✅ | 근접 |
| Bomb | false | true | 폭발 ✅ | 폭발 |
| BombSuper | false | true | 폭발 ✅ | 폭발 |
| Thump | false | true | 폭발 ✅ | 폭발 |
| Flame | false | false | 근접 ⚠️ | 폭발? |
| Burn | false | false | 근접 ⚠️ | 폭발? |
| Vaporize | false | false | 근접 ⚠️ | 폭발? |
| TornadoScratch | false | false | 근접 ⚠️ | 근접? |

## 문제점 및 개선 사항

### 1. Flame/Burn/Vaporize 처리

**현재:** 근접으로 처리됨
**문제:** Flame은 화염 데미지이므로 폭발로 처리하는 것이 합리적일 수 있음

**해결 방안:**
- `explosionHeatEnergyPerCell` 속성이 있으면 폭발로 처리
- 또는 명시적으로 Flame 계열을 폭발로 분류

### 2. Bomb이 원거리로 처리되는 경우

**가능한 원인:**
- Bomb을 투사하는 원거리 무기의 Verb가 `isRanged = true`인 다른 DamageDef를 사용
- 또는 로그를 확인하여 실제 처리 방식 확인 필요

**확인 방법:**
- 로그에서 Bomb 데미지가 들어올 때 어떤 분류로 처리되는지 확인
- `isRanged`, `isExplosive`, `isMeleeAttack` 값 확인

## 로그 분석 권장사항

다음 정보를 로그에 추가하여 실제 분류를 확인:

```csharp
Log.Message($"[StaminaShield] DamageDef 분류 정보:");
Log.Message($"[StaminaShield]   - DamageDef: {dinfo.Def.defName}");
Log.Message($"[StaminaShield]   - isRanged: {dinfo.Def.isRanged}");
Log.Message($"[StaminaShield]   - isExplosive: {dinfo.Def.isExplosive}");
Log.Message($"[StaminaShield]   - isMeleeAttack (Verb): {isMeleeAttack}");
Log.Message($"[StaminaShield]   - 처리 타입: {처리타입}");
```

