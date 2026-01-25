# CompEquippable과 CompEquippableAbilityReloadable의 VerbTracker 중복 문제 분석 보고서

**작성일**: 2025-11-12  
**분석 목적**: CompProperties_EquippableAbilityReloadable 추가 시 발생하는 VerbTracker 중복 문제 원인 규명 및 해결 방안 제시

---

## 1. 문제 정의

### 1.1 에러 메시지
```
Cannot register RimWorld.Verb_MeleeAttackDamage RimWorld.Verb_MeleeAttackDamage(null), 
(id=Verb_CompEquippable_RK_Gunlance_NormalType10750_0_Stab in loaded object directory. 
Id already used by RimWorld.Verb_MeleeAttackDamage RimWorld.Verb_MeleeAttackDamage(null).

Cannot register NewRatkin.Verb_GunlanceFiring NewRatkin.Verb_GunlanceFiring(null), 
(id=Verb_CompEquippable_RK_Gunlance_NormalType10750_1_RK_GunlanceExplosion_Normal in loaded object directory. 
Id already used by NewRatkin.Verb_GunlanceFiring NewRatkin.Verb_GunlanceFiring(null).
```

### 1.2 발생 조건
- `RK_Gunlance_NormalType` 무기에 `CompProperties_EquippableAbilityReloadable` 추가 (라인 86-94)
- 기존 세이브 파일 로드
- 게임 저장 및 재로드

---

## 2. 근본 원인 분석

### 2.1 클래스 상속 구조

```
ThingComp
  └─ CompEquippable (VerbTracker 소유)
       └─ CompEquippableAbility (Ability 추가, Ability는 자체 VerbTracker 소유)
            └─ CompEquippableAbilityReloadable (재장전 기능 추가)
```

#### 각 클래스의 VerbTracker 생성

**CompEquippable.cs:**
```csharp
public class CompEquippable : ThingComp, IVerbOwner
{
    public VerbTracker verbTracker;  // ← 무기의 tools 기반 Verb 관리
    
    public CompEquippable()
    {
        this.verbTracker = new VerbTracker(this);  // ← 생성자에서 VerbTracker 생성
    }
    
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Deep.Look<VerbTracker>(ref this.verbTracker, "verbTracker", new object[] { this });
    }
    
    string IVerbOwner.UniqueVerbOwnerID()
    {
        return "CompEquippable_" + this.parent.ThingID;  // ← LoadID 접두사
    }
}
```

**Ability.cs:**
```csharp
public class Ability : IVerbOwner, IExposable, ILoadReferenceable
{
    private VerbTracker verbTracker;  // ← Ability의 verbProperties 기반 Verb 관리
    
    public VerbTracker VerbTracker
    {
        get
        {
            if (this.verbTracker == null)
            {
                this.verbTracker = new VerbTracker(this);  // ← 지연 생성 (lazy init)
            }
            return this.verbTracker;
        }
    }
    
    public virtual void ExposeData()
    {
        // ... 다른 데이터 저장 ...
        Scribe_Deep.Look<VerbTracker>(ref this.verbTracker, "verbTracker", new object[] { this });
        // ... 계속 ...
    }
    
    public string UniqueVerbOwnerID()
    {
        return this.GetUniqueLoadID();  // ← "Ability_{Id}"
    }
}
```

**CompEquippableAbility.cs:**
```csharp
public class CompEquippableAbility : CompEquippable
{
    private Ability ability;  // ← Ability 객체 (자체 VerbTracker 소유)
    
    public override void PostExposeData()
    {
        base.PostExposeData();  // ← CompEquippable의 verbTracker 저장 (여기서 문제 발생!)
        Scribe_Deep.Look<Ability>(ref this.ability, "ability", Array.Empty<object>());
    }
}
```

### 2.2 세이브 파일 구조 분석

