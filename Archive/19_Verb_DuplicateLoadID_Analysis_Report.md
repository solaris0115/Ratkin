# Verb 중복 LoadID 문제 분석 보고서

## 문제 현상

게임 저장 후 불러올 때 다음 오류 발생:
```
Cannot register RimWorld.Verb_MeleeAttackDamage RimWorld.Verb_MeleeAttackDamage(null), 
(id=Verb_CompEquippable_RK_Gunlance_NormalType85822_0_Stab in loaded object directory. 
Id already used by RimWorld.Verb_MeleeAttackDamage RimWorld.Verb_MeleeAttackDamage(null).
```

**발생 조건:**
- `RK_Gunlance_NormalType` 무기를 장착하고 공격 명령 실행
- 게임 저장
- 게임 불러오기

## 원인 분석

### 1. RimWorld의 Verb 저장/불러오기 메커니즘

#### Verb LoadID 생성 방식
```466:474:RimworldSource/Verse/Verb.cs
public static string CalculateUniqueLoadID(IVerbOwner owner, Tool tool, ManeuverDef maneuver)
{
	return string.Format("{0}_{1}_{2}", owner.UniqueVerbOwnerID(), (tool != null) ? tool.id : "NT", (maneuver != null) ? maneuver.defName : "NM");
}

public static string CalculateUniqueLoadID(IVerbOwner owner, int index)
{
	return string.Format("{0}_{1}", owner.UniqueVerbOwnerID(), index);
}
```

#### CompEquippable의 UniqueVerbOwnerID
```129:132:RimworldSource/Verse/CompEquippable.cs
string IVerbOwner.UniqueVerbOwnerID()
{
	return "CompEquippable_" + this.parent.ThingID;
}
```

**결과적으로 LoadID 형식:**
- Tool/Maneuver 기반: `CompEquippable_{ThingID}_{ToolID}_{ManeuverDefName}`
- 예시: `Verb_CompEquippable_RK_Gunlance_NormalType85822_0_Stab`
  - `RK_Gunlance_NormalType85822`: 무기의 ThingID
  - `0`: Tool 인덱스 (첫 번째 tool)
  - `Stab`: Tool의 capacity 중 하나

### 2. 저장/불러오기 프로세스

#### 저장 단계 (VerbTracker.ExposeData)
```166:168:RimworldSource/Verse/VerbTracker.cs
public void ExposeData()
{
	Scribe_Collections.Look<Verb>(ref this.verbs, "verbs", LookMode.Deep, Array.Empty<object>());
```

- `Scribe_Collections.Look`이 `LookMode.Deep`으로 각 Verb를 깊게 저장
- 각 Verb의 `loadID`가 저장됨
- Verb가 `ILoadReferenceable`을 구현하므로 `LoadedObjectDirectory`에 등록됨

#### 불러오기 단계 (VerbTracker.ExposeData - ResolvingCrossRefs)
```169:187:RimworldSource/Verse/VerbTracker.cs
if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && this.verbs != null)
{
	if (this.verbs.RemoveAll((Verb x) => x == null) != 0)
	{
		Log.Error("Some verbs were null after loading. directOwner=" + this.directOwner.ToStringSafe<IVerbOwner>());
	}
	List<Verb> sources = this.verbs;
	this.verbs = new List<Verb>();
	this.InitVerbs(delegate(Type type, string id)
	{
		Verb verb = sources.FirstOrDefault((Verb v) => v.loadID == id && v.GetType() == type);
		if (verb == null)
		{
			Log.Warning(string.Format("Replaced verb {0}/{1}; may have been changed through a version update or a mod change", type, id));
			verb = (Verb)Activator.CreateInstance(type);
		}
		this.verbs.Add(verb);
		return verb;
	});
}
```

**문제 발생 지점:**

1. **168번 라인**: 저장된 verbs를 불러옴
   - 기존 Verb 객체들이 `LoadedObjectDirectory`에 등록됨
   - 예: `Verb_MeleeAttackDamage` (loadID: `CompEquippable_RK_Gunlance_NormalType85822_0_Stab`)

