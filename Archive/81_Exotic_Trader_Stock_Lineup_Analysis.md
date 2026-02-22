# Exotic Trader Stock Lineup Analysis

## 개요
각 팩션별 Exotic 상인의 상품 라인업을 카테고리/유형별로 정리하고, 공통점을 제외한 고유 특징만 정리한 보고서

---

## 1. Outlander Exotic (캐러반)
**DefName**: `Caravan_Outlander_Exotic`

### 상품 라인업

#### 자원 (Resources)
- Silver: 750~1200
- ComponentIndustrial: 6~20
- ComponentSpacer: 1~4
- Plasteel: 50~150
- Gold: 40~80
- MedicineUltratech: 2~8

#### 연구 (Research)
- Techprints: 3개

#### 책 (Books)
- TextBook: 1~2
- Schematic: 0~1
- Novel: 0~1
- Tomes (Anomaly): 0~1

#### 임플란트 (Implants)
- TechHediff: 3종류, 1개

#### 건물 (Buildings)
- Television: 1~2종류, 1개
- Telescope: 0~1

#### 희귀품 (Exotic)
- ExoticMisc: 3종류, 1개 (Luciferium 7~25, Hyperweave 40~120)
- Artifact: 2종류, 1개

#### 생물학 (Biotech)
- Genepack: 3~4개
- ArchiteCapsule: 10% 확률 (0~1)
- DeathrestCapacitySerum: 0~1

#### 어노말리 (Anomaly)
- Serums: 4종류, 20% 확률 (-3~1)

#### 동물 (Animals)
- 판매: AnimalUncommon, AnimalFighter, AnimalFarm, AnimalPet (3종류, 1~3마리)
- 구매: AnimalExotic

---

## 2. Orbital Exotic (궤도)
**DefName**: `Orbital_Exotic`

### 상품 라인업

#### 자원 (Resources)
- Silver: 2000~4000
- ComponentIndustrial: 5~30
- ComponentSpacer: 3~8
- Gold: 200~400
- Plasteel: 100~400
- Bioferrite (Anomaly): 25~150
- MedicineUltratech: 5~30
- Neutroamine: 100~500
- Chocolate
- ShuttleEngine (Odyssey): 0~1

#### 마약 (Drugs)
- Drugs 카테고리: 3~4종류, 총가격 2000~3000

#### 연구 (Research)
- Techprints: 3개

#### 책 (Books)
- TextBook: 1~2
- Schematic: 1
- Novel: 1
- Tomes (Anomaly): 1

#### 임플란트 (Implants)
- TechHediff: 3~5종류, 1개

#### 건물 (Buildings)
- Television: 1~3종류, 1개
- Telescope: 1
- Art: 4~8개

#### 희귀품 (Exotic)
- ExoticMisc: 3~5종류, 1~2개 (Hyperweave 50~200, Luciferium은 Drugs로 처리)
- Artifact: 2~4종류, 1개

#### 생물학 (Biotech)
- Genepack: 3~5개
- ArchiteCapsule: 1~2개
- DeathrestCapacitySerum: 1개
- PoluxSeed: 0~1

#### 어노말리 (Anomaly)
- Serums: 4종류, 20% 확률 (-3~1)

#### 동물 (Animals)
- 판매: AnimalUncommon, AnimalFighter, AnimalFarm (1~2종류, 2~3마리, 온도 체크 없음)
- 구매: AnimalExotic

#### 구매 (Buying)
- GravshipUpgrade (Odyssey)

---

## 3. Empire Exotic (캐러반)
**DefName**: `Empire_Caravan_TraderGeneral`

### 상품 라인업

#### 자원 (Resources)
- Silver: 750~1200
- ResourcesRaw 카테고리: 0~1종류
- ComponentIndustrial: 3~7
- ComponentSpacer: 3~7
- Steel: 250~400
- Cloth: 250~400
- MedicineUltratech: 8~16
- ReinforcedBarrels: 1~4
- MortarShell: 1~2종류, 10~20개

#### 마약 (Drugs)
- Drugs 카테고리

