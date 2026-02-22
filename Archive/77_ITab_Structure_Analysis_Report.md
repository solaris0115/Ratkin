# ITab 구조 분석 보고서

## 개요
RimWorld의 ITab(Inspect Tab) 시스템은 객체를 선택했을 때 표시되는 정보 창의 탭 구조를 정의합니다.

## 클래스 계층 구조

### 기본 클래스
```
InspectTabBase (Verse)
  └─ ITab (RimWorld)
      ├─ ITab_Pawn_*
      ├─ ITab_ContentsBase
      ├─ ITab_Bills
      ├─ ITab_Storage
      └─ 기타 특수 ITab들
```

### 핵심 클래스

#### InspectTabBase (Verse/InspectTabBase.cs)
- **역할**: 모든 Inspect Tab의 기본 추상 클래스
- **주요 멤버**:
  - `labelKey`: 탭 라벨 키
  - `size`: 탭 크기 (Vector2)
  - `tutorTag`: 튜토리얼 태그
  - `IsVisible`: 탭 표시 여부
  - `Hidden`: 탭 숨김 여부
  - `VisibleInBlueprintMode`: 청사진 모드에서 표시 여부
- **주요 메서드**:
  - `DoTabGUI()`: 탭 GUI 렌더링
  - `FillTab()`: 탭 내용 채우기 (추상)
  - `CloseTab()`: 탭 닫기 (추상)
  - `UpdateSize()`: 크기 업데이트
  - `TabTick()`: 매 틱 호출
  - `TabUpdate()`: 업데이트 호출
  - `ExtraOnGUI()`: 추가 GUI 렌더링

#### ITab (RimWorld/ITab.cs)
- **역할**: 맵 내 객체용 Inspect Tab 기본 클래스
- **주요 속성**:
  - `SelObject`: 선택된 단일 객체
  - `AllSelObjects`: 선택된 모든 객체 리스트
  - `SelThing`: 선택된 Thing
  - `SelPawn`: 선택된 Pawn (가상)
  - `InspectPane`: MainTabWindow_Inspect 참조
- **특징**: `MainTabWindow_Inspect`와 연동하여 작동

## 주요 ITab 구현 예시

### 1. ITab_Pawn_Character
- **용도**: Pawn의 캐릭터 정보 표시
- **특징**: `CharacterCardUtility` 사용
- **구현 패턴**:
```csharp
protected override void FillTab()
{
    CharacterCardUtility.DrawCharacterCard(rect, pawn, ...);
}
```

### 2. ITab_Bills
- **용도**: 작업대의 Bill 목록 관리
- **특징**: 스크롤뷰, FloatMenu 사용
- **구현 패턴**:
```csharp
protected override void FillTab()
{
    Widgets.BeginScrollView(...);
    SelTable.billStack.DoListing(...);
    Widgets.EndScrollView();
}
```

### 3. ITab_ContentsBase
- **용도**: 컨테이너 내용물 표시 (추상 베이스)
- **파생 클래스**:
  - `ITab_ContentsCasket`
  - `ITab_ContentsTransporter`
  - `ITab_ContentsOutfitStand`
  - `ITab_ContentsGenepackHolder`
  - `ITab_ContentsBooks`
  - `ITab_ContentsMapPortal`
- **구현 패턴**:
```csharp
protected override void FillTab()
{
    Widgets.BeginScrollView(...);
    DoItemsLists(rect, ref curY);
    Widgets.EndScrollView();
}
```

### 4. ITab_Pawn_Gear
- **용도**: Pawn의 장비/의복/인벤토리 표시
- **특징**: 복잡한 스크롤뷰, 드롭 기능
- **구현 패턴**:
```csharp
protected override void FillTab()
{
    Widgets.BeginScrollView(...);
    // Equipment, Apparel, Inventory 섹션별 렌더링
    DrawThingRow(...);
    Widgets.EndScrollView();
}
```

### 5. ITab_Storage
- **용도**: 저장소 설정
- **파생 클래스**:
  - `ITab_Shells`
  - `ITab_BiosculpterNutritionStorage`

### 6. ITab_PenBase
- **용도**: 펜 관련 탭 베이스
- **파생 클래스**:
  - `ITab_PenAnimals`
  - `ITab_PenFood`
  - `ITab_PenAutoCut`

### 7. ITab_Pawn_Visitor
- **용도**: 방문자 관련 탭 베이스
- **파생 클래스**:
  - `ITab_Pawn_Guest`
  - `ITab_Pawn_Prisoner`
  - `ITab_Pawn_Slave`

