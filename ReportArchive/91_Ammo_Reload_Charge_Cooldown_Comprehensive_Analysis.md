# 탄약과 재장전, 쿨다운 연계 종합 분석 보고서

**작성일**: 2025-01-XX  
**분석 목적**: 탄약 기반 재장전 시스템과 Ability 쿨다운 시스템의 연계 및 충돌 지점 분석

**태그**: Ammo Reload Charge Cooldown Ability CompEquippableAbilityReloadable Flow Analysis WyvernFire Gunlance

---

## 1. 핵심 문제 요약

**현재 상황**: 건랜스 Wyvern Fire를 2발 발사 후 쿨다운이 끝나면 자동으로 재장전됨  
**예상 동작**: 탄약(ammo)을 사용해서 수동 재장전해야 함  
**원인**: `cooldownPerCharge == false`일 때 `Ability.StartCooldown()`이 호출되면 charge가 자동으로 `maxCharges`로 회복됨

---

## 2. 시스템 구성 요소

### 2.1 관련 시스템

1. **Ability 시스템** (RimWorld 코어)
   - Charge 관리 (최대 충전 수, 현재 충전 수)
   - Cooldown 관리 (쿨다운 시간, 쿨다운 종료 처리)
   - `cooldownPerCharge` 속성으로 쿨다운 동작 방식 제어

2. **CompEquippableAbilityReloadable 시스템** (RimWorld 코어)
   - 탄약 기반 재장전 (`ReloadFrom()`)
   - 자동 충전 시스템 (`replenishAfterCooldown`)
   - Charge 표시 및 관리

3. **WorkGiver 시스템** (RimWorld 코어)
   - `IReloadableComp` 인터페이스를 통해 재장전 작업 자동 생성

### 2.2 현재 설정 (건랜스 Wyvern Fire)

```xml
<!-- CompProperties_EquippableAbilityReloadable -->
<maxCharges>2</maxCharges>
<ammoDef>RK_Ammo_WyvernFire</ammoDef>
<ammoCountPerCharge>1</ammoCountPerCharge>
<replenishAfterCooldown>False</replenishAfterCooldown>

<!-- AbilityDef -->
<cooldownTicksRange>2400</cooldownTicksRange>
<cooldownPerCharge>False</cooldownPerCharge>
```

---

## 3. 전체 플로우

### 3.1 어빌리티 사용 플로우

```
사용자가 어빌리티 사용
  ↓
Verb_CastAbility.TryCastShot() 성공
  ↓
Ability.ConsumeCharges() 호출
  ├─ charges-- (예: 2 → 1)
  └─ CompEquippableAbilityReloadable.UsedOnce() 호출
      ├─ replenishAfterCooldown == true && charges == 0?
      │   └─ Yes → replenishInTicks = baseReloadTicks 설정
      └─ No → 종료
  ↓
Ability.PreActivate() 호출
  ├─ Charge 소모 확인 (이미 소모됨)
  └─ StartCooldown() 호출
      ├─ cooldownPerCharge == false?
      │   └─ Yes → charges = maxCharges (자동 회복!) ⚠️
      └─ cooldownPerCharge == true?
          └─ Yes → 쿨다운만 시작 (charge 회복 안 함)
  ↓
어빌리티 이펙트 발동
```

**핵심 문제**: `cooldownPerCharge == false`일 때 `StartCooldown()`이 호출되면 **즉시 charge가 `maxCharges`로 회복**됨

### 3.2 재장전 플로우

```
WorkGiver 또는 수동 재장전 명령
  ↓
CompEquippableAbilityReloadable.NeedsReload() 확인
  ├─ Ability 없음? → false
  ├─ ammoDef 없음? → false
  ├─ charges == MaxCharges? → false (재장전 불필요)
  └─ charges < MaxCharges? → true (재장전 필요)
  ↓
재장전 작업 생성 및 실행
  ↓
CompEquippableAbilityReloadable.ReloadFrom(ammo) 호출
  ├─ NeedsReload(true) 재확인
  ├─ 탄약 수량 확인
  ├─ 탄약 소모
  └─ RemainingCharges = MaxCharges (또는 부분 충전)
  ↓
재장전 완료
```

**정상 동작**: 탄약을 소모하여 charge를 회복

### 3.3 쿨다운 종료 플로우