2. **177-187번 라인**: `InitVerbs` 호출
   - `InitVerbs`는 Tool/Maneuver 기반으로 새로운 Verb를 생성하려고 시도
   - 동일한 loadID를 가진 Verb를 찾아 재사용하려고 함 (179번 라인)
   - **하지만** 기존 Verb가 이미 `LoadedObjectDirectory`에 등록되어 있음

3. **InitVerb에서 문제 발생**
```252:260:RimworldSource/Verse/VerbTracker.cs
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

- `InitVerb`에서 `verb.loadID = id`를 설정
- Verb가 `ILoadReferenceable`이므로 자동으로 `LoadedObjectDirectory`에 등록 시도
- **이미 같은 loadID가 존재하므로 중복 등록 오류 발생**

### 3. RK_Gunlance_NormalType의 Tool 구조

```37:63:Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml
<tools>
	<li>
		<label>point</label>
		<capacities>
			<li>GunlanceShell_Normal</li>
			<li>Stab</li>
		</capacities>
		<power>16</power>
		<cooldownTime>3</cooldownTime>
	</li>
	<li>
		<label>edge</label>
		<capacities>
			<li>Cut</li>
		</capacities>
		<power>27</power>
		<cooldownTime>3.4</cooldownTime>
	</li>
	<li>
		<label>gun fire</label>
		<capacities>
			<li>GunlanceShell_Normal</li>
		</capacities>
		<power>16</power>
		<cooldownTime>3.4</cooldownTime>
	</li>
</tools>
```

- 첫 번째 tool (`point`)에 `Stab` capacity가 포함됨
- `Stab` capacity를 사용하는 기본 ManeuverDef가 존재
- 이 ManeuverDef는 기본적으로 `Verb_MeleeAttackDamage` 또는 `Verb_MeleeAttack`을 사용

### 4. Stab ManeuverDef 확인

```18:30:RimWorldData/Core/Defs/Maneuvers/Maneuvers.xml
<ManeuverDef>
	<defName>Stab</defName>
	<requiredCapacity>Stab</requiredCapacity>
	<verb>
		<verbClass>Verb_MeleeAttackDamage</verbClass>
		<meleeDamageDef>Stab</meleeDamageDef>
	</verb>
	<logEntryDef>MeleeAttack</logEntryDef>
	<combatLogRulesHit>Maneuver_Stab_MeleeHit</combatLogRulesHit>
	<combatLogRulesDeflect>Maneuver_Stab_MeleeDeflect</combatLogRulesDeflect>
	<combatLogRulesMiss>Maneuver_Stab_MeleeMiss</combatLogRulesMiss>
	<combatLogRulesDodge>Maneuver_Stab_MeleeDodge</combatLogRulesDodge>
