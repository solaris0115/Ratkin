# 햄스터 휠 발전기 ITab 설정 시스템 구현 계획서

## 개요
RK_HamsterWheelGenerator에 ITab을 추가하여 사용자 지정 및 소유자 타입 설정 기능을 구현합니다.

## 요구사항
1. ITab으로 캐릭터 지정 설정 창 구현
2. 각 Generator마다 독립적으로 설정 저장
3. 복사/붙여넣기 기능으로 설정 전송
4. 플레이어 진영 pawn, 노예, 죄수 사용 가능
5. 침대처럼 "죄수용, 노예용, 정착민용" 구분
6. 죄수용으로 분류 시 죄수들이 자동 사용 가능

## 구현 단계

### 1단계: 데이터 구조 확장

#### 1.1 CompPowerPlantHamsterWheel 확장
**파일**: `Project/1.6/Source/HamsterWheel/PowerComp.cs`

**추가할 필드**:
```csharp
public class CompPowerPlantHamsterWheel : CompPowerPlant
{
    // 기존 필드...
    
    // 새로 추가할 필드
    private BedOwnerType forOwnerType = BedOwnerType.Colonist;
    private List<Pawn> assignedPawns = new List<Pawn>(); // 특정 Pawn 지정용
    private bool allowSpecificPawns = false; // 특정 Pawn만 사용 허용 여부
    
    // 복사/붙여넣기용 클립보드
    private static HamsterWheelSettings clipboard = null;
}
```

**추가할 속성**:
```csharp
public BedOwnerType ForOwnerType
{
    get => forOwnerType;
    set => forOwnerType = value;
}

public bool ForPrisoners => forOwnerType == BedOwnerType.Prisoner;
public bool ForSlaves => forOwnerType == BedOwnerType.Slave;
public bool ForColonists => forOwnerType == BedOwnerType.Colonist;

public List<Pawn> AssignedPawns => assignedPawns;
public bool AllowSpecificPawns => allowSpecificPawns;
```

**PostExposeData 확장**:
```csharp
public override void PostExposeData()
{
    base.PostExposeData();
    // 기존...
    Scribe_Values.Look(ref forOwnerType, "forOwnerType", BedOwnerType.Colonist);
    Scribe_Collections.Look(ref assignedPawns, "assignedPawns", LookMode.Reference);
    Scribe_Values.Look(ref allowSpecificPawns, "allowSpecificPawns", false);
}
```

#### 1.2 설정 데이터 클래스 생성
**파일**: `Project/1.6/Source/HamsterWheel/HamsterWheelSettings.cs` (신규)

```csharp
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NewRatkin
{
    public class HamsterWheelSettings
    {
        public BedOwnerType forOwnerType = BedOwnerType.Colonist;
        public List<Pawn> assignedPawns = new List<Pawn>();
        public bool allowSpecificPawns = false;
        
        public void CopyFrom(CompPowerPlantHamsterWheel comp)
        {
            forOwnerType = comp.ForOwnerType;
            assignedPawns = new List<Pawn>(comp.AssignedPawns);
            allowSpecificPawns = comp.AllowSpecificPawns;
        }
        
        public void ApplyTo(CompPowerPlantHamsterWheel comp)
        {
            comp.ForOwnerType = forOwnerType;
            comp.AssignedPawns.Clear();
            comp.AssignedPawns.AddRange(assignedPawns);
            comp.AllowSpecificPawns = allowSpecificPawns;
        }
    }
}
```

### 2단계: ITab 구현

#### 2.1 ITab_HamsterWheelSettings 클래스 생성
**파일**: `Project/1.6/Source/HamsterWheel/ITab_HamsterWheelSettings.cs` (신규)

**기본 구조**:
```csharp
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace NewRatkin
{
    public class ITab_HamsterWheelSettings : ITab
    {
        private Vector2 scrollPosition;
        private float scrollViewHeight;
        private static HamsterWheelSettings clipboard = null;
        
        private const float WinWidth = 420f;
        private const float WinHeight = 500f;
        
        private CompPowerPlantHamsterWheel SelWheel
        {
            get => base.SelThing?.GetComp<CompPowerPlantHamsterWheel>();
        }
        
        public ITab_HamsterWheelSettings()
        {
            this.size = new Vector2(WinWidth, WinHeight);
            this.labelKey = "RK_TabHamsterWheelSettings";
            this.tutorTag = "HamsterWheelSettings";
        }
        
        public override bool IsVisible
        {
            get
            {
                return SelWheel != null && base.SelThing.Faction == Faction.OfPlayer;
            }
        }
        
        protected override void FillTab()
        {
            // 구현 내용은 3단계에서 상세화
        }
    }
}
```

