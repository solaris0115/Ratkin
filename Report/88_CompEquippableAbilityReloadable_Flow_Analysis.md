# CompEquippableAbilityReloadable 플로우 분석 보고서

**작성일**: 2025-01-XX  
**분석 목적**: CompEquippableAbilityReloadable의 탄 충전/소모, 쿨다운, 재장전 로직 전반적 플로우 분석

**태그**: CompEquippableAbilityReloadable Reload Ammo Charge Cooldown Flow Analysis

---

## 1. 클래스 구조

### 1.1 상속 구조
```
ThingComp
  └─ CompEquippable (VerbTracker 소유)
       └─ CompEquippableAbility (Ability 추가)
            └─ CompEquippableAbilityReloadable (재장전 기능 추가)
```

### 1.2 인터페이스 구현
- `IReloadableComp`: 재장전 가능한 컴포넌트 인터페이스
- `ICompWithCharges`: Charge를 가진 컴포넌트 인터페이스

---

## 2. 주요 속성 및 설정

### 2.1 CompProperties_EquippableAbilityReloadable 속성
- `maxCharges`: 최대 충전 수
- `ammoDef`: 탄약 정의 (ThingDef)
- `ammoCountToRefill`: 전체 재충전에 필요한 탄약 수 (0이면 ammoCountPerCharge 사용)
- `ammoCountPerCharge`: 충전당 필요한 탄약 수
- `baseReloadTicks`: 재장전에 걸리는 틱 수 (기본값: 60)
- `replenishAfterCooldown`: 쿨다운 종료 시 자동 충전 여부 (기본값: false)
- `soundReload`: 재장전 사운드
- `chargeNoun`: Charge 명칭 (번역용)

### 2.2 Charge 관리
- `RemainingCharges`: 현재 남은 충전 수 (Ability.RemainingCharges와 동기화)
- `MaxCharges`: 최대 충전 수 (Props.maxCharges)

---

## 3. 전반적 플로우

### 3.1 초기화 플로우

**PostPostMake()**
- `Notify_PropsChanged()` 호출
- Ability의 `maxCharges` 설정
- `RemainingCharges`를 `MaxCharges`로 초기화

**Notify_PropsChanged()**
- Ability가 없으면 종료
- Ability의 `maxCharges` 업데이트
- `RemainingCharges`를 `MaxCharges`로 설정

### 3.2 어빌리티 사용 플로우

**어빌리티 사용 시퀀스:**
1. `Verb_CastAbility.TryCastShot()` 성공
2. `Ability.ConsumeCharges()` 호출 → `Ability.RemainingCharges` 감소
3. `CompEquippableAbilityReloadable.UsedOnce()` 호출
   - `replenishAfterCooldown == true`이고 `RemainingCharges == 0`이면
   - `replenishInTicks = baseReloadTicks` 설정
4. `Ability.StartCooldown()` 호출 → 쿨다운 시작

**UsedOnce() 메서드:**
- 어빌리티 사용 후 호출됨
- `replenishAfterCooldown`이 활성화되어 있고 charges가 0이면 재충전 타이머 시작

### 3.3 Charge 소모 로직

**Charge 소모는 Ability 클래스에서 처리:**
- `Ability.ConsumeCharges()`: 어빌리티 사용 시 charge 소모
- `Ability.RemainingCharges`가 직접 감소
- `CompEquippableAbilityReloadable.RemainingCharges`는 Ability의 charge를 읽기/쓰기하는 프로퍼티

**중요**: CompEquippableAbilityReloadable은 charge를 직접 소모하지 않음. Ability가 소모하고, Comp는 이를 읽어서 표시/관리함.

### 3.4 쿨다운 처리

**두 가지 쿨다운 시스템:**

1. **Ability 쿨다운** (Ability 클래스)
   - `Ability.StartCooldown()`: 어빌리티 사용 후 쿨다운 시작
   - `Ability.CooldownTick()`: 매 틱마다 쿨다운 감소
   - `cooldownPerCharge == true`이면 쿨다운 종료 시 charge 자동 증가 (1씩)

