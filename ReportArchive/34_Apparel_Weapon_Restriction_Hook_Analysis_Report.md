# Apparel-Weapon Tag 기반 동시 착용 제한 Hook 분석 보고서

## 개요
이 보고서는 Apparel과 Primary Weapon의 동시 착용을 제한하기 위한 Hook 포인트 분석 및 Harmony 없이 구현하는 방법을 정리합니다.

## 1. 현재 구현 상태

### 1.1 CompShieldWeaponIncompatible (기존 구현)

**파일**: `Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs`

```csharp
public class CompProperties_ShieldWeaponIncompatible : CompProperties
{
    /// <summary>
    /// 허용할 무기의 WeaponTag 리스트 (화이트리스트)
    /// </summary>
    public List<string> allowedWeaponTags;
    
    /// <summary>
    /// 경고 메시지 키 (번역 키)
    /// </summary>
    public string blockReasonKey = null;
}

public class CompShieldWeaponIncompatible : ThingComp
{
    public bool TryIsWeaponAllowed(ThingDef weaponDef, out string reason)
    {
        // allowedWeaponTags와 weaponDef.weaponTags를 비교
        // 허용된 태그가 하나라도 있으면 true 반환
    }
}
```

**현재 문제점**:
- ✅ allowedWeaponTags 리스트 구현됨
- ✅ TryIsWeaponAllowed() 검증 로직 있음
- ❌ **실제 착용 시점에 검증하는 로직 없음**
- ❌ FloatMenu에서 경고 표시 안 됨

---

## 2. RimWorld 착용 시스템 Hook 포인트

### 2.1 Apparel 착용 흐름

```
사용자가 Apparel 착용 명령 (FloatMenu에서 "Force Wear" 선택)
    ↓
JobDriver_Wear 실행
    ↓
Pawn_ApparelTracker.Wear(apparel, dropReplacedApparel, locked) ⭐
    ├─ 1. 기본 검증 (ApparelUtility.HasPartsToWear, PawnCanWear 등)
    ├─ 2. 충돌하는 기존 apparel 제거
    ├─ 3. wornApparel.TryAdd(apparel)
    └─ 4. Notify_ApparelAdded(apparel) 호출 ⭐
            ↓
        apparel.Notify_Equipped(pawn) 호출 ⭐ (862번째 줄)
            ↓
        base.Notify_Equipped(pawn) [ThingWithComps]
            ↓
        foreach (ThingComp comp in AllComps)
            comp.Notify_Equipped(pawn) ⭐⭐⭐ [Hook 포인트!]
```

### 2.2 Weapon 착용 흐름

```
사용자가 Weapon 장착 명령
    ↓
JobDriver_Equip 실행
    ↓
Pawn_EquipmentTracker.AddEquipment(weapon) ⭐
    ├─ 1. 기존 Primary 무기가 있으면 에러 (MakeRoomFor로 먼저 제거)
    ├─ 2. equipment.TryAdd(weapon)
    └─ 3. Notify_EquipmentAdded(weapon) 호출 ⭐
            ↓
        weapon.Notify_Equipped(pawn) 호출 ⭐ (349번째 줄)
            ↓
        foreach (ThingComp comp in AllComps)
            comp.Notify_Equipped(pawn) ⭐⭐⭐ [Hook 포인트!]
```

### 2.3 ThingComp.Notify_Equipped

**파일**: `RimworldSource/Verse/ThingComp.cs` (193-195번째 줄)

```csharp
public virtual void Notify_Equipped(Pawn pawn)
{
    // 빈 가상 메서드 - 오버라이드 가능!
}
```

**중요**: 이 메서드는 **virtual**이므로 **Harmony 없이 오버라이드 가능**합니다!

---

## 3. Harmony 없이 구현하는 방법 ✅

### 3.1 방법 1: Apparel 착용 시 Weapon 검증 (추천)

**Apparel에 CompShieldWeaponIncompatible을 추가하고, Apparel 착용 시 Weapon을 검증합니다.**

#### 구현 코드

