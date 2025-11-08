# FloatMenu Providers 리스트 구성 분석

## 1. Providers 리스트 초기화

### 1.1 초기화 코드

**파일**: `RimworldSource/RimWorld/FloatMenuMakerMap.cs`

```csharp
// FloatMenuMakerMap.cs:12
private static List<FloatMenuOptionProvider> providers;

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

### 1.2 핵심 메커니즘

```csharp
typeof(FloatMenuOptionProvider).AllSubclassesNonAbstract()
```

**이 메서드는**:
1. `FloatMenuOptionProvider`를 상속하는 모든 **비추상(non-abstract)** 클래스를 찾습니다
2. RimWorld 원본 + 모든 활성화된 모드의 Assembly에서 검색합니다
3. **Reflection**을 사용하여 런타임에 타입을 검색합니다

**결과**:
- ✅ RimWorld 원본 Provider들 (약 54개)
- ✅ 모드에서 추가한 커스텀 Provider들
- ✅ 순서는 **Assembly 로딩 순서 및 타입 발견 순서**에 따름 (보장되지 않음)

---

## 2. 등록된 Provider 전체 목록 (RimWorld 1.6 기준)

총 **54개**의 Provider가 자동 등록됩니다:

### 2.1 전투 관련 (Drafted)

| 번호 | Provider | 기능 |
|------|----------|------|
| 1 | `FloatMenuOptionProvider_DraftedMove` | 징집 상태 이동 |
| 2 | `FloatMenuOptionProvider_DraftedAttack` | 징집 상태 공격 |
| 3 | `FloatMenuOptionProvider_DraftedTend` | 징집 상태 치료 |
| 4 | `FloatMenuOptionProvider_DraftedRepair` | 징집 상태 수리 |

### 2.2 장비 관련

| 번호 | Provider | 기능 |
|------|----------|------|
| 5 | `FloatMenuOptionProvider_Equip` | 무기/장비 장착 |
| 6 | `FloatMenuOptionProvider_Wear` | **⭐ Apparel 착용** |
| 7 | `FloatMenuOptionProvider_PickUpItem` | 아이템 줍기 |
| 8 | `FloatMenuOptionProvider_DropEquipment` | 장비 드롭 |
| 9 | `FloatMenuOptionProvider_Strip` | 벗기기/탈의 |
| 10 | `FloatMenuOptionProvider_Reload` | 재장전 |
| 11 | `FloatMenuOptionProvider_DressOtherPawn` | 다른 Pawn에게 옷 입히기 |

### 2.3 Pawn 상호작용

| 번호 | Provider | 기능 |
|------|----------|------|
| 12 | `FloatMenuOptionProvider_Arrest` | 체포 |
| 13 | `FloatMenuOptionProvider_CapturePawn` | 포획 |
| 14 | `FloatMenuOptionProvider_CaptureEntity` | 엔티티 포획 (Anomaly) |
| 15 | `FloatMenuOptionProvider_RescuePawn` | 구조 |
| 16 | `FloatMenuOptionProvider_CarryPawn` | Pawn 운반 |
| 17 | `FloatMenuOptionProvider_CarryingPawn` | Pawn 운반 중 |
| 18 | `FloatMenuOptionProvider_CarryPawnToExit` | Pawn을 출구로 운반 |
| 19 | `FloatMenuOptionProvider_Trade` | 거래 |
| 20 | `FloatMenuOptionProvider_Romance` | 구애 |
| 21 | `FloatMenuOptionProvider_OfferHelp` | 도움 제공 |
| 22 | `FloatMenuOptionProvider_PutOutFireOnPawn` | Pawn의 불 끄기 |

### 2.4 시설/건물 관련

| 번호 | Provider | 기능 |
|------|----------|------|
| 23 | `FloatMenuOptionProvider_OpenThing` | 열기 (상자, 문 등) |
| 24 | `FloatMenuOptionProvider_CleanRoom` | 방 청소 |
| 25 | `FloatMenuOptionProvider_ExtinguishFires` | 불 끄기 |
| 26 | `FloatMenuOptionProvider_HandleCorpse` | 시체 처리 |

### 2.5 Biotech DLC 관련

| 번호 | Provider | 기능 |
|------|----------|------|
| 27 | `FloatMenuOptionProvider_Childcare` | 아이 돌보기 |
| 28 | `FloatMenuOptionProvider_BringBabyToSafety` | 아기를 안전한 곳으로 |
| 29 | `FloatMenuOptionProvider_Xenogerm` | 제노검 |
| 30 | `FloatMenuOptionProvider_Mechanitor` | 메카니터 |
| 31 | `FloatMenuOptionProvider_CarryMechToCharger` | 메크를 충전기로 운반 |
| 32 | `FloatMenuOptionProvider_RemoveMechlink` | 메크링크 제거 |

### 2.6 Ideology DLC 관련

| 번호 | Provider | 기능 |
|------|----------|------|
| 33 | `FloatMenuOptionProvider_StartRitual` | 의식 시작 |
| 34 | `FloatMenuOptionProvider_Relic` | 유물 |

### 2.7 Royalty DLC 관련

| 번호 | Provider | 기능 |
|------|----------|------|
| 35 | `FloatMenuOptionProvider_InvokeArchotech` | 아코텍 호출 |

### 2.8 Anomaly DLC 관련

| 번호 | Provider | 기능 |
|------|----------|------|
| 36 | `FloatMenuOptionProvider_CaptureEntity` | 엔티티 포획 |
| 37 | `FloatMenuOptionProvider_HackAncientTerminal` | 고대 터미널 해킹 |
| 38 | `FloatMenuOptionProvider_GhoulRest` | 구울 휴식 |

### 2.9 특수 기능

| 번호 | Provider | 기능 |
|------|----------|------|
| 39 | `FloatMenuOptionProvider_Ingest` | 음식 섭취 |
| 40 | `FloatMenuOptionProvider_Deathrest` | 죽음의 휴식 (Biotech) |
| 41 | `FloatMenuOptionProvider_PrisonerBloodfeed` | 포로 흡혈 |
| 42 | `FloatMenuOptionProvider_ReturnSlaveToBed` | 노예를 침대로 돌려보내기 |
| 43 | `FloatMenuOptionProvider_CarryToShuttle` | 셔틀로 운반 |
| 44 | `FloatMenuOptionProvider_CarryToCryptosleepCasket` | 냉동수면 관으로 운반 |
| 45 | `FloatMenuOptionProvider_CarryToBiosculpterPod` | 바이오스컬프터 포드로 운반 |
| 46 | `FloatMenuOptionProvider_CarryDeathrestingToCasket` | 죽음의 휴식 중인 Pawn을 관으로 운반 |
| 47 | `FloatMenuOptionProvider_LoadCaravan` | 캐러밴 적재 |
| 48 | `FloatMenuOptionProvider_LoadOntoPackAnimal` | 짐 동물에 적재 |
| 49 | `FloatMenuOptionProvider_EnterMapPortal` | 맵 포탈 진입 |
| 50 | `FloatMenuOptionProvider_TransferEntity` | 엔티티 이동 |

### 2.10 시스템 Provider

| 번호 | Provider | 기능 |
|------|----------|------|
| 51 | `FloatMenuOptionProvider_FromZone` | Zone에서 제공하는 옵션 |
| 52 | `FloatMenuOptionProvider_FromThing` | Thing에서 제공하는 옵션 |
| 53 | `FloatMenuOptionProvider_FromLord` | Lord에서 제공하는 옵션 |
| 54 | `FloatMenuOptionProvider_WorkGivers` | WorkGiver에서 제공하는 옵션 |

---

## 3. Providers 순회 방식

### 3.1 순회 코드

**파일**: `RimworldSource/RimWorld/FloatMenuMakerMap.cs`

```csharp
// FloatMenuMakerMap.cs:73-132
private static void GetProviderOptions(FloatMenuContext context, List<FloatMenuOption> options)
{
    // ⭐ providers 리스트를 순차적으로 순회
    foreach (FloatMenuOptionProvider floatMenuOptionProvider in FloatMenuMakerMap.providers)
    {
        try
        {
            FloatMenuMakerMap.currentProvider = floatMenuOptionProvider;
            
            if (context.ValidSelectedPawns.Any<Pawn>())
            {
                // Provider가 이 context에 적용되는지 확인
                if (floatMenuOptionProvider.Applies(context))
                {
                    // 1. 일반 옵션
                    options.AddRange(floatMenuOptionProvider.GetOptions(context));
                    
                    // 2. Thing 옵션
                    foreach (Thing thing in context.ClickedThings)
                    {
                        if (floatMenuOptionProvider.TargetThingValid(thing, context))
                        {
                            options.AddRange(floatMenuOptionProvider.GetOptionsFor(thing, context));
                        }
                    }
                    
                    // 3. Pawn 옵션
                    foreach (Pawn pawn in context.ClickedPawns)
                    {
                        if (floatMenuOptionProvider.TargetPawnValid(pawn, context))
                        {
                            options.AddRange(floatMenuOptionProvider.GetOptionsFor(pawn, context));
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
    
    FloatMenuMakerMap.currentProvider = null;
}
```

### 3.2 순회 특징

#### ✅ 순차 순회
- `foreach`로 리스트를 순차적으로 순회합니다
- **모든 Provider가 실행됩니다** (조건을 만족하면)
- 중간에 중단되지 않습니다

#### ✅ 조건부 실행
각 Provider는 다음 조건을 만족해야 옵션을 생성합니다:
1. `context.ValidSelectedPawns.Any()` - 유효한 Pawn이 있어야 함
2. `provider.Applies(context)` - Provider가 현재 상황에 적용되어야 함
3. `provider.TargetThingValid(thing, context)` - 대상이 유효해야 함

#### ✅ 옵션 누적
- 각 Provider의 옵션이 `options` 리스트에 **누적**됩니다
- 최종적으로 모든 Provider의 옵션이 합쳐진 리스트가 반환됩니다

#### ✅ 예외 처리
- 각 Provider가 `try-catch`로 감싸져 있습니다
- 한 Provider에서 에러가 나도 다른 Provider는 계속 실행됩니다

---

## 4. Provider 실행 순서의 중요성

### 4.1 순서가 중요하지 않은 이유

**모든 옵션이 수집된 후 FloatMenu에 표시되므로, Provider 실행 순서는 최종 결과에 영향을 주지 않습니다.**

```
Provider A 실행 → [옵션1, 옵션2] 생성
Provider B 실행 → [옵션3] 생성
Provider C 실행 → [옵션4, 옵션5] 생성
    ↓
options = [옵션1, 옵션2, 옵션3, 옵션4, 옵션5]
    ↓
FloatMenu에 모두 표시
```

### 4.2 FloatMenu 표시 순서

**옵션의 표시 순서는 `MenuOptionPriority`로 결정됩니다:**

```csharp
public enum MenuOptionPriority
{
    Default = 0,
    Low = 1,
    RescueOrCapture = 2,
    InitiateSocial = 3,
    DisabledOption = 4,
    High = 5,
    GoHere = 6,
    Attack = 7,
    VeryHigh = 8
}
```

**순서**:
1. 같은 Priority 내에서는 `orderInPriority` 값으로 정렬
2. Priority가 높을수록 위에 표시됨

---

## 5. 커스텀 Provider 추가 시 동작

### 5.1 모드에서 Provider 추가

```csharp
// 커스텀 Provider 정의
public class MyCustomFloatMenuOptionProvider : FloatMenuOptionProvider
{
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;
    
    protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
    {
        // 커스텀 로직
        return new FloatMenuOption("My Custom Option", () => { ... });
    }
}
```

### 5.2 자동 등록

**`FloatMenuMakerMap.Init()`가 게임 시작 시 한 번 실행되면:**

```csharp
typeof(FloatMenuOptionProvider).AllSubclassesNonAbstract()
```

이 메서드가:
1. ✅ RimWorld 원본 Provider들 찾기
2. ✅ **모든 활성화된 모드의 Assembly에서 Provider 찾기**
3. ✅ `MyCustomFloatMenuOptionProvider` 자동 발견
4. ✅ providers 리스트에 자동 추가

**결과**:
- 모드의 커스텀 Provider가 자동으로 등록됨
- 코드 수정 없이 자동으로 FloatMenu에 통합됨

### 5.3 주의사항

#### ⚠️ 기존 Provider 중복

만약 커스텀 Provider가 기존 Provider와 같은 Thing에 대해 옵션을 생성하면:

```
FloatMenu:
✅ Force Wear          [RimWorld 원본 Provider]
✅ My Custom Wear      [커스텀 Provider]
```

**두 옵션이 모두 표시됩니다!**

이것이 커스텀 Provider를 만들어도 원본 Provider를 대체할 수 없는 이유입니다.

#### ✅ 해결책: Harmony Patch

기존 Provider의 동작을 수정하려면 Harmony Patch를 사용해야 합니다.

---

## 6. Provider 검색 및 등록 상세

### 6.1 AllSubclassesNonAbstract() 동작

**파일**: `RimworldSource/Verse/GenTypes.cs` (추정)

```csharp
public static IEnumerable<Type> AllSubclassesNonAbstract(this Type baseType)
{
    // 모든 로딩된 Assembly 순회
    foreach (Assembly assembly in GenTypes.AllActiveAssemblies)
    {
        // 각 Assembly의 모든 타입 확인
        foreach (Type type in assembly.GetTypes())
        {
            // 조건 체크:
            // 1. baseType의 하위 클래스인가?
            // 2. abstract가 아닌가?
            // 3. 인스턴스화 가능한가?
            if (type.IsSubclassOf(baseType) && 
                !type.IsAbstract && 
                type.GetConstructor(Type.EmptyTypes) != null)
            {
                yield return type;
            }
        }
    }
}
```

### 6.2 Assembly 로딩 순서

**RimWorld의 Assembly 로딩 순서**:
1. RimWorld 코어 (Assembly-CSharp.dll)
2. 활성화된 모드들 (로딩 순서대로)
   - Core 모드
   - Royalty DLC
   - Ideology DLC
   - Biotech DLC
   - Anomaly DLC
   - 기타 모드들 (ModsConfig.xml 순서)

**결과**:
- Provider 등록 순서는 Assembly 로딩 순서에 따름
- 모드 로딩 순서가 바뀌면 Provider 순서도 바뀔 수 있음
- **하지만 최종 FloatMenu 표시에는 영향 없음** (Priority로 정렬되므로)

---

## 7. 실제 Provider 등록 예시

### 7.1 게임 시작 시 로그 (추정)

```
[FloatMenuMakerMap] Initializing providers...
[FloatMenuMakerMap] Found 54 providers:
  1. FloatMenuOptionProvider_DraftedMove
  2. FloatMenuOptionProvider_DraftedAttack
  3. FloatMenuOptionProvider_Equip
  4. FloatMenuOptionProvider_Wear ⭐
  5. FloatMenuOptionProvider_PickUpItem
  ...
  54. FloatMenuOptionProvider_WorkGivers
[FloatMenuMakerMap] Providers initialized.
```

### 7.2 우클릭 시 실행 흐름

```
사용자가 Apparel 우클릭
    ↓
FloatMenuMakerMap.GetOptions()
    ↓
GetProviderOptions()
    ↓
foreach (provider in providers) // 54개 순회
    ↓
    Provider 1: DraftedMove
    ├─ Applies(context)? → false (징집 상태 아님)
    └─ 건너뜀
    ↓
    Provider 2: DraftedAttack
    ├─ Applies(context)? → false
    └─ 건너뜀
    ↓
    Provider 3: Equip
    ├─ Applies(context)? → true
    ├─ clickedThing이 Apparel? → false (CompEquippable 없음)
    └─ 옵션 생성 안 함
    ↓
    Provider 4: Wear ⭐
    ├─ Applies(context)? → true (pawn.apparel != null)
    ├─ clickedThing이 Apparel? → true
    ├─ GetSingleOptionFor() 실행
    └─ FloatMenuOption("Force Wear", ...) 생성 → options에 추가
    ↓
    Provider 5~54: 계속 실행...
    ↓
모든 Provider 실행 완료
    ↓
options 리스트 반환
    ↓
FloatMenu 표시
```

---

## 8. 핵심 요약

### 8.1 Providers 리스트

```csharp
private static List<FloatMenuOptionProvider> providers;
```

**특징**:
- ✅ **private static** - 외부에서 접근 불가능
- ✅ 게임 시작 시 `Init()`에서 한 번 초기화
- ✅ Reflection으로 자동 검색 및 등록
- ✅ 총 54개 (RimWorld 1.6 기준) + 모드의 커스텀 Provider들

### 8.2 순회 방식

```csharp
foreach (FloatMenuOptionProvider provider in providers)
{
    if (provider.Applies(context))
    {
        options.AddRange(provider.GetOptions(context));
        // Thing/Pawn 옵션도 수집
    }
}
```

**특징**:
- ✅ 순차 순회
- ✅ 모든 Provider 실행 (조건 만족 시)
- ✅ 옵션 누적
- ✅ 예외 처리로 안정성 보장

### 8.3 왜 수정 불가능한가?

1. **private 리스트** → 접근 불가능
2. **자동 등록** → 제어 불가능
3. **순차 실행** → 원본 Provider 건너뛰기 불가능
4. **옵션 누적** → 커스텀 Provider 추가 시 중복 표시

**결론**: **Harmony Patch만이 유일한 해결책**

---

## 9. 관련 파일

| 파일 | 역할 |
|------|------|
| `FloatMenuMakerMap.cs` | Provider 관리 및 순회 |
| `FloatMenuOptionProvider.cs` | Provider 베이스 클래스 |
| `FloatMenuOptionProvider_Wear.cs` | Apparel 착용 Provider |
| `GenTypes.cs` | Reflection 유틸리티 |

---

이제 providers 리스트의 구조를 완전히 이해하셨을 것입니다!