2. **재충전 쿨다운** (CompEquippableAbilityReloadable)
   - `replenishAfterCooldown == true`일 때만 작동
   - `replenishInTicks`: 재충전까지 남은 틱 수
   - `CompTick()`에서 매 틱마다 감소
   - 0이 되면 `RemainingCharges = MaxCharges`로 자동 충전

**CompTick() 로직:**
- `replenishAfterCooldown && RemainingCharges == 0`일 때
- `replenishInTicks > 0`이면 감소
- `replenishInTicks == 0`이면 `MaxCharges`로 충전

### 3.5 재장전 플로우

**재장전 트리거:**
- `IReloadableComp` 인터페이스를 통해 WorkGiver가 감지
- `NeedsReload()`가 `true`를 반환하면 재장전 작업 생성

**NeedsReload() 조건:**
- Ability가 없으면 `false`
- `ammoDef`가 없으면 `false`
- `ammoCountToRefill != 0`: 전체 재충전 모드
  - `allowForcedReload == false`: `RemainingCharges == 0`일 때만 필요
  - `allowForcedReload == true`: `RemainingCharges != MaxCharges`일 때 필요
- `ammoCountToRefill == 0`: 부분 재충전 모드 (ammoCountPerCharge 사용)
  - `allowForcedReload == false`: `RemainingCharges == 0`일 때만 필요
  - `allowForcedReload == true`: `RemainingCharges != MaxCharges`일 때 필요

**ReloadFrom() 메서드:**
- `NeedsReload(true)`가 `false`면 종료
- `ammoCountToRefill != 0`: 전체 재충전 모드
  - `ammo.stackCount < ammoCountToRefill`이면 종료
  - 탄약 소모 후 `RemainingCharges = MaxCharges`
- `ammoCountToRefill == 0`: 부분 재충전 모드
  - `ammo.stackCount < ammoCountPerCharge`이면 종료
  - 가능한 만큼 충전 (최대 `MaxCharges - RemainingCharges`까지)
  - 탄약 소모량 = 충전 수 × `ammoCountPerCharge`
- 재장전 사운드 재생 (`soundReload`)

### 3.6 어빌리티 사용 가능 여부 체크

**CanBeUsed() 메서드:**
- Ability가 없으면 `false`
- `RemainingCharges <= 0`이면 `false` (이유 반환)
- 그 외에는 `true`

**DisabledReason() 메서드:**
- `replenishAfterCooldown == true`: 쿨다운 메시지 반환
- `ammoDef == null`: Charge 부족 메시지 반환
- 그 외: 탄약 필요 메시지 반환 (필요 수량 포함)

---

## 4. 주요 함수 요약

### 4.1 초기화 관련
- `PostPostMake()`: 생성 후 초기화
- `Notify_PropsChanged()`: Props 변경 시 호출, Ability 설정 동기화

### 4.2 Charge 관리
- `RemainingCharges` (getter/setter): Ability의 charge와 동기화
- `MaxCharges` (getter): Props.maxCharges 반환

### 4.3 재장전 관련
- `NeedsReload()`: 재장전 필요 여부 확인
- `ReloadFrom()`: 탄약으로부터 재장전 수행
- `MinAmmoNeeded()`: 최소 필요 탄약 수
- `MaxAmmoNeeded()`: 최대 필요 탄약 수
- `MaxAmmoAmount()`: 최대 탄약 수용량

### 4.4 사용 관련
- `UsedOnce()`: 어빌리티 사용 후 호출, 재충전 타이머 설정
- `CanBeUsed()`: 사용 가능 여부 확인
- `DisabledReason()`: 사용 불가 이유 반환

### 4.5 틱 처리
- `CompTick()`: 매 틱마다 호출, 재충전 타이머 처리

### 4.6 UI 표시
- `CompInspectStringExtra()`: 인스펙트 패널에 charge 표시
- `SpecialDisplayStats()`: 스탯 패널에 charge 정보 표시
- `CompGetEquippedGizmosExtra()`: 개발 모드에서 재장전 Gizmo 제공

---

## 5. 특수 조건 및 주의사항

### 5.1 재장전 모드

