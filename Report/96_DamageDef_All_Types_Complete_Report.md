# DamageDef 모든 유형 완전 분석 보고서

**태그**: DamageDef Types Complete Analysis RimWorld Core Biotech Anomaly Odyssey Custom

## 개요

RimWorld 기본 게임, 모든 DLC, 그리고 Ratkin 프로젝트 커스텀 DamageDef를 포함한 모든 DamageDef 유형을 완전히 정리한 보고서입니다.

---

## 통계 요약

### 총 개수
- **Core**: 30개
- **Biotech**: 3개
- **Anomaly**: 6개
- **Odyssey**: 6개
- **Ratkin 프로젝트**: 2개
- **합계**: 47개

### 카테고리별 분류

| 카테고리 | 개수 | DamageDef 목록 |
|---------|------|----------------|
| 근접 무기 | 12개 | Cut, Crush, Blunt, Poke, Demolish, Stab, Scratch, ScratchToxic, Bite, ToxicBite, PorcupineBite, PorcupineScratch |
| 원거리 무기 | 9개 | RangedStab, Bullet, Arrow, ArrowHighVelocity, BulletToxic, Bullet_TraitTox, Bullet_TraitIncendiary, Beam, BeamBypassShields |
| 환경 데미지 | 6개 | Flame, Burn, Frostbite, TornadoScratch, ElectricalBurn, VacuumBurn |
| 폭발/충격 | 6개 | Bomb, BombSuper, Thump, Vaporize, MiningBomb, NociosphereVaporize |
| 기절/EMP | 4개 | Stun, EMP, NerveStun, MechBandShockwave |
| 특수 데미지 | 10개 | Deterioration, Mining, Rotting, Extinguish, Smoke, AcidBurn, Decayed, Nerve, SurgicalCut, ExecutionCut, Digested, EnergyBolt, Psychic, DeadlifeDust, ToxGas |

---

## Core DamageDef (30개)

### 근접 무기 (Damages_MeleeWeapon.xml)

#### Cut
- **부모**: CutBase (Abstract)
- **WorkerClass**: DamageWorker_Cut
- **Hediff**: Cut (일반), Cut (피부), Crack (고체)
- **ArmorCategory**: Sharp
- **특이사항**: 
  - 베기 데미지, `cutExtraTargetsCurve`로 추가 타겟 생성
  - `cutCleaveBonus: 1.4`로 클리브 보너스
  - `harmAllLayersUntilOutside: true` - 모든 레이어 관통
  - `overkillPctToDestroyPart: 0~0.1` - 낮은 부위 파괴 확률

#### Crush
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: Crush (일반), Cut (피부), Crack (고체)
- **ArmorCategory**: Blunt
- **특이사항**: 
  - 압착 데미지
  - `overkillPctToDestroyPart: 0.4~1.0` - 높은 부위 파괴 확률
  - 출혈량이 매우 낮음 (0.01)

#### Blunt
- **부모**: BluntBase (Abstract)
- **WorkerClass**: DamageWorker_Blunt
- **Hediff**: Crush (일반), Bruise (피부), Crack (고체)
- **ArmorCategory**: Blunt
- **특이사항**: 
  - 둔기 데미지
  - `bluntStunDuration: 2.0` - 기절 지속 시간
  - `bluntInnerHitChance: 0.4` - 내부 타격 가능
  - `buildingDamageFactor: 1.5` - 건물 데미지 보너스
  - 출혈 없음 (Bruise는 출혈 없음)

#### Poke
- **부모**: BluntBase
- **WorkerClass**: DamageWorker_Stab
- **Hediff**: Blunt과 동일 (Crush, Bruise, Crack)
- **특이사항**: 
  - Blunt과 동일하지만 `DamageWorker_Stab` 사용
  - `stabChanceOfForcedInternal: 0.4` - 찌르기처럼 동작
  - 소총 총구 등에 사용

#### Demolish
- **부모**: BluntBase
- **Hediff**: Blunt과 동일
- **특이사항**: 
  - 건물 파괴 전용
  - `buildingDamageFactor: 10` - 건물 데미지 10배
  - `buildingDamageFactorImpassable: 0.75` - 통과 불가능한 건물에 대한 보정

