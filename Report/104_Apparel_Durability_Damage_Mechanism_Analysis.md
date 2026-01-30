# Apparel Durability Damage Mechanism Analysis
# 의류 내구도 피해 메커니즘 분석
# 방패 스태미나 없을 때 내구도 감소 문제

## 개요

림월드에서 의류(방어구)의 내구도가 깎이는 조건과 메커니즘을 분석하고, 방패가 스태미나가 없을 때 내구도가 빠르게 감소하는 문제를 조사합니다.

## 핵심 발견사항

### 1. 의류 내구도 감소 조건

의류의 내구도는 다음 경우에 감소합니다:

1. **방어구가 피해를 흡수/감소시킬 때** (`ArmorUtility.ApplyArmor`)
   - 피해량의 **25%**가 의류 내구도에 직접 피해로 전달됨
   - 위치: `RimworldSource/Verse/ArmorUtility.cs:73-74`

2. **파운이 죽을 때** (`Pawn_ApparelTracker.Notify_PawnKilled`)
   - 외부 폭력으로 죽은 경우, 의류 내구도의 15-40%가 감소
   - 위치: `RimworldSource/RimWorld/Pawn_ApparelTracker.cs:687-688`

3. **일상적인 마모** (`Pawn_ApparelTracker.TakeWearoutDamageForDay`)
   - 매일 `wearPerDay` 값만큼 내구도 감소
   - 위치: `RimworldSource/RimWorld/Pawn_ApparelTracker.cs:450-456`

4. **화재 피해**
   - 위치: `RimworldSource/RimWorld/Fire.cs:428`

### 2. 피해 처리 흐름

```
Pawn.TakeDamage()
  ↓
Pawn_HealthTracker.PreApplyDamage()
  ↓
Apparel.CheckPreAbsorbDamage()  ← 방패가 여기서 피해 흡수 시도
  ↓ (방패가 흡수하지 못하면)
DamageWorker_AddInjury.ApplyToPawn()
  ↓
ArmorUtility.GetPostArmorDamage()
  ↓
ArmorUtility.ApplyArmor()  ← 여기서 의류가 피해의 25%를 받음
```

### 3. 방패의 피해 흡수 메커니즘

**CompShield.PostPreApplyDamage** (`RimworldSource/RimWorld/CompShield.cs:247-278`)

```csharp
public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
{
    absorbed = false;
    if (this.ShieldState != ShieldState.Active || this.PawnOwner == null)
    {
        return;  // 스태미나 없거나 비활성화 시 피해 흡수 안 함
    }
    if (dinfo.Def.isRanged || dinfo.Def.isExplosive)
    {
        this.energy -= dinfo.Amount * this.Props.energyLossPerDamage;
        if (this.energy < 0f)
        {
            this.Break();
        }
        else
        {
            this.AbsorbedDamage(dinfo);
        }
        absorbed = true;  // 피해 완전 흡수
        return;
    }
}
```

**조건:**
- 방패가 `ShieldState.Active` 상태여야 함
- 스태미나(energy)가 충분해야 함
- 원거리 또는 폭발 피해만 흡수 가능
- EMP 피해는 방패를 즉시 파괴

### 4. 문제점 분석

**방패가 스태미나가 없을 때:**

1. `CompShield.PostPreApplyDamage`에서 `absorbed = false` 반환
2. 피해가 그대로 `ArmorUtility.GetPostArmorDamage`로 전달됨
3. 의류가 방어를 시도하면서 **피해량의 25%**를 내구도로 받음
4. 결과적으로 의류 내구도가 빠르게 감소

**코드 위치:**
- `RimworldSource/Verse/ArmorUtility.cs:73-74`
```csharp
if (armorThing != null)
{
    float f = damAmount * 0.25f;
    armorThing.TakeDamage(new DamageInfo(damageDef, (float)GenMath.RoundRandom(f), 0f, -1f, ...));
}
```

## 상세 분석

### ArmorUtility.ApplyArmor 메서드

```csharp
private static void ApplyArmor(ref float damAmount, float armorPenetration, float armorRating, Thing armorThing, ref DamageDef damageDef, Pawn pawn, out bool metalArmor)
{
    // ...
    if (armorThing != null)
    {
        // 의류가 피해의 25%를 내구도로 받음
        float f = damAmount * 0.25f;
        armorThing.TakeDamage(new DamageInfo(damageDef, (float)GenMath.RoundRandom(f), 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true, false));
    }
    // 방어 계산...
}
```

**의미:**
- 방어구가 피해를 흡수하거나 감소시킬 때마다 피해량의 25%가 의류 내구도에 전달됨
- 이는 방어구가 "작동"하고 있다는 것을 의미
- 방패가 피해를 흡수하지 못하면, 의류가 더 많은 피해를 받게 됨

### 방패가 피해를 흡수할 때

- 방패가 활성화되어 있고 스태미나가 충분하면:
  - `absorbed = true` 반환
  - 피해가 완전히 흡수되어 `ArmorUtility`로 전달되지 않음
  - **의류 내구도 감소 없음**

### 방패가 피해를 흡수하지 못할 때

- 방패가 비활성화되어 있거나 스태미나가 부족하면:
  - `absorbed = false` 반환
  - 피해가 그대로 전달되어 `ArmorUtility`에서 처리됨
  - 의류가 방어를 시도하면서 피해의 25%를 내구도로 받음
  - **의류 내구도가 빠르게 감소**

## 결론

1. **의류 내구도 감소 조건:**
   - 방어구가 피해를 흡수/감소시킬 때: 피해량의 25%
   - 파운이 외부 폭력으로 죽을 때: 내구도의 15-40%
   - 일상적인 마모: 매일 `wearPerDay` 값

2. **방패 스태미나 부족 시 문제:**
   - 방패가 피해를 흡수하지 못하면 모든 피해가 의류로 전달됨
   - 의류가 방어를 시도할 때마다 피해의 25%가 내구도로 전달됨
   - 결과적으로 의류 내구도가 빠르게 감소

3. **해결 방안 고려사항:**
   - 방패가 스태미나가 없을 때도 최소한의 피해 흡수 기능 제공
   - 의류 내구도 감소율 조정 (방패 상태에 따라)
   - 방패가 피해를 흡수하지 못할 때 의류 내구도 감소율 감소

## 참고 파일

- `RimworldSource/Verse/ArmorUtility.cs` - 방어 계산 및 의류 피해 처리
- `RimworldSource/RimWorld/CompShield.cs` - 방패 피해 흡수 로직
- `RimworldSource/Verse/Pawn_HealthTracker.cs` - 피해 처리 흐름
- `RimworldSource/RimWorld/Pawn_ApparelTracker.cs` - 의류 관리 및 피해 처리
- `RimworldSource/Verse/DamageWorker_AddInjury.cs` - 파운 피해 처리
