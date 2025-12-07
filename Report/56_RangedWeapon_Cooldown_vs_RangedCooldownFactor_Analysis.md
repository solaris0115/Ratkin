# RangedWeapon_Cooldown vs RangedCooldownFactor 분석 보고서

**작성일**: 2025-01-XX  
**목적**: RangedWeapon_Cooldown과 RangedCooldownFactor의 차이 및 HediffDef에서 사용 가능 여부 확인

---

## 핵심 발견

### ❌ RangedWeapon_Cooldown은 무기(ThingDef)의 Stat입니다
- **카테고리**: `Weapon_Ranged`
- **용도**: 무기 자체의 재장전 시간 정의
- **위치**: `RimworldData/Core/Defs/Stats/Stats_Weapons_Ranged.xml`
- **HediffDef에서 사용**: ❌ 불가능 (Pawn의 Stat이 아님)

### ✅ RangedCooldownFactor는 Pawn의 Stat입니다
- **카테고리**: `PawnCombat`
- **용도**: 무기 cooldown에 곱해지는 multiplier
- **위치**: `RimworldData/Core/Defs/Stats/Stats_Pawns_Combat.xml`
- **HediffDef에서 사용**: ✅ 가능 (Pawn의 Stat)

---

## Cooldown 계산 공식

```
최종 Cooldown = 무기의 RangedWeapon_Cooldown × Pawn의 RangedCooldownFactor
```

**코드 위치**: `RimworldSource/Verse/VerbProperties.cs:563, 573`

```csharp
// 무기의 cooldown 가져오기
num = equipment.GetStatValue(StatDefOf.RangedWeapon_Cooldown, true, -1);

// Pawn의 factor 곱하기
num *= attacker.GetStatValue(StatDefOf.RangedCooldownFactor, true, -1);
```

---

## 예시 계산

### 기본 상황
- 무기 cooldown: 1.6초
- Pawn factor: 1.0 (기본값)
- 최종 cooldown: 1.6 × 1.0 = **1.6초**

### Hediff로 0.3초 감소 시도

#### 방법 1: statFactors 사용
```xml
<statFactors>
  <RangedCooldownFactor>0.8125</RangedCooldownFactor>  <!-- 1.3 / 1.6 -->
</statFactors>
```
- 최종 cooldown: 1.6 × 0.8125 = **1.3초** ✅ (0.3초 감소)

**문제점**: 무기 cooldown이 다르면 효과가 달라짐
- 무기 cooldown 2.0초 → 최종 1.625초 (0.375초 감소)
- 무기 cooldown 1.0초 → 최종 0.8125초 (0.1875초 감소)

#### 방법 2: statOffsets 사용
```xml
<statOffsets>
  <RangedCooldownFactor>-0.1875</RangedCooldownFactor>  <!-- 1.0 - 0.1875 = 0.8125 -->
</statOffsets>
```
- factor: 1.0 - 0.1875 = 0.8125
- 최종 cooldown: 1.6 × 0.8125 = **1.3초** ✅

**문제점**: 동일 (무기 cooldown에 따라 효과가 달라짐)

---

## 정확한 0.3초 감소를 위한 해결책

### ✅ 방법: Verb에서 직접 조정

`Verb_RatHolicGun`에서 `AdjustedCooldown()`을 오버라이드하여 Hediff 개수에 따라 직접 감소:

```csharp
public override float AdjustedCooldown(Pawn attacker)
{
    float baseCooldown = base.AdjustedCooldown(attacker);
    
    int stackCount = GetSpoolingHediffCount(attacker);
    float reduction = 0.3f * stackCount;  // 중첩당 0.3초 감소
    
    return Mathf.Max(0.01f, baseCooldown - reduction);
}
```

**장점**:
- 무기 cooldown과 무관하게 정확히 0.3초씩 감소
- 최대 5중첩 시 정확히 1.5초 감소

**단점**:
- HediffDef의 statOffsets/statFactors는 표시용으로만 사용

---

## 결론

1. **RangedWeapon_Cooldown**: 무기의 Stat → HediffDef에서 수정 불가
2. **RangedCooldownFactor**: Pawn의 Stat → HediffDef에서 수정 가능
3. **정확한 0.3초 감소**: Verb에서 직접 조정 필요

---

## 관련 파일 목록

- `RimworldData/Core/Defs/Stats/Stats_Weapons_Ranged.xml` - RangedWeapon_Cooldown 정의
- `RimworldData/Core/Defs/Stats/Stats_Pawns_Combat.xml` - RangedCooldownFactor 정의
- `RimworldSource/Verse/VerbProperties.cs` - Cooldown 계산 로직

