# WeaponTags 사용 가능 목록

## 개요
프로젝트 내에서 사용되는 모든 weaponTags 값 목록입니다.

## Ratkin 프로젝트 커스텀 태그

### 계층/티어 태그
- `RK_1TierRange` - 1티어 원거리 무기
- `RK_2TierWeapon` - 2티어 무기
- `RK_WeaponTag_2Tier` - 2티어 태그
- `RK_3TierWeapon` - 3티어 무기
- `RK_4TierWeapon` - 4티어 무기

### 무기 유형 태그
- `RK_Weapon` - 일반 무기
- `RK_RangeWeapon` - 원거리 무기
- `RK_MeleeWeapon_Short` - 단거리 근접 무기
- `RK_MeleeWeapon_Lance` - 랜스 계열 무기
- `RK_Crossbow` - 석궁
- `RK_CrossbowAdvanced` - 고급 석궁
- `RK_Rifle` - 라이플
- `RK_Shotgun` - 샷건
- `RK_Sword` - 검
- `RK_Gunlance` - 건랜스
- `RK_HeavyLance` - 헤비 랜스

### 역할/직업 태그
- `RK_Combatant` - 전투원
- `RK_Defender` - 방어자
- `RK_EliteDefender` - 정예 방어자
- `RK_Knight` - 기사
- `RK_Guardener` - 정원사
- `RK_Murderer` - 살인마
- `RK_Demoman` - 데모맨
- `RK_WorkerTool` - 작업 도구
- `RK_ChefTool` - 요리 도구

### 무게/크기 태그
- `RK_LightWeapon` - 경량 무기

### 손 사용 태그
- `RK_WeaponTag_OneHand` - 한손 무기
- `RK_WeaponTag_OneHandMelee` - 한손 근접 무기
- `RK_WeaponTag_OneHandMeleeAttack` - 한손 근접 공격
- `RK_WeaponTag_OneHandRange` - 한손 원거리 무기
- `RK_WeaponTag_OneHandRangeAttack` - 한손 원거리 공격
- `RK_WeaponTag_TwoHand` - 양손 무기
- `RK_WeaponTag_TwoHandMelee` - 양손 근접 무기
- `RK_WeaponTag_TwoHandMeleeAttack` - 양손 근접 공격
- `RK_WeaponTag_TwoHandRange` - 양손 원거리 무기
- `RK_WeaponTag_TwoHandRangeAttack` - 양손 원거리 공격

### 호환성 태그
- `RK_WeaponTag_LightShieldCompatible` - 경량 방패 호환

### 범용 태그
- `RK_WeaponTag_All` - 모든 무기
- `RK_WeaponTag_Melee` - 모든 근접 무기
- `RK_Weapon_All` - 모든 무기 (대체)
- `RK_Weapon_Range` - 모든 원거리 무기
- `RK_Weapon_RangeAttack` - 모든 원거리 공격
- `RK_Weapon_TwoHand` - 모든 양손 무기
- `RK_Weapon_TwoHandRange` - 모든 양손 원거리 무기
- `RK_Weapon_TwoHandRangeAttack` - 모든 양손 원거리 공격

### 특수 태그
- `RK_WeaponTag_Reward` - 보상 무기
- `RK_WeaponTag_Exotic` - 이국적인 무기

## 사용 예시

### PawnKindDef에서 사용
```xml
<weaponTags>
    <li>RK_Rifle</li>
    <li>RK_Combatant</li>
</weaponTags>
```

### ThingDef에서 사용
```xml
<weaponTags Inherit="false">
    <li>RK_WeaponTag_OneHand</li>
    <li>RK_WeaponTag_OneHandRange</li>
    <li>RK_WeaponTag_OneHandRangeAttack</li>
</weaponTags>
```

### CompShieldWeaponIncompatible에서 사용
```xml
<allowedWeaponTags>
    <li>RK_WeaponTag_OneHand</li>
</allowedWeaponTags>
```

## 참고
- `Inherit="false"` 속성을 사용하여 부모 Def의 weaponTags를 상속받지 않도록 설정 가능
- PawnKindDef의 weaponTags는 해당 PawnKind가 사용할 수 있는 무기를 필터링하는 데 사용됨
- CompShieldWeaponIncompatible의 allowedWeaponTags는 방패와 함께 사용 가능한 무기를 정의함

