# FloatMenu 착용 불가 표시 - Harmony 불가피성 분석 보고서

## 개요
이 보고서는 FloatMenu에서 Apparel 착용 불가를 표시하기 위해 **왜 Harmony Patch가 불가피한지**를 분석합니다.

---

## 1. FloatMenu 생성 구조 (복습)

### 1.1 전체 흐름

```
사용자 우클릭
    ↓
FloatMenuMakerMap.GetOptions() [static]
    ↓
GetProviderOptions() [private static]
    ↓
foreach (FloatMenuOptionProvider provider in providers) [자동 등록된 리스트]
    ↓
    provider.Applies(context) [virtual]
    ↓
    provider.GetSingleOptionFor(clickedThing, context) [protected virtual]
        ↓
        [FloatMenuOptionProvider_Wear]
        GetSingleOptionFor() [protected override]
            ↓
            기본 검증들 (CanReach, IsBurning, ...)
            ↓
            EquipmentUtility.CanEquip() [static]
            ↓
            return new FloatMenuOption("Force Wear", action, ...)
```

### 1.2 핵심 문제점

**모든 로직이 RimWorld 원본 코드의 private/static/sealed 메서드로 구성되어 있습니다.**

---

## 2. Harmony 없이 시도 가능한 방법들 (모두 실패)

### ❌ 방법 1: Apparel.PawnCanWear 오버라이드

#### 시도

```csharp
public class Shield : Apparel
{
    public override bool PawnCanWear(Pawn pawn, bool ignoreGender = false)
    {
        // 커스텀 로직 추가?
        if (pawn.equipment?.Primary != null)
        {
            CompShieldWeaponIncompatible comp = this.TryGetComp<CompShieldWeaponIncompatible>();
            if (comp != null)
            {
                string reason;
                if (!comp.TryIsWeaponAllowed(pawn.equipment.Primary.def, out reason))
                {
                    return false;
                }
            }
        }
        return base.PawnCanWear(pawn, ignoreGender);
    }
}
```

#### 실패 이유

**`FloatMenuOptionProvider_Wear`가 `PawnCanWear`를 호출하지 않습니다!**

**FloatMenuOptionProvider_Wear.GetSingleOptionFor 실제 코드**:

```csharp
protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
{
    Apparel apparel = clickedThing as Apparel;
    if (apparel == null) return null;
    
    // ❌ PawnCanWear 호출 안 함!
    
    // 1. CanReach 체크
    if (!context.FirstSelectedPawn.CanReach(apparel, ...))
        return new FloatMenuOption("CannotWear: NoPath", null, ...);
    
    // 2. IsBurning 체크
    if (apparel.IsBurning())
        return new FloatMenuOption("CannotWear: Burning", null, ...);
    
    // 3. WouldReplaceLockedApparel 체크
    if (context.FirstSelectedPawn.apparel.WouldReplaceLockedApparel(apparel))
        return new FloatMenuOption("CannotWear: WouldReplaceLockedApparel", null, ...);
    
    // 4. HasPartsToWear 체크
    if (!ApparelUtility.HasPartsToWear(context.FirstSelectedPawn, apparel.def))
        return new FloatMenuOption("CannotWear: MissingBodyParts", null, ...);
    
    // 5. EquipmentUtility.CanEquip 체크 ⭐
    string t;
    if (!EquipmentUtility.CanEquip(apparel, context.FirstSelectedPawn, out t, true))
        return new FloatMenuOption("CannotWear: " + t, null, ...);
    
    // ✅ 모든 체크 통과 → 착용 가능
    return new FloatMenuOption("ForceWear", action, ...);
}
```

**PawnCanWear는 나중에 `Pawn_ApparelTracker.Wear()`에서만 호출됩니다.**

```csharp
// Pawn_ApparelTracker.cs:499
if (!newApparel.PawnCanWear(this.pawn, true))
{
    Log.Warning(...);
    return;
}
```

**결론**: FloatMenu 생성 시점에는 영향 없음.

---