```csharp
public class CompShieldWeaponIncompatible : ThingComp
{
    public CompProperties_ShieldWeaponIncompatible Props => 
        (CompProperties_ShieldWeaponIncompatible)this.props;
    
    // ✅ Harmony 없이 착용 시점 Hook!
    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        
        if (pawn?.equipment?.Primary == null)
        {
            return; // 착용한 무기가 없으면 OK
        }
        
        ThingWithComps primaryWeapon = pawn.equipment.Primary;
        string reason;
        
        if (!TryIsWeaponAllowed(primaryWeapon.def, out reason))
        {
            // 허용되지 않은 무기 착용 중 → 무기 제거
            DropIncompatibleWeapon(pawn, primaryWeapon, reason);
        }
    }
    
    private void DropIncompatibleWeapon(Pawn pawn, ThingWithComps weapon, string reason)
    {
        // 무기를 바닥에 떨어뜨림
        ThingWithComps droppedWeapon;
        if (pawn.equipment.TryDropEquipment(weapon, out droppedWeapon, pawn.Position, false))
        {
            // 메시지 표시
            if (!reason.NullOrEmpty() && PawnUtility.ShouldSendNotificationAbout(pawn))
            {
                Messages.Message(
                    "RK_ApparelWeaponIncompatible".Translate(
                        parent.Label, 
                        weapon.Label, 
                        reason
                    ),
                    pawn,
                    MessageTypeDefOf.CautionInput,
                    false
                );
            }
        }
    }
    
    public bool TryIsWeaponAllowed(ThingDef weaponDef, out string reason)
    {
        // 기존 로직 유지
        // ...
    }
}
```

#### 동작 흐름

```
1. 사용자가 방패(Apparel) 착용
    ↓
2. Pawn_ApparelTracker.Wear() 실행
    ↓
3. CompShieldWeaponIncompatible.Notify_Equipped(pawn) 호출 ⭐
    ↓
4. pawn.equipment.Primary 확인
    ↓
5. TryIsWeaponAllowed(weaponDef, out reason) 호출
    ↓
    ├─ 허용됨 → 아무것도 안 함
    └─ 허용 안 됨 → DropIncompatibleWeapon() 호출
            ↓
        pawn.equipment.TryDropEquipment(weapon, ...)
            ↓
        Messages.Message("방패와 양손 무기는 동시 착용 불가", ...)
```

**장점**:
- ✅ Harmony 불필요
- ✅ 모든 Apparel 착용 경로에서 자동 동작 (수동 착용, Job, 강제 착용 등)
- ✅ 기존 RimWorld 시스템과 완벽 호환
- ✅ 다른 모드와 충돌 가능성 거의 없음

**단점**:
- ⚠️ FloatMenu에서 미리 경고를 표시하지 못함 (착용 후 무기가 떨어짐)
- ⚠️ 사용자가 착용을 시도한 후에야 알 수 있음

---

### 3.2 방법 2: Weapon 착용 시 Apparel 검증 (대칭 구현)

**반대로, Weapon 착용 시 Apparel을 검증할 수도 있습니다.**

하지만 이 방법은 **Weapon에 Comp를 추가해야** 하므로 권장하지 않습니다:
- 모든 Weapon ThingDef를 수정해야 함 (Ratkin 무기뿐 아니라 바닐라 무기도)
- Apparel이 제한의 "주체"이므로 Apparel에서 검증하는 것이 논리적

---

## 4. Harmony를 사용하는 방법 (FloatMenu 경고 추가)

### 4.1 방법 3: FloatMenuOptionProvider_Wear Postfix Patch

**FloatMenu에서 미리 경고를 표시**하려면 Harmony가 필요합니다.

#### 구현 코드

