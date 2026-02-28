# HeavyLance 돌진 Hediff 미적용 버그 리포트

> **태그**: HeavyLance LanceCharge Hediff ICompAbilityEffectOnJumpCompleted GetAbility includeTemporary CompEquippableAbility PawnFlyer RespawnPawn  
> **목적**: 돌진 완료 후 Exhaustion/Focus Hediff가 전혀 추가되지 않는 원인 분석

---

## 1. 증상

- **증상**: RK_HeavyLance 돌진(charge) 완료 후 `RK_Hediff_LanceChargeExhaustion`, `RK_Hediff_LanceChargeFocus` Hediff가 추가되지 않음
- **영향**: 좋은 버프(Focus)든 나쁜 디버프(Exhaustion)든 모두 적용되지 않음

---

## 2. 원인 분석

### 2.1 호출 흐름

```
[돌진 실행]
Verb_CastAbilityCharge (Verb_CastAbilityJump 상속)
  → TryCastShot() → JumpUtility.DoJump(..., this.ability, ..., this.JumpFlyerDef)
  → PawnFlyer.MakeFlyer(..., triggeringAbility, target)
  → PawnFlyer 생성, pawn이 innerContainer에 담김

[비행 완료 시]
PawnFlyer.TickInterval() → RespawnPawn() → Destroy()
  → RespawnPawn() 내부:
     Ability ability = pawn.abilities.GetAbility(this.triggeringAbility, false);  // ← 여기서 null 반환
     if (ability != null && ability.comps != null)
       foreach → ICompAbilityEffectOnJumpCompleted.OnJumpCompleted()  // ← 실행되지 않음
```

### 2.2 근본 원인: `GetAbility(..., false)` vs 장비 부여 능력

**PawnFlyer.RespawnPawn()** (RimworldSource):

```csharp
Ability ability = pawn.abilities.GetAbility(this.triggeringAbility, false);
```

- 두 번째 인자 `false` = `includeTemporary`
- `includeTemporary == false` → **영구 능력(`pawn.abilities`)만 검색**
- `includeTemporary == true` → **AllAbilitiesForReading** 검색 (장비, 헤딥, 의복 등 포함)

**RK_Ability_LanceCharge**는 **CompEquippableAbility**로 무기(RK_HeavyLance)에서 부여됨:

- 장비 부여 능력은 `pawn.abilities`에 저장되지 않음
- `CompEquippableAbility.AbilityForReading`로 동적 제공
- `AllAbilitiesForReading`에만 포함됨 (임시 능력으로 취급)

따라서 `GetAbility(RK_Ability_LanceCharge, false)` → **항상 null** → OnJumpCompleted 콜백 미실행 → Hediff 미적용.

### 2.3 Pawn_AbilityTracker.GetAbility 동작

```csharp
// Pawn_AbilityTracker.cs
public Ability GetAbility(AbilityDef def, bool includeTemporary = false)
{
    List<Ability> list = includeTemporary ? this.AllAbilitiesForReading : this.abilities;
    // ...
}
```

- `this.abilities`: 영구 능력 (GainAbility로 추가된 것)
- `AllAbilitiesForReading`: abilities + hediff + **equipment.Primary.CompEquippableAbility** + apparel + mutant 등

---

## 3. 영향 범위

| 능력 출처                 | GetAbility(def, false) | GetAbility(def, true) |
|---------------------------|------------------------|------------------------|
| pawn.abilities (영구)     | ✓ 찾음                 | ✓ 찾음                 |
| CompEquippableAbility     | ✗ null                 | ✓ 찾음                 |
| Hediff abilities          | ✗ null                 | ✓ 찾음                 |
| Mutant abilities          | ✗ null                 | ✓ 찾음                 |

**RK_Ability_LanceCharge**는 CompEquippableAbility → `false`로는 절대 찾을 수 없음.

---

## 4. 해결 방안

### 4.1 권장: Harmony 패치

`PawnFlyer.RespawnPawn`에서 `GetAbility(triggeringAbility, false)` 호출을 `GetAbility(triggeringAbility, true)`로 변경.

- **방법 A**: Transpiler로 `ldc.i4.0` → `ldc.i4.1` 치환
- **방법 B**: Postfix에서 ability가 null일 때 `GetAbility(triggeringAbility, true)`로 재시도 후 OnJumpCompleted 호출

### 4.2 기타

- Rimworld 원본 수정 불가 (모드 프로젝트)
- ConsumeLeap 등 다른 점프 능력도 장비/헤딥 기반이면 동일 문제 가능

---

## 5. 관련 파일

| 구분 | 경로 |
|------|------|
| Hediff 적용 로직 | `Project/1.6/Source/HeavyLanceCharge/CompAbilityEffect_ChargeOnJump.cs` |
| 문제 발생 지점 | `RimworldSource/RimWorld/PawnFlyer.cs` RespawnPawn() |
| Ability 조회 | `RimworldSource/RimWorld/Pawn_AbilityTracker.cs` GetAbility() |
| 장비 능력 제공 | `RimworldSource/RimWorld/CompEquippableAbility.cs` |
