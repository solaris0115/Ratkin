# RimWorld Exotic Trader Types Analysis

## 개요
일반 벌크/의류/무기 상인을 제외한 특수 상인 유형 조사 보고서

---

## 특수 상인 유형

### 1. Exotic Goods Trader (희귀품 상인)
**DefName**: `Caravan_Outlander_Exotic`, `Orbital_Exotic`

**주요 재고**:
- 고가 자원: ComponentSpacer, Plasteel, Gold, MedicineUltratech
- Techprints 3개
- ExoticMisc 태그 (Luciferium, Hyperweave 등)
- Artifact 태그
- TechHediff (임플란트)
- Genepack (Biotech)
- ArchiteCapsule (Biotech, 궤도만)
- DeathrestCapacitySerum (Biotech)
- Serums (Anomaly, 20% 확률)
- 동물: AnimalUncommon, AnimalFighter, AnimalFarm, AnimalPet

**구매**: ImplantEmpireCommon/Royal, PsylinkNeuroformer, ExoticBuilding, UtilitySpecial 등

---

### 2. Imperial Trader (제국 상인)
**DefName**: `Empire_Caravan_TraderGeneral`, `Orbital_Empire`

**주요 재고**:
- Royal 의류 태그
- PsychicApparel, PsychicWeapon
- HiTechArmor
- SpacerGun 무기
- UltratechMelee, Bladelink 무기
- ImplantEmpireCommon, ImplantEmpireRoyal

**특징**:
- `permitRequiredForTrading`: TradeCaravan/TradeOrbital (허가 필요)
- `faction: Empire`

---

## 비교표

| 상인 유형 | 주요 특징 | DLC 연동 | 거래 통화 |
|---------|---------|---------|---------|
| Exotic Goods | 고급 자원, 유물, 임플란트 | Biotech, Anomaly | Silver |
| Imperial | 로열 의류, 사이킥 장비, 제국 임플란트 | Royalty 필수 | Silver |

---

## 랫킨 Exotic 상인 vs 기본 Exotic 상인 차이점

### 랫킨 Exotic (`RK_TraderKind_KingdomExotic`)
**있음**:
- Silver 더 많음 (1800~3000 vs 750~1200)
- Schematic 더 많음 (2~3 vs 0~1)
- Tomes 더 많음 (2~3 vs 0~1)
- Relic 무기/의류 판매 (Ideology DLC, -1~1)
- ExoticMisc 더 많음 (3~5 vs 3)
- Artifact 더 많음 (2~4 vs 2)

**없음**:
- 기본 자원 (ComponentIndustrial, ComponentSpacer, Plasteel, Gold, MedicineUltratech)
- TextBook, Novel
- TechHediff (임플란트)
- Television, Telescope
- Genepack (Biotech)
- Serums (Anomaly)
- 동물
- ArchiteCapsule, DeathrestCapacitySerum (Biotech)

### 기본 Exotic (`Caravan_Outlander_Exotic`)
**있음**:
- 기본 자원 다양하게 판매
- TechHediff (임플란트)
- Television, Telescope
- Genepack (Biotech, 3~4개)
- Serums (Anomaly, 20% 확률)
- 동물 (AnimalUncommon, AnimalFighter, AnimalFarm, AnimalPet)
- ArchiteCapsule (Biotech, 10% 확률)
- DeathrestCapacitySerum (Biotech)

**없음**:
- Relic 무기/의류

---

## 랫킨 Exotic 상인 개선 참고사항

현재 `RK_TraderKind_KingdomExotic`는 기본 Exotic Goods Trader보다 **Relic 중심**, **자원 판매 없음**이 특징.

**개선 가능 방향**:
1. 랫킨 고유 아이템 추가
2. 랫킨 테마에 맞는 StockGenerator 커스터마이징
