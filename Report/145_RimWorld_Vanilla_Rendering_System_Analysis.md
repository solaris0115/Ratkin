# RimWorld Rendering System Terrain Pawn Apparel Depth Graphics Pipeline Analysis

> **태그**: RimWorld Rendering MapDrawer Section SectionLayer Terrain Pawn PawnRenderer PawnRenderTree Apparel Graphics Depth AltitudeLayer TextureAtlas MaterialPool GraphicDatabase DynamicDrawManager DrawerType

---

## 1. 아키텍처 개요

림월드는 Unity 엔진 위에서 동작하지만, **Unity의 GameObject/Renderer 시스템을 사용하지 않는다.** 대신 자체 렌더링 파이프라인을 구축했으며, 핵심 전략은 다음과 같다:

```
PNG 파일 → ContentFinder<Texture2D> → MaterialPool → Graphic 객체 → 메시(Mesh) + Material → Graphics.DrawMesh()
```

모든 렌더링은 최종적으로 Unity의 `Graphics.DrawMesh()` 또는 `Graphics.DrawMeshNow()` 호출로 귀결된다. Unity의 MeshRenderer, SpriteRenderer 등은 사용하지 않음.

### 렌더링 이원 구조: `DrawerType`

```csharp
public enum DrawerType : byte
{
    None,           // 렌더링 안 함
    RealtimeOnly,   // DynamicDrawManager에서만 (매 프레임 개별 Draw)
    MapMeshOnly,    // Section 메시에 베이크 (Print)
    MapMeshAndRealTime  // 양쪽 모두
}
```

| DrawerType | 대상 | 렌더링 방식 |
|---|---|---|
| `MapMeshOnly` | 벽, 바닥, 식물, 가구 등 정적 오브젝트 | Section 메시에 사전 베이크 → 한 번에 드로우콜 |
| `RealtimeOnly` | Pawn, 발사체 등 동적 오브젝트 | 매 프레임 개별 `Graphics.DrawMesh` 호출 |
| `MapMeshAndRealTime` | 일부 특수 오브젝트 | 양쪽 모두 등록 |

---

## 2. 지형/지물 렌더링: Section 기반 배칭 시스템

### 2.1 전체 흐름

```
MapDrawer
 ├── Section[,] sections   (17×17 셀 단위 그리드)
 │    └── List<SectionLayer> layers
 │         ├── SectionLayer_Terrain      → 지형 메시
 │         ├── SectionLayer_ThingsGeneral → 건물/식물 등 정적 Thing 메시
 │         ├── SectionLayer_SunShadows   → 그림자
 │         ├── SectionLayer_FogOfWar     → 전장의 안개
 │         └── ... (기타 레이어)
 └── List<MapDrawLayer> global  → 전역 레이어
```

### 2.2 Section 구조

맵은 **17×17 셀** 단위의 `Section`으로 분할된다.

```csharp
public class Section
{
    public IntVec3 botLeft;            // 섹션 좌하단 좌표
    public ulong dirtyFlags;           // 변경 플래그
    private readonly List<SectionLayer> layers;        // 정적 레이어
    private readonly List<SectionLayer_Dynamic> dynamic; // 동적 레이어
    public const int Size = 17;
}
```

- 250×250 맵 → 약 15×15 = 225개 섹션
- 각 섹션은 여러 `SectionLayer`를 보유
- `Section` 생성 시 `SectionLayer`의 모든 서브클래스를 리플렉션으로 인스턴스화

### 2.3 MapDrawLayer / SectionLayer 기반 클래스

```csharp
public abstract class MapDrawLayer
{
    public ulong relevantChangeTypes;          // 이 레이어가 반응하는 변경 타입 비트마스크
    public List<LayerSubMesh> subMeshes;       // Material별로 분리된 서브메시
    
    public abstract void Regenerate();         // 핵심: 메시 재생성
    
    public virtual void DrawLayer()            // 최종 렌더링
    {
        for (int i = 0; i < subMeshes.Count; i++)
        {
            LayerSubMesh sm = subMeshes[i];
            if (sm.finalized && !sm.disabled)
                Graphics.DrawMesh(sm.mesh, Matrix4x4.identity, sm.material, sm.renderLayer);
        }
    }
}
```

