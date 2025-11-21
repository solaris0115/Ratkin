# RimWorld 기본 DamageDef 유형 참조

## 개요
RimWorld 기본 게임 및 DLC에서 정의된 모든 DamageDef를 DefName 기준으로 정리한 보고서입니다.

## Core DamageDef

### 근접 무기 (Damages_MeleeWeapon.xml)

#### Cut
- **부모**: CutBase
- **특이사항**: 베기 데미지, `cutExtraTargetsCurve`로 추가 타겟 생성, `cutCleaveBonus: 1.4`로 클리브 보너스

#### Crush
- **특이사항**: 압착 데미지, `armorCategory: Blunt`, `overkillPctToDestroyPart: 0.4~1.0`로 높은 부위 파괴 확률

#### Blunt
- **부모**: BluntBase
- **특이사항**: 둔기 데미지, `bluntStunDuration: 2.0`, `bluntInnerHitChance: 0.4`로 내부 타격 가능, `buildingDamageFactor: 1.5`

#### Poke
- **부모**: BluntBase
- **특이사항**: Blunt과 동일하지만 `DamageWorker_Stab` 사용, `stabChanceOfForcedInternal: 0.4`로 찌르기처럼 동작

#### Demolish
- **부모**: BluntBase
- **특이사항**: 건물 파괴 전용, `buildingDamageFactor: 10`, `buildingDamageFactorImpassable: 0.75`

#### Stab
- **특이사항**: 찌르기 데미지, `stabChanceOfForcedInternal: 0.6`, `overkillPctToDestroyPart: 0.4~1.0`

#### Scratch
- **특이사항**: 할퀴기 데미지, `scratchSplitPercentage: 0.67`로 데미지 분할, `overkillPctToDestroyPart: 0~0.7`

#### ScratchToxic
- **부모**: Scratch
- **특이사항**: 독성 할퀴기, `ToxicBuildup` 헤딥 추가, `impactSoundType: Toxic`

#### Bite
- **특이사항**: 물기 데미지, `overkillPctToDestroyPart: 0~0.1`로 낮은 부위 파괴 확률

#### ToxicBite
- **부모**: Bite
- **특이사항**: 독성 물기, `ToxicBuildup` 헤딥 추가

### 원거리 무기 (Damages_RangedWeapon.xml)

#### RangedStab
- **특이사항**: 원거리 찌르기, `isRanged: true`, `makesAnimalsFlee: true`, `stabChanceOfForcedInternal: 0.6`

#### Bullet
- **특이사항**: 총알 데미지, `isRanged: true`, `makesAnimalsFlee: true`, `overkillPctToDestroyPart: 0~0.7`

#### Arrow
- **특이사항**: 화살 데미지, `isRanged: true`, `makesAnimalsFlee: true`, `hediff: Cut` 사용

#### ArrowHighVelocity
- **부모**: Arrow
- **특이사항**: 고속 화살, `hediff: Stab` 사용 (부모와 다름)

### 환경 데미지 (Damages_Environmental.xml)

#### Flame
- **특이사항**: 화염 데미지, `harmsHealth: false`가 아님, `explosionHeatEnergyPerCell: 15`, `scaleDamageToBuildingsBasedOnFlammability: true`

#### Burn
- **부모**: Flame
- **특이사항**: 화염과 동일하지만 `DamageWorker_AddInjury` 사용, 불을 붙이지 않음

#### Frostbite
- **특이사항**: 동상 데미지, `externalViolence: false`, `canUseDeflectMetalEffect: false`

#### TornadoScratch
- **특이사항**: 토네이도 할퀴기, `impactSoundType: Tornado`

### 기타 데미지 (Damages_Misc.xml)

#### Deterioration
- **특이사항**: 부패 데미지, `hasForcefulImpact: false`, `makesBlood: false`, `canInterruptJobs: false`

#### Mining
- **특이사항**: 채굴 데미지 (기본 설정만)

#### Rotting
- **특이사항**: 부패 데미지, `hasForcefulImpact: false`, `makesBlood: false`, `canInterruptJobs: false`

#### Extinguish
- **특이사항**: 소화 데미지, `defaultDamage: 999999`, `harmsHealth: false`, `consideredHelpful: true`, `hediff: CoveredInFirefoam`

#### Bomb
- **특이사항**: 폭발 데미지, `isExplosive: true`, `armorCategory: Sharp`, `buildingDamageFactorImpassable: 4`, `buildingDamageFactorPassable: 2`, `plantDamageFactor: 4`, `corpseDamageFactor: 0.5`

