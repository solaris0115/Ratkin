# VerbClass 종합 참조 문서

## 개요

이 문서는 RimWorld에서 사용 가능한 모든 `verbClass`를 정리한 종합 참조 문서입니다. RimWorld 데이터 XML에서 실제로 사용된 verbClass와 소스 코드에서 정의된 모든 Verb 클래스를 포함합니다.

## 1. RimWorld XML 데이터에서 사용된 verbClass (우선 열거)

### 1.1 원거리 무기 (Ranged Weapons)

#### Verb_Shoot
- **가장 많이 사용되는 verbClass**
- **설명**: 일반적인 총기 발사
- **사용 위치**: 대부분의 원거리 무기
- **예시**:
  - `RangedIndustrial.xml` - 모든 산업 시대 총기
  - `RangedSpacer.xml` - 우주 시대 총기
  - `RangedNeolithic.xml` - 신석기 시대 무기
  - `Buildings_Security_Turrets.xml` - 포탑
  - `RangedMechanoid.xml` - 메카노이드 무기

#### Verb_ShootBeam
- **설명**: 빔 발사 (레이저/에너지 무기)
- **사용 위치**:
  - `RangedMechanoid_Medium.xml` - 메카노이드 중형 무기

#### Verb_LaunchProjectile
- **설명**: 투사체 발사 (수류탄, 폭발물 등)
- **사용 위치**:
  - `RangedIndustrial.xml` - 산업 시대 무기
  - `RangedIndustrialGrenades.xml` - 수류탄

#### Verb_LaunchProjectileStatic
- **설명**: 정적 위치에서 투사체 발사
- **사용 위치**:
  - `Apparel_Various.xml` (Royalty) - 장비
  - 특수 장비

#### Verb_LaunchProjectileStaticPsychic
- **설명**: 사이킥 능력을 사용한 정적 투사체 발사
- **사용 위치**:
  - `Apparel_Packs.xml` (Anomaly) - 사이킥 장비

#### Verb_LaunchProjectileStaticOneUse
- **설명**: 일회용 정적 투사체 발사
- **사용 위치**:
  - `Apparel_Packs.xml` (Anomaly, Odyssey) - 일회용 장비

#### Verb_ShootOneUse
- **설명**: 일회용 발사 (소비성 무기)
- **사용 위치**:
  - `RangedIndustrialConsumable.xml` - 소비성 무기

#### Verb_SpewFire
- **설명**: 화염 분사
- **사용 위치**:
  - `RangedMechanoid_Light.xml` - 메카노이드 경형 화염 무기

#### Verb_ArcSprayIncinerator
- **설명**: 화염 방사기 스프레이
- **사용 위치**:
  - `Weapons_Ranged.xml` (Anomaly) - 화염 방사 무기

#### Verb_ArcSprayProjectile
- **설명**: 아크 스프레이 투사체
- **사용 위치**:
  - `Buildings_Security_Turrets.xml` - 특수 포탑

### 1.2 특수 무기 (Special Weapons)

#### Verb_Bombardment
- **설명**: 포격 공격
- **사용 위치**:
  - `RangedSpecial.xml` - 특수 무기

#### Verb_PowerBeam
- **설명**: 파워 빔 공격
- **사용 위치**:
  - `RangedSpecial.xml` - 특수 무기

#### Verb_Spawn
- **설명**: 스폰 생성
- **사용 위치**:
  - `RangedSpecial.xml` - 특수 무기

#### Verb_MechCluster
- **설명**: 메크 클러스터 생성
- **사용 위치**:
  - `OrbitalWeapons.xml` (Royalty) - 궤도 무기

### 1.3 근접 무기 (Melee Weapons)

#### Verb_MeleeAttackDamage
- **설명**: 근접 공격 피해 적용
- **사용 위치**:
  - `Maneuvers.xml` (Core, Odyssey) - 근접 전투 기동

### 1.4 능력 (Abilities)

#### Verb_CastAbility
- **설명**: 일반 능력 시전
- **가장 많이 사용되는 능력 verbClass**
- **사용 위치**:
  - `Abilities.xml` (Core, Anomaly, Biotech, Odyssey) - 대부분의 능력
  - `WeaponTraitAbilities.xml` (Odyssey) - 무기 특성 능력
  - `Hediffs_Mechanitor.xml` (Biotech) - 메카니터 헤딥