#### Stab
- **WorkerClass**: DamageWorker_Stab
- **Hediff**: Stab (일반), Crack (고체)
- **ArmorCategory**: Sharp
- **특이사항**: 
  - 찌르기 데미지
  - `stabChanceOfForcedInternal: 0.6` - 내부 강제 타격 확률 60%
  - `overkillPctToDestroyPart: 0.4~1.0` - 높은 부위 파괴 확률

#### Scratch
- **부모**: Scratch (Abstract)
- **WorkerClass**: DamageWorker_Scratch
- **Hediff**: Scratch (일반), Crack (고체)
- **ArmorCategory**: Sharp
- **특이사항**: 
  - 할퀴기 데미지
  - `scratchSplitPercentage: 0.67` - 데미지 분할 67%
  - `overkillPctToDestroyPart: 0~0.7` - 중간 부위 파괴 확률

#### ScratchToxic
- **부모**: Scratch
- **Hediff**: Scratch + ToxicBuildup 추가
- **특이사항**: 
  - 독성 할퀴기
  - `ToxicBuildup` 헤딥 추가 (`severityPerDamageDealt: 0.015`)
  - `impactSoundType: Toxic`
  - `damageEffecter: Impact_Toxic`

#### Bite
- **부모**: Bite (Abstract)
- **WorkerClass**: DamageWorker_Bite
- **Hediff**: Bite (일반), Crack (고체)
- **ArmorCategory**: Sharp
- **특이사항**: 
  - 물기 데미지
  - `overkillPctToDestroyPart: 0~0.1` - 낮은 부위 파괴 확률
  - 감염 확률 30% (가장 높음)

#### ToxicBite
- **부모**: Bite
- **Hediff**: Bite + ToxicBuildup 추가
- **특이사항**: 
  - 독성 물기
  - `ToxicBuildup` 헤딥 추가 (`severityPerDamageDealt: 0.015`)
  - `impactSoundType: Toxic`
  - `damageEffecter: Impact_Toxic`

### 원거리 무기 (Damages_RangedWeapon.xml)

#### RangedStab
- **WorkerClass**: DamageWorker_Stab
- **Hediff**: Stab (일반), Crack (고체)
- **ArmorCategory**: Sharp
- **특이사항**: 
  - 원거리 찌르기
  - `isRanged: true`
  - `makesAnimalsFlee: true`
  - `stabChanceOfForcedInternal: 0.6`
  - `overkillPctToDestroyPart: 0~0.7`

#### Bullet
- **부모**: Bullet (Abstract)
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: Gunshot
- **ArmorCategory**: Sharp
- **특이사항**: 
  - 총알 데미지
  - `isRanged: true`
  - `makesAnimalsFlee: true`
  - `harmAllLayersUntilOutside: true` - 모든 레이어 관통
  - `overkillPctToDestroyPart: 0~0.7`

#### Arrow
- **부모**: Arrow (Abstract)
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: Cut (일반), Crack (고체)
- **ArmorCategory**: Sharp
- **특이사항**: 
  - 화살 데미지
  - `isRanged: true`
  - `makesAnimalsFlee: true`
  - `harmAllLayersUntilOutside: true`
  - `overkillPctToDestroyPart: 0~0.7`

#### ArrowHighVelocity
- **부모**: Arrow
- **Hediff**: Stab (일반), Crack (고체) - 부모와 다름
- **특이사항**: 
  - 고속 화살
  - 부모(Arrow)는 Cut 헤딥 사용, 이건 Stab 헤딥 사용

### 환경 데미지 (Damages_Environmental.xml)

#### Flame
- **부모**: Flame (Abstract)
- **WorkerClass**: DamageWorker_Flame
- **Hediff**: Burn
- **ArmorCategory**: Heat
- **특이사항**: 
  - 화염 데미지
  - `defaultDamage: 10`
  - `explosionHeatEnergyPerCell: 15` - 폭발 시 셀당 열 에너지
  - `scaleDamageToBuildingsBasedOnFlammability: true` - 가연성에 따라 건물 데미지 조정
  - 불을 붙임
  - 고통 수치 0.01875 (가장 높음, 50% 증가)

#### Burn
- **부모**: Flame
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: Burn
- **특이사항**: 
  - Flame과 동일하지만 불을 붙이지 않음
  - 고통 수치 0.01875