#### 2.2 UI 구성 요소
- **소유자 타입 선택**: 라디오 버튼 (Colonist/Prisoner/Slave)
- **특정 Pawn 지정**: 체크박스 + Pawn 선택 리스트
- **복사 버튼**: 현재 설정을 클립보드에 복사
- **붙여넣기 버튼**: 클립보드 설정을 현재 Generator에 적용
- **Pawn 추가/제거 버튼**: 특정 Pawn 목록 관리

### 3단계: ITab UI 상세 구현

#### 3.1 FillTab 메서드 구현
```csharp
protected override void FillTab()
{
    if (SelWheel == null) return;
    
    Rect outRect = new Rect(0f, 0f, WinWidth, WinHeight).ContractedBy(10f);
    Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, scrollViewHeight);
    
    Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);
    
    float curY = 0f;
    
    // 소유자 타입 선택
    DrawOwnerTypeSelection(viewRect.width, ref curY);
    
    // 특정 Pawn 지정 옵션
    DrawSpecificPawnsOption(viewRect.width, ref curY);
    
    // 복사/붙여넣기 버튼
    DrawCopyPasteButtons(viewRect.width, ref curY);
    
    // 크기 업데이트
    if (Event.current.type == EventType.Layout)
    {
        scrollViewHeight = curY + 20f;
    }
    
    Widgets.EndScrollView();
}
```

#### 3.2 소유자 타입 선택 UI
```csharp
private void DrawOwnerTypeSelection(float width, ref float curY)
{
    Widgets.ListSeparator(ref curY, width, "RK_OwnerType".Translate());
    
    // Colonist
    Rect colonistRect = new Rect(0f, curY, width, 30f);
    if (Widgets.RadioButtonLabeled(colonistRect, "RK_ForColonists".Translate(), 
        SelWheel.ForColonists))
    {
        SelWheel.ForOwnerType = BedOwnerType.Colonist;
    }
    curY += 30f;
    
    // Prisoner
    Rect prisonerRect = new Rect(0f, curY, width, 30f);
    if (Widgets.RadioButtonLabeled(prisonerRect, "RK_ForPrisoners".Translate(), 
        SelWheel.ForPrisoners))
    {
        SelWheel.ForOwnerType = BedOwnerType.Prisoner;
    }
    curY += 30f;
    
    // Slave (Ideology DLC 체크 필요)
    if (ModsConfig.IdeologyActive)
    {
        Rect slaveRect = new Rect(0f, curY, width, 30f);
        if (Widgets.RadioButtonLabeled(slaveRect, "RK_ForSlaves".Translate(), 
            SelWheel.ForSlaves))
        {
            SelWheel.ForOwnerType = BedOwnerType.Slave;
        }
        curY += 30f;
    }
    
    curY += 10f;
}
```

#### 3.3 특정 Pawn 지정 UI
```csharp
private void DrawSpecificPawnsOption(float width, ref float curY)
{
    Widgets.ListSeparator(ref curY, width, "RK_SpecificPawns".Translate());
    
    // 체크박스
    Rect checkRect = new Rect(0f, curY, width, 30f);
    bool allowSpecific = SelWheel.AllowSpecificPawns;
    Widgets.CheckboxLabeled(checkRect, "RK_AllowSpecificPawns".Translate(), 
        ref allowSpecific);
    SelWheel.AllowSpecificPawns = allowSpecific;
    curY += 35f;
    
    if (allowSpecific)
    {
        // Pawn 목록 표시
        foreach (Pawn pawn in SelWheel.AssignedPawns.ToList())
        {
            Rect pawnRect = new Rect(20f, curY, width - 40f, 25f);
            Widgets.Label(pawnRect, pawn.LabelShortCap);
            
            // 제거 버튼
            Rect removeRect = new Rect(width - 30f, curY, 25f, 25f);
            if (Widgets.ButtonText(removeRect, "X"))
            {
                SelWheel.AssignedPawns.Remove(pawn);
            }
            curY += 28f;
        }
        
        // Pawn 추가 버튼
        Rect addRect = new Rect(20f, curY, width - 40f, 30f);
        if (Widgets.ButtonText(addRect, "RK_AddPawn".Translate()))
        {
            OpenPawnSelectionMenu();
        }
        curY += 35f;
    }
    
    curY += 10f;
}
```

