# 캐릭터 카드에서 Ideo 정보 드로잉 분석 리포트

## 개요
RimWorld의 캐릭터 상세 정보 창(Character Card)에서 Pawn으로부터 Ideo(이데올로기) 관련 정보를 가져와 화면에 그리는 메커니즘을 분석한 리포트입니다.

## 분석 일자
2025-01-27

## 1. 전체 흐름 개요

캐릭터 카드에서 Ideo 정보를 표시하는 과정은 다음과 같습니다:

1. **ITab_Pawn_Character** → 캐릭터 탭 UI 클래스
2. **CharacterCardUtility.DrawCharacterCard()** → 캐릭터 카드 전체 렌더링
3. **CharacterCardUtility.DoTopStack()** → 상단 스택 요소 렌더링 (Ideo 포함)
4. **IdeoUIUtility.DrawIdeoPlate()** → Ideo 아이콘 및 이름 렌더링
5. **Ideo.DrawIcon()** → 실제 Ideo 아이콘 텍스처 그리기

## 2. Pawn에서 Ideo 정보 추출

### 2.1 Pawn의 Ideo 접근

**코드 위치:** `CharacterCardUtility.cs:935-949`

```csharp
if (!Find.IdeoManager.classicMode && CS$<>8__locals1.pawn.Ideo != null && ModsConfig.IdeologyActive)
{
    // pawn.Ideo를 통해 Ideo 객체에 접근
    IdeoUIUtility.DrawIdeoPlate(r, CS$<>8__locals1.pawn.Ideo, CS$<>8__locals1.pawn);
}
```

**접근 경로:**
- `pawn.Ideo` → `Ideo` 객체
- `pawn.Ideo`는 `Pawn_IdeoTracker` 타입의 `ideo` 필드를 통해 접근됩니다.

### 2.2 Role 정보 추출

**코드 위치:** `CharacterCardUtility.cs:950-997`

```csharp
if (ModsConfig.IdeologyActive)
{
    Ideo ideo = CS$<>8__locals1.pawn.Ideo;
    Precept_Role role = (ideo != null) ? ideo.GetRole(CS$<>8__locals1.pawn) : null;
    
    if (role != null)
    {
        // Role 아이콘 그리기
        GUI.color = CS$<>8__locals1.pawn.Ideo.Color;
        GUI.DrawTexture(position, CS$<>8__locals3.role.Icon);
        GUI.color = Color.white;
    }
}
```

**접근 경로:**
- `pawn.Ideo` → `Ideo` 객체
- `ideo.GetRole(pawn)` → `Precept_Role` 객체 (해당 Pawn의 역할)
- `role.Icon` → Role 아이콘 텍스처

## 3. Ideo 아이콘 렌더링 과정

### 3.1 DrawIdeoPlate 메서드

**코드 위치:** `IdeoUIUtility.cs:2300-2326`

```csharp
public static void DrawIdeoPlate(Rect r, Ideo ideo, Pawn pawn = null)
{
    Widgets.DrawHighlightIfMouseover(r);
    Rect rect = new Rect(r.x, r.y, r.width, r.height);
    Rect rect2 = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);  // 아이콘 영역 (20x20)
    
    // Ideo 아이콘 그리기
    ideo.DrawIcon(rect2);
    
    // Ideo 이름 표시
    Widgets.Label(new Rect(rect.x + rect.height + 5f, rect.y, rect.width - 10f, rect.height), ideo.name);
    
    // 클릭 시 Ideo 정보 창 열기
    if (Widgets.ButtonInvisible(r, true))
    {
        IdeoUIUtility.OpenIdeoInfo(ideo);
    }
    
    // 툴팁 표시
    if (Mouse.IsOver(r))
    {
        // Ideo 이름, 확신도(Certainty), 이전 Ideo 정보 등 표시
    }
}
```

