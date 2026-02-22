# 맥주 근접 공격 시 Shoot 발동 문제 분석

**작성일**: 2024-12-30  
**문제**: tools를 통한 근접 공격 시 `Verb_StrawberryBeerShoot`가 발동되어 맥주가 폭발하고 소진됨

## 문제 상황

사용자가 맥주를 들고 근접 공격을 시도했는데, `Verb_StrawberryBeerShoot`가 발동되어 맥주가 폭발하고 소진되었습니다.

## 예상 동작

1. **근접 공격 시**:
   - `Pawn_MeleeVerbs.TryMeleeAttack()` 호출
   - `GetUpdatedAvailableVerbsList()`에서 `IsMeleeAttack == true`인 Verb만 수집
   - `Verb_StrawberryBeerMelee` 선택 (tools에서 생성된 Verb)
   - `Verb_StrawberryBeerMelee.TryCastShot()` 실행
   - 맥주 소진 없음

2. **원거리 공격 시**:
   - 사용자가 수동으로 원거리 공격 버튼 클릭
   - `Verb_StrawberryBeerShoot.TryCastShot()` 실행
   - 맥주 던지기 (폭발)
   - 맥주 소진됨

## 실제 동작 분석

### 1. Verb 생성 과정

**VerbTracker.InitVerbs()**:
```csharp
// 1. verbs 섹션의 VerbProperties로 Verb 생성
List<VerbProperties> verbProperties = this.directOwner.VerbProperties;
for (int i = 0; i < verbProperties.Count; i++)
{
    VerbProperties verbProperties2 = verbProperties[i];
    string text = Verb.CalculateUniqueLoadID(this.directOwner, i);
    this.InitVerb(creator(verbProperties2.verbClass, text), verbProperties2, null, null, text);
    // Verb_StrawberryBeerShoot 생성됨
}

// 2. tools 섹션의 Tool들로부터 ManeuverDef를 찾아 Verb 생성
List<Tool> tools = this.directOwner.Tools;
foreach (ManeuverDef maneuverDef in tool.Maneuvers)
{
    VerbProperties verb = maneuverDef.verb;
    string text2 = Verb.CalculateUniqueLoadID(this.directOwner, tool, maneuverDef);
    this.InitVerb(creator(verb.verbClass, text2), verb, tool, maneuverDef, text2);
    // Verb_StrawberryBeerMelee 생성됨
}
```

**결과**: `CompEquippable.AllVerbs`에는 두 Verb가 모두 포함됨
- `Verb_StrawberryBeerShoot` (verbs 섹션)
- `Verb_StrawberryBeerMelee` (tools 섹션)

### 2. 근접 공격 Verb 선택 과정

**Pawn_MeleeVerbs.GetUpdatedAvailableVerbsList()**:
```csharp
// equipment.AllVerbs 수집
CompEquippable comp = allEquipmentListForReading[j].GetComp<CompEquippable>();
List<Verb> list = (comp != null) ? comp.AllVerbs : null;
for (int k = 0; k < list.Count; k++)
{
    if (this.<GetUpdatedAvailableVerbsList>g__IsUsableMeleeVerb|18_0(list[k]))
    {
        verbsToAdd.Add(list[k]);
    }
}
```

**IsUsableMeleeVerb 필터**:
```csharp
private bool <GetUpdatedAvailableVerbsList>g__IsUsableMeleeVerb|18_0(Verb v)
{
    return v.IsMeleeAttack && v.IsStillUsableBy(this.pawn);
}
```

**VerbProperties.IsMeleeAttack**:
```csharp
public bool IsMeleeAttack
{
    get
    {
        return typeof(Verb_MeleeAttack).IsAssignableFrom(this.verbClass);
    }
}
```

**결과**:
- `Verb_StrawberryBeerShoot`: `IsMeleeAttack == false` (Verb_Shoot 상속) → **필터링됨**
- `Verb_StrawberryBeerMelee`: `IsMeleeAttack == true` (Verb_MeleeAttackDamage 상속) → **포함됨**

