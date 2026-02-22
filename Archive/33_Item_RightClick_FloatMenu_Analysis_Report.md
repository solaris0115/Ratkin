# 아이템 우클릭 FloatMenu UI 로직 분석 보고서

## 개요
이 보고서는 RimWorld에서 아이템을 우클릭했을 때 표시되는 FloatMenu가 어떻게 생성되고, 장비 착용 가능 여부를 어떻게 판단하는지에 대한 분석 결과를 정리합니다.

## 1. FloatMenu 생성 전체 흐름

### 1.1 진입점: FloatMenuMakerMap.GetOptions

**파일**: `RimworldSource/RimWorld/FloatMenuMakerMap.cs`

```csharp
public static List<FloatMenuOption> GetOptions(List<Pawn> selectedPawns, Vector3 clickPos, out FloatMenuContext context)
{
    List<FloatMenuOption> list = new List<FloatMenuOption>();
    context = new FloatMenuContext(selectedPawns, clickPos, Find.CurrentMap);
    
    // 1️⃣ Provider들로부터 옵션 수집
    GetProviderOptions(context, list);
    
    return list;
}
```

### 1.2 Provider 패턴 구조

RimWorld는 **FloatMenuOptionProvider** 패턴을 사용하여 각 상황별로 다른 메뉴 옵션을 제공합니다:

```csharp
private static void GetProviderOptions(FloatMenuContext context, List<FloatMenuOption> options)
{
    // 모든 Provider를 순회
    foreach (FloatMenuOptionProvider provider in providers)
    {
        currentProvider = provider;
        
        if (provider.Applies(context))
        {
            // 1. 일반 옵션 추가
            options.AddRange(provider.GetOptions(context));
            
            // 2. Thing(아이템)에 대한 옵션 추가
            foreach (Thing thing in context.ClickedThings)
            {
                if (provider.TargetThingValid(thing, context))
                {
                    options.AddRange(provider.GetOptionsFor(thing, context));
                }
            }
            
            // 3. Pawn에 대한 옵션 추가
            foreach (Pawn pawn in context.ClickedPawns)
            {
                if (provider.TargetPawnValid(pawn, context))
                {
                    options.AddRange(provider.GetOptionsFor(pawn, context));
                }
            }
        }
    }
}
```

### 1.3 Provider 초기화

게임 시작 시 모든 Provider 하위 클래스가 자동으로 등록됩니다:

```csharp
public static void Init()
{
    providers = new List<FloatMenuOptionProvider>();
    foreach (Type type in typeof(FloatMenuOptionProvider).AllSubclassesNonAbstract())
    {
        providers.Add((FloatMenuOptionProvider)Activator.CreateInstance(type));
    }
}
```

**등록되는 Provider 예시**:
- `FloatMenuOptionProvider_Wear` - Apparel 착용
- `FloatMenuOptionProvider_Equip` - 무기/장비 장착
- `FloatMenuOptionProvider_PickUpItem` - 아이템 줍기
- `FloatMenuOptionProvider_Ingest` - 음식 섭취
- 기타 50여 개의 Provider들...

---

## 2. Apparel 착용 메뉴 생성 (FloatMenuOptionProvider_Wear)

### 2.1 Provider 속성

**파일**: `RimworldSource/RimWorld/FloatMenuOptionProvider_Wear.cs`

```csharp
public class FloatMenuOptionProvider_Wear : FloatMenuOptionProvider
{
    protected override bool Drafted => true;      // 징집 상태에서도 가능
    protected override bool Undrafted => true;    // 비징집 상태에서도 가능
    protected override bool Multiselect => false; // 다중 선택 불가
    
    protected override bool AppliesInt(FloatMenuContext context)
    {
        // Pawn이 apparel 시스템을 가진 경우에만 적용
        return context.FirstSelectedPawn.apparel != null;
    }
}
```

### 2.2 착용 옵션 생성 흐름

