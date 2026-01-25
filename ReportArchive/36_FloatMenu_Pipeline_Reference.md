# FloatMenu 파이프라인 완전 분석 자료

## 전체 호출 흐름

```
사용자 우클릭
    ↓
FloatMenuMakerMap.GetOptions(selectedPawns, clickPos, out context)
    ↓
GetProviderOptions(context, options)
    ↓
foreach (FloatMenuOptionProvider provider in providers)
    ↓
    provider.Applies(context)
    ↓
    provider.GetOptions(context)
    provider.GetOptionsFor(clickedThing, context)
        ↓
        [FloatMenuOptionProvider_Wear]
        GetSingleOptionFor(clickedThing, context)
            ↓
            검증 단계들
            ↓
            return new FloatMenuOption(...)
```

---

## 1. 진입점: FloatMenuMakerMap.GetOptions()

**파일**: `RimworldSource/RimWorld/FloatMenuMakerMap.cs`

### 메서드 시그니처

```csharp
public static List<FloatMenuOption> GetOptions(
    List<Pawn> selectedPawns, 
    Vector3 clickPos, 
    out FloatMenuContext context
)
```

### 전체 코드

```csharp
// FloatMenuMakerMap.cs:27-71
public static List<FloatMenuOption> GetOptions(List<Pawn> selectedPawns, Vector3 clickPos, out FloatMenuContext context)
{
    List<FloatMenuOption> list = new List<FloatMenuOption>();
    context = null;
    
    // 1. 맵 범위 체크
    if (!clickPos.InBounds(Find.CurrentMap))
    {
        return list;
    }
    
    // 2. FloatMenuContext 생성
    context = new FloatMenuContext(selectedPawns, clickPos, Find.CurrentMap);
    
    // 3. Pawn 선택 여부 체크
    if (!context.allSelectedPawns.Any<Pawn>())
    {
        return list;
    }
    
    // 4. 클릭 위치 유효성 체크
    if (!context.ClickedCell.IsValid || !context.ClickedCell.InBounds(Find.CurrentMap))
    {
        return list;
    }
    
    // 5. 단일 선택일 때 Pawn 상태 체크
    if (!context.IsMultiselect)
    {
        AcceptanceReport acceptanceReport = FloatMenuMakerMap.ShouldGenerateFloatMenuForPawn(context.FirstSelectedPawn);
        if (!acceptanceReport.Accepted)
        {
            if (!acceptanceReport.Reason.NullOrEmpty())
            {
                Messages.Message(acceptanceReport.Reason, context.FirstSelectedPawn, MessageTypeDefOf.RejectInput, false);
            }
            return list;
        }
    }
    else
    {
        // 6. 다중 선택일 때 유효한 Pawn만 필터링
        context.allSelectedPawns.RemoveAll((Pawn selectedPawn) => !FloatMenuMakerMap.ShouldGenerateFloatMenuForPawn(selectedPawn));
        if (!context.allSelectedPawns.Any<Pawn>())
        {
            return list;
        }
    }
    
    // 7. makingFor 설정 (디버깅/추적용)
    if (!context.IsMultiselect)
    {
        FloatMenuMakerMap.makingFor = context.FirstSelectedPawn;
    }
    
    // 8. ⭐ Provider들로부터 옵션 수집
    FloatMenuMakerMap.GetProviderOptions(context, list);
    
    // 9. makingFor 초기화
    FloatMenuMakerMap.makingFor = null;
    
    return list;
}
```

---

## 2. Provider 순회: GetProviderOptions()

**파일**: `RimworldSource/RimWorld/FloatMenuMakerMap.cs`

### 메서드 시그니처

```csharp
private static void GetProviderOptions(
    FloatMenuContext context, 
    List<FloatMenuOption> options
)
```

### 전체 코드

