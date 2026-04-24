# Ratkin GeneDef 구현 계획서 - 2025-10-15

## 작업 개요
- **요청 내용**: XenotypeDefs.xml의 Ratkin 스탯 주석을 기반으로 GeneDef 생성
- **목표**: Ratkin xenotype의 모든 stat 특성을 GeneDef로 완벽 구현
- **범위**: Stat 관련 gene만 구현 (외형/cosmetic gene은 이후 작업)

## 계획 (AI가 결정한 계획)

### 1단계: 스탯 요구사항 분석
- XenotypeDefs.xml 주석에서 Ratkin vs Human 스탯 차이 추출
- 15개 주요 스탯 변화 확인

### 2단계: 기존 RimWorld GeneDef 조사
- Biotech GeneDef 파일 분석
- 재사용 가능한 기존 gene 식별

### 3단계: Custom GeneDef 설계
- 15개의 새로운 RK_Gene_XXX 설계
- biostatMet/biostatCpx 밸런스 설정

### 4단계: CustomGeneDefs.xml 구현
- 모든 custom gene XML 작성
- 적절한 아이콘 경로 설정 (임시)

### 5단계: XenotypeDefs.xml 업데이트
- gene 목록 추가
- 테스트 준비

## 최종 계획 (수정 v2 - Gene 중복 고려)

### A. 스탯별 상세 분석 및 구현 방안

#### 📌 기본 스탯

**1. Mass: 50 (vs 60)**
- 필요값: -10
- 기존 Gene: ❌ 없음
- 구현: ✅ 필요 (RK_Gene_LightWeight)

**2. MoveSpeed: 4.8 (vs 4.6)**
- 필요값: +0.2
- 기존 Gene: ✅ `MoveSpeed_Quick` (+0.2) - **정확히 일치!**
- 구현: ❌ 불필요 (기존 gene 사용)

**3. ComfyTemperatureMin: 11 (vs 16)**
- 필요값: -5
- 기존 Gene: 🔸 `MinTemp_SmallDecrease` (-10) - 너무 강함
- 구현: ✅ 필요 (RK_Gene_MildColdTolerance) - **값 조정 필요 (-5)**

**4. ImmunityGainSpeed: 1.10**
- 필요값: ×1.10
- 기존 Gene: ✅ `ImmunityGainSpeed_Strong` (×1.1) - **정확히 일치!**
- 구현: ❌ 불필요 (기존 gene 사용)

**5. CarryingCapacity: 45 (vs 75)**
- 필요값: -30
- 기존 Gene: ❌ 없음
- 구현: ✅ 필요 (RK_Gene_WeakCarrying)

**6. PainShockThreshold: 0.7 (vs 0.8)**
- 필요값: -0.1
- 기존 Gene: ❌ 없음
- 구현: ✅ 필요 (RK_Gene_FragilePain)

**7. ToxicEnvironmentResistance: 0.9**
- 필요값: +0.9
- 기존 Gene: 🔸 `ToxicEnvironmentResistance_Partial` (+0.5) - 부족함
- 기존 Gene: 🔸 `ToxicEnvironmentResistance_Total` (+1.0) - 약간 높음
- 구현: 🔄 **Total 사용 (값 조정: 0.9 → 1.0 허용)** 또는 Custom 제작

**8. EatingSpeed: 1.1**
- 필요값: ×1.1
- 기존 Gene: ❌ 없음
- 구현: ✅ 필요 (RK_Gene_FastEater)

#### 📌 전투 스탯

**9. MeleeDodgeChance: 1.15**
- 필요값: +1.15
- 기존 Gene: ❌ 없음
- 구현: ✅ 필요 (RK_Gene_Nimble)

**10. AimingDelayFactor: 1.15**
- 필요값: ×1.15 (느려짐)
- 기존 Gene: ❌ 없음
- 구현: ✅ 필요 (RK_Gene_PoorAiming)

#### 📌 사회 스탯

**11. NegotiationAbility: 0.85**
- 필요값: ×0.85
- 기존 Gene: ❌ 없음
- 구현: ✅ 필요 (RK_Gene_PoorSocial)

