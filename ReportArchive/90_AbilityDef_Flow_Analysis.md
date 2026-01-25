# AbilityDef 플로우 분석 보고서

**작성일**: 2025-01-XX  
**분석 목적**: AbilityDef의 Charge 관리, Cooldown 관리, Ability 사용 플로우 전반적 분석

**태그**: AbilityDef Ability Charge Cooldown Flow Analysis

---

## 1. 클래스 구조

### 1.1 AbilityDef 클래스
- **위치**: `RimworldSource/RimWorld/AbilityDef.cs`
- **상속**: `Def`
- **역할**: 능력의 정의(Definition) - XML에서 설정 가능한 모든 속성 포함

### 1.2 Ability 클래스
- **위치**: `RimworldSource/RimWorld/Ability.cs`
- **인터페이스**: `IVerbOwner`, `IExposable`, `ILoadReferenceable`
- **역할**: 능력의 인스턴스 - 실제 게임 내에서 동작하는 능력 객체

---

## 2. AbilityDef 주요 속성

### 2.1 Charge 관련 속성
- **`charges`** (int, 기본값: -1)
  - AbilityDef에서 정의하는 최대 충전 수
  - `-1`이면 충전 시스템 미사용
  - `> 0`이면 `Ability.maxCharges`와 `Ability.charges` 초기화에 사용

### 2.2 Cooldown 관련 속성
- **`cooldownTicksRange`** (IntRange)
  - 쿨다운 틱 범위 (예: `2400` 또는 `180000~300000`)
  - `StartCooldown()` 호출 시 `RandomInRange`로 랜덤 값 선택

- **`cooldownPerCharge`** (bool, 기본값: false)
  - Charge당 쿨다운 적용 여부
  - `true`: 각 charge 사용 시마다 쿨다운 시작, 쿨다운 종료 시 charge 1개씩 회복
  - `false`: 모든 charge 소진 시 쿨다운 시작, 쿨다운 종료 시 모든 charge 회복

- **`hasExternallyHandledCooldown`** (bool, 기본값: false)
  - 외부에서 쿨다운을 관리하는 경우 (예: AbilityGroupDef)

- **`groupDef`** (AbilityGroupDef)
  - 능력 그룹 정의
  - 같은 그룹의 능력들은 공유 쿨다운 사용

- **`overrideGroupCooldown`** (bool, 기본값: false)
  - 그룹 쿨다운 대신 개별 쿨다운 사용 여부

### 2.3 Verb 관련 속성
- **`verbProperties`** (VerbProperties)
  - 능력 시전에 사용되는 Verb 설정
  - `verbClass`: Verb 클래스 타입 (예: `Verb_CastAbility`)
  - `range`: 사거리
  - `warmupTime`: 워밍업 시간
  - `targetParams`: 타겟팅 파라미터

### 2.4 기타 주요 속성
- **`abilityClass`** (Type, 기본값: `typeof(Ability)`)
  - Ability 인스턴스 생성 시 사용할 클래스 타입

- **`gizmoClass`** (Type, 기본값: `typeof(Command_Ability)`)
  - UI에 표시될 Gizmo 클래스 타입

- **`comps`** (List<AbilityCompProperties>)
  - 능력에 추가되는 컴포넌트 속성들
  - 예: `CompProperties_AbilityEffect`, `CompProperties_AbilityGiveHediff`

- **`aiCanUse`** (bool, 기본값: false)
  - AI가 사용 가능한지 여부

- **`targetRequired`** (bool, 기본값: true)
  - 타겟이 필요한지 여부

- **`casterMustBeCapableOfViolence`** (bool, 기본값: true)
  - 시전자가 폭력 행위 가능해야 하는지 여부

---

## 3. 전반적 플로우

### 3.1 초기화 플로우

**Ability 생성자 → Initialize()**
```csharp
public Ability(Pawn pawn, AbilityDef def)
{
    this.pawn = pawn;
    this.def = def;
    this.Initialize();
}
```

**Initialize() 주요 동작**:
1. **컴포넌트 초기화**
   - `def.comps`를 순회하며 각 `AbilityComp` 인스턴스 생성
   - 각 컴포넌트의 `Initialize()` 호출

2. **ID 할당**
   - `Id == -1`이면 `Find.UniqueIDsManager.GetNextAbilityID()`로 새 ID 할당

3. **VerbTracker 초기화**
   - `VerbTracker` 생성 (지연 초기화)
   - `IAbilityVerb` 인터페이스 구현 시 `Ability` 연결