```csharp
// FloatMenuMakerMap.cs:73-132
private static void GetProviderOptions(FloatMenuContext context, List<FloatMenuOption> options)
{
    // ⭐ 모든 Provider 순회
    foreach (FloatMenuOptionProvider floatMenuOptionProvider in FloatMenuMakerMap.providers)
    {
        try
        {
            // 현재 처리 중인 Provider 설정 (디버깅/추적용)
            FloatMenuMakerMap.currentProvider = floatMenuOptionProvider;
            
            // 유효한 Pawn이 있는지 체크
            if (context.ValidSelectedPawns.Any<Pawn>())
            {
                // Provider가 이 context에 적용되는지 확인
                if (floatMenuOptionProvider.Applies(context))
                {
                    // 1️⃣ 일반 옵션 추가
                    options.AddRange(floatMenuOptionProvider.GetOptions(context));
                    
                    // 2️⃣ Thing(아이템)에 대한 옵션 추가
                    foreach (Thing thing in context.ClickedThings)
                    {
                        if (floatMenuOptionProvider.TargetThingValid(thing, context))
                        {
                            Thing thing2 = thing;
                            
                            // CompSelectProxy 처리 (특수 선택 프록시)
                            CompSelectProxy compSelectProxy;
                            if (thing2.TryGetComp(out compSelectProxy) && compSelectProxy.thingToSelect != null)
                            {
                                thing2 = compSelectProxy.thingToSelect;
                            }
                            
                            // ⭐ Thing에 대한 옵션들 가져오기
                            foreach (FloatMenuOption floatMenuOption in floatMenuOptionProvider.GetOptionsFor(thing2, context))
                            {
                                FloatMenuOption floatMenuOption2 = floatMenuOption;
                                
                                // iconThing 설정
                                if (floatMenuOption2.iconThing == null)
                                {
                                    floatMenuOption2.iconThing = thing2;
                                }
                                
                                // Despawned 상태 표시
                                floatMenuOption.targetsDespawned = !thing2.Spawned;
                                
                                options.Add(floatMenuOption);
                            }
                        }
                    }
                    
                    // 3️⃣ Pawn에 대한 옵션 추가
                    foreach (Pawn pawn in context.ClickedPawns)
                    {
                        if (floatMenuOptionProvider.TargetPawnValid(pawn, context))
                        {
                            foreach (FloatMenuOption floatMenuOption3 in floatMenuOptionProvider.GetOptionsFor(pawn, context))
                            {
                                FloatMenuOption floatMenuOption2 = floatMenuOption3;
                                
                                // iconThing 설정
                                if (floatMenuOption2.iconThing == null)
                                {
                                    floatMenuOption2.iconThing = pawn;
                                }
                                
                                // Despawned 상태 표시
                                floatMenuOption3.targetsDespawned = !pawn.Spawned;
                                
                                options.Add(floatMenuOption3);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception arg)
        {
            Log.Error(string.Format("Error in FloatMenuWorker {0}: {1}", floatMenuOptionProvider.GetType().Name, arg));
        }
    }
    
    // 현재 Provider 초기화
    FloatMenuMakerMap.currentProvider = null;
}
```

---

## 3. Provider 초기화: Init()

**파일**: `RimworldSource/RimWorld/FloatMenuMakerMap.cs`

### 코드

```csharp
// FloatMenuMakerMap.cs:18-25
public static void Init()
{
    FloatMenuMakerMap.providers = new List<FloatMenuOptionProvider>();
    
    // ⭐ 모든 FloatMenuOptionProvider 하위 클래스를 자동으로 찾아서 등록
    foreach (Type type in typeof(FloatMenuOptionProvider).AllSubclassesNonAbstract())
    {
        FloatMenuMakerMap.providers.Add((FloatMenuOptionProvider)Activator.CreateInstance(type));
    }
}
```

### 자동 등록되는 Provider들

- `FloatMenuOptionProvider_Wear` - Apparel 착용
- `FloatMenuOptionProvider_Equip` - 무기/장비 장착
- `FloatMenuOptionProvider_PickUpItem` - 아이템 줍기
- `FloatMenuOptionProvider_Ingest` - 음식 섭취
- `FloatMenuOptionProvider_Trade` - 거래
- `FloatMenuOptionProvider_Arrest` - 체포
- `FloatMenuOptionProvider_Rescue` - 구조
- ... 총 50여 개

---

## 4. FloatMenuOptionProvider 베이스 클래스

**파일**: `RimworldSource/RimWorld/FloatMenuOptionProvider.cs`

### 주요 메서드

