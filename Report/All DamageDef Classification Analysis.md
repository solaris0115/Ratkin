# 모든 DamageDef 원거리/근거리 분류 분석

## 개요

RimWorld Core 및 DLC의 모든 DamageDef를 열거하고, 각각이 원거리인지 근거리인지 확인합니다. 원거리 판단이 실패한 경우 `isRanged` 속성을 체크합니다.

## 현재 분류 로직

```csharp
private AttackType GetAttackType(DamageInfo dinfo)
{
    // 1순위: 폭발 공격 확인 (최우선, 근접/원거리 무시)
    if (dinfo.Def.isExplosive)
        return AttackType.Explosive;
    
    // 2순위: ignoreShields 또는 EMP는 ETC로 처리 (통과)
    if (dinfo.Def.ignoreShields || dinfo.Def == DamageDefOf.EMP)
        return AttackType.Etc;
    
    // 3순위: Tool 확인 (근접 무기는 항상 Tool을 가짐)
    if (dinfo.Tool != null)
        return AttackType.Melee;
    
    // 4순위: Weapon의 Verbs 확인
    // 5순위: Instigator의 Verb 확인
    // 6순위: DamageDef의 isRanged 속성 확인 (폴백)
    if (dinfo.Def.isRanged)
        return AttackType.Ranged;
    
    // 7순위: 그 외 (ETC - 통과)
    return AttackType.Etc;
}
```

## 모든 DamageDef 분류

### Core DamageDef

#### 원거리 무기 (`isRanged: true`)

| DamageDef | isRanged | 부모 | 판단 방법 | 분류 결과 |
|-----------|----------|------|-----------|-----------|
| RangedStab | true | - | isRanged 속성 | ✅ 원거리 |
| Bullet | true | - | isRanged 속성 | ✅ 원거리 |
| Arrow | true | - | isRanged 속성 | ✅ 원거리 |
| ArrowHighVelocity | true | Arrow | 부모 상속 | ✅ 원거리 |
| Beam | true | - | isRanged 속성 | ✅ 원거리 |
| Nerve | true | Arrow | 부모 상속 | ✅ 원거리 |

**특징:**
- 모두 `isRanged: true` 속성을 가지고 있음
- 6순위 `isRanged` 체크로 원거리로 분류됨

#### 근접 무기 (`isRanged: false` 또는 없음)

| DamageDef | isRanged | isExplosive | 부모 | 판단 방법 | 분류 결과 |
|-----------|----------|-------------|------|-----------|-----------|
| Cut | false | false | CutBase | Tool 또는 Weapon Verbs | ✅ 근접 |
| Crush | false | false | - | Tool 또는 Weapon Verbs | ✅ 근접 |
| Blunt | false | false | BluntBase | Tool 또는 Weapon Verbs | ✅ 근접 |
| Poke | false | false | BluntBase | Tool 또는 Weapon Verbs | ✅ 근접 |
| Demolish | false | false | BluntBase | Tool 또는 Weapon Verbs | ✅ 근접 |
| Stab | false | false | - | Tool 또는 Weapon Verbs | ✅ 근접 |
| Scratch | false | false | Scratch | Tool 또는 Weapon Verbs | ✅ 근접 |
| ScratchToxic | false | false | Scratch | Tool 또는 Weapon Verbs | ✅ 근접 |
| Bite | false | false | Bite | Tool 또는 Weapon Verbs | ✅ 근접 |
| ToxicBite | false | false | Bite | Tool 또는 Weapon Verbs | ✅ 근접 |

**특징:**
- 모두 `isRanged: false` 또는 설정 없음 (기본값 false)
- Tool이 있으면 3순위에서 근접으로 분류
- Weapon의 Verbs에 `IsMeleeAttack = true`가 있으면 4순위에서 근접으로 분류
- 그 외는 ETC로 분류될 수 있음 (판단 실패 시)

#### 폭발 공격 (`isExplosive: true`)

| DamageDef | isRanged | isExplosive | 부모 | 판단 방법 | 분류 결과 |
|-----------|----------|-------------|------|-----------|-----------|
| Bomb | false | true | - | isExplosive (1순위) | ✅ 폭발 |
| BombSuper | false | true | Bomb | 부모 상속 | ✅ 폭발 |
| Thump | false | true | - | isExplosive (1순위) | ✅ 폭발 |