## 주요 UI 컴포넌트 (Widgets 클래스)

### 기본 위젯
- `Widgets.Label()`: 텍스트 라벨
- `Widgets.ButtonText()`: 텍스트 버튼
- `Widgets.ButtonImage()`: 이미지 버튼
- `Widgets.ButtonInvisible()`: 투명 버튼
- `Widgets.Checkbox()`: 체크박스
- `Widgets.RadioButton()`: 라디오 버튼

### 스크롤뷰
- `Widgets.BeginScrollView()`: 스크롤뷰 시작
- `Widgets.EndScrollView()`: 스크롤뷰 종료

### 리스트/테이블
- `Widgets.ListSeparator()`: 리스트 구분선
- `Widgets.NoneLabel()`: "없음" 라벨

### 정보 표시
- `Widgets.InfoCardButton()`: 정보 카드 버튼
- `Widgets.ThingIcon()`: Thing 아이콘
- `Widgets.DrawTextureFitted()`: 텍스처 그리기
- `TooltipHandler.TipRegion()`: 툴팁 표시

### 입력
- `Widgets.TextField()`: 텍스트 입력 필드
- `Widgets.IntRange()`: 정수 범위 슬라이더
- `Widgets.FloatRange()`: 실수 범위 슬라이더
- `Widgets.Slider()`: 슬라이더

### 기타
- `Widgets.Dropdown()`: 드롭다운 메뉴
- `Widgets.ColorSelector()`: 색상 선택기
- `Widgets.FillableBar()`: 채움 바
- `Widgets.HorizontalSlider()`: 수평 슬라이더

## MainTabWindow_Inspect 연동

### 역할
- ITab들을 관리하는 메인 윈도우
- `CurTabs` 속성으로 현재 열린 탭들 관리
- `PaneTopY` 속성으로 탭 위치 계산

### ITab과의 관계
- ITab은 `MainTabWindow_Inspect`를 통해 표시됨
- `Find.MainTabsRoot.OpenTab.TabWindow`로 접근
- `CloseOpenTab()` 메서드로 탭 닫기

## 사용 패턴

### 기본 ITab 구현
```csharp
public class ITab_Custom : ITab
{
    public ITab_Custom()
    {
        this.size = new Vector2(460f, 450f);
        this.labelKey = "TabCustom";
        this.tutorTag = "Custom";
    }

    protected override void FillTab()
    {
        Rect rect = new Rect(0f, 0f, this.size.x, this.size.y);
        Widgets.Label(rect, "Custom Tab Content");
    }

    public override bool IsVisible
    {
        get
        {
            return base.SelThing != null;
        }
    }
}
```

### 스크롤뷰 사용
```csharp
protected override void FillTab()
{
    Rect outRect = new Rect(0f, 0f, this.size.x, this.size.y);
    Rect viewRect = new Rect(0f, 0f, this.size.x - 16f, scrollViewHeight);
    Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);
    // 내용 렌더링
    Widgets.EndScrollView();
}
```

### Thing 리스트 표시
```csharp
protected void DrawThingRow(ref float y, float width, Thing thing)
{
    Rect rect = new Rect(0f, y, width, 28f);
    Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing);
    Widgets.Label(new Rect(36f, y, width - 36f, 28f), thing.LabelCap);
    y += 28f;
}
```

## 주요 유틸리티 클래스

### CharacterCardUtility
- Pawn 캐릭터 카드 렌더링
- `DrawCharacterCard()`: 캐릭터 카드 그리기
- `PawnCardSize()`: 카드 크기 계산

### CaravanThingsTabUtility
- 캐러밴 관련 UI 유틸리티
- `DrawMass()`: 무게 표시
- `AbandonButtonTex`: 버튼 텍스처

### BillUtility
- Bill 관련 유틸리티
- `Clipboard`: 클립보드 Bill

## Def 연동

### InspectTabDef
- ITab을 Def로 정의하는 시스템 (확인 필요)
- ThingDef의 `inspectTabs` 필드로 연결 가능

## 참고사항

1. **크기 관리**: `UpdateSize()`에서 동적으로 크기 조정 가능
2. **가시성 제어**: `IsVisible` 속성으로 조건부 표시
3. **선택 객체 접근**: `SelThing`, `SelPawn` 등으로 접근
4. **윈도우 레이어**: `WindowLayer.GameUI`에서 렌더링
5. **이벤트 처리**: `TabTick()`, `TabUpdate()`로 업데이트 처리