**핵심 포인트**: `DrawLayer()`는 서브메시 개수만큼만 `DrawMesh`를 호출한다. 같은 Material을 쓰는 셀은 하나의 서브메시로 병합되므로, **수백 개의 셀이 단 1~수 회의 드로우콜로 처리**된다.

### 2.4 SectionLayer_Terrain: 지형 렌더링

```csharp
public class SectionLayer_Terrain : SectionLayer
{
    public override void Regenerate()
    {
        ClearSubMeshes(MeshParts.All);
        
        foreach (IntVec3 cell in section.CellRect)
        {
            // 1) 기본 지형 쿼드 생성
            TerrainDef terrain = terrainGrid.TerrainAt(cell);
            Material mat = GetMaterialFor(cellTerrain);
            LayerSubMesh subMesh = GetSubMesh(mat);
            
            float y = AltitudeLayer.Terrain.AltitudeFor(); // Y=0.731... (고정 높이)
            
            // 1셀 = 1×1 쿼드 (4 vertices, 2 triangles)
            subMesh.verts.Add(new Vector3(x,   y, z));
            subMesh.verts.Add(new Vector3(x,   y, z+1));
            subMesh.verts.Add(new Vector3(x+1, y, z+1));
            subMesh.verts.Add(new Vector3(x+1, y, z));
            
            // 2) 인접 셀과의 경계 블렌딩 (부채꼴 8방향 메시)
            // renderPrecedence 비교로 어떤 지형이 위에 오는지 결정
            // 버텍스 컬러 알파로 경계를 부드럽게 처리
        }
        
        FinalizeMesh(MeshParts.All);
    }
}
```

**지형 렌더링 특징**:
- 각 셀 = 1×1 크기 쿼드, Y 좌표는 `AltitudeLayer.Terrain`으로 고정
- 같은 Material(지형 타입)의 셀은 하나의 `LayerSubMesh`로 병합
- 인접 지형 간 경계는 **버텍스 컬러 알파**를 이용한 블렌딩으로 부드럽게 처리
- `renderPrecedence`로 어떤 지형이 위에 오는지 결정

### 2.5 SectionLayer_Things: 정적 Thing 렌더링 (건물, 식물 등)

```csharp
public abstract class SectionLayer_Things : SectionLayer
{
    public override void Regenerate()
    {
        ClearSubMeshes(MeshParts.All);
        
        foreach (IntVec3 cell in section.CellRect)
        {
            List<Thing> things = Map.thingGrid.ThingsListAt(cell);
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                // DrawerType 필터링, 안개/눈 체크 등
                if (thing.def.drawerType != DrawerType.None 
                    && thing.def.drawerType != DrawerType.RealtimeOnly)
                {
                    TakePrintFrom(thing);  // → thing.Print(this)
                }
            }
        }
        FinalizeMesh(MeshParts.All);
    }
}
```

**Thing.Print() → Graphic.Print()**: Thing이 자신의 그래픽을 Section 메시에 "인쇄"

```csharp
// Graphic.Print() 핵심 로직
public virtual void Print(SectionLayer layer, Thing thing, float extraRotation)
{
    Vector3 center = thing.TrueCenter() + DrawOffset(thing.Rotation);
    Material mat = MatAt(thing.Rotation, thing);
    
    // 텍스처 아틀라스 치환 시도 (같은 Material 그룹을 아틀라스로 묶음)
    Graphic.TryGetTextureAtlasReplacementInfo(mat, group, flipUv, true, 
        out mat, out uvs, out vertexColor);
    
    // 평면 쿼드를 SectionLayer의 서브메시에 추가
    Printer_Plane.PrintPlane(layer, center, drawSize, mat, angle, flip, uvs, colors);
}
```

**Print 패턴의 핵심**: 
- 정적 오브젝트는 **메시에 직접 쿼드를 추가** (GPU 인스턴싱 대신 CPU 메시 병합)
- 같은 Material을 쓰는 오브젝트끼리 **하나의 메시로 배칭**
- 변경 시에만 해당 Section의 레이어가 `Dirty` → `Regenerate()` 호출

### 2.6 Dirty 플래그와 점진적 갱신

