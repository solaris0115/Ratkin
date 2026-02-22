# WyvernFire 어빌리티 Ammo 자동 충전 문제 분석 보고서

**작성일**: 2025-01-XX  
**분석 목적**: 어빌리티 cooldown 종료 시 ammo 없이 자동으로 충전되는 문제 원인 규명

---

## 1. 문제 정의

### 1.1 현상
- 발사 시 ammo가 정상적으로 줄어듦
- 어빌리티의 cooldown이 종료되면 ammo 없이 다시 충전되어 버림
- 재장전 로직과 어빌리티 cooldown 로직이 충돌하는 것으로 추정

### 1.2 관련 파일
- **무기 Def**: `Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml` (95-103줄)
- **어빌리티 Def**: `Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml` (253-295줄)

---

## 2. 코드 분석

### 2.1 CompEquippableAbilityReloadable의 CompTick 로직

**파일**: `RimworldSource/RimWorld/CompEquippableAbilityReloadable.cs` (113-129줄)

```csharp
public override void CompTick()
{
    base.CompTick();
    if (base.AbilityForReading == null)
    {
        return;
    }
    if (this.Props.replenishAfterCooldown && this.RemainingCharges == 0)
    {
        if (this.replenishInTicks > 0)
        {
            this.replenishInTicks--;
            return;
        }
        this.RemainingCharges = this.MaxCharges;  // ← 여기서 자동 충전!
    }
}
```

**동작 방식**:
- `replenishAfterCooldown`이 `true`이고 `RemainingCharges == 0`일 때
- `replenishInTicks`가 0이 되면 `MaxCharges`로 자동 충전
- `replenishInTicks`는 `UsedOnce()`에서 설정됨

### 2.2 UsedOnce() 메서드

**파일**: `RimworldSource/RimWorld/CompEquippableAbilityReloadable.cs` (288-298줄)

```csharp
public override void UsedOnce()
{
    if (base.AbilityForReading == null)
    {
        return;
    }
    if (this.Props.replenishAfterCooldown && this.RemainingCharges == 0)
    {
        this.replenishInTicks = this.Props.baseReloadTicks;  // ← baseReloadTicks로 설정
    }
}
```

**동작 방식**:
- 어빌리티 사용 시 호출됨
- `replenishAfterCooldown`이 `true`이고 charges가 0이면 `replenishInTicks`를 `baseReloadTicks`로 설정

### 2.3 Ability의 CooldownTick 로직

**파일**: `RimworldSource/RimWorld/Ability.cs` (914-949줄)

```csharp
private void CooldownTick()
{
    if (!this.inCooldown || !this.CanCooldown || this.CooldownTicksRemaining > 0)
    {
        return;
    }
    this.inCooldown = false;
    if (this.UsesCharges && this.def.cooldownPerCharge)  // ← cooldownPerCharge 체크
    {
        int a2 = this.charges + 1;
        this.charges = a2;
        this.charges = Mathf.Min(a2, this.maxCharges);  // ← 여기서 charge 증가!
        if (this.charges < this.maxCharges)
        {
            this.StartCooldown(this.def.cooldownTicksRange.RandomInRange);
        }
    }
    // ...
}
```

**동작 방식**:
- `cooldownPerCharge`가 `true`이고 `UsesCharges`가 `true`일 때
- cooldown이 끝날 때마다 charge를 1씩 증가시킴
- `maxCharges`에 도달할 때까지 반복

---

## 3. 문제 원인 분석

### 3.1 가능한 원인 1: replenishAfterCooldown이 true로 설정됨

**확인 필요 사항**:
- XML에 `replenishAfterCooldown`이 명시적으로 `false`로 설정되어 있는지 확인
- 기본값이 `false`이지만, 부모 Def에서 상속받았을 가능성

**현재 XML 상태**:
```xml
<li Class="CompProperties_EquippableAbilityReloadable">
    <abilityDef>RK_WyvernFire_Ability</abilityDef>
    <maxCharges>2</maxCharges>
    <soundReload>RK_Sound_Reload</soundReload>
    <chargeNoun>wyvern charge</chargeNoun>
    <ammoDef>RK_Ammo_WyvernFire</ammoDef>
    <ammoCountPerCharge>1</ammoCountPerCharge>
    <baseReloadTicks>60</baseReloadTicks>
    <!-- replenishAfterCooldown이 명시되지 않음 -->
</li>
```

**문제점**:
- `replenishAfterCooldown`이 명시되지 않아 기본값 `false`를 사용해야 함
- 하지만 실제 동작은 `true`처럼 보임

### 3.2 가능한 원인 2: cooldownPerCharge가 true로 설정됨

