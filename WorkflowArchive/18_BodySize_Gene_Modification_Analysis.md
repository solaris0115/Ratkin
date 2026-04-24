# BodySize Gene 수정 가능성 분석 - 2025-10-15

## 질문
Gene이나 다른 방법으로 BodySize를 조정할 수 있는가?

## 핵심 발견 사항

### ❌ GeneDef로는 bodySize를 **직접** 조정할 수 없습니다

## 1. BodySize 계산 방식

**파일**: `RimworldSource/Verse/Pawn.cs` (line 5408-5414)

```csharp
public float BodySize
{
    get
    {
        // ★ LifeStage의 bodySizeFactor × 종족의 baseBodySize
        return this.ageTracker.CurLifeStage.bodySizeFactor 
             * this.RaceProps.baseBodySize;
    }
}
```

**공식**:
```
Pawn.BodySize = LifeStage.bodySizeFactor × ThingDef.race.baseBodySize
```

## 2. 조정 가능한 요소들

### A. LifeStageDef (성장 단계별)

**파일**: `RimworldSource/RimWorld/LifeStageDef.cs` (line 70)

```csharp
public class LifeStageDef : Def
{
    public float bodySizeFactor = 1f;  // ★ 성장 단계별 체형 배율
    public float healthScaleFactor = 1f;
    public float hungerRateFactor = 1f;
    public float marketValueFactor = 1f;
    public float foodMaxFactor = 1f;
    // ... 기타 파라미터들
}
```

**예시**: `RimWorldData/Core/Defs/Misc/LifeStageDefs/LifeStages.xml`

```xml
<LifeStageDef>
    <defName>HumanlikeBaby</defName>
    <bodySizeFactor>0.2</bodySizeFactor>  <!-- 20% 크기 -->
    <healthScaleFactor>0.2</healthScaleFactor>
</LifeStageDef>

<LifeStageDef>
    <defName>HumanlikeChild</defName>
    <bodySizeFactor>0.35</bodySizeFactor>  <!-- 35% 크기 -->
</LifeStageDef>

<LifeStageDef>
    <defName>HumanlikeTeenager</defName>
    <bodySizeFactor>0.8</bodySizeFactor>  <!-- 80% 크기 -->
</LifeStageDef>

<LifeStageDef>
    <defName>HumanlikeAdult</defName>
    <bodySizeFactor>1.0</bodySizeFactor>  <!-- 100% 크기 -->
</LifeStageDef>
```

### B. ThingDef.race.baseBodySize (종족별)

**파일**: `RimWorldData/Core/Defs/ThingDefs_Races/Races_Humanlike.xml`

```xml
<ThingDef Name="Human">
    <race>
        <baseBodySize>1</baseBodySize>
    </race>
</ThingDef>
```

**Ratkin 예시**:
```xml
<ThingDef Name="RK_Race_Ratkin">
    <race>
        <baseBodySize>0.8</baseBodySize>
    </race>
</ThingDef>
```

## 3. GeneDef의 파라미터 확인

**파일**: `RimworldSource/Verse/GeneDef.cs`

```csharp
public class GeneDef : Def
{
    public List<StatModifier> statOffsets;      // ✅ 스탯 오프셋
    public List<StatModifier> statFactors;      // ✅ 스탯 배율
    public List<Aptitude> aptitudes;            // ✅ 적성
    public List<AbilityDef> abilities;          // ✅ 능력
    public List<PawnCapacityModifier> capMods;  // ✅ 능력치 수정
    public float painOffset;                    // ✅ 고통 오프셋
    public float painFactor = 1f;               // ✅ 고통 배율
    public float socialFightChanceFactor = 1f;  // ✅ 사회적 싸움 확률
    // ... 기타 많은 파라미터들
    
    // ❌ bodySizeFactor 없음!
    // ❌ bodySize 관련 파라미터 전혀 없음!
}
```

### GeneDef가 수정할 수 있는 것들

**스탯 수정**:
- `statOffsets`: 스탯에 값 더하기/빼기
- `statFactors`: 스탯에 배율 적용

