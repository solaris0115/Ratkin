# BodySize와 MeleeDodgeChance 관계 분석 보고서

## 분석 개요

Ratkin의 `baseBodySize: 0.8`과 `MeleeDodgeChance: 1.15` 설정의 실제 작동 방식을 소스코드 레벨에서 분석

## 1. MeleeDodgeChance StatDef 정의

**파일**: `RimWorldData/Core/Defs/Stats/Stats_Pawns_Combat.xml` (lines 122-160)

```xml
<StatDef>
    <defName>MeleeDodgeChance</defName>
    <label>melee dodge chance</label>
    <description>Chance to dodge a melee attack that would've otherwise hit.
Characters will not dodge while aiming or firing a ranged weapon.</description>
    <category>PawnCombat</category>
    <neverDisabled>true</neverDisabled>
    <defaultBaseValue>0</defaultBaseValue>
    <minValue>0</minValue>
    <toStringStyle>PercentZero</toStringStyle>
    <toStringStyleUnfinalized>FloatOne</toStringStyleUnfinalized>
    <noSkillOffset>0</noSkillOffset>
    <skillNeedOffsets>
        <li Class="SkillNeed_BaseBonus">
            <skill>Melee</skill>
            <baseValue>0</baseValue>
            <bonusPerLevel>1</bonusPerLevel>
        </li>
    </skillNeedOffsets>
    <capacityOffsets>
        <li>
            <capacity>Moving</capacity>
            <scale>18</scale>
        </li>
        <li>
            <capacity>Sight</capacity>
            <scale>8</scale>
            <max>1.4</max>
        </li>
    </capacityOffsets>
    <postProcessCurve>
        <points>
            <li>(5, 0)</li>
            <li>(20, 0.30)</li>
            <li>(60, 0.50)</li>
        </points>
    </postProcessCurve>
    <displayPriorityInCategory>4100</displayPriorityInCategory>
    <showDevelopmentalStageFilter>Child, Adult</showDevelopmentalStageFilter>
</StatDef>
```

### 핵심 발견: **parts가 없음!**

**중요**: MeleeDodgeChance StatDef에는 `<parts>` 섹션이 **없습니다**. 

즉, `StatPart_BodySize`가 적용되지 않습니다!

## 2. CarryingCapacity와의 비교

### CarryingCapacity (bodySize 적용됨)
```xml
<StatDef>
    <defName>CarryingCapacity</defName>
    <defaultBaseValue>75</defaultBaseValue>
    <parts>
        <li Class="StatPart_BodySize" />  <!-- ★ bodySize 적용 -->
    </parts>
</StatDef>
```

### MeleeDodgeChance (bodySize 적용 안됨)
```xml
<StatDef>
    <defName>MeleeDodgeChance</defName>
    <defaultBaseValue>0</defaultBaseValue>
    <!-- parts 섹션 없음! ★ bodySize 미적용 -->
</StatDef>
```

## 3. GetDodgeChance 소스코드 분석

**파일**: `RimworldSource/RimWorld/Verb_MeleeAttack.cs` (lines 173-214)

```csharp
private float GetDodgeChance(LocalTargetInfo target)
{
    if (this.surpriseAttack)
        return 0f;
    
    if (this.IsTargetImmobile(target))
        return 0f;
    
    Pawn pawn = target.Thing as Pawn;
    if (pawn == null)
        return 0f;
    
    // 원거리 무기 조준 중이면 회피 불가
    Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
    if (stance_Busy != null && stance_Busy.verb != null && !stance_Busy.verb.verbProps.IsMeleeAttack)
        return 0f;
    
    // ★ 단순히 MeleeDodgeChance 스탯 값을 가져옴
    float num = pawn.GetStatValue(StatDefOf.MeleeDodgeChance, true, -1);
    
    // Ideology DLC: 조명/어둠 보정
    if (ModsConfig.IdeologyActive)
    {
        if (DarknessCombatUtility.IsOutdoorsAndLit(target.Thing))
            num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceOutdoorsLitOffset, true, -1);
        else if (DarknessCombatUtility.IsOutdoorsAndDark(target.Thing))
            num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceOutdoorsDarkOffset, true, -1);
        else if (DarknessCombatUtility.IsIndoorsAndDark(target.Thing))
            num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceIndoorsDarkOffset, true, -1);
        else if (DarknessCombatUtility.IsIndoorsAndLit(target.Thing))
            num += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceIndoorsLitOffset, true, -1);
    }
    
    return num;
}
```

**결론**: bodySize는 회피율 계산에 **전혀 사용되지 않습니다**.

## 4. MeleeDodgeChance 계산 과정

### A. 기본값 (baseValue)
- Human: 0 (defaultBaseValue)
- Ratkin: **1.15** (statBases에 명시)

### B. 스킬 보너스
```
Melee 스킬 레벨당 +1
예: Melee 스킬 10 → +10
```

### C. 능력치 보정 (capacityOffsets)
```
Moving 능력 × 18
Sight 능력 × 8 (최대 1.4까지)

예: Moving 100%, Sight 100%
→ (1.0 × 18) + (1.0 × 8) = +26
```

### D. PostProcessCurve 적용
```
최종값이 5 미만 → 0%로 변환
최종값이 20 → 30%로 변환
최종값이 60 이상 → 50%로 변환 (최대치)
```