#### 연구 (Research)
- Techprints: 1개

#### 책 (Books)
- TextBook: 0~2
- Schematic: 1
- Novel: 1

#### 무기 (Weapons)
- SpacerGun: 1~3개
- PsychicWeapon: 1~2종류, 1~2개

#### 의류 (Apparel)
- Royal BasicClothing: 3~7개
- Royal Clothing: 1~3개
- PsychicApparel: 2~3종류, 1~3개
- HiTechArmor: 0~2종류, 1~2개

#### 임플란트 (Implants)
- ImplantEmpireCommon: 1~2종류, 1~2개
- ImplantEmpireRoyal: 1~2종류, 1~2개

#### 생물학 (Biotech)
- Genepack: 1~2개

#### 동물 (Animals)
- 판매: AnimalUncommon, AnimalExotic, AnimalFarm (2종류, 3~4마리, maxWildness 0.70)
- 구매: AnimalExotic

#### 특징
- `permitRequiredForTrading`: TradeCaravan (허가 필요)
- `faction: Empire`

---

## 4. Empire Exotic (궤도)
**DefName**: `Orbital_Empire`

### 상품 라인업

#### 자원 (Resources)
- Silver: 2000~4000
- ResourcesRaw 카테고리: 0~1종류
- ComponentIndustrial: 5~10
- ComponentSpacer: 10~15
- MedicineUltratech: 30~50
- ReinforcedBarrels: 1~4
- MortarShell: 1~2종류, 20~40개

#### 마약 (Drugs)
- Drugs 카테고리

#### 연구 (Research)
- Techprints: 1개

#### 책 (Books)
- TextBook: 1~2
- Schematic: 1
- Novel: 1

#### 무기 (Weapons)
- SpacerGun: 3~6개
- PsychicWeapon: 1~2종류, 1개
- UltratechMelee: 1~2개
- Bladelink: 0~1개

#### 의류 (Apparel)
- HiTechArmor: 2~4종류, 1~2개
- Royal Clothing: 7~12개
- Royal BasicClothing: 3~4개
- PsychicApparel: 2~3종류, 1~3개

#### 임플란트 (Implants)
- ImplantEmpireCommon: 1~2종류, 1~2개
- ImplantEmpireRoyal: 1~2종류, 1~2개

#### 생물학 (Biotech)
- Genepack: 1~3개

#### 동물 (Animals)
- 판매: AnimalUncommon, AnimalExotic, AnimalFarm (2종류, 3~4마리, maxWildness 0.70)
- 구매: AnimalExotic

#### 특징
- `permitRequiredForTrading`: TradeOrbital (허가 필요)
- `faction: Empire`

---

## 5. Ratkin Exotic (캐러반)
**DefName**: `RK_TraderKind_KingdomExotic`

### 상품 라인업

#### 자원 (Resources)
- Silver: 1800~3000

#### 연구 (Research)
- Techprints: 3개

#### 책 (Books)
- Schematic: 2~3
- Tomes (Anomaly): 2~3

#### 무기 (Weapons)
- Relic 무기 (Ideology): WeaponGun 태그, Gun 태그, -1~1개

#### 의류 (Apparel)
- Relic 의류 (Ideology): Clothing 태그, Armor 태그, -1~1개

#### 희귀품 (Exotic)
- ExoticMisc: 3~5종류, 1개 (Luciferium 7~25, Hyperweave 40~120)
- Artifact: 2~4종류, 1개

---

## 각 상인별 고유 특징 (공통점 제외)

### Outlander Exotic (캐러반)
**고유 특징**:
- 기본 자원 다양하게 판매 (ComponentIndustrial, ComponentSpacer, Plasteel, Gold, MedicineUltratech)
- TextBook, Novel 판매
- TechHediff (임플란트) 판매
- Television, Telescope 판매
- Genepack 3~4개 (가장 많음)
- ArchiteCapsule 10% 확률
- DeathrestCapacitySerum 판매
- Serums 판매 (Anomaly)
- 동물 판매 (AnimalUncommon, AnimalFighter, AnimalFarm, AnimalPet)
- 구매: ExoticBuilding, UtilitySpecial, Anomaly 관련 아이템들