**전체 재충전 모드 (`ammoCountToRefill != 0`):**
- 항상 `MaxCharges`까지 충전
- 탄약이 부족하면 재장전 불가
- `allowForcedReload == false`일 때는 charges가 0일 때만 재장전 가능

**부분 재충전 모드 (`ammoCountToRefill == 0`):**
- 가능한 만큼 부분 충전 가능
- 탄약이 부족해도 일부만 충전 가능
- `allowForcedReload == false`일 때는 charges가 0일 때만 재장전 가능

### 5.2 자동 충전 시스템

**replenishAfterCooldown:**
- `true`일 때: 쿨다운 종료 후 자동 충전 (ammo 불필요)
- `false`일 때: 수동 재장전만 가능 (ammo 필요)

**주의사항:**
- `replenishAfterCooldown == true`이면 ammo 없이도 충전 가능
- `replenishInTicks`는 `UsedOnce()`에서만 설정됨 (charges가 0일 때)
- `CompTick()`에서 매 틱마다 감소하여 자동 충전

### 5.3 Ability 쿨다운과의 상호작용

**cooldownPerCharge (Ability 속성):**
- `true`일 때: 쿨다운 종료 시 charge 자동 증가 (1씩)
- `false`일 때: 쿨다운만 작동, charge는 수동 재장전 필요

**충돌 가능성:**
- `replenishAfterCooldown`과 `cooldownPerCharge`가 동시에 활성화되면 두 시스템이 경쟁할 수 있음
- 일반적으로 둘 중 하나만 사용하는 것이 권장됨

### 5.4 Charge 소모 시점

**중요**: Charge 소모는 CompEquippableAbilityReloadable이 아닌 Ability 클래스에서 처리됨
- `Ability.ConsumeCharges()`: 어빌리티 사용 시 호출
- `CompEquippableAbilityReloadable`은 charge를 읽어서 표시/관리만 함
- `RemainingCharges` 프로퍼티는 `Ability.RemainingCharges`를 래핑

### 5.5 CompEquippableAbilityReloadable → Ability 값 전달 메커니즘

**Notify_PropsChanged()** (라인 103-111):
```csharp
public void Notify_PropsChanged()
{
    if (base.AbilityForReading == null) { return; }
    base.AbilityForReading.maxCharges = this.MaxCharges;  // ← Ability의 maxCharges 설정
    this.RemainingCharges = this.MaxCharges;              // ← Ability의 charges를 MaxCharges로 설정
}
```

**호출 시점**:
- `PostPostMake()`: 무기 생성 시
- Props 변경 시 (수동 호출)

**RemainingCharges setter** (라인 82-88):
```csharp
set
{
    if (base.AbilityForReading != null)
    {
        base.AbilityForReading.RemainingCharges = value;  // ← Ability의 charges 직접 설정
    }
}
```

**중요**: `RemainingCharges` setter는 `Ability.RemainingCharges`를 직접 조작합니다.

### 5.6 쿨다운과 재장전 충돌 문제

**문제 시나리오 1: Ability.StartCooldown()과 재장전 충돌**

**Ability.StartCooldown()** (라인 1196-1205):
```csharp
public void StartCooldown(int ticks)
{
    this.inCooldown = true;
    this.cooldownEndTick = GenTicks.TicksGame + ticks;
    this.cooldownDuration = ticks;
    if (!this.def.cooldownPerCharge)  // ← cooldownPerCharge == false일 때
    {
        this.charges = this.maxCharges;  // ← 쿨다운 시작 시 charge를 maxCharges로 설정!
    }
}
```

**충돌 상황**:
1. 사용자가 ammo로 재장전 완료 → `RemainingCharges = 2`
2. 능력 사용 → `charges = 1`
3. `PreActivate()`에서 `StartCooldown()` 호출
4. `cooldownPerCharge == false`이면 → `charges = maxCharges` (2로 복구!)
5. **결과**: 쿨다운 시작 시 charge가 자동으로 회복되어 버림

**문제 시나리오 2: Ability.CooldownTick()과 replenishAfterCooldown 충돌**