```csharp
protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
{
    Apparel apparel = clickedThing as Apparel;
    if (apparel == null) return null;
    
    // ========================================
    // 🔍 착용 불가 조건 체크 (순서대로)
    // ========================================
    
    // ❌ 1. 경로가 없음
    if (!context.FirstSelectedPawn.CanReach(apparel, ...))
        return new FloatMenuOption("CannotWear: NoPath", null, ...);
    
    // ❌ 2. 불타고 있음
    if (apparel.IsBurning())
        return new FloatMenuOption("CannotWear: Burning", null, ...);
    
    // ❌ 3. 잠긴 의복을 대체하려 함
    if (context.FirstSelectedPawn.apparel.WouldReplaceLockedApparel(apparel))
        return new FloatMenuOption("CannotWear: WouldReplaceLockedApparel", null, ...);
    
    // ❌ 4. Mutant가 의복 착용 불가 설정됨
    if (context.FirstSelectedPawn.IsMutant && 
        context.FirstSelectedPawn.mutant.Def.disableApparel)
        return new FloatMenuOption("CannotWear: [MutantType]", null, ...);
    
    // ❌ 5. 착용 가능한 신체 부위가 없음
    if (!ApparelUtility.HasPartsToWear(context.FirstSelectedPawn, apparel.def))
        return new FloatMenuOption("CannotWear: MissingBodyParts", null, ...);
    
    // ❌ 6. EquipmentUtility.CanEquip 체크 (핵심!)
    string failReason;
    if (!EquipmentUtility.CanEquip(apparel, context.FirstSelectedPawn, out failReason, true))
        return new FloatMenuOption("CannotWear: " + failReason, null, ...);
    
    // ========================================
    // ✅ 착용 가능 - Job 생성
    // ========================================
    
    return FloatMenuUtility.DecoratePrioritizedTask(
        new FloatMenuOption("ForceWear", delegate()
        {
            apparel.SetForbidden(false, true);
            Job job = JobMaker.MakeJob(JobDefOf.Wear, apparel);
            context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc, false);
        }, MenuOptionPriority.High, ...), 
        context.FirstSelectedPawn, apparel, "ReservedBy", null
    );
}
```

---

## 3. 착용 가능 여부 판단 (EquipmentUtility.CanEquip)

### 3.1 EquipmentUtility.CanEquip 메서드

**파일**: `RimworldSource/RimWorld/EquipmentUtility.cs`

```csharp
public static bool CanEquip(Thing thing, Pawn pawn, out string cantReason, bool checkBonded = true)
{
    cantReason = null;
    
    // ❌ 1. Bladelink 무기가 다른 사람과 연결됨
    CompBladelinkWeapon compBladelinkWeapon = thing.TryGetComp<CompBladelinkWeapon>();
    if (compBladelinkWeapon != null && 
        compBladelinkWeapon.Biocodable && 
        compBladelinkWeapon.CodedPawn != null && 
        compBladelinkWeapon.CodedPawn != pawn)
    {
        cantReason = "BladelinkBondedToSomeoneElse".Translate();
        return false;
    }
    
    // ❌ 2. 생체코딩된 장비가 다른 사람용
    if (CompBiocodable.IsBiocoded(thing) && 
        !CompBiocodable.IsBiocodedFor(thing, pawn))
    {
        cantReason = "BiocodedCodedForSomeoneElse".Translate();
        return false;
    }
    
    // ❌ 3. 이미 다른 Bladelink 무기와 연결됨
    if (checkBonded && AlreadyBondedToWeapon(thing, pawn))
    {
        cantReason = "BladelinkAlreadyBondedMessage".Translate(...);
        return false;
    }
    
    // ❌ 4. 역할(Role)에 의해 사용 제한됨 (Ideology DLC)
    if (RolePreventsFromUsing(pawn, thing, out cantReason))
    {
        return false;
    }
    
    // ❌ 5. 발달 단계(DevelopmentalStage)가 맞지 않음 (Biotech DLC)
    if (thing.def.IsApparel && 
        !thing.def.apparel.developmentalStageFilter.Has(pawn.DevelopmentalStage))
    {
        cantReason = "WrongDevelopmentalStageForClothing".Translate(...);
        return false;
    }
    
    // ✅ 모든 체크 통과
    return true;
}
```

### 3.2 검증 흐름 다이어그램

```
아이템 우클릭
    ↓
FloatMenuMakerMap.GetOptions()
    ↓
각 Provider 순회 (providers 리스트)
    ↓
FloatMenuOptionProvider_Wear.AppliesInt()
    → Pawn에 apparel 시스템 있는지 확인
    ↓
FloatMenuOptionProvider_Wear.GetSingleOptionFor()
    ↓
    ├─ CanReach? (경로 확인)
    ├─ IsBurning? (불타는지 확인)
    ├─ WouldReplaceLockedApparel? (잠긴 의복 대체)
    ├─ IsMutant && disableApparel? (Mutant 제한)
    ├─ HasPartsToWear? (착용 가능 신체 부위)
    └─ EquipmentUtility.CanEquip? ⭐
        ↓
        ├─ Bladelink 연결 확인
        ├─ 생체코딩 확인
        ├─ 기존 Bladelink 무기 확인
        ├─ Role(역할) 제한 확인
        └─ DevelopmentalStage 확인
            ↓
        FloatMenuOption 생성 (착용 가능/불가)
```

---

## 4. Apparel.PawnCanWear vs EquipmentUtility.CanEquip

### 4.1 Apparel.PawnCanWear

**파일**: `RimworldSource/RimWorld/Apparel.cs`, `RimworldSource/RimWorld/ApparelProperties.cs`

