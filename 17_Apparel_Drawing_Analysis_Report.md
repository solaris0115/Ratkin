# Apparel 드로잉 시스템 분석 보고서

## 1. Apparel 드로잉 기본 구조

### 1.1 호출 흐름
```
PawnRenderUtility.DrawEquipmentAndApparelExtras()
  └─> foreach (Apparel apparel in pawn.apparel.WornApparel)
        └─> apparel.DrawWornExtras()
              └─> foreach (ThingComp comp in AllComps)
                    └─> comp.CompDrawWornExtras()
```

**핵심 포인트:**
- 모든 착용한 Apparel의 `DrawWornExtras()` 메서드가 자동으로 호출됨
- `PawnRenderUtility.cs`의 275-281번째 줄에서 처리

### 1.2 Apparel 클래스의 DrawWornExtras() 메서드
```csharp
// RimworldSource/RimWorld/Apparel.cs:261-268
public virtual void DrawWornExtras()
{
    List<ThingComp> allComps = base.AllComps;
    for (int i = 0; i < allComps.Count; i++)
    {
        allComps[i].CompDrawWornExtras();
    }
}
```

**특징:**
- `virtual` 메서드이므로 오버라이드 가능
- 기본적으로는 모든 Comp의 `CompDrawWornExtras()` 호출
- **커스텀 드로잉을 위해 이 메서드를 오버라이드하면 됨**

---

## 2. 프로젝트 내 구현 예제: Shield 클래스

### 2.1 Shield 클래스 구조 (`Project/1.6/Source/ShieldOfRatkinia/WoodenShield.cs`)

```csharp
public class Shield : Apparel
{
    // 소집 상태 확인 프로퍼티
    private bool ShouldShieldUp
    {
        get
        {
            Pawn wearer = Wearer;
            return wearer.Spawned && 
                (wearer.InAggroMentalState || 
                 wearer.Drafted || 
                 (wearer.CurJob != null && wearer.CurJob.def.alwaysShowWeapon) || 
                 (wearer.mindState.duty != null && wearer.mindState.duty.def.alwaysShowWeapon));
        }
    }

    // 드로잉 오버라이드
    public override void DrawWornExtras()
    {
        Pawn pawn = Wearer;
        Vector3 rootLoc = pawn.DrawPos;
        
        if (ShouldShieldUp)  // 소집 시 - 앞쪽에 그리기
        {
            switch(pawn.Rotation.AsInt)
            {
                case 0: // North
                    DrawShield(shieldGraphic.MatNorth, rootLoc + drawDraftedLocNorth, 0);
                    break;
                case 1: // East
                    DrawShield(shieldGraphic.MatEast, rootLoc + drawDraftedLocEast, 0);
                    break;
                case 2: // South
                    DrawShield(shieldGraphic.MatSouth, rootLoc + drawDraftedLocSouth, 0);
                    break;
                case 3: // West
                    DrawShield(shieldGraphic.MatWest, rootLoc + drawDraftedLocWest, 0);
                    break;
            }
        }
        else  // 평상시 - 등에 그리기
        {
            if (!pawn.Dead && pawn.GetPosture() == PawnPosture.Standing)
            {
                switch (pawn.Rotation.AsInt)
                {
                    case 0: // North - 남쪽 텍스처를 등에
                        DrawShield(shieldGraphic.MatSouth, rootLoc + drawBackLocNorth, 0);
                        break;
                    case 1: // East - 서쪽 텍스처를 등에
                        DrawShield(shieldGraphic.MatWest, rootLoc + drawBackLocEast, 15);
                        break;
                    case 2: // South - 북쪽 텍스처를 등에
                        DrawShield(shieldGraphic.MatNorth, rootLoc + drawBackLocSouth, 0);
                        break;
                    case 3: // West - 동쪽 텍스처를 등에
                        DrawShield(shieldGraphic.MatEast, rootLoc + draWBackLocWest, -15);
                        break;
                }
            }
        }
    }

    // 실제 메시 드로잉
    public void DrawShield(Material mat, Vector3 drawLoc, float angle)
    {
        Mesh mesh = MeshPool.plane10;
        Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(angle, Vector3.up), mat, 0);
    }
}
```

### 2.2 위치 오프셋 벡터들

**소집 시 위치 (앞쪽):**
```csharp
static readonly Vector3 drawDraftedLocNorth = new Vector3(-0.2f, -0.2f, -0.09f);
static readonly Vector3 drawDraftedLocSouth = new Vector3(0.2f, 0.2f, -0.15f);
static readonly Vector3 drawDraftedLocEast = new Vector3(0.2f, -0.2f, -0.2f);
static readonly Vector3 drawDraftedLocWest = new Vector3(-0.2f, 0.2f, -0.15f);
```