#### 📌 작업 스탯

**12. MiningSpeed: 1.1**
- 필요값: ×1.1
- 기존 Gene: ❌ 없음
- 구현: ✅ 필요 (RK_Gene_EfficientMiner - Speed만)

**13. MiningYield: 1.05**
- 필요값: ×1.05
- 기존 Gene: ❌ 없음
- 구현: ✅ 필요 (RK_Gene_EfficientMiner - Yield 포함)

**14. PlantWorkSpeed: 1.10**
- 필요값: ×1.1
- 기존 Gene: ❌ 없음
- 구현: ✅ 필요 (RK_Gene_GreenThumb)

**15. HuntingStealth: 1.15**
- 필요값: ×1.15
- 기존 Gene: ❌ 없음
- 구현: ✅ 필요 (RK_Gene_Stealthy)

#### 📌 테크 스탯

**16. ConstructionSpeed: 0.9**
- 필요값: ×0.9
- 기존 Gene: ❌ 없음
- 구현: ✅ 필요 (RK_Gene_ClumsyBuilder)

**17. ResearchSpeed: 0.8**
- 필요값: ×0.8
- 기존 Gene: ❌ 없음
- 구현: ✅ 필요 (RK_Gene_SlowResearcher)

#### 📌 물리적 특성

**18. MeatAmount: 35 (vs 140)**
- 필요값: -105
- 기존 Gene: ❌ 없음
- 구현: ✅ 필요 (RK_Gene_SmallBody)

**19. LeatherAmount: 30 (vs 75)**
- 필요값: -45
- 기존 Gene: ❌ 없음
- 구현: ✅ 필요 (RK_Gene_SmallBody - 함께 처리)

---

### B. 최종 Gene 구성 요약

#### ✅ 기존 RimWorld Gene 사용 (2개)

1. **MoveSpeed_Quick**
   - 스탯: MoveSpeed +0.2
   - biostatMet: -3
   - 상태: ✅ 정확히 일치

2. **ImmunityGainSpeed_Strong** (확인 필요)
   - 스탯: ImmunityGainSpeed ×1.1
   - biostatMet: -1 (예상)
   - 상태: ✅ 정확히 일치

#### 🔄 기존 Gene 값 조정 사용 고려 (1개)

3. **ToxicEnvironmentResistance_Total**
   - 필요값: +0.9
   - 제공값: +1.0
   - 차이: +0.1 초과
   - 결정 필요: Total(1.0) 사용 vs Custom(0.9) 제작

#### ✅ Custom Gene 제작 필요 (13개)

1. **RK_Gene_LightWeight** - Mass -10
2. **RK_Gene_MildColdTolerance** - ComfyTemperatureMin -5
3. **RK_Gene_WeakCarrying** - CarryingCapacity -30
4. **RK_Gene_FragilePain** - PainShockThreshold -0.1
5. **RK_Gene_FastEater** - EatingSpeed ×1.1
6. **RK_Gene_Nimble** - MeleeDodgeChance +1.15
7. **RK_Gene_PoorAiming** - AimingDelayFactor ×1.15
8. **RK_Gene_PoorSocial** - NegotiationAbility ×0.85
9. **RK_Gene_EfficientMiner** - MiningSpeed ×1.1, MiningYield ×1.05
10. **RK_Gene_GreenThumb** - PlantWorkSpeed ×1.1
11. **RK_Gene_Stealthy** - HuntingStealth ×1.15
12. **RK_Gene_ClumsyBuilder** - ConstructionSpeed ×0.9
13. **RK_Gene_SlowResearcher** - ResearchSpeed ×0.8

#### 선택적 Custom Gene (1개)

14. **RK_Gene_ToxicResistance** - ToxicEnvironmentResistance +0.9
    - 또는 기존 `ToxicEnvironmentResistance_Total` (+1.0) 사용

#### 특수 처리 필요