</ManeuverDef>
```

**확인된 사실:**
- `Stab` ManeuverDef는 `Verb_MeleeAttackDamage`를 사용함 (22번 라인)
- 이것이 정확히 오류 메시지에서 나타나는 Verb 클래스임
- `RK_Gunlance_NormalType`의 첫 번째 tool에 `Stab` capacity가 있어서, 이 ManeuverDef를 사용하여 `Verb_MeleeAttackDamage`가 생성됨

## 근본 원인

**핵심 문제:**
1. 저장 시 Tool/Maneuver로 생성된 Verb가 저장되고 `LoadedObjectDirectory`에 등록됨
2. 불러올 때 `VerbTracker.ExposeData`의 `ResolvingCrossRefs` 단계에서:
   - 저장된 Verb를 불러오면서 다시 `LoadedObjectDirectory`에 등록됨
   - 동시에 `InitVerbs`를 호출하여 같은 loadID를 가진 Verb를 찾거나 생성함
   - 기존 Verb를 재사용하려고 하지만, 이미 `LoadedObjectDirectory`에 등록되어 있어 중복 등록 시도 시 오류 발생

**추가 가능성:**
- `InitVerbs`에서 기존 Verb를 찾지 못하고 새로운 Verb를 생성하는 경우
- 새로 생성된 Verb가 같은 loadID로 등록하려고 할 때 충돌

## 해결 방안 (제안)

### 방안 1: Verb 재등록 방지
- `InitVerbs`에서 기존 Verb를 재사용할 때, `LoadedObjectDirectory`에서 먼저 제거 후 재등록
- RimWorld 소스 코드 수정이 필요하므로 비현실적

### 방안 2: Verb LoadID 수정
- Custom Verb 클래스에서 `GetUniqueLoadID()`를 오버라이드하여 고유한 접두사 추가
- 예: `"RK_Verb_" + this.loadID`

### 방안 3: Verb 등록 로직 수정
- `InitVerb` 전에 기존 Verb의 등록을 해제
- RimWorld 소스 코드 수정이 필요하므로 비현실적

### 방안 4: ManeuverDef에서 Verb 클래스 확인
- `Stab` capacity를 사용하는 ManeuverDef가 `Verb_MeleeAttackDamage` 대신 다른 Verb 클래스를 사용하도록 수정
- 또는 해당 ManeuverDef의 verbClass가 올바르게 설정되어 있는지 확인

## 확인 완료 사항

✅ `Stab` capacity를 사용하는 ManeuverDef 찾기 완료
✅ 해당 ManeuverDef의 verbClass 확인 완료: `Verb_MeleeAttackDamage`
✅ 문제가 되는 Verb 클래스 식별 완료: `Verb_MeleeAttackDamage`

## 해결 방안 상세 분석

### 문제의 핵심

**VerbTracker.ExposeData()의 로직:**
1. 저장된 Verb 객체를 불러올 때 (168번 라인) `LoadedObjectDirectory`에 자동 등록됨
2. `ResolvingCrossRefs` 단계에서 `InitVerbs`를 호출 (177번 라인)
3. `InitVerbs`는 Tool/Maneuver 조합으로 loadID를 계산하여 기존 Verb를 찾거나 새로 생성
4. **문제**: 저장된 Verb와 새로 찾은/생성한 Verb가 같은 loadID를 가지지만, 저장된 Verb가 이미 `LoadedObjectDirectory`에 등록되어 있음
5. `InitVerb`에서 `verb.loadID = id`를 설정하면, Verb의 `GetUniqueLoadID()`가 호출되어 다시 등록 시도
6. 이미 같은 ID가 존재하므로 중복 등록 오류 발생

### 해결 방안 우선순위

#### 방안 1: Harmony 패치로 InitVerb 수정 (권장)
- `InitVerb`에서 loadID를 설정하기 전에 기존 등록 해제
- 또는 기존 Verb를 재사용할 때 `LoadedObjectDirectory`에서 제거 후 재등록

#### 방안 2: RK_Gunlance_NormalType에서 Stab capacity 제거
- 가장 간단하지만, 기능 축소됨
- 다른 Tool로 Stab 공격 불가능

#### 방안 3: Custom Verb 클래스 오버라이드
- `Verb_MeleeAttackDamage`를 상속받는 Custom 클래스 생성
- `GetUniqueLoadID()` 오버라이드로 고유 접두사 추가
- RimWorld의 기본 동작과 충돌할 수 있음

#### 방안 4: Patch로 ManeuverDef 수정
- `Stab` ManeuverDef의 verbClass를 Custom 클래스로 변경
- 다른 모드와의 호환성 문제 가능

## 참고 사항

- RimWorld의 Verb 저장/불러오기 시스템은 복잡한 참조 해결 프로세스를 포함
- `ResolvingCrossRefs` 단계에서 객체들이 재초기화되면서 ID 충돌이 발생할 수 있음
- 모드 개발 시 Custom Verb 클래스를 사용할 때는 LoadID 생성 방식을 주의 깊게 설계해야 함
