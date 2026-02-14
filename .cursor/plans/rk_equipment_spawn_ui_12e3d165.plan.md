---
name: RK Equipment Spawn UI
overview: 정착민 수 기준 장비 스폰 디버그 기능을 체크박스 UI로 개선하고, 품질 선택·아이템 선택·스폰 위치 지정을 지원하며, 설정을 게임 재시작 후에도 유지하도록 구현합니다.
todos: []
isProject: false
---

# RK Equipment Spawn 디버그 UI 개선 계획

## 요구사항 정리

1. **버튼 클릭 시**: Spawn Thing처럼 UI 창(Dialog)이 열림
2. **UI 구성**:
  - [Spawn] 버튼
  - 품질 선택: 빈약/일반/상급/우수/걸작/전설 (단일 선택, 기본값: 일반)
  - 아이템 목록: ThingDef별 체크박스 (선택된 것만 스폰)
3. **체크박스/품질 설정**: 게임 종료 후 재실행해도 유지 (영구 저장)
4. **Spawn 버튼 클릭 후**: Spawn Thing처럼 맵에서 지점 선택(ToolMap)

## 구현 전략

### 1. 설정 저장 클래스 (영구 저장)

- **파일**: `GenFilePaths.ConfigFolderPath` + `"Ratkin_DebugSpawnConfig.xml"`
- **저장 데이터**:
  - `QualityCategory selectedQuality` (기본: Normal)
  - `List<string> enabledThingDefNames` (체크된 ThingDef defName 목록)
- **방식**: `IExposable` + `Scribe`로 XML 저장/로드
- **로드 시점**: Dialog 열릴 때
- **저장 시점**: 체크박스/품질 변경 시마다 `Write()` 호출

### 2. Dialog 클래스 신규 생성

**파일**: [Project/1.6/Source/Dialog_RKSpawnEquipmentConfig.cs](Project/1.6/Source/Dialog_RKSpawnEquipmentConfig.cs)

- `Window` 상속, `Dialog_OptionLister` 또는 `Window` 직접 상속
- **UI 레이아웃** (상단→하단):
  1. **Spawn 버튼**: 클릭 시 창 닫고 `DebugTools.curTool`에 맵 클릭용 `DebugTool` 설정
  2. **품질 행**: `QualityCategory` 6개를 가로 배치, `Widgets.RadioButton` 또는 `Widgets.CheckboxLabeled`로 단일 선택
  3. **아이템 목록**: 스크롤 영역, `Widgets.CheckboxLabeled(rect, def.LabelCap, ref enabled)` 형태로 각 ThingDef 체크박스
- **아이템 목록 소스**:  
Ratkin 모드 ThingDef 중 `IsApparel` 또는 `IsWeapon`인 것, `defName.StartsWith("RK_")` 필터 (또는 기존 5종 + 확장 가능 목록)
- **참고**: [LudeonTK/Dialog_DebugOptionLister.cs](RimworldSource/LudeonTK/Dialog_DebugOptionLister.cs), [LudeonTK/Dialog_DebugOptionLister.cs](RimworldSource/LudeonTK/Dialog_DebugOptionLister.cs)의 `DoListingItems` 패턴

### 3. Spawn 로직 (ToolMap)

- Spawn 버튼 클릭 시:
  - `DebugTools.curTool = new DebugTool("Spawn RK Equipment...", () => { ... }, null)`
  - 콜백에서 `UI.MouseCell()`로 클릭한 셀 사용
  - 선택된 ThingDef × colonist 수만큼 생성, `selectedQuality` 적용
  - `GenPlace.TryPlaceThing`으로 배치 (기존 `SpawnRKEquipmentPerColonist` 로직 재사용)

### 4. DebugActions 수정

**파일**: [Project/1.6/Source/DebugActions.cs](Project/1.6/Source/DebugActions.cs)

- `SpawnRKEquipmentPerColonist`를 **Action** 타입으로 변경
- 동작: `Find.WindowStack.Add(new Dialog_RKSpawnEquipmentConfig())` 호출
- 기존 즉시 스폰 로직은 Dialog 내부 Spawn 버튼 → ToolMap 콜백으로 이동

### 5. 설정 저장용 데이터 클래스

**파일**: `RKSpawnEquipmentConfig.cs` (또는 Dialog 파일 내 중첩 클래스)

```csharp
public class RKSpawnEquipmentConfig : IExposable
{
    public QualityCategory quality = QualityCategory.Normal;
    public List<string> enabledDefNames = new List<string> { "RK_Apparel_SpaceArmor", ... };
    public void ExposeData() { ... }
}
```

- 로드: `ConfigFolderPath` + `Ratkin_DebugSpawnConfig.xml`에서 `Scribe_Deep.Look` 또는 `DirectXmlLoader`
- 저장: `ScribeSaver` 또는 `DirectXmlSaver`로 동일 경로에 저장

### 6. csproj 반영

- 새 C# 파일 추가 시 [NewRatkin.csproj](Project/1.6/Source/NewRatkin.csproj)에 `Compile Include` 추가 (현재 `**\*.cs`로 자동 포함 가능성 있음)

## 파일 변경 요약


| 파일                                 | 작업                                    |
| ---------------------------------- | ------------------------------------- |
| `Dialog_RKSpawnEquipmentConfig.cs` | 신규 - Dialog UI, Spawn 버튼, 품질/아이템 체크박스 |
| `RKSpawnEquipmentConfig.cs`        | 신규 - IExposable 설정 데이터 + Load/Save    |
| `DebugActions.cs`                  | 수정 - 버튼 클릭 시 Dialog 열기, 기존 스폰 로직 제거   |


## 참고 코드

- **체크박스**: `Widgets.CheckboxLabeled(rect, label, ref value)` ([TransferableUIUtility.cs](RimworldSource/RimWorld/TransferableUIUtility.cs) 등)
- **품질 라디오**: `Widgets.RadioButton` 또는 품질별 버튼
- **ToolMap 패턴**: [DebugThingPlaceHelper.cs](RimworldSource/Verse/DebugThingPlaceHelper.cs) 78행 - `DebugActionType.ToolMap` + `UI.MouseCell()`
- **설정 저장**: [LoadedModManager.cs](RimworldSource/Verse/LoadedModManager.cs) 631-676행 (ModSettings 대신 직접 파일 I/O)