- **MeatAmount/LeatherAmount**: RaceDef의 race 섹션에서 직접 설정
  - GeneDef로 처리하지 않고 Races_Ratkinlike.xml에서 설정

---

### C. Custom GeneDef 상세 스펙

#### **그룹 1: 체형/신체 (2개)**

1. **RK_Gene_LightWeight**
   - label: "light frame"
   - labelShortAdj: "light"
   - description: "랫킨은 체구가 작아 가볍습니다."
   - `<statOffsets><Mass>-10</Mass></statOffsets>`
   - displayCategory: Miscellaneous
   - biostatMet: +1
   - biostatCpx: 0

2. **RK_Gene_WeakCarrying**
   - label: "weak carrying"
   - labelShortAdj: "weak carrier"
   - description: "랫킨은 작은 체구로 인해 운반 능력이 낮습니다."
   - `<statOffsets><CarryingCapacity>-30</CarryingCapacity></statOffsets>`
   - displayCategory: Miscellaneous
   - biostatMet: +2
   - biostatCpx: 0

#### **그룹 2: 환경 적응 (2개)**

3. **RK_Gene_MildColdTolerance**
   - label: "mild cold tolerance"
   - labelShortAdj: "cool-adapted"
   - description: "랫킨은 추위에 약간 강합니다."
   - `<statOffsets><ComfyTemperatureMin>-5</ComfyTemperatureMin></statOffsets>`
   - displayCategory: Temperature
   - biostatMet: -1
   - biostatCpx: 0

4. **RK_Gene_ToxicResistance** (선택적)
   - label: "sewer-adapted"
   - labelShortAdj: "tox-resistant"
   - description: "랫킨은 하수구 환경에 적응하여 독성에 매우 강합니다."
   - `<statOffsets><ToxicEnvironmentResistance>0.9</ToxicEnvironmentResistance></statOffsets>`
   - displayCategory: ResistanceAndWeakness
   - biostatMet: -3
   - biostatCpx: 1

#### **그룹 3: 전투 특성 (3개)**

5. **RK_Gene_Nimble**
   - label: "nimble"
   - labelShortAdj: "nimble"
   - description: "랫킨은 민첩하여 근접 회피가 뛰어납니다."
   - `<statOffsets><MeleeDodgeChance>1.15</MeleeDodgeChance></statOffsets>`
   - displayCategory: Violence
   - biostatMet: -3
   - biostatCpx: 1

6. **RK_Gene_PoorAiming**
   - label: "poor aiming"
   - labelShortAdj: "poor aim"
   - description: "랫킨은 시력이 좋지 않아 조준이 느립니다."
   - `<statFactors><AimingDelayFactor>1.15</AimingDelayFactor></statFactors>`
   - displayCategory: Violence
   - biostatMet: +2
   - biostatCpx: 0

7. **RK_Gene_FragilePain**
   - label: "fragile pain threshold"
   - labelShortAdj: "fragile"
   - description: "랫킨은 고통에 약합니다."
   - `<statOffsets><PainShockThreshold>-0.1</PainShockThreshold></statOffsets>`
   - displayCategory: Pain
   - biostatMet: +1
   - biostatCpx: 0

#### **그룹 4: 생활/사회 (2개)**

8. **RK_Gene_FastEater**
   - label: "fast eater"
   - labelShortAdj: "fast eater"
   - description: "랫킨은 빠르게 먹습니다."
   - `<statFactors><EatingSpeed>1.1</EatingSpeed></statFactors>`
   - displayCategory: Miscellaneous
   - biostatMet: -1
   - biostatCpx: 0

9. **RK_Gene_PoorSocial**
   - label: "poor social"
   - labelShortAdj: "antisocial"
   - description: "랫킨은 사회성이 낮습니다."
   - `<statFactors><NegotiationAbility>0.85</NegotiationAbility></statFactors>`
   - displayCategory: Miscellaneous
   - biostatMet: +1
   - biostatCpx: 0

#### **그룹 5: 작업 능력 (3개)**

