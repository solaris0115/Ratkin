# CarryingCapacity와 BodySize 계산 메커니즘 분석 - 2025-10-15

## 작업 개요
- 요청 내용: CarryingCapacity와 bodySize가 림월드 소스코드에서 어떻게 계산되는지 상세 확인
- 목표: Ratkin의 CarryingCapacity 45와 bodySize 0.8이 실제로 어떻게 적용되는지 증명

## 작업 세부 진행
1. Human의 CarryingCapacity와 bodySize 확인 [v]
2. CarryingCapacity StatDef 정의 확인 [v]
3. StatPart_BodySize 소스코드 분석 [v]
4. GetBaseValueFor 메커니즘 확인 [v]
5. Ratkin의 실제 적용값 계산 [v]
6. 분석 보고서 작성 [v]

## 진행 상황

### 1. Human의 기본 값 확인
**파일**: `RimWorldData/Core/Defs/ThingDefs_Races/Races_Humanlike.xml`

```xml
<race>
    <baseBodySize>1</baseBodySize>
</race>
```

- Human의 `baseBodySize`: **1**
- Human의 `statBases`에는 `CarryingCapacity`가 **명시되지 않음**
- 따라서 defaultBaseValue 사용

### 2. CarryingCapacity StatDef 정의
**파일**: `RimWorldData/Core/Defs/Stats/Stats_Pawns_General.xml` (lines 301-321)

```xml
<StatDef>
    <defName>CarryingCapacity</defName>
    <label>carrying capacity</label>
    <description>The amount of stuff this creature can carry...</description>
    <category>BasicsPawn</category>
    <showOnEntities>false</showOnEntities>
    <showOnDrones>false</showOnDrones>
    <defaultBaseValue>75</defaultBaseValue>
    <minValue>1</minValue>
    <toStringStyle>Integer</toStringStyle>
    <parts>
        <li Class="StatPart_BodySize" />
    </parts>
    <capacityFactors>
        <li>
            <capacity>Manipulation</capacity>
            <weight>1.0</weight>
        </li>
    </capacityFactors>
    <displayPriorityInCategory>2205</displayPriorityInCategory>
</StatDef>
```

**핵심 요소**:
- `defaultBaseValue`: **75**
- `parts`: **StatPart_BodySize** 포함
- `capacityFactors`: Manipulation 능력에 따른 추가 보정 (1.0 가중치)

### 3. StatPart_BodySize 소스코드 분석
**파일**: `RimworldSource/RimWorld/StatPart_BodySize.cs`

```csharp
public class StatPart_BodySize : StatPart
{
    public override void TransformValue(StatRequest req, ref float val)
    {
        float num;
        if (this.TryGetBodySize(req, out num))
        {
            val *= num;  // ★★★ bodySize를 곱함 ★★★
        }
    }

    public override string ExplanationPart(StatRequest req)
    {
        float f;
        if (this.TryGetBodySize(req, out f))
        {
            return "StatsReport_BodySize".Translate(f.ToString("F2")) + ": x" + f.ToStringPercent();
        }
        return null;
    }

    private bool TryGetBodySize(StatRequest req, out float bodySize)
    {
        return PawnOrCorpseStatUtility.TryGetPawnOrCorpseStat(
            req, 
            (Pawn x) => x.BodySize, 
            (ThingDef x) => x.race.baseBodySize, 
            out bodySize
        );
    }
}
```

**결론**: `TransformValue`에서 **val *= bodySize**를 수행하여 baseValue에 bodySize를 곱합니다.

### 4. GetBaseValueFor 메커니즘
**파일**: `RimworldSource/RimWorld/StatWorker.cs` (lines 1367-1382)

```csharp
public virtual float GetBaseValueFor(StatRequest request)
{
    float result = this.stat.defaultBaseValue;  // 1. defaultBaseValue로 시작
    if (request.StatBases != null)
    {
        for (int i = 0; i < request.StatBases.Count; i++)
        {
            if (request.StatBases[i].stat == this.stat)
            {
                result = request.StatBases[i].value;  // 2. statBases가 있으면 덮어씀
                break;
            }
        }
    }
    return result;
}
```

**처리 순서**:
1. `defaultBaseValue` (75)로 시작
2. ThingDef의 `statBases`에 해당 stat이 명시되어 있으면 그 값으로 **덮어씀**
3. 이 baseValue에 StatPart들 (StatPart_BodySize 등)이 적용됨
4. 추가로 capacityFactors (Manipulation 등) 적용