#### Frostbite
- **WorkerClass**: DamageWorker_Frostbite
- **Hediff**: Frostbite
- **특이사항**: 
  - 동상 데미지
  - `externalViolence: false` - 외부 폭력으로 간주 안됨
  - `canUseDeflectMetalEffect: false`
  - 감염 확률 25%

#### TornadoScratch
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: Scratch (일반), Crack (고체)
- **ArmorCategory**: Sharp
- **특이사항**: 
  - 토네이도 할퀴기
  - `impactSoundType: Tornado`
  - `overkillPctToDestroyPart: 0~0.7`

### 기타 데미지 (Damages_Misc.xml)

#### Deterioration
- **특이사항**: 
  - 부패 데미지
  - `hasForcefulImpact: false`
  - `makesBlood: false`
  - `canInterruptJobs: false`

#### Mining
- **특이사항**: 
  - 채굴 데미지
  - 기본 설정만 있음

#### Rotting
- **특이사항**: 
  - 부패 데미지
  - `hasForcefulImpact: false`
  - `makesBlood: false`
  - `canInterruptJobs: false`

#### Extinguish
- **WorkerClass**: DamageWorker_Extinguish
- **Hediff**: CoveredInFirefoam
- **특이사항**: 
  - 소화 데미지
  - `defaultDamage: 999999` (실제로는 체력 해치지 않음)
  - `harmsHealth: false`
  - `consideredHelpful: true` - 도움되는 데미지
  - 체력 해치지 않음

#### Bomb
- **부모**: Bomb (Abstract)
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: Shredded (일반), Crack (고체)
- **ArmorCategory**: Sharp
- **특이사항**: 
  - 폭발 데미지
  - `isExplosive: true`
  - `defaultDamage: 50`
  - `defaultArmorPenetration: 0.10`
  - `buildingDamageFactorImpassable: 4`
  - `buildingDamageFactorPassable: 2`
  - `plantDamageFactor: 4`
  - `corpseDamageFactor: 0.5`
  - `harmAllLayersUntilOutside: true` - 모든 레이어 관통

#### BombSuper
- **부모**: Bomb
- **Hediff**: Bomb와 동일 (Shredded, Crack)
- **특이사항**: 
  - 초강력 폭발
  - `defaultDamage: 550`
  - `defaultArmorPenetration: 1.30` (100% 관통)
  - `defaultStoppingPower: 2.0`

#### Smoke
- **특이사항**: 
  - 연기 데미지
  - `defaultDamage: 0`
  - `harmsHealth: false`
  - `canInterruptJobs: false`
  - `makesBlood: false`
  - 체력 해치지 않음

#### Thump
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: Crush (일반), Crack (고체)
- **ArmorCategory**: Sharp
- **특이사항**: 
  - 충격 데미지
  - `isExplosive: true`
  - `defaultDamage: 5`
  - `defaultArmorPenetration: 0`
  - `buildingDamageFactorImpassable: 15`
  - `buildingDamageFactorPassable: 7.5`
  - `plantDamageFactor: 4`
  - 출혈량 0.01 (매우 낮음)

#### Vaporize
- **부모**: Vaporize (Abstract)
- **WorkerClass**: DamageWorker_Vaporize
- **Hediff**: Burn
- **ArmorCategory**: Heat
- **특이사항**: 
  - 증발 데미지
  - `defaultDamage: 800`
  - `defaultArmorPenetration: 1` (100% 관통)
  - `defaultStoppingPower: 1.5`
  - `buildingDamageFactorImpassable: 4`
  - `buildingDamageFactorPassable: 2`
  - `plantDamageFactor: 2`
  - `expolosionPropagationSpeed: 0.3`
  - `makesAnimalsFlee: true`
  - 고통 수치 0.01875 (화염 계열)

#### AcidBurn
- **부모**: Flame
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: AcidBurn
- **ArmorCategory**: Sharp (부모와 다름)
- **특이사항**: 
  - 산성 화상
  - `scaleDamageToBuildingsBasedOnFlammability: false`
  - 고통 수치 0.01875 (BurnBase 상속)

#### Decayed
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: Decayed
- **특이사항**: 
  - 부패 장기 데미지
  - 치료 불가능한 영구 헤딥
  - 고통 수치 없음