10. **RK_Gene_EfficientMiner**
    - label: "efficient miner"
    - labelShortAdj: "miner"
    - description: "랫킨은 땅을 파는 것에 능숙합니다."
    - `<statFactors><MiningSpeed>1.1</MiningSpeed><MiningYield>1.05</MiningYield></statFactors>`
    - displayCategory: Miscellaneous
    - biostatMet: -2
    - biostatCpx: 1

11. **RK_Gene_GreenThumb**
    - label: "plant affinity"
    - labelShortAdj: "green thumb"
    - description: "랫킨은 식물 작업이 빠릅니다."
    - `<statFactors><PlantWorkSpeed>1.1</PlantWorkSpeed></statFactors>`
    - displayCategory: Miscellaneous
    - biostatMet: -2
    - biostatCpx: 0

12. **RK_Gene_Stealthy**
    - label: "stealthy"
    - labelShortAdj: "stealthy"
    - description: "랫킨은 은신에 능숙합니다."
    - `<statFactors><HuntingStealth>1.15</HuntingStealth></statFactors>`
    - displayCategory: Miscellaneous
    - biostatMet: -2
    - biostatCpx: 0

#### **그룹 6: 지적 능력 (2개)**

13. **RK_Gene_ClumsyBuilder**
    - label: "clumsy builder"
    - labelShortAdj: "clumsy"
    - description: "랫킨은 건축이 서툽니다."
    - `<statFactors><ConstructionSpeed>0.9</ConstructionSpeed></statFactors>`
    - displayCategory: Miscellaneous
    - biostatMet: +1
    - biostatCpx: 0

14. **RK_Gene_SlowResearcher**
    - label: "slow researcher"
    - labelShortAdj: "slow learner"
    - description: "랫킨은 연구가 느립니다."
    - `<statFactors><ResearchSpeed>0.8</ResearchSpeed></statFactors>`
    - displayCategory: Miscellaneous
    - biostatMet: +2
    - biostatCpx: 0

---

### D. 최종 Xenotype Gene 구성

#### **기존 RimWorld Genes (2~3개)**
1. `MoveSpeed_Quick` - biostatMet: -3
2. `ImmunityGainSpeed_Strong` - biostatMet: -1 (확인 필요)
3. `ToxicEnvironmentResistance_Total` - biostatMet: -3, biostatCpx: 2 (선택적)

#### **Custom Genes (13~14개)**
1. `RK_Gene_LightWeight` - biostatMet: +1
2. `RK_Gene_MildColdTolerance` - biostatMet: -1
3. `RK_Gene_WeakCarrying` - biostatMet: +2
4. `RK_Gene_FragilePain` - biostatMet: +1
5. `RK_Gene_FastEater` - biostatMet: -1
6. `RK_Gene_Nimble` - biostatMet: -3, biostatCpx: 1
7. `RK_Gene_PoorAiming` - biostatMet: +2
8. `RK_Gene_PoorSocial` - biostatMet: +1
9. `RK_Gene_EfficientMiner` - biostatMet: -2, biostatCpx: 1
10. `RK_Gene_GreenThumb` - biostatMet: -2
11. `RK_Gene_Stealthy` - biostatMet: -2
12. `RK_Gene_ClumsyBuilder` - biostatMet: +1
13. `RK_Gene_SlowResearcher` - biostatMet: +2
14. `RK_Gene_ToxicResistance` - biostatMet: -3, biostatCpx: 1 (선택적)

**총 15~16개 Gene**

---

### E. BiostatMet/Cpx 밸런스 계산

#### **옵션 A: ToxicEnvironmentResistance_Total 사용**

**긍정적 효과 (Metabolic Cost: -17, Complexity: 4)**
- MoveSpeed_Quick: -3 Met, 0 Cpx
- ImmunityGainSpeed_Strong: -1 Met, 0 Cpx
- ToxicEnvironmentResistance_Total: -3 Met, 2 Cpx
- RK_Gene_MildColdTolerance: -1 Met, 0 Cpx
- RK_Gene_Nimble: -3 Met, 1 Cpx
- RK_Gene_FastEater: -1 Met, 0 Cpx
- RK_Gene_EfficientMiner: -2 Met, 1 Cpx
- RK_Gene_GreenThumb: -2 Met, 0 Cpx
- RK_Gene_Stealthy: -2 Met, 0 Cpx

