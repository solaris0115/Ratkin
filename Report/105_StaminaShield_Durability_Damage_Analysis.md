# 스태미나 실드 내구도 피해 분석 보고서

**태그**: StaminaShield, Durability, Damage, Shield, 방패, 내구도, 25%, ArmorUtility

---

## 결론: **수정 가능** (Harmony 패치 필요)

---

## 데미지 파이프라인 분석

### 흐름도

```
Pawn 피격
    ↓
Pawn_HealthTracker.PreApplyDamage
    ↓
wornApparel[i].CheckPreAbsorbDamage(dinfo)
    ↓
CompStaminaShield.PostPreApplyDamage
    ↓
┌─────────────────────────────────────────┐
│ 스태미나 충분?                           │
├─────────────────────────────────────────┤
│ YES → absorbed=true, 피해 감소          │
│       + Comp 내구도 손상 (reducedDmg×0.25)│
│       → 파이프라인 종료                  │
├─────────────────────────────────────────┤
│ NO  → absorbed=false                    │
│       → 바닐라 파이프라인 계속          │
└─────────────────────────────────────────┘
    ↓ (absorbed=false인 경우)
DamageWorker_AddInjury.ApplyToPawn
    ↓
ArmorUtility.GetPostArmorDamage
    ↓
ArmorUtility.ApplyArmor  ← ★ 여기서 25% 내구도 피해 발생
```

---

## 핵심 코드 위치

### 1. CompStaminaShield (커스텀 코드)
- `CompStaminaShield.cs:477` - 스태미나 **충분 시** 내구도 손상
- `durabilityDamagePercent = 0.25f`

### 2. ArmorUtility (바닐라 코드) ★ 문제 지점
- `RimworldSource/Verse/ArmorUtility.cs:73-74`
```csharp
float f = damAmount * 0.25f;
armorThing.TakeDamage(new DamageInfo(damageDef, GenMath.RoundRandom(f), ...));
```
- **모든 방어구**가 피격 부위를 커버하면 `damAmount × 0.25` 내구도 피해

---

## 현재 동작

| 상황 | 피해 감소 | 내구도 손상 원천 |
|------|----------|-----------------|
| 스태미나 **충분** | O | Comp (reducedDamage × 0.25) |
| 스태미나 **부족** | X | **바닐라 ArmorUtility** (damAmount × 0.25) |

---

## 수정 방법

### 방법 1: Harmony 패치로 ArmorUtility 수정 (권장)
`ArmorUtility.ApplyArmor`를 Prefix/Transpiler로 패치하여 특정 Apparel에 대해 내구도 피해 비율 조정

```csharp
[HarmonyPatch(typeof(ArmorUtility), "ApplyArmor")]
public static class ArmorUtility_ApplyArmor_Patch
{
    static void Prefix(ref float damAmount, Thing armorThing, ...)
    {
        // CompStaminaShield가 있는 장비면 별도 처리
    }
}
```

### 방법 2: CompStaminaShield에서 항상 absorbed=true
스태미나 부족해도 absorbed=true로 설정하여 바닐라 파이프라인 진입 차단
→ 단, 피해 감소는 안 됨 (원래 데미지 그대로 적용)

### 방법 3: XML만으로는 불가능
바닐라 ArmorUtility는 하드코딩된 0.25f이므로 XML 조정 불가

---

## 권장 방향

1. **Harmony 패치**로 ArmorUtility.ApplyArmor에서 CompStaminaShield 보유 장비 감지
2. 해당 장비에 대해 내구도 피해 비율을 낮추거나 무시
3. 또는 CompStaminaShield 로직 수정으로 absorbed=true 유지하면서 내구도 피해만 조정