```csharp
// FloatMenuOptionProvider.cs:7-130
public abstract class FloatMenuOptionProvider
{
    // ========================================
    // 속성 (하위 클래스에서 오버라이드)
    // ========================================
    
    protected abstract bool Drafted { get; }      // 징집 상태에서 적용?
    protected abstract bool Undrafted { get; }    // 비징집 상태에서 적용?
    protected abstract bool Multiselect { get; }  // 다중 선택 가능?
    
    protected virtual bool RequiresManipulation => false;
    protected virtual bool MechanoidCanDo => false;
    protected virtual bool CanSelfTarget => false;
    public virtual bool CanTargetDespawned => false;
    protected virtual bool IgnoreFogged => true;
    
    // ========================================
    // 검증 메서드
    // ========================================
    
    // 선택한 Pawn이 이 Provider를 사용할 수 있는지 확인
    public virtual bool SelectedPawnValid(Pawn pawn, FloatMenuContext context)
    {
        return (!pawn.IsMutant || pawn.mutant.Def.whitelistedFloatMenuProviders == null || 
                pawn.mutant.Def.whitelistedFloatMenuProviders.Contains(FloatMenuMakerMap.currentProvider.GetType())) && 
               (this.Drafted || !pawn.Drafted) && 
               (this.Undrafted || pawn.Drafted) && 
               (this.MechanoidCanDo || !pawn.RaceProps.IsMechanoid) && 
               (!this.RequiresManipulation || pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
    }
    
    // 대상 Thing이 유효한지 확인
    public virtual bool TargetThingValid(Thing thing, FloatMenuContext context)
    {
        if (!this.CanTargetDespawned && !thing.Spawned)
        {
            return false;
        }
        Pawn pawn = thing as Pawn;
        return pawn == null || this.TargetPawnValid(pawn, context);
    }
    
    // 대상 Pawn이 유효한지 확인
    public virtual bool TargetPawnValid(Pawn pawn, FloatMenuContext context)
    {
        return this.CanSelfTarget || pawn != context.FirstSelectedPawn;
    }
    
    // 이 Provider가 현재 context에 적용되는지 확인
    public virtual bool Applies(FloatMenuContext context)
    {
        return (this.Multiselect || !context.IsMultiselect) && 
               (!this.IgnoreFogged || !context.ClickedCell.Fogged(context.map)) && 
               this.AppliesInt(context);
    }
    
    protected virtual bool AppliesInt(FloatMenuContext context)
    {
        return true;
    }
    
    // ========================================
    // 옵션 생성 메서드
    // ========================================
    
    // 일반 옵션 생성
    public virtual IEnumerable<FloatMenuOption> GetOptions(FloatMenuContext context)
    {
        FloatMenuOption singleOption = this.GetSingleOption(context);
        if (singleOption != null)
        {
            yield return singleOption;
        }
        yield break;
    }
    
    // Thing에 대한 옵션 생성
    public virtual IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
    {
        FloatMenuOption singleOptionFor = this.GetSingleOptionFor(clickedThing, context);
        if (singleOptionFor != null)
        {
            yield return singleOptionFor;
        }
        yield break;
    }
    
    // Pawn에 대한 옵션 생성
    public virtual IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
    {
        FloatMenuOption singleOptionFor = this.GetSingleOptionFor(clickedPawn, context);
        if (singleOptionFor != null)
        {
            yield return singleOptionFor;
        }
        yield break;
    }
    
    // ========================================
    // 하위 클래스에서 구현할 메서드들
    // ========================================
    
    protected virtual FloatMenuOption GetSingleOption(FloatMenuContext context)
    {
        return null;
    }
    
    protected virtual FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
    {
        return null;
    }
    
    protected virtual FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
    {
        return null;
    }
}
```

---

## 5. FloatMenuOptionProvider_Wear (Apparel 착용)

**파일**: `RimworldSource/RimWorld/FloatMenuOptionProvider_Wear.cs`

### 속성 설정

```csharp
// FloatMenuOptionProvider_Wear.cs:7-36
public class FloatMenuOptionProvider_Wear : FloatMenuOptionProvider
{
    protected override bool Drafted
    {
        get { return true; }  // 징집 상태에서도 가능
    }

    protected override bool Undrafted
    {
        get { return true; }  // 비징집 상태에서도 가능
    }

    protected override bool Multiselect
    {
        get { return false; }  // 다중 선택 불가
    }

    protected override bool AppliesInt(FloatMenuContext context)
    {
        // Pawn이 apparel 시스템을 가진 경우에만 적용
        return context.FirstSelectedPawn.apparel != null;
    }
}
```