**주요 동작:**
1. 아이콘 영역: `(x+1, y+1, 20, 20)` 크기의 Rect 생성
2. `ideo.DrawIcon(rect2)` 호출하여 아이콘 그리기
3. 아이콘 옆에 Ideo 이름 텍스트 표시
4. 클릭 가능한 영역으로 설정하여 Ideo 정보 창 열기

### 3.2 Ideo.DrawIcon 메서드

**코드 위치:** `Ideo.cs:1277-1283`

```csharp
public void DrawIcon(Rect rect)
{
    Color color = GUI.color;
    GUI.color = this.Color;  // Ideo의 색상으로 변경
    GUI.DrawTexture(rect, this.Icon);  // 아이콘 텍스처 그리기
    GUI.color = color;  // 원래 색상으로 복원
}
```

**주요 동작:**
1. 현재 GUI 색상 저장
2. `this.Color`로 GUI 색상 변경 (Ideo의 고유 색상)
3. `this.Icon` 텍스처를 지정된 Rect에 그리기
4. 원래 색상으로 복원

### 3.3 Ideo.Icon 속성

**코드 위치:** `Ideo.cs:131-142`

```csharp
public Texture2D Icon
{
    get
    {
        Texture2D result;
        if ((result = this.icon) == null)
        {
            // iconDef가 있으면 iconDef.iconPath에서 로드, 없으면 BadTex
            result = (this.icon = ContentFinder<Texture2D>.Get(
                (this.iconDef != null) ? this.iconDef.iconPath : BaseContent.BadTexPath, 
                true));
        }
        return result;
    }
}
```

**아이콘 로드 과정:**
1. `this.icon` 캐시 확인
2. 캐시가 없으면:
   - `this.iconDef`가 있으면 → `iconDef.iconPath`에서 텍스처 로드
   - `this.iconDef`가 없으면 → `BaseContent.BadTexPath` (기본 배드 텍스처) 사용
3. 로드한 텍스처를 `this.icon`에 캐시
4. 텍스처 반환

### 3.4 Ideo.Color 속성

**코드 위치:** `Ideo.cs:168-188`

```csharp
public Color Color
{
    get
    {
        if (this.classicMode)
        {
            return Color.white;  // 클래식 모드면 흰색
        }
        
        // primaryFactionColor가 있으면 우선 사용
        Color? color = this.primaryFactionColor;
        if (color != null)
        {
            return color.GetValueOrDefault();
        }
        
        // colorDef에서 색상 가져오기
        ColorDef colorDef = this.colorDef;
        if (colorDef == null)
        {
            return Color.white;  // 기본값은 흰색
        }
        return colorDef.color;
    }
}
```

**색상 우선순위:**
1. `classicMode`가 true → `Color.white`
2. `primaryFactionColor`가 있으면 → 해당 색상 사용
3. `colorDef`가 있으면 → `colorDef.color` 사용
4. 모두 없으면 → `Color.white`

## 4. Role 아이콘 렌더링 과정

### 4.1 Role 정보 가져오기

**코드 위치:** `CharacterCardUtility.cs:955-956`

```csharp
Ideo ideo = CS$<>8__locals1.pawn.Ideo;
Precept_Role role = (ideo != null) ? ideo.GetRole(CS$<>8__locals1.pawn) : null;
```

**접근 경로:**
- `pawn.Ideo` → `Ideo` 객체
- `ideo.GetRole(pawn)` → 해당 Pawn이 가진 Role 반환 (없으면 null)

### 4.2 Role 아이콘 그리기

**코드 위치:** `CharacterCardUtility.cs:973-977`

```csharp
Rect position = new Rect(r.x + 1f, r.y + 1f, 20f, 20f);
GUI.color = CS$<>8__locals1.pawn.Ideo.Color;  // Ideo 색상으로 변경
GUI.DrawTexture(position, CS$<>8__locals3.role.Icon);  // Role 아이콘 그리기
GUI.color = Color.white;  // 색상 복원
```