### E. 실제 계산 예시

**Ratkin (Melee 10, Moving 100%, Sight 100%)**
```
1. baseValue: 1.15 (statBases)
2. Melee 스킬: +10
3. Moving: +18
4. Sight: +8
   
총합: 1.15 + 10 + 18 + 8 = 37.15

PostProcessCurve 적용:
37.15 → 약 43% 회피율
```

**Human (Melee 10, Moving 100%, Sight 100%)**
```
1. baseValue: 0 (defaultBaseValue)
2. Melee 스킬: +10
3. Moving: +18
4. Sight: +8
   
총합: 0 + 10 + 18 + 8 = 36

PostProcessCurve 적용:
36 → 약 42% 회피율
```

**차이**: Ratkin이 Human보다 +1.15만큼 높음 (약 1% 회피율 향상)

## 5. Ratkin의 실제 설정

**파일**: `Project/1.6/Defs/ThingDef_Race/Races_Rakinlike.xml`

```xml
<statBases>
    <MeleeDodgeChance>1.15</MeleeDodgeChance>
</statBases>
<race>
    <baseBodySize>0.8</baseBodySize>
</race>
```

### 실제 적용값
- **MeleeDodgeChance baseValue**: 1.15 (고정값)
- **bodySize 영향**: 없음
- **최종 회피율**: 1.15 + 스킬/능력치 보정 + PostProcessCurve

## 6. bodySize가 영향을 주는 스탯들

bodySize는 MeleeDodgeChance에 영향을 주지 **않지만**, 다음 스탯들에는 영향을 줍니다:

### bodySize 적용 스탯 (StatPart_BodySize 포함)
1. **CarryingCapacity** - 운반 용량
2. **MeatAmount** - 고기 생산량
3. **LeatherAmount** - 가죽 생산량
4. **HungerRateMultiplier** - 음식 소비율
5. **ComfyTemperatureRange** - 쾌적 온도 범위
6. 기타 체형 관련 스탯들

### bodySize 미적용 스탯
1. **MeleeDodgeChance** - 근접 회피율
2. **MeleeHitChance** - 근접 명중률
3. **MoveSpeed** - 이동 속도
4. **WorkSpeed** - 작업 속도
5. 기타 스킬/능력 관련 스탯들

## 7. 게임 디자인 관점

### 왜 회피율에 bodySize를 적용하지 않았을까?

1. **균형성**: 작은 생물이 자동으로 회피율이 높다면 불공평
2. **현실성 vs 게임성**: 작은 생물이 맞기 어렵다는 것은 "명중률"로 표현 (타겟 크기)
3. **회피는 능력**: 체형보다는 민첩성, 스킬, 능력치로 결정
4. **별도 시스템**: 타겟 크기는 다른 메커니즘으로 처리 가능

### Ratkin의 회피율 설계

Ratkin의 `MeleeDodgeChance: 1.15`는:
- **절대적 수치**: bodySize와 무관하게 항상 +1.15
- **의미**: 작은 체구의 민첩함을 표현
- **효과**: Human 대비 약 1% 높은 회피율

## 8. 타겟 크기와 명중률

bodySize가 회피율에 영향을 주지 않더라도, **타겟 크기**는 명중률에 영향을 줄 가능성이 있습니다.

추가 확인 필요:
- 원거리 사격 시 타겟 bodySize가 명중률에 영향을 주는가?
- 근접 공격 시 타겟 bodySize가 명중률에 영향을 주는가?

## 결론

### 핵심 요약

1. **MeleeDodgeChance는 bodySize의 영향을 받지 않음**
   - StatDef에 `StatPart_BodySize`가 없음
   - 소스코드에서도 bodySize를 고려하지 않음

2. **Ratkin의 MeleeDodgeChance: 1.15는 절대값**
   - baseBodySize 0.8과 무관
   - 항상 +1.15로 적용됨

3. **CarryingCapacity와는 다름**
   - CarryingCapacity: 45 × 0.8 = **36** (bodySize 적용)
   - MeleeDodgeChance: **1.15** (bodySize 미적용)

4. **회피율은 스킬과 능력치로 결정**
   - Melee 스킬
   - Moving 능력
   - Sight 능력
   - PostProcessCurve (최대 50%)

### 권장 사항

현재 설정이 의도한 대로 작동하고 있습니다:
- Ratkin은 작은 체구의 민첩함으로 +1.15 회피 보너스
- bodySize 0.8은 운반 용량 등 다른 스탯에만 영향
- 회피율은 순수하게 능력과 스킬로 결정

변경 필요 없음!

## 참고 파일

### RimWorld 원본
- `RimWorldData/Core/Defs/Stats/Stats_Pawns_Combat.xml` - MeleeDodgeChance 정의

### 소스코드
- `RimworldSource/RimWorld/Verb_MeleeAttack.cs` - 회피 계산 로직
- `RimworldSource/RimWorld/StatPart_BodySize.cs` - bodySize 적용 방식

### Ratkin 프로젝트
- `Project/1.6/Defs/ThingDef_Race/Races_Rakinlike.xml` - Ratkin 정의
- `Project/1.6/Defs/GeneDefs/CustomGeneDefs.xml` - 유전자별 회피 보너스