#### Verb_CastAbilityTouch
- **설명**: 터치 능력 시전
- **사용 위치**:
  - `Abilities.xml` (Biotech, Ideology) - 터치 능력

#### Verb_CastAbilityJump
- **설명**: 점프 능력 시전
- **사용 위치**:
  - `Abilities.xml` (Biotech) - 점프 능력

#### Verb_CastAbilityConsumeLeap
- **설명**: 소비형 도약 능력 시전
- **사용 위치**:
  - `Abilities.xml` (Anomaly) - 특수 능력

#### Verb_AbilityShoot
- **설명**: 능력 발사 (능력과 발사 결합)
- **사용 위치**:
  - `Abilities.xml` (Anomaly) - 발사 능력

#### Verb_EntitySkip
- **설명**: 엔티티 스킵 (텔레포트)
- **사용 위치**:
  - `Abilities.xml` (Anomaly) - 텔레포트 능력

### 1.5 특수 장비 (Special Apparel/Equipment)

#### Verb_Smokepop (실제 클래스명: Verb_SmokePop)
- **설명**: 연기 폭발
- **사용 위치**:
  - `Apparel_Packs.xml` (Core) - 연기 팩

#### Verb_FirefoamPop
- **설명**: 소화제 폭발
- **사용 위치**:
  - `Apparel_Packs.xml` (Core) - 소화제 팩

#### Verb_Jump
- **설명**: 점프
- **사용 위치**:
  - `Apparel_Packs.xml` (Royalty) - 점프 팩
  - `Apparel_Various.xml` (Royalty) - 점프 장비

#### Verb_DeployBroadshield
- **설명**: 방패 배치
- **사용 위치**:
  - `Apparel_Packs.xml` (Royalty) - 방패 팩

#### Verb_DeployToxPack
- **설명**: 독 팩 배치
- **사용 위치**:
  - `Apparel_Packs.xml` (Biotech) - 독 팩

#### Verb_CastTargetEffectLances
- **설명**: 랜스 타겟 효과 시전
- **사용 위치**:
  - `Apparel_Utility.xml` (Core, Anomaly) - 랜스 장비

#### Verb_CastTargetEffectBiomutationLance
- **설명**: 생물 돌연변이 랜스 타겟 효과 시전
- **사용 위치**:
  - `Apparel_Utility.xml` (Anomaly) - 특수 랜스 장비

---

## 2. RimWorld 소스 코드에서 정의된 모든 Verb 클래스

### 2.1 기본 클래스 (Base Classes)

#### Verb (abstract)
- **네임스페이스**: `Verse`
- **설명**: 모든 Verb의 기본 추상 클래스
- **위치**: `Verse/Verb.cs`
- **특징**: 직접 사용 불가, 상속해서 사용

### 2.2 원거리 무기 계열 (Ranged Weapon Classes)

#### Verb_LaunchProjectile
- **네임스페이스**: `Verse`
- **상속**: `Verb`
- **설명**: 투사체 발사의 기본 클래스
- **위치**: `Verse/Verb_LaunchProjectile.cs`

#### Verb_Shoot
- **네임스페이스**: `Verse`
- **상속**: `Verb_LaunchProjectile`
- **설명**: 일반 총기 발사
- **위치**: `Verse/Verb_Shoot.cs`
- **특징**: 사격 스킬 경험치 획득

#### Verb_ShootOneUse
- **네임스페이스**: `RimWorld`
- **상속**: `Verb_Shoot`
- **설명**: 일회용 발사
- **위치**: `RimWorld/Verb_ShootOneUse.cs`

#### Verb_ShootBeam
- **네임스페이스**: `Verse`
- **상속**: `Verb`
- **설명**: 빔 발사
- **위치**: `Verse/Verb_ShootBeam.cs`

#### Verb_LaunchProjectileStatic
- **네임스페이스**: `Verse`
- **상속**: `Verb_LaunchProjectile`
- **설명**: 정적 위치에서 투사체 발사
- **위치**: `Verse/Verb_LaunchProjectileStatic.cs`

#### Verb_LaunchProjectileStaticOneUse
- **네임스페이스**: `Verse`
- **상속**: `Verb_LaunchProjectileStatic`
- **설명**: 일회용 정적 투사체 발사
- **위치**: `Verse/Verb_LaunchProjectileStaticOneUse.cs`