### ❌ 방법 2: EquipmentUtility.CanEquip 확장

#### 시도

`EquipmentUtility.CanEquip`에 커스텀 로직을 추가?

#### 실패 이유

**`EquipmentUtility`는 static class이고 `CanEquip`은 static 메서드입니다.**

```csharp
// RimworldSource/RimWorld/EquipmentUtility.cs
public static class EquipmentUtility
{
    public static bool CanEquip(Thing thing, Pawn pawn, out string cantReason, bool checkBonded = true)
    {
        // RimWorld 원본 로직
        // 오버라이드 불가능!
    }
}
```

**확장 방법**:
- Extension Method? → 기존 호출 코드는 원본 메서드를 호출함
- 상속? → static class는 상속 불가능
- 오버로드? → 시그니처가 같으면 컴파일 에러

**결론**: RimWorld 원본 코드를 수정하지 않고는 불가능.

---

### ❌ 방법 3: 커스텀 FloatMenuOptionProvider 생성

#### 시도

```csharp
public class CustomFloatMenuOptionProvider_WearWithWeaponCheck : FloatMenuOptionProvider
{
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;
    
    protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
    {
        // 커스텀 로직
    }
}
```

#### 실패 이유 1: Provider 우선순위 문제

**FloatMenuMakerMap는 모든 Provider를 순회하며 옵션을 수집합니다.**

```csharp
// FloatMenuMakerMap.cs:73-106
private static void GetProviderOptions(FloatMenuContext context, List<FloatMenuOption> options)
{
    foreach (FloatMenuOptionProvider provider in providers)
    {
        if (provider.Applies(context))
        {
            // 모든 Provider의 옵션을 options에 추가
            foreach (Thing thing in context.ClickedThings)
            {
                options.AddRange(provider.GetOptionsFor(thing, context));
            }
        }
    }
}
```

**문제점**:
- `FloatMenuOptionProvider_Wear`도 실행됨
- 커스텀 Provider도 실행됨
- **결과**: FloatMenu에 중복된 옵션 2개 표시!

```
FloatMenu:
✅ Force Wear [원본 Provider]
❌ Cannot Wear: ... [커스텀 Provider]
```

#### 실패 이유 2: 원본 Provider 제거 불가능

**Provider 리스트는 `FloatMenuMakerMap.Init()`에서 자동 생성됩니다.**

```csharp
// FloatMenuMakerMap.cs:18-25
public static void Init()
{
    providers = new List<FloatMenuOptionProvider>();
    foreach (Type type in typeof(FloatMenuOptionProvider).AllSubclassesNonAbstract())
    {
        providers.Add((FloatMenuOptionProvider)Activator.CreateInstance(type));
    }
}
```

- `FloatMenuOptionProvider_Wear`는 RimWorld 원본이므로 자동으로 포함됨
- 리스트가 private이므로 접근 불가능
- Init()은 게임 시작 시 한 번만 실행됨

**결론**: 원본 Provider를 제거하거나 비활성화할 방법이 없음.

---

### ❌ 방법 4: Apparel 클래스 확장으로 검증 Hook 추가

#### 시도

```csharp
public class Shield : Apparel
{
    // 커스텀 메서드 추가?
    public virtual bool CanWearWith(Pawn pawn)
    {
        // 무기 검증
    }
}
```

#### 실패 이유

**FloatMenuOptionProvider_Wear가 이 메서드를 호출하지 않습니다.**

RimWorld 원본 코드는 커스텀 메서드의 존재를 알 수 없으므로 호출하지 않습니다.

**결론**: Hook 포인트가 없음.

---

## 3. 왜 Harmony가 필요한가?

### 3.1 FloatMenu 생성은 밀폐된 시스템

```
[RimWorld 원본 코드 - 수정 불가능]
    ↓
FloatMenuMakerMap (sealed 동작)
    ├─ private static methods
    ├─ private providers list
    └─ 자동 Provider 등록
        ↓
FloatMenuOptionProvider_Wear (sealed 클래스는 아니지만 인스턴스는 자동 생성)
    ├─ protected virtual GetSingleOptionFor
    │   (오버라이드는 가능하지만 원본 인스턴스를 대체할 방법 없음)
    └─ 고정된 검증 로직
```