#### Beam
- **부모**: Beam (Abstract)
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: BeamWound
- **ArmorCategory**: Heat
- **특이사항**: 
  - 빔 데미지
  - `isRanged: true`
  - `makesAnimalsFlee: true`
  - `defaultDamage: 10`
  - `defaultArmorPenetration: 0.5`
  - `buildingDamageFactorImpassable: 0.4`
  - `buildingDamageFactorPassable: 0.2`
  - `overkillPctToDestroyPart: 0~0.7`
  - 감염 확률 없음

#### Nerve
- **부모**: Arrow
- **WorkerClass**: DamageWorker_Nerve
- **특이사항**: 
  - 신경 충격 데미지
  - Arrow의 모든 속성 상속 (`isRanged: true` 등)

#### NerveStun
- **특이사항**: 
  - 신경 기절 데미지
  - `causeStun: true`
  - `stunAdaptationTicks: 240`

### 의료 데미지 (Damages_Medical.xml)

#### SurgicalCut
- **부모**: CutBase
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: SurgicalCut
- **특이사항**: 
  - 수술 절개
  - `harmAllLayersUntilOutside: false` - 외부 부위를 해치지 않고 내부 장기만 제거 가능
  - `hasForcefulImpact: false`
  - `canInterruptJobs: false`
  - `armorCategory` 없음

#### ExecutionCut
- **부모**: CutBase
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: ExecutionCut (일반), ExecutionCut (피부)
- **특이사항**: 
  - 처형 절개
  - `execution: true`
  - `hasForcefulImpact: false`
  - `canInterruptJobs: false`
  - `armorCategory` 없음

### 기절 데미지 (Damages_Stun.xml)

#### StunBase
- **Abstract**: True
- **특이사항**: 
  - 추상 부모 클래스
  - `harmsHealth: false`
  - `makesBlood: false`

#### Stun
- **부모**: StunBase
- **WorkerClass**: DamageWorker_Stun
- **특이사항**: 
  - 기절 데미지
  - `causeStun: true`
  - `defaultDamage: 20`
  - `harmsHealth: false`

#### EMP
- **부모**: StunBase
- **특이사항**: 
  - EMP 데미지
  - `externalViolenceForMechanoids: true` - 메카노이드에게는 외부 폭력
  - `harmsHealth: false`
  - `defaultDamage: 50`
  - `causeStun: true`
  - `stunAdaptationTicks: 2200`
  - `stunResistStat: EMPResistance` (Biotech/Anomaly 필요)
  - `impactSoundType: Electric`
  - `explosionCellFleck: BlastEMP`
  - `explosionInteriorFleck: ElectricalSpark`

---

## Biotech DLC DamageDef (3개)

### 기타 데미지 (Damages_Misc.xml)

#### ToxGas
- **특이사항**: 
  - 독가스 데미지
  - `defaultDamage: 0`
  - `harmsHealth: false`
  - `canInterruptJobs: false`
  - `makesBlood: false`
  - 체력 해치지 않음

### 원거리 무기 (Damages_RangedWeapon.xml)

#### BulletToxic
- **부모**: Bullet
- **Hediff**: Gunshot + ToxicBuildup 추가
- **특이사항**: 
  - 독성 총알
  - `ToxicBuildup` 헤딥 추가 (`severityPerDamageDealt: 0.0065`)
  - `impactSoundType: Bullet`

### 기절 데미지 (Damages_Stun.xml)

#### MechBandShockwave
- **부모**: StunBase
- **WorkerClass**: DamageWorker_Stun
- **특이사항**: 
  - 기계 밴드 충격파
  - `externalViolenceForMechanoids: true`
  - `harmsHealth: false`
  - `defaultDamage: 50`
  - `causeStun: true`
  - `constantStunDurationTicks: 1200` - 고정 기절 시간
  - `impactSoundType: MechBandShockwave`
  - 특수 이펙터 사용 (`BlastMechBandShockwave`, `MechBandElectricityArc`)

---

## Anomaly DLC DamageDef (6개)

### 기타 데미지 (Damages_Misc.xml)

#### Digested
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: Digested
- **특이사항**: 
  - 소화 데미지
  - 특수 헤딥 사용