```csharp
[HarmonyPatch(typeof(FloatMenuOptionProvider_Wear), "GetSingleOptionFor")]
public static class FloatMenuOptionProvider_Wear_ShieldWeaponCheck_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ref FloatMenuOption __result, Thing clickedThing, FloatMenuContext context)
    {
        // 이미 착용 불가로 표시된 경우 무시
        if (__result == null || __result.Disabled) return;
        
        Apparel apparel = clickedThing as Apparel;
        if (apparel == null) return;
        
        Pawn pawn = context.FirstSelectedPawn;
        if (pawn?.equipment?.Primary == null) return;
        
        // CompShieldWeaponIncompatible 검증
        CompShieldWeaponIncompatible comp = apparel.TryGetComp<CompShieldWeaponIncompatible>();
        if (comp != null)
        {
            string failReason;
            if (!comp.TryIsWeaponAllowed(pawn.equipment.Primary.def, out failReason))
            {
                // 착용 불가 옵션으로 대체
                string key = apparel.def.apparel.LastLayer.IsUtilityLayer 
                    ? "CannotEquipApparel" 
                    : "CannotWear";
                    
                __result = new FloatMenuOption(
                    key.Translate(apparel.Label) + ": " + failReason, 
                    null, // action = null → 비활성화
                    MenuOptionPriority.Default, 
                    null, null, 0f, null, null, true, 0
                );
            }
        }
    }
}
```

#### 동작 흐름

```
1. 사용자가 Apparel 우클릭
    ↓
2. FloatMenuOptionProvider_Wear.GetSingleOptionFor() 실행
    ├─ 기본 검증 (경로, 불타는지, 성별 등)
    ├─ FloatMenuOption 생성 ("Force Wear")
    └─ return floatMenuOption
        ↓
3. [Harmony Postfix] FloatMenuOptionProvider_Wear_ShieldWeaponCheck_Patch.Postfix() ⭐
    ├─ CompShieldWeaponIncompatible 확인
    ├─ TryIsWeaponAllowed() 호출
    └─ 허용 안 됨 → __result를 착용 불가 옵션으로 대체
            ↓
4. FloatMenu 표시:
   ❌ "착용 불가 (Cannot Wear): 양손 무기와 함께 착용 불가"
```

**장점**:
- ✅ 사용자가 착용 전에 경고를 볼 수 있음
- ✅ FloatMenu에서 회색으로 표시되어 클릭 불가

**단점**:
- ❌ Harmony 필요
- ⚠️ 다른 모드가 같은 메서드를 Patch할 경우 충돌 가능성

---

## 5. 추천 구현 방식

### 5.1 1단계: Harmony 없이 구현 (필수)

**CompShieldWeaponIncompatible.Notify_Equipped() 오버라이드**

```csharp
public override void Notify_Equipped(Pawn pawn)
{
    base.Notify_Equipped(pawn);
    
    if (pawn?.equipment?.Primary == null) return;
    
    string reason;
    if (!TryIsWeaponAllowed(pawn.equipment.Primary.def, out reason))
    {
        DropIncompatibleWeapon(pawn, pawn.equipment.Primary, reason);
    }
}
```

**결과**:
- ✅ Apparel 착용 시 자동으로 호환되지 않는 무기 제거
- ✅ 사용자에게 메시지 표시
- ✅ Harmony 불필요

### 5.2 2단계: Harmony로 UX 개선 (선택)

**FloatMenuOptionProvider_Wear Postfix Patch**

```csharp
[HarmonyPatch(typeof(FloatMenuOptionProvider_Wear), "GetSingleOptionFor")]
public static void Postfix(ref FloatMenuOption __result, ...)
{
    // FloatMenu에서 미리 경고 표시
}
```

**결과**:
- ✅ FloatMenu에서 착용 전에 경고 표시
- ✅ 더 나은 사용자 경험

---

## 6. 반대 경우: Weapon 착용 시 Apparel 검증

**사용자가 무기를 장착할 때 호환되지 않는 Apparel을 제거하려면?**

### 6.1 문제점

Weapon에는 "허용된 Apparel Tag"를 정의할 수 없습니다:
- **Apparel**이 제한의 주체 (Apparel이 "나는 이런 무기와만 착용 가능"이라고 정의)
- Weapon은 수동적 객체 (Weapon은 Apparel에 대해 알 필요 없음)

### 6.2 구현 방법 (비추천)

만약 **반드시** Weapon 착용 시에도 검증하고 싶다면:

#### 방법 A: Pawn_EquipmentTracker.AddEquipment Prefix Patch (Harmony)

