# WyvernFire Burner 오리지널 코드 비교 분석 보고서

## 개요

이 보고서는 커스텀 `CompAbilityEffect_WyvernFireBurner`와 RimWorld 오리지널 `CompAbilityEffect_Burner`의 차이점을 분석하고, 오프셋 문제를 해결한 내용을 정리합니다.

## 오리지널 vs 커스텀 코드 비교

### 1. 콘 각도 계산 방식

**오리지널 (CompAbilityEffect_Burner)**
```csharp
float angle = Rand.Range(-this.Props.coneSizeDegrees, this.Props.coneSizeDegrees);
```
- 고정된 `coneSizeDegrees` 값을 사용
- `-coneSizeDegrees ~ +coneSizeDegrees` 범위에서 균등 분포 랜덤

**기존 커스텀 (수정 전)**
```csharp
float dynamicConeHalfAngle = this.CalculateConeHalfAngle(target);
float randomAngle = Rand.Range(-dynamicConeHalfAngle, dynamicConeHalfAngle);
```
- 거리 기반 동적 각도 계산
- `lineWidthEnd`, `minConeAngleDegrees` 속성 사용

**문제점:**
- 오리지널과 다른 각도 계산 방식으로 인해 스프레이 패턴이 의도와 다르게 나타남

### 2. 속성 구조

**오리지널 (CompProperties_AbilityBurner)**
```csharp
public float coneSizeDegrees;     // 콘 각도
```

**기존 커스텀 (수정 전)**
```csharp
public float lineWidthEnd;        // 불필요한 속성
public float minConeAngleDegrees; // 불필요한 속성
```

**문제점:**
- 오리지널에 없는 속성 추가
- `coneSizeDegrees` 누락

### 3. Effecter 처리

**오리지널**
```csharp
if (Vector3.Dot((intVec2.ToVector3() - drawPos).normalized, vector) > 0.5f)
{
    // spray.Add(...)
    map.effecterMaintainer.AddEffecterToMaintain(..., 100);
}
```
- if 블록 안에서 spray 추가와 effecter 추가를 함께 처리
- 100 틱 유지

**기존 커스텀 (수정 전)**
```csharp
spray.Add(...);

if (props.effecterDef != null)
{
    map.effecterMaintainer.AddEffecterToMaintain(..., 50ㅍㅍㅍㅍ...);
}
```
- 별도 if 블록으로 분리
- 깨진 문자로 인한 컴파일 에러
- 50 틱으로 다른 값 사용

**문제점:**
- effecter 유지 시간이 달라 이펙트 지속 시간 차이
- 깨진 문자로 인한 빌드 오류

### 4. 오프셋 계산

**오리지널**
```csharp
worldSource = drawPos + vector * this.Props.barrelOffsetDistance
```

**기존 커스텀**
```csharp
worldSource = origin + streamDir * props.barrelOffsetDistance
```

**분석:**
- 변수명만 다를 뿐 동일한 계산 방식
- `drawPos` = `origin`, `vector` = `streamDir`
- 오프셋 문제는 **각도 계산 방식**의 차이에서 발생

## 수정 내역

### 1. CompProperties_AbilityWyvernFireBurner.cs

**변경사항:**
- ❌ 제거: `lineWidthEnd`, `minConeAngleDegrees`
- ✅ 추가: `coneSizeDegrees`

```csharp
public class CompProperties_AbilityWyvernFireBurner : CompProperties_AbilityEffect
{
    public ThingDef moteDef;
    public int numStreams;
    public float coneSizeDegrees;  // 추가
    public float range;
    public float rangeNoise;
    public float barrelOffsetDistance;
    public int lifespanNoise;
    public float sizeReductionDistanceThreshold;
    public EffecterDef effecterDef;
}
```

### 2. CompAbilityEffect_WyvernFireBurner.cs

**주요 변경사항:**
1. 속성 접근자를 `BurnerProps` → `Props`로 변경 (new 키워드 사용)
2. `SpawnWyvernFireSpray` 메서드를 inline delegate로 변경
3. `CalculateConeHalfAngle()` 메서드 제거
4. 고정 `coneSizeDegrees` 범위로 랜덤 각도 생성
5. effecter 추가를 if 블록 안으로 이동
6. effecter 유지 시간 100 틱으로 수정
7. 깨진 문자 제거