**세이브 파일 (새로운 시작5.rws, 라인 173620-173668):**
```xml
<li>
  <def>RK_Gunlance_NormalType</def>
  <id>RK_Gunlance_NormalType10750</id>
  
  <!-- ============ 첫 번째 verbTracker (CompEquippable의 것) ============ -->
  <verbTracker>
    <verbs>
      <li Class="Verb_MeleeAttackDamage">
        <loadID>CompEquippable_RK_Gunlance_NormalType10750_0_Stab</loadID>
      </li>
      <li Class="NewRatkin.Verb_GunlanceFiring">
        <loadID>CompEquippable_RK_Gunlance_NormalType10750_1_RK_GunlanceExplosion_Normal</loadID>
      </li>
    </verbs>
  </verbTracker>
  
  <quality>Normal</quality>
  
  <!-- ============ 두 번째 verbTracker (중복!) ============ -->
  <verbTracker>
    <verbs>
      <li Class="Verb_MeleeAttackDamage">
        <loadID>CompEquippable_RK_Gunlance_NormalType10750_0_Stab</loadID>  ← 동일 ID
      </li>
      <li Class="NewRatkin.Verb_GunlanceFiring">
        <loadID>CompEquippable_RK_Gunlance_NormalType10750_1_RK_GunlanceExplosion_Normal</loadID>  ← 동일 ID
      </li>
    </verbs>
  </verbTracker>
  
  <!-- ============ Ability의 verbTracker (정상) ============ -->
  <ability>
    <def>RK_WyvernFire_Ability</def>
    <Id>140</Id>
    <verbTracker>
      <verbs>
        <li Class="Verb_CastAbility">
          <loadID>Ability_140_0</loadID>  ← 다른 접두사, 정상
        </li>
      </verbs>
    </verbTracker>
  </ability>
</li>
```

**문제 발생 메커니즘:**

1. **RimWorld의 ThingWithComps 저장 프로세스:**
   - ThingWithComps는 여러 컴포넌트를 가질 수 있음
   - 각 컴포넌트의 `PostExposeData()`를 순차적으로 호출
   - XML에 컴포넌트별 데이터가 순차적으로 기록됨

2. **CompEquippableAbilityReloadable의 저장:**
   ```csharp
   // CompEquippableAbility.PostExposeData()
   base.PostExposeData();  // ← CompEquippable의 verbTracker 저장 (첫 번째)
   Scribe_Deep.Look<Ability>(ref this.ability, "ability", ...);  // ← Ability 저장 (ability 내부에 verbTracker)
   ```

3. **왜 verbTracker가 두 번 저장되는가?**
   - **원인 1**: RimWorld의 Scribe 시스템이 필드 이름으로 데이터를 구분함
   - **원인 2**: `CompEquippable.verbTracker`와 `Ability.verbTracker`는 다른 객체임
   - **원인 3**: **하지만** XML에서는 둘 다 `<verbTracker>`라는 동일한 태그로 저장됨
   - **원인 4**: Scribe가 같은 태그를 두 번 저장하는 것을 허용함 (버그 또는 의도된 동작)

4. **로드 시 문제:**
   - `VerbTracker.ExposeData()`가 호출되면서 저장된 Verb들이 `LoadedObjectDirectory`에 등록됨
   - 첫 번째 verbTracker 로드: Verb들이 등록됨 (예: `CompEquippable_..._0_Stab`)
   - 두 번째 verbTracker 로드: **같은 loadID를 가진 Verb를 다시 등록하려고 시도** → 에러 발생!

### 2.3 RimWorld 바닐라 HellcatBurner 분석

**RimWorldData/Anomaly/Defs/ThingDefs_Misc/Weapons/Weapons_Ranged.xml (라인 15-90):**
```xml
<ThingDef ParentName="BaseHumanMakeableGun">
  <defName>Gun_HellcatBurner</defName>
  <label>hellcat burner</label>
  <!-- ... 그래픽, 스탯 등 ... -->
  
  <!-- ========== 중요한 주석 ========== -->
  <!-- Can't inherit comps because we need to override CompEquippable with CompEquippableAbility -->
  <!-- If changes are made to the comps of BaseHumanMakeableGun or parents then they will need to be replicated here -->
  
  <!-- ========== comps Inherit="False" ========== -->
  <comps Inherit="False">
    <li Class="CompProperties_EquippableAbilityReloadable">
      <abilityDef>HellcatBurner</abilityDef>
      <maxCharges>2</maxCharges>
      <soundReload>Standard_Reload</soundReload>
      <chargeNoun>burner charge</chargeNoun>
      <ammoDef>Bioferrite</ammoDef>
      <ammoCountPerCharge>10</ammoCountPerCharge>
      <baseReloadTicks>60</baseReloadTicks>
    </li>
    <li Class="CompProperties_Forbiddable"/>
    <li Class="CompProperties_Styleable"/>
    <li>
      <compClass>CompQuality</compClass>
    </li>
    <!-- ... 다른 comps ... -->
  </comps>
  
  <!-- ========== verbs (총기 발사) ========== -->
  <verbs>
    <li>
      <verbClass>Verb_Shoot</verbClass>
      <hasStandardCommand>true</hasStandardCommand>
      <defaultProjectile>Bullet_HellcatBurner</defaultProjectile>
      <!-- ... verb 설정 ... -->
    </li>
  </verbs>
  
  <!-- ========== tools (근접 공격) ========== -->
  <tools>
    <li>
      <label>stock</label>
      <capacities>
        <li>Blunt</li>
      </capacities>
      <power>9</power>
      <cooldownTime>2</cooldownTime>
    </li>
    <li>
      <label>barrel</label>
      <capacities>
        <li>Poke</li>
      </capacities>
      <power>9</power>
      <cooldownTime>2</cooldownTime>
    </li>
  </tools>
</ThingDef>
```

