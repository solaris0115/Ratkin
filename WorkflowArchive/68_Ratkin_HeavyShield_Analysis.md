# 랫킨 모드 헤비 쉴드 분석 보고서

**태그**: Ratkin HeavyShield RK_HeavyShield CompStaminaShield 방패 분석

## 개요

랫킨 모드의 헤비 쉴드(`RK_HeavyShield`)는 스태미나 기반 방어 시스템을 사용하는 철제 방패입니다. 림월드 코어의 에너지 쉴드와 달리 완전 차단이 아닌 데미지 감소 방식을 사용하며, 근접/원거리/폭발 공격을 모두 처리할 수 있습니다.

## ThingClass 및 Comp 구성

### ThingClass
- **기본 클래스**: `Apparel`
- **ThingDef**: `RK_HeavyShield`
- **부모 클래스**: `RK_ApparelAttr_ShieldBase` (추상 부모)

### Comp 구성
1. **CompProperties_ExtraDrawer**: 조건부 드로잉 컴포넌트
   - 소집 시 팔/어깨 위치에 방패 표시
   - 비소집 시 등 위치에 방패 표시
   
2. **CompProperties_ShieldWeaponIncompatible**: 무기 호환성 제한 컴포넌트
   - 허용 무기 태그: `RK_WeaponTag_OneHand`, `RK_WeaponTag_LightShieldCompatible`
   - 호환되지 않는 무기 자동 제거
   
3. **CompProperties_StaminaShield**: 스태미나 쉴드 컴포넌트
   - **Comp 클래스**: `CompStaminaShield` (CompProperties_StaminaShield에서 자동 지정)

## 방어막 작동

### 데미지 흡수 과정

방어막은 `PostPreApplyDamage` 메서드를 통해 공격을 처리합니다. 작동 조건은 다음과 같습니다:

1. **기본 조건 확인**: 쉴드 상태가 Active이고 착용자가 존재해야 합니다.
2. **EMP 공격 처리**: EMP 공격은 통과시킵니다. 스태미나 감소 없이 데미지가 그대로 전달됩니다.
3. **쉴드 무시 속성 확인**: `ignoreShields` 속성을 가진 공격은 통과시킵니다. 스태미나 감소 없이 데미지가 그대로 전달됩니다.
4. **공격 타입별 처리**: 근접/원거리/폭발 공격을 모두 처리합니다.
   - **근접 공격**: `damageReductionPercentMelee` (70%)만큼 데미지 감소
   - **원거리 공격**: `damageReductionPercentRanged` (80%)만큼 데미지 감소
   - **폭발 공격**: `damageReductionPercentExplosive` (50%)만큼 데미지 감소
5. **스태미나 소모 계산**: 원래 데미지량에 비례하여 스태미나를 소모합니다 (감쇄 전 데미지 기준).
6. **스태미나 확인**: 스태미나가 0 이하가 되면 쉴드를 파괴합니다.
7. **데미지 처리**: 감쇄된 데미지만 차단하고, 나머지 데미지는 통과시킵니다. 감쇄된 데미지가 원래 데미지와 같거나 크면 `absorbed = true`를 반환하여 완전 차단합니다.

### 스태미나 소모

- **소모 공식**: `스태미나 -= 원래 데미지량 × staminaLossPerDamage`
- **근접 공격**: `staminaLossPerDamageMelee = 0.033` (데미지 30당 스태미나 0.99 소모)
- **원거리 공격**: `staminaLossPerDamageRanged = 0.033` (데미지 30당 스태미나 0.99 소모)
- **폭발 공격**: `staminaLossPerDamageExplosive = 0.7` (데미지 30당 스태미나 21.0 소모)
- **특징**: 원래 데미지량에 비례하여 스태미나가 소모되므로, 강한 공격일수록 더 많은 스태미나가 필요합니다. 감쇄 전 데미지를 기준으로 계산됩니다.

### 스태미나 충전

- **충전 조건**: 쉴드 상태가 Active일 때만 충전됩니다.
- **충전 속도**: 틱당 `RK_Stat_ShieldStaminaRechargeRate / 60`만큼 충전됩니다.
  - 헤비 쉴드: `0.35 / 60 = 0.00583` 스태미나/틱 (초당 0.35 스태미나)
- **최대 스태미나**: `RK_Stat_ShieldStamina`를 초과하지 않습니다.
  - 헤비 쉴드: 최대 1.5 스태미나
- **충전 중단**: 쉴드가 파괴되어 Resetting 상태가 되면 충전이 중단됩니다.

### 쉴드 상태

쉴드는 세 가지 상태를 가집니다:

1. **Active**: 스태미나가 있고 재충전 대기 시간이 0 이하인 상태. 공격을 차단하고 스태미나를 충전할 수 있습니다.
2. **Resetting**: 스태미나가 0이 되어 파괴된 후 재충전 대기 중인 상태. 공격을 차단할 수 없고 충전도 되지 않습니다. 기본 재충전 대기 시간은 3200틱(약 53초)입니다.
3. **Disabled**: 착용자가 충전 중이거나 셧다운 상태, 또는 Dormant 상태인 경우. 공격을 차단할 수 없습니다.

### 쉴드 파괴 및 재충전

- **파괴 조건**: 스태미나가 0 이하가 되면 쉴드가 파괴됩니다.
- **파괴 시 처리**: 스태미나를 0으로 설정하고 재충전 대기 시간을 시작합니다. 파괴 시각 효과를 표시합니다.
- **재충전 완료**: 재충전 대기 시간이 끝나면 초기 스태미나의 일부(기본값 20%)로 재충전됩니다.
- **재충전 후**: 재충전 완료 후 다시 Active 상태가 되어 스태미나를 충전할 수 있습니다.

### 데미지 감소율

