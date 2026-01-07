# DamageInfo를 통한 근접/원거리/그외 구분 방법 분석

## 개요

`DamageDef`의 `isRanged` 속성 대신 `DamageInfo` 구조체의 정보를 활용하여 데미지가 근접 공격인지, 원거리 공격인지, 그 외인지 구분하는 방법을 분석합니다.

## DamageInfo 구조체 주요 필드

```csharp
public struct DamageInfo
{
    public ThingDef Weapon { get; }           // 무기 정의
    public Thing Instigator { get; }          // 데미지를 가한 주체
    public Tool Tool { get; }                 // 근접 무기의 Tool 정보
    public DamageDef Def { get; }             // 데미지 타입
    public DamageInfo.SourceCategory Category { get; }  // 소스 카테고리
    // ... 기타 필드들
}
```

## 구분 방법

### 방법 1: Tool 속성 확인 (가장 간단)

**로직:**
- `Tool`이 있으면 → **근접 공격**
- `Tool`이 없으면 → 원거리 또는 그 외

**장점:**
- 가장 간단하고 빠름
- 근접 무기는 항상 Tool을 가짐

**단점:**
- Tool이 없는 근접 공격(예: 주먹)은 구분 불가
- 원거리와 그 외를 구분하지 못함

**코드 예시:**
```csharp
if (dinfo.Tool != null)
{
    // 근접 공격
    return AttackType.Melee;
}
else
{
    // 원거리 또는 그 외
    // 추가 확인 필요
}
```

### 방법 2: Weapon의 Verbs 확인 (가장 정확)

**로직:**
- `Weapon`이 있으면 → `Weapon.Verbs` 확인
- 각 `VerbProperties`의 `IsMeleeAttack` 또는 `Ranged` 속성 확인

**VerbProperties 속성:**
```csharp
public bool IsMeleeAttack
{
    get
    {
        return typeof(Verb_MeleeAttack).IsAssignableFrom(this.verbClass);
    }
}

public bool Ranged
{
    get
    {
        return this.LaunchesProjectile || 
               typeof(Verb_ShootBeam).IsAssignableFrom(this.verbClass) || 
               typeof(Verb_SpewFire).IsAssignableFrom(this.verbClass) || 
               typeof(Verb_Spray).IsAssignableFrom(this.verbClass);
    }
}
```

**코드 예시:**
```csharp
private AttackType GetAttackTypeFromDamageInfo(DamageInfo dinfo)
{
    // 1. Tool이 있으면 근접 공격
    if (dinfo.Tool != null)
    {
        return AttackType.Melee;
    }
    
    // 2. Weapon이 있으면 Verbs 확인
    if (dinfo.Weapon != null && dinfo.Weapon.Verbs != null)
    {
        foreach (VerbProperties verbProps in dinfo.Weapon.Verbs)
        {
            if (verbProps.IsMeleeAttack)
            {
                return AttackType.Melee;
            }
            if (verbProps.Ranged)
            {
                return AttackType.Ranged;
            }
        }
    }
    
    // 3. Instigator가 Pawn이면 현재 사용 중인 Verb 확인
    if (dinfo.Instigator is Pawn instigatorPawn)
    {
        Verb currentVerb = instigatorPawn.CurJob?.verbToUse;
        if (currentVerb != null)
        {
            if (currentVerb.verbProps.IsMeleeAttack)
            {
                return AttackType.Melee;
            }
            if (currentVerb.verbProps.Ranged)
            {
                return AttackType.Ranged;
            }
        }
        
        // Pawn의 PrimaryVerb 확인
        Verb primaryVerb = instigatorPawn.equipment?.PrimaryEq?.PrimaryVerb;
        if (primaryVerb != null)
        {
            if (primaryVerb.verbProps.IsMeleeAttack)
            {
                return AttackType.Melee;
            }
            if (primaryVerb.verbProps.Ranged)
            {
                return AttackType.Ranged;
            }
        }
    }
    
    // 4. 그 외 (폭발, 환경 데미지 등)
    return AttackType.Other;
}
```

### 방법 3: Instigator의 현재 Verb 확인 (실시간 확인)

**로직:**
- `Instigator`가 `Pawn`이면 → 현재 작업 중인 `Verb` 확인
- 또는 장비 중인 무기의 `PrimaryVerb` 확인

**코드 예시:**
```csharp
if (dinfo.Instigator is Pawn attacker)
{
    // 현재 작업 중인 Verb
    Verb currentVerb = attacker.CurJob?.verbToUse;
    if (currentVerb != null)
    {
        if (currentVerb.verbProps.IsMeleeAttack)
            return AttackType.Melee;
        if (currentVerb.verbProps.Ranged)
            return AttackType.Ranged;
    }
    
    // 장비 중인 무기의 PrimaryVerb
    Verb primaryVerb = attacker.equipment?.PrimaryEq?.PrimaryVerb;
    if (primaryVerb != null)
    {
        if (primaryVerb.verbProps.IsMeleeAttack)
            return AttackType.Melee;
        if (primaryVerb.verbProps.Ranged)
            return AttackType.Ranged;
    }
}
```