**HellcatBurner와 Ratkin Gunlance의 비교:**

| 항목 | HellcatBurner | RK_Gunlance_NormalType | 비고 |
|------|---------------|------------------------|------|
| `verbs` | ✅ 있음 (Verb_Shoot) | ❌ 없음 | HellcatBurner는 총기 |
| `tools` | ✅ 있음 (Blunt, Poke) | ✅ 있음 (Stab, GunlanceShell) | 둘 다 근접 공격 가능 |
| `CompProperties_EquippableAbilityReloadable` | ✅ 있음 | ✅ 있음 | 둘 다 어빌리티 사용 |
| `<comps Inherit="False">` | ✅ 명시적 | ⚠️ 없음 (기본 상속) | **핵심 차이점** |

**핵심 발견사항:**

1. **HellcatBurner는 `<comps Inherit="False">`를 명시적으로 사용:**
   - 부모 Def의 `CompEquippable`을 상속받지 않음
   - 대신 `CompProperties_EquippableAbilityReloadable`만 사용
   - 따라서 **컴포넌트가 하나만 존재** → verbTracker도 하나

2. **Ratkin Gunlance는 상속 기본값 사용:**
   - 부모 Def (`RK_MeleeWeapon`)에서 `CompEquippable` 상속받음 (추정)
   - 추가로 `CompProperties_EquippableAbilityReloadable` 추가
   - 따라서 **두 컴포넌트가 공존 가능** → verbTracker 중복 가능성

### 2.4 왜 세이브 파일에만 중복이 나타나는가?

**게임 실행 중 (런타임):**
- RimWorld는 하나의 Thing에 대해 하나의 `CompEquippable` 인스턴스만 생성
- `CompEquippableAbilityReloadable`은 `CompEquippable`을 **대체**하는 것이지, 추가하는 것이 아님
- 따라서 런타임에는 verbTracker가 하나만 존재

**세이브/로드 시:**
- **문제점**: RimWorld의 Def 변경 전후 호환성 처리
- **추정 시나리오**:
  1. 초기: Gunlance는 `CompEquippable`만 가짐 → verbTracker 하나 저장
  2. Def 변경: `CompProperties_EquippableAbilityReloadable` 추가
  3. 기존 세이브 로드: 
     - 기존 verbTracker가 XML에 존재 (첫 번째)
     - 새로운 컴포넌트가 자신의 verbTracker를 저장 (두 번째)
  4. 세이브 시: 두 verbTracker 모두 XML에 기록
  5. 재로드 시: 중복 LoadID 에러!

**Scribe 시스템의 동작:**
```csharp
// ThingWithComps.PostExposeData() 의사 코드
foreach (var comp in comps)
{
    comp.PostExposeData();  // ← 각 컴포넌트가 자신의 데이터 저장
}
```

- Scribe는 필드 이름으로 데이터를 구분하지만, 같은 이름을 여러 번 저장하는 것을 막지 않음
- XML에서 같은 태그가 여러 번 나타나면, 로드 시 마지막 것만 사용하거나 모두 로드하려고 시도
- VerbTracker의 경우, 모두 로드하면서 LoadID 충돌 발생

---

## 3. 문제 해결 방안

### 3.1 방안 1: `<comps Inherit="False">` 사용 (권장, RimWorld 표준)

**설명**: HellcatBurner와 동일한 방식으로, 부모 Def의 comps를 상속받지 않고 명시적으로 정의