#### EnergyBolt
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: EnergyBolt
- **ArmorCategory**: Sharp
- **특이사항**: 
  - 에너지 볼트
  - `isRanged: true`
  - `makesAnimalsFlee: true`
  - `harmAllLayersUntilOutside: true`
  - `overkillPctToDestroyPart: 0~0.7`
  - `igniteChanceByTargetFlammability` - 가연성에 따라 점화 확률
  - `igniteCellChance: 1` - 셀 점화 확률 100%
  - `impactSoundType: Electric`

#### Psychic
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: PsychicInjury (일반), PsychicInjurySkin (피부), PsychicInjurySolid (고체)
- **특이사항**: 
  - 사이킥 데미지
  - `externalViolence: true`
  - 특수 헤딥 사용

#### DeadlifeDust
- **특이사항**: 
  - 데드라이프 먼지
  - `defaultDamage: 0`
  - `harmsHealth: false`
  - `canInterruptJobs: false`
  - `makesBlood: false`
  - 체력 해치지 않음

#### NociosphereVaporize
- **부모**: Vaporize
- **특이사항**: 
  - 노시오스피어 증발
  - Vaporize의 모든 속성 상속
  - 특수 사운드 사용 (`FleshmelterBolt_Blast`)

### 환경 데미지 (Damages_Environmental.xml)

#### ElectricalBurn
- **부모**: Flame
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: ElectricalBurn
- **특이사항**: 
  - 전기 화상
  - `minDamageToFragment: 1`
  - `scaleDamageToBuildingsBasedOnFlammability: false`
  - 특수 색상 사용 (파란색 계열)
  - 고통 수치 0.01875 (Flame 상속)

---

## Odyssey DLC DamageDef (6개)

### 근접 무기 (Damages_MeleeWeapon.xml)

#### PorcupineBite
- **부모**: Bite
- **Hediff**: Bite + PorcupineQuill 추가
- **특이사항**: 
  - 고슴도치 물기
  - `PorcupineQuill` 헤딥 추가 (`additionalHediffsThisPart`)
  - `applyAdditionalHediffsIfHuntingForFood: true` - 사냥 시에만 추가 헤딥 적용

#### PorcupineScratch
- **부모**: Scratch
- **Hediff**: Scratch + PorcupineQuill 추가
- **특이사항**: 
  - 고슴도치 할퀴기
  - `PorcupineQuill` 헤딥 추가 (`additionalHediffsThisPart`)
  - `applyAdditionalHediffsIfHuntingForFood: true`

### 기타 데미지 (Damages_Misc.xml)

#### VacuumBurn
- **WorkerClass**: DamageWorker_AddInjury
- **Hediff**: VacuumBurn
- **특이사항**: 
  - 진공 화상
  - `makesBlood: false`
  - `hasForcefulImpact: false`
  - `defaultArmorPenetration: 0`

#### MiningBomb
- **부모**: Bomb
- **WorkerClass**: DamageWorker_MiningBomb
- **Hediff**: Bomb와 동일 (Shredded, Crack)
- **특이사항**: 
  - 채굴 폭탄
  - `buildingDamageFactorImpassable: 30` - 통과 불가능한 건물에 매우 높은 데미지
  - `buildingDamageFactorPassable: 5` - 통과 가능한 건물에 높은 데미지
  - `plantDamageFactor: 10` - 식물에 높은 데미지

### 원거리 무기 (Damages_RangedWeapon.xml)

#### Bullet_TraitTox
- **부모**: Bullet
- **Hediff**: Gunshot + ToxicBuildup 추가
- **특이사항**: 
  - 독성 특성 총알
  - `ToxicBuildup` 헤딥 추가 (`severityPerDamageDealt: 0.015`)
  - `impactSoundType: Toxic`
  - `damageEffecter: Impact_Toxic`

#### Bullet_TraitIncendiary
- **부모**: Bullet
- **Hediff**: Gunshot
- **특이사항**: 
  - 소이 특성 총알
  - `igniteChanceByTargetFlammability` - 가연성에 따라 점화 확률 (0~30%)
  - 불을 붙일 수 있음

#### BeamBypassShields
- **부모**: Beam
- **Hediff**: BeamWound
- **특이사항**: 
  - 실드 무시 빔
  - `ignoreShields: true` - 실드를 무시하고 통과
  - Beam의 모든 속성 상속 (`isRanged: true` 등)

