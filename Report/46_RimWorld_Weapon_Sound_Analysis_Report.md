# 림월드 총기류 사운드 분석 보고서

## 개요
림월드 코어 데이터에서 일반 총기류 사운드 정의를 분석하고, 프로젝트 내 사운드 사용 현황과 비교 분석한 보고서입니다.

## 림월드 코어 사운드 구조

### 사운드 정의 파일 위치
- **주요 파일**: `RimworldData/Core/Defs/SoundDefs/World_Oneshots_Weapons.xml`
- **꼬리 사운드**: `RimworldData/Core/Defs/SoundDefs/World_Oneshots_Tails.xml`

### 사운드 분류

#### 1. Interact 사운드 (무기 조작 시)
무기를 들거나 조작할 때 재생되는 사운드입니다.

| 사운드 이름 | 설명 | 클립 경로 |
|------------|------|----------|
| `Interact_Revolver` | 리볼버 조작 | `UI/WeaponHandling/HandleWeapon_SmallA` |
| `Interact_Autopistol` | 자동권총 조작 | `UI/WeaponHandling/HandleWeapon_SmallA` |
| `Interact_Shotgun` | 샷건 조작 | `UI/WeaponHandling/HandleWeapon_BigALow` |
| `Interact_Rifle` | 소총 조작 | `UI/WeaponHandling/HandleWeaponA` |
| `Interact_AssaultRifle` | 돌격소총 조작 | `UI/WeaponHandling/HandleWeaponB` |
| `Interact_SMG` | 기관단총 조작 | `UI/WeaponHandling/HandleWeaponA` |
| `Interact_ChargeRifle` | 충전 소총 조작 | `UI/WeaponHandling/HandleWeaponB` |
| `Interact_ChargeLance` | 충전 랜스 조작 | `UI/WeaponHandling/HandleWeaponB` |

#### 2. Shot 사운드 (발사 시)
무기를 발사할 때 재생되는 사운드입니다.

| 사운드 이름 | 설명 | 클립 경로 | 볼륨 범위 | 피치 범위 |
|------------|------|----------|----------|----------|
| `Shot_Revolver` | 리볼버 발사 | `Weapon/Revolver` (폴더) | 35 | - |
| `Shot_Autopistol` | 자동권총 발사 | `Weapon/Autopistol` (폴더) | 61.76 | - |
| `Shot_MachinePistol` | 기관권총 발사 | `Weapon/MachinePistol` (폴더) | - | - |
| `Shot_HeavySMG` | 중기관단총 발사 | `Weapon/HeavySMG` (폴더) | 40.59 | - |
| `Shot_AssaultRifle` | 돌격소총 발사 | `Weapon/AssaultRifle` (폴더) | 45.29 | 1.09~1.0 |
| `Shot_Shotgun` | 샷건 발사 | `Weapon/Shotgun/Shot` + `Weapon/Shotgun/Rack` | 94.71 | - |
| `Shot_BoltActionRifle` | 볼트액션 소총 발사 | `Weapon/BoltActionRifle` (폴더) | - | 0.98~1.11 |
| `Shot_SniperRifle` | 저격소총 발사 | `Weapon/SniperRifle` (폴더) | 70 | - |
| `Shot_Minigun` | 미니건 발사 | `Weapon/Minigun/Shot` (폴더) | 33.53 | - |
| `Shot_ChargeRifle` | 충전 소총 발사 | `Weapon/ChargeRifle` (폴더) | - | - |
| `Shot_NeedleGun` | 니들건 발사 | `Weapon/Autocannon` (폴더) | - | 1.75~1.85 |
| `Shot_IncendiaryLauncher` | 소이 발사기 발사 | `Weapon/IncendiaryLauncher` (폴더) | 40.59 | - |
| `Shot_ChargeBlaster` | 충전 블래스터 발사 | `Weapon/ChargeShotA` (클립) | - | 1.4~1.6 |
| `Shot_MiniSlug` | 미니 슬러그 발사 | `Weapon/HeavySMG` (폴더) | 40 | 0.75~0.80 |

**특수 사운드:**
- `ChargeLance_Fire`: 충전 랜스 발사 (별도 정의)
- `InfernoCannon_Fire`: 인페르노 캐논 발사
- `ThumpCannon_Fire`: 썸프 캐논 발사
- `OrbitalTargeter_Fire`: 궤도 타겟터 발사

#### 3. GunTail 사운드 (발사 후 꼬리 사운드)
발사 후 재생되는 반향/꼬리 사운드입니다.

| 사운드 이름 | 설명 | 사용 무기 |
|------------|------|----------|
| `GunTail_Light` | 경량 총기 꼬리 | 리볼버, 자동권총, 기관권총 |
| `GunTail_Medium` | 중량 총기 꼬리 | 돌격소총, 미니건, 충전 소총, 소이 발사기 |
| `GunTail_Heavy` | 중무기 꼬리 | 샷건, 볼트액션, 저격소총, 중기관단총, 충전 랜스 |

## 무기별 사운드 사용 패턴

### 산업 시대 무기 (RangedIndustrial.xml)

