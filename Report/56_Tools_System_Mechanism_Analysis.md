# Tools 시스템 작동 메커니즘 분석 보고서

**작성일**: 2024-12-30  
**분석 목적**: RimWorld 코어에서 Tools 시스템이 어떻게 작동하는지 구조 분석

## 핵심 구조

### 1. Tool → ManeuverDef → Verb 생성 과정

#### 1.1 Tool 정의
```xml
<tools>
    <li>
        <label>bottle</label>
        <capacities>
            <li>RK_BeerBottle</li>
        </capacities>
        <power>10</power>
        <cooldownTime>2.1</cooldownTime>
        <chanceFactor>2.0</chanceFactor>
    </li>
</tools>
```

#### 1.2 ToolCapacityDef와 ManeuverDef 연결
- **Tool.cs (66-74)**: `Tool.Maneuvers` 속성
```csharp
public IEnumerable<ManeuverDef> Maneuvers
{
    get
    {
        return from x in DefDatabase<ManeuverDef>.AllDefsListForReading
        where this.capacities.Contains(x.requiredCapacity)
        select x;
    }
}
```
- Tool의 `capacities`에 포함된 `requiredCapacity`를 가진 모든 `ManeuverDef`를 찾습니다.

#### 1.3 ManeuverDef에서 Verb 생성
```xml
<ManeuverDef>
    <defName>RK_BeerBottleStrike</defName>
    <requiredCapacity>RK_BeerBottle</requiredCapacity>
    <verb>
        <verbClass>NewRatkin.Verb_StrawberryBeerMelee</verbClass>
        <meleeDamageDef>Blunt</meleeDamageDef>
    </verb>
</ManeuverDef>
```

#### 1.4 VerbTracker에서 Verb 인스턴스 생성
**VerbTracker.cs (225-248)**:
```csharp
List<Tool> tools = this.directOwner.Tools;
if (tools != null)
{
    for (int j = 0; j < tools.Count; j++)
    {
        Tool tool = tools[j];
        foreach (ManeuverDef maneuverDef in tool.Maneuvers)
        {
            VerbProperties verb = maneuverDef.verb;
            string text2 = Verb.CalculateUniqueLoadID(this.directOwner, tool, maneuverDef);
            this.InitVerb(creator(verb.verbClass, text2), verb, tool, maneuverDef, text2);
        }
    }
}
```

**VerbTracker.cs (252-260)**: `InitVerb` 메서드
```csharp
private void InitVerb(Verb verb, VerbProperties properties, Tool tool, ManeuverDef maneuver, string id)
{
    verb.loadID = id;
    verb.verbProps = properties;
    verb.verbTracker = this;
    verb.tool = tool;
    verb.maneuver = maneuver;
    verb.caster = this.directOwner.ConstantCaster;
}
```

**핵심**: `ManeuverDef.verb.verbClass`를 사용하여 Verb 인스턴스를 생성하고, `verb.tool`과 `verb.maneuver`를 설정합니다.

### 2. Verb 선택 로직

#### 2.1 Pawn_MeleeVerbs.GetUpdatedAvailableVerbsList
**Pawn_MeleeVerbs.cs (141-248)**:
- `pawn.verbTracker.AllVerbs` 수집 (147줄)
- `equipment.AllVerbs` 수집 (160-171줄)
- `IsUsableMeleeVerb`로 필터링 (300줄)

**IsUsableMeleeVerb 필터 (300줄)**:
```csharp
private bool <GetUpdatedAvailableVerbsList>g__IsUsableMeleeVerb|18_0(Verb v)
{
    return v.IsMeleeAttack && v.IsStillUsableBy(this.pawn);
}
```

#### 2.2 Verb.IsMeleeAttack 속성
**Verb.cs (293-297)**:
```csharp
public virtual bool IsMeleeAttack
{
    get
    {
        return this.verbProps.IsMeleeAttack;
    }
}
```

**VerbProperties.cs (295-299)**:
```csharp
public bool IsMeleeAttack
{
    get
    {
        return typeof(Verb_MeleeAttack).IsAssignableFrom(this.verbClass);
    }
}
```

**핵심**: `verbClass`가 `Verb_MeleeAttack`의 서브클래스인지 확인합니다.

#### 2.3 Verb 선택 (가중치 기반)
**Pawn_MeleeVerbs.cs (61-98)**: `ChooseMeleeVerb`
```csharp
private void ChooseMeleeVerb(Thing target)
{
    bool flag = Rand.Chance(0.04f);
    List<VerbEntry> updatedAvailableVerbsList = this.GetUpdatedAvailableVerbsList(flag);
    bool flag2 = false;
    VerbEntry verbEntry;
    if (updatedAvailableVerbsList.TryRandomElementByWeight((VerbEntry ve) => ve.GetSelectionWeight(target), out verbEntry))
    {
        flag2 = true;
    }
    // ...
    if (flag2)
    {
        this.SetCurMeleeVerb(verbEntry.verb, target);
        return;
    }
}
```

**가중치 계산**: `VerbProperties.AdjustedMeleeSelectionWeight()`
- `(예상 피해량²) × ManeuverDef.commonality × Tool.chanceFactor × (Pawn 기본 Tool인 경우 0.3)`

### 3. Verb 실행 흐름

#### 3.1 TryMeleeAttack 호출
**Pawn_MeleeVerbs.cs (100-139)**:
```csharp
public bool TryMeleeAttack(Thing target, Verb verbToUse = null, bool surpriseAttack = false)
{
    // ...
    Verb verb = verbToUse ?? this.TryGetMeleeVerb(target);
    if (verb == null)
    {
        return false;
    }
    verb.TryStartCastOn(target, surpriseAttack, true, false, false);
    return true;
}
```