**확인 필요 사항**:
- 어빌리티 Def에 `cooldownPerCharge`가 명시적으로 `false`로 설정되어 있는지 확인

**현재 XML 상태**:
```xml
<AbilityDef>
    <defName>RK_WyvernFire_Ability</defName>
    <!-- ... -->
    <cooldownTicksRange>2400</cooldownTicksRange>
    <!-- cooldownPerCharge가 명시되지 않음 -->
</AbilityDef>
```

**문제점**:
- `cooldownPerCharge`가 명시되지 않아 기본값 `false`를 사용해야 함
- 하지만 실제 동작은 `true`처럼 보임

### 3.3 가능한 원인 3: 두 로직의 상호작용

**시나리오**:
1. 어빌리티 사용 → `UsedOnce()` 호출
2. `replenishAfterCooldown`이 `true`이고 charges가 0이면 `replenishInTicks` 설정
3. `CompTick()`에서 `replenishInTicks`가 0이 되면 자동 충전
4. 동시에 `Ability.CooldownTick()`에서 `cooldownPerCharge`가 `true`이면 charge 증가

**충돌 가능성**:
- 두 로직이 동시에 작동하여 예상치 못한 동작 발생 가능

---

## 4. 해결 방안

### 4.1 방안 1: replenishAfterCooldown을 명시적으로 false로 설정 (권장)

**수정 내용**:
```xml
<li Class="CompProperties_EquippableAbilityReloadable">
    <abilityDef>RK_WyvernFire_Ability</abilityDef>
    <maxCharges>2</maxCharges>
    <soundReload>RK_Sound_Reload</soundReload>
    <chargeNoun>wyvern charge</chargeNoun>
    <ammoDef>RK_Ammo_WyvernFire</ammoDef>
    <ammoCountPerCharge>1</ammoCountPerCharge>
    <baseReloadTicks>60</baseReloadTicks>
    <replenishAfterCooldown>False</replenishAfterCooldown>  <!-- 명시적 설정 -->
</li>
```

**효과**:
- cooldown 종료 시 자동 충전 방지
- ammo를 통한 재장전만 가능

### 4.2 방안 2: cooldownPerCharge를 명시적으로 false로 설정

**수정 내용**:
```xml
<AbilityDef>
    <defName>RK_WyvernFire_Ability</defName>
    <!-- ... -->
    <cooldownTicksRange>2400</cooldownTicksRange>
    <cooldownPerCharge>False</cooldownPerCharge>  <!-- 명시적 설정 -->
</AbilityDef>
```

**효과**:
- cooldown 종료 시 charge 자동 증가 방지
- ammo를 통한 재장전만 가능

### 4.3 방안 3: 두 설정 모두 명시적으로 false로 설정 (가장 확실)

**수정 내용**:
- `replenishAfterCooldown`을 `False`로 설정
- `cooldownPerCharge`를 `False`로 설정

**효과**:
- 모든 자동 충전 로직 비활성화
- ammo를 통한 재장전만 가능

---

## 5. 권장 사항

### 5.1 즉시 적용할 수정

1. **`replenishAfterCooldown`을 명시적으로 `False`로 설정**
   - 가장 가능성 높은 원인
   - XML에 명시적으로 추가하여 의도 명확화

2. **`cooldownPerCharge`를 명시적으로 `False`로 설정**
   - 추가 안전장치
   - 어빌리티 Def에 명시적으로 추가

### 5.2 테스트 항목

1. 어빌리티 사용 후 ammo 소모 확인
2. cooldown 종료 후 자동 충전 여부 확인
3. ammo를 통한 재장전 정상 작동 확인

---

## 6. 참고 자료

### 6.1 관련 소스코드

- `RimworldSource/RimWorld/CompEquippableAbilityReloadable.cs`
- `RimworldSource/RimWorld/CompProperties_EquippableAbilityReloadable.cs`
- `RimworldSource/RimWorld/Ability.cs`
- `RimworldSource/RimWorld/AbilityDef.cs`

### 6.2 관련 속성

- `replenishAfterCooldown`: cooldown 종료 시 자동 충전 여부
- `cooldownPerCharge`: charge당 cooldown 적용 여부
- `baseReloadTicks`: 재장전에 걸리는 틱 수
- `ammoCountPerCharge`: 충전당 필요한 ammo 수량

---

## 7. 결론

**문제 원인**: `replenishAfterCooldown` 또는 `cooldownPerCharge`가 예상치 못하게 활성화되어 cooldown 종료 시 자동 충전이 발생하는 것으로 추정

**해결 방법**: 두 속성을 모두 명시적으로 `False`로 설정하여 자동 충전 로직을 비활성화하고, ammo를 통한 재장전만 가능하도록 수정