**특징:**
- 모두 `isExplosive: true` 속성을 가지고 있음
- 1순위에서 폭발로 분류됨 (근접/원거리 무시)

#### 환경 데미지

| DamageDef | isRanged | isExplosive | 부모 | 판단 방법 | 분류 결과 |
|-----------|----------|-------------|------|-----------|-----------|
| Flame | false | false | - | 판단 실패 → ETC | ⚠️ ETC (통과) |
| Burn | false | false | Flame | 판단 실패 → ETC | ⚠️ ETC (통과) |
| Frostbite | false | false | - | 판단 실패 → ETC | ⚠️ ETC (통과) |
| TornadoScratch | false | false | - | 판단 실패 → ETC | ⚠️ ETC (통과) |

**특징:**
- Tool 없음, Weapon 없음, Instigator 없을 수 있음
- `isRanged: false`이므로 원거리 아님
- 근접/원거리 판단 실패 → ETC로 분류되어 통과

#### 특수 데미지

| DamageDef | isRanged | isExplosive | ignoreShields | 부모 | 판단 방법 | 분류 결과 |
|-----------|----------|-------------|---------------|------|-----------|-----------|
| Deterioration | false | false | - | - | 판단 실패 → ETC | ⚠️ ETC (통과) |
| Mining | false | false | - | - | 판단 실패 → ETC | ⚠️ ETC (통과) |
| Rotting | false | false | - | - | 판단 실패 → ETC | ⚠️ ETC (통과) |
| Extinguish | false | false | - | - | 판단 실패 → ETC | ⚠️ ETC (통과) |
| Smoke | false | false | - | - | 판단 실패 → ETC | ⚠️ ETC (통과) |
| Vaporize | false | false | - | - | 판단 실패 → ETC | ⚠️ ETC (통과) |
| AcidBurn | false | false | - | Flame | 판단 실패 → ETC | ⚠️ ETC (통과) |
| Decayed | false | false | - | - | 판단 실패 → ETC | ⚠️ ETC (통과) |
| NerveStun | false | false | - | StunBase | 판단 실패 → ETC | ⚠️ ETC (통과) |
| EMP | false | false | - | StunBase | EMP 체크 (2순위) | ✅ ETC (통과) |
| Stun | false | false | - | StunBase | 판단 실패 → ETC | ⚠️ ETC (통과) |
| SurgicalCut | false | false | - | CutBase | Tool 또는 Weapon Verbs | ✅ 근접? |
| ExecutionCut | false | false | - | CutBase | Tool 또는 Weapon Verbs | ✅ 근접? |

**특징:**
- EMP는 2순위에서 명시적으로 ETC로 분류
- 나머지는 Tool/Weapon/Instigator가 없으면 판단 실패 → ETC로 분류

### Biotech DLC DamageDef

| DamageDef | isRanged | 부모 | 판단 방법 | 분류 결과 |
|-----------|----------|------|-----------|-----------|
| BulletToxic | true | Bullet | 부모 상속 | ✅ 원거리 |
| MechBandShockwave | false | StunBase | 판단 실패 → ETC | ⚠️ ETC (통과) |

### Anomaly DLC DamageDef

| DamageDef | isRanged | 부모 | 판단 방법 | 분류 결과 |
|-----------|----------|------|-----------|-----------|
| Digested | false | - | 판단 실패 → ETC | ⚠️ ETC (통과) |
| EnergyBolt | true | - | isRanged 속성 | ✅ 원거리 |
| Psychic | false | - | 판단 실패 → ETC | ⚠️ ETC (통과) |
| DeadlifeDust | false | - | 판단 실패 → ETC | ⚠️ ETC (통과) |
| NociosphereVaporize | false | Vaporize | 판단 실패 → ETC | ⚠️ ETC (통과) |
| ElectricalBurn | false | Flame | 판단 실패 → ETC | ⚠️ ETC (통과) |

### Odyssey DLC DamageDef