#### 3.4 복사/붙여넣기 UI
```csharp
private void DrawCopyPasteButtons(float width, ref float curY)
{
    Widgets.ListSeparator(ref curY, width, "RK_CopyPaste".Translate());
    
    Rect buttonRect = new Rect(0f, curY, (width - 10f) / 2f, 35f);
    
    // 복사 버튼
    if (Widgets.ButtonText(buttonRect, "RK_CopySettings".Translate()))
    {
        clipboard = new HamsterWheelSettings();
        clipboard.CopyFrom(SelWheel);
        Messages.Message("RK_SettingsCopied".Translate(), MessageTypeDefOf.PositiveEvent);
    }
    
    // 붙여넣기 버튼
    buttonRect.x += buttonRect.width + 10f;
    bool canPaste = clipboard != null;
    if (Widgets.ButtonText(buttonRect, "RK_PasteSettings".Translate(), 
        enabled: canPaste))
    {
        clipboard.ApplyTo(SelWheel);
        Messages.Message("RK_SettingsPasted".Translate(), MessageTypeDefOf.PositiveEvent);
    }
    
    curY += 40f;
}
```

#### 3.5 Pawn 선택 메뉴
```csharp
private void OpenPawnSelectionMenu()
{
    List<FloatMenuOption> options = new List<FloatMenuOption>();
    
    foreach (Pawn pawn in Find.CurrentMap.mapPawns.FreeColonistsAndPrisonersSpawned)
    {
        if (!SelWheel.AssignedPawns.Contains(pawn))
        {
            options.Add(new FloatMenuOption(pawn.LabelShortCap, () =>
            {
                SelWheel.AssignedPawns.Add(pawn);
            }));
        }
    }
    
    if (options.Any())
    {
        Find.WindowStack.Add(new FloatMenu(options));
    }
    else
    {
        Messages.Message("RK_NoAvailablePawns".Translate(), MessageTypeDefOf.RejectInput);
    }
}
```

### 4단계: WorkGiver 및 JobGiver 수정

#### 4.1 WorkGiver_HamsterWheel 수정
**파일**: `Project/1.6/Source/HamsterWheel/Work.cs`

**HasJobOnThing 메서드 수정**:
```csharp
public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
{
    // 기존 체크...
    
    CompPowerPlantHamsterWheel compHW = building.TryGetComp<CompPowerPlantHamsterWheel>();
    if (!compHW.CanUseNow || compHW.user != null || building.IsBurning())
    {
        return false;
    }
    
    // 소유자 타입 체크
    if (!compHW.CanPawnUse(pawn))
    {
        return false;
    }
    
    // 특정 Pawn 지정 체크
    if (compHW.AllowSpecificPawns && !compHW.AssignedPawns.Contains(pawn))
    {
        return false;
    }
    
    return true;
}
```

#### 4.2 CompPowerPlantHamsterWheel에 CanPawnUse 메서드 추가
```csharp
public bool CanPawnUse(Pawn pawn)
{
    if (pawn.Faction != Faction.OfPlayer && !pawn.IsPrisonerOfColony && !pawn.IsSlaveOfColony)
    {
        return false;
    }
    
    switch (forOwnerType)
    {
        case BedOwnerType.Prisoner:
            return pawn.IsPrisonerOfColony;
        case BedOwnerType.Slave:
            return pawn.IsSlaveOfColony;
        case BedOwnerType.Colonist:
            return pawn.IsColonist && !pawn.IsPrisoner && !pawn.IsSlave;
        default:
            return false;
    }
}
```

#### 4.3 JobGiver_RunningWheelPrisoner 수정
**파일**: `Project/1.6/Source/HamsterWheel/JobGiver.cs`

**TryGiveJob 메서드 수정**:
```csharp
protected override Job TryGiveJob(Pawn pawn)
{
    if (!pawn.IsPrisoner) return null;
    
    foreach (Building wheel in pawn.Map.listerThings.ThingsMatching(
        ThingRequest.ForDef(RK_ThingDefOf.RK_HamsterWheelGenerator)))
    {
        CompPowerPlantHamsterWheel comp = wheel.GetComp<CompPowerPlantHamsterWheel>();
        
        // 소유자 타입 체크
        if (!comp.ForPrisoners) continue;
        
        // 특정 Pawn 지정 체크
        if (comp.AllowSpecificPawns && !comp.AssignedPawns.Contains(pawn))
        {
            continue;
        }
        
        if (pawn.CanReserveAndReach(wheel, PathEndMode.InteractionCell, Danger.None) 
            && comp.user == null)
        {
            return new Job(RK_JobDefOf.RK_Job_HamsterWheel, wheel);
        }
    }
    
    return null;
}
```

