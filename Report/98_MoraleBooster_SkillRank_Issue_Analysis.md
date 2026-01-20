# 사기 진작 코드 Skill 랭크 이슈 분석 리포트

## 문제 상황
사기 진작 능력(`RK_Ability_MeleeMoraleBooster`) 사용 시 skill 랭크가 일정 수준 이하인 경우 효과를 받지 못하는 문제가 발생

## 코드 분석

### 1. 관련 파일 구조
- **AbilityDef**: `Project/1.6/Defs/AbilityDefs/AbilityDefs.xml` (라인 33-57)
- **시전자 Hediff**: `RK_Hediff_MoraleBoosterCaster` (라인 104-127)
- **버프 Hediff**: `RK_Hediff_MoraleBoosterBuffDefault` (라인 130-144)
- **컴포넌트**: `Project/1.6/Source/MoraleBooster/HediffComp_GiveHediffsInRange.cs`

### 2. HediffComp_GiveHediffsInRange.cs 분석

#### 타겟팅 로직 (라인 67-93)
```csharp
foreach (Pawn pawn in readOnlyList)
{
    if (pawn.RaceProps.Humanlike && !pawn.Dead && pawn.health != null && 
        pawn != this.parent.pawn && 
        pawn.Position.DistanceTo(this.parent.pawn.Position) <= this.Props.range && 
        this.Props.targetingParameters.CanTarget(pawn, null))
    {
        // Hediff 부여 로직
    }
}
```

#### 타겟팅 조건 체크 순서
1. `pawn.RaceProps.Humanlike` - 인간형 종족인지 확인
2. `!pawn.Dead` - 생존 여부 확인
3. `pawn.health != null` - 헬스 시스템 존재 확인
4. `pawn != this.parent.pawn` - 시전자 본인 제외
5. `pawn.Position.DistanceTo(...) <= this.Props.range` - 범위 내 확인
6. `this.Props.targetingParameters.CanTarget(pawn, null)` - 타겟팅 파라미터 확인

### 3. TargetingParameters.CanTarget 분석

`RimworldSource/RimWorld/TargetingParameters.cs`의 `CanTarget` 메서드를 확인한 결과:
- **Skill 랭크 체크 로직 없음**
- 타겟팅 파라미터는 다음 항목들만 체크:
  - `canTargetPawns`, `canTargetBuildings`, `canTargetAnimals`, `canTargetMechs` 등
  - `onlyTargetColonists`, `onlyTargetPrisoners` 등
  - `validator` (Predicate<TargetInfo>) - 커스텀 검증 로직

### 4. Def 파일 설정 확인

#### AbilityDefs.xml의 targetingParameters 설정 (라인 118-122)
```xml
<targetingParameters>
    <canTargetBuildings>false</canTargetBuildings>
    <canTargetAnimals>false</canTargetAnimals>
    <canTargetMechs>false</canTargetMechs>
</targetingParameters>
```

**문제점**: `canTargetHumans` 또는 `canTargetPawns` 설정이 없음

### 5. 원인 분석

#### 가능한 원인 1: canTargetPawns 기본값 문제
- `TargetingParameters`의 기본값은 `canTargetPawns = true`
- 하지만 Def에서 명시적으로 설정하지 않으면 예상치 못한 동작 가능

#### 가능한 원인 2: validator가 설정되어 있을 가능성
- `targetingParameters`에 `validator`가 설정되어 있을 경우 skill 랭크 체크 가능
- 현재 Def 파일에는 `validator` 설정이 보이지 않음

#### 가능한 원인 3: 부모 HediffDef의 컴포넌트
- `RK_Hediff_MoraleBoosterBuffDefault`의 부모: `CombatRoleAuraBuffHediffBase`
- 부모 HediffDef에 `HediffCompProperties_Link` 컴포넌트가 있지만 skill 랭크 체크 없음

### 6. 해결 방안

#### 방안 1: targetingParameters에 명시적 설정 추가
```xml
<targetingParameters>
    <canTargetPawns>true</canTargetPawns>
    <canTargetHumans>true</canTargetHumans>
    <canTargetBuildings>false</canTargetBuildings>
    <canTargetAnimals>false</canTargetAnimals>
    <canTargetMechs>false</canTargetMechs>
</targetingParameters>
```

#### 방안 2: 코드에서 skill 랭크 체크 제거 (만약 있다면)
- 현재 코드에는 skill 랭크 체크가 보이지 않음
- 다른 곳에서 체크하고 있을 가능성 확인 필요

#### 방안 3: 디버깅 로그 추가
- `HediffComp_GiveHediffsInRange.cs`에 로그 추가하여 어떤 조건에서 실패하는지 확인

## 권장 사항

1. **즉시 조치**: `targetingParameters`에 `canTargetPawns`와 `canTargetHumans` 명시적 설정 추가
2. **디버깅**: 문제 재현 시 로그 추가하여 정확한 원인 파악
3. **테스트**: 다양한 skill 랭크의 Pawn으로 테스트하여 문제 재현 여부 확인

## 추가 확인 사항

- 다른 모드나 Harmony 패치가 `TargetingParameters.CanTarget`을 수정하고 있는지 확인
- `CombatRoleAuraBuffHediffBase` 부모 HediffDef의 다른 컴포넌트 확인
- 게임 내 실제 동작 확인 (로그 확인)