**문제점**:
1. **Provider 리스트가 private** → 접근 불가능
2. **GetProviderOptions가 private static** → 오버라이드 불가능
3. **FloatMenuOptionProvider_Wear 인스턴스가 자동 생성** → 교체 불가능
4. **GetSingleOptionFor 로직이 고정** → 수정 불가능

### 3.2 Harmony Patch만이 해결책인 이유

**Harmony는 런타임에 IL 코드를 수정합니다.**

```csharp
[HarmonyPatch(typeof(FloatMenuOptionProvider_Wear), "GetSingleOptionFor")]
[HarmonyPostfix]
public static void Postfix(ref FloatMenuOption __result, ...)
{
    // 원본 메서드 실행 후 결과(__result)를 수정
    if (호환되지 않는 무기 착용 중)
    {
        __result = new FloatMenuOption("Cannot Wear: ...", null, ...);
    }
}
```

**Harmony가 가능한 이유**:
1. ✅ **런타임 IL 수정** - 원본 코드를 직접 수정하지 않고 바이트코드 레벨에서 개입
2. ✅ **Postfix Patch** - 원본 로직 실행 후 결과만 수정 (안전함)
3. ✅ **ref __result** - 반환값을 수정할 수 있음
4. ✅ **기존 시스템과 호환** - 다른 Provider나 로직에 영향 없음

---

## 4. Harmony 없이 가능한 것들

### ✅ Notify_Equipped(pawn) 오버라이드

**이것은 Harmony 없이 가능합니다!**

```csharp
public class CompShieldWeaponIncompatible : ThingComp
{
    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        
        // 착용 완료 후 검증
        if (호환되지 않는 무기 착용 중)
        {
            무기 제거();
        }
    }
}
```

**가능한 이유**:
- `ThingComp.Notify_Equipped`는 **virtual 메서드**
- 모든 Comp의 `Notify_Equipped`가 자동으로 호출됨
- 오버라이드만 하면 자동으로 동작

**한계**:
- ❌ FloatMenu 생성 시점에는 개입 불가능
- ❌ 사용자가 착용 시도 후에만 알 수 있음
- ✅ 안전망 역할은 가능 (FloatMenu를 거치지 않는 경로)

---

## 5. 구현 방법 비교표

| 구현 목표 | Harmony 필요 여부 | 가능 여부 | 방법 |
|-----------|------------------|-----------|------|
| **FloatMenu에서 착용 불가 표시** | ✅ 필요 | ✅ 가능 | Harmony Postfix Patch |
| **착용 시도 후 무기 제거** | ❌ 불필요 | ✅ 가능 | Notify_Equipped 오버라이드 |
| **Apparel.PawnCanWear 오버라이드** | ❌ 불필요 | ✅ 가능 (하지만 FloatMenu에 영향 없음) | Apparel 클래스 상속 |
| **EquipmentUtility.CanEquip 확장** | ✅ 필요 | ⚠️ 가능 (Prefix Patch) | Harmony Prefix Patch |
| **커스텀 Provider 추가** | ❌ 불필요 | ❌ 실패 (중복 옵션) | FloatMenuOptionProvider 상속 |

---

## 6. 결론

### 6.1 FloatMenu 착용 불가 표시는 Harmony 불가피

**이유**:
1. FloatMenu 생성 로직이 RimWorld 원본의 private/static 메서드로 밀폐되어 있음
2. Provider 시스템이 자동으로 동작하며 외부에서 제어 불가능
3. `GetSingleOptionFor` 메서드의 반환값을 수정하는 것이 유일한 방법
4. 런타임 IL 수정 (Harmony)만이 이를 가능하게 함

### 6.2 추천 구현 방식

**2단계 구현** (이미 완료됨):