**직접 제어 가능**:
- 특성 (forcedTraits/suppressedTraits)
- 능력 (abilities)
- 스킬 적성 (aptitudes)
- 신체 능력 (capMods)
- 고통, 중독, 면역 등 특수 효과
- 작업 제한 (disabledWorkTags)
- 외형 (bodyType, hairColor, skinColor 등)

**제어 불가**:
- ❌ bodySize
- ❌ LifeStage
- ❌ 종족의 baseBodySize

## 4. 우회 방법 없음

### A. StatModifier로 가능한가?

```xml
<GeneDef>
    <statFactors>
        <BodySize>1.2</BodySize>  <!-- ❌ BodySize는 스탯이 아님! -->
    </statFactors>
</GeneDef>
```

**불가능 이유**: BodySize는 StatDef가 **아닙니다**. Property일 뿐입니다.

### B. 간접적 영향은?

Gene이 bodySize에 **간접적으로** 영향을 줄 수 있는 방법:
1. ❌ LifeStage 변경 → GeneDef에 LifeStage 관련 파라미터 없음
2. ❌ baseBodySize 수정 → ThingDef는 Gene으로 수정 불가
3. ❌ 다른 어떤 방법도 없음

## 5. bodySize가 영향을 주는 스탯들

bodySize를 직접 수정할 수는 없지만, **bodySize가 영향을 주는 스탯**들을 Gene으로 조정할 수는 있습니다:

### bodySize 영향을 받는 스탯들

| 스탯 | bodySize 적용 | Gene 수정 가능 |
|------|--------------|---------------|
| CarryingCapacity | ✅ baseValue × bodySize | ✅ statOffsets/statFactors로 가능 |
| MeatAmount | ✅ baseValue × bodySize | ❌ (생산량은 Gene 수정 불가) |
| LeatherAmount | ✅ baseValue × bodySize | ❌ (생산량은 Gene 수정 불가) |
| MaxNutrition | ✅ baseValue × bodySize | ✅ statOffsets/statFactors로 가능 |

### Gene으로 CarryingCapacity 직접 수정

bodySize를 수정할 수는 없지만, **최종 스탯 값**을 직접 수정할 수 있습니다:

```xml
<GeneDef>
    <defName>RK_Gene_StrongCarrier</defName>
    <label>strong carrier</label>
    <statOffsets>
        <CarryingCapacity>20</CarryingCapacity>  <!-- +20 -->
    </statOffsets>
    <!-- 또는 -->
    <statFactors>
        <CarryingCapacity>1.5</CarryingCapacity>  <!-- ×150% -->
    </statFactors>
</GeneDef>
```

## 6. 실제 적용 계산

### Ratkin 예시

**현재 상황**:
```
baseBodySize: 0.8
LifeStage.bodySizeFactor: 1.0 (성인)
BodySize = 0.8 × 1.0 = 0.8

CarryingCapacity statBases: 45
CarryingCapacity 실제값 = 45 × 0.8 = 36
```

**Gene으로 보정 시도**:
```xml
<GeneDef>
    <defName>RK_Gene_EnhancedCarrier</defName>
    <statOffsets>
        <CarryingCapacity>14</CarryingCapacity>  <!-- 36 + 14 = 50 -->
    </statOffsets>
</GeneDef>
```

**결과**:
- BodySize: 여전히 0.8 (변경 불가)
- CarryingCapacity: 50 (직접 수정으로 보정 성공)

## 7. 다른 모드의 접근 방법

일부 모드들이 bodySize를 조정하는 방법:

### A. Custom RaceDef 생성
완전히 새로운 ThingDef 생성:
```xml
<ThingDef ParentName="RK_Race_Ratkin">
    <defName>RK_Race_Ratkin_Large</defName>
    <race>
        <baseBodySize>1.0</baseBodySize>  <!-- 새로운 종족 -->
    </race>
</ThingDef>
```

**단점**: 별도 종족이 되어 호환성 문제

### B. C# 패치 (Harmony)
```csharp
[HarmonyPatch(typeof(Pawn), "BodySize", MethodType.Getter)]
public static void Postfix(Pawn __instance, ref float __result)
{
    if (__instance.genes != null && 
        __instance.genes.HasGene(MyGeneDef.BodySizeModifier))
    {
        __result *= 1.25f;  // 125%로 증가
    }
}
```

**단점**: C# 코딩 필요, 복잡도 증가