4. **Charge 초기화**
   - `def.charges > 0`이면:
     - `maxCharges = def.charges`
     - `charges = maxCharges` (초기값은 최대값)

**ExposeData() (세이브/로드)**:
- `def`, `Id`, `verbTracker`, `inCooldown`, `cooldownEndTick`, `cooldownDuration`, `maxCharges`, `charges` 저장/로드
- 로드 시 `Initialize()` 재호출

---

### 3.2 Charge 관리 플로우

**Charge 소모**:
- **PreActivate()** (라인 541-606)
  - `UsesCharges`가 `true`이면 `charges--` 실행
  - Charge 소모는 능력 시전 전에 발생

**Charge 회복**:
1. **Cooldown 종료 시 자동 회복** (`CooldownTick()`, 라인 914-949)
   - `cooldownPerCharge == false`:
     - 쿨다운 종료 시 `charges = maxCharges` (모든 charge 회복)
   - `cooldownPerCharge == true`:
     - 쿨다운 종료 시 `charges++` (1개씩 회복)
     - `charges < maxCharges`이면 쿨다운 재시작

2. **외부에서 직접 설정**
   - `RemainingCharges` setter를 통해 직접 설정 가능
   - 예: `CompEquippableAbilityReloadable`에서 재장전 시

**Charge 확인**:
- **CanCast** (라인 192-239)
  - `UsesCharges`이고 `charges <= 0`이면 `false` 반환
  - `cooldownPerCharge == true`이고 쿨다운 중이면 `charges > 0`일 때만 사용 가능

- **GizmoDisabled()** (라인 765-835)
  - `UsesCharges`이고 `charges <= 0`이면 "AbilityNoCharges" 메시지 반환

---

### 3.3 Cooldown 관리 플로우

**Cooldown 시작** (`StartCooldown()`, 라인 1196-1205):
```csharp
public void StartCooldown(int ticks)
{
    this.inCooldown = true;
    this.cooldownEndTick = GenTicks.TicksGame + ticks;
    this.cooldownDuration = ticks;
    if (!this.def.cooldownPerCharge)
    {
        this.charges = this.maxCharges;  // ← cooldownPerCharge가 false면 charge 회복
    }
}
```

**Cooldown 시작 조건** (`PreActivate()`, 라인 547-590):
1. **그룹 쿨다운** (`groupDef != null`):
   - 그룹의 모든 능력에 쿨다운 적용
   - `overrideGroupCooldown`이 `true`이면 개별 쿨다운 사용

2. **Charge 기반 쿨다운** (`UsesCharges == true`):
   - `cooldownPerCharge == true`:
     - Charge 사용 후 `charges < maxCharges`이고 쿨다운이 없으면 쿨다운 시작
   - `cooldownPerCharge == false`:
     - `charges <= 0`일 때 쿨다운 시작

3. **일반 쿨다운** (`UsesCharges == false`):
   - 능력 사용 시 항상 쿨다운 시작

**Cooldown 진행** (`CooldownTick()`, 라인 914-949):
- 매 틱마다 `AbilityTick()` → `CooldownTick()` 호출
- `inCooldown == true`이고 `CooldownTicksRemaining <= 0`이면:
  - `inCooldown = false`
  - Charge 회복 로직 실행 (위 참조)
  - 알림 메시지/레터 전송 (설정된 경우)

**Cooldown 확인**:
- **OnCooldown** (라인 184-190): `HasCooldown && inCooldown`
- **HasCooldown** (라인 176-182): `cooldownTicksRange != default` 또는 `groupDef != null` 또는 `hasExternallyHandledCooldown`

---

### 3.4 Ability 사용 플로우

**1. 사용 가능 여부 확인** (`CanCast`, 라인 192-239):
- 컴포넌트들의 `CanCast` 확인
- 쿨다운 상태 확인
- Charge 확인 (`UsesCharges`일 때)
- 맵/바이옴 제약 확인 (포켓맵, 진공 등)
- 행성 레이어 제약 확인

**2. Gizmo 표시 여부 확인** (`GizmoDisabled()`, 라인 765-835):
- 쿨다운 상태 확인
- Charge 확인
- 컴포넌트들의 `GizmoDisabled()` 확인
- `CanCast` 확인
- Lord 제약 확인
- Draft 상태 확인
- Pawn 상태 확인 (Downed, Deathresting, Baby 등)