```
매 틱마다 Ability.CooldownTick() 호출
  ↓
쿨다운 종료 확인 (cooldownEndTick <= 현재 틱)
  ├─ 쿨다운 종료 아님 → 종료
  └─ 쿨다운 종료됨
      ├─ cooldownPerCharge == true?
      │   └─ Yes → charges++ (1개씩 회복)
      │       └─ charges < maxCharges? → 쿨다운 재시작
      └─ cooldownPerCharge == false?
          └─ Yes → 쿨다운만 종료 (charge는 이미 StartCooldown()에서 회복됨)
```

**주의**: `cooldownPerCharge == false`일 때는 쿨다운 종료 시 charge 회복이 없음 (이미 `StartCooldown()`에서 회복됨)

### 3.4 자동 충전 플로우 (replenishAfterCooldown)

```
매 틱마다 CompEquippableAbilityReloadable.CompTick() 호출
  ↓
replenishAfterCooldown == true && RemainingCharges == 0?
  ├─ No → 종료
  └─ Yes → replenishInTicks 확인
      ├─ replenishInTicks > 0 → replenishInTicks--
      └─ replenishInTicks == 0 → RemainingCharges = MaxCharges (자동 충전)
```

**현재 설정**: `replenishAfterCooldown == False`이므로 이 플로우는 작동하지 않음

---

## 4. 충돌 지점 분석

### 4.1 충돌 시나리오 1: StartCooldown()과 재장전 충돌

**문제 상황**:
1. Charge 소진 → `charges = 0`
2. 사용자가 ammo로 재장전 시작
3. 재장전 중에 어빌리티 사용 (또는 다른 트리거로 `StartCooldown()` 호출)
4. `cooldownPerCharge == false`이면 → `charges = maxCharges` (재장전 무시!)
5. 재장전 완료 → `ReloadFrom()`에서 `RemainingCharges = MaxCharges` (중복 충전)

**실제 발생 시나리오** (건랜스 Wyvern Fire):
1. 어빌리티 사용 → `charges = 1`
2. 어빌리티 사용 → `charges = 0`
3. `PreActivate()`에서 `StartCooldown()` 호출
4. `cooldownPerCharge == false`이면 → `charges = 2` (자동 회복!)
5. **결과**: 탄약 없이도 charge가 회복되어 버림

### 4.2 충돌 시나리오 2: cooldownPerCharge와 replenishAfterCooldown 동시 사용

**문제 상황**:
- `cooldownPerCharge == true`이고 `replenishAfterCooldown == true`일 때
- 쿨다운 종료 시 `Ability.CooldownTick()`에서 charge 1개 증가
- 동시에 `CompEquippableAbilityReloadable.CompTick()`에서도 자동 충전 시도
- 두 시스템이 경쟁하여 예상치 못한 동작 발생

**현재 설정**: 둘 다 `False`이므로 충돌 없음

### 4.3 충돌 시나리오 3: Notify_PropsChanged()와 기존 charge 덮어쓰기

**문제 상황**:
1. 게임 중 무기 Props 변경 (예: `maxCharges` 증가)
2. `Notify_PropsChanged()` 호출
3. `this.RemainingCharges = this.MaxCharges` 실행
4. **결과**: 기존 charge 값이 무조건 `MaxCharges`로 덮어써짐

**현재 설정**: 게임 중 Props 변경이 없으므로 문제 없음

---

## 5. 핵심 문제: StartCooldown()의 자동 charge 회복

### 5.1 Ability.StartCooldown() 동작

**코드 로직** (의사코드):
```
StartCooldown(ticks):
  inCooldown = true
  cooldownEndTick = 현재 틱 + ticks
  cooldownDuration = ticks
  
  if (cooldownPerCharge == false):
    charges = maxCharges  // ← 여기서 자동 회복!
```

**의도**: `cooldownPerCharge == false`일 때는 모든 charge 소진 후 쿨다운 시작, 쿨다운 종료 시 모든 charge 회복

**문제**: 쿨다운 시작 시점에 charge를 회복하므로, 탄약 기반 재장전과 충돌

### 5.2 현재 설정에서의 동작

**건랜스 Wyvern Fire 설정**:
- `cooldownPerCharge == False`
- `replenishAfterCooldown == False`
- `ammoDef` 존재 (탄약 필요)

**실제 동작**:
1. 어빌리티 사용 → `charges = 1`
2. 어빌리티 사용 → `charges = 0`
3. `StartCooldown()` 호출 → `charges = 2` (자동 회복!)
4. 쿨다운 종료 → charge는 이미 회복되어 있음
5. **결과**: 탄약 없이도 charge가 회복됨