#### Verb_LaunchProjectileStaticPsychic
- **네임스페이스**: `Verse`
- **상속**: `Verb_LaunchProjectileStatic`
- **설명**: 사이킥 정적 투사체 발사
- **위치**: `Verse/Verb_LaunchProjectileStaticPsychic.cs`

#### Verb_Spray (abstract)
- **네임스페이스**: `Verse`
- **상속**: `Verb`
- **설명**: 스프레이 공격의 기본 클래스
- **위치**: `Verse/Verb_Spray.cs`

#### Verb_ArcSpray
- **네임스페이스**: `Verse`
- **상속**: `Verb_Spray`
- **설명**: 아크 스프레이
- **위치**: `Verse/Verb_ArcSpray.cs`

#### Verb_ArcSprayIncinerator
- **네임스페이스**: `Verse`
- **상속**: `Verb_ShootBeam`
- **설명**: 화염 방사기 아크 스프레이
- **위치**: `Verse/Verb_ArcSprayIncinerator.cs`

#### Verb_ArcSprayProjectile
- **네임스페이스**: `Verse`
- **상속**: `Verb_ArcSpray`
- **설명**: 아크 스프레이 투사체
- **위치**: `Verse/Verb_ArcSprayProjectile.cs`

#### Verb_SpewFire
- **네임스페이스**: `Verse`
- **상속**: `Verb`
- **설명**: 화염 분사
- **위치**: `Verse/Verb_SpewFire.cs`

### 2.3 근접 무기 계열 (Melee Weapon Classes)

#### Verb_MeleeAttack (abstract)
- **네임스페이스**: `RimWorld`
- **상속**: `Verb`
- **설명**: 근접 공격의 기본 클래스
- **위치**: `RimWorld/Verb_MeleeAttack.cs`
- **특징**: 근접 전투 스킬 경험치 획득

#### Verb_MeleeAttackDamage
- **네임스페이스**: `RimWorld`
- **상속**: `Verb_MeleeAttack`
- **설명**: 근접 공격 피해 적용
- **위치**: `RimWorld/Verb_MeleeAttackDamage.cs`

#### Verb_MeleeApplyHediff
- **네임스페이스**: `RimWorld`
- **상속**: `Verb_MeleeAttack`
- **설명**: 근접 공격으로 헤딥 적용
- **위치**: `RimWorld/Verb_MeleeApplyHediff.cs`

### 2.4 능력 계열 (Ability Classes)

#### Verb_CastAbility
- **네임스페이스**: `RimWorld`
- **상속**: `Verb`, `IAbilityVerb`
- **설명**: 일반 능력 시전
- **위치**: `RimWorld/Verb_CastAbility.cs`

#### Verb_CastAbilityTouch
- **네임스페이스**: `RimWorld`
- **상속**: `Verb_CastAbility`
- **설명**: 터치 능력 시전
- **위치**: `RimWorld/Verb_CastAbilityTouch.cs`

#### Verb_CastAbilityJump
- **네임스페이스**: `RimWorld`
- **상속**: `Verb_CastAbility`
- **설명**: 점프 능력 시전
- **위치**: `RimWorld/Verb_CastAbilityJump.cs`

#### Verb_CastAbilityConsumeLeap
- **네임스페이스**: `RimWorld`
- **상속**: `Verb_CastAbilityJump`
- **설명**: 소비형 도약 능력 시전
- **위치**: `RimWorld/Verb_CastAbilityConsumeLeap.cs`

#### Verb_CastPsycast
- **네임스페이스**: `RimWorld`
- **상속**: `Verb_CastAbility`
- **설명**: 사이킥 시전
- **위치**: `RimWorld/Verb_CastPsycast.cs`

#### Verb_AbilityShoot
- **네임스페이스**: `Verse`
- **상속**: `Verb_Shoot`, `IAbilityVerb`
- **설명**: 능력 발사
- **위치**: `Verse/Verb_AbilityShoot.cs`

#### Verb_EntitySkip
- **네임스페이스**: `RimWorld`
- **상속**: `Verb_CastAbility`
- **설명**: 엔티티 스킵 (텔레포트)
- **위치**: `RimWorld/Verb_EntitySkip.cs`

### 2.5 특수 효과 계열 (Special Effect Classes)

#### Verb_CastBase (abstract)
- **네임스페이스**: `Verse.AI`
- **상속**: `Verb`
- **설명**: 시전 기본 클래스
- **위치**: `Verse/AI/Verb_CastBase.cs`