## 통합 구분 로직 (권장)

다음 순서로 확인하여 가장 정확한 구분:

```csharp
public enum AttackType
{
    Melee,      // 근접 공격
    Ranged,     // 원거리 공격
    Other       // 그 외 (폭발, 환경 데미지 등)
}

private AttackType GetAttackType(DamageInfo dinfo)
{
    // 1순위: Tool 확인 (가장 빠름)
    if (dinfo.Tool != null)
    {
        return AttackType.Melee;
    }
    
    // 2순위: Weapon의 Verbs 확인
    if (dinfo.Weapon != null && dinfo.Weapon.Verbs != null)
    {
        bool hasMeleeVerb = false;
        bool hasRangedVerb = false;
        
        foreach (VerbProperties verbProps in dinfo.Weapon.Verbs)
        {
            if (verbProps.IsMeleeAttack)
            {
                hasMeleeVerb = true;
            }
            if (verbProps.Ranged)
            {
                hasRangedVerb = true;
            }
        }
        
        // 근접과 원거리 모두 있으면 우선순위 결정 필요
        // 일반적으로 근접이 우선 (근접 무기로 원거리 공격 불가)
        if (hasMeleeVerb)
        {
            return AttackType.Melee;
        }
        if (hasRangedVerb)
        {
            return AttackType.Ranged;
        }
    }
    
    // 3순위: Instigator의 현재 Verb 확인
    if (dinfo.Instigator is Pawn attacker)
    {
        Verb currentVerb = attacker.CurJob?.verbToUse ?? attacker.equipment?.PrimaryEq?.PrimaryVerb;
        if (currentVerb != null)
        {
            if (currentVerb.verbProps.IsMeleeAttack)
            {
                return AttackType.Melee;
            }
            if (currentVerb.verbProps.Ranged)
            {
                return AttackType.Ranged;
            }
        }
    }
    
    // 4순위: DamageDef의 isRanged 속성 확인 (폴백)
    if (dinfo.Def.isRanged)
    {
        return AttackType.Ranged;
    }
    
    // 5순위: 그 외
    return AttackType.Other;
}
```

## 각 방법의 장단점 비교

| 방법 | 정확도 | 성능 | 구현 난이도 | 권장도 |
|------|--------|------|-------------|--------|
| Tool 확인 | 중 | 높음 | 낮음 | ⭐⭐⭐ |
| Weapon Verbs 확인 | 높음 | 중 | 중 | ⭐⭐⭐⭐⭐ |
| Instigator Verb 확인 | 높음 | 낮음 | 높음 | ⭐⭐⭐⭐ |
| 통합 로직 | 매우 높음 | 중 | 높음 | ⭐⭐⭐⭐⭐ |

## 실제 사용 예시

### CompStaminaShield에 적용

```csharp
public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
{
    absorbed = false;
    
    // 공격 타입 구분
    AttackType attackType = GetAttackType(dinfo);
    
    float damageReductionPercent;
    float staminaLossPerDamage;
    
    switch (attackType)
    {
        case AttackType.Melee:
            damageReductionPercent = this.Props.damageReductionPercentMelee;
            staminaLossPerDamage = this.Props.staminaLossPerDamageMelee;
            break;
            
        case AttackType.Ranged:
            damageReductionPercent = this.Props.damageReductionPercentRanged;
            staminaLossPerDamage = this.Props.staminaLossPerDamageRanged;
            break;
            
        case AttackType.Other:
        default:
            damageReductionPercent = this.Props.damageReductionPercentExplosive;
            staminaLossPerDamage = this.Props.staminaLossPerDamageExplosive;
            break;
    }
    
    // ... 나머지 로직
}
```

## 주의사항

1. **Tool이 없는 근접 공격**
   - 주먹, 발차기 등은 Tool이 없을 수 있음
   - 이 경우 Weapon이나 Instigator Verb 확인 필요

2. **Weapon이 null인 경우**
   - 환경 데미지, 폭발 등은 Weapon이 null일 수 있음
   - 이 경우 `AttackType.Other`로 처리

3. **Instigator가 null인 경우**
   - 환경 데미지, 함정 등은 Instigator가 null일 수 있음
   - 이 경우 `AttackType.Other`로 처리

4. **성능 고려**
   - Tool 확인이 가장 빠르므로 먼저 확인
   - Weapon Verbs 확인은 반복문이 필요하므로 두 번째로 확인
   - Instigator Verb 확인은 가장 느리므로 마지막에 확인

## 결론

**3가지 구분 가능:**
- ✅ **근접 공격**: Tool이 있거나, Weapon의 Verb가 `IsMeleeAttack = true`
- ✅ **원거리 공격**: Weapon의 Verb가 `Ranged = true`이거나, DamageDef의 `isRanged = true`
- ✅ **그 외**: 위 조건에 해당하지 않는 모든 경우 (폭발, 환경 데미지 등)

**권장 방법:**
통합 로직을 사용하여 순차적으로 확인하되, 성능을 위해 Tool 확인을 먼저 수행하는 것이 좋습니다.