**평상시 위치 (등):**
```csharp
static readonly Vector3 drawBackLocNorth = new Vector3(0f, 0.2f, -0.2f);
static readonly Vector3 drawBackLocSouth = new Vector3(0f, -0.2f, -0.09f);
static readonly Vector3 drawBackLocEast = new Vector3(-0.15f, 0.05f, -0.07f);
static readonly Vector3 draWBackLocWest = new Vector3(0.15f, -2f, -0.07f);
```

**벡터 의미:**
- `x`: 좌우 (음수: 왼쪽, 양수: 오른쪽)
- `y`: 앞뒤 레이어링 (음수: 뒤, 양수: 앞) - **매우 중요!**
- `z`: 고도 (렌더링 순서)

---

## 3. RimWorld Belt 드로잉 비교

### 3.1 SmokepopBelt (`RimworldSource/RimWorld/SmokepopBelt.cs`)
```csharp
public class SmokepopBelt : Apparel
{
    // DrawWornExtras를 오버라이드하지 않음
    // 기본 Apparel의 텍스처 렌더링만 사용
}
```

**특징:**
- Belt는 별도의 `DrawWornExtras()` 오버라이드가 없음
- 일반 Apparel처럼 기본 텍스처 렌더링에 의존
- **조건부 렌더링은 Def의 `wornGraphicPath`와 `bodyPartGroups`로 제어**

### 3.2 CompShield 예제 (`RimworldSource/RimWorld/CompShield.cs`)
```csharp
public override void CompDrawWornExtras()
{
    base.CompDrawWornExtras();
    if (this.IsApparel)
    {
        this.Draw();
    }
}

private void Draw()
{
    if (this.ShieldState == ShieldState.Active && this.ShouldDisplay)
    {
        // 쉬ール드 버블 드로잉
        float num = Mathf.Lerp(this.Props.minDrawSize, this.Props.maxDrawSize, this.energy);
        Vector3 vector = this.PawnOwner.Drawer.DrawPos;
        vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
        // ... 메시 그리기
        Graphics.DrawMesh(MeshPool.plane10, matrix, CompShield.BubbleMat, 0);
    }
}
```

**특징:**
- Comp를 통한 드로잉 (Apparel 클래스가 아닌 Component 사용)
- 조건부 렌더링: `ShieldState == Active && ShouldDisplay`

---

## 4. 소집(Drafted) 상태 확인 방법

### 4.1 기본 방법
```csharp
Pawn wearer = Wearer;
if (wearer.Drafted)
{
    // 소집 상태
}
```

### 4.2 확장 방법 (Shield 예제에서 사용)
```csharp
private bool ShouldShieldUp
{
    get
    {
        Pawn wearer = Wearer;
        return wearer.Spawned && 
            (wearer.InAggroMentalState ||         // 공격적 정신 상태
             wearer.Drafted ||                     // 소집됨
             (wearer.CurJob != null && wearer.CurJob.def.alwaysShowWeapon) || // 항상 무기 표시 작업
             (wearer.mindState.duty != null && wearer.mindState.duty.def.alwaysShowWeapon)); // duty가 항상 무기 표시
    }
}
```

### 4.3 RimWorld CarryWeaponOpenly 참고 (`PawnRenderUtility.cs:289-305`)
```csharp
public static bool CarryWeaponOpenly(Pawn pawn)
{
    if (pawn.carryTracker?.CarriedThing != null)
        return false;
    
    if (pawn.Drafted)
        return true;
    
    if (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon)
        return true;
    
    if (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon)
        return true;
    
    return false;
}
```

---

## 5. Graphic 로딩 및 관리

### 5.1 Graphic 초기화 (Shield 예제)
```csharp
private Graphic shieldGraphic;

public override void PostMake()
{
    base.PostMake();
    if (shieldGraphic != null) { return; }

    Action finishAction = () =>
    {
        Color shieldColor = Color.white;
        if (def.defName != "RK_WoodenShield") 
        {
            shieldColor = Stuff.stuffProps.color; // 소재 색상 적용
        }
        
        shieldGraphic = GraphicDatabase.Get<Graphic_Multi>(
            path + def.defName, 
            ShaderDatabase.Cutout, 
            def.graphicData.drawSize, 
            shieldColor);
    };
    LongEventHandler.ExecuteWhenFinished(finishAction);
}
```