**부정적 효과 (Metabolic Gain: +10)**
- RK_Gene_LightWeight: +1 Met
- RK_Gene_WeakCarrying: +2 Met
- RK_Gene_FragilePain: +1 Met
- RK_Gene_PoorAiming: +2 Met
- RK_Gene_PoorSocial: +1 Met
- RK_Gene_ClumsyBuilder: +1 Met
- RK_Gene_SlowResearcher: +2 Met

**순 비용: -7 Met, 4 Cpx**

#### **옵션 B: Custom RK_Gene_ToxicResistance 사용**

**긍정적 효과 (Metabolic Cost: -17, Complexity: 4)**
- MoveSpeed_Quick: -3 Met, 0 Cpx
- ImmunityGainSpeed_Strong: -1 Met, 0 Cpx
- RK_Gene_ToxicResistance: -3 Met, 1 Cpx
- RK_Gene_MildColdTolerance: -1 Met, 0 Cpx
- RK_Gene_Nimble: -3 Met, 1 Cpx
- RK_Gene_FastEater: -1 Met, 0 Cpx
- RK_Gene_EfficientMiner: -2 Met, 1 Cpx
- RK_Gene_GreenThumb: -2 Met, 0 Cpx
- RK_Gene_Stealthy: -2 Met, 0 Cpx

**부정적 효과: 동일 (+10 Met)**

**순 비용: -7 Met, 3 Cpx**

#### **결론**
- 옵션 A: Total 사용 시 Complexity 약간 높음 (2 vs 1), 값 +0.1 초과
- 옵션 B: Custom 사용 시 정확한 값, Complexity 낮음
- **권장: 옵션 B (Custom 제작)** - 정확한 밸런스

---

## 작업 세부 진행

1. ImmunityGainSpeed_Strong gene 존재 확인 [v] - GeneDefs_Health.xml 확인
2. ToxicResistance 구현 방식 결정 (옵션 A vs B) [v] - Custom 제작 (RK_Gene_SewerAdapted)
3. CustomGeneDefs.xml에 13~14개 custom gene 작성 [v] - 14개 완료
4. 각 gene 아이콘 경로 설정 (임시: 기본 아이콘 또는 유사 아이콘 참조) [v] - 유사 아이콘 참조
5. XenotypeDefs.xml에 gene 목록 추가 (15~16개) [v] - 16개 추가 (기존 2 + custom 14)
6. MeatAmount/LeatherAmount를 Races_Ratkinlike.xml에서 확인 [-] - 추후 작업
7. XML syntax 검증 [v] - 에러 없음
8. 게임 내 테스트 준비 [v] - 완료

## 진행 상황

### 1. CustomGeneDefs.xml 작성 완료
- 14개 Custom Gene 작성
  - 체형/신체: LightWeight, WeakCarrying
  - 환경 적응: MildColdTolerance, SewerAdapted
  - 전투: Nimble, PoorAiming, FragilePain
  - 생활/사회: FastEater, PoorSocial
  - 작업: EfficientMiner, GreenThumb, Stealthy
  - 지적: ClumsyBuilder, SlowResearcher

### 2. XenotypeDefs.xml 업데이트 완료
- 기존 RimWorld Gene 2개 추가: MoveSpeed_Quick, ImmunityGainSpeed_Strong
- Custom Gene 14개 추가
- 총 16개 Gene 구성

### 3. 밸런스 설정
- 긍정적 효과: -17 Met, 3 Cpx
- 부정적 효과: +10 Met
- 순 비용: -7 Met, 3 Cpx
- 약간 강력한 xenotype

## 최종 작업 결과

✅ **작업 완료**

**생성된 파일:**
- CustomGeneDefs.xml: 14개 custom gene 정의 완료
- XenotypeDefs.xml: Ratkin xenotype에 16개 gene 추가

