# RimWorld Core 무기 완전 목록

## 태그
RimWorld Core Weapon List 무기 목록 Weapon ThingDef List 원거리 무기 근접 무기

---

## 개요

RimWorld Core 게임에 포함된 모든 무기를 조사하여 분류 및 정리한 문서입니다.

**데이터 출처**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/`

---

## 1. 근접 무기 (Melee Weapons)

### 1.1 신석기 시대 무기 (Neolithic)

| DefName | 한글명 | 설명 | 무게 | 제작 작업량 | 무기 클래스 |
|---------|--------|------|------|------------|------------|
| `MeleeWeapon_Club` | 곤봉 | 한쪽 끝이 무거운 막대기. 인간 생물학의 일부가 된 가장 오래된 무기. | 2.0 | 1200 | Melee, Neolithic |
| `MeleeWeapon_Knife` | 나이프 | 인류가 제작한 가장 오래된 물건 중 하나. 손잡이와 날을 가진 절단 도구. | 0.5 | 1800 | Melee, Neolithic |
| `MeleeWeapon_Ikwa` | 이크와 | 짧은 창대에 긴 날이 달린 창. 빠른 찌르기로 생명 기관을 공격. | 1.1 | 5000 | Melee, Neolithic |
| `MeleeWeapon_Spear` | 창 | 찌르기용 날카로운 끝이 달린 폴암. | 2.0 | 12000 | Melee, Neolithic |

### 1.2 중세 시대 무기 (Medieval)

| DefName | 한글명 | 설명 | 무게 | 제작 작업량 | 무기 클래스 |
|---------|--------|------|------|------------|------------|
| `MeleeWeapon_Mace` | 철퇴 | 효율적인 휘두르기와 치명적인 충격을 위해 설계된 정제된 곤봉. | 1.25 | 6000 | Melee, Medieval |
| `MeleeWeapon_Gladius` | 글라디우스 | 고대 디자인의 단검. 찌르기와 베기에 좋음. 가볍고 민첩함. | 0.85 | 12000 | Melee, Medieval |
| `MeleeWeapon_LongSword` | 롱소드 | 고대 왕들의 무기. 베기와 찌르기 모두 가능. | 2.0 | 18000 | Melee, Medieval |

### 1.3 브리치 무기 (Breach)

| DefName | 한글명 | 설명 | 무게 | 제작 작업량 | 무기 클래스 |
|---------|--------|------|------|------------|------------|
| `MeleeWeapon_BreachAxe` | 브리치 도끼 | 견고한 자루에 결합된 날과 도구 머리. 벽, 문, 구조물 파괴에 특화. | 1.1 | 5000 | Melee, Neolithic |

---

## 2. 원거리 무기 (Ranged Weapons)

### 2.1 신석기 시대 무기 (Neolithic)

| DefName | 한글명 | 설명 | 무게 | 사거리 | 무기 클래스 |
|---------|--------|------|------|--------|------------|
| `Bow_Short` | 쇼트 보우 | 단일 나무 조각으로 만든 단순한 쇼트 셀프보우. | 0.8 | 22.9 | Ranged, RangedLight, Neolithic |
| `Pila` | 필룸 | 던지기용 창. 던지는 데 시간이 걸리지만 한 번 맞추면 큰 피해. | 4.0 | 18.9 | Ranged, Neolithic |
| `Bow_Recurve` | 리커브 보우 | 리커브 보우. 튜닝된 스프링처럼 작동하여 에너지를 효율적으로 저장하고 빠른 발사를 제공. | 1.3 | 25.9 | Ranged, RangedLight, Neolithic |
| `Bow_Great` | 그레이트 보우 | 강력한 그레이트 보우. 무거운 화살을 먼 거리까지 발사. | 3.0 | 29.9 | Ranged, RangedHeavy, Neolithic |

**투사체**:
- `Arrow_Short` - 쇼트보우 화살 (데미지: 11)
- `Pilum_Thrown` - 필룸 (데미지: 25)
- `Arrow_Recurve` - 리커브 보우 화살 (데미지: 14)
- `Arrow_Great` - 그레이트보우 화살 (데미지: 17)

### 2.2 산업 시대 무기 (Industrial)

#### 2.2.1 권총 계열

| DefName | 한글명 | 설명 | 무게 | 사거리 | 무기 클래스 |
|---------|--------|------|------|--------|------------|
| `Gun_Revolver` | 리볼버 | 고대 패턴 더블액션 리볼버. 강력하지 않지만 권총 치고는 사거리가 좋고 빠르게 발사. | 1.4 | 25.9 | RangedLight |
| `Gun_Autopistol` | 오토피스톨 | 고대 패턴 블로우백 작동 자동 장전 권총. 정지력과 사거리가 부족하지만 발사가 빠름. | 1.2 | 25.9 | RangedLight |
| `Gun_MachinePistol` | 머신 피스톨 | 마이크로 기관단총. 사거리가 짧지만 매우 가볍고 연사 속도가 빠름. | 2.5 | 19.9 | RangedLight, ShortShots |

#### 2.2.2 소총 계열

| DefName | 한글명 | 설명 | 무게 | 사거리 | 무기 클래스 |
|---------|--------|------|------|--------|------------|
| `Gun_BoltActionRifle` | 볼트액션 라이플 | 고대 패턴 볼트액션 라이플. 긴 사거리와 낮은 발사 속도로 사냥에 좋음. | 3.5 | 36.9 | LongShots |
| `Gun_AssaultRifle` | 어썰트 라이플 | 범용 가스 작동 어썰트 라이플. 좋은 사거리, 괜찮은 위력, 좋은 정확도. | 3.5 | 30.9 | - |
| `Gun_SniperRifle` | 스나이퍼 라이플 | 고대 디자인의 정밀 스나이퍼 라이플. 볼트액션. 매우 긴 사거리, 뛰어난 정확도와 좋은 위력. | 4.0 | 44.9 | LongShots, RangedHeavy |

#### 2.2.3 샷건 계열

| DefName | 한글명 | 설명 | 무게 | 사거리 | 무기 클래스 |
|---------|--------|------|------|--------|------------|
| `Gun_PumpShotgun` | 펌프 샷건 | 탄환을 빽빽하게 분사하는 고대 디자인의 샷건. 치명적이지만 사거리가 짧음. | 3.4 | 15.9 | ShortShots |
| `Gun_ChainShotgun` | 체인 샷건 | 탄창식 완전 자동 샷건. 일반 샷건보다 사거리가 더 짧지만 연발 사격으로 매우 위험함. | 4.5 | 12.9 | ShortShots |

#### 2.2.4 기관총 계열

| DefName | 한글명 | 설명 | 무게 | 사거리 | 무기 클래스 |
|---------|--------|------|------|--------|------------|
| `Gun_HeavySMG` | 헤비 SMG | 컴팩트한 와이드 캘리버 슬러그 스로어. 매우 짧은 사거리지만 위력이 있고 다루기 좋음. | 3.5 | 22.9 | ShortShots, RangedHeavy |
| `Gun_LMG` | LMG | 가스 작동 경기관총. 다루기 어렵고 부정확하지만 긴 연발 사격이 적 그룹에 효과적. | 8.5 | 25.9 | RangedHeavy |
| `Gun_Minigun` | 미니건 | 다중 총열 기관총. 다루기 어렵지만 발사가 시작되면 매우 빠르게 발사. 전기 모터로 작동. | 10.0 | 30.9 | RangedHeavy |

#### 2.2.5 런처 계열

| DefName | 한글명 | 설명 | 무게 | 사거리 | 무기 클래스 |
|---------|--------|------|------|--------|------------|
| `Gun_IncendiaryLauncher` | 소이 런처 | 넓은 총열의 소이 볼트 런처. 볼트가 충돌 시 작은 소이 폭발을 일으켜 화재를 시작. | 3.4 | 23.9 | RangedHeavy |
| `Gun_SmokeLauncher` | 연막 런처 | 넓은 총열의 연막 포탄 런처. 충돌 시 연기 구름을 방출하여 시야를 가리고 포탑의 록온을 방지. | 3.4 | 23.9 | RangedHeavy |
| `Gun_EmpLauncher` | EMP 런처 | 넓은 총열의 EMP 포탄 런처. 충돌 시 전자기 에너지 폭발을 방출하여 기계적 목표물을 기절시키고 실드를 고갈시킴. | 3.4 | 23.9 | RangedHeavy |

**투사체**:
- `Bullet_Revolver` - 리볼버 탄환 (데미지: 12)
- `Bullet_Autopistol` - 오토피스톨 탄환 (데미지: 10)
- `Bullet_MachinePistol` - 머신 피스톨 탄환 (데미지: 6)
- `Bullet_BoltActionRifle` - 볼트액션 라이플 탄환 (데미지: 18)
- `Bullet_Shotgun` - 샷건 발사 (데미지: 18)
- `Bullet_HeavySMG` - 헤비 SMG 탄환 (데미지: 12)
- `Bullet_LMG` - LMG 탄환 (데미지: 12)
- `Bullet_AssaultRifle` - 어썰트 라이플 탄환 (데미지: 11)
- `Bullet_SniperRifle` - 스나이퍼 라이플 탄환 (데미지: 25)
- `Bullet_Minigun` - 미니건 탄환 (데미지: 10)
- `Bullet_IncendiaryLauncher` - 소이 볼트 (폭발 반경: 1.1)
- `Bullet_SmokeLauncher` - 연막 포탄 (폭발 반경: 2.4)
- `Bullet_EMPLauncher` - EMP 포탄 (폭발 반경: 1.1)

### 2.3 스페이서 시대 무기 (Spacer)

| DefName | 한글명 | 설명 | 무게 | 사거리 | 무기 클래스 |
|---------|--------|------|------|--------|------------|
| `Gun_ChargeRifle` | 차지 라이플 | 충전 사격 어썰트 라이플. 펄스 차지 기술로 각 발사체에 불안정한 에너지를 충전. | 4.6 | 27.9 | RangedLight |
| `Gun_ChargeLance` | 차지 랜스 | 펄스 충전 레일 보조 랜스 무기. 단일 고속 발사를 가속 레일을 통해 불안정한 에너지로 충전. | 8.0 | 32.9 | RangedHeavy |

**투사체**:
- `Bullet_ChargeRifle` - 차지 샷 (데미지: 16, 관통력: 0.35)
- `Bullet_ChargeLance` - 차지 랜스 샷 (데미지: 30)

### 2.4 메카노이드 무기 (Mechanoid)

| DefName | 한글명 | 설명 | 무게 | 사거리 | 무기 클래스 |
|---------|--------|------|------|--------|------------|
| `Gun_ChargeBlasterHeavy` | 헤비 차지 블래스터 | 펄스 충전 연속 발사 블래스터. 지역 억제 사격용. 무거운 냉각으로 긴 치명적인 연발 가능. | 22.0 | 26.9 | RangedHeavy |
| `Gun_InfernoCannon` | 인페르노 캐논 | 소이 포탄 미니 포병 장치. 큰 소이 탄두를 발사. | 18.0 | 26.9 | RangedHeavy |
| `Gun_Needle` | 니들 건 | 메카노이드가 사용하는 장거리 무기. 니들 같은 투사체로 명명됨. 단일 발사로 큰 정확도. | 2.6 | 44.9 | RangedHeavy, LongShots |
| `Gun_ThumpCannon` | 썸프 캐논 | 메카노이드가 사용하는 브리칭 폭발 런처. 진동 폭탄을 발사하여 벽과 구조물에 매우 효과적. | 20.0 | 24.9 | - |

**투사체**:
- `Bullet_ChargeBlasterHeavy` - 차지 블래스터 샷 (데미지: 15)
- `Bullet_InfernoCannon` - 인페르노 캐논 포탄 (폭발 반경: 2.4)
- `Bullet_NeedleGun` - 니들 샷 (데미지: 15, 관통력: 0.35)
- `Bullet_ThumpCannon` - 썸프 폭탄 (데미지: 9, 폭발 반경: 1.9)

**특징**: 메카노이드 무기는 모두 `tradeability: None`, `destroyOnDrop: true` 속성을 가짐 (거래 불가, 드롭 시 파괴)

### 2.5 특수 무기 (Special)

#### 2.5.1 일회용 로켓 런처

| DefName | 한글명 | 설명 | 무게 | 사거리 | 무기 클래스 |
|---------|--------|------|------|--------|------------|
| `Gun_TripleRocket` | 트리플 로켓 런처 | 일회용 로켓 런처. 세 발의 대구경 폭발 로켓을 발사. 작은 그룹의 강한 목표물에 좋음. | 7.0 | 35.9 | RangedHeavy |
| `Gun_DoomsdayRocket` | 둠스데이 로켓 런처 | 일회용 로켓 런처. 거대한 폭발 투사체를 발사. 큰 그룹의 약한 목표물에 좋음. 화재를 시작함. | 8.0 | 35.9 | RangedHeavy |

**투사체**:
- `Bullet_Rocket` - 로켓 (폭발 반경: 3.9)
- `Bullet_DoomsdayRocket` - 둠스데이 로켓 (폭발 반경: 7.8)

#### 2.5.2 궤도 타겟터

| DefName | 한글명 | 설명 | 무게 | 사거리 | 특징 |
|---------|--------|------|------|--------|------|
| `OrbitalTargeterBombardment` | 궤도 폭격 타겟터 | 궤도 폭격 시스템용 고대 타겟팅 장치. 위성 네트워크에 좌표를 전송하여 운동 충격체로 목표 지역을 폭격. | 0.2 | 44.9 | 일회용 (벨트 장착) |
| `OrbitalTargeterPowerBeam` | 궤도 파워 빔 타겟터 | 파워 수집 위성 네트워크용 고대 타겟팅 유닛. 거대한 전자기 에너지 기둥을 발사. | 0.2 | 44.9 | 일회용 (벨트 장착) |
| `TornadoGenerator` | 토네이도 생성기 | 날씨 제어 위성 네트워크용 고대 제어 유닛. 대규모 공기 흐름 교란을 일으켜 토네이도를 생성. | 0.2 | 44.9 | 일회용 (벨트 장착) |

**특징**: 모든 궤도 타겟터는 `Apparel` 카테고리로 분류되며 벨트에 장착됨. 일회용이며 사용 후 파괴됨.

### 2.6 수류탄 (Grenades)

| DefName | 한글명 | 설명 | 무게 | 사거리 | 특징 |
|---------|--------|------|------|--------|------|
| `Weapon_GrenadeFrag` | 파편 수류탄 | 구식 파편 수류탄. 짧은 거리로 던질 수 있으며 폭발하여 주변의 모든 것을 손상시킴. | 1.0 | 12.9 | 폭발 데미지 |
| `Weapon_GrenadeMolotov` | 몰로토프 칵테일 | 목에 불타는 천이 있는 가연성 액체가 담긴 유리 병. 화재를 시작함. | 1.0 | 12.9 | 화염 데미지 |
| `Weapon_GrenadeEMP` | EMP 수류탄 | 전자 장비에 손상을 주는 전자기 펄스 수류탄. | 1.0 | 12.9 | EMP 데미지 |

**투사체**:
- `Proj_GrenadeFrag` - 파편 수류탄 (폭발 반경: 1.9)
- `Proj_GrenadeMolotov` - 몰로토프 칵테일 (폭발 반경: 1.1)
- `Proj_GrenadeEMP` - EMP 수류탄 (폭발 반경: 3.5)

---

## 3. 무기 분류 통계

### 3.1 기술 레벨별 분류

- **Neolithic (신석기)**: 8개 (근접 4개, 원거리 4개)
- **Medieval (중세)**: 3개 (근접 3개)
- **Industrial (산업)**: 15개 (원거리 15개)
- **Spacer (스페이서)**: 2개 (원거리 2개)
- **Mechanoid (메카노이드)**: 4개 (원거리 4개)
- **Special (특수)**: 5개 (원거리 5개)

**총계**: 37개 무기

### 3.2 무기 클래스별 분류

- **Melee (근접)**: 8개
- **Ranged (원거리)**: 29개
  - RangedLight: 4개
  - RangedHeavy: 12개
  - LongShots: 3개
  - ShortShots: 4개
  - Neolithic: 4개
  - 기타: 2개

### 3.3 무기 태그별 분류

- **NeolithicMeleeBasic**: Club, Knife
- **NeolithicMeleeDecent**: Ikwa
- **NeolithicMeleeAdvanced**: Spear
- **MedievalMeleeBasic**: Knife
- **MedievalMeleeDecent**: Mace, Gladius
- **MedievalMeleeAdvanced**: LongSword
- **NeolithicRangedBasic**: Bow_Short
- **NeolithicRangedDecent**: Bow_Recurve
- **NeolithicRangedHeavy**: Pila, Bow_Great
- **SimpleGun**: Revolver, Autopistol
- **IndustrialGunAdvanced**: ChainShotgun, HeavySMG, LMG, AssaultRifle, SniperRifle
- **SpacerGun**: ChargeRifle, ChargeLance
- **MechanoidGunMedium**: ChargeLance
- **MechanoidGunHeavy**: ChargeBlasterHeavy, InfernoCannon
- **MechanoidGunLongRange**: Needle Gun
- **MechanoidGunBreach**: Thump Cannon
- **GunHeavy**: Minigun
- **GunSingleUse**: TripleRocket, DoomsdayRocket
- **GrenadeDestructive**: Frag Grenade, Molotov
- **GrenadeFlame**: Molotov
- **GrenadeEMP**: EMP Grenade, EMP Launcher
- **GrenadeSmoke**: Smoke Launcher

---

## 4. 특수 무기 특징

### 4.1 일회용 무기
- `Gun_TripleRocket`: 사용 후 파괴
- `Gun_DoomsdayRocket`: 사용 후 파괴
- 모든 궤도 타겟터: 사용 후 파괴

### 4.2 메카노이드 전용 무기
- 거래 불가 (`tradeability: None`)
- 드롭 시 파괴 (`destroyOnDrop: true`)
- 유물 생성 불가 (`relicChance: 0`)

### 4.3 제작 불가 무기
- 메카노이드 무기 전부
- 궤도 타겟터 전부
- 일회용 로켓 런처

---

## 5. 참고 사항

- 모든 무기는 `BaseWeapon` 또는 그 하위 클래스를 상속받음
- 원거리 무기는 대부분 `BaseGun` 또는 `BaseHumanMakeableGun`을 상속
- 근접 무기는 `BaseMeleeWeapon`, `BaseMeleeWeapon_Sharp`, `BaseMeleeWeapon_Blunt` 등을 상속
- 제작 가능한 무기는 `recipeMaker` 속성을 가짐
- 품질 시스템이 있는 무기는 `CompQuality` 컴포넌트를 가짐
- 예술품이 될 수 있는 무기는 `CompProperties_Art` 컴포넌트를 가짐

---

## 6. 파일 위치

- **기본 무기 정의**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/BaseWeapons.xml`
- **근접 무기 (중세)**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/MeleeMedieval.xml`
- **근접 무기 (신석기)**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/MeleeNeolithic.xml`
- **원거리 무기 (신석기)**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedNeolithic.xml`
- **원거리 무기 (산업)**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedIndustrial.xml`
- **원거리 무기 (스페이서)**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedSpacer.xml`
- **원거리 무기 (메카노이드)**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedMechanoid.xml`
- **원거리 무기 (특수)**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedSpecial.xml`
- **수류탄**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedIndustrialGrenades.xml`
- **일회용 무기**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/RangedIndustrialConsumable.xml`
- **브리치 무기**: `RimworldData/Core/Defs/ThingDefs_Misc/Weapons/Weapons_Breach.xml`, `Breach.xml`

---

**작성일**: 2026-01-30
**데이터 버전**: RimWorld Core (최신)