#### Verb_CastTargetEffect
- **네임스페이스**: `Verse.AI`
- **상속**: `Verb_CastBase`
- **설명**: 타겟 효과 시전
- **위치**: `Verse/AI/Verb_CastTargetEffect.cs`

#### Verb_CastTargetEffectLances
- **네임스페이스**: `Verse.AI`
- **상속**: `Verb_CastTargetEffect`
- **설명**: 랜스 타겟 효과 시전
- **위치**: `Verse/AI/Verb_CastTargetEffectLances.cs`

#### Verb_CastTargetEffectBiomutationLance
- **네임스페이스**: `Verse.AI`
- **상속**: `Verb_CastTargetEffect`
- **설명**: 생물 돌연변이 랜스 타겟 효과 시전
- **위치**: `Verse/AI/Verb_CastTargetEffectBiomutationLance.cs`

#### Verb_Spawn
- **네임스페이스**: `RimWorld`
- **상속**: `Verb_CastBase`
- **설명**: 스폰 생성
- **위치**: `RimWorld/Verb_Spawn.cs`

#### Verb_Bombardment
- **네임스페이스**: `RimWorld`
- **상속**: `Verb_CastBase`
- **설명**: 포격 공격
- **위치**: `RimWorld/Verb_Bombardment.cs`

#### Verb_PowerBeam
- **네임스페이스**: `RimWorld`
- **상속**: `Verb_CastBase`
- **설명**: 파워 빔 공격
- **위치**: `RimWorld/Verb_PowerBeam.cs`

#### Verb_MechCluster
- **네임스페이스**: `RimWorld`
- **상속**: `Verb_CastBase`
- **설명**: 메크 클러스터 생성
- **위치**: `RimWorld/Verb_MechCluster.cs`

### 2.6 장비/도구 계열 (Equipment/Tool Classes)

#### Verb_Jump
- **네임스페이스**: `RimWorld`
- **상속**: `Verb`
- **설명**: 점프
- **위치**: `RimWorld/Verb_Jump.cs`

#### Verb_SmokePop
- **네임스페이스**: `RimWorld`
- **상속**: `Verb`
- **설명**: 연기 폭발 (XML에서는 `Verb_Smokepop`으로 표기)
- **위치**: `RimWorld/Verb_SmokePop.cs`

#### Verb_FirefoamPop
- **네임스페이스**: `RimWorld`
- **상속**: `Verb`
- **설명**: 소화제 폭발
- **위치**: `RimWorld/Verb_FirefoamPop.cs`

#### Verb_DeployBroadshield
- **네임스페이스**: `RimWorld`
- **상속**: `Verb`
- **설명**: 방패 배치
- **위치**: `RimWorld/Verb_DeployBroadshield.cs`

#### Verb_DeployToxPack
- **네임스페이스**: `RimWorld`
- **상속**: `Verb`
- **설명**: 독 팩 배치
- **위치**: `RimWorld/Verb_DeployToxPack.cs`

#### Verb_DeployDeadlifePack
- **네임스페이스**: `RimWorld`
- **상속**: `Verb`
- **설명**: 데드라이프 팩 배치
- **위치**: `RimWorld/Verb_DeployDeadlifePack.cs`

### 2.7 기타 특수 클래스 (Miscellaneous Classes)

#### Verb_Ignite
- **네임스페이스**: `RimWorld`
- **상속**: `Verb`
- **설명**: 점화
- **위치**: `RimWorld/Verb_Ignite.cs`

#### Verb_BeatFire
- **네임스페이스**: `RimWorld`
- **상속**: `Verb`
- **설명**: 불 꺼뜨리기
- **위치**: `RimWorld/Verb_BeatFire.cs`

---

## 3. Ratkin 프로젝트에서 사용하는 커스텀 Verb 클래스

### 3.1 NewRatkin.Verb_GunlanceFiring
- **상속**: `Verb_MeleeAttack`
- **설명**: 건랜스 포격 공격
- **위치**: `Project/1.6/Source/Gunlance/Verb_GunlanceFiring.cs`
- **사용 위치**: `Weapon_HighTech.xml` - 건랜스 무기

