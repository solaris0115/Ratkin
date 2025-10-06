# Ratkin vs Human StatBase 비교 보고서

## 개요
- **작성일**: 2024년
- **목적**: Ratkin 종족과 Human 종족의 statBases 값 비교 분석
- **데이터 소스**: 
  - Human: `RimWorldData/Core/Defs/ThingDefs_Races/Races_Humanlike.xml`
  - Ratkin: `Project/1.6/Defs/ThingDef_Race/Races_Rakinlike.xml`

## StatBase 비교표

| Stat | Human | Ratkin | 차이 | 비고 |
|------|-------|--------|------|------|
| **기본 스탯** |
| MarketValue | 1750 | 1750 | 0 | 동일 |
| MoveSpeed | 4.6 | 4.8 | +0.2 | Ratkin이 더 빠름 |
| ComfyTemperatureMin | 16 | 11 | -5 | Ratkin이 더 추위에 강함 |
| ComfyTemperatureMax | 26 | 26 | 0 | 동일 |
| LeatherAmount | 75 | 30 | -45 | Ratkin이 가죽을 적게 제공 |
| **Ratkin 전용 스탯** |
| Mass | - | 50 | - | Human에는 없음 |
| Flammability | - | 1.0 | - | Human에는 없음 |
| ImmunityGainSpeed | - | 1.10 | - | Human에는 없음 |
| CarryingCapacity | - | 45 | - | Human에는 없음 |
| PainShockThreshold | - | 0.7 | - | Human에는 없음 |
| ToxicEnvironmentResistance | - | 0.9 | - | Human에는 없음 |
| EatingSpeed | - | 1.1 | - | Human에는 없음 |
| MeatAmount | - | 35 | - | Human에는 없음 |
| **전투 스탯** |
| MeleeDodgeChance | - | 1.15 | - | Human에는 없음 |
| AimingDelayFactor | - | 1.15 | - | Human에는 없음 |
| **사회 스탯** |
| NegotiationAbility | - | 0.85 | - | Human에는 없음 |
| **작업 스탯** |
| MiningSpeed | - | 1.1 | - | Human에는 없음 |
| MiningYield | - | 1.05 | - | Human에는 없음 |
| PlantWorkSpeed | - | 1.10 | - | Human에는 없음 |
| HuntingStealth | - | 1.15 | - | Human에는 없음 |
| **테크 스탯** |
| ConstructionSpeed | - | 0.9 | - | Human에는 없음 |
| ResearchSpeed | - | 0.8 | - | Human에는 없음 |

## 주요 차이점 분석

### 1. Human에만 있는 스탯
- **RoyalFavorValue**: 3 (로열티 관련)
- **Wildness**: 0.75 (야생성 관련)

### 2. Ratkin에만 있는 스탯 (총 15개)
- **기본 능력**: Mass, Flammability, ImmunityGainSpeed, CarryingCapacity, PainShockThreshold, ToxicEnvironmentResistance, EatingSpeed, MeatAmount
- **전투 능력**: MeleeDodgeChance, AimingDelayFactor
- **사회 능력**: NegotiationAbility
- **작업 능력**: MiningSpeed, MiningYield, PlantWorkSpeed, HuntingStealth
- **테크 능력**: ConstructionSpeed, ResearchSpeed

### 3. 공통 스탯의 차이
- **MoveSpeed**: Ratkin이 0.2 더 빠름 (4.6 → 4.8)
- **ComfyTemperatureMin**: Ratkin이 5도 더 추위에 강함 (16 → 11)
- **LeatherAmount**: Ratkin이 45 적음 (75 → 30)

## Ratkin의 특징적 능력

### 장점
1. **이동성**: 더 빠른 이동속도
2. **환경 적응**: 더 넓은 온도 범위 (11-26도)
3. **전투**: 근접 회피율 15% 증가
4. **작업 효율**: 채굴, 식물 작업, 사냥 스텔스 향상
5. **생존력**: 면역력 증가, 독성 환경 저항, 통증 임계값

### 단점
1. **자원**: 가죽 제공량 60% 감소 (75 → 30)
2. **사회**: 협상 능력 15% 감소
3. **기술**: 건설 속도 10% 감소, 연구 속도 20% 감소
4. **사격**: 조준 지연 15% 증가

## 결론

Ratkin은 Human에 비해 **전문화된 종족**으로 설계되었습니다:
- **생존과 전투**에 특화된 능력들
- **채굴과 사냥** 등 특정 작업에 우수
- **기술 발전**에는 상대적으로 불리
- **사회적 상호작용**에서 약간의 페널티

이는 Ratkin을 **야생적이고 실용적인 종족**으로 포지셔닝하는 설계 의도로 보입니다.