**Ability.CooldownTick()** (라인 921-930):
```csharp
if (this.UsesCharges && this.def.cooldownPerCharge)  // ← cooldownPerCharge == true일 때
{
    int a2 = this.charges + 1;
    this.charges = a2;
    this.charges = Mathf.Min(a2, this.maxCharges);  // ← 쿨다운 종료 시 charge 1개 증가
    if (this.charges < this.maxCharges)
    {
        this.StartCooldown(this.def.cooldownTicksRange.RandomInRange);  // ← 다시 쿨다운 시작
    }
}
```

**CompEquippableAbilityReloadable.CompTick()** (라인 120-128):
```csharp
if (this.Props.replenishAfterCooldown && this.RemainingCharges == 0)
{
    if (this.replenishInTicks > 0)
    {
        this.replenishInTicks--;
        return;
    }
    this.RemainingCharges = this.MaxCharges;  // ← 자동 충전 (ammo 없이!)
}
```

**충돌 상황**:
1. `cooldownPerCharge == true`이고 `replenishAfterCooldown == true`일 때
2. 쿨다운 종료 시 `Ability.CooldownTick()`에서 charge 1개 증가
3. 동시에 `CompEquippableAbilityReloadable.CompTick()`에서도 자동 충전 시도
4. **결과**: 두 시스템이 경쟁하여 예상치 못한 동작 발생

**문제 시나리오 3: StartCooldown()과 ReloadFrom() 타이밍 문제**

**시나리오**:
1. Charge 소진 → `charges = 0`
2. 사용자가 ammo로 재장전 시작
3. 재장전 중에 `Ability.StartCooldown()` 호출 (다른 트리거로)
4. `cooldownPerCharge == false`이면 → `charges = maxCharges` (재장전 무시!)
5. 재장전 완료 → `ReloadFrom()`에서 `RemainingCharges = MaxCharges` (중복 충전)

**문제 시나리오 4: Notify_PropsChanged()와 기존 charge 값 덮어쓰기**

**시나리오**:
1. 게임 중 무기 Props 변경 (예: `maxCharges` 증가)
2. `Notify_PropsChanged()` 호출
3. `this.RemainingCharges = this.MaxCharges` 실행
4. **결과**: 기존 charge 값이 무조건 `MaxCharges`로 덮어써짐
5. 사용자가 재장전한 charge가 사라질 수 있음

### 5.7 권장 해결 방법

**1. cooldownPerCharge와 replenishAfterCooldown 동시 사용 금지**:
- Ammo 기반 재장전을 원할 경우 둘 다 `false`로 설정
- `cooldownPerCharge == false`이면 `StartCooldown()`에서 charge를 자동 회복하므로 주의

**2. StartCooldown()의 charge 자동 회복 로직 주의**:
- `cooldownPerCharge == false`일 때 `StartCooldown()`은 charge를 `maxCharges`로 설정
- 이는 ammo 기반 재장전과 충돌할 수 있음
- **해결**: `cooldownPerCharge`를 명시적으로 `false`로 설정하고, `replenishAfterCooldown`도 `false`로 설정

**3. Notify_PropsChanged() 호출 시점 주의**:
- 게임 중 Props 변경 시 기존 charge 값이 덮어써질 수 있음
- 필요 시 기존 charge 값을 보존하는 로직 추가 고려

**4. 재장전 중 쿨다운 시작 방지**:
- 재장전 작업 중에는 쿨다운 시작을 지연시키는 로직 고려

---

## 6. 놓친 부분 및 추가 확인 사항

### 6.1 확인된 부분
✅ Charge 소모 로직 (Ability에서 처리)  
✅ 재장전 로직 (ReloadFrom)  
✅ 쿨다운 처리 (두 가지 시스템)  
✅ 자동 충전 시스템 (replenishAfterCooldown)  
✅ 재장전 필요 여부 체크 (NeedsReload)

### 6.2 추가 확인 필요 사항

**1. WorkGiver 연동:**
- `IReloadableComp` 인터페이스를 구현하여 WorkGiver가 재장전 작업을 생성하는지 확인 필요
- WorkGiver가 `NeedsReload()`를 호출하는지 확인 필요

**2. Ability 초기화 시점:**
- `CompEquippableAbility`에서 Ability가 언제 생성되는지 확인 필요
- `PostPostMake()` 시점에 Ability가 이미 존재하는지 확인 필요