### C. AlienRace 모드 확장
AlienRace 모드가 제공하는 추가 기능 사용

**단점**: AlienRace 의존성 필요

## 8. Ratkin 프로젝트 권장 사항

### 현재 상황
- baseBodySize: 0.8 (고정)
- CarryingCapacity: 36 (45 × 0.8)
- 원거리 명중률: -20% 이득
- 근접 회피율: +1.15 보너스

### 권장 옵션

#### 옵션 1: 현재 유지 (권장)
**장점**:
- 단순하고 안정적
- bodySize의 모든 혜택 자동 적용 (원거리 생존력 +20%)
- 일관성 있는 설계

**단점**:
- 운반력 부족 (36)

#### 옵션 2: Gene으로 CarryingCapacity 보정
```xml
<GeneDef>
    <defName>RK_Gene_CarryingEnhanced</defName>
    <label>enhanced carrying</label>
    <description>Ratkin's efficient muscle structure allows better weight distribution.</description>
    <statOffsets>
        <CarryingCapacity>19</CarryingCapacity>  <!-- 36 + 19 = 55 -->
    </statOffsets>
    <biostatMet>-1</biostatMet>
    <biostatCpx>1</biostatCpx>
</GeneDef>
```

**장점**:
- bodySize 0.8 유지 (원거리 생존력 유지)
- CarryingCapacity만 선택적 증가

**단점**:
- Gene 슬롯 1개 사용
- 설정 복잡도 증가

#### 옵션 3: baseBodySize 변경
```xml
<race>
    <baseBodySize>0.9</baseBodySize>  <!-- 0.8 → 0.9 -->
</race>
```

**결과**:
- CarryingCapacity: 45 × 0.9 = 40.5
- 원거리 명중률: -10% 이득 (20% → 10%)

**장점**:
- 간단한 조정
- 균형잡힌 절충안

**단점**:
- 원거리 생존력 감소

## 9. 결론

### 핵심 요약

1. **GeneDef로 bodySize 직접 수정 불가능**
   - GeneDef에 bodySize 관련 파라미터 없음
   - LifeStage나 baseBodySize 수정도 불가

2. **BodySize는 2가지 요소로만 결정**
   ```
   BodySize = LifeStage.bodySizeFactor × ThingDef.race.baseBodySize
   ```

3. **우회 방법**
   - ThingDef 수정 (baseBodySize 변경)
   - Gene으로 관련 스탯 직접 수정 (CarryingCapacity 등)
   - C# Harmony 패치 (고급)

4. **Ratkin 프로젝트 권장**
   - **Option 1**: 현재 bodySize 0.8 유지 (원거리 생존력 우선)
   - **Option 2**: Gene으로 CarryingCapacity만 보정
   - **Option 3**: baseBodySize 0.9로 절충

### 최종 답변

**Gene이나 간단한 방법으로 bodySize를 조정할 수 없습니다.**

bodySize를 바꾸려면:
1. ThingDef의 `race.baseBodySize` 직접 수정
2. 또는 C# 코드로 Pawn.BodySize 프로퍼티 패치

bodySize는 바꿀 수 없지만, bodySize의 **영향을 받는 개별 스탯**들은 Gene으로 조정 가능합니다.

## 참고 파일

### 소스코드
- `RimworldSource/Verse/Pawn.cs` - BodySize 프로퍼티 정의
- `RimworldSource/RimWorld/LifeStageDef.cs` - bodySizeFactor 정의
- `RimworldSource/Verse/GeneDef.cs` - Gene 파라미터 목록

### 설정 파일
- `RimWorldData/Core/Defs/Misc/LifeStageDefs/LifeStages.xml` - LifeStage 예시
- `Project/1.6/Defs/ThingDef_Race/Races_Rakinlike.xml` - Ratkin baseBodySize

### 관련 분석 문서
- `WorkFlow/16_CarryingCapacity_BodySize_Analysis.md` - bodySize와 CarryingCapacity 관계
- `WorkFlow/17_RangedAccuracy_BodySize_Analysis.md` - bodySize와 원거리 명중률 관계
- `10_BodySize_MeleeDodge_Analysis_Report.md` - bodySize와 회피율 관계 (없음)

