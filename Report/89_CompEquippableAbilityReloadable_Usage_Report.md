# CompEquippableAbilityReloadable 사용례 보고서

**작성일**: 2025-01-XX  
**분석 목적**: CompEquippableAbilityReloadable이 프로젝트에서 어디에 사용되었는지 정리

**태그**: CompEquippableAbilityReloadable Usage Examples Gunlance WyvernFire

---

## 1. 개요

`CompProperties_EquippableAbilityReloadable`은 어빌리티 기반 재장전 시스템을 제공하는 컴포넌트입니다. 프로젝트 내에서 총 **11개**의 무기에서 사용되고 있습니다.

---

## 2. 실제 사용 무기 목록

### 2.1 건랜스 계열 (Gunlance)

#### ✅ RK_Weapon_Gunlance (건랜스 일반형)
- **파일**: `Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml` (라인 95-104)
- **어빌리티**: `RK_WyvernFire_Ability`
- **설정**:
  ```xml
  <li Class="CompProperties_EquippableAbilityReloadable">
      <abilityDef>RK_WyvernFire_Ability</abilityDef>
      <maxCharges>2</maxCharges>
      <soundReload>RK_Sound_Reload</soundReload>
      <chargeNoun>wyvern charge</chargeNoun>
      <ammoDef>RK_Ammo_WyvernFire</ammoDef>
      <ammoCountPerCharge>1</ammoCountPerCharge>
      <baseReloadTicks>60</baseReloadTicks>
      <replenishAfterCooldown>False</replenishAfterCooldown>
  </li>
  ```
- **특징**: 
  - 용격포(Wyvern Fire) 어빌리티 사용
  - 전용 탄약 `RK_Ammo_WyvernFire` 필요
  - 최대 2발 충전 가능
  - 재사용 대기시간 2시간 (어빌리티 쿨다운)
  - `replenishAfterCooldown`이 `False`로 설정되어 수동 재장전만 가능

#### ✅ RK_Weapon_Gunlance_Unique (건랜스 유니크)
- **파일**: `Project/Odyssey/Defs/ThingDefs_Items/Weapon_Unique` (라인 31-39)
- **어빌리티**: `RK_WyvernFire_Ability`
- **설정**: 일반형과 동일한 설정
- **특징**: 유니크 무기 버전, 일반형과 동일한 어빌리티 사용

---

### 2.2 원거리 무기 유니크 버전들

다음 무기들은 `CompProperties_EquippableAbilityReloadable`을 포함하고 있지만, **어빌리티 정의가 명시되지 않은 빈 태그**로만 존재합니다:

#### ⚠️ RK_SniperRifle_Unique (저격 라이플 유니크)
- **파일**: `Project/Odyssey/Defs/ThingDefs_Items/Weapon_Unique` (라인 445)
- **설정**: `<li Class="CompProperties_EquippableAbilityReloadable" />` (빈 태그)
- **상태**: 어빌리티 정의 없음, 실제 기능 미사용 가능

#### ⚠️ RK_FlechetteRifle_Unique (플레셰트 라이플 유니크)
- **파일**: `Project/Odyssey/Defs/ThingDefs_Items/Weapon_Unique` (라인 489)
- **설정**: 빈 태그
- **상태**: 어빌리티 정의 없음

#### ⚠️ RK_FlechetteSniperRifle_Unique (플레셰트 저격 라이플 유니크)
- **파일**: `Project/Odyssey/Defs/ThingDefs_Items/Weapon_Unique` (라인 533)
- **설정**: 빈 태그
- **상태**: 어빌리티 정의 없음

#### ⚠️ RK_Rifle_Unique (라이플 유니크)
- **파일**: `Project/Odyssey/Defs/ThingDefs_Items/Weapon_Unique` (라인 657)
- **설정**: 빈 태그
- **상태**: 어빌리티 정의 없음

#### ⚠️ RK_Rifle_line_Unique (샷건 유니크)
- **파일**: `Project/Odyssey/Defs/ThingDefs_Items/Weapon_Unique` (라인 701)
- **설정**: 빈 태그
- **상태**: 어빌리티 정의 없음

#### ⚠️ RK_Weapon_Bolter_Unique (볼터건 유니크)
- **파일**: `Project/Odyssey/Defs/ThingDefs_Items/Weapon_Unique` (라인 748)
- **설정**: 빈 태그
- **상태**: 어빌리티 정의 없음