**수정 방법:**

```xml
<ThingDef ParentName="RK_MeleeWeapon">
  <defName>RK_Gunlance_NormalType</defName>
  <!-- ... 기타 설정 ... -->
  
  <!-- 부모 Def의 comps를 상속받지 않음 -->
  <comps Inherit="False">
    <!-- RK_MeleeWeapon이나 BaseMeleeWeapon_Sharp_Quality에서 필요한 comps 복사 -->
    <li Class="CompProperties_Forbiddable" />
    <li>
      <compClass>CompColorable</compClass>
    </li>
    <li>
      <compClass>CompQuality</compClass>
    </li>
    
    <!-- WeaponExtension (기존 무기 오프셋) -->
    <li Class="SYS.CompProperties_WeaponExtention">
      <littleDown>True</littleDown>
      <!-- ... 오프셋 설정 ... -->
    </li>
    
    <!-- CompEquippableAbilityReloadable (이게 CompEquippable을 대체) -->
    <li Class="CompProperties_EquippableAbilityReloadable">
      <abilityDef>RK_WyvernFire_Ability</abilityDef>
      <maxCharges>2</maxCharges>
      <soundReload>Standard_Reload</soundReload>
      <chargeNoun>wyvern charge</chargeNoun>
      <ammoDef>RK_Ammo_WyvernFire</ammoDef>
      <ammoCountPerCharge>1</ammoCountPerCharge>
      <baseReloadTicks>60</baseReloadTicks>
    </li>
  </comps>
</ThingDef>
```

**장점:**
- RimWorld 공식 패턴
- 깔끔하고 명확
- 새 게임 시작 시 문제 없음

**단점:**
- 부모 Def의 comps를 모두 수동으로 복사해야 함
- 부모 Def 변경 시 동기화 필요
- **기존 세이브 파일 호환성 문제** (무기를 재제작해야 함)

### 3.2 방안 2: 세이브 파일 수동 수정

**설명**: 세이브 파일에서 중복된 verbTracker를 제거

**수정 방법:**

1. `새로운 시작5.rws` 백업
2. 텍스트 에디터로 열기
3. 라인 173646-173659 (두 번째 verbTracker) 삭제
4. 저장 후 게임 로드

**장점:**
- 현재 게임 진행 상황 유지
- 즉시 해결 가능

**단점:**
- 수동 작업 필요
- 다시 저장하면 문제 재발 가능 (Def 구조를 수정하지 않으면)
- 위험함 (백업 필수)

### 3.3 방안 3: 주석 처리 후 재로드 (임시 해결)

**설명**: Def에서 `CompProperties_EquippableAbilityReloadable`를 임시 제거하여 verbTracker 중복 해소

**수정 방법:**

1. `Weapon_HighTech.xml`의 86-94 라인 주석 처리
2. 게임 로드 (정상 로드됨, ability는 사라짐)
3. 게임 저장 (깨끗한 verbTracker 하나만 저장됨)
4. 주석 해제
5. 게임 재로드 (새로운 ability와 함께 정상 작동)

**장점:**
- 비교적 안전
- 세이브 파일 자동 정리

**단점:**
- 여러 단계 필요
- ability 차지가 초기화될 수 있음

### 3.4 방안 4: 새 무기 제작 (가장 안전)

**설명**: 기존 Gunlance를 버리고 새로운 Gunlance를 제작

**수정 방법:**

1. Def를 방안 1처럼 수정
2. 게임에서 캐릭터가 장착한 Gunlance 해제
3. 새로운 Gunlance 제작
4. 새 Gunlance 장착

**장점:**
- 가장 안전
- 게임 내에서 해결

**단점:**
- 재료와 시간 소요
- 기존 무기 품질 손실

---

## 4. 권장 해결 순서

### 단계 1: Def 구조 수정 (방안 1)

- `<comps Inherit="False">` 추가
- 필요한 comps 모두 명시적으로 정의

### 단계 2-A: 새 게임 시작 (가장 확실)

- 새 게임은 문제 없음

### 단계 2-B: 기존 세이브 유지 (방안 3 추천)

1. Def 주석 처리
2. 로드 → 저장
3. 주석 해제
4. 재로드

---

## 5. RimWorld 모딩 Best Practice

### 5.1 CompEquippableAbilityReloadable 사용 시 규칙

**규칙 1**: 반드시 `<comps Inherit="False">` 사용