```csharp
// 셀 변경 시
MapDrawer.MapMeshDirty(IntVec3 loc, ulong dirtyFlags)
{
    SectionAt(loc).dirtyFlags |= dirtyFlags;   // 해당 섹션만 dirty
    globalDirtyFlags |= dirtyFlags;
    
    // 건물/안개/지붕 변경은 인접 8셀도 dirty
    if (Buildings | FogOfWar | Roofs)
        for each adjacent cell → SectionAt(adj).dirtyFlags |= dirtyFlags;
}

// 매 프레임 (MapMeshDrawerUpdate_First)
Section.TryUpdate(CellRect view)
{
    // 뷰포트 내 섹션만 우선 재생성
    if (bounds.Overlaps(view))
        layer.Regenerate();  // dirty 레이어만
    // 뷰포트 밖은 나중에
}
```

**최적화 포인트**:
- 프레임당 1개 섹션씩만 재생성 (급작스러운 프레임 드롭 방지)
- 뷰포트 내 섹션 우선
- `ulong` 비트마스크로 어떤 레이어가 변경되었는지 정밀 추적

---

## 3. Pawn(동적 개체) 렌더링

### 3.1 DynamicDrawManager: 동적 렌더링 진입점

정적 Section 메시와 별도로, 움직이는 오브젝트는 `DynamicDrawManager`가 관리한다.

```csharp
public sealed class DynamicDrawManager
{
    private readonly List<Thing> drawThings;    // 등록된 동적 Thing 목록
    
    public void DrawDynamicThings()
    {
        // 1단계: 컬링 (Burst 잡으로 병렬 처리)
        ComputeCulledThings(details);    // 뷰포트, 안개, 눈 덮임 체크
        
        // 2단계: 그래픽 초기화 (메인 스레드)
        for (visible things)
            thing.DynamicDrawPhase(DrawPhase.EnsureInitialized);
        
        // 3단계: 사전 렌더링 (병렬 잡)
        PreDrawVisibleThings(details);   // 매트릭스 계산 등
        
        // 4단계: 실제 드로우 (메인 스레드)
        for (visible things)
            thing.DynamicDrawPhase(DrawPhase.Draw);
        
        // 5단계: 실루엣 (선택된 Pawn 하이라이트)
        DrawSilhouettes(details);
    }
}
```

**컬링 최적화**:
- `CullJob` : **Burst 컴파일된 병렬 잡**으로 뷰포트/안개/눈 체크
- `NativeBitArray fogGrid`, `NativeArray<float> depthGrid` 사용
- Pawn은 `DrawPos`(보간된 위치), 일반 Thing은 `Position`(셀 좌표) 기준

### 3.2 PawnRenderer: Pawn 렌더링 핵심

```
DynamicDrawManager.DrawDynamicThings()
  → Thing.DynamicDrawPhase(Draw)
    → Pawn.DrawAt()
      → PawnRenderer.DynamicDrawPhaseAt(phase, drawLoc)
        → PawnRenderer.RenderPawnAt(drawLoc)
```

#### 3단계 파이프라인

```csharp
public class PawnRenderer
{
    public PawnRenderTree renderTree;    // 렌더 트리 (노드 기반 구조)
    
    // Phase 1: 그래픽 초기화
    EnsureGraphicsInitialized()
    {
        renderTree.EnsureInitialized(defaultRenderFlagsNow);
    }
    
    // Phase 2: 사전 계산 (병렬 가능)
    ParallelPreRenderPawnAt(drawLoc)
    {
        // 자세(Standing/Laying), 방향, 각도 계산
        PawnPosture posture = pawn.GetPosture();
        Vector3 bodyPos = GetBodyPos(drawLoc, posture, out showBody);
        float bodyAngle = (posture == Standing) ? 0 : BodyAngle();
        Rot4 bodyFacing = pawn.Rotation;
        
        // 캐시 사용 여부 판단
        // 조건: 인간형 + 줌 18 이상 + 포트레이트 아님 + 애니메이션 없음 등
        useCached = (zoom > 18f && humanlike && ...);
        
        if (!useCached)
            renderTree.ParallelPreDraw(parms);   // 매트릭스 사전 계산
    }
    
    // Phase 3: 실제 렌더링
    RenderPawnAt(drawLoc)
    {
        if (useCached)
        {
            // 텍스처 아틀라스 캐시에서 사전 렌더링된 이미지 사용
            PawnTextureAtlasFrameSet frameSet;
            GlobalTextureAtlasManager.TryGetPawnFrameSet(pawn, out frameSet);
            GenDraw.DrawMeshNowOrLater(frameSet.meshes[index], bodyPos, rotation, mat);
        }
        else
        {
            // 풀 렌더링
            renderTree.Draw(parms);
        }
        
        // 그림자 별도 처리
        DrawShadowInternal(drawLoc);
    }
}
```

