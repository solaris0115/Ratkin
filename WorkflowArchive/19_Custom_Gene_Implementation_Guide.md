# 커스텀 Gene 구현 가이드 - 2025-10-15

## 질문
새로운 Gene을 구현하는 방법이 있는가?

## 답변
**✅ 가능합니다!** C# 프로젝트를 추가하여 커스텀 Gene 클래스를 만들 수 있습니다.

## 1. 기본 개념

### Gene 시스템 구조
```
GeneDef (XML) → Gene Class (C#)
```

- **GeneDef**: XML로 정의하는 Gene의 데이터 (스탯, 설명, 아이콘 등)
- **Gene Class**: C# 코드로 구현하는 Gene의 로직 (동작, 효과 등)

### Gene 클래스 계층 구조
```
Gene (기본 클래스)
├─ Gene_Resource (리소스 관리)
│  └─ Gene_Hemogen (혈액 관리)
├─ Gene_Deathrest (죽음의 휴식)
├─ Gene_PollutionRush (오염 자극)
└─ 커스텀 Gene (직접 구현)
```

## 2. Gene 클래스 구조

**파일**: `RimworldSource/Verse/Gene.cs`

```csharp
namespace Verse
{
    public class Gene : IExposable, ILoadReferenceable
    {
        // 기본 속성
        public GeneDef def;
        public Pawn pawn;
        public Gene overriddenByGene;
        
        // 가상 속성 (오버라이드 가능)
        public virtual string Label { get; }
        public virtual bool Active { get; }
        
        // 라이프사이클 메서드 (오버라이드 가능)
        public virtual void PostMake() { }      // Gene 생성 시
        public virtual void PostAdd() { }       // Pawn에 추가 시
        public virtual void PostRemove() { }    // Pawn에서 제거 시
        public virtual void Tick() { }          // 매 틱마다
        public virtual void TickInterval(int delta) { }  // 일정 간격
        
        // 기타 메서드들
        public virtual void Notify_IngestedThing(Thing thing, int numTaken) { }
        public virtual void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit) { }
        // ... 더 많은 메서드들
    }
}
```

## 3. 커스텀 Gene 구현 단계

### 단계 1: C# 프로젝트 생성