| 무기 | soundInteract | soundCast | soundCastTail |
|------|--------------|-----------|---------------|
| 리볼버 | `Interact_Revolver` | `Shot_Revolver` | `GunTail_Light` |
| 자동권총 | `Interact_Autopistol` | `Shot_Autopistol` | `GunTail_Light` |
| 기관권총 | - | `Shot_MachinePistol` | `GunTail_Light` |
| 중기관단총 | `Interact_SMG` | `Shot_HeavySMG` | `GunTail_Heavy` |
| 돌격소총 | `Interact_Rifle` | `Shot_AssaultRifle` | `GunTail_Medium` |
| 볼트액션 소총 | `Interact_Rifle` | `Shot_BoltActionRifle` | `GunTail_Heavy` |
| 샷건 | `Interact_Shotgun` | `Shot_Shotgun` | `GunTail_Heavy` |
| 펌프 샷건 | `Interact_Shotgun` | `Shot_Shotgun` | `GunTail_Heavy` |
| 미니건 | `Interact_Rifle` | `Shot_Minigun` | `GunTail_Medium` |
| 저격소총 | `Interact_Rifle` | `Shot_SniperRifle` | `GunTail_Heavy` |

### 우주 시대 무기 (RangedSpacer.xml)

| 무기 | soundInteract | soundCast | soundCastTail |
|------|--------------|-----------|---------------|
| 충전 소총 | `Interact_ChargeRifle` | `Shot_ChargeRifle` | `GunTail_Medium` |
| 충전 랜스 | `Interact_ChargeLance` | `ChargeLance_Fire` | `GunTail_Heavy` |

### 기계족 무기 (RangedMechanoid.xml)

| 무기 | soundInteract | soundCast | soundCastTail |
|------|--------------|-----------|---------------|
| 충전 블래스터 | - | `Shot_ChargeBlaster` | `GunTail_Heavy` |
| 인페르노 캐논 | - | `InfernoCannon_Fire` | `GunTail_Light` |
| 니들건 | - | `Shot_NeedleGun` | `GunTail_Heavy` |

## 프로젝트 사운드 현황

### 현재 프로젝트 SoundDef.xml

```xml
<SoundDef>
    <defName>Rifle</defName>
    <subSounds>
        <li>
            <grains>
                <li Class="AudioGrain_Clip">
                    <clipPath>FlechetteRifle</clipPath>
                </li>
            </grains>
        </li>
    </subSounds>
</SoundDef>

<SoundDef>
    <defName>FlechetteRifle</defName>
    <subSounds>
        <li>
            <grains>
                <li Class="AudioGrain_Clip">
                    <clipPath>Rifle</clipPath>
                </li>
            </grains>
        </li>
    </subSounds>
</SoundDef>
```

**문제점:**
1. **순환 참조**: `Rifle` SoundDef가 `FlechetteRifle` 클립을 참조하고, `FlechetteRifle` SoundDef가 `Rifle` 클립을 참조하는 순환 구조
2. **명명 혼란**: `Rifle`이라는 일반적인 이름 사용
3. **림월드 표준 미준수**: 림월드는 `Shot_` 접두사를 사용하지만 프로젝트는 사용하지 않음

### 프로젝트 무기에서 사용 중인 사운드

**Weapon_Range.xml:**
- `soundInteract`: `Interact_Rifle` (다수 무기)
- `soundCast`: `FlechetteRifle` (플레셰트 라이플)

**Weapon_HighTech.xml:**
- 건랜스 관련 커스텀 사운드 사용 (RatkinSoundDefOf 참조)

## 권장 사항

### 1. 사운드 정의 수정
- 순환 참조 제거
- 림월드 표준에 맞춰 `Shot_` 접두사 사용 고려
- 명확한 사운드 이름 사용

### 2. 사운드 클립 경로 확인
- `FlechetteRifle` 클립 파일 존재 여부 확인
- `Rifle` 클립 파일 존재 여부 확인
- 실제 오디오 파일과 경로 일치 여부 확인

### 3. 림월드 표준 준수
- 가능한 경우 림월드 코어 사운드 재사용
- 커스텀 사운드 필요 시 명확한 네이밍 규칙 적용

## 참고 자료

### 림월드 코어 파일
- `RimworldData/Core/Defs/SoundDefs/World_Oneshots_Weapons.xml`
- `RimworldData/Core/Defs/SoundDefs/World_Oneshots_Tails.xml`
- `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedIndustrial.xml`
- `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedSpacer.xml`

### 프로젝트 파일
- `Project/1.6/Defs/SoundDefs/SoundDef.xml`
- `Project/1.6/Defs/ThingsDefs/Weapon_Range.xml`
- `Project/1.6/Defs/ThingsDefs/Weapon_HighTech.xml`

## 결론

림월드 코어는 총기류 사운드를 다음과 같이 체계적으로 분류합니다:
- **Interact 사운드**: 무기 조작 시
- **Shot 사운드**: 발사 시
- **GunTail 사운드**: 발사 후 반향

프로젝트의 현재 사운드 정의는 순환 참조 문제가 있으며, 림월드 표준과의 일관성을 개선할 여지가 있습니다. 사운드 클립 파일의 실제 존재 여부와 경로를 확인하고, 필요시 림월드 코어 사운드를 재사용하는 것을 권장합니다.