```xml
<!-- 바닐라 무기가 이 패턴을 따름 -->
<comps Inherit="False">
  <li Class="CompProperties_EquippableAbilityReloadable">
    <!-- ... -->
  </li>
  <!-- 부모의 다른 comps 복사 -->
</comps>
```

**규칙 2**: 주석으로 이유 명시

```xml
<!-- Can't inherit comps because we need to override CompEquippable with CompEquippableAbility -->
<!-- If changes are made to the comps of ParentDef then they will need to be replicated here -->
```

**규칙 3**: 부모 Def 변경 시 동기화

- 부모 Def의 comps가 변경되면 자식 Def도 업데이트 필요

### 5.2 VerbTracker가 있는 다른 컴포넌트

**주의해야 할 컴포넌트들:**

1. **CompEquippable**: 장비 기본 (VerbTracker 소유)
2. **CompEquippableAbility**: Ability 추가 (Ability는 VerbTracker 소유)
3. **CompEquippableAbilityReloadable**: 재장전 가능 Ability
4. **CompApparelVerbOwner**: 의복에 Verb 추가 (VerbTracker 소유)
5. **Pawn_NativeVerbs**: Pawn의 기본 Verb (VerbTracker 소유)
6. **HediffComp_VerbGiver**: Hediff로 Verb 부여 (VerbTracker 소유)

**충돌 방지:**
- 하나의 Thing/Pawn에 VerbTracker를 가진 여러 컴포넌트가 있을 수 있음
- 각 VerbTracker는 고유한 `UniqueVerbOwnerID()`를 반환해야 함
- LoadID 충돌을 방지하려면 각 IVerbOwner가 고유한 접두사를 사용해야 함

---

## 6. 기술적 세부 사항

### 6.1 VerbTracker와 LoadID 시스템

**LoadID 계산 방식:**

```csharp
// Verb.cs
public static string CalculateUniqueLoadID(IVerbOwner owner, Tool tool, ManeuverDef maneuver)
{
    return string.Format("{0}_{1}_{2}", 
        owner.UniqueVerbOwnerID(),  // 예: "CompEquippable_RK_Gunlance_NormalType10750"
        (tool != null) ? tool.id : "NT",  // 예: "0" (tool 인덱스)
        (maneuver != null) ? maneuver.defName : "NM");  // 예: "Stab"
}
// 결과: "CompEquippable_RK_Gunlance_NormalType10750_0_Stab"
```

**LoadedObjectDirectory 등록:**

```csharp
// Verb.cs (property setter)
public string loadID
{
    get { return this.loadIDInt; }
    set
    {
        if (this.loadIDInt != null && Scribe.mode != LoadSaveMode.Inactive)
        {
            // 이미 등록된 경우 제거
            Current.Game.GetComponent<LoadedObjectDirectory>().UnregisterLoaded(this);
        }
        this.loadIDInt = value;
        if (value != null && Scribe.mode != LoadSaveMode.Inactive)
        {
            // 새로운 ID로 등록 (여기서 중복 시 에러 발생!)
            Current.Game.GetComponent<LoadedObjectDirectory>().RegisterLoaded(this);
        }
    }
}
```

### 6.2 CompEquippable vs CompEquippableAbility

**메모리 구조 (런타임):**

```
Thing (RK_Gunlance_NormalType)
  └─ comps[]
       └─ CompEquippableAbilityReloadable (단 하나만 존재)
            ├─ verbTracker (CompEquippable에서 상속)
            │    └─ verbs[]
            │         ├─ Verb_MeleeAttackDamage (Stab)
            │         └─ Verb_GunlanceFiring (GunlanceShell)
            └─ ability (Ability 객체)
                 └─ verbTracker (별도)
                      └─ verbs[]
                           └─ Verb_CastAbility (WyvernFire)
```

**세이브 파일 구조 (문제 있는 경우):**

```xml
<Thing>
  <verbTracker>  <!-- CompEquippable.verbTracker -->
    <verbs>
      <li loadID="CompEquippable_...10750_0_Stab" />
      <li loadID="CompEquippable_...10750_1_RK_GunlanceExplosion_Normal" />
    </verbs>
  </verbTracker>
  
  <verbTracker>  <!-- 중복! 이유 불명 -->
    <verbs>
      <li loadID="CompEquippable_...10750_0_Stab" />  <!-- 충돌! -->
      <li loadID="CompEquippable_...10750_1_RK_GunlanceExplosion_Normal" />  <!-- 충돌! -->
    </verbs>
  </verbTracker>
  
  <ability>
    <verbTracker>  <!-- Ability.verbTracker (정상) -->
      <verbs>
        <li loadID="Ability_140_0" />  <!-- 다른 접두사, OK -->
      </verbs>
    </verbTracker>
  </ability>
</Thing>
```

