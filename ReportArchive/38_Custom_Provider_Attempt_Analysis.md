# 커스텀 FloatMenuOptionProvider로 Harmony 없이 구현 시도 분석

## 시도 목표

**Harmony Patch 없이** 커스텀 `FloatMenuOptionProvider`를 만들어서 Apparel 착용을 방지할 수 있는가?

---

## 시도 1: 커스텀 Provider 생성 (검증 로직 포함)

### 구현

```csharp
namespace NewRatkin
{
    /// <summary>
    /// Apparel 착용 시 CompShieldWeaponIncompatible 검증을 수행하는 커스텀 Provider
    /// </summary>
    public class FloatMenuOptionProvider_WearWithWeaponCheck : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;
        
        protected override bool AppliesInt(FloatMenuContext context)
        {
            return context.FirstSelectedPawn.apparel != null;
        }
        
        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            Apparel apparel = clickedThing as Apparel;
            if (apparel == null)
            {
                return null;
            }
            
            Pawn pawn = context.FirstSelectedPawn;
            
            // CompShieldWeaponIncompatible 검증
            CompShieldWeaponIncompatible comp = apparel.TryGetComp<CompShieldWeaponIncompatible>();
            if (comp != null && pawn.equipment?.Primary != null)
            {
                string reason;
                if (!comp.TryIsWeaponAllowed(pawn.equipment.Primary.def, out reason))
                {
                    // 착용 불가 옵션 반환
                    return new FloatMenuOption(
                        "CannotWear".Translate(apparel.Label) + ": " + reason,
                        null,  // action = null → 비활성화
                        MenuOptionPriority.Default,
                        null, null, 0f, null, null, true, 0
                    );
                }
            }
            
            // 착용 가능하면 null 반환 (원본 Provider가 처리하도록)
            return null;
        }
    }
}
```

### 등록 확인

게임 시작 시 `FloatMenuMakerMap.Init()`가 자동으로 호출되어:

```csharp
typeof(FloatMenuOptionProvider).AllSubclassesNonAbstract()
```

이 메서드가 `FloatMenuOptionProvider_WearWithWeaponCheck`를 자동으로 발견하고 providers 리스트에 추가합니다.

```
[FloatMenuMakerMap] Found providers:
  ...
  FloatMenuOptionProvider_Wear                    ← 원본
  FloatMenuOptionProvider_WearWithWeaponCheck     ← 커스텀 ✅
  ...
```

### 실행 결과

**시나리오**: Ratkin이 양손 무기(Bolter) 착용 중, 방패(Shield) 우클릭

```
GetProviderOptions() 실행
    ↓
┌─────────────────────────────────────────────────┐
│ Provider: Equip                                  │
│ clickedThing이 Apparel?                          │
│ → false (CompEquippable 없음)                    │
│ → 옵션 생성 안 함                                 │
└─────────────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────────────┐
│ Provider: FloatMenuOptionProvider_Wear ⭐ (원본) │
│ Applies()? → true                                │
│ GetSingleOptionFor() 실행                        │
│ → 모든 검증 통과 (EquipmentUtility.CanEquip)     │
│ → FloatMenuOption("Force Wear", action)         │
│ → options에 추가 ✅                              │
└─────────────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────────────┐
│ Provider: WearWithWeaponCheck ⭐ (커스텀)        │
│ Applies()? → true                                │
│ GetSingleOptionFor() 실행                        │
│ → CompShieldWeaponIncompatible 검증              │
│ → TryIsWeaponAllowed(Bolter)? → false           │
│ → FloatMenuOption("Cannot Wear: ...", null)     │
│ → options에 추가 ✅                              │
└─────────────────────────────────────────────────┘
    ↓
options = [
    FloatMenuOption("Force Wear", action),      ← 원본
    FloatMenuOption("Cannot Wear: ...", null)   ← 커스텀
]
    ↓
FloatMenu 표시:
    ✅ Force Wear          [클릭 가능, 녹색]
    ❌ Cannot Wear: ...    [클릭 불가, 회색]
```