### 3.3 PawnRenderTree: 노드 기반 렌더 트리

Pawn 렌더링은 **트리 구조**로 구성된다. 각 부위가 하나의 노드.

```
RootNode (PawnRenderNode)
 ├── Body
 │    ├── Apparel_Body (의상 몸통)
 │    ├── Equipment (무기)
 │    └── ...
 ├── Head
 │    ├── Hair
 │    ├── Beard
 │    ├── Eyes
 │    ├── Apparel_Head (모자/헬멧)
 │    └── ...
 └── Overlays (상태 오버레이)
```

```csharp
public class PawnRenderTree
{
    public PawnRenderNode rootNode;
    private readonly List<PawnGraphicDrawRequest> drawRequests;
    private readonly Dictionary<PawnRenderNode, List<PawnRenderNode>> nodeAncestors;
    
    // 사전 계산 (병렬 가능)
    public void ParallelPreDraw(PawnDrawParms parms)
    {
        // 1) 변경 여부 체크 (facing, posture 등이 바뀌었을 때만 재계산)
        if (oldParms.ShouldRecache(parms) || rootNode.RecacheRequested)
        {
            drawRequests.Clear();
            rootNode.AppendRequests(parms, drawRequests);  // 트리 순회하며 요청 수집
        }
        
        // 2) 각 노드의 변환 매트릭스 계산
        for (int i = 0; i < drawRequests.Count; i++)
        {
            TryGetMatrix(node, parms, out matrix);   // 부모→자식 매트릭스 누적
            request.preDrawnComputedMatrix = matrix;
        }
    }
    
    // 실제 드로우
    public void Draw(PawnDrawParms parms)
    {
        for (int i = 0; i < drawRequests.Count; i++)
        {
            Material material = drawRequests[i].material;
            GenDraw.DrawMeshNowOrLater(
                drawRequests[i].mesh,
                drawRequests[i].preDrawnComputedMatrix,
                material, drawNow, materialPropertyBlock);
        }
    }
}
```

**매트릭스 계산 (부모→자식 누적)**:
```csharp
bool TryGetMatrix(PawnRenderNode node, PawnDrawParms parms, out Matrix4x4 matrix)
{
    matrix = parms.matrix;   // 루트 TRS (위치 + 회전)
    
    List<PawnRenderNode> ancestors = nodeAncestors[node];
    for (int i = 0; i < ancestors.Count; i++)
    {
        ancestors[i].GetTransform(parms, out offset, out pivot, out rotation, out scale);
        // offset → pivot → rotation → scale → pivot inverse
        ComputeMatrix(ref matrix, offset, pivot, rotation, scale, canRotate);
    }
    
    // 최종 altitude 적용
    float alt = node.Worker.AltitudeFor(node, parms);
    matrix *= Matrix4x4.Translate(Vector3.up * alt);
}
```

### 3.4 DrawPos와 위치 보간

Pawn은 셀 단위로 이동하지만, 시각적으로는 **보간(interpolation)**을 적용한다:
- `Pawn.DrawPos` → 현재 틱에서의 보간된 위치
- `PawnRenderer.GetBodyPos()` → 자세에 따른 최종 위치 조정
  - 침대에 누운 경우: 침대 위치 + body offset
  - 캐리되는 경우: 캐리어의 위치
  - 일반: DrawPos 그대로

---

## 4. 의상 변경 시 갱신 흐름

### 4.1 전체 콜 체인

```
Pawn_ApparelTracker.Wear(newApparel)
  → wornApparel.TryAdd(newApparel)
    → ThingOwner.TryAdd → Notify_Added
      → Pawn_ApparelTracker.Notify_ApparelAdded(apparel)
        → SortWornApparelIntoDrawOrder()
        → Notify_ApparelChanged()
          → pawn.Drawer.renderer.SetAllGraphicsDirty()  ★핵심★
            → LongEventHandler.ExecuteWhenFinished(delegate {
                renderTree.SetDirty();                    // 트리 완전 무효화
                SilhouetteUtility.NotifyGraphicDirty();   // 실루엣 캐시
                WoundOverlays.ClearCache();               // 상처 오버레이
                PortraitsCache.SetDirty(pawn);             // 초상화
                GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty();  // 아틀라스
              });
```