#### ⚠️ RK_PrototypePulseRifle_Unique (프로토타입 펄스 라이플 유니크)
- **파일**: `Project/Odyssey/Defs/ThingDefs_Items/Weapon_Unique` (라인 791)
- **설정**: 빈 태그
- **상태**: 어빌리티 정의 없음

#### ⚠️ RK_Weapon_BFR_Unique (BFR 3000 유니크)
- **파일**: `Project/Odyssey/Defs/ThingDefs_Items/Weapon_Unique` (라인 835)
- **설정**: 빈 태그
- **상태**: 어빌리티 정의 없음

#### ⚠️ RK_Weapon_RatHolicGun_Unique (RatHolic Gun 유니크)
- **파일**: `Project/Odyssey/Defs/ThingDefs_Items/Weapon_Unique` (라인 879)
- **설정**: 빈 태그
- **상태**: 어빌리티 정의 없음

---

## 3. 사용 통계

### 3.1 전체 사용 현황
- **총 사용 횟수**: 11개 무기
- **실제 기능 사용**: 2개 (건랜스 일반형, 건랜스 유니크)
- **빈 태그만 존재**: 9개 (원거리 무기 유니크 버전들)

### 3.2 무기 타입별 분류
- **근접 무기**: 2개 (건랜스 계열)
- **원거리 무기**: 9개 (모두 유니크 버전, 빈 태그)

### 3.3 어빌리티 사용 현황
- **RK_WyvernFire_Ability**: 2개 무기에서 사용
- **어빌리티 미정의**: 9개 무기

---

## 4. 실제 작동하는 사용례 상세 분석

### 4.1 RK_Weapon_Gunlance (건랜스 일반형)

**무기 정보**:
- **타입**: 근접 무기 (Melee)
- **부모**: `RK_MeleeWeapon`
- **기술 레벨**: Industrial
- **무기 태그**: `RK_Gunlance`, `RK_EliteDefender`, `RK_WeaponTag_TwoHand`

**어빌리티 정보**:
- **어빌리티 Def**: `RK_WyvernFire_Ability`
- **설명**: 전방 원뿔 범위에 폭발 데미지를 가하는 용격포
- **쿨다운**: 2400 틱 (2시간)
- **사거리**: 5.5 타일
- **데미지**: 35 (Bomb 타입)
- **관통력**: 0.7

**재장전 시스템**:
- **최대 충전**: 2발
- **탄약**: `RK_Ammo_WyvernFire`
- **충전당 탄약**: 1개
- **재장전 시간**: 60 틱
- **자동 충전**: 비활성화 (`replenishAfterCooldown: False`)

**사용 흐름**:
1. 무기 장착 시 최대 충전 상태로 시작
2. 어빌리티 사용 시 charge 1개 소모
3. charge가 0이 되면 탄약을 사용하여 재장전 필요
4. 재장전 작업 수행 (60 틱 소요)
5. 어빌리티 쿨다운 종료 후에도 자동 충전되지 않음 (수동 재장전만 가능)

---

## 5. 문제점 및 개선 사항

### 5.1 빈 태그 문제

**문제**: 9개의 유니크 무기가 `CompProperties_EquippableAbilityReloadable`을 빈 태그로만 포함하고 있음

**영향**:
- 컴포넌트는 생성되지만 어빌리티가 없어 실제 기능 작동 안 함
- 불필요한 컴포넌트 생성으로 인한 성능 낭비 가능성
- 코드 가독성 저하

**권장 사항**:
1. **옵션 1**: 어빌리티를 정의하여 실제 기능 활성화
2. **옵션 2**: 어빌리티가 필요 없는 경우 컴포넌트 제거
3. **옵션 3**: 부모 Def에서 상속받도록 변경 (현재는 `Inherit="False"`로 명시적 정의)

### 5.2 부모 무기와의 관계

**현재 상태**:
- 유니크 무기들은 `Inherit="False"`로 설정되어 부모의 comps를 상속받지 않음
- 부모 무기들(`RK_WeaponAttr_SniperRifle`, `RK_WeaponAttr_Rifle` 등)은 `CompProperties_EquippableAbilityReloadable`을 포함하지 않음
- 유니크 버전에서만 빈 태그로 추가됨