### GetSingleOptionFor 전체 코드

```csharp
// FloatMenuOptionProvider_Wear.cs:38-97
protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
{
    // 1️⃣ Apparel 타입 체크
    Apparel apparel = clickedThing as Apparel;
    if (apparel == null)
    {
        return null;
    }
    
    // 번역 키 설정
    string key = "CannotWear";
    string key2 = "ForceWear";
    if (apparel.def.apparel.LastLayer.IsUtilityLayer)
    {
        key = "CannotEquipApparel";
        key2 = "ForceEquipApparel";
    }
    
    // ========================================
    // 🔍 착용 불가 조건 체크 (순서대로)
    // ========================================
    
    // 2️⃣ 경로 체크
    if (!context.FirstSelectedPawn.CanReach(apparel, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
    {
        return new FloatMenuOption(
            key.Translate(apparel.Label, apparel) + ": " + "NoPath".Translate().CapitalizeFirst(), 
            null, 
            MenuOptionPriority.Default, 
            null, null, 0f, null, null, true, 0
        );
    }
    
    // 3️⃣ 불타고 있는지 체크
    if (apparel.IsBurning())
    {
        return new FloatMenuOption(
            key.Translate(apparel.Label, apparel) + ": " + "Burning".Translate(), 
            null, 
            MenuOptionPriority.Default, 
            null, null, 0f, null, null, true, 0
        );
    }
    
    // 4️⃣ 잠긴 의복을 대체하는지 체크
    if (context.FirstSelectedPawn.apparel.WouldReplaceLockedApparel(apparel))
    {
        return new FloatMenuOption(
            key.Translate(apparel.Label, apparel) + ": " + "WouldReplaceLockedApparel".Translate().CapitalizeFirst(), 
            null, 
            MenuOptionPriority.Default, 
            null, null, 0f, null, null, true, 0
        );
    }
    
    // 5️⃣ Mutant의 의복 착용 불가 체크
    if (context.FirstSelectedPawn.IsMutant && context.FirstSelectedPawn.mutant.Def.disableApparel)
    {
        return new FloatMenuOption(
            key.Translate(apparel.Label, apparel) + ": " + context.FirstSelectedPawn.mutant.Def.LabelCap, 
            null, 
            MenuOptionPriority.Default, 
            null, null, 0f, null, null, true, 0
        );
    }
    
    // 6️⃣ 착용 가능한 신체 부위가 있는지 체크
    if (!ApparelUtility.HasPartsToWear(context.FirstSelectedPawn, apparel.def))
    {
        return new FloatMenuOption(
            key.Translate(apparel.Label, apparel) + ": " + "CannotWearBecauseOfMissingBodyParts".Translate().CapitalizeFirst(), 
            null, 
            MenuOptionPriority.Default, 
            null, null, 0f, null, null, true, 0
        );
    }
    
    // 7️⃣ ⭐ EquipmentUtility.CanEquip 체크 (최종 검증)
    string t;
    if (!EquipmentUtility.CanEquip(apparel, context.FirstSelectedPawn, out t, true))
    {
        return new FloatMenuOption(
            key.Translate(apparel.Label, apparel) + ": " + t, 
            null, 
            MenuOptionPriority.Default, 
            null, null, 0f, null, null, true, 0
        );
    }
    
    // ========================================
    // ✅ 착용 가능 - Job 생성
    // ========================================
    
    Action <>9__1;
    return FloatMenuUtility.DecoratePrioritizedTask(
        new FloatMenuOption(
            key2.Translate(apparel.LabelShort, apparel), 
            delegate()
            {
                Action action;
                if ((action = <>9__1) == null)
                {
                    action = (<>9__1 = delegate()
                    {
                        // 금지 해제
                        apparel.SetForbidden(false, true);
                        
                        // Wear Job 생성
                        Job job = JobMaker.MakeJob(JobDefOf.Wear, apparel);
                        
                        // Job 실행
                        context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
                    });
                }
                Action action2 = action;
                
                // 대체될 의복 확인 (Biotech - Mechanitor 대역폭 경고)
                Apparel apparelReplacedByNewApparel = ApparelUtility.GetApparelReplacedByNewApparel(context.FirstSelectedPawn, apparel);
                if (apparelReplacedByNewApparel == null || 
                    !ModsConfig.BiotechActive || 
                    !MechanitorUtility.TryConfirmBandwidthLossFromDroppingThing(context.FirstSelectedPawn, apparelReplacedByNewApparel, action2))
                {
                    action2();
                }
            }, 
            MenuOptionPriority.High, 
            null, null, 0f, null, null, true, 0
        ), 
        context.FirstSelectedPawn, 
        apparel, 
        "ReservedBy", 
        null
    );
}
```