### 5단계: ThingDef에 ITab 연결

#### 5.1 ThingDef 수정
**파일**: `Project/1.6/Defs/ThingDefs_Building/Buildings_Power.xml`

**inspectTabs 추가**:
```xml
<ThingDef ParentName="BuildingBase">
    <defName>RK_HamsterWheelGenerator</defName>
    <!-- 기존 내용... -->
    <inspectTabs>
        <li Class="NewRatkin.ITab_HamsterWheelSettings"/>
    </inspectTabs>
</ThingDef>
```

### 6단계: 번역 키 추가

#### 6.1 번역 파일 수정
**파일**: `Project/Contents/Languages/Korean/Keyed/RK_HamsterWheel.xml` (신규 또는 기존 파일에 추가)

```xml
<?xml version="1.0" encoding="utf-8"?>
<LanguageData>
    <RK_TabHamsterWheelSettings>햄스터 휠 설정</RK_TabHamsterWheelSettings>
    <RK_OwnerType>소유자 타입</RK_OwnerType>
    <RK_ForColonists>정착민용</RK_ForColonists>
    <RK_ForPrisoners>죄수용</RK_ForPrisoners>
    <RK_ForSlaves>노예용</RK_ForSlaves>
    <RK_SpecificPawns>특정 캐릭터 지정</RK_SpecificPawns>
    <RK_AllowSpecificPawns>특정 캐릭터만 사용 허용</RK_AllowSpecificPawns>
    <RK_AddPawn>캐릭터 추가</RK_AddPawn>
    <RK_CopyPaste>복사/붙여넣기</RK_CopyPaste>
    <RK_CopySettings>설정 복사</RK_CopySettings>
    <RK_PasteSettings>설정 붙여넣기</RK_PasteSettings>
    <RK_SettingsCopied>설정이 복사되었습니다.</RK_SettingsCopied>
    <RK_SettingsPasted>설정이 붙여넣어졌습니다.</RK_SettingsPasted>
    <RK_NoAvailablePawns>추가할 수 있는 캐릭터가 없습니다.</RK_NoAvailablePawns>
</LanguageData>
```

### 7단계: 프로젝트 파일 업데이트

#### 7.1 NewRatkin.csproj 수정
**파일**: `Project/1.6/Source/NewRatkin.csproj`

**추가할 항목**:
```xml
<ItemGroup>
    <!-- 기존 항목들... -->
    <Compile Include="HamsterWheel\ITab_HamsterWheelSettings.cs" />
    <Compile Include="HamsterWheel\HamsterWheelSettings.cs" />
</ItemGroup>
```

## 테스트 체크리스트

### 기능 테스트
- [ ] ITab이 정상적으로 표시되는가?
- [ ] 소유자 타입 변경이 저장되는가?
- [ ] 특정 Pawn 지정이 작동하는가?
- [ ] 복사/붙여넣기가 작동하는가?
- [ ] 정착민용 설정 시 정착민만 사용하는가?
- [ ] 죄수용 설정 시 죄수만 사용하는가?
- [ ] 노예용 설정 시 노예만 사용하는가?
- [ ] 특정 Pawn 지정 시 해당 Pawn만 사용하는가?

### 저장/로드 테스트
- [ ] 게임 저장 후 로드 시 설정이 유지되는가?
- [ ] 각 Generator의 설정이 독립적으로 저장되는가?

### UI 테스트
- [ ] 스크롤뷰가 정상 작동하는가?
- [ ] 버튼 클릭이 정상 작동하는가?
- [ ] 툴팁이 표시되는가?

## 참고사항

### Building_Bed 참고
- `BedOwnerType` enum 사용
- `ForPrisoners`, `ForSlaves`, `ForColonists` 속성 패턴 참고
- `SetBedOwnerTypeByInterface` 메서드 패턴 참고

### ITab 구현 참고
- `ITab_Bills.cs`: 복사/붙여넣기 패턴 참고
- `ITab_Storage.cs`: 설정 UI 패턴 참고
- `Widgets` 클래스의 다양한 UI 컴포넌트 활용

### 주의사항
1. **Ideology DLC 체크**: 노예 기능은 Ideology DLC 필요
2. **Pawn 참조 저장**: `Scribe_Collections.Look`에서 `LookMode.Reference` 사용
3. **Null 체크**: Pawn이 삭제되었을 경우 처리 필요
4. **동시 사용 방지**: `user != null` 체크 유지