### 4.2 SetDirty → 재구축

```csharp
public void SetDirty()   // PawnRenderTree
{
    nodeAncestors.Clear();    // 조상 캐시 제거
    drawRequests.Clear();     // 드로우 요청 제거
    rootNode = null;          // 루트 노드 파괴 (전체 트리 해제)
    nodesByTag.Clear();       // 태그 매핑 제거
    oldParms = default;       // 이전 파라미터 리셋
}
```

**SetDirty() 호출 후 다음 렌더링 시**:
1. `EnsureInitialized()` → `TrySetupGraphIfNeeded()` 호출
2. `rootNode`가 null이므로 **전체 렌더 트리를 재구축**
3. `PawnRenderNodeProperties`에서 트리 구조 로드
4. `SetupDynamicNodes()` → 의상, 유전자, 컴포넌트에서 동적 노드 생성
5. `InitializeAncestors()` → 조상 캐시 재구축
6. 다음 `ParallelPreDraw()`에서 모든 노드의 매트릭스 재계산

### 4.3 갱신되는 캐시 목록

| 캐시 | 무효화 메서드 | 용도 |
|---|---|---|
| PawnRenderTree | `SetDirty()` | 렌더 노드 트리 전체 |
| PawnTextureAtlasFrameSet | `TryMarkPawnFrameSetDirty()` | 줌 아웃 시 사용하는 사전 렌더 캐시 |
| PortraitsCache | `SetDirty(pawn)` | UI 초상화 |
| WoundOverlays | `ClearCache()` | 상처 오버레이 |
| SilhouetteGraphic | `NotifyGraphicDirty()` | 선택 시 실루엣 |

### 4.4 의상 렌더 노드 생성 과정

의상은 `DynamicPawnRenderNodeSetup` 시스템으로 동적 노드로 추가된다:

```csharp
// PawnRenderTree.SetupDynamicNodes()에서
foreach (DynamicPawnRenderNodeSetup setup in dynamicNodeTypeInstances)
{
    foreach ((PawnRenderNode child, PawnRenderNode parent) in setup.GetDynamicNodes(pawn, tree))
    {
        AddChild(child, parent);   // parentTagDef로 Body/Head에 붙음
    }
}
```

의상 노드는 `PawnRenderNodeWorker_Apparel_Body`, `PawnRenderNodeWorker_Apparel_Head` 등의 Worker를 통해 렌더링된다.

---

## 5. Depth/Altitude 시스템: 2D에서의 Z-깊이

### 5.1 핵심 메커니즘

림월드는 **Y축(Unity의 Up)을 depth로 사용**한다. 카메라가 위에서 비스듬히 내려다보는 구조이므로, Y값이 높을수록 화면상 "위에" 그려진다.

```csharp
public enum AltitudeLayer : byte   // 총 40개 레이어
{
    BelowTerrain = 0,    // Y ≈ 0.000
    TerrainEdges = 1,    // Y ≈ 0.366
    Terrain = 2,         // Y ≈ 0.732
    TerrainScatter = 3,
    Floor = 4,
    Conduits = 5,
    FloorCoverings = 6,
    FloorEmplacement = 7,
    Filth = 8,
    Zone = 9,
    SmallWire = 10,
    LowPlant = 11,
    MoteLow = 12,
    Shadows = 13,
    DoorMoveable = 14,
    Building = 15,       // Y ≈ 5.488
    BuildingBelowTop = 16,
    BuildingOnTop = 17,
    Item = 18,
    ItemImportant = 19,
    LayingPawn = 20,     // Y ≈ 7.317
    PawnRope = 21,
    Projectile = 22,
    Pawn = 23,           // Y ≈ 8.415
    PawnUnused = 24,
    PawnState = 25,
    Blueprint = 26,
    MoteOverheadLow = 27,
    MoteOverhead = 28,
    Gas = 29,
    Skyfaller = 30,
    Weather = 31,
    LightingOverlay = 32,
    VisEffects = 33,
    FogOfWar = 34,       // Y ≈ 12.439
    Darkness = 35,
    WorldClipper = 36,
    Silhouettes = 37,
    MapDataOverlay = 38,
    MetaOverlays = 39,   // Y ≈ 14.268
}
```