| DamageDef | isRanged | 부모 | 판단 방법 | 분류 결과 |
|-----------|----------|------|-----------|-----------|
| Bullet_TraitTox | true | Bullet | 부모 상속 | ✅ 원거리 |
| Bullet_TraitIncendiary | true | Bullet | 부모 상속 | ✅ 원거리 |
| BeamBypassShields | true | Beam | 부모 상속 + ignoreShields | ✅ ETC (통과) |
| PorcupineBite | false | Bite | Tool 또는 Weapon Verbs | ✅ 근접 |
| PorcupineScratch | false | Scratch | Tool 또는 Weapon Verbs | ✅ 근접 |
| VacuumBurn | false | - | 판단 실패 → ETC | ⚠️ ETC (통과) |
| MiningBomb | false | Bomb | isExplosive (1순위) | ✅ 폭발 |

## 원거리 판단 실패 케이스 분석

### 판단 실패 원인

1. **Tool 없음**: 근접 무기가 아닌 경우
2. **Weapon 없음**: 환경 데미지, 특수 데미지 등
3. **Instigator 없음**: 환경 데미지, 함정 등
4. **isRanged = false**: 원거리 속성이 없음

### 판단 실패 시 처리

현재 로직:
- 6순위에서 `isRanged` 속성 체크
- `isRanged = true`이면 원거리로 분류
- 그 외는 ETC로 분류되어 통과

### 개선 방안

원거리 판단 실패 시 `isRanged` 속성을 체크하는 것은 이미 구현되어 있습니다 (6순위).

하지만 다음 케이스들은 주의가 필요합니다:

1. **BeamBypassShields**
   - `isRanged: true` (부모 Beam 상속)
   - `ignoreShields: true`
   - 현재: 2순위에서 ETC로 분류됨 (올바름)

2. **환경 데미지 (Flame, Burn, Frostbite 등)**
   - Tool/Weapon/Instigator 없음
   - `isRanged: false`
   - 현재: ETC로 분류되어 통과 (의도된 동작)

3. **특수 데미지 (EMP, Stun 등)**
   - EMP는 명시적으로 ETC로 분류
   - 나머지는 ETC로 분류되어 통과

## 분류 결과 요약

### 원거리 (Ranged)
- **Core**: RangedStab, Bullet, Arrow, ArrowHighVelocity, Beam, Nerve (6개)
- **Biotech**: BulletToxic (1개)
- **Anomaly**: EnergyBolt (1개)
- **Odyssey**: Bullet_TraitTox, Bullet_TraitIncendiary (2개)
- **합계**: 10개

### 근접 (Melee)
- **Core**: Cut, Crush, Blunt, Poke, Demolish, Stab, Scratch, ScratchToxic, Bite, ToxicBite, SurgicalCut, ExecutionCut (12개)
- **Odyssey**: PorcupineBite, PorcupineScratch (2개)
- **합계**: 14개 (Tool/Weapon 있을 때만)

### 폭발 (Explosive)
- **Core**: Bomb, BombSuper, Thump (3개)
- **Odyssey**: MiningBomb (1개)
- **합계**: 4개

### ETC (통과)
- **Core**: Deterioration, Mining, Rotting, Extinguish, Smoke, Vaporize, AcidBurn, Decayed, NerveStun, EMP, Stun, Flame, Burn, Frostbite, TornadoScratch (15개)
- **Biotech**: MechBandShockwave (1개)
- **Anomaly**: Digested, Psychic, DeadlifeDust, NociosphereVaporize, ElectricalBurn (5개)
- **Odyssey**: BeamBypassShields, VacuumBurn (2개)
- **합계**: 23개

## 결론

1. **원거리 판단**: `isRanged: true` 속성으로 정확히 분류됨 ✅
2. **근접 판단**: Tool 또는 Weapon Verbs로 분류됨 ✅
3. **폭발 판단**: `isExplosive: true` 속성으로 정확히 분류됨 ✅
4. **ETC 판단**: 위 조건에 해당하지 않으면 ETC로 분류되어 통과 ✅

**원거리 판단 실패 시 `isRanged` 체크는 이미 구현되어 있으며, 정상적으로 동작합니다.**