**3. Charge 소모 시점:**
- `Ability.ConsumeCharges()`가 정확히 언제 호출되는지 확인 필요
- `Verb_CastAbility.TryCastShot()` 성공 후인지 확인 필요

**4. 세이브/로드:**
- `replenishInTicks`는 `PostExposeData()`에서 저장/로드됨
- Ability의 charge는 별도로 저장/로드되는지 확인 필요

**5. 여러 무기 동시 장착:**
- Pawn이 여러 개의 재장전 가능 무기를 장착했을 때 동작 확인 필요

**6. 탄약 소모 검증:**
- `ReloadFrom()`에서 탄약 소모가 정확히 이루어지는지 확인 필요
- `SplitOff()` 후 `Destroy()` 호출이 정상 작동하는지 확인 필요

**7. 재장전 중 어빌리티 사용:**
- 재장전 중에 어빌리티를 사용할 수 있는지 확인 필요
- 재장전 작업이 취소되면 어떻게 되는지 확인 필요

---

## 7. 플로우 다이어그램

### 7.1 어빌리티 사용 플로우
```
어빌리티 사용 시도
  ↓
Verb_CastAbility.TryCastShot()
  ↓
Ability.ConsumeCharges() → RemainingCharges 감소
  ↓
CompEquippableAbilityReloadable.UsedOnce()
  ├─ replenishAfterCooldown && RemainingCharges == 0?
  │   └─ Yes → replenishInTicks = baseReloadTicks
  └─ No → 종료
  ↓
Ability.StartCooldown() → 쿨다운 시작
```

### 7.2 재장전 플로우
```
WorkGiver 또는 수동 재장전
  ↓
NeedsReload() 체크
  ├─ false → 재장전 불가
  └─ true → 재장전 작업 생성
      ↓
ReloadFrom(ammo) 호출
  ↓
NeedsReload(true) 재확인
  ├─ false → 종료
  └─ true → 탄약 확인
      ├─ ammoCountToRefill != 0 → 전체 재충전
      │   └─ 탄약 소모 → RemainingCharges = MaxCharges
      └─ ammoCountToRefill == 0 → 부분 재충전
          └─ 탄약 소모 → RemainingCharges 증가
  ↓
재장전 사운드 재생
```

### 7.3 자동 충전 플로우 (replenishAfterCooldown)
```
CompTick() 매 틱 호출
  ↓
replenishAfterCooldown && RemainingCharges == 0?
  ├─ No → 종료
  └─ Yes → replenishInTicks 체크
      ├─ replenishInTicks > 0 → replenishInTicks--
      └─ replenishInTicks == 0 → RemainingCharges = MaxCharges
```

---

## 8. 결론

### 8.1 핵심 메커니즘
1. **Charge 소모**: Ability 클래스에서 처리, Comp는 읽기/쓰기만 담당
2. **재장전**: `ReloadFrom()` 메서드로 탄약 소모 후 charge 증가
3. **쿨다운**: Ability 쿨다운과 재충전 쿨다운 두 가지 시스템 존재
4. **자동 충전**: `replenishAfterCooldown` 활성화 시 ammo 없이 자동 충전 가능

### 8.2 주의사항
- `replenishAfterCooldown`과 `cooldownPerCharge` 동시 사용 시 충돌 가능
- Charge 소모는 Comp가 아닌 Ability에서 처리됨
- 재장전 모드에 따라 동작이 다름 (전체 재충전 vs 부분 재충전)

### 8.3 추가 확인 필요
- WorkGiver 연동 메커니즘
- Ability 초기화 시점
- Charge 소모 정확한 시점
- 여러 무기 동시 장착 시 동작

---

## 9. 참고 파일

- `RimworldSource/RimWorld/CompEquippableAbilityReloadable.cs`
- `RimworldSource/RimWorld/CompProperties_EquippableAbilityReloadable.cs`
- `RimworldSource/RimWorld/CompEquippableAbility.cs`
- `RimworldSource/RimWorld/Ability.cs`
- `Report/87_WyvernFire_Ammo_AutoRecharge_Analysis.md`