### 5.2 Y값 계산

```csharp
public static class Altitudes
{
    private const float LayerSpacing = 0.36585367f;   // 레이어 간 간격
    public const float AltInc = 0.03658537f;          // 같은 레이어 내 미세 오프셋
    
    // Alts[i] = i * 0.36585367
    public static float AltitudeFor(this AltitudeLayer alt)
        => Alts[(int)alt];
    
    // 같은 레이어 내 미세 오프셋
    public static float AltitudeFor(this AltitudeLayer alt, float incOffset)
        => alt.AltitudeFor() + incOffset * AltInc;
}
```

### 5.3 같은 레이어 내 깊이 충돌 해결

- `AltInc = 0.03658537f` 단위로 미세한 Y 오프셋을 부여
- `Graphic.Print()`에서 `Printer_Plane.PrintPlane()`에 `topVerticesAltitudeBias` 파라미터로 0.01f 추가
- 각 PawnRenderNode에서 `Worker.AltitudeFor(node, parms)`로 노드별 상대 오프셋 계산
  - 예: Body → 0, Head → +offset, Hair → +offset, Apparel_Shell(북쪽) → 88f...

### 5.4 렌더링 순서 정리

```
(아래 → 위, Y값 증가 순)

지형 (Y≈0.7)
  └─ 지형 경계 블렌딩
바닥재 (Y≈1.5)
필스 (Y≈2.9)
식물 (Y≈4.0)
그림자 (Y≈4.8)
건물 (Y≈5.5)
아이템 (Y≈6.6)
누운 Pawn (Y≈7.3)
서있는 Pawn (Y≈8.4)
발사체 (Y≈8.0)
모트/이펙트 (Y≈10.2)
안개/어둠 (Y≈12.4~12.8)
실루엣/메타 (Y≈13.5~14.3)
```

---

## 6. 그래픽 로딩 파이프라인: PNG → 화면

### 6.1 전체 파이프라인

```
[디스크]                 [메모리]                [GPU]
PNG 파일 ──────→ Texture2D ──────→ Material ──────→ Mesh + DrawMesh()
            ContentFinder     MaterialPool      Graphic / Print
            (캐시: ModContent) (캐시: Dict)     (캐시: GraphicDatabase)
```

### 6.2 ContentFinder: 텍스처 로딩

```csharp
public static class ContentFinder<T>
{
    public static T Get(string itemPath, bool reportFailure = true)
    {
        // 1) 모든 활성 모드를 역순으로 검색 (나중 로드된 모드 우선)
        for (int i = runningMods.Count - 1; i >= 0; i--)
            t = runningMods[i].GetContentHolder<T>().Get(itemPath);
            
        // 2) Unity Resources 폴더
        t = Resources.Load<Texture2D>(path);
        
        // 3) AssetBundle
        t = TryFindAssetInModBundles(itemPath);
    }
}
```

- **모드 오버라이드**: 나중에 로드된 모드의 같은 경로 텍스처가 우선
- 메인 스레드에서만 호출 가능

### 6.3 MaterialPool: Material 캐싱

```csharp
public static class MaterialPool
{
    private static Dictionary<MaterialRequest, Material> matDictionary;
    
    public static Material MatFrom(MaterialRequest req)
    {
        if (matDictionary.TryGetValue(req, out material))
            return material;   // 캐시 히트
        
        // 새 Material 생성
        material = MaterialAllocator.Create(req.shader);
        material.mainTexture = req.mainTex;
        material.color = req.color;
        if (req.maskTex != null)
        {
            material.SetTexture("_MaskTex", req.maskTex);
            material.SetColor("_ColorTwo", req.colorTwo);
        }
        matDictionary.Add(req, material);
        return material;
    }
}
```

- **MaterialRequest** = (Texture + Shader + Color + ColorTwo + MaskTex + RenderQueue) 조합이 키
- 같은 조합이면 같은 Material 인스턴스 재사용 → 드로우콜 배칭에 필수

### 6.4 GraphicDatabase: Graphic 캐싱