#### 3.2 Verb.TryStartCastOn → TryCastShot
**Verb_MeleeAttack.cs (13-73)**: `TryCastShot`
```csharp
protected override bool TryCastShot()
{
    Pawn casterPawn = this.CasterPawn;
    if (!casterPawn.Spawned || casterPawn.stances.FullBodyBusy) return false;

    Thing targetThing = this.currentTarget.Thing;
    if (!this.CanHitTarget(targetThing))
        Log.Warning($"{casterPawn} meleed {targetThing} from out of melee position.");

    casterPawn.rotationTracker.Face(targetThing.DrawPos);
    // ...
    
    // 명중률 계산
    if (Rand.Chance(this.GetNonMissChance(targetThing)))
    {
        // 회피률 계산
        if (!Rand.Chance(this.GetDodgeChance(targetThing)))
        {
            // 타겟에 근접 피해 적용
            DamageWorker.DamageResult damageResult = this.ApplyMeleeDamageToTarget(this.currentTarget);
            // ...
        }
    }
    // ...
}
```

#### 3.3 ApplyMeleeDamageToTarget 호출
**Verb_MeleeAttackDamage.cs (107-119)**:
```csharp
protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
{
    DamageWorker.DamageResult result = new DamageWorker.DamageResult();
    foreach (DamageInfo dinfo in this.DamageInfosToApply(target))
    {
        if (target.ThingDestroyed)
        {
            break;
        }
        result = target.Thing.TakeDamage(dinfo);
    }
    return result;
}
```

**핵심**: `Verb_StrawberryBeerMelee`가 `ApplyMeleeDamageToTarget`을 오버라이드하면, 이 메서드가 호출됩니다.

## 문제 분석

### 현재 설정
1. **Tool**: `RK_BeerBottle` capacity 사용
2. **ManeuverDef**: `RK_BeerBottleStrike` - `verbClass` = `NewRatkin.Verb_StrawberryBeerMelee`
3. **Verb_StrawberryBeerMelee**: `Verb_MeleeAttackDamage` 상속 → `Verb_MeleeAttack` 상속 → `IsMeleeAttack` = true

### 예상 동작
1. `VerbTracker.InitVerbsFromZero()`에서 Tool의 ManeuverDef를 찾아 `Verb_StrawberryBeerMelee` 인스턴스 생성
2. `Pawn_MeleeVerbs.GetUpdatedAvailableVerbsList()`에서 `IsMeleeAttack` = true이므로 포함
3. `ChooseMeleeVerb()`에서 가중치 기반 선택
4. `TryMeleeAttack()`에서 선택된 Verb 실행
5. `Verb_StrawberryBeerMelee.TryCastShot()` → `ApplyMeleeDamageToTarget()` 호출

### 실제 문제
- 근접 공격 시 맥주가 폭발하고 소진됨
- 이는 `Verb_StrawberryBeerShoot`가 발동되고 있다는 의미

### 가능한 원인
1. **Verb 생성 실패**: `Verb_StrawberryBeerMelee`가 제대로 생성되지 않음
2. **Verb 선택 실패**: `IsMeleeAttack` 체크 실패 또는 가중치 문제
3. **Verb 실행 실패**: 다른 Verb가 선택되어 실행됨

## 디버깅 체크리스트

1. **Verb 생성 확인**
   - `VerbTracker.AllVerbs`에 `Verb_StrawberryBeerMelee` 인스턴스가 있는지 확인
   - `verb.tool`, `verb.maneuver`가 제대로 설정되었는지 확인

2. **Verb 선택 확인**
   - `GetUpdatedAvailableVerbsList()`에서 `Verb_StrawberryBeerMelee`가 포함되는지 확인
   - `IsMeleeAttack`이 true인지 확인
   - `GetSelectionWeight()` 값 확인

3. **Verb 실행 확인**
   - `TryMeleeAttack()`에서 선택된 Verb가 `Verb_StrawberryBeerMelee`인지 확인
   - `Verb_StrawberryBeerMelee.TryCastShot()`이 호출되는지 확인
   - `ApplyMeleeDamageToTarget()`이 호출되는지 확인

## 해결 방안

### 1. Verb 생성 확인
- 로그 추가: `VerbTracker.InitVerb`에서 Verb 생성 시 로그 출력
- `Verb_StrawberryBeerMelee` 생성자에 로그 추가

### 2. Verb 선택 확인
- 로그 추가: `GetUpdatedAvailableVerbsList()`에서 수집된 Verb 목록 출력
- 로그 추가: `ChooseMeleeVerb()`에서 선택된 Verb 출력

### 3. Verb 실행 확인
- 로그 추가: `Verb_StrawberryBeerMelee.TryCastShot()` 시작 시 로그 출력
- 로그 추가: `ApplyMeleeDamageToTarget()` 시작 시 로그 출력

## 관련 파일

- `RimworldSource/Verse/Tool.cs` - Tool 클래스 정의
- `RimworldSource/Verse/VerbTracker.cs` - Verb 생성 로직
- `RimworldSource/RimWorld/Pawn_MeleeVerbs.cs` - Verb 선택 로직
- `RimworldSource/Verse/VerbProperties.cs` - IsMeleeAttack 속성
- `RimworldSource/RimWorld/Verb_MeleeAttack.cs` - TryCastShot 구현
- `RimworldSource/RimWorld/Verb_MeleeAttackDamage.cs` - ApplyMeleeDamageToTarget 구현
- `Project/1.6/Source/Beer/Verb_StrawberryBeerMelee.cs` - 커스텀 Verb
- `Project/1.6/Defs/Things_ItemDefs/Thing_Drink.xml` - Tool 및 ManeuverDef 정의