---

## 6. EquipmentUtility.CanEquip() (최종 검증)

**파일**: `RimworldSource/RimWorld/EquipmentUtility.cs`

### 코드

```csharp
// EquipmentUtility.cs:66-95
public static bool CanEquip(Thing thing, Pawn pawn, out string cantReason, bool checkBonded = true)
{
    cantReason = null;
    
    // 1️⃣ Bladelink 무기 체크
    CompBladelinkWeapon compBladelinkWeapon = thing.TryGetComp<CompBladelinkWeapon>();
    if (compBladelinkWeapon != null && 
        compBladelinkWeapon.Biocodable && 
        compBladelinkWeapon.CodedPawn != null && 
        compBladelinkWeapon.CodedPawn != pawn)
    {
        cantReason = "BladelinkBondedToSomeoneElse".Translate();
        return false;
    }
    
    // 2️⃣ 생체코딩 체크
    if (CompBiocodable.IsBiocoded(thing) && !CompBiocodable.IsBiocodedFor(thing, pawn))
    {
        cantReason = "BiocodedCodedForSomeoneElse".Translate();
        return false;
    }
    
    // 3️⃣ 이미 다른 Bladelink 무기와 연결됨
    if (checkBonded && EquipmentUtility.AlreadyBondedToWeapon(thing, pawn))
    {
        cantReason = "BladelinkAlreadyBondedMessage".Translate(pawn.Named("PAWN"), pawn.equipment.bondedWeapon.Named("BONDEDWEAPON"));
        return false;
    }
    
    // 4️⃣ 역할(Role)에 의한 제한 (Ideology DLC)
    if (EquipmentUtility.RolePreventsFromUsing(pawn, thing, out cantReason))
    {
        return false;
    }
    
    // 5️⃣ 발달 단계(DevelopmentalStage) 체크 (Biotech DLC)
    if (thing.def.IsApparel && !thing.def.apparel.developmentalStageFilter.Has(pawn.DevelopmentalStage))
    {
        cantReason = "WrongDevelopmentalStageForClothing".Translate(
            pawn.DevelopmentalStage.ToString().Translate(), 
            Find.ActiveLanguageWorker.WithIndefiniteArticlePostProcessed(
                thing.def.apparel.developmentalStageFilter.ToCommaListOr(), 
                false, 
                false
            )
        );
        return false;
    }
    
    // ✅ 모든 체크 통과
    return true;
}
```

---

## 7. 검증 흐름 다이어그램

```
FloatMenuOptionProvider_Wear.GetSingleOptionFor()
    ↓
┌─────────────────────────────────────────┐
│  1️⃣ Apparel 타입 체크                    │
│     → null이면 return null              │
└─────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────┐
│  2️⃣ CanReach 체크 (경로)                 │
│     → 경로 없으면 "NoPath" 반환          │
└─────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────┐
│  3️⃣ IsBurning 체크 (불타는지)            │
│     → 불타면 "Burning" 반환              │
└─────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────┐
│  4️⃣ WouldReplaceLockedApparel 체크      │
│     → 잠긴 의복 대체하면 반환             │
└─────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────┐
│  5️⃣ IsMutant && disableApparel 체크     │
│     → Mutant가 의복 금지면 반환           │
└─────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────┐
│  6️⃣ HasPartsToWear 체크                 │
│     → 신체 부위 없으면 반환               │
└─────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────┐
│  7️⃣ ⭐ EquipmentUtility.CanEquip 체크    │
│     ├─ Bladelink 연결                   │
│     ├─ 생체코딩                         │
│     ├─ 기존 Bladelink 무기              │
│     ├─ Role 제한 (Ideology)             │
│     └─ DevelopmentalStage (Biotech)     │
└─────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────┐
│  ✅ 모든 체크 통과                       │
│     → FloatMenuOption("ForceWear", ...)  │
│       ├─ Job 생성 (JobDefOf.Wear)       │
│       └─ action 실행 시 Job 시작         │
└─────────────────────────────────────────┘
```