**예상 동작**:
1. 어빌리티 사용 → `charges = 1`
2. 어빌리티 사용 → `charges = 0`
3. `StartCooldown()` 호출 → 쿨다운만 시작 (charge 회복 안 함)
4. 쿨다운 종료 → charge는 여전히 0
5. 탄약으로 재장전 필요

---

## 6. 해결 방법

### 6.1 방법 1: cooldownPerCharge를 true로 변경 (권장하지 않음)

**설정 변경**:
```xml
<cooldownPerCharge>True</cooldownPerCharge>
```

**동작**:
- Charge 사용 후 쿨다운 시작 (charge가 남아있어도)
- 쿨다운 종료 시 charge 1개씩 회복
- 모든 charge 회복까지 쿨다운 반복

**문제점**:
- Charge당 쿨다운이 적용되어 사용 빈도가 낮아짐
- 탄약 기반 재장전과 여전히 충돌 가능성 있음

### 6.2 방법 2: AbilityDef에서 charges 제거 (권장)

**설정 변경**:
```xml
<!-- AbilityDef에서 charges 속성 제거 또는 0으로 설정 -->
<!-- CompEquippableAbilityReloadable에서만 charge 관리 -->
```

**동작**:
- `Ability.StartCooldown()`에서 charge 자동 회복 안 함
- `CompEquippableAbilityReloadable`에서만 charge 관리
- 탄약 기반 재장전만 작동

**주의사항**:
- `AbilityDef.charges`가 설정되어 있으면 `Ability.Initialize()`에서 `maxCharges` 설정됨
- `CompEquippableAbilityReloadable`이 `Notify_PropsChanged()`에서 덮어쓰지만, 초기화 시점 문제 가능

### 6.3 방법 3: AbilityDef에 charges를 -1로 설정 (권장)

**설정 변경**:
```xml
<!-- AbilityDef에 charges 속성 추가 -->
<charges>-1</charges>
```

**의미**:
- `charges == -1`이면 Ability 레벨에서 charge 시스템 미사용
- `CompEquippableAbilityReloadable`에서만 charge 관리
- `StartCooldown()`에서 charge 자동 회복 안 함

**동작**:
- `Ability.UsesCharges`가 `false` 반환
- `StartCooldown()`에서 charge 회복 로직 실행 안 함
- `CompEquippableAbilityReloadable`에서 charge 관리

### 6.4 방법 4: 커스텀 Ability 클래스 생성 (복잡함)

**방법**:
- `Ability` 클래스를 상속받아 `StartCooldown()` 오버라이드
- `cooldownPerCharge == false`일 때 charge 자동 회복 로직 제거

**단점**:
- 코드 수정 필요
- 유지보수 복잡도 증가

---

## 7. 권장 해결책

### 7.1 즉시 적용 가능한 해결책

**AbilityDef에 `charges` 속성 추가**:
```xml
<AbilityDef>
    <defName>RK_WyvernFire_Ability</defName>
    <!-- 기존 설정들... -->
    <charges>-1</charges>  <!-- 추가 -->
    <cooldownTicksRange>2400</cooldownTicksRange>
    <cooldownPerCharge>False</cooldownPerCharge>
    <!-- ... -->
</AbilityDef>
```

**효과**:
- `Ability.UsesCharges`가 `false` 반환
- `StartCooldown()`에서 charge 자동 회복 안 함
- `CompEquippableAbilityReloadable`에서만 charge 관리
- 탄약 기반 재장전만 작동

### 7.2 검증 방법

**테스트 시나리오**:
1. 건랜스 장착 → charge 2개 확인
2. 어빌리티 사용 → charge 1개 확인
3. 어빌리티 사용 → charge 0개 확인
4. 쿨다운 시작 → charge 여전히 0개 확인 (자동 회복 안 됨)
5. 쿨다운 종료 → charge 여전히 0개 확인
6. 탄약으로 재장전 → charge 2개 확인

---

## 8. 시스템 간 상호작용 요약

### 8.1 Charge 관리 주체

| 시스템 | Charge 관리 여부 | 관리 시점 |
|--------|-----------------|----------|
| Ability | `charges != -1`일 때 | 초기화, 사용 시 소모, 쿨다운 시 회복 |
| CompEquippableAbilityReloadable | 항상 | 재장전 시 회복, 표시 |

**충돌 지점**: 두 시스템이 동시에 charge를 관리하려고 할 때