### 문제점 ❌

**두 옵션이 모두 표시됩니다!**

```
FloatMenu:
┌──────────────────────────────────────┐
│ ✅ Force Wear                         │  ← 사용자가 이걸 클릭 가능!
│ ❌ Cannot Wear: 양손 무기와 함께...   │
└──────────────────────────────────────┘
```

**사용자는 여전히 "Force Wear"를 클릭하여 착용할 수 있습니다!**

---

## 시도 2: 커스텀 Provider에서 착용 가능할 때 옵션 제공

### 구현

```csharp
public class FloatMenuOptionProvider_WearWithWeaponCheck : FloatMenuOptionProvider
{
    // ... (속성 동일)
    
    protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
    {
        Apparel apparel = clickedThing as Apparel;
        if (apparel == null) return null;
        
        Pawn pawn = context.FirstSelectedPawn;
        
        // 기본 검증들 (원본 Provider와 동일하게)
        if (!pawn.CanReach(apparel, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
        {
            return new FloatMenuOption("CannotWear: NoPath", null, ...);
        }
        
        if (apparel.IsBurning())
        {
            return new FloatMenuOption("CannotWear: Burning", null, ...);
        }
        
        // ... (모든 검증 복사)
        
        // CompShieldWeaponIncompatible 검증
        CompShieldWeaponIncompatible comp = apparel.TryGetComp<CompShieldWeaponIncompatible>();
        if (comp != null && pawn.equipment?.Primary != null)
        {
            string reason;
            if (!comp.TryIsWeaponAllowed(pawn.equipment.Primary.def, out reason))
            {
                return new FloatMenuOption("CannotWear: " + reason, null, ...);
            }
        }
        
        // ✅ 착용 가능 - Job 생성
        return new FloatMenuOption("ForceWear", delegate()
        {
            apparel.SetForbidden(false, true);
            Job job = JobMaker.MakeJob(JobDefOf.Wear, apparel);
            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc, false);
        }, MenuOptionPriority.High, ...);
    }
}
```

### 실행 결과

```
FloatMenu:
┌──────────────────────────────────────┐
│ ✅ Force Wear          [원본 Provider]│
│ ✅ Force Wear          [커스텀 Provider]│
└──────────────────────────────────────┘
```

### 문제점 ❌

1. **중복된 옵션 2개 표시** - 사용자 혼란
2. **원본 Provider 로직을 모두 복사**해야 함 - 유지보수 악몽
3. **여전히 원본 Provider는 실행됨** - 근본적 해결 안 됨

---

## 시도 3: 원본 Provider 제거 시도

### 구현 시도

```csharp
// 게임 시작 시 실행되는 초기화 코드
[StaticConstructorOnStartup]
public static class FloatMenuProviderRemover
{
    static FloatMenuProviderRemover()
    {
        // ❌ FloatMenuMakerMap.providers는 private이라 접근 불가능!
        // FloatMenuMakerMap.providers.RemoveAll(p => p is FloatMenuOptionProvider_Wear);
        
        // ❌ Reflection을 써도 읽기 전용 컬렉션으로 보호될 수 있음
        var providersField = typeof(FloatMenuMakerMap).GetField("providers", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var providers = providersField.GetValue(null) as List<FloatMenuOptionProvider>;
        providers.RemoveAll(p => p is FloatMenuOptionProvider_Wear); // ← 위험!
    }
}
```

### 문제점 ❌

1. **private 필드에 Reflection으로 접근** - 매우 위험
2. **다른 모드와 충돌 가능성 높음**
3. **게임 업데이트 시 깨질 가능성 높음**
4. **RimWorld가 의도하지 않은 방식** - 안정성 보장 안 됨

---

## 시도 4: Priority 조작으로 원본 숨기기

### 구현