**코드 구조 (오리지널과 동일):**
```csharp
public override IEnumerable<PreCastAction> GetPreCastActions()
{
    yield return new PreCastAction
    {
        action = delegate(LocalTargetInfo a, LocalTargetInfo _)
        {
            // 초기화
            Vector3 drawPos = this.parent.pawn.DrawPos;
            IntVec3 intVec = drawPos.Yto0().ToIntVec3();
            Map map = this.parent.pawn.Map;
            IncineratorSpray incineratorSpray = GenSpawn.Spawn(...);
            
            // 스트림 생성
            for (int i = 0; i < numStreams; i++)
            {
                float angle = Rand.Range(-this.Props.coneSizeDegrees, this.Props.coneSizeDegrees);
                Vector3 vector = normalized.RotatedBy(angle);
                
                // ... 계산 ...
                
                if (Vector3.Dot(...) > 0.5f)
                {
                    // spray 추가
                    incineratorSpray.Add(...);
                    
                    // effecter 추가 (같은 블록 내)
                    map.effecterMaintainer.AddEffecterToMaintain(..., 100);
                }
            }
        },
        ticksAwayFromCast = 5
    };
}
```

### 3. Weapon_HighTech.xml

**변경사항:**
```xml
<li Class="NewRatkin.CompProperties_AbilityWyvernFireBurner">
    <numStreams>6</numStreams>
    <coneSizeDegrees>15</coneSizeDegrees>  <!-- 추가 -->
    <range>5</range>
    <moteDef>Mote_IncineratorBurst</moteDef>
    <barrelOffsetDistance>-1.5</barrelOffsetDistance>
    <sizeReductionDistanceThreshold>8</sizeReductionDistanceThreshold>
    <lifespanNoise>40</lifespanNoise>
    <rangeNoise>.4</rangeNoise>
    <effecterDef>RK_Effecter_BurnerUsed</effecterDef>
    <!-- lineWidthEnd, minConeAngleDegrees 제거 -->
</li>
```

## 문제 원인 분석

### 오프셋 불일치의 실제 원인

초기 의심: "오프셋 계산이 다르다"
- 실제로는 `worldSource` 계산 방식은 동일

**실제 원인:**
1. **동적 각도 계산**: `CalculateConeHalfAngle()` 메서드가 거리에 따라 각도를 변화시킴
   - 가까울 때: `minConeAngleDegrees` (5도)
   - 멀 때: `lineWidthEnd`에 기반한 각도 계산
   - 이로 인해 스프레이 패턴이 거리에 따라 변하여 의도하지 않은 방향으로 발사됨

2. **Effecter 지속 시간 차이**: 50 vs 100 틱
   - 시각적 피드백 차이

3. **코드 구조 차이**: 별도 if 블록
   - 로직 흐름의 미묘한 차이

## 결과

### 수정 후 개선사항

1. ✅ **일관된 스프레이 패턴**: 거리와 관계없이 일정한 각도로 발사
2. ✅ **올바른 오프셋**: 오리지널과 동일한 계산 방식 적용
3. ✅ **컴파일 오류 해결**: 깨진 문자 제거
4. ✅ **코드 가독성 향상**: 오리지널과 동일한 구조로 유지보수 용이
5. ✅ **effecter 지속 시간 통일**: 100 틱으로 표준화

### 유지된 커스터마이징

오리지널에 없지만 유용한 기능은 유지:
- `moteDef` 커스터마이징: `props.moteDef ?? ThingDefOf.Mote_IncineratorBurst`

## 결론

"오프셋 문제"의 실제 원인은 **동적 각도 계산 방식**이었습니다. 오리지널은 고정된 `coneSizeDegrees` 값을 사용하지만, 커스텀 버전은 거리 기반 동적 계산을 시도했습니다. 이로 인해:

1. 스프레이 패턴이 예상과 다르게 나타남
2. 발사 방향이 의도와 다르게 보임 (오프셋처럼 보임)
3. 거리에 따른 각도 변화로 일관성 없는 동작

수정 후 오리지널과 완전히 동일한 동작을 하도록 통일되었으며, 빌드도 성공적으로 완료되었습니다.

## 관련 파일

- `Project/1.6/Source/WyvernFire/CompProperties_AbilityWyvernFireBurner.cs`
- `Project/1.6/Source/WyvernFire/CompAbilityEffect_WyvernFireBurner.cs`
- `Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml`
- `RimworldSource/RimWorld/CompAbilityEffect_Burner.cs` (참조)
- `RimworldSource/RimWorld/CompProperties_AbilityBurner.cs` (참조)