**Project/1.6/Source/** 디렉토리에 C# 프로젝트를 만듭니다.

**RatkinGenes.csproj** 예시:
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProjectGuid>{YOUR-GUID-HERE}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>RatkinGenes</RootNamespace>
    <AssemblyName>RatkinGenes</AssemblyName>
    <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
  </PropertyGroup>
  
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    <DebugSymbols>false</DebugSymbols>
    <DebugType>none</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>..\..\Assemblies\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
  </PropertyGroup>
  
  <ItemGroup>
    <Reference Include="Assembly-CSharp">
      <HintPath>..\..\..\..\RimWorldWin64_Data\Managed\Assembly-CSharp.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>..\..\..\..\RimWorldWin64_Data\Managed\UnityEngine.CoreModule.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="System" />
    <Reference Include="System.Core" />
  </ItemGroup>
  
  <ItemGroup>
    <Compile Include="Gene_BodySizeModifier.cs" />
    <Compile Include="Gene_EnhancedCarrier.cs" />
  </ItemGroup>
  
  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
</Project>
```

### 단계 2: 간단한 Gene 클래스 예시

**Gene_EnhancedCarrier.cs**:
```csharp
using RimWorld;
using Verse;

namespace RatkinGenes
{
    /// <summary>
    /// 향상된 운반 능력 Gene
    /// </summary>
    public class Gene_EnhancedCarrier : Gene
    {
        // Gene이 활성화될 때 호출
        public override void PostAdd()
        {
            base.PostAdd();
            
            // 로그 출력 (디버깅용)
            if (Prefs.DevMode)
            {
                Log.Message($"[RatkinGenes] EnhancedCarrier gene added to {pawn.Name}");
            }
        }
        
        // Gene이 제거될 때 호출
        public override void PostRemove()
        {
            base.PostRemove();
            
            if (Prefs.DevMode)
            {
                Log.Message($"[RatkinGenes] EnhancedCarrier gene removed from {pawn.Name}");
            }
        }
    }
}
```

**대응하는 GeneDef (XML)**:
```xml
<GeneDef>
    <defName>RK_Gene_EnhancedCarrier</defName>
    <label>enhanced carrier</label>
    <description>Ratkin's efficient muscle structure allows better weight distribution, enhancing carrying capacity without bodySize penalties.</description>
    <geneClass>RatkinGenes.Gene_EnhancedCarrier</geneClass>
    <iconPath>UI/Icons/Genes/Gene_EnhancedCarrier</iconPath>
    <displayCategory>Miscellaneous</displayCategory>
    <displayOrderInCategory>100</displayOrderInCategory>
    <statOffsets>
        <CarryingCapacity>19</CarryingCapacity>
    </statOffsets>
    <biostatMet>-1</biostatMet>
    <biostatCpx>1</biostatCpx>
</GeneDef>
```

### 단계 3: 고급 Gene 예시 - BodySize 수정

**Gene_BodySizeModifier.cs**:
```csharp
using RimWorld;
using Verse;
using HarmonyLib;

namespace RatkinGenes
{
    /// <summary>
    /// BodySize를 수정하는 Gene (Harmony 패치 사용)
    /// </summary>
    public class Gene_BodySizeModifier : Gene
    {
        // Gene 설정값
        public float bodySizeMultiplier = 1.25f;
        
        public override void PostAdd()
        {
            base.PostAdd();
            
            // Pawn의 그래픽 갱신 (체형이 바뀌므로)
            if (pawn.Spawned)
            {
                pawn.Drawer.renderer.SetAllGraphicsDirty();
            }
        }
        
        public override void PostRemove()
        {
            base.PostRemove();
            
            // 원래 크기로 복귀
            if (pawn.Spawned)
            {
                pawn.Drawer.renderer.SetAllGraphicsDirty();
            }
        }
    }
    
    // Harmony 패치로 BodySize 수정
    [HarmonyPatch(typeof(Pawn), "BodySize", MethodType.Getter)]
    public static class Pawn_BodySize_Patch
    {
        public static void Postfix(Pawn __instance, ref float __result)
        {
            if (__instance?.genes == null) return;
            
            // Gene_BodySizeModifier를 가진 Gene 찾기
            foreach (var gene in __instance.genes.GenesListForReading)
            {
                if (gene is Gene_BodySizeModifier modifier && gene.Active)
                {
                    __result *= modifier.bodySizeMultiplier;
                    break;
                }
            }
        }
    }
}
```

**Harmony 초기화 (필수)**:
```csharp
using Verse;
using HarmonyLib;

namespace RatkinGenes
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("com.ratkin.genes");
            harmony.PatchAll();
            
            Log.Message("[RatkinGenes] Harmony patches applied successfully!");
        }
    }
}
```

**대응 GeneDef**:
```xml
<GeneDef>
    <defName>RK_Gene_BodySizeEnhanced</defName>
    <label>enhanced body size</label>
    <description>This gene modifies body structure, increasing effective body size by 25%.</description>
    <geneClass>RatkinGenes.Gene_BodySizeModifier</geneClass>
    <iconPath>UI/Icons/Genes/Gene_BodySize</iconPath>
    <displayCategory>Miscellaneous</displayCategory>
    <biostatCpx>2</biostatCpx>
    <biostatMet>-2</biostatMet>
</GeneDef>
```

### 단계 4: 리소스 관리 Gene 예시

**Gene_RatkinStamina.cs**:
```csharp
using RimWorld;
using Verse;
using UnityEngine;

namespace RatkinGenes
{
    /// <summary>
    /// 스태미나 리소스를 관리하는 Gene
    /// </summary>
    public class Gene_RatkinStamina : Gene_Resource
    {
        public override float InitialResourceMax => 1f;
        public override float MinLevelForAlert => 0.2f;
        public override float MaxLevelOffset => 0f;
        
        protected override Color BarColor => new Color(0.9f, 0.85f, 0.2f);
        protected override Color BarHighlightColor => new Color(1f, 0.95f, 0.5f);
        
        public override void Tick()
        {
            base.Tick();
            
            if (!pawn.IsHashIntervalTick(250)) return;
            
            // 이동 중이면 스태미나 소모
            if (pawn.pather?.Moving == true)
            {
                Value -= 0.001f;
            }
            // 휴식 중이면 스태미나 회복
            else if (pawn.jobs?.curJob?.def == JobDefOf.LayDown)
            {
                Value += 0.002f;
            }
        }
    }
}
```

**대응 GeneDef**:
```xml
<GeneDef>
    <defName>RK_Gene_Stamina</defName>
    <label>ratkin stamina</label>
    <description>Ratkins have a special stamina system...</description>
    <geneClass>RatkinGenes.Gene_RatkinStamina</geneClass>
    <resourceGizmoType>GeneGizmo_Resource</resourceGizmoType>
    <resourceLabel>stamina</resourceLabel>
    <iconPath>UI/Icons/Genes/Gene_Stamina</iconPath>
    <displayCategory>Miscellaneous</displayCategory>
</GeneDef>
```

## 4. 프로젝트 구조

```
Project/
├── 1.6/
│   ├── Assemblies/
│   │   └── RatkinGenes.dll  (빌드 결과물)
│   ├── Defs/
│   │   └── GeneDefs/
│   │       └── RatkinCustomGenes.xml
│   ├── Source/
│   │   ├── RatkinGenes.csproj
│   │   ├── Gene_EnhancedCarrier.cs
│   │   ├── Gene_BodySizeModifier.cs
│   │   ├── Gene_RatkinStamina.cs
│   │   └── HarmonyPatches.cs
│   └── About/
│       └── About.xml
```

## 5. 필수 DLL 참조

C# 프로젝트에 필요한 DLL들:

```xml
<ItemGroup>
  <!-- RimWorld 핵심 -->
  <Reference Include="Assembly-CSharp">
    <HintPath>..\..\..\..\RimWorldWin64_Data\Managed\Assembly-CSharp.dll</HintPath>
    <Private>False</Private>
  </Reference>
  
  <!-- Unity 엔진 -->
  <Reference Include="UnityEngine.CoreModule">
    <HintPath>..\..\..\..\RimWorldWin64_Data\Managed\UnityEngine.CoreModule.dll</HintPath>
    <Private>False</Private>
  </Reference>
  
  <!-- Harmony (패치용) -->
  <Reference Include="0Harmony">
    <HintPath>..\..\..\..\RimWorldWin64_Data\Managed\0Harmony.dll</HintPath>
    <Private>False</Private>
  </Reference>
</ItemGroup>
```

## 6. 실전 예시들

### 예시 1: 주기적 효과 Gene
```csharp
public class Gene_PeriodicHealing : Gene
{
    private const int CheckInterval = 2500;  // 매 2500 틱마다
    
    public override void Tick()
    {
        base.Tick();
        
        if (!pawn.IsHashIntervalTick(CheckInterval)) return;
        if (!Active) return;
        
        // 작은 체력 회복
        if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.WoundInfection) == null)
        {
            HealthUtility.AdjustSeverity(pawn, HediffDefOf.Food_Carbs, 0.05f);
        }
    }
}
```

### 예시 2: 조건부 효과 Gene
```csharp
public class Gene_NightVision : Gene
{
    public override void PostAdd()
    {
        base.PostAdd();
        RecacheGlowGrid();
    }
    
    public override void PostRemove()
    {
        base.PostRemove();
        RecacheGlowGrid();
    }
    
    private void RecacheGlowGrid()
    {
        if (pawn.Map != null)
        {
            pawn.Map.mapDrawer.MapMeshDirty(
                pawn.Position, 
                MapMeshFlagDefOf.Things
            );
        }
    }
}
```

### 예시 3: 스탯 동적 변경 Gene
```csharp
public class Gene_AdaptiveMetabolism : Gene
{
    public override void TickInterval(int delta)
    {
        base.TickInterval(delta);
        
        if (!Active) return;
        if (!pawn.IsHashIntervalTick(1000, delta)) return;
        
        // 음식 소비율을 온도에 따라 조절
        float temp = pawn.AmbientTemperature;
        if (temp < 0f)
        {
            // 추우면 대사 증가 (더 많은 음식 필요)
            // 이건 Hediff로 처리해야 함
        }
    }
}
```

## 7. 디버깅 팁

### 로그 출력
```csharp
if (Prefs.DevMode)
{
    Log.Message($"[RatkinGenes] {pawn.Name}: Value = {Value}");
    Log.Warning($"[RatkinGenes] Warning message");
    Log.Error($"[RatkinGenes] Error message");
}
```

### 개발자 모드 기즈모
```csharp
public override IEnumerable<Gizmo> GetGizmos()
{
    foreach (var gizmo in base.GetGizmos())
    {
        yield return gizmo;
    }
    
    if (DebugSettings.ShowDevGizmos)
    {
        yield return new Command_Action
        {
            defaultLabel = "DEV: Reset Resource",
            action = () => Value = InitialResourceMax
        };
    }
}
```

## 8. 주의사항

### 1. 네임스페이스
- 다른 모드와 충돌하지 않도록 고유한 네임스페이스 사용
- 예: `RatkinGenes`, `Ratkin.Genes` 등

### 2. Harmony 패치
- 패치 ID는 고유해야 함: `"com.yourname.modname"`
- 과도한 패치는 성능 저하 유발
- 필요한 경우에만 사용

### 3. 저장/로드
- 상태를 저장해야 하는 경우 `ExposeData()` 구현:
```csharp
public override void ExposeData()
{
    base.ExposeData();
    Scribe_Values.Look(ref customValue, "customValue", 0f);
}
```

### 4. 성능
- `Tick()`은 매 프레임 호출 → 가벼운 작업만
- `TickInterval()`이나 `IsHashIntervalTick()` 사용 권장
- 무거운 계산은 캐싱

### 5. 호환성
- RimWorld 버전 확인: `ModsConfig.BiotechActive`
- 다른 Gene과의 충돌 방지: `exclusionTags` 사용

## 9. Ratkin 프로젝트 적용 방안

### 추천 Gene 구현 목록

#### 1. Gene_EnhancedCarrier (간단)
- statOffsets만으로 충분하므로 **XML만으로 가능**
- C# 불필요

#### 2. Gene_BodySizeModifier (고급)
- bodySize 수정이 필요하면 **C# + Harmony 필수**
- 복잡도 높음

#### 3. Gene_RatkinNightVision (중급)
- 야간 시야 향상
- ignoreDarkness로 **XML만으로 가능**

#### 4. Gene_QuickReflexes (중급)
- 근접 회피 보너스
- statOffsets로 **XML만으로 가능**

### 권장 사항

**현재 단계에서는 C# 프로젝트 불필요**
- 대부분의 Gene 효과는 GeneDef의 XML 파라미터로 구현 가능
- `statOffsets`, `statFactors`, `abilities`, `traits` 등 활용

**C# 프로젝트가 필요한 경우**:
- ✅ bodySize 같은 프로퍼티를 Gene으로 수정
- ✅ 복잡한 조건부 로직
- ✅ 커스텀 리소스 시스템
- ✅ 특수한 AI 동작
- ✅ 고급 UI/Gizmo

## 10. 결론

### 핵심 요약

1. **Gene 구현은 가능합니다**
   - C# 프로젝트 생성
   - Gene 클래스 상속
   - GeneDef에서 geneClass 지정

2. **간단한 Gene은 XML만으로 충분**
   - statOffsets/statFactors
   - abilities
   - traits
   - capacityModifiers

3. **고급 Gene은 C# 필요**
   - 커스텀 로직
   - Harmony 패치
   - 리소스 관리
   - 특수 효과

4. **Ratkin 프로젝트**
   - 현재는 XML GeneDef만으로 충분
   - bodySize 수정 필요 시 C# 프로젝트 고려
   - 단계적 접근 권장

## 참고 자료

### 소스코드
- `RimworldSource/Verse/Gene.cs` - Gene 기본 클래스
- `RimworldSource/RimWorld/Gene_Hemogen.cs` - 리소스 Gene 예시
- `RimworldSource/Verse/Gene_PollutionRush.cs` - 간단한 Gene 예시

### XML 정의
- `RimWorldData/Biotech/Defs/GeneDefs/` - 바닐라 GeneDef 예시들

### 다음 단계
1. XML GeneDef로 시작
2. 필요시 C# 프로젝트 추가
3. 점진적 기능 확장