**1단계 (안전망, Harmony 불필요)**:
```csharp
public override void Notify_Equipped(Pawn pawn)
{
    // 착용 완료 후 호환되지 않는 무기 제거
}
```

**2단계 (UX 개선, Harmony 필요)**:
```csharp
[HarmonyPostfix]
public static void Postfix(ref FloatMenuOption __result, ...)
{
    // FloatMenu에서 착용 불가 표시
}
```

### 6.3 Harmony 사용의 정당성

**Harmony는 모드 제작에서 표준**입니다:
- ✅ RimWorld 모드의 90% 이상이 Harmony 사용
- ✅ Ludeon Studios(RimWorld 개발사)가 공식 지원
- ✅ 안정적이고 잘 검증된 라이브러리
- ✅ Postfix Patch는 안전함 (원본 로직에 영향 없음)

**Harmony를 피해야 하는 경우**:
- ⚠️ Prefix Patch로 원본 로직 건너뛰기
- ⚠️ 여러 모드가 같은 메서드를 Patch할 때 충돌 가능성
- ⚠️ 복잡한 IL Transpiler 사용

**현재 구현 (Postfix Patch)**:
- ✅ 안전함 - 원본 로직 실행 후 결과만 수정
- ✅ 충돌 가능성 낮음 - ref __result 수정만 함
- ✅ 다른 모드와 호환 가능

---

## 7. 대안 (Harmony 없이 타협안)

### 대안 1: 착용 후 즉시 제거 + 명확한 메시지

```csharp
public override void Notify_Equipped(Pawn pawn)
{
    base.Notify_Equipped(pawn);
    
    if (호환되지 않는 무기 착용 중)
    {
        무기 제거();
        
        // ⭐ 명확한 메시지로 UX 보완
        Messages.Message(
            "경고: {방패}는 {무기}와 함께 착용할 수 없습니다. {무기}를 내려놓았습니다.",
            pawn,
            MessageTypeDefOf.CautionInput // 주황색 경고
        );
    }
}
```

**장점**:
- ✅ Harmony 불필요
- ✅ 모든 착용 경로에서 동작
- ✅ 명확한 메시지로 사용자에게 알림

**단점**:
- ❌ 착용 시도 후에만 알 수 있음
- ❌ FloatMenu에서는 미리 알 수 없음

### 대안 2: Inspect 패널에 경고 표시

```csharp
public override string CompInspectStringExtra()
{
    Pawn wearer = Wearer;
    if (wearer?.equipment?.Primary != null)
    {
        string reason;
        if (!TryIsWeaponAllowed(wearer.equipment.Primary.def, out reason))
        {
            return "경고: " + reason;
        }
    }
    return null;
}
```

**장점**:
- ✅ Harmony 불필요
- ✅ 착용 중 지속적으로 경고 표시

**단점**:
- ❌ FloatMenu에는 표시 안 됨
- ❌ Inspect 패널을 봐야만 알 수 있음

---

## 8. 최종 권장사항

### ✅ 현재 구현 (Harmony 사용) 유지

**이유**:
1. **FloatMenu 착용 불가 표시는 Harmony 없이 불가능**
2. **Postfix Patch는 안전하고 표준적인 방법**
3. **이중 안전 메커니즘 (FloatMenu + Notify_Equipped)**으로 완벽함
4. **사용자 경험이 가장 좋음**

### 구현 구조

```
[사용자 시나리오]

일반적인 경우 (FloatMenu 사용):
------------------------------
1. 사용자가 방패 우클릭
2. FloatMenu Harmony Patch ⭐ (1차 방어선)
3. ❌ "착용 불가: 양손 무기와 함께 착용 불가" (회색)
4. 착용 시도 자체 차단!

FloatMenu를 거치지 않는 경우:
------------------------------
1. Job/다른 모드가 직접 착용 시도
2. Notify_Equipped ⭐ (2차 방어선)
3. 방패 착용 → 무기 자동 제거
4. 메시지 표시
```

**결론**: **Harmony Patch가 최선의 방법이며, 불가피합니다.**