---

## Ratkin 프로젝트 커스텀 DamageDef (2개)

### RK_EMP
- **부모**: StunBase
- **파일**: `Project/1.6/Defs/DamageDefs/Damage_Def.xml`
- **특이사항**: 
  - 커스텀 EMP 데미지
  - `externalViolenceForMechanoids: true`
  - `harmsHealth: false`
  - `defaultDamage: 50`
  - `impactSoundType: Electric`
  - `explosionCellFleck: BlastEMP`
  - `explosionInteriorFleck: ElectricalSpark`
  - 커스텀 사운드 사용 (`RK_Sound_Emp_Crack`)

### RK_Damage_PickaxeStab
- **파일**: `Project/1.6/Defs/DamageDefs/Damage_Def.xml`
- **WorkerClass**: DamageWorker_Stab
- **Hediff**: Stab (일반), Crack (고체)
- **ArmorCategory**: Sharp
- **특이사항**: 
  - 곡괭이용 찌르기 데미지
  - `stabChanceOfForcedInternal: 0.6`
  - `overkillPctToDestroyPart: 0.4~1.0`
  - `buildingDamageFactor: 10` - 건물에 추가 피해 (Demolish와 동일)
  - `impactSoundType: Blunt` - 둔기 사운드 사용

---

## DamageDef 속성 상세 분석

### 고통 수치 (Pain Per Severity)

각 DamageDef가 생성하는 Hediff의 고통 수치:

- **화염 계열 (가장 높음)**: `painPerSeverity: 0.01875` (50% 증가)
  - Burn (Flame 데미지)
  - ElectricalBurn
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

### 방어관통 (Armor Penetration)

- **100% 방어관통**:
  - **BombSuper**: `defaultArmorPenetration: 1.30`
  - **Vaporize**: `defaultArmorPenetration: 1`

- **높은 방어관통**:
  - **Beam**: `defaultArmorPenetration: 0.5`
  - **Bomb**: `defaultArmorPenetration: 0.10`

### 체력을 해치지 않는 데미지

- **StunBase 계열**: Stun, EMP, MechBandShockwave
  - `harmsHealth: false`
- **Extinguish**: `harmsHealth: false`
- **Smoke**: `harmsHealth: false`
- **ToxGas**: `harmsHealth: false`
- **DeadlifeDust**: `harmsHealth: false`

### 독성 데미지

독성 헤딥을 추가하는 DamageDef:

- **ScratchToxic**: ToxicBuildup 추가 (`severityPerDamageDealt: 0.015`)
- **ToxicBite**: ToxicBuildup 추가 (`severityPerDamageDealt: 0.015`)
- **BulletToxic**: ToxicBuildup 추가 (`severityPerDamageDealt: 0.0065`)
- **Bullet_TraitTox**: ToxicBuildup 추가 (`severityPerDamageDealt: 0.015`)

### 점화 가능 데미지

- **EnergyBolt**: 
  - `igniteChanceByTargetFlammability` (0~100%)
  - `igniteCellChance: 1` (100%)
- **Bullet_TraitIncendiary**: 
  - `igniteChanceByTargetFlammability` (0~30%)

### 건물 파괴 특화

- **Demolish**: `buildingDamageFactor: 10`
- **RK_Damage_PickaxeStab**: `buildingDamageFactor: 10`
- **MiningBomb**: 
  - `buildingDamageFactorImpassable: 30`
  - `buildingDamageFactorPassable: 5`
- **Thump**: 
  - `buildingDamageFactorImpassable: 15`
  - `buildingDamageFactorPassable: 7.5`

### 실드 무시

- **BeamBypassShields**: `ignoreShields: true`

### 원거리 데미지 (`isRanged: true`)

- RangedStab
- Bullet (및 모든 자식)
- Arrow (및 모든 자식)
- Beam (및 모든 자식)
- EnergyBolt

### 폭발 데미지 (`isExplosive: true`)

- Bomb (및 모든 자식)
- Thump

### WorkerClass별 분류