### 3.2 NewRatkin.Verb_MeleeAttackDamage
- **상속**: `Verb_MeleeAttack`
- **설명**: 근접 공격 피해 적용 (커스텀)
- **위치**: `Project/1.6/Source/RatkinGuerrilla/MeeleExplosion.cs`
- **사용 위치**: `Weapon_DropOnly.xml` - 매직완드

---

## 4. Verb 클래스 계층 구조

```
Verb (abstract)
├── Verb_LaunchProjectile
│   ├── Verb_Shoot
│   │   ├── Verb_ShootOneUse
│   │   └── Verb_AbilityShoot
│   └── Verb_LaunchProjectileStatic
│       ├── Verb_LaunchProjectileStaticOneUse
│       └── Verb_LaunchProjectileStaticPsychic
├── Verb_Spray (abstract)
│   └── Verb_ArcSpray
│       └── Verb_ArcSprayProjectile
├── Verb_ShootBeam
│   └── Verb_ArcSprayIncinerator
├── Verb_SpewFire
├── Verb_MeleeAttack (abstract)
│   ├── Verb_MeleeAttackDamage
│   ├── Verb_MeleeApplyHediff
│   └── NewRatkin.Verb_GunlanceFiring
│   └── NewRatkin.Verb_MeleeAttackDamage
├── Verb_CastAbility
│   ├── Verb_CastAbilityTouch
│   ├── Verb_CastAbilityJump
│   │   └── Verb_CastAbilityConsumeLeap
│   ├── Verb_CastPsycast
│   └── Verb_EntitySkip
├── Verb_CastBase (abstract)
│   ├── Verb_Spawn
│   ├── Verb_Bombardment
│   ├── Verb_PowerBeam
│   ├── Verb_MechCluster
│   └── Verb_CastTargetEffect
│       ├── Verb_CastTargetEffectLances
│       └── Verb_CastTargetEffectBiomutationLance
├── Verb_Jump
├── Verb_SmokePop
├── Verb_FirefoamPop
├── Verb_DeployBroadshield
├── Verb_DeployToxPack
├── Verb_DeployDeadlifePack
├── Verb_Ignite
└── Verb_BeatFire
```

---

## 5. 사용 빈도 요약

### XML에서 가장 많이 사용되는 verbClass (Top 10)

1. **Verb_Shoot** - 원거리 무기의 대부분
2. **Verb_CastAbility** - 능력의 대부분
3. **Verb_CastAbilityTouch** - 터치 능력
4. **Verb_MeleeAttackDamage** - 근접 전투 기동
5. **Verb_LaunchProjectile** - 투사체 발사
6. **Verb_LaunchProjectileStatic** - 정적 투사체
7. **Verb_ShootBeam** - 빔 발사
8. **Verb_CastAbilityJump** - 점프 능력
9. **Verb_AbilityShoot** - 능력 발사
10. **Verb_ShootOneUse** - 일회용 발사

---

## 6. 참고사항

### XML에서의 표기법
- XML에서는 네임스페이스를 생략하고 클래스명만 사용합니다.
- 예: `Verb_Shoot` (실제로는 `Verse.Verb_Shoot`)
- 커스텀 모드의 경우 네임스페이스 포함: `NewRatkin.Verb_GunlanceFiring`

### VerbProperties와의 관계
- `verbClass`는 `VerbProperties` 클래스의 `verbClass` 필드에 정의됩니다.
- XML에서 `<verbClass>` 태그로 지정합니다.

### 추상 클래스
- `Verb`, `Verb_MeleeAttack`, `Verb_Spray`, `Verb_CastBase`는 추상 클래스로 직접 사용할 수 없습니다.
- 반드시 상속받은 클래스를 사용해야 합니다.

---

## 7. 관련 파일

### RimWorld 소스 코드
- `RimworldSource/Verse/Verb.cs` - 기본 Verb 클래스
- `RimworldSource/Verse/VerbProperties.cs` - Verb 속성 정의

### Ratkin 프로젝트
- `Project/1.6/Source/Gunlance/Verb_GunlanceFiring.cs` - 건랜스 Verb
- `Project/1.6/Source/RatkinGuerrilla/MeeleExplosion.cs` - 근접 폭발 Verb

---

## 업데이트 날짜
2025-01-XX

---

## 관련 문서
- [Verb_DuplicateLoadID_Analysis_Report.md](19_Verb_DuplicateLoadID_Analysis_Report.md)
- [GunlanceAttack_Flow_Analysis.md](22_GunlanceAttack_Flow_Analysis.md)