### 5.2 Save/Load 처리
```csharp
public override void ExposeData()
{
    base.ExposeData();
    if(Scribe.mode == LoadSaveMode.PostLoadInit)
    {
        LongEventHandler.ExecuteWhenFinished(delegate
        {
            if (shieldGraphic == null)
            {
                // Graphic 다시 로드
                shieldGraphic = GraphicDatabase.Get<Graphic_Multi>(
                    path + def.defName, 
                    ShaderDatabase.Cutout, 
                    def.graphicData.drawSize, 
                    stuffColor);
            }
        });
    }
}
```

### 5.3 Graphic_Multi 사용
```csharp
// 방향별 Material 접근
shieldGraphic.MatNorth  // 북쪽 (0도)
shieldGraphic.MatEast   // 동쪽 (90도)
shieldGraphic.MatSouth  // 남쪽 (180도)
shieldGraphic.MatWest   // 서쪽 (270도)
```

**텍스처 파일 네이밍:**
- `RK_ItemName_north.png`
- `RK_ItemName_east.png`
- `RK_ItemName_south.png`
- `RK_ItemName_west.png`

---

## 6. 구현 가이드: 배너 Apparel 예제

### 6.1 요구사항
- 소집 시: 팔에 배너 표시 (Belt처럼)
- 평상시: 등에 배너 표시

### 6.2 구현 방법

#### 방법 1: 직접 Apparel 상속
```csharp
namespace NewRatkin
{
    [StaticConstructorOnStartup]
    public class BannerApparel : Apparel
    {
        private Graphic bannerGraphic;
        
        // 위치 오프셋 정의
        static readonly Vector3 draftedLocNorth = new Vector3(-0.3f, 0.1f, -0.1f);
        static readonly Vector3 draftedLocSouth = new Vector3(0.3f, 0.1f, -0.1f);
        static readonly Vector3 draftedLocEast = new Vector3(0.2f, 0.1f, -0.15f);
        static readonly Vector3 draftedLocWest = new Vector3(-0.2f, 0.1f, -0.15f);
        
        static readonly Vector3 backLocNorth = new Vector3(0f, -0.2f, -0.1f);
        static readonly Vector3 backLocSouth = new Vector3(0f, -0.1f, -0.15f);
        static readonly Vector3 backLocEast = new Vector3(-0.1f, -0.15f, -0.1f);
        static readonly Vector3 backLocWest = new Vector3(0.1f, -0.15f, -0.1f);
        
        public override void PostMake()
        {
            base.PostMake();
            if (bannerGraphic != null) return;
            
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                bannerGraphic = GraphicDatabase.Get<Graphic_Multi>(
                    "Apparel/Util/" + def.defName,
                    ShaderDatabase.Cutout,
                    def.graphicData.drawSize,
                    DrawColor);
            });
        }
        
        public override void DrawWornExtras()
        {
            if (bannerGraphic == null) return;
            
            Pawn pawn = Wearer;
            if (pawn == null || !pawn.Spawned) return;
            
            Vector3 rootLoc = pawn.DrawPos;
            
            // 소집 상태 확인
            bool isDrafted = pawn.Drafted || 
                           (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon) ||
                           (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon);
            
            if (isDrafted)
            {
                // 소집 시 - 팔에 표시
                DrawBanner(rootLoc, true);
            }
            else if (!pawn.Dead && pawn.GetPosture() == PawnPosture.Standing)
            {
                // 평상시 - 등에 표시
                DrawBanner(rootLoc, false);
            }
        }
        
        private void DrawBanner(Vector3 rootLoc, bool drafted)
        {
            Pawn pawn = Wearer;
            Material mat;
            Vector3 offset;
            
            switch (pawn.Rotation.AsInt)
            {
                case 0: // North
                    mat = bannerGraphic.MatNorth;
                    offset = drafted ? draftedLocNorth : backLocNorth;
                    break;
                case 1: // East
                    mat = bannerGraphic.MatEast;
                    offset = drafted ? draftedLocEast : backLocEast;
                    break;
                case 2: // South
                    mat = bannerGraphic.MatSouth;
                    offset = drafted ? draftedLocSouth : backLocSouth;
                    break;
                case 3: // West
                    mat = bannerGraphic.MatWest;
                    offset = drafted ? draftedLocWest : backLocWest;
                    break;
                default:
                    return;
            }
            
            Graphics.DrawMesh(
                MeshPool.plane10, 
                rootLoc + offset, 
                Quaternion.identity, 
                mat, 
                0);
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                LongEventHandler.ExecuteWhenFinished(() =>
                {
                    if (bannerGraphic == null)
                    {
                        bannerGraphic = GraphicDatabase.Get<Graphic_Multi>(
                            "Apparel/Util/" + def.defName,
                            ShaderDatabase.Cutout,
                            def.graphicData.drawSize,
                            DrawColor);
                    }
                });
            }
        }
    }
}
```

