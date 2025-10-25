# StuffCategories 분석 보고서

## StuffCategory 정의

림월드에서 StuffCategories는 제작 가능한 아이템들이 어떤 종류의 재료로 만들어질 수 있는지를 정의합니다.

### 기본 StuffCategories (Core)

1. **Metallic** - 금속류
   - label: metallic
   - noun: metal
   - 파괴 사운드: BuildingDestroyed_Metal_*

2. **Woody** - 나무류
   - label: woody
   - noun: wood
   - 파괴 사운드: BuildingDestroyed_Wood_*

3. **Stony** - 돌류
   - label: stony
   - noun: stone
   - 파괴 사운드: BuildingDestroyed_Stone_*

4. **Fabric** - 직물류
   - label: fabric
   - noun: fabric
   - 파괴 사운드: BuildingDestroyed_Soft_*

5. **Leathery** - 가죽류
   - label: leathery
   - noun: leather
   - 파괴 사운드: BuildingDestroyed_Soft_*

### DLC 추가 StuffCategories

6. **Bioferrite** (Anomaly DLC)
   - label: bioferrite
   - noun: bioferrite
   - 파괴 사운드: BuildingDestroyed_Metal_*

## StuffCategories 사용 패턴

### 의류 (Apparel)
- **Fabric + Leathery**: 대부분의 의류 아이템
- **Metallic**: 일부 방어구 (헬멧, 갑옷)
- **Woody**: 일부 헤드기어

### 건물 (Buildings)
- **Metallic + Woody + Stony**: 대부분의 건물
- **Fabric + Leathery**: 텐트, 침구류
- **Bioferrite**: Anomaly DLC 관련 건물들

### 특수 아이템
- **Bioferrite**: Anomaly DLC의 특수 장비들
- **Metallic**: 무기, 방어구, 기계류

## 분석 결과

총 **6개의 StuffCategory**가 존재하며, 각각 고유한 특성과 사운드를 가지고 있습니다. 대부분의 제작 가능한 아이템들은 2-3개의 StuffCategory를 지원하여 다양한 재료로 제작할 수 있도록 설계되어 있습니다.