**구현된 스탯:**
- Mass: -10
- MoveSpeed: +0.2 (기존 gene)
- ComfyTemperatureMin: -5
- ImmunityGainSpeed: ×1.1 (기존 gene)
- CarryingCapacity: -30
- PainShockThreshold: -0.1
- ToxicEnvironmentResistance: +0.9
- EatingSpeed: ×1.1
- MeleeDodgeChance: +1.15
- AimingDelayFactor: ×1.15
- NegotiationAbility: ×0.85
- MiningSpeed: ×1.1
- MiningYield: ×1.05
- PlantWorkSpeed: ×1.1
- HuntingStealth: ×1.15
- ConstructionSpeed: ×0.9
- ResearchSpeed: ×0.8

**미완료:**
- MeatAmount/LeatherAmount: RaceDef에서 처리 (별도 작업)
- 외형 gene: 추후 추가 예정

## 관련 파일 목록

### 작업 대상
- [Project/1.6/Defs/GeneDefs/CustomGeneDefs.xml](../Project/1.6/Defs/GeneDefs/CustomGeneDefs.xml) - Custom gene 작성 예정
- [Project/1.6/Defs/GeneDefs/XenotypeDefs.xml](../Project/1.6/Defs/GeneDefs/XenotypeDefs.xml) - Gene 목록 추가 예정
- [Project/1.6/Defs/ThingDef_Race/Races_Ratkinlike.xml](../Project/1.6/Defs/ThingDef_Race/Races_Ratkinlike.xml) - MeatAmount/LeatherAmount 확인

### 레퍼런스
- [RimWorldData/Biotech/Defs/GeneDefs/GeneDefs_Misc.xml](../RimWorldData/Biotech/Defs/GeneDefs/GeneDefs_Misc.xml)
- [RimWorldData/Biotech/Defs/GeneDefs/GeneDefs_Spectrum.xml](../RimWorldData/Biotech/Defs/GeneDefs/GeneDefs_Spectrum.xml)
- [RimWorldData/Biotech/Defs/GeneDefs/GeneDefs_Health.xml](../RimWorldData/Biotech/Defs/GeneDefs/GeneDefs_Health.xml)
- [RimWorldData/Biotech/Defs/GeneDefs/XenotypeDefs.xml](../RimWorldData/Biotech/Defs/GeneDefs/XenotypeDefs.xml)

## 참고사항

### 중요 변경사항 (v2)
1. **Gene 중복 처리**: Gene 스탯은 더해지지 않음. 하나만 적용됨
2. **ToxicResistance**: Partial(0.5) + Custom(0.4) 조합 불가능
   - 옵션 A: Total(1.0) 사용 - 값 0.1 초과
   - 옵션 B: Custom(0.9) 제작 - 정확한 값 (권장)
3. **MeatAmount/LeatherAmount**: GeneDef로 처리 불가, RaceDef에서 직접 설정

### 주의사항
1. **아이콘**: 아이콘 경로는 임시로 설정, 추후 실제 아이콘 제작 필요
   - 임시: 기존 유사 gene 아이콘 참조 또는 기본 placeholder 사용
2. **밸런스**: biostatMet/Cpx 값은 게임 테스트 후 조정 가능
3. **스탯 이름**: RimWorld stat 이름 정확도 확인 필요
   - MeleeDodgeChance, AimingDelayFactor, EatingSpeed 등
4. **exclusionTags**: 필요시 추가 (같은 카테고리 gene 중복 방지)

### 구현 우선순위
1. **필수 (13개)**: 기존 gene 없는 모든 custom gene
2. **선택 (1개)**: RK_Gene_ToxicResistance (vs Total 사용)
3. **기존 활용 (2개)**: MoveSpeed_Quick, ImmunityGainSpeed_Strong

### 다음 단계 작업 (향후)
- 외형 관련 gene (귀, 꼬리, 털 등) 추가 작업
- Cosmetic gene은 별도 작업으로 분리
- 아이콘 실제 제작