```csharp
public class FloatMenuOptionProvider_WearWithWeaponCheck : FloatMenuOptionProvider
{
    protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
    {
        // ... 검증 로직 ...
        
        // 착용 가능하면 매우 높은 Priority로 옵션 생성
        return new FloatMenuOption(
            "ForceWear", 
            action, 
            MenuOptionPriority.VeryHigh,  // ← 원본보다 높은 Priority
            ...
        );
    }
}
```

### 실행 결과

```
FloatMenu: (Priority 순으로 정렬)
┌──────────────────────────────────────┐
│ ✅ Force Wear  [커스텀, VeryHigh]     │  ← 위에 표시
│ ✅ Force Wear  [원본, High]           │  ← 아래에 표시
└──────────────────────────────────────┘
```

### 문제점 ❌

1. **여전히 두 옵션 모두 표시됨**
2. Priority는 **정렬 순서만** 결정, 숨기기 불가
3. 사용자가 둘 다 볼 수 있음

---

## 시도 5: 착용 불가 조건에서만 옵션 제공

### 개념

"착용 불가일 때만 커스텀 Provider가 옵션을 생성하고, 착용 가능할 때는 null을 반환하여 원본 Provider가 처리하도록"

### 구현

```csharp
public class FloatMenuOptionProvider_WearWithWeaponCheck : FloatMenuOptionProvider
{
    protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
    {
        Apparel apparel = clickedThing as Apparel;
        if (apparel == null) return null;
        
        Pawn pawn = context.FirstSelectedPawn;
        
        // ⭐ 착용 불가 조건에서만 옵션 생성
        CompShieldWeaponIncompatible comp = apparel.TryGetComp<CompShieldWeaponIncompatible>();
        if (comp != null && pawn.equipment?.Primary != null)
        {
            string reason;
            if (!comp.TryIsWeaponAllowed(pawn.equipment.Primary.def, out reason))
            {
                // 착용 불가 옵션 반환
                return new FloatMenuOption(
                    "CannotWear: " + reason,
                    null,
                    MenuOptionPriority.Default,
                    ...
                );
            }
        }
        
        // 착용 가능하면 null 반환 → 원본 Provider가 처리
        return null;
    }
}
```

### 실행 결과 (착용 불가 시)

```
FloatMenu:
┌──────────────────────────────────────┐
│ ✅ Force Wear          [원본, 착용 가능]│  ← 문제!
│ ❌ Cannot Wear: ...    [커스텀, 불가]   │
└──────────────────────────────────────┘
```

### 문제점 ❌

**원본 Provider는 CompShieldWeaponIncompatible을 알지 못합니다!**

원본 Provider는:
- CanReach ✅
- IsBurning ✅
- WouldReplaceLockedApparel ✅
- HasPartsToWear ✅
- EquipmentUtility.CanEquip ✅ (하지만 CompShieldWeaponIncompatible 체크 없음)

**결과**: 원본 Provider는 "착용 가능"으로 판단하고 "Force Wear" 옵션을 생성합니다.

---

## 시도 6: EquipmentUtility.CanEquip에서 검증 (Harmony 없이)

### 개념

"EquipmentUtility.CanEquip을 확장하여 CompShieldWeaponIncompatible 검증 추가"

### 시도

```csharp
// ❌ 불가능: EquipmentUtility는 static class, CanEquip은 static 메서드
public static class EquipmentUtility
{
    public static bool CanEquip(Thing thing, Pawn pawn, out string cantReason, bool checkBonded = true)
    {
        // RimWorld 원본 로직
        // 오버라이드 불가능!
    }
}

// Extension Method도 소용없음
public static class EquipmentUtilityExtensions
{
    // 이름이 달라서 원본 호출 코드는 여전히 원본 메서드를 호출함
    public static bool CanEquipWithShieldCheck(this Thing thing, Pawn pawn, ...)
    {
        // ...
    }
}
```

### 문제점 ❌

1. **static 메서드는 오버라이드 불가능**
2. Extension Method는 **기존 호출 코드를 바꾸지 못함**
3. FloatMenuOptionProvider_Wear는 여전히 **원본 EquipmentUtility.CanEquip을 호출**