- **DamageWorker_AddInjury**: 가장 일반적인 데미지 처리
- **DamageWorker_Cut**: 베기 데미지 (추가 타겟 생성)
- **DamageWorker_Blunt**: 둔기 데미지 (기절 효과)
- **DamageWorker_Stab**: 찌르기 데미지 (내부 타격)
- **DamageWorker_Scratch**: 할퀴기 데미지 (데미지 분할)
- **DamageWorker_Bite**: 물기 데미지
- **DamageWorker_Flame**: 화염 데미지 (점화)
- **DamageWorker_Frostbite**: 동상 데미지
- **DamageWorker_Extinguish**: 소화 데미지
- **DamageWorker_Vaporize**: 증발 데미지
- **DamageWorker_Stun**: 기절 데미지
- **DamageWorker_Nerve**: 신경 충격 데미지
- **DamageWorker_MiningBomb**: 채굴 폭탄 데미지

---

## 부모-자식 관계도

### Abstract 부모 클래스

1. **CutBase** (Abstract)
   - Cut
   - SurgicalCut
   - ExecutionCut

2. **BluntBase** (Abstract)
   - Blunt
   - Poke
   - Demolish

3. **Scratch** (Abstract)
   - Scratch
   - ScratchToxic
   - PorcupineScratch

4. **Bite** (Abstract)
   - Bite
   - ToxicBite
   - PorcupineBite

5. **Bullet** (Abstract)
   - Bullet
   - BulletToxic
   - Bullet_TraitTox
   - Bullet_TraitIncendiary

6. **Arrow** (Abstract)
   - Arrow
   - ArrowHighVelocity
   - Nerve

7. **Flame** (Abstract)
   - Flame
   - Burn
   - AcidBurn
   - ElectricalBurn

8. **Bomb** (Abstract)
   - Bomb
   - BombSuper
   - MiningBomb

9. **Vaporize** (Abstract)
   - Vaporize
   - NociosphereVaporize

10. **Beam** (Abstract)
    - Beam
    - BeamBypassShields

11. **StunBase** (Abstract)
    - Stun
    - EMP
    - MechBandShockwave
    - RK_EMP

---

## 참고 파일 위치

### RimWorld Core
- `RimworldData/Core/Defs/DamageDefs/Damages_MeleeWeapon.xml`
- `RimworldData/Core/Defs/DamageDefs/Damages_RangedWeapon.xml`
- `RimworldData/Core/Defs/DamageDefs/Damages_Environmental.xml`
- `RimworldData/Core/Defs/DamageDefs/Damages_Misc.xml`
- `RimworldData/Core/Defs/DamageDefs/Damages_Medical.xml`
- `RimworldData/Core/Defs/DamageDefs/Damages_Stun.xml`

### Biotech DLC
- `RimworldData/Biotech/Defs/DamageDefs/Damages_Misc.xml`
- `RimworldData/Biotech/Defs/DamageDefs/Damages_RangedWeapon.xml`
- `RimworldData/Biotech/Defs/DamageDefs/Damages_Stun.xml`

### Anomaly DLC
- `RimworldData/Anomaly/Defs/DamageDefs/Damages_Environmental.xml`
- `RimworldData/Anomaly/Defs/DamageDefs/Damages_Misc.xml`

### Odyssey DLC
- `RimworldData/Odyssey/Defs/DamageDefs/Damages_MeleeWeapon.xml`
- `RimworldData/Odyssey/Defs/DamageDefs/Damages_Misc.xml`
- `RimworldData/Odyssey/Defs/DamageDefs/Damages_RangedWeapon.xml`

### Ratkin 프로젝트
- `Project/1.6/Defs/DamageDefs/Damage_Def.xml`

---

## 결론

이 보고서는 RimWorld의 모든 DamageDef 유형을 완전히 정리한 것입니다. 총 **47개의 DamageDef**가 있으며, 각각 고유한 특성과 용도를 가지고 있습니다. 

주요 특징:
- **근접 무기**: 12개 (베기, 찌르기, 둔기, 할퀴기, 물기 등)
- **원거리 무기**: 9개 (총알, 화살, 빔 등)
- **환경 데미지**: 6개 (화염, 동상, 전기 등)
- **폭발/충격**: 6개 (폭탄, 증발 등)
- **기절/EMP**: 4개
- **특수 데미지**: 10개 (의료, 독성, 사이킥 등)

각 DamageDef는 고유한 Hediff, WorkerClass, 속성들을 가지고 있어 게임 내 다양한 전투 및 환경 상황을 표현합니다.