### 3. 문제 원인 추정

`IsMeleeAttack` 필터링이 제대로 작동한다면 `Verb_StrawberryBeerShoot`는 근접 공격 선택 목록에 포함되지 않아야 합니다.

**가능한 원인**:

1. **사용자가 수동으로 원거리 공격 버튼을 클릭했을 가능성**
   - UI에서 원거리 공격 버튼 클릭 시 `Verb_StrawberryBeerShoot`가 직접 호출됨
   - 이 경우 `TryMeleeAttack()`이 아닌 `Verb.TryStartCastOn()`이 직접 호출됨

2. **AI가 원거리 공격을 선택했을 가능성**
   - AI가 근접 범위 내에서도 원거리 공격을 선택할 수 있음
   - `Verb_StrawberryBeerShoot.CanHitTarget()`이 true를 반환하면 발동됨

3. **JobDriver_AttackMelee에서 verbToUse로 Verb_Shoot가 전달되었을 가능성**
   - `TryMeleeAttack(Thing target, Verb verbToUse = null)`에서 `verbToUse`가 null이 아니면 그 Verb를 사용
   - `verbToUse`가 `Verb_StrawberryBeerShoot`일 경우, `IsMeleeAttack` 체크에서 경고만 출력하고 계속 진행

**Pawn_MeleeVerbs.TryMeleeAttack()**:
```csharp
if (verbToUse != null)
{
    if (!verbToUse.IsStillUsableBy(pawn))
    {
        return false;
    }

    if (!verbToUse.IsMeleeAttack)
    {
        Log.Warning("Pawn " + pawn?.ToString() + " tried to melee attack " + target?.ToString() + " with non melee-attack verb " + verbToUse?.ToString() + ".");
        return false;  // ← 여기서 false 반환해야 하는데...
    }
}

Verb verb = verbToUse ?? TryGetMeleeVerb(target);
```

**중요**: `verbToUse`가 `Verb_Shoot`이고 `IsMeleeAttack == false`이면 `TryMeleeAttack()`은 `false`를 반환하고 종료됩니다. 따라서 이 경로로는 `Verb_Shoot`가 발동되지 않아야 합니다.

## 해결 방안

### 현재 구현된 보호 장치

1. **Verb_StrawberryBeerShoot.CanHitTarget()**: 근접 범위 내에서는 false 반환
2. **Verb_StrawberryBeerShoot.TryCastShot()**: 근접 범위 내에서는 false 반환

### 추가 보호 장치 필요 여부

현재 구현으로도 충분해야 하지만, 만약 여전히 문제가 발생한다면:

1. **로그 추가**: `Verb_StrawberryBeerShoot.TryCastShot()` 시작 시 로그 출력하여 실제로 호출되는지 확인
2. **Verb_StrawberryBeerShoot.IsMeleeAttack 오버라이드**: 항상 false 반환 (이미 Verb_Shoot 상속이므로 불필요)
3. **Verb_StrawberryBeerShoot.IsUsableOn() 오버라이드**: 근접 범위 내에서는 false 반환

## 결론

이론적으로는 `IsMeleeAttack` 필터링으로 인해 `Verb_StrawberryBeerShoot`는 근접 공격 선택 목록에 포함되지 않아야 합니다. 

만약 여전히 문제가 발생한다면:
1. 사용자가 수동으로 원거리 공격 버튼을 클릭했을 가능성
2. AI가 원거리 공격을 선택했을 가능성
3. 다른 경로로 `Verb_Shoot`가 호출되었을 가능성

현재 구현된 `CanHitTarget()`과 `TryCastShot()` 보호 장치가 제대로 작동한다면, 근접 범위 내에서는 `Verb_StrawberryBeerShoot`가 발동되지 않아야 합니다.