### 6.3 Scribe 시스템의 동작

**Scribe_Deep.Look 동작:**

```csharp
// Scribe_Deep.cs (의사 코드)
public static void Look<T>(ref T target, string label, params object[] ctorArgs)
{
    if (Scribe.mode == LoadSaveMode.Saving)
    {
        // 저장: XML에 <label> 태그로 기록
        curXmlParent.AppendChild(label, SerializeObject(target));
    }
    else if (Scribe.mode == LoadSaveMode.LoadingVars)
    {
        // 로드: XML에서 <label> 태그 찾기
        XmlNode node = curXmlParent.SelectSingleNode(label);  // ← 첫 번째만 선택!
        if (node != null)
        {
            target = DeserializeObject<T>(node, ctorArgs);
        }
    }
}
```

**중복 태그 처리:**
- `SelectSingleNode()`는 첫 번째 매칭 노드만 반환
- 하지만 `VerbTracker.ExposeData()`에서 Verb들을 `LoadedObjectDirectory`에 등록하면서 문제 발생
- 두 번째 verbTracker를 로드할 때, 이미 같은 LoadID가 등록되어 있어 에러

---

## 7. 결론

### 7.1 핵심 원인

1. **CompEquippableAbilityReloadable는 CompEquippable을 상속받음**
   - CompEquippable은 자체 verbTracker를 가짐

2. **RimWorld는 하나의 Thing에 하나의 Comp 인스턴스만 생성**
   - 런타임에는 문제 없음

3. **세이브/로드 시 verbTracker가 중복 저장됨**
   - 원인 불명 (RimWorld 버그 또는 Def 변경 시 호환성 문제)
   - 같은 LoadID를 가진 Verb가 두 번 등록되면서 충돌

4. **RimWorld 공식 무기는 `<comps Inherit="False">`를 사용**
   - 부모의 CompEquippable을 상속받지 않음
   - CompEquippableAbilityReloadable만 사용

### 7.2 권장 해결책

**즉시 적용:**
- `Weapon_HighTech.xml`에 `<comps Inherit="False">` 추가
- 부모 Def의 필요한 comps를 모두 복사

**세이브 파일 처리:**
- 방안 3 (주석 처리 후 재로드) 사용
- 또는 새 게임 시작

### 7.3 향후 참고사항

**CompEquippableAbility 계열 사용 시:**
1. 항상 `<comps Inherit="False">` 사용
2. 부모 comps를 모두 명시적으로 정의
3. 주석으로 이유 설명

**모딩 일반 원칙:**
- 바닐라 무기의 구조를 참고
- VerbTracker를 가진 여러 컴포넌트 조합 시 주의
- LoadID 충돌 가능성 항상 염두

---

## 8. 참고 자료

### 8.1 관련 소스 파일
- `RimworldSource/Verse/CompEquippable.cs`
- `RimworldSource/RimWorld/CompEquippableAbility.cs`
- `RimworldSource/RimWorld/CompEquippableAbilityReloadable.cs`
- `RimworldSource/RimWorld/Ability.cs`
- `RimworldSource/Verse/VerbTracker.cs`
- `RimworldSource/Verse/Verb.cs`

### 8.2 관련 Def 파일
- `RimWorldData/Anomaly/Defs/ThingDefs_Misc/Weapons/Weapons_Ranged.xml` (HellcatBurner)
- `RimWorldData/Odyssey/Defs/ThingDefs_Items/Weapons_Unique.xml` (Odyssey 무기들)

### 8.3 프로젝트 내 관련 문서
- `Report/19_Verb_DuplicateLoadID_Analysis_Report.md` - 이전 분석
- `WorkFlow/45_Verb_DuplicateID_CompEquippableAbility_Analysis.md` - 작업 진행 기록

---

**보고서 작성일**: 2025-11-12  
**작성자**: AI Assistant (Claude 4.5 Sonnet)  
**버전**: 1.0