```csharp
// Apparel.cs
public bool PawnCanWear(Pawn pawn, bool ignoreGender = false)
{
    return this.def.IsApparel && this.def.apparel.PawnCanWear(pawn, ignoreGender);
}

// ApparelProperties.cs
public bool PawnCanWear(Pawn pawn, bool ignoreGender = false)
{
    return PawnCanWear(
        ignoreGender ? Gender.None : pawn.gender, 
        pawn.DevelopmentalStage
    );
}

public bool PawnCanWear(Gender gender, DevelopmentalStage developmentalStage)
{
    return CorrectGenderForWearing(gender) && 
           developmentalStageFilter.Has(developmentalStage);
}
```

### 4.2 두 메서드의 차이점

| 메서드 | 목적 | 검증 범위 | 사용 시점 |
|--------|------|-----------|-----------|
| **Apparel.PawnCanWear** | 기본 착용 가능 여부 | - 성별<br>- 발달 단계 | - 장비 필터링<br>- 기본 검증 |
| **EquipmentUtility.CanEquip** | 실제 착용 가능 여부 | - Bladelink 연결<br>- 생체코딩<br>- 역할 제한<br>- 발달 단계 | - FloatMenu 옵션 생성<br>- Job 실행 전 최종 검증 |

**📌 중요**: FloatMenu에서는 `EquipmentUtility.CanEquip`을 사용하며, 이것이 **최종 검증 단계**입니다.

---

## 5. CompShieldWeaponIncompatible 통합 방법

### 5.1 현재 문제점

현재 `CompShieldWeaponIncompatible.AllowEquipmentWith()` 메서드는 다음 위치에서 호출됩니다:
- `ApparelShield.CheckDropEquippedWeapon()` - 방패 착용 시

하지만 **FloatMenu 생성 시점에서는 호출되지 않아** 사용자가 착용 불가 메시지를 미리 볼 수 없습니다.

### 5.2 통합 방법 1: EquipmentUtility.CanEquip 확장 (❌ 불가능)

`EquipmentUtility.CanEquip`은 **static 메서드**이고 RimWorld 원본 코드이므로 직접 수정 불가능합니다.

### 5.3 통합 방법 2: Harmony Patch (✅ 추천)

**FloatMenuOptionProvider_Wear.GetSingleOptionFor** 메서드를 Postfix Patch하여 추가 검증을 삽입합니다:

```csharp
[HarmonyPatch(typeof(FloatMenuOptionProvider_Wear), "GetSingleOptionFor")]
public static class FloatMenuOptionProvider_Wear_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ref FloatMenuOption __result, Thing clickedThing, FloatMenuContext context)
    {
        // 이미 착용 불가로 표시된 경우 무시
        if (__result == null || __result.Disabled) return;
        
        Apparel apparel = clickedThing as Apparel;
        if (apparel == null) return;
        
        Pawn pawn = context.FirstSelectedPawn;
        
        // CompShieldWeaponIncompatible 검증
        CompShieldWeaponIncompatible comp = apparel.TryGetComp<CompShieldWeaponIncompatible>();
        if (comp != null && pawn.equipment?.Primary != null)
        {
            string failReason;
            if (!comp.AllowEquipmentWith(pawn.equipment.Primary, out failReason))
            {
                // 착용 불가 옵션으로 대체
                string key = apparel.def.apparel.LastLayer.IsUtilityLayer 
                    ? "CannotEquipApparel" 
                    : "CannotWear";
                    
                __result = new FloatMenuOption(
                    key.Translate(apparel.Label) + ": " + failReason, 
                    null, 
                    MenuOptionPriority.Default, 
                    null, null, 0f, null, null, true, 0
                );
            }
        }
    }
}
```

### 5.4 통합 방법 3: 커스텀 Provider (⚠️ 복잡함)

완전히 새로운 `FloatMenuOptionProvider`를 생성하여 우선순위를 조정할 수도 있지만, 기존 Provider와의 충돌 가능성이 있어 권장하지 않습니다.

---

## 6. 핵심 확장 포인트 정리

### 6.1 FloatMenu 옵션 추가/수정 지점

| 지점 | 방법 | 난이도 | 추천도 |
|------|------|--------|--------|
| **FloatMenuOptionProvider_Wear.GetSingleOptionFor** | Harmony Postfix | 중간 | ⭐⭐⭐⭐⭐ |
| **EquipmentUtility.CanEquip** | Harmony Prefix/Postfix | 낮음 | ⭐⭐⭐ |
| **FloatMenuMakerMap.GetProviderOptions** | Harmony Postfix | 낮음 | ⭐⭐ |
| **Custom Provider** | Provider 추가 | 높음 | ⭐ |

### 6.2 추천 Patch 위치