```csharp
public static class GraphicDatabase
{
    private static Dictionary<GraphicRequest, Graphic> allGraphics;
    
    private static T GetInner<T>(GraphicRequest req) where T : Graphic, new()
    {
        if (!allGraphics.TryGetValue(req, out graphic))
        {
            graphic = Activator.CreateInstance<T>();
            graphic.Init(req);              // 여기서 MaterialPool.MatFrom 호출
            allGraphics.Add(req, graphic);
        }
        return (T)graphic;
    }
}
```

- **GraphicRequest** = (Type + Path + Shader + DrawSize + Color + ColorTwo + ...) 조합이 키
- `Graphic.Init()`에서 텍스처 로딩 + Material 생성

### 6.5 Graphic 클래스 계층

```
Graphic (base)
 ├── Graphic_Single      → 단일 텍스처 (모든 방향 동일 Material)
 ├── Graphic_Multi       → 방향별 텍스처 (_north, _south, _east, _west)
 │                         Material[4] 배열. 없는 방향은 폴백/플립
 ├── Graphic_Random      → 랜덤 텍스처 선택
 ├── Graphic_Linked      → 인접 셀 연결 (벽, 배관 등)
 ├── Graphic_Appearances → 외관별 텍스처
 ├── Graphic_Shadow      → 그림자 전용
 └── Graphic_Mote        → 파티클/이펙트
```

#### Graphic_Multi 텍스처 로딩:
```csharp
// Init()에서
textures[0] = ContentFinder<Texture2D>.Get(path + "_north", false);
textures[1] = ContentFinder<Texture2D>.Get(path + "_east", false);
textures[2] = ContentFinder<Texture2D>.Get(path + "_south", false);
textures[3] = ContentFinder<Texture2D>.Get(path + "_west", false);

// 폴백: north 없으면 south, south 없으면 east...
// west 없으면 east를 flip해서 사용 (westFlipped = true)
```

### 6.6 셰이더 종류

| 셰이더 | 용도 |
|---|---|
| `Cutout` | 기본. 알파 테스트로 투명 영역 컷아웃 |
| `CutoutComplex` | 마스크 텍스처로 2색 칠하기 (의상 등) |
| `CutoutPlant` | 식물. 바람에 흔들림 |
| `Transparent` | 반투명 |
| `TransparentPlant` | 반투명 식물 |
| `MetaOverlay` | 존/오버레이 |

### 6.7 텍스처 아틀라스 시스템

#### Static Atlas (정적 오브젝트용):
```
로딩 시 → Graphic.TryInsertIntoAtlas(group)
  → 개별 텍스처를 그룹별 아틀라스로 병합
  → Print() 시 아틀라스 내 UV 좌표로 치환
  → 같은 아틀라스의 오브젝트 = 같은 Material = 1 드로우콜
```

제한: 512×512 이상 텍스처는 아틀라스에 포함 불가

#### Pawn Atlas (동적 Pawn용):
```
줌 아웃(18 이상) 시 → PawnTextureAtlasFrameSet 사용
  → 별도 카메라(PawnCacheCamera)로 Pawn을 RenderTexture에 사전 렌더링
  → 방향(4방향) × 모드(Body+Head/HeadOnly) = 8개 프레임 캐시
  → isDirty[index]가 true일 때만 재렌더링
  → 줌 아웃 시 사전 렌더링된 1쿼드만 표시 (드로우콜 1)
```

---

## 7. 렌더링 흐름 종합 다이어그램

```
                    ┌─────────────────────────────────┐
                    │          Game Loop               │
                    │   MapDrawer.MapMeshDrawerUpdate  │
                    │   MapDrawer.DrawMapMesh          │
                    │   DynamicDrawManager.DrawDynamic │
                    └─────┬───────────────┬───────────┘
                          │               │
           ┌──────────────▼─────┐  ┌──────▼──────────────┐
           │  Section 기반      │  │  Dynamic 기반        │
           │  (정적 렌더링)     │  │  (동적 렌더링)       │
           ├────────────────────┤  ├──────────────────────┤
           │ SectionLayer_Terrain│  │ Pawn                 │
           │  → 셀별 쿼드 병합  │  │  → PawnRenderer      │
           │                    │  │    → PawnRenderTree   │
           │ SectionLayer_Things│  │      → DrawRequests   │
           │  → Thing.Print()   │  │                       │
           │  → Graphic.Print() │  │ Projectile            │
           │  → Printer_Plane   │  │  → Graphic.Draw()     │
           ├────────────────────┤  ├──────────────────────┤
           │ 변경 시에만 재생성 │  │ 매 프레임 개별 Draw  │
           │ 드로우콜 수: ~수십 │  │ 드로우콜 수: N개체   │
           └────────┬───────────┘  └──────┬───────────────┘
                    │                      │
                    ▼                      ▼
           Graphics.DrawMesh(mesh, identity, mat, layer)
                                   │
                    ┌──────────────▼──────────────┐
                    │         Unity GPU            │
                    │  Z-sort by Y coordinate      │
                    │  (AltitudeLayer → Y값)       │
                    └─────────────────────────────┘
```