#### BombSuper
- **부모**: Bomb
- **특이사항**: 초강력 폭발, `defaultDamage: 550`, `defaultArmorPenetration: 1.30` (100% 관통)

#### Smoke
- **특이사항**: 연기 데미지, `defaultDamage: 0`, `harmsHealth: false`, `canInterruptJobs: false`, `makesBlood: false`

#### Thump
- **특이사항**: 충격 데미지, `isExplosive: true`, `defaultDamage: 5`, `defaultArmorPenetration: 0`, `buildingDamageFactorImpassable: 15`, `buildingDamageFactorPassable: 7.5`

#### Vaporize
- **특이사항**: 증발 데미지, `DamageWorker_Vaporize` 사용, `defaultDamage: 800`, `defaultArmorPenetration: 1` (100% 관통), `armorCategory: Heat`, `makesAnimalsFlee: true`, `expolosionPropagationSpeed: 0.3`

#### AcidBurn
- **부모**: Flame
- **특이사항**: 산성 화상, `armorCategory: Sharp`, `scaleDamageToBuildingsBasedOnFlammability: false`

#### Decayed
- **특이사항**: 부패 장기 데미지, `hediff: Decayed`

#### Beam
- **특이사항**: 빔 데미지, `armorCategory: Heat`, `isRanged: true`, `makesAnimalsFlee: true`, `overkillPctToDestroyPart: 0~0.7`, `buildingDamageFactorImpassable: 0.4`, `buildingDamageFactorPassable: 0.2`

#### Nerve
- **부모**: Arrow
- **특이사항**: 신경 충격 데미지, `DamageWorker_Nerve` 사용

#### NerveStun
- **특이사항**: 신경 기절 데미지, `causeStun: true`, `stunAdaptationTicks: 240`

### 의료 데미지 (Damages_Medical.xml)

#### SurgicalCut
- **부모**: CutBase
- **특이사항**: 수술 절개, `harmAllLayersUntilOutside: false`, `hasForcefulImpact: false`, `canInterruptJobs: false`, `armorCategory` 없음

#### ExecutionCut
- **부모**: CutBase
- **특이사항**: 처형 절개, `execution: true`, `hasForcefulImpact: false`, `canInterruptJobs: false`, `armorCategory` 없음

### 기절 데미지 (Damages_Stun.xml)

#### StunBase
- **Abstract**: True
- **특이사항**: 추상 부모 클래스, `harmsHealth: false`, `makesBlood: false`

#### Stun
- **부모**: StunBase
- **특이사항**: 기절 데미지, `causeStun: true`, `defaultDamage: 20`

#### EMP
- **부모**: StunBase
- **특이사항**: EMP 데미지, `externalViolenceForMechanoids: true`, `harmsHealth: false`, `defaultDamage: 50`, `causeStun: true`, `stunAdaptationTicks: 2200`, `stunResistStat: EMPResistance` (Biotech/Anomaly 필요)

## Biotech DLC DamageDef

### 기타 데미지 (Damages_Misc.xml)

#### ToxGas
- **특이사항**: 독가스 데미지, `defaultDamage: 0`, `harmsHealth: false`, `canInterruptJobs: false`, `makesBlood: false`

### 원거리 무기 (Damages_RangedWeapon.xml)

#### BulletToxic
- **부모**: Bullet
- **특이사항**: 독성 총알, `ToxicBuildup` 헤딥 추가, `severityPerDamageDealt: 0.0065`

### 기절 데미지 (Damages_Stun.xml)

#### MechBandShockwave
- **부모**: StunBase
- **특이사항**: 기계 밴드 충격파, `externalViolenceForMechanoids: true`, `harmsHealth: false`, `defaultDamage: 50`, `causeStun: true`, `constantStunDurationTicks: 1200`, 특수 이펙터 사용

## Anomaly DLC DamageDef

### 기타 데미지 (Damages_Misc.xml)

#### Digested
- **특이사항**: 소화 데미지, `hediff: Digested`

#### EnergyBolt
- **특이사항**: 에너지 볼트, `isRanged: true`, `makesAnimalsFlee: true`, `igniteChanceByTargetFlammability`로 점화 확률, `igniteCellChance: 1`

#### Psychic
- **특이사항**: 사이킥 데미지, `hediff: PsychicInjury`, `hediffSkin: PsychicInjurySkin`, `hediffSolid: PsychicInjurySolid`