**주요 동작:**
1. 아이콘 영역: `(x+1, y+1, 20, 20)` 크기의 Rect 생성
2. GUI 색상을 Ideo의 색상으로 변경
3. `role.Icon` 텍스처를 그리기
4. 색상 복원

### 4.3 Precept_Role.Icon 속성

**코드 위치:** `Precept.cs:147-153`

```csharp
public virtual Texture2D Icon
{
    get
    {
        // def.Icon이 있으면 우선 사용, 없으면 ideo.Icon 사용
        return this.def.Icon ?? this.ideo.Icon;
    }
}
```

**아이콘 우선순위:**
1. `this.def.Icon` (PreceptDef의 아이콘)이 있으면 사용
2. 없으면 `this.ideo.Icon` (Ideo의 아이콘) 사용

### 4.4 Precept_Role.DrawIcon 메서드

**코드 위치:** `Precept_Role.cs:594-599`

```csharp
public override void DrawIcon(Rect rect)
{
    GUI.color = this.ideo.Color;  // Ideo 색상으로 변경
    GUI.DrawTexture(rect, this.Icon);  // Role 아이콘 그리기
    GUI.color = Color.white;  // 색상 복원
}
```

## 5. 데이터 구조 요약

### 5.1 Pawn → Ideo 접근 경로

```
Pawn
  └─ ideo: Pawn_IdeoTracker
       └─ Ideo: Ideo 객체
            ├─ Icon: Texture2D (iconDef.iconPath에서 로드)
            ├─ Color: Color (primaryFactionColor 또는 colorDef.color)
            ├─ name: string
            └─ GetRole(Pawn): Precept_Role
                 └─ Icon: Texture2D (def.Icon ?? ideo.Icon)
```

### 5.2 Ideo 아이콘 정보 구조

```
Ideo
  ├─ iconDef: IdeoIconDef
  │    └─ iconPath: string (텍스처 경로)
  ├─ icon: Texture2D (캐시된 텍스처)
  ├─ colorDef: ColorDef
  │    └─ color: Color
  └─ primaryFactionColor: Color? (우선순위 높음)
```

### 5.3 Role 아이콘 정보 구조

```
Precept_Role
  ├─ def: PreceptDef
  │    └─ Icon: Texture2D? (우선 사용)
  └─ ideo: Ideo
       └─ Icon: Texture2D (def.Icon이 없을 때 사용)
```

## 6. 렌더링 위치 및 크기

### 6.1 Ideo 아이콘

- **위치:** `DoTopStack()` 메서드 내에서 상단 스택 요소로 렌더링
- **아이콘 크기:** 20x20 픽셀 (`Rect(x+1, y+1, 20, 20)`)
- **전체 요소 크기:** `Text.CalcSize(ideo.name).x + 22f + 15f` (텍스트 너비 + 아이콘 너비 + 여백)

### 6.2 Role 아이콘

- **위치:** `DoTopStack()` 메서드 내에서 Ideo 아이콘 다음에 렌더링
- **아이콘 크기:** 20x20 픽셀 (`Rect(x+1, y+1, 20, 20)`)
- **전체 요소 크기:** `Text.CalcSize(roleLabel).x + 22f + 14f` (텍스트 너비 + 아이콘 너비 + 여백)

## 7. 조건부 렌더링

### 7.1 Ideo 아이콘 표시 조건

```csharp
if (!Find.IdeoManager.classicMode && 
    CS$<>8__locals1.pawn.Ideo != null && 
    ModsConfig.IdeologyActive)
{
    // Ideo 아이콘 렌더링
}
```

**조건:**
1. `Find.IdeoManager.classicMode`가 false (클래식 모드 아님)
2. `pawn.Ideo`가 null이 아님
3. `ModsConfig.IdeologyActive`가 true (Ideology DLC 활성화)

### 7.2 Role 아이콘 표시 조건