**의도 추정**:
- 향후 어빌리티 추가를 위한 플레이스홀더로 보임
- 또는 부모 무기에서 상속받을 예정이었으나 미완성 상태

---

## 6. 참고 사항

### 6.1 RimWorld 바닐라 예시

**Gun_HellcatBurner** (Anomaly DLC):
- `CompProperties_EquippableAbilityReloadable` 사용
- `HellcatBurner` 어빌리티 사용
- `Bioferrite` 탄약 사용
- `ammoCountPerCharge: 10`
- `<comps Inherit="False">` 사용 (표준 패턴)

### 6.2 프로젝트 내 다른 재장전 시스템

**Comp_RatHolicGun**:
- 별도의 재장전 시스템 사용
- `CompProperties_EquippableAbilityReloadable` 미사용
- 커스텀 재장전 로직 구현

---

## 7. 결론

### 7.1 실제 사용 현황
- **실제 작동**: 건랜스 계열 2개 무기만 실제로 어빌리티를 사용하여 재장전 기능 작동
- **미완성**: 9개의 유니크 무기는 컴포넌트만 포함하고 어빌리티 정의 없음

### 7.2 권장 사항
1. **빈 태그 정리**: 어빌리티가 필요 없는 유니크 무기에서 컴포넌트 제거
2. **어빌리티 추가**: 필요하다면 각 유니크 무기에 맞는 어빌리티 정의 추가
3. **문서화**: 향후 어빌리티 추가 예정인 경우 주석으로 명시

### 7.3 사용 패턴
- **성공 사례**: 건랜스 계열 - 완전한 설정으로 정상 작동
- **미완성 사례**: 원거리 무기 유니크 버전들 - 컴포넌트만 존재, 기능 미사용

---

## 8. 관련 파일

### 8.1 Def 파일
- `Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml` - 건랜스 일반형
- `Project/Odyssey/Defs/ThingDefs_Items/Weapon_Unique` - 유니크 무기들
- `Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml` - RK_WyvernFire_Ability 정의

### 8.2 분석 보고서
- `Report/88_CompEquippableAbilityReloadable_Flow_Analysis.md` - 플로우 분석
- `Report/87_WyvernFire_Ammo_AutoRecharge_Analysis.md` - 자동 충전 문제 분석
- `Report/45_CompEquippable_VerbTracker_Duplication_Analysis_Report.md` - VerbTracker 중복 문제

---

## 9. 사용례 요약표

| 무기 이름 | 파일 위치 | 어빌리티 | 상태 | 비고 |
|---------|---------|---------|------|------|
| RK_Weapon_Gunlance | Weapon_HighTech.xml:95 | RK_WyvernFire_Ability | ✅ 작동 | 건랜스 일반형 |
| RK_Weapon_Gunlance_Unique | Weapon_Unique:31 | RK_WyvernFire_Ability | ✅ 작동 | 건랜스 유니크 |
| RK_SniperRifle_Unique | Weapon_Unique:445 | 없음 | ⚠️ 빈 태그 | 어빌리티 미정의 |
| RK_FlechetteRifle_Unique | Weapon_Unique:489 | 없음 | ⚠️ 빈 태그 | 어빌리티 미정의 |
| RK_FlechetteSniperRifle_Unique | Weapon_Unique:533 | 없음 | ⚠️ 빈 태그 | 어빌리티 미정의 |
| RK_Rifle_Unique | Weapon_Unique:657 | 없음 | ⚠️ 빈 태그 | 어빌리티 미정의 |
| RK_Rifle_line_Unique | Weapon_Unique:701 | 없음 | ⚠️ 빈 태그 | 어빌리티 미정의 |
| RK_Weapon_Bolter_Unique | Weapon_Unique:748 | 없음 | ⚠️ 빈 태그 | 어빌리티 미정의 |
| RK_PrototypePulseRifle_Unique | Weapon_Unique:791 | 없음 | ⚠️ 빈 태그 | 어빌리티 미정의 |
| RK_Weapon_BFR_Unique | Weapon_Unique:835 | 없음 | ⚠️ 빈 태그 | 어빌리티 미정의 |
| RK_Weapon_RatHolicGun_Unique | Weapon_Unique:879 | 없음 | ⚠️ 빈 태그 | 어빌리티 미정의 |

---

**작성 완료**: 2025-01-XX