---

## 8. Harmony Patch 지점

### 8.1 Postfix Patch

```csharp
[HarmonyPatch(typeof(FloatMenuOptionProvider_Wear))]
[HarmonyPatch("GetSingleOptionFor")]
public static class FloatMenuPatch_ShieldWeaponCheck
{
    [HarmonyPostfix]
    public static void Postfix(ref FloatMenuOption __result, Thing clickedThing, FloatMenuContext context)
    {
        // ⭐ 이 시점에서 __result는:
        // - null (Apparel이 아님)
        // - 착용 불가 옵션 (기본 검증 실패)
        // - 착용 가능 옵션 (모든 검증 통과)
        
        // 추가 검증 로직
        if (__result != null && !__result.Disabled)
        {
            // CompShieldWeaponIncompatible 체크
            // 호환되지 않으면 __result를 착용 불가 옵션으로 대체
        }
    }
}
```

### 8.2 Patch 실행 흐름

```
FloatMenuMakerMap.GetOptions()
    ↓
GetProviderOptions()
    ↓
foreach (provider in providers)
    ↓
FloatMenuOptionProvider_Wear.GetSingleOptionFor()
    ├─ [원본 메서드 실행]
    ├─ 모든 검증 수행
    └─ return FloatMenuOption
        ↓
    [Harmony Postfix]
    FloatMenuPatch_ShieldWeaponCheck.Postfix()
    ├─ __result 확인
    ├─ CompShieldWeaponIncompatible 체크
    └─ __result 수정 (필요시)
        ↓
수정된 FloatMenuOption이 options에 추가됨
```

---

## 9. 주요 클래스 및 메서드 참조

### FloatMenuMakerMap

| 메서드 | 접근 제한자 | 역할 |
|--------|------------|------|
| `Init()` | public static | Provider 자동 등록 |
| `GetOptions(selectedPawns, clickPos, out context)` | public static | 진입점 |
| `GetProviderOptions(context, options)` | private static | Provider 순회 |
| `ShouldGenerateFloatMenuForPawn(pawn)` | public static | Pawn 상태 체크 |

### FloatMenuOptionProvider

| 메서드 | 접근 제한자 | 역할 |
|--------|------------|------|
| `Applies(context)` | public virtual | Provider 적용 여부 |
| `AppliesInt(context)` | protected virtual | Provider 적용 여부 (내부) |
| `GetOptions(context)` | public virtual | 일반 옵션 생성 |
| `GetOptionsFor(thing, context)` | public virtual | Thing 옵션 생성 |
| `GetOptionsFor(pawn, context)` | public virtual | Pawn 옵션 생성 |
| `GetSingleOption(context)` | protected virtual | 단일 옵션 생성 |
| `GetSingleOptionFor(thing, context)` | protected virtual | Thing 단일 옵션 |
| `GetSingleOptionFor(pawn, context)` | protected virtual | Pawn 단일 옵션 |
| `SelectedPawnValid(pawn, context)` | public virtual | Pawn 유효성 검증 |
| `TargetThingValid(thing, context)` | public virtual | Thing 유효성 검증 |
| `TargetPawnValid(pawn, context)` | public virtual | Pawn 유효성 검증 |

### FloatMenuOptionProvider_Wear

| 메서드 | 접근 제한자 | 역할 |
|--------|------------|------|
| `Drafted` | protected override | 징집 상태 허용 |
| `Undrafted` | protected override | 비징집 상태 허용 |
| `Multiselect` | protected override | 다중 선택 불가 |
| `AppliesInt(context)` | protected override | apparel 시스템 체크 |
| `GetSingleOptionFor(thing, context)` | protected override | ⭐ 핵심 로직 |

### EquipmentUtility

| 메서드 | 접근 제한자 | 역할 |
|--------|------------|------|
| `CanEquip(thing, pawn)` | public static | 단순 버전 |
| `CanEquip(thing, pawn, out cantReason, checkBonded)` | public static | ⭐ 최종 검증 |
| `AlreadyBondedToWeapon(thing, pawn)` | public static | Bladelink 체크 |
| `RolePreventsFromUsing(pawn, thing, out reason)` | public static | Role 제한 체크 |