헤비 쉴드는 공격 타입별로 다른 데미지 감소율을 적용합니다:

- **근접 공격**: 70% 감소 (`damageReductionPercentMelee = 0.7`)
  - 예: 데미지 30 → 9 데미지 통과 (21 데미지 차단)
- **원거리 공격**: 80% 감소 (`damageReductionPercentRanged = 0.8`)
  - 예: 데미지 30 → 6 데미지 통과 (24 데미지 차단)
- **폭발 공격**: 50% 감소 (`damageReductionPercentExplosive = 0.5`)
  - 예: 데미지 30 → 15 데미지 통과 (15 데미지 차단)

### Apparel 내구도 소모 조건

헤비 쉴드의 내구도 소모는 다음과 같습니다:

- **일일 착용 소모**: `wearPerDay = 0.2`로 설정되어 있어 착용만으로도 내구도가 소모됩니다.
- **데미지 차단 시**: CompStaminaShield가 공격을 처리한 경우, 감쇄된 데미지만 차단하고 나머지는 통과시킵니다. Apparel 자체에는 감쇄된 데미지에 해당하는 내구도 손상이 발생할 수 있습니다.
- **데미지 통과 시**: 차단하지 못한 공격(EMP 공격, `ignoreShields` 속성 공격, 쉴드 비활성 상태)이 통과되면 일반적인 방어구와 동일하게 내구도 손상이 발생합니다.
- **내구도 관련 설정**: 기본 설정을 따르며, 내구도가 낮아도 착용 가능합니다.

## 장착 관련

### UI 탭 분류

헤비 쉴드는 "의류(Apparel)" 탭에 표시됩니다.

- **분류 기준**: `ITab_Pawn_Gear`에서 `ApparelLayerDefOf.Belt` 레이어를 가진 Apparel은 "Equipment" 섹션에 표시하고, 그렇지 않은 Apparel은 "Apparel" 섹션에 표시합니다.
- **헤비 쉴드 설정**: `apparel/layers`에 `OffHand` 레이어가 포함되어 있어 Belt 레이어가 아니므로 의류 탭에 분류됩니다.
- **ShieldBelt와의 차이**: 림월드 코어의 `Apparel_ShieldBelt`는 `Belt` 레이어를 사용하여 "Equipment" 탭에 표시되지만, 랫킨 헤비 쉴드는 `OffHand` 레이어를 사용하여 "Apparel" 탭에 표시됩니다.
- **의미**: 방패는 일반 의류와 함께 의류 탭에서 관리되며, 무기와는 구분되어 표시됩니다.

### 무기 호환성 제한

헤비 쉴드는 특정 무기 태그를 가진 무기만 허용합니다:

- **허용 무기 태그**:
  - `RK_WeaponTag_OneHand`: 한손 무기
  - `RK_WeaponTag_LightShieldCompatible`: 경량 방패 호환 무기
- **제한 방식**: `CompShieldWeaponIncompatible`이 착용 시 호환되지 않는 무기를 자동으로 제거합니다.
- **메시지**: 무기 제거 시 경고 메시지가 표시됩니다.

### 드로잉 시스템

헤비 쉴드는 `CompExtraDrawer`를 통해 조건부 드로잉을 수행합니다:

- **소집 시**: 팔/어깨 위치에 `RK_HeavyShield` 그래픽 표시
  - 방향별 오프셋 및 각도 설정
- **비소집 시**: 등 위치에 `RK_HeavyShieldUnarm` 그래픽 표시
  - 방향별 오프셋 및 각도 설정

## 스탯 정보

### 기본 스탯

- **최대 내구도**: 240 HP
- **작업량**: 35,000 작업
- **무게**: 7 kg
- **방어력**: 
  - 날카로운 공격: 0.1 (`ArmorRating_Sharp`)
  - 둔한 공격: 0.0 (`ArmorRating_Blunt`)
- **가연성**: 0.3
- **장착 지연**: 1초

### 쉴드 스탯

- **최대 스태미나**: 1.5 (`RK_Stat_ShieldStamina`)
- **스태미나 충전 속도**: 초당 0.35 (`RK_Stat_ShieldStaminaRechargeRate`)

### 착용 시 스탯 보정

- **근접 회피 확률**: -10% (`MeleeDodgeChance = -0.1`)
- **이동 속도**: -10% (`MoveSpeed = -0.1`)

## 제작 정보

- **연구 필요**: `RK_Research_SwordAndShield`
- **제작 시설**: `RK_FueledSmithy`, `RK_ElectricSmithy`
- **재료**: 금속류 (`Metallic`)
- **기본 재료**: 강철 60개
- **재료량**: 120개
- **제외 재료**: 금, 은

## 관련 파일

### Def 파일
- `Project/1.6/Defs/ThingsDefs/Apparel_Util.xml`: 헤비 쉴드 정의 (라인 386-497)
- `Project/1.6/Defs/Stats/Stats_ShieldStamina.xml`: 쉴드 스태미나 스탯 정의

### 소스 코드
- `Project/1.6/Source/ShieldOfRatkinia/CompStaminaShield.cs`: 스태미나 쉴드 컴포넌트 구현
- `Project/1.6/Source/ShieldOfRatkinia/CompProperties_StaminaShield.cs`: 스태미나 쉴드 속성 정의
- `Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs`: 무기 호환성 제한 컴포넌트
- `Project/1.6/Source/ExtraWornDrawer/CompExtraDrawer.cs`: 조건부 드로잉 컴포넌트

### 그래픽 리소스
- `Project/Contents/Textures/Apparel/Util/RK_HeavyShield_*.png`: 소집 시 방패 그래픽
- `Project/Contents/Textures/Apparel/Util/RK_HeavyShieldUnarm_*.png`: 비소집 시 방패 그래픽