```csharp
[HarmonyPatch(typeof(Pawn_EquipmentTracker), "AddEquipment")]
public static class Pawn_EquipmentTracker_AddEquipment_Patch
{
    [HarmonyPrefix]
    public static void Prefix(Pawn ___pawn, ThingWithComps newEq)
    {
        if (___pawn?.apparel == null || newEq == null) return;
        
        // 착용 중인 Apparel 검사
        foreach (Apparel apparel in ___pawn.apparel.WornApparel)
        {
            CompShieldWeaponIncompatible comp = apparel.TryGetComp<CompShieldWeaponIncompatible>();
            if (comp != null)
            {
                string reason;
                if (!comp.TryIsWeaponAllowed(newEq.def, out reason))
                {
                    // Apparel 제거
                    ___pawn.apparel.Remove(apparel);
                    // 메시지 표시
                    Messages.Message($"{apparel.Label}을(를) 벗었습니다: {reason}", ...);
                }
            }
        }
    }
}
```

**단점**:
- ❌ Harmony 필수
- ❌ 로직이 복잡해짐
- ❌ Apparel이 여러 개일 경우 처리 복잡

---

## 7. 핵심 발견사항 정리

### 7.1 Harmony 없이 구현 가능한 Hook 포인트

| Hook 포인트 | 호출 시점 | Harmony 필요 여부 | 용도 |
|-------------|----------|-------------------|------|
| **ThingComp.Notify_Equipped(pawn)** | Apparel/Weapon 착용 완료 후 | ❌ 불필요 | ⭐⭐⭐ 착용 시 검증 및 제거 |
| **ThingComp.Notify_Unequipped(pawn)** | Apparel/Weapon 제거 완료 후 | ❌ 불필요 | 정리 작업 |
| **Apparel.AllowVerbCast(verb)** | Verb 사용 시도 시 | ❌ 불필요 | 특정 공격 차단 (이미 구현됨) |

### 7.2 Harmony가 필요한 확장 포인트

| 확장 포인트 | 호출 시점 | 용도 |
|-------------|----------|------|
| **FloatMenuOptionProvider_Wear.GetSingleOptionFor** (Postfix) | FloatMenu 생성 시 | FloatMenu에서 미리 경고 표시 |
| **Pawn_EquipmentTracker.AddEquipment** (Prefix) | Weapon 장착 전 | Weapon 장착 시 Apparel 제거 |

### 7.3 추천 구현 순서

1. ✅ **1단계 (필수, Harmony 불필요)**:
   - `CompShieldWeaponIncompatible.Notify_Equipped(pawn)` 오버라이드
   - Apparel 착용 시 호환되지 않는 Weapon 제거

2. ✅ **2단계 (선택, Harmony 필요)**:
   - `FloatMenuOptionProvider_Wear.GetSingleOptionFor` Postfix Patch
   - FloatMenu에서 미리 경고 표시

3. ⚠️ **3단계 (선택, Harmony 필요, 비추천)**:
   - `Pawn_EquipmentTracker.AddEquipment` Prefix Patch
   - Weapon 장착 시 호환되지 않는 Apparel 제거

---

## 8. 결론

### 8.1 Harmony 없이 구현 가능! ✅

**ThingComp.Notify_Equipped(pawn)**을 오버라이드하면 Harmony 없이도 Apparel-Weapon 동시 착용 제한을 구현할 수 있습니다.

### 8.2 구현 핵심

```csharp
public class CompShieldWeaponIncompatible : ThingComp
{
    // ⭐ 이 메서드만 추가하면 Harmony 불필요!
    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        
        // Weapon 검증 및 제거
        if (pawn?.equipment?.Primary != null)
        {
            string reason;
            if (!TryIsWeaponAllowed(pawn.equipment.Primary.def, out reason))
            {
                DropIncompatibleWeapon(pawn, pawn.equipment.Primary, reason);
            }
        }
    }
}
```

### 8.3 Harmony는 UX 개선용으로 선택적 사용

- FloatMenu에서 미리 경고를 표시하고 싶다면 Harmony 사용
- 기본 기능만으로도 충분하다면 Harmony 불필요

### 8.4 권장 구현 방식

**1단계 (Harmony 없음)**: 착용 후 검증 및 제거  
**2단계 (Harmony 선택)**: FloatMenu 경고 추가

이렇게 하면 **최소한의 Harmony 사용**으로도 완전한 기능을 구현할 수 있습니다!