#### DeadlifeDust
- **특이사항**: 데드라이프 먼지, `defaultDamage: 0`, `harmsHealth: false`, `canInterruptJobs: false`, `makesBlood: false`

#### NociosphereVaporize
- **부모**: Vaporize
- **특이사항**: 노시오스피어 증발, 특수 사운드 사용

### 환경 데미지 (Damages_Environmental.xml)

#### ElectricalBurn
- **부모**: Flame
- **특이사항**: 전기 화상, `minDamageToFragment: 1`, `scaleDamageToBuildingsBasedOnFlammability: false`, 특수 색상 사용

## Odyssey DLC DamageDef

### 기타 데미지 (Damages_Misc.xml)

#### VacuumBurn
- **특이사항**: 진공 화상, `makesBlood: false`, `hasForcefulImpact: false`, `defaultArmorPenetration: 0`

#### MiningBomb
- **부모**: Bomb
- **특이사항**: 채굴 폭탄, `DamageWorker_MiningBomb` 사용, `buildingDamageFactorImpassable: 30`, `buildingDamageFactorPassable: 5`, `plantDamageFactor: 10`

### 원거리 무기 (Damages_RangedWeapon.xml)

#### Bullet_TraitTox
- **부모**: Bullet
- **특이사항**: 독성 특성 총알, `ToxicBuildup` 헤딥 추가, `severityPerDamageDealt: 0.015`, `impactSoundType: Toxic`

#### Bullet_TraitIncendiary
- **부모**: Bullet
- **특이사항**: 소이 특성 총알, `igniteChanceByTargetFlammability`로 점화 확률

#### BeamBypassShields
- **부모**: Beam
- **특이사항**: 실드 무시 빔, `ignoreShields: true`

### 근접 무기 (Damages_MeleeWeapon.xml)

#### PorcupineBite
- **부모**: Bite
- **특이사항**: 고슴도치 물기, `PorcupineQuill` 헤딥 추가, `applyAdditionalHediffsIfHuntingForFood: true`

#### PorcupineScratch
- **부모**: Scratch
- **특이사항**: 고슴도치 할퀴기, `PorcupineQuill` 헤딥 추가, `applyAdditionalHediffsIfHuntingForFood: true`

## DamageDef 통계

### 총 개수
- **Core**: 30개
- **Biotech**: 3개
- **Anomaly**: 6개
- **Odyssey**: 6개
- **합계**: 45개

### 카테고리별 분류

#### 근접 무기 (10개)
Cut, Crush, Blunt, Poke, Demolish, Stab, Scratch, ScratchToxic, Bite, ToxicBite, PorcupineBite, PorcupineScratch

#### 원거리 무기 (7개)
RangedStab, Bullet, Arrow, ArrowHighVelocity, BulletToxic, Bullet_TraitTox, Bullet_TraitIncendiary

#### 환경 데미지 (6개)
Flame, Burn, Frostbite, TornadoScratch, ElectricalBurn, VacuumBurn

#### 폭발/충격 (5개)
Bomb, BombSuper, Thump, Vaporize, MiningBomb, NociosphereVaporize

#### 기절/EMP (4개)
Stun, EMP, NerveStun, MechBandShockwave

#### 특수 데미지 (13개)
Deterioration, Mining, Rotting, Extinguish, Smoke, AcidBurn, Decayed, Beam, Nerve, SurgicalCut, ExecutionCut, Digested, EnergyBolt, Psychic, DeadlifeDust, ToxGas, BeamBypassShields

## DamageDef별 Hediff 및 속성 상세

### 근접 무기 데미지

#### Cut
- **Hediff**: Cut (일반), Cut (피부), Crack (고체)
- **고통 수치**: 0.0125
- **출혈량**: 0.06
- **감염 확률**: 15%
- **특이사항**: 베기 데미지, 클리브 보너스 1.4배

#### Crush
- **Hediff**: Crush (일반), Cut (피부), Crack (고체)
- **고통 수치**: 0.0125
- **출혈량**: 0.01 (매우 낮음)
- **감염 확률**: 15%
- **특이사항**: 압착 데미지, 출혈이 거의 없음

#### Blunt
- **Hediff**: Crush (일반), Bruise (피부), Crack (고체)
- **고통 수치**: 0.0125
- **출혈량**: 없음 (Bruise는 출혈 없음)
- **감염 확률**: 없음 (Bruise는 감염 없음)
- **특이사항**: 둔기 데미지, 기절 효과 가능, 내부 타격 가능