---

## 10. 파일 위치

| 파일 | 경로 |
|------|------|
| FloatMenuMakerMap.cs | `RimworldSource/RimWorld/FloatMenuMakerMap.cs` |
| FloatMenuOptionProvider.cs | `RimworldSource/RimWorld/FloatMenuOptionProvider.cs` |
| FloatMenuOptionProvider_Wear.cs | `RimworldSource/RimWorld/FloatMenuOptionProvider_Wear.cs` |
| FloatMenuOptionProvider_Equip.cs | `RimworldSource/RimWorld/FloatMenuOptionProvider_Equip.cs` |
| EquipmentUtility.cs | `RimworldSource/RimWorld/EquipmentUtility.cs` |
| FloatMenuOption.cs | `RimworldSource/Verse/FloatMenuOption.cs` |
| FloatMenuContext.cs | `RimworldSource/RimWorld/FloatMenuContext.cs` |

---

## 11. 핵심 데이터 구조

### FloatMenuContext

```csharp
public class FloatMenuContext
{
    public List<Pawn> allSelectedPawns;      // 선택한 모든 Pawn
    public IntVec3 ClickedCell;              // 클릭한 셀
    public Map map;                          // 현재 맵
    
    public Pawn FirstSelectedPawn { get; }   // 첫 번째 Pawn
    public bool IsMultiselect { get; }       // 다중 선택 여부
    public List<Pawn> ValidSelectedPawns { get; } // 유효한 Pawn들
    public List<Thing> ClickedThings { get; }     // 클릭한 Thing들
    public List<Pawn> ClickedPawns { get; }       // 클릭한 Pawn들
}
```

### FloatMenuOption

```csharp
public class FloatMenuOption
{
    public string Label;                    // 표시될 텍스트
    public Action action;                   // 선택 시 실행할 액션 (null이면 비활성화)
    public MenuOptionPriority priority;     // 우선순위
    public Thing iconThing;                 // 아이콘으로 표시할 Thing
    public bool Disabled { get; }           // 비활성화 여부
    public bool targetsDespawned;           // 대상이 Despawned인지
    
    public FloatMenuOption(
        string label, 
        Action action, 
        MenuOptionPriority priority,
        Action mouseoverGuiAction = null,
        Thing revalidateClickTarget = null,
        float extraPartWidth = 0f,
        Func<Rect, bool> extraPartOnGUI = null,
        Thing iconThing = null,
        bool forceBasicStyle = true,
        int orderInPriority = 0
    )
}
```

---

## 12. 검증 순서 요약

| 순서 | 검증 | 메서드/클래스 | 실패 시 반환 |
|------|------|--------------|-------------|
| 1 | Apparel 타입 | `FloatMenuOptionProvider_Wear` | null |
| 2 | 경로 (CanReach) | `Pawn.CanReach()` | "NoPath" |
| 3 | 불타는지 | `Thing.IsBurning()` | "Burning" |
| 4 | 잠긴 의복 대체 | `Pawn_ApparelTracker.WouldReplaceLockedApparel()` | "WouldReplaceLockedApparel" |
| 5 | Mutant 제한 | `Pawn.IsMutant` | Mutant 타입명 |
| 6 | 신체 부위 | `ApparelUtility.HasPartsToWear()` | "MissingBodyParts" |
| 7 | 최종 검증 | `EquipmentUtility.CanEquip()` | cantReason |
| 7-1 | Bladelink | `CompBladelinkWeapon` | "BladelinkBondedToSomeoneElse" |
| 7-2 | 생체코딩 | `CompBiocodable` | "BiocodedCodedForSomeoneElse" |
| 7-3 | 기존 Bladelink | `AlreadyBondedToWeapon()` | "BladelinkAlreadyBondedMessage" |
| 7-4 | Role 제한 | `RolePreventsFromUsing()` | role.reason |
| 7-5 | 발달 단계 | `developmentalStageFilter` | "WrongDevelopmentalStageForClothing" |
| ✅ | 착용 가능 | - | FloatMenuOption("ForceWear", action) |

---

이 자료로 분석하시면 됩니다!