### Orbital Exotic (궤도)
**고유 특징**:
- Drugs 카테고리 판매 (3~4종류, 총가격 2000~3000)
- Bioferrite 판매 (Anomaly)
- Neutroamine 판매 (100~500)
- Chocolate 판매
- ShuttleEngine 판매 (Odyssey)
- TechHediff 3~5종류 (가장 다양)
- Art 판매 (4~8개)
- ArchiteCapsule 1~2개 (확정 판매)
- DeathrestCapacitySerum 확정 판매
- PoluxSeed 판매 (Biotech)
- 동물 온도 체크 없음
- 구매: GravshipUpgrade (Odyssey)

### Empire Exotic (캐러반)
**고유 특징**:
- ResourcesRaw 카테고리 판매
- Steel, Cloth 판매
- ReinforcedBarrels 판매
- MortarShell 판매 (10~20개)
- Drugs 카테고리 판매
- Techprints 1개만 (가장 적음)
- SpacerGun 무기 판매
- PsychicWeapon 판매
- Royal 의류 판매 (BasicClothing, Clothing)
- PsychicApparel 판매
- HiTechArmor 판매
- ImplantEmpireCommon/Royal 판매
- Genepack 1~2개
- 동물 maxWildness 0.70 제한
- 구매: PsylinkNeuroformer만 (다른 상인들보다 구매 항목 적음)
- `permitRequiredForTrading`: TradeCaravan 필수

### Empire Exotic (궤도)
**고유 특징**:
- ResourcesRaw 카테고리 판매
- ComponentSpacer 10~15개 (가장 많음)
- MedicineUltratech 30~50개 (가장 많음)
- MortarShell 20~40개 (가장 많음)
- Drugs 카테고리 판매
- Techprints 1개만
- SpacerGun 3~6개 (가장 많음)
- UltratechMelee 판매
- Bladelink 판매
- Royal 의류 7~12개 (가장 많음)
- HiTechArmor 2~4종류 (가장 다양)
- ImplantEmpireCommon/Royal 판매
- Genepack 1~3개
- 동물 maxWildness 0.70 제한
- 구매: PsylinkNeuroformer만
- `permitRequiredForTrading`: TradeOrbital 필수

### Ratkin Exotic (캐러반)
**고유 특징**:
- Silver만 판매 (다른 자원 없음)
- Schematic 2~3개 (가장 많음)
- Tomes 2~3개 (가장 많음)
- Relic 무기/의류 판매 (Ideology DLC, 다른 상인들에는 없음)
- ExoticMisc 3~5종류 (가장 다양)
- Artifact 2~4종류 (가장 다양)
- 구매: ExoticBuilding, UtilitySpecial, ImplantEmpireCommon/Royal, Anomaly 관련 아이템들
- **없는 것**: 기본 자원, TextBook, Novel, TechHediff, Television, Telescope, Genepack, Serums, 동물, ArchiteCapsule, DeathrestCapacitySerum

---

## 요약 비교

| 상인 | 자원 | 무기/의류 | 특수 아이템 | 동물 | DLC 연동 |
|------|------|----------|------------|------|---------|
| Outlander (캐러반) | 다양 | 없음 | TechHediff, Genepack, Serums | 있음 | Biotech, Anomaly |
| Orbital (궤도) | 매우 다양 + Drugs | 없음 | TechHediff, Genepack, Serums, Art | 있음 | Biotech, Anomaly, Odyssey |
| Empire (캐러반) | 기본 + 전투용 | SpacerGun, Psychic, Royal | ImplantEmpire | 있음 | Royalty 필수 |
| Empire (궤도) | 기본 + 전투용 (많음) | SpacerGun, Psychic, Royal, Ultratech | ImplantEmpire | 있음 | Royalty 필수 |
| Ratkin (캐러반) | Silver만 | Relic 무기/의류 | 없음 | 없음 | Ideology (Relic용) |