#### Poke
- **Hediff**: Blunt과 동일 (Crush, Bruise, Crack)
- **고통 수치**: 0.0125
- **출혈량**: 없음
- **특이사항**: Blunt과 동일하지만 찌르기처럼 동작

#### Stab
- **Hediff**: Stab (일반), Crack (고체)
- **고통 수치**: 0.0125
- **출혈량**: 0.06
- **감염 확률**: 15%
- **특이사항**: 찌르기 데미지, 내부 강제 타격 확률 60%

#### Scratch
- **Hediff**: Scratch (일반), Crack (고체)
- **고통 수치**: 0.0125
- **출혈량**: 0.06
- **감염 확률**: 15%
- **특이사항**: 할퀴기 데미지, 데미지 분할 67%

#### ScratchToxic
- **Hediff**: Scratch와 동일 + ToxicBuildup 추가
- **고통 수치**: 0.0125
- **출혈량**: 0.06
- **감염 확률**: 15%
- **특이사항**: 독성 할퀴기, ToxicBuildup 헤딥 추가

#### Bite
- **Hediff**: Bite (일반), Crack (고체)
- **고통 수치**: 0.0125
- **출혈량**: 0.06
- **감염 확률**: 30% (가장 높음)
- **특이사항**: 물기 데미지, 감염 확률이 높음

#### ToxicBite
- **Hediff**: Bite와 동일 + ToxicBuildup 추가
- **고통 수치**: 0.0125
- **출혈량**: 0.06
- **감염 확률**: 30%
- **특이사항**: 독성 물기, ToxicBuildup 헤딥 추가

### 원거리 무기 데미지

#### Bullet
- **Hediff**: Gunshot
- **고통 수치**: 0.0125
- **출혈량**: 0.06
- **감염 확률**: 15%
- **특이사항**: 총알 데미지, 모든 레이어 관통

#### Arrow
- **Hediff**: Cut (일반), Crack (고체)
- **고통 수치**: 0.0125
- **출혈량**: 0.06
- **감염 확률**: 15%
- **특이사항**: 화살 데미지, Cut 헤딥 사용

#### ArrowHighVelocity
- **Hediff**: Stab (일반), Crack (고체)
- **고통 수치**: 0.0125
- **출혈량**: 0.06
- **감염 확률**: 15%
- **특이사항**: 고속 화살, Stab 헤딥 사용 (부모와 다름)

#### RangedStab
- **Hediff**: Stab (일반), Crack (고체)
- **고통 수치**: 0.0125
- **출혈량**: 0.06
- **감염 확률**: 15%
- **특이사항**: 원거리 찌르기, 내부 강제 타격 확률 60%

### 환경 데미지

#### Flame
- **Hediff**: Burn
- **고통 수치**: 0.01875 (가장 높음, 50% 증가)
- **출혈량**: 없음
- **감염 확률**: 30%
- **특이사항**: 화염 데미지, 불을 붙임, 고통 수치가 매우 높음

#### Burn
- **Hediff**: Burn
- **고통 수치**: 0.01875
- **출혈량**: 없음
- **감염 확률**: 30%
- **특이사항**: Flame과 동일하지만 불을 붙이지 않음

#### AcidBurn
- **Hediff**: AcidBurn
- **고통 수치**: 0.01875 (BurnBase 상속)
- **출혈량**: 없음
- **감염 확률**: 30%
- **특이사항**: 산성 화상, 화염 계열 중 가장 높은 고통

#### Frostbite
- **Hediff**: Frostbite
- **고통 수치**: 0.0125
- **출혈량**: 없음
- **감염 확률**: 25%
- **특이사항**: 동상 데미지, 외부 폭력으로 간주 안됨

#### TornadoScratch
- **Hediff**: Scratch (일반), Crack (고체)
- **고통 수치**: 0.0125
- **출혈량**: 0.06
- **감염 확률**: 15%
- **특이사항**: 토네이도 할퀴기

### 폭발/충격 데미지

#### Bomb
- **Hediff**: Shredded (일반), Crack (고체)
- **고통 수치**: 0.0125
- **출혈량**: 0.06
- **감염 확률**: 20%
- **특이사항**: 폭발 데미지, 모든 레이어 관통