---

## 8. 보충: Pawn 위치 보간 상세

### 8.1 DrawPos 계산 체인

```
Pawn.DrawPos
  → Pawn_DrawTracker.DrawPos
    = tweener.TweenedPos       // 이동 보간
    + jitterer.CurrentOffset   // 피격 진동
    + leaner.LeanOffset        // 기울기
    + OffsetForcedByJob()      // 작업 강제 오프셋
    + FlyingOffset()           // 비행 오프셋
    .WithY(pawn.def.Altitude + FlightYOffset + SeededYOffset)
```

### 8.2 PawnTweener: 이동 보간

- 매 프레임 `PreDrawPosCalculation()`에서 현재→목표 위치를 보간
- 보간 비율: `0.09f * (deltaTime * 60 * TickRateMultiplier)`
- 목표 위치: 경로의 `MovePercentage`로 현재 셀↔다음 셀 선형 보간 + 충돌 오프셋
- 틱 배율 5배 이상이면 즉시 스냅

### 8.3 의상 노드 생성 상세

`DynamicPawnRenderNodeSetup_Apparel`이 `WornApparel`을 순회하여:
- `LastLayer`가 `Overhead`/`EyeCover` → Head 노드의 자식
- 그 외 → Body 노드의 자식
- `Shell` 레이어는 북향 시 오프셋 88로 coat가 머리 뒤로 가도록 처리
- `ApparelGraphicRecordGetter.TryGetGraphicApparel`로 `Graphic_Multi` 생성 (bodyType 접미사 포함)

---

## 9. 핵심 인사이트 정리

### Q: 맵 렌더 루프 호출 순서는?
```csharp
// Map.MapUpdate() 내부
mapDrawer.MapMeshDrawerUpdate_First();  // 1) dirty 섹션 메시 재생성
mapDrawer.DrawMapMesh();                // 2) 섹션 정적 메시 드로우
dynamicDrawManager.DrawDynamicThings(); // 3) Pawn/동적 Thing 드로우
```

### Q: 왜 Unity GameObject를 안 쓰는가?
- 250×250 맵 = 62,500셀 + 수천 개 오브젝트
- GameObject마다 MeshRenderer 붙이면 드로우콜 폭발
- Section 배칭으로 **수만 개 오브젝트를 수십 개 드로우콜**로 압축

### Q: 2D인데 어떻게 depth가 있는가?
- Unity 3D 공간에서 **XZ 평면** = 맵 바닥, **Y축** = depth
- 카메라가 70도 각도로 내려다봄 (pseudo-2D 탑뷰)
- 40개 `AltitudeLayer`로 Y값 분배 → 자동 Z-소팅
- 같은 레이어 내 미세 차이는 `AltInc` (0.0366)으로 해결

### Q: PNG를 어떻게 로드하고 그리는가?
```
PNG → ContentFinder → Texture2D (Unity 내장)
Texture2D → MaterialPool → Material (Shader + Texture + Color)
Material → Graphic (Multi/Single) → Material[4] (방향별)
Graphic → Print() (정적) 또는 Draw() (동적) → Graphics.DrawMesh()
```

### Q: 의상 변경 시 무엇이 갱신되는가?
1. `PawnRenderTree.SetDirty()` → 전체 렌더 노드 트리 파괴 후 재구축
2. `GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty()` → 8프레임 캐시 무효화
3. `PortraitsCache.SetDirty()` → UI 초상화 재렌더
4. `WoundOverlays.ClearCache()` → 상처 오버레이 재계산
5. 다음 프레임에서 `EnsureInitialized()` → `TrySetupGraphIfNeeded()` → 새 트리 구축