#### 방법 2: ThingComp 사용
```csharp
public class CompBannerDrawer : ThingComp
{
    private Graphic bannerGraphic;
    
    public Apparel Apparel => parent as Apparel;
    public Pawn Wearer => Apparel?.Wearer;
    
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        LoadGraphic();
    }
    
    private void LoadGraphic()
    {
        if (bannerGraphic != null) return;
        
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            bannerGraphic = GraphicDatabase.Get<Graphic_Multi>(
                "Apparel/Util/" + parent.def.defName,
                ShaderDatabase.Cutout,
                parent.def.graphicData.drawSize,
                parent.DrawColor);
        });
    }
    
    public override void CompDrawWornExtras()
    {
        base.CompDrawWornExtras();
        
        if (bannerGraphic == null || Wearer == null || !Wearer.Spawned)
            return;
        
        bool isDrafted = Wearer.Drafted || 
                       (Wearer.CurJob != null && Wearer.CurJob.def.alwaysShowWeapon);
        
        Vector3 drawLoc = Wearer.DrawPos;
        
        if (isDrafted)
        {
            DrawBanner(drawLoc, true);
        }
        else if (!Wearer.Dead && Wearer.GetPosture() == PawnPosture.Standing)
        {
            DrawBanner(drawLoc, false);
        }
    }
    
    private void DrawBanner(Vector3 rootLoc, bool drafted)
    {
        // ... 위와 동일한 드로잉 로직
    }
}
```

### 6.3 Def 설정 (Comp 사용 시)
```xml
<ThingDef ParentName="ApparelUtilityBase">
    <defName>RK_Apparel_BannerArm</defName>
    <label>배너</label>
    <graphicData>
        <texPath>Apparel/Util/RK_TextureApparel_BannerArm</texPath>
        <graphicClass>Graphic_Multi</graphicClass>
        <drawSize>1.0</drawSize>
    </graphicData>
    <comps>
        <li Class="NewRatkin.CompProperties_BannerDrawer">
            <!-- 커스텀 속성 -->
        </li>
    </comps>
    <!-- ... 기타 설정 ... -->
</ThingDef>
```

---

## 7. 핵심 정리

### 7.1 Apparel 드로잉 구조
1. **자동 호출**: `PawnRenderUtility.DrawEquipmentAndApparelExtras()` → 모든 Apparel의 `DrawWornExtras()` 호출
2. **오버라이드 지점**: `DrawWornExtras()` 메서드
3. **조건부 렌더링**: `pawn.Drafted`, `pawn.CurJob.def.alwaysShowWeapon` 등으로 상태 확인
4. **위치 제어**: Vector3 오프셋으로 정확한 위치 지정 (x, y, z)

### 7.2 구현 선택 가이드

| 방식 | 장점 | 단점 | 사용 시기 |
|------|------|------|-----------|
| **Apparel 직접 상속** | - 간단하고 직관적<br>- 완전한 제어<br>- 빠른 실행 | - 재사용 어려움<br>- 다른 Comp와 충돌 가능 | - 단일 아이템 전용<br>- 복잡한 로직 필요 |
| **ThingComp 사용** | - 재사용 가능<br>- 모듈화<br>- 다른 Comp와 공존 | - 구조 복잡<br>- 설정 필요 | - 여러 아이템에 적용<br>- 확장 가능성 필요 |

### 7.3 주의사항
1. **Graphic 초기화**: `PostMake()` + `LongEventHandler.ExecuteWhenFinished()`
2. **Save/Load**: `ExposeData()`에서 Graphic 재로딩
3. **Null 체크**: `Wearer`, `bannerGraphic` 등 항상 확인
4. **Spawned 체크**: 드로잉 전 `pawn.Spawned` 확인
5. **y축 값**: 레이어링 순서 결정 (음수: 뒤, 양수: 앞)

---

## 8. 다음 단계

### 8.1 텍스처 준비
- `RK_TextureApparel_BannerArm_north.png`
- `RK_TextureApparel_BannerArm_east.png`
- `RK_TextureApparel_BannerArm_south.png`
- `RK_TextureApparel_BannerArm_west.png`

### 8.2 위치 오프셋 튜닝
- 게임 내에서 DevMode로 실시간 확인
- 각 방향별 최적 위치 조정
- 소집/평상시 구분하여 세밀 조정

### 8.3 테스트
1. 착용 시 정상 렌더링
2. 소집 시 위치 전환
3. 회전 시 방향별 텍스처
4. Save/Load 후 정상 작동