#### BombSuper
- **Hediff**: Bomb와 동일 (Shredded, Crack)
- **고통 수치**: 0.0125
- **출혈량**: 0.06
- **감염 확률**: 20%
- **특이사항**: 초강력 폭발, 100% 방어관통

#### Thump
- **Hediff**: Crush (일반), Crack (고체)
- **고통 수치**: 0.0125
- **출혈량**: 0.01
- **감염 확률**: 15%
- **특이사항**: 충격 데미지, 출혈이 거의 없음

#### Vaporize
- **Hediff**: Burn
- **고통 수치**: 0.01875
- **출혈량**: 없음
- **감염 확률**: 30%
- **특이사항**: 증발 데미지, 100% 방어관통, 화염 계열 고통

### 특수 데미지

#### Beam
- **Hediff**: BeamWound
- **고통 수치**: 0.0125
- **출혈량**: 0.06
- **감염 확률**: 없음
- **특이사항**: 빔 데미지, 열 방어 카테고리

#### Decayed
- **Hediff**: Decayed
- **고통 수치**: 없음 (영구 헤딥만)
- **출혈량**: 없음
- **감염 확률**: 없음
- **특이사항**: 부패 장기, 치료 불가능

#### Extinguish
- **Hediff**: CoveredInFirefoam
- **고통 수치**: 없음
- **출혈량**: 없음
- **특이사항**: 소화 데미지, 체력 해치지 않음, 도움되는 데미지

#### Smoke
- **Hediff**: 없음
- **고통 수치**: 없음
- **출혈량**: 없음
- **특이사항**: 연기 데미지, 체력 해치지 않음

## 특이사항 요약

### 고통 수치 (Pain Per Severity)
각 DamageDef가 생성하는 Hediff의 고통 수치:

- **화염 계열 (가장 높음)**: `painPerSeverity: 0.01875`
  - Burn (Flame 데미지)
  - ElectricalBurn
  - ChemicalBurn
  - AcidBurn
  - Vaporize (Burn 헤딥 사용)

- **일반 데미지**: `painPerSeverity: 0.0125`
  - Cut, Crush, Stab, Scratch, Bite
  - Gunshot, Shredded, Bruise
  - Frostbite, BeamWound

- **가장 낮음**: `painPerSeverity: 0.01`
  - Crack (고체 부위)

**결론**: 화염 피해는 다른 데미지 타입보다 **50% 더 높은 고통 수치**를 가집니다.

### 출혈량 (Bleed Rate)
- **높은 출혈**: 0.06
  - Cut, Stab, Scratch, Bite, Gunshot, Shredded, BeamWound
- **낮은 출혈**: 0.01
  - Crush, Thump
- **출혈 없음**: 0
  - Burn 계열 (화염 데미지)
  - Bruise (Blunt 피부)
  - Crack (고체 부위)

### 감염 확률 (Infection Chance)
- **가장 높음**: 30%
  - Burn 계열 (화염 데미지)
  - Bite (물기)
- **높음**: 25%
  - Frostbite (동상)
- **보통**: 20%
  - Shredded (폭발)
- **일반**: 15%
  - Cut, Crush, Stab, Scratch, Gunshot, Arrow 등 대부분
- **없음**: 0%
  - Bruise, BeamWound, Decayed

### 100% 방어관통
- **BombSuper**: `defaultArmorPenetration: 1.30`
- **Vaporize**: `defaultArmorPenetration: 1`

### 체력을 해치지 않는 데미지
- **StunBase 계열**: Stun, EMP, MechBandShockwave
- **Extinguish**: `harmsHealth: false`
- **Smoke**: `harmsHealth: false`
- **ToxGas**: `harmsHealth: false`
- **DeadlifeDust**: `harmsHealth: false`

### 독성 데미지
- **ScratchToxic**: ToxicBuildup 추가
- **ToxicBite**: ToxicBuildup 추가
- **BulletToxic**: ToxicBuildup 추가
- **Bullet_TraitTox**: ToxicBuildup 추가

### 점화 가능 데미지
- **EnergyBolt**: `igniteChanceByTargetFlammability`, `igniteCellChance: 1`
- **Bullet_TraitIncendiary**: `igniteChanceByTargetFlammability`

### 건물 파괴 특화
- **Demolish**: `buildingDamageFactor: 10`
- **MiningBomb**: `buildingDamageFactorImpassable: 30`

### 실드 무시
- **BeamBypassShields**: `ignoreShields: true`