### 5. Ratkin의 실제 값
**파일**: `Project/1.6/Defs/ThingDef_Race/Races_Rakinlike.xml`

```xml
<statBases>
    <CarryingCapacity>45</CarryingCapacity>  <!-- 명시적 설정 -->
</statBases>
<race>
    <baseBodySize>0.8</baseBodySize>
</race>
```

**계산 과정**:
```
1. GetBaseValueFor 호출
   → statBases에 CarryingCapacity: 45가 있음
   → baseValue = 45 (defaultBaseValue 75를 덮어씀)

2. StatPart_BodySize.TransformValue 적용
   → val = 45 * 0.8 = 36

3. capacityFactors (Manipulation) 적용
   → 정상 Manipulation (100%)이면 * 1.0
   → 최종값 = 36
```

**최종 결과**: Ratkin의 실제 CarryingCapacity는 **36**

### 6. Human과의 비교

| 종족 | baseBodySize | statBases 명시값 | 실제 baseValue | bodySize 적용 후 | 최종 CarryingCapacity |
|------|--------------|------------------|----------------|------------------|----------------------|
| Human | 1.0 | 없음 (defaultBaseValue 사용) | 75 | 75 * 1.0 = 75 | **75** |
| Ratkin | 0.8 | 45 | 45 | 45 * 0.8 = 36 | **36** |

## 핵심 결론

### 1. statBases 명시값의 효과
- `statBases`에 명시적으로 설정된 값은 `defaultBaseValue`를 **완전히 덮어씁니다**
- Ratkin의 경우 CarryingCapacity를 45로 명시했으므로, defaultBaseValue 75는 무시됩니다

### 2. bodySize의 적용 방식
- `StatPart_BodySize`는 **baseValue에 bodySize를 곱합니다** (`val *= bodySize`)
- 이는 statBases에 명시된 값이든 defaultBaseValue든 관계없이 **항상 적용**됩니다

### 3. Ratkin CarryingCapacity의 실제값
- Ratkin의 CarryingCapacity는 명시값 45가 아닌 **36**입니다
- 계산: 45 (명시값) × 0.8 (bodySize) = **36**

### 4. 설계 의도 분석
현재 설정이 의도한 것이라면:
- Ratkin이 bodySize 보정 없이 45를 가지려면 → statBases에 **56.25**로 설정해야 함
- 56.25 × 0.8 = 45

현재 설정 (45)이 의도라면:
- 실제 운반 능력은 36으로, Human(75)의 **48%**입니다
- bodySize 0.8을 고려하면 상대적으로 더 작은 운반 능력

## 권장 사항

### 옵션 1: bodySize 보정 없이 45를 원하는 경우
```xml
<statBases>
    <CarryingCapacity>56.25</CarryingCapacity>  <!-- 56.25 * 0.8 = 45 -->
</statBases>
```

### 옵션 2: 현재 설정 (36) 유지
```xml
<statBases>
    <CarryingCapacity>45</CarryingCapacity>  <!-- 45 * 0.8 = 36 -->
</statBases>
```
- Human 대비 48%의 운반 능력
- 작은 체구에 어울리는 설정

### 옵션 3: bodySize 비례로 설정
```xml
<statBases>
    <!-- CarryingCapacity 삭제 → defaultBaseValue 사용 -->
</statBases>
```
- 75 × 0.8 = 60
- Human 대비 80%의 운반 능력
- bodySize에 완전히 비례

## 참고 파일 목록

### RimWorld 원본 파일
- `RimWorldData/Core/Defs/ThingDefs_Races/Races_Humanlike.xml` - Human 정의
- `RimWorldData/Core/Defs/Stats/Stats_Pawns_General.xml` - CarryingCapacity StatDef

### 소스코드 파일
- `RimworldSource/RimWorld/StatPart_BodySize.cs` - bodySize 적용 로직
- `RimworldSource/RimWorld/StatWorker.cs` - 기본값 계산 로직 (GetBaseValueFor)
- `RimworldSource/Verse/Pawn_CarryTracker.cs` - 운반 트래커 (CarryingCapacity 사용)

### Ratkin 프로젝트 파일
- `Project/1.6/Defs/ThingDef_Race/Races_Rakinlike.xml` - Ratkin 정의

## 최종 작업 결과
✅ **완료**: CarryingCapacity와 bodySize의 계산 메커니즘을 소스코드 레벨에서 완전히 증명
- Ratkin의 실제 CarryingCapacity는 **36** (45 × 0.8)
- statBases 명시값도 bodySize 보정을 **반드시** 받음
- 설계 의도에 따라 값 조정 가능