### 8.2 쿨다운 관리 주체

| 시스템 | 쿨다운 관리 여부 | 관리 시점 |
|--------|-----------------|----------|
| Ability | 항상 | 어빌리티 사용 후, 쿨다운 종료 처리 |
| CompEquippableAbilityReloadable | `replenishAfterCooldown == true`일 때 | 재충전 타이머 관리 |

**충돌 지점**: `cooldownPerCharge`와 `replenishAfterCooldown` 동시 사용 시

### 8.3 재장전 관리 주체

| 시스템 | 재장전 관리 여부 | 관리 시점 |
|--------|-----------------|----------|
| WorkGiver | `IReloadableComp` 구현 시 | 재장전 필요 시 작업 생성 |
| CompEquippableAbilityReloadable | 항상 | `ReloadFrom()` 메서드로 재장전 수행 |

**정상 동작**: WorkGiver가 `NeedsReload()` 확인 후 재장전 작업 생성

---

## 9. 플로우 다이어그램

### 9.1 정상 동작 플로우 (권장 설정)

```
어빌리티 사용
  ↓
Ability.ConsumeCharges() → charges--
  ↓
Ability.PreActivate()
  └─ StartCooldown() 호출
      └─ charges == -1이면 charge 회복 안 함 ✅
  ↓
쿨다운 진행
  ↓
쿨다운 종료 → charge 여전히 0
  ↓
WorkGiver가 NeedsReload() 확인 → true
  ↓
재장전 작업 생성
  ↓
ReloadFrom(ammo) 호출
  └─ 탄약 소모 → RemainingCharges = MaxCharges ✅
```

### 9.2 현재 문제 플로우

```
어빌리티 사용
  ↓
Ability.ConsumeCharges() → charges--
  ↓
Ability.PreActivate()
  └─ StartCooldown() 호출
      └─ cooldownPerCharge == false이면 charges = maxCharges ⚠️
  ↓
쿨다운 진행 → charge는 이미 회복됨
  ↓
쿨다운 종료 → charge는 여전히 회복된 상태
  ↓
탄약 없이도 charge 사용 가능 (문제!)
```

---

## 10. 결론

### 10.1 핵심 문제

**`cooldownPerCharge == false`일 때 `Ability.StartCooldown()`이 호출되면 charge가 자동으로 `maxCharges`로 회복됨**

이로 인해:
- 탄약 기반 재장전이 무의미해짐
- 쿨다운만 지나면 자동으로 charge 회복
- 의도한 게임플레이와 다름

### 10.2 해결 방법

**AbilityDef에 `charges="-1"` 추가**:
- Ability 레벨에서 charge 시스템 비활성화
- `CompEquippableAbilityReloadable`에서만 charge 관리
- 탄약 기반 재장전만 작동

### 10.3 주의사항

1. **`cooldownPerCharge`와 `replenishAfterCooldown` 동시 사용 금지**
   - 둘 다 활성화 시 예상치 못한 동작 발생

2. **`AbilityDef.charges` 설정 주의**
   - `charges > 0`이면 Ability에서 charge 관리 시작
   - 탄약 기반 재장전을 원할 경우 `charges = -1` 권장

3. **초기화 시점 확인**
   - `CompEquippableAbilityReloadable.PostPostMake()`에서 `Notify_PropsChanged()` 호출
   - `Ability.Initialize()`보다 먼저 호출되는지 확인 필요

---

## 11. 참고 자료

### 11.1 관련 보고서
- `Report/90_AbilityDef_Flow_Analysis.md` - AbilityDef 플로우 분석
- `Report/88_CompEquippableAbilityReloadable_Flow_Analysis.md` - CompEquippableAbilityReloadable 플로우 분석
- `Report/89_CompEquippableAbilityReloadable_Usage_Report.md` - 사용례 보고서
- `Report/87_WyvernFire_Ammo_AutoRecharge_Analysis.md` - 자동 충전 문제 분석

### 11.2 관련 소스코드
- `RimworldSource/RimWorld/Ability.cs` - Ability 클래스
- `RimworldSource/RimWorld/AbilityDef.cs` - AbilityDef 클래스
- `RimworldSource/RimWorld/CompEquippableAbilityReloadable.cs` - 재장전 컴포넌트

### 11.3 관련 Def 파일
- `Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml` - 건랜스 및 어빌리티 정의

---

**작성 완료**: 2025-01-XX
