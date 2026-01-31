# 랫킨 제노타입 스탯 완전 분석

**태그**: Ratkin Xenotype Gene Stats Metabolism BiostatMet BiostatCpx

## 1. 랫킨 제노타입 유전자 목록

### 1.1 기본 유전자 (RimWorld 원본)

| 유전자 | 대사효율 | 복잡도 | 설명 |
|--------|---------|--------|------|
| `MoveSpeed_Quick` | **-3** | - | 이동속도 증가 |
| `Immunity_Strong` | **-1** | - | 면역력 증가 |
| `MinTemp_SmallDecrease` | **-1** | - | 최소 쾌적온도 감소 |

### 1.2 커스텀 랫킨 유전자

| 유전자 | 대사효율 | 복잡도 | 설명 |
|--------|---------|--------|------|
| `RK_Gene_ToxicEnvironmentResistance_Mild` | **-1** | 0 | 독성 환경 저항 |
| `RK_Gene_SmallBody` | **0** | 0 | 작은 체형 |
| `RK_Gene_Nimble` | **-1** | 1 | 재빠름 |
| `RK_Gene_PoorEyesight` | **+3** | 1 | 근시 |
| `RK_Gene_FragilePain` | **+2** | 1 | 약한 통증 역치 |
| `RK_Gene_EfficientMiner` | **0** | 1 | 효율적인 채굴 |
| `RK_Gene_GreenThumb` | **0** | 1 | 식물 친화력 |
| `RK_Gene_ClumsyBuilder` | **+1** | 1 | 서툰 건축가 |
| `RK_Gene_SlowResearcher` | **+1** | 1 | 느린 학습자 |
| `RK_Gene_LargeEars` | **0** | 0 | 큰 귀 (외형) |
| `RK_Gene_ThinTail` | **0** | 0 | 얇은 꼬리 (외형) |
| `RK_Gene_CuteFace` | **0** | 1 | 귀여운 얼굴 |

## 2. 대사효율 (Metabolism) 계산

### 2.1 대사효율 합계

**음수 유전자 (대사효율 감소)**:
- `MoveSpeed_Quick`: -3
- `Immunity_Strong`: -1
- `RK_Gene_ToxicEnvironmentResistance_Mild`: -1
- `MinTemp_SmallDecrease`: -1
- `RK_Gene_Nimble`: -1
- **합계: -7**

**양수 유전자 (대사효율 증가)**:
- `RK_Gene_PoorEyesight`: +3
- `RK_Gene_FragilePain`: +2
- `RK_Gene_ClumsyBuilder`: +1
- `RK_Gene_SlowResearcher`: +1
- **합계: +7**

**최종 대사효율**: **0** (균형)

### 2.2 복잡도 (Complexity) 합계

**복잡도 1인 유전자**:
- `RK_Gene_Nimble`: 1
- `RK_Gene_PoorEyesight`: 1
- `RK_Gene_FragilePain`: 1
- `RK_Gene_EfficientMiner`: 1
- `RK_Gene_GreenThumb`: 1
- `RK_Gene_ClumsyBuilder`: 1
- `RK_Gene_SlowResearcher`: 1
- `RK_Gene_CuteFace`: 1
- **합계: 8**

## 3. 스탯 변화 상세 분석

### 3.1 이동 및 전투 스탯

| 스탯 | 변화 | 유전자 | 비고 |
|------|------|--------|------|
| `MoveSpeed` | **×1.2** | MoveSpeed_Quick | 이동속도 20% 증가 |
| `MeleeDodgeChance` | **×1.25** | RK_Gene_Nimble | 근접 회피율 25% 증가 |
| `MeleeCooldownFactor` | **×0.9** | RK_Gene_Nimble | 근접 공격 쿨다운 10% 감소 |
| `MeleeDamageFactor` | **×0.9** | RK_Gene_SmallBody | 근접 공격력 10% 감소 |
| `AimingDelayFactor` | **×1.15** | RK_Gene_PoorEyesight | 조준 시간 15% 증가 |
| `ShootingAccuracyPawn` | **×0.9** | RK_Gene_PoorEyesight | 사격 정확도 10% 감소 |

### 3.2 생존 및 저항 스탯

| 스탯 | 변화 | 유전자 | 비고 |
|------|------|--------|------|
| `ImmunityGainSpeed` | **×1.3** | Immunity_Strong | 면역력 획득 속도 30% 증가 |
| `ToxicEnvironmentResistance` | **+0.35** | RK_Gene_ToxicEnvironmentResistance_Mild | 독성 환경 저항 35% 증가 |
| `PainShockThreshold` | **-0.1** | RK_Gene_FragilePain | 통증 쇼크 역치 0.1 감소 |
| `ComfyTemperatureMin` | **-10** | MinTemp_SmallDecrease | 최소 쾌적온도 10도 감소 |

### 3.3 작업 능력 스탯