```
FloatMenuOptionProvider_Wear.GetSingleOptionFor (Postfix)
    ↓
    ├─ 모든 기본 검증이 완료된 상태
    ├─ FloatMenuOption이 이미 생성됨
    └─ 추가 검증 후 __result를 수정/대체 가능
```

**장점**:
- ✅ 모든 기본 검증이 완료된 후 실행
- ✅ 다른 모드와의 호환성 높음
- ✅ FloatMenuOption을 직접 수정 가능
- ✅ 착용 불가 사유를 명확히 표시 가능

---

## 7. 실전 활용: 착용 제한 로직 흐름

### 7.1 사용자 관점 흐름

```
1. 사용자가 Pawn 선택
2. 방패(Shield) 우클릭
3. FloatMenu 표시:
   
   [현재 구현]
   ✅ "강제 착용 (Force Wear)" → 착용 가능
   
   [Patch 적용 후]
   ❌ "착용 불가 (Cannot Wear): 양손 무기와 함께 착용 불가"
```

### 7.2 내부 로직 흐름

```
FloatMenuMakerMap.GetOptions()
    ↓
FloatMenuOptionProvider_Wear.GetSingleOptionFor()
    ├─ CanReach? ✅
    ├─ IsBurning? ✅
    ├─ WouldReplaceLockedApparel? ✅
    ├─ IsMutant? ✅
    ├─ HasPartsToWear? ✅
    └─ EquipmentUtility.CanEquip? ✅
        ↓
    FloatMenuOption 생성 ("Force Wear")
        ↓
    [Harmony Postfix Patch]
    CompShieldWeaponIncompatible.AllowEquipmentWith()
        ├─ 양손 무기 확인
        └─ twoHandedWeaponTag 매치? ❌
            ↓
        FloatMenuOption 대체 ("Cannot Wear: ...")
            ↓
        사용자에게 착용 불가 메시지 표시
```

---

## 8. 참고 사항

### 8.1 FloatMenuOption 주요 속성

```csharp
public FloatMenuOption(
    string label,           // 표시될 텍스트
    Action action,          // 선택 시 실행할 액션 (null이면 비활성화)
    MenuOptionPriority priority,
    Action mouseoverGuiAction = null,
    Thing revalidateClickTarget = null,
    float extraPartWidth = 0f,
    Func<Rect, bool> extraPartOnGUI = null,
    Thing iconThing = null,
    bool forceBasicStyle = true,
    int orderInPriority = 0
)
```

**착용 불가 표시**: `action`을 `null`로 설정하면 회색으로 표시되고 클릭 불가

### 8.2 관련 파일 목록

**RimWorld 원본 소스**:
- `RimworldSource/RimWorld/FloatMenuMakerMap.cs` - FloatMenu 생성 관리자
- `RimworldSource/RimWorld/FloatMenuOptionProvider.cs` - Provider 베이스 클래스
- `RimworldSource/RimWorld/FloatMenuOptionProvider_Wear.cs` - Apparel 착용 Provider
- `RimworldSource/RimWorld/FloatMenuOptionProvider_Equip.cs` - 무기 장착 Provider
- `RimworldSource/RimWorld/EquipmentUtility.cs` - 장비 착용 가능 여부 판단
- `RimworldSource/RimWorld/Apparel.cs` - Apparel 클래스
- `RimworldSource/RimWorld/ApparelProperties.cs` - Apparel 속성 정의

**Ratkin 프로젝트**:
- `Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs` - 방패-무기 호환성 검증

---

## 9. 결론

### 9.1 FloatMenu 생성 메커니즘

RimWorld의 FloatMenu는 **Provider 패턴**을 사용하여 확장 가능하고 모듈화된 구조로 설계되었습니다. 각 Provider는 독립적으로 동작하며, `FloatMenuMakerMap`이 이들을 순회하면서 메뉴 옵션을 수집합니다.

### 9.2 착용 제한 검증 흐름

1. **FloatMenuOptionProvider_Wear**: UI 생성 단계에서 기본 검증
2. **EquipmentUtility.CanEquip**: 최종 착용 가능 여부 판단
3. **Apparel.PawnCanWear**: 성별/발달 단계 같은 기본 속성 검증

### 9.3 커스텀 로직 통합 방법

`CompShieldWeaponIncompatible` 같은 커스텀 제한 로직을 FloatMenu에 통합하려면:
- ✅ **추천**: `FloatMenuOptionProvider_Wear.GetSingleOptionFor` Postfix Patch
- ⚠️ **대안**: `EquipmentUtility.CanEquip` Prefix Patch
- ❌ **비추천**: Custom Provider 추가 (복잡도 높음)

### 9.4 확장 가능성

이 구조는 다른 착용 제한 로직(예: 특정 종족 전용 장비, 스킬 요구사항 등)을 추가할 때도 동일한 방식으로 적용 가능합니다.

