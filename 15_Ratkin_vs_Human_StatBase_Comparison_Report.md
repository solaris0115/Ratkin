# Ratkin vs Human StatBase 비교 보고서

## 개요
- **작성일**: 2024년
- **목적**: Ratkin 종족과 Human 종족의 statBases 값 비교 분석 (BasePawn 상속 고려)
- **데이터 소스**: 
  - Human: `RimWorldData/Core/Defs/ThingDefs_Races/Races_Humanlike.xml`
  - Ratkin: `Project/1.6/Defs/ThingDef_Race/Races_Rakinlike.xml`
  - BasePawn 기본값: `RimWorldData/Core/Defs/Stats/` 파일들

## StatBase 비교표 (BasePawn 상속 포함)

| Stat | Human (명시적) | Human (실제값) | Ratkin | 차이 | 비고 |
|------|----------------|----------------|--------|------|------|
| **기본 스탯** |
| MarketValue | 1750 | 1750 | 1750 | 0 | 동일 |
| MoveSpeed | 4.6 | 4.6 | 4.8 | +0.2 | Ratkin이 더 빠름 |
| ComfyTemperatureMin | 16 | 16 | 11 | -5 | Ratkin이 더 추위에 강함 |
| ComfyTemperatureMax | 26 | 26 | 26 | 0 | 동일 |
| LeatherAmount | 75 | 75 | 30 | -45 | Ratkin이 가죽을 적게 제공 |
| Mass | - | 1 (기본값) | 50 | +49 | Ratkin이 훨씬 무거움 |
| Flammability | - | 0 (기본값) | 1.0 | +1.0 | Ratkin이 더 잘 타는 편 |
| **생존 스탯** |
| ImmunityGainSpeed | - | 1 (기본값) | 1.10 | +0.10 | Ratkin이 면역력 증가 |
| CarryingCapacity | - | 75 (기본값) | 45 | -30 | Ratkin이 운반량 적음 |
| PainShockThreshold | - | 0.8 (기본값) | 0.7 | -0.1 | Ratkin이 통증에 더 민감 |
| ToxicEnvironmentResistance | - | 0 (기본값) | 0.9 | +0.9 | Ratkin이 독성 환경 저항 |
| EatingSpeed | - | 1 (기본값) | 1.1 | +0.1 | Ratkin이 먹는 속도 빠름 |
| MeatAmount | - | 140 (기본값) | 35 | -105 | Ratkin이 고기 제공량 적음 |
| **전투 스탯** |
| MeleeDodgeChance | - | 0 (기본값) | 1.15 | +1.15 | Ratkin이 근접 회피율 증가 |
| AimingDelayFactor | - | 1 (기본값) | 1.15 | +0.15 | Ratkin이 조준 시간 증가 |
| **사회 스탯** |
| NegotiationAbility | - | 1 (기본값) | 0.85 | -0.15 | Ratkin이 협상 능력 감소 |
| **작업 스탯** |
| MiningSpeed | - | 1 (기본값) | 1.1 | +0.1 | Ratkin이 채굴 속도 증가 |
| MiningYield | - | 1 (기본값) | 1.05 | +0.05 | Ratkin이 채굴 수율 증가 |
| PlantWorkSpeed | - | 1 (기본값) | 1.10 | +0.10 | Ratkin이 식물 작업 속도 증가 |
| HuntingStealth | - | 1 (기본값) | 1.15 | +0.15 | Ratkin이 사냥 스텔스 증가 |
| ConstructionSpeed | - | 1 (기본값) | 0.9 | -0.1 | Ratkin이 건설 속도 감소 |
| ResearchSpeed | - | 1 (기본값) | 0.8 | -0.2 | Ratkin이 연구 속도 감소 |
| **Human 전용 스탯** |
| RoyalFavorValue | 3 | 3 | - | - | Human에만 있음 |
| Wildness | 0.75 | 0.75 | - | - | Human에만 있음 |

## 주요 차이점 분석

### 1. Human의 BasePawn 상속 스탯들
Human은 BasePawn에서 다음 기본값들을 상속받습니다:
- **MoveSpeed**: 3.0 (기본값) → 4.6 (Human 오버라이드)
- **ComfyTemperatureMin**: 0 (기본값) → 16 (Human 오버라이드)
- **ComfyTemperatureMax**: 40 (기본값) → 26 (Human 오버라이드)
- **LeatherAmount**: 0 (기본값) → 75 (Human 오버라이드)
- **기타 모든 스탯**: 기본값 사용

### 2. Ratkin의 특화된 스탯들
Ratkin은 Human과 달리 많은 스탯을 명시적으로 정의하여 특화되었습니다:
- **생존 특화**: 면역력, 독성 저항, 통증 임계값 조정
- **전투 특화**: 근접 회피율 증가, 조준 시간 증가
- **작업 특화**: 채굴, 식물 작업, 사냥 능력 향상
- **기술 페널티**: 건설, 연구 속도 감소

### 3. 공통 스탯의 차이
- **MoveSpeed**: Ratkin이 0.2 더 빠름 (4.6 → 4.8)
- **ComfyTemperatureMin**: Ratkin이 5도 더 추위에 강함 (16 → 11)
- **LeatherAmount**: Ratkin이 45 적음 (75 → 30)
- **Mass**: Ratkin이 49 더 무거움 (1 → 50)

## Ratkin의 특징적 능력

### 장점
1. **이동성**: 더 빠른 이동속도
2. **환경 적응**: 더 넓은 온도 범위 (11-26도)
3. **전투**: 근접 회피율 15% 증가
4. **작업 효율**: 채굴, 식물 작업, 사냥 스텔스 향상
5. **생존력**: 면역력 증가, 독성 환경 저항
6. **식사**: 먹는 속도 10% 증가

### 단점
1. **자원**: 가죽 제공량 60% 감소, 고기 제공량 75% 감소
2. **사회**: 협상 능력 15% 감소
3. **기술**: 건설 속도 10% 감소, 연구 속도 20% 감소
4. **사격**: 조준 지연 15% 증가
5. **통증**: 통증 임계값 10% 감소
6. **운반**: 운반량 40% 감소
7. **가연성**: 더 잘 타는 편

## 결론

Ratkin은 Human에 비해 **전문화된 종족**으로 설계되었습니다:
- **생존과 전투**에 특화된 능력들
- **채굴과 사냥** 등 특정 작업에 우수
- **기술 발전**에는 상대적으로 불리
- **사회적 상호작용**에서 약간의 페널티
- **자원 생산**에서는 불리

이는 Ratkin을 **야생적이고 실용적인 종족**으로 포지셔닝하는 설계 의도로 보입니다. Human이 모든 면에서 균형잡힌 종족이라면, Ratkin은 특정 분야에 특화된 대신 다른 분야에서 페널티를 받는 종족입니다.