**3. 능력 시전** (`Activate()`, 라인 609-624):
```
Activate(target, dest)
  ↓
PreActivate(target)
  ├─ Charge 소모 (charges--)
  ├─ Cooldown 시작
  └─ 기타 전처리
  ↓
EffectComps 적용
  ├─ GetAffectedTargets(target) - AOE 타겟 수집
  └─ ApplyEffects(EffectComps, targets, dest)
```

**4. PreActivate() 세부 동작** (라인 541-606):
- Charge 소모
- Cooldown 시작 (조건에 따라)
- 그룹 쿨다운 적용 (설정된 경우)
- Equipment에 알림 (`Notify_AbilityUsed()`)
- 전투 로그 기록 (설정된 경우)

**5. Verb 시전** (`QueueCastingJob()`, 라인 680-696):
- `CanQueueCast` 확인
- `CanApplyOn(target)` 확인
- 확인 다이얼로그 표시 (필요 시)
- Job 생성 및 큐에 추가

---

### 3.5 AbilityTick() 플로우

**AbilityTick()** (라인 837-862) - 매 틱마다 호출:
1. **VerbTracker.VerbsTick()** - Verb 업데이트
2. **이펙트 발산** (`emittedFleck` 설정 시)
3. **MoteTick()** - 워밍업 Mote 관리
4. **EffectersTick()** - 지속 이펙터 관리
5. **WarmupTick()** - 워밍업 진행 및 중단 처리
6. **CastingTick()** - 시전 중 이펙트 관리
7. **SoundTick()** - 사운드 관리
8. **CooldownTick()** - 쿨다운 진행 및 종료 처리
9. **컴포넌트 Tick** - 각 `AbilityComp.CompTick()` 호출
10. **lastCastTick 업데이트** - 시전 종료 시점 기록

---

## 4. 특수 조건 및 상호작용

### 4.1 cooldownPerCharge vs replenishAfterCooldown

**cooldownPerCharge** (AbilityDef 속성):
- Ability 레벨에서 관리
- Charge당 쿨다운 적용
- 쿨다운 종료 시 charge 자동 회복

**replenishAfterCooldown** (CompProperties_EquippableAbilityReloadable 속성):
- Comp 레벨에서 관리
- 쿨다운 종료 후 자동 충전
- Ammo 없이도 충전 가능 (주의 필요)

**충돌 가능성**:
- 두 속성이 모두 활성화되면 예상치 못한 동작 발생 가능
- Ammo 기반 재장전을 원할 경우 둘 다 `false`로 설정 권장

### 4.2 AbilityGroupDef 쿨다운

**그룹 쿨다운 동작**:
- 같은 `groupDef`를 가진 모든 능력이 공유 쿨다운 사용
- 한 능력 사용 시 그룹의 모든 능력에 쿨다운 적용
- `overrideGroupCooldown == true`이면 개별 쿨다운 사용

**예시**: `RK_AbilityGroup_MoraleBoost`
- `RK_Ability_MeleeMoraleBooster`가 이 그룹에 속함
- 그룹 쿨다운: 60000 ticks

### 4.3 Charge와 Cooldown의 관계

**시나리오 1: cooldownPerCharge == false, UsesCharges == true**
- Charge 사용 시마다 소모
- 모든 charge 소진 시 쿨다운 시작
- 쿨다운 종료 시 모든 charge 회복

**시나리오 2: cooldownPerCharge == true, UsesCharges == true**
- Charge 사용 시마다 소모
- Charge 사용 후 쿨다운 시작 (charge가 남아있어도)
- 쿨다운 종료 시 charge 1개 회복
- `maxCharges`에 도달할 때까지 반복

**시나리오 3: UsesCharges == false**
- Charge 시스템 미사용
- 능력 사용 시마다 쿨다운 시작
- 쿨다운 종료 시 다음 사용 가능

---

## 5. 주요 메서드 요약

### 5.1 초기화 관련
- **`Initialize()`**: Ability 초기화 (컴포넌트, ID, VerbTracker, Charge)
- **`ExposeData()`**: 세이브/로드

### 5.2 Charge 관련
- **`RemainingCharges`** (getter/setter): 현재 charge 수
- **`UsesCharges`**: Charge 시스템 사용 여부 (`maxCharges > 0`)

### 5.3 Cooldown 관련
- **`StartCooldown(int ticks)`**: 쿨다운 시작
- **`ResetCooldown()`**: 쿨다운 리셋
- **`CooldownTick()`**: 쿨다운 진행 및 종료 처리
- **`CooldownTicksRemaining`**: 남은 쿨다운 틱 수
- **`OnCooldown`**: 쿨다운 중 여부