---

## 왜 모든 시도가 실패하는가?

### 근본적인 문제

```
[RimWorld 원본 코드 - 수정 불가능]
    ↓
FloatMenuMakerMap.GetProviderOptions()
    ├─ foreach (provider in providers)  ← 모든 Provider 순회
    │   ↓
    │   FloatMenuOptionProvider_Wear (원본)
    │   ├─ Applies()? → true
    │   ├─ GetSingleOptionFor() 실행
    │   └─ return FloatMenuOption("Force Wear", ...) ✅
    │   
    │   FloatMenuOptionProvider_WearWithWeaponCheck (커스텀)
    │   ├─ Applies()? → true
    │   ├─ GetSingleOptionFor() 실행
    │   └─ return FloatMenuOption("Cannot Wear: ...", ...) ✅
    │   
    └─ options = [원본 옵션, 커스텀 옵션]  ← 두 옵션 모두 추가됨!
```

**핵심**:
1. **providers 리스트는 private** → 접근/수정 불가능
2. **모든 Provider가 순차적으로 실행됨** → 건너뛰기 불가능
3. **옵션은 누적됨** → 제거 불가능
4. **원본 Provider 로직은 밀폐됨** → 수정 불가능

---

## 결론

### ❌ 커스텀 Provider로는 불가능

**이유**:
1. 원본 Provider와 커스텀 Provider가 **모두 실행됨**
2. 결과: **중복된 옵션 2개 표시**
3. 사용자는 여전히 원본 "Force Wear"를 클릭 가능
4. 원본 Provider를 제거/비활성화할 방법 없음

### ✅ Harmony Patch만이 유일한 해결책

**이유**:
1. **원본 Provider의 반환값을 수정** 가능
2. 런타임 IL 코드 수정으로 원본 로직에 개입
3. 중복 없이 하나의 옵션만 표시
4. 안전하고 표준적인 방법

```csharp
[HarmonyPatch(typeof(FloatMenuOptionProvider_Wear), "GetSingleOptionFor")]
[HarmonyPostfix]
public static void Postfix(ref FloatMenuOption __result, Thing clickedThing, FloatMenuContext context)
{
    // 원본 실행 완료 후 __result 수정
    if (__result != null && !__result.Disabled)
    {
        // CompShieldWeaponIncompatible 검증
        // 착용 불가면 __result를 "Cannot Wear" 옵션으로 대체
    }
}
```

**결과**:
```
FloatMenu:
┌──────────────────────────────────────┐
│ ❌ Cannot Wear: 양손 무기와 함께...   │  ← 단일 옵션!
└──────────────────────────────────────┘
```

---

## 비교표

| 방법 | 구현 난이도 | Harmony 필요 | 동작 여부 | 중복 옵션 | 유지보수 |
|------|------------|-------------|-----------|----------|---------|
| **커스텀 Provider (착용 불가만)** | 낮음 | ❌ | ❌ 실패 | ⚠️ 중복 2개 | 낮음 |
| **커스텀 Provider (전체 로직)** | 높음 | ❌ | ❌ 실패 | ⚠️ 중복 2개 | 매우 높음 |
| **원본 Provider 제거 (Reflection)** | 높음 | ❌ | ⚠️ 위험 | ✅ 없음 | 매우 높음 |
| **Harmony Postfix Patch** | 중간 | ✅ | ✅ 성공 | ✅ 없음 | 낮음 |

---

## 최종 답변

**Q: 커스텀 FloatMenuOptionProvider로 Harmony 없이 구현할 수 있는가?**

**A: 불가능합니다.**

**이유**:
- 원본 Provider와 커스텀 Provider가 **모두 실행**되어 중복 옵션 표시
- 원본 Provider를 제거/비활성화할 방법 없음 (private 리스트)
- FloatMenu 시스템이 밀폐되어 있어 외부 개입 불가능

**Harmony Patch가 유일한 해결책입니다.**