| 스탯 | 변화 | 유전자 | 비고 |
|------|------|--------|------|
| `MiningSpeed` | **×0.9** | RK_Gene_EfficientMiner | 채굴 속도 10% 감소 |
| `MiningYield` | **×1.10** | RK_Gene_EfficientMiner | 채굴 수확량 10% 증가 |
| `PlantWorkSpeed` | **×0.9** | RK_Gene_GreenThumb | 식물 작업 속도 10% 감소 |
| `PlantHarvestYield` | **×1.1** | RK_Gene_GreenThumb | 식물 수확량 10% 증가 |
| `ConstructionSpeed` | **×0.8** | RK_Gene_ClumsyBuilder | 건축 속도 20% 감소 |
| `GlobalLearningFactor` | **×0.9** | RK_Gene_SlowResearcher | 전역 학습 속도 10% 감소 |
| `ResearchSpeed` | **×0.9** | RK_Gene_SlowResearcher | 연구 속도 10% 감소 |

### 3.4 은신 및 수확 스탯

| 스탯 | 변화 | 유전자 | 비고 |
|------|------|--------|------|
| `HuntingStealth` | **+1.15** | RK_Gene_SmallBody | 사냥 은신도 1.15 증가 |
| `MeatAmount` | **-50** | RK_Gene_SmallBody | 고기 수확량 50 감소 |
| `LeatherAmount` | **-30** | RK_Gene_SmallBody | 가죽 수확량 30 감소 |
| `CarryingCapacity` | **-15** | RK_Gene_FragilePain | 운반 용량 15 감소 |

### 3.5 사회적 스탯

| 스탯 | 변화 | 유전자 | 비고 |
|------|------|--------|------|
| `NegotiationAbility` | **×0.80** | RK_Gene_CuteFace | 협상 능력 20% 감소 |
| `TradePriceImprovement` | **×0.90** | RK_Gene_CuteFace | 거래 가격 개선 10% 감소 |
| `SocialImpact` | **×1.20** | RK_Gene_CuteFace | 사회적 영향력 20% 증가 |

## 4. 스탯 변화 요약

### 4.1 증가하는 스탯

**배율 증가 (×1.0 이상)**:
- 이동속도: ×1.2
- 근접 회피율: ×1.25
- 면역력 획득 속도: ×1.3
- 채굴 수확량: ×1.10
- 식물 수확량: ×1.1
- 사회적 영향력: ×1.20

**수치 증가 (+)**:
- 사냥 은신도: +1.15
- 독성 환경 저항: +0.35

### 4.2 감소하는 스탯

**배율 감소 (×1.0 미만)**:
- 근접 공격력: ×0.9
- 근접 공격 쿨다운: ×0.9
- 조준 시간: ×1.15 (증가, 불리)
- 사격 정확도: ×0.9
- 채굴 속도: ×0.9
- 식물 작업 속도: ×0.9
- 건축 속도: ×0.8
- 전역 학습 속도: ×0.9
- 연구 속도: ×0.9
- 협상 능력: ×0.80
- 거래 가격 개선: ×0.90

**수치 감소 (-)**:
- 고기 수확량: -50
- 가죽 수확량: -30
- 운반 용량: -15
- 통증 쇼크 역치: -0.1
- 최소 쾌적온도: -10

## 5. 종합 평가

### 5.1 강점

1. **이동 및 회피**: 빠른 이동속도와 높은 근접 회피율로 생존성 향상
2. **면역력**: 강한 면역력으로 질병에 대한 저항력 증가
3. **환경 적응**: 독성 환경 저항과 낮은 최소 온도로 다양한 환경에서 활동 가능
4. **수확 효율**: 채굴과 식물 수확량 증가로 자원 획득 효율 향상
5. **은신**: 작은 체형으로 사냥 시 은신도 증가

### 5.2 약점

1. **전투력**: 근접 공격력 감소, 사격 정확도 감소, 조준 시간 증가
2. **건축 및 연구**: 건축 속도와 연구 속도 감소
3. **운반 능력**: 운반 용량 감소
4. **통증 저항**: 낮은 통증 쇼크 역치로 쉽게 전투 불능 상태가 됨
5. **수확량**: 고기와 가죽 수확량 감소

### 5.3 특성

- **대사효율 균형**: 대사효율이 0으로 균형잡혀 있어 음식 소비에 특별한 영향 없음
- **복잡도**: 복잡도 8로 중간 수준의 유전자 복잡도
- **외형**: 큰 귀, 얇은 꼬리, 귀여운 얼굴로 랫킨 특유의 외형

## 6. 게임플레이 권장사항

### 6.1 적합한 역할

- **탐험가**: 빠른 이동속도와 높은 회피율로 위험 지역 탐험에 적합
- **채굴자**: 채굴 수확량 증가로 광물 채집에 유리
- **농부**: 식물 수확량 증가로 농업에 적합
- **사냥꾼**: 은신도 증가로 사냥에 유리

### 6.2 부적합한 역할

- **전투원**: 공격력 감소와 통증 저항 약화로 전투에 불리
- **건축가**: 건축 속도 감소로 건축 작업에 불리
- **연구원**: 연구 속도 감소로 연구에 불리
- **운반꾼**: 운반 용량 감소로 운반 작업에 불리

## 7. 참고 자료

- **유전자 정의**: `Project/Biotech/Defs/GeneDefs/CustomGeneDefs.xml`
- **제노타입 정의**: `Project/Biotech/Defs/GeneDefs/XenotypeDefs.xml`