### 5.4 사용 관련
- **`CanCast`**: 사용 가능 여부
- **`CanQueueCast`**: 큐에 추가 가능 여부
- **`Activate(LocalTargetInfo, LocalTargetInfo)`**: 능력 시전
- **`PreActivate(LocalTargetInfo?)`**: 시전 전처리
- **`QueueCastingJob(LocalTargetInfo, LocalTargetInfo)`**: 시전 Job 큐에 추가

### 5.5 Gizmo 관련
- **`GetGizmos()`**: Gizmo 목록 반환
- **`GizmoDisabled(out string reason)`**: Gizmo 비활성화 여부 및 이유

---

## 6. 플로우 다이어그램

### 6.1 Ability 초기화 플로우
```
Ability 생성
  ↓
Initialize()
  ├─ 컴포넌트 초기화
  ├─ ID 할당
  ├─ VerbTracker 초기화
  └─ Charge 초기화 (def.charges > 0일 때)
```

### 6.2 Ability 사용 플로우
```
사용자 클릭
  ↓
GizmoDisabled() 확인
  ├─ 쿨다운 확인
  ├─ Charge 확인
  ├─ CanCast 확인
  └─ 기타 제약 확인
  ↓
QueueCastingJob()
  ↓
Job 실행 (Verb 시전)
  ↓
Activate()
  ├─ PreActivate()
  │   ├─ Charge 소모
  │   └─ Cooldown 시작
  └─ EffectComps 적용
```

### 6.3 Charge & Cooldown 플로우
```
능력 사용
  ↓
PreActivate()
  ├─ charges-- (UsesCharges일 때)
  └─ StartCooldown() (조건에 따라)
      ├─ cooldownPerCharge == true: charge 사용 후 쿨다운
      └─ cooldownPerCharge == false: charge 소진 시 쿨다운
  ↓
CooldownTick() (매 틱)
  ├─ 쿨다운 진행
  └─ 쿨다운 종료 시
      ├─ cooldownPerCharge == true: charges++ (1개씩)
      └─ cooldownPerCharge == false: charges = maxCharges (전체)
```

---

## 7. 주의사항

### 7.1 Charge 초기화
- `def.charges > 0`일 때만 `maxCharges`와 `charges` 초기화
- 외부 컴포넌트(예: `CompEquippableAbilityReloadable`)에서 `maxCharges`를 덮어쓸 수 있음

### 7.2 Cooldown 시작 시점
- `cooldownPerCharge == true`: Charge 사용 후 즉시 쿨다운 시작 (charge가 남아있어도)
- `cooldownPerCharge == false`: 모든 charge 소진 시 쿨다운 시작

### 7.3 그룹 쿨다운
- 같은 그룹의 모든 능력에 쿨다운 적용
- `overrideGroupCooldown == true`로 개별 쿨다운 사용 가능

### 7.4 CompEquippableAbilityReloadable와의 상호작용
- `CompEquippableAbilityReloadable`는 `Ability.RemainingCharges`를 직접 조작
- `replenishAfterCooldown`과 `cooldownPerCharge`의 충돌 주의
- Ammo 기반 재장전을 원할 경우 둘 다 `false`로 설정 권장

---

## 8. 참고 자료

### 8.1 관련 소스코드
- `RimworldSource/RimWorld/AbilityDef.cs`
- `RimworldSource/RimWorld/Ability.cs`
- `RimworldSource/RimWorld/CompEquippableAbilityReloadable.cs`
- `RimworldSource/RimWorld/CompProperties_EquippableAbilityReloadable.cs`

### 8.2 관련 보고서
- `Report/88_CompEquippableAbilityReloadable_Flow_Analysis.md`
- `Report/87_WyvernFire_Ammo_AutoRecharge_Analysis.md`
- `Report/45_CompEquippable_VerbTracker_Duplication_Analysis_Report.md`

---

## 9. 결론

**AbilityDef는 능력의 정의를 담고 있으며, Ability 클래스가 실제 인스턴스를 관리합니다.**

**주요 특징**:
- Charge 시스템: `def.charges > 0`일 때 활성화, `cooldownPerCharge`로 동작 방식 제어
- Cooldown 시스템: `cooldownTicksRange`로 쿨다운 시간 설정, 그룹 쿨다운 지원
- 컴포넌트 시스템: `comps`를 통해 다양한 효과 추가 가능

**주의사항**:
- `cooldownPerCharge`와 `replenishAfterCooldown`의 충돌 가능성
- Charge 초기화는 `Initialize()`에서만 수행
- 그룹 쿨다운은 모든 그룹 멤버에 동시 적용