```csharp
if (ModsConfig.IdeologyActive)
{
    Ideo ideo = pawn.Ideo;
    Precept_Role role = (ideo != null) ? ideo.GetRole(pawn) : null;
    
    if (role != null)
    {
        // Role 아이콘 렌더링
    }
}
```

**조건:**
1. `ModsConfig.IdeologyActive`가 true
2. `pawn.Ideo`가 null이 아님
3. `ideo.GetRole(pawn)`이 null이 아님 (Pawn이 Role을 가지고 있음)

## 8. 주요 메서드 호출 체인

```
ITab_Pawn_Character.FillTab()
  └─ CharacterCardUtility.DrawCharacterCard()
       └─ CharacterCardUtility.DoTopStack()
            ├─ IdeoUIUtility.DrawIdeoPlate()
            │    └─ Ideo.DrawIcon()
            │         └─ GUI.DrawTexture(rect, Ideo.Icon)
            │              └─ ContentFinder<Texture2D>.Get(iconDef.iconPath)
            │
            └─ (Role이 있는 경우)
                 └─ GUI.DrawTexture(position, role.Icon)
                      └─ Precept.Icon (def.Icon ?? ideo.Icon)
```

## 9. 참고 사항

### 9.1 Meme 아이콘은 표시되지 않음

캐릭터 카드에서는 **Meme 아이콘을 개별적으로 표시하지 않습니다**. Ideo 아이콘은 Ideo의 전체적인 아이콘만 표시하며, Meme 아이콘은 Ideo 상세 정보 창에서만 볼 수 있습니다.

### 9.2 아이콘 캐싱

- `Ideo.Icon` 속성은 첫 접근 시 텍스처를 로드하고 `this.icon` 필드에 캐시합니다.
- 이후 접근 시에는 캐시된 텍스처를 재사용하여 성능을 최적화합니다.

### 9.3 색상 적용

- Ideo 아이콘과 Role 아이콘 모두 `GUI.color`를 Ideo의 색상으로 변경한 후 그립니다.
- 이는 아이콘 텍스처에 Ideo의 고유 색상을 적용하기 위함입니다.

## 10. 소스코드 참조

- **CharacterCardUtility.cs**: `RimworldSource/RimWorld/CharacterCardUtility.cs`
  - `DoTopStack()`: Line 736-1125
  - Ideo 렌더링: Line 935-949
  - Role 렌더링: Line 950-997

- **IdeoUIUtility.cs**: `RimworldSource/RimWorld/IdeoUIUtility.cs`
  - `DrawIdeoPlate()`: Line 2300-2326

- **Ideo.cs**: `RimworldSource/RimWorld/Ideo.cs`
  - `Icon` 속성: Line 131-142
  - `Color` 속성: Line 168-188
  - `DrawIcon()`: Line 1277-1283

- **Precept.cs**: `RimworldSource/RimWorld/Precept.cs`
  - `Icon` 속성: Line 147-153

- **Precept_Role.cs**: `RimworldSource/RimWorld/Precept_Role.cs`
  - `DrawIcon()`: Line 594-599

## 결론

캐릭터 카드에서 Ideo 정보를 표시하는 과정은 다음과 같이 요약됩니다:

1. **Pawn에서 Ideo 추출**: `pawn.Ideo`를 통해 Ideo 객체에 접근
2. **아이콘 렌더링**: `IdeoUIUtility.DrawIdeoPlate()`를 통해 Ideo 아이콘과 이름 표시
3. **Role 렌더링**: `ideo.GetRole(pawn)`을 통해 Role 정보를 가져와 Role 아이콘 표시
4. **텍스처 로드**: `ContentFinder<Texture2D>.Get()`를 통해 아이콘 텍스처 로드 및 캐싱
5. **색상 적용**: `GUI.color`를 Ideo 색상으로 변경하여 아이콘에 색상 적용

이 과정을 통해 캐릭터 카드에 Pawn의 Ideo와 Role 정보가 시각적으로 표시됩니다.

