# ThingDef Apparel 계열 분석 보고서

## 1. 림월드 데이터에서 DLC 포함 각 파일들 링크 숏컷

### Core (기본 게임)
- **Apparel_Various.xml**: [RimWorldData/Core/Defs/ThingDefs_Misc/Apparel_Various.xml](RimWorldData/Core/Defs/ThingDefs_Misc/Apparel_Various.xml)
- **Apparel_Headgear.xml**: [RimWorldData/Core/Defs/ThingDefs_Misc/Apparel_Headgear.xml](RimWorldData/Core/Defs/ThingDefs_Misc/Apparel_Headgear.xml)
- **Apparel_Utility.xml**: [RimWorldData/Core/Defs/ThingDefs_Misc/Apparel_Utility.xml](RimWorldData/Core/Defs/ThingDefs_Misc/Apparel_Utility.xml)
- **Apparel_Packs.xml**: [RimWorldData/Core/Defs/ThingDefs_Misc/Apparel_Packs.xml](RimWorldData/Core/Defs/ThingDefs_Misc/Apparel_Packs.xml)
- **Apparel_Belts.xml**: [RimWorldData/Core/Defs/ThingDefs_Misc/Apparel_Belts.xml](RimWorldData/Core/Defs/ThingDefs_Misc/Apparel_Belts.xml)

### Royalty DLC
- **Apparel_Various.xml**: [RimWorldData/Royalty/Defs/ThingDefs_Misc/Apparel_Various.xml](RimWorldData/Royalty/Defs/ThingDefs_Misc/Apparel_Various.xml)
- **Apparel_Royal.xml**: [RimWorldData/Royalty/Defs/ThingDefs_Misc/Apparel_Royal.xml](RimWorldData/Royalty/Defs/ThingDefs_Misc/Apparel_Royal.xml)
- **Apparel_Psychic.xml**: [RimWorldData/Royalty/Defs/ThingDefs_Misc/Apparel_Psychic.xml](RimWorldData/Royalty/Defs/ThingDefs_Misc/Apparel_Psychic.xml)
- **Apparel_Packs.xml**: [RimWorldData/Royalty/Defs/ThingDefs_Misc/Apparel_Packs.xml](RimWorldData/Royalty/Defs/ThingDefs_Misc/Apparel_Packs.xml)

### Ideology DLC
- **Apparel_Various.xml**: [RimWorldData/Ideology/Defs/ThingDefs_Misc/Apparel_Various.xml](RimWorldData/Ideology/Defs/ThingDefs_Misc/Apparel_Various.xml)
- **Apparel_Headgear.xml**: [RimWorldData/Ideology/Defs/ThingDefs_Misc/Apparel_Headgear.xml](RimWorldData/Ideology/Defs/ThingDefs_Misc/Apparel_Headgear.xml)

### Biotech DLC
- **Apparel_Various.xml**: [RimWorldData/Biotech/Defs/ThingDefs_Misc/Apparel_Various.xml](RimWorldData/Biotech/Defs/ThingDefs_Misc/Apparel_Various.xml)
- **Apparel_Headgear.xml**: [RimWorldData/Biotech/Defs/ThingDefs_Misc/Apparel_Headgear.xml](RimWorldData/Biotech/Defs/ThingDefs_Misc/Apparel_Headgear.xml)
- **Apparel_Packs.xml**: [RimWorldData/Biotech/Defs/ThingDefs_Misc/Apparel_Packs.xml](RimWorldData/Biotech/Defs/ThingDefs_Misc/Apparel_Packs.xml)
- **Apparel_Mech.xml**: [RimWorldData/Biotech/Defs/ThingDefs_Misc/Apparel_Mech.xml](RimWorldData/Biotech/Defs/ThingDefs_Misc/Apparel_Mech.xml)
- **Apparel_Child.xml**: [RimWorldData/Biotech/Defs/ThingDefs_Misc/Apparel_Child.xml](RimWorldData/Biotech/Defs/ThingDefs_Misc/Apparel_Child.xml)

### Anomaly DLC
- **Apparel_Various.xml**: [RimWorldData/Anomaly/Defs/ThingDefs_Misc/Apparel_Various.xml](RimWorldData/Anomaly/Defs/ThingDefs_Misc/Apparel_Various.xml)
- **Apparel_Packs.xml**: [RimWorldData/Anomaly/Defs/ThingDefs_Misc/Apparel_Packs.xml](RimWorldData/Anomaly/Defs/ThingDefs_Misc/Apparel_Packs.xml)
- **Apparel_Cult.xml**: [RimWorldData/Anomaly/Defs/ThingDefs_Misc/Apparel_Cult.xml](RimWorldData/Anomaly/Defs/ThingDefs_Misc/Apparel_Cult.xml)
- **Apparel_Utility.xml**: [RimWorldData/Anomaly/Defs/ThingDefs_Misc/Apparel_Utility.xml](RimWorldData/Anomaly/Defs/ThingDefs_Misc/Apparel_Utility.xml)

### Odyssey DLC
- **Apparel_Various.xml**: [RimWorldData/Odyssey/Defs/ThingDefs_Misc/Apparel_Various.xml](RimWorldData/Odyssey/Defs/ThingDefs_Misc/Apparel_Various.xml)
- **Apparel_Headgear.xml**: [RimWorldData/Odyssey/Defs/ThingDefs_Misc/Apparel_Headgear.xml](RimWorldData/Odyssey/Defs/ThingDefs_Misc/Apparel_Headgear.xml)
- **Apparel_Packs.xml**: [RimWorldData/Odyssey/Defs/ThingDefs_Misc/Apparel_Packs.xml](RimWorldData/Odyssey/Defs/ThingDefs_Misc/Apparel_Packs.xml)

### 관련 설정 파일
- **ApparelLayerDefs.xml**: [RimWorldData/Core/Defs/Misc/ApparelLayerDefs/ApparelLayerDefs.xml](RimWorldData/Core/Defs/Misc/ApparelLayerDefs/ApparelLayerDefs.xml)
- **Stats_Apparel.xml**: [RimWorldData/Core/Defs/Stats/Stats_Apparel.xml](RimWorldData/Core/Defs/Stats/Stats_Apparel.xml)

## 2. Apparel Def 구조

### 기본 상속 구조
```
ApparelNoQualityBase (Abstract)
├── ApparelBase (Abstract)
│   ├── ApparelMakeableBase (Abstract)
│   │   ├── HatMakeableBase (Abstract)
│   │   ├── NobleHatMakeableBase (Abstract)
│   │   └── ArmorSmithableBase (Abstract)
│   │       └── ArmorMachineableBase (Abstract)
│   │           ├── ApparelArmorPowerBase (Abstract)
│   │           ├── ApparelArmorReconBase (Abstract)
│   │           └── ApparelArmorCataphractBase (Abstract)
│   └── HatBase (Abstract)
│       └── ArmorHelmetMakeableBase (Abstract)
│           ├── ApparelArmorHelmetPowerBase (Abstract)
│           ├── ApparelArmorHelmetReconBase (Abstract)
│           └── ApparelArmorHelmetCataphractBase (Abstract)
```

### 주요 컴포넌트 (Comps)
- **CompProperties_Forbiddable**: 금지 설정
- **CompColorable**: 색상 변경 가능
- **CompQuality**: 품질 시스템
- **CompProperties_Styleable**: 스타일 시스템
- **CompProperties_Shield**: 에너지 실드
- **CompProperties_ApparelReloadable**: 재장전 가능한 장비
- **CompProperties_Biocodable**: 생체 인증
- **CompProperties_Explosive**: 폭발 기능
- **CompProperties_CauseHediff_Apparel**: 상태 효과 부여

### Apparel 레이어 시스템
1. **OnSkin** (drawOrder: 0): 피부에 직접 착용
2. **Middle** (drawOrder: 100): 중간 레이어
3. **Shell** (drawOrder: 200): 외부 레이어
4. **Belt** (drawOrder: 300): 벨트/유틸리티
5. **Overhead** (drawOrder: 400): 머리 장식
6. **EyeCover** (drawOrder: 500): 눈 가리개

### 신체 부위 그룹 (BodyPartGroups)
- **Torso**: 몸통
- **Legs**: 다리
- **Arms**: 팔
- **Shoulders**: 어깨
- **Neck**: 목
- **UpperHead**: 머리 상부
- **FullHead**: 머리 전체
- **Eyes**: 눈
- **Mouth**: 입
- **Waist**: 허리

## 3. 각 앨리먼트별 기능과 설명

### 기본 의류 (Core Apparel)

#### Neolithic 시대
- **Apparel_TribalA**: 원시 부족 의상, 전체 몸통+다리 커버
- **Apparel_Parka**: 파카, 추위 보호용 외투
- **Apparel_TribalHeaddress**: 부족 머리 장식, 사회적 영향력 증가
- **Apparel_WarMask**: 전쟁 가면, 고통 임계값 증가
- **Apparel_WarVeil**: 베일, 고통 임계값 및 독성 환경 저항

#### Medieval 시대
- **Apparel_Pants**: 바지, 기본 하의
- **Apparel_BasicShirt**: 기본 티셔츠
- **Apparel_CollarShirt**: 버튼다운 셔츠
- **Apparel_Duster**: 더스터 코트, 노예 억압 감소
- **Apparel_Jacket**: 재킷, 보온 및 보호
- **Apparel_PlateArmor**: 판금 갑옷, 강력한 방어력

#### Industrial 시대
- **Apparel_FlakVest**: 플랙 조끼, 가슴 보호
- **Apparel_FlakPants**: 플랙 바지, 다리 보호
- **Apparel_FlakJacket**: 플랙 재킷, 상체 보호
- **Apparel_SimpleHelmet**: 간단한 헬멧
- **Apparel_AdvancedHelmet**: 고급 헬멧 (플랙 헬멧)

#### Spacer 시대
- **Apparel_PowerArmor**: 마린 아머, 강력한 방어력
- **Apparel_ArmorRecon**: 정찰 아머, 경량 고기술 아머
- **Apparel_PowerArmorHelmet**: 마린 헬멧
- **Apparel_ArmorHelmetRecon**: 정찰 헬멧
- **Apparel_ShieldBelt**: 실드 벨트, 투사체 차단

### 유틸리티 장비

#### 방어용 팩
- **Apparel_SmokepopBelt**: 연막 팩, 시야 차단
- **Apparel_FirefoampopPack**: 소화제 팩, 화재 진압

#### 특수 장비
- **Apparel_PsychicShockLance**: 사이킥 충격 랜스, 정신 충격
- **Apparel_PsychicInsanityLance**: 사이킥 광기 랜스, 광분 유발
- **Apparel_PsychicFoilHelmet**: 사이킥 포일 헬멧, 사이킥 감수성 감소

### Royalty DLC 전용

#### 프레스티지 아머
- **Apparel_ArmorReconPrestige**: 프레스티지 정찰 아머
- **Apparel_ArmorMarinePrestige**: 프레스티지 마린 아머
- **Apparel_ArmorCataphractPrestige**: 프레스티지 카타프랙트 아머
- 사이킥 감수성 및 엔트로피 회복률 증가

#### 카타프랙트 아머
- **Apparel_ArmorCataphract**: 카타프랙트 아머, 최고 방어력
- **Apparel_ArmorHelmetCataphract**: 카타프랙트 헬멧

#### 변형 아머
- **Apparel_ArmorLocust**: 로커스트 아머, 점프 기능
- **Apparel_ArmorMarineGrenadier**: 그레네이더 아머, 유탄 발사기
- **Apparel_ArmorCataphractPhoenix**: 피닉스 아머, 화염 저항

#### 로얄 의류
- **Apparel_ShirtRuffle**: 정장 셔츠
- **Apparel_Corset**: 코르셋 (여성용)
- **Apparel_VestRoyal**: 정장 조끼 (남성용)
- **Apparel_RobeRoyal**: 프레스티지 로브
- **Apparel_Cape**: 망토

#### 로얄 머리 장식
- **Apparel_HatLadies**: 레이디스 모자 (여성용)
- **Apparel_HatTop**: 탑햇 (남성용)
- **Apparel_Beret**: 베레모
- **Apparel_Coronet**: 소관
- **Apparel_Crown**: 왕관
- **Apparel_CrownStellic**: 스텔릭 왕관

### Ideology DLC 전용

#### 종교/이데올로기 의류
- **Apparel_BodyStrap**: 노예 몸끈, 노예 억압 증가
- **Apparel_Burka**: 부르카, 전체 몸 가림
- **Apparel_TortureCrown**: 고문 왕관, 고통 유발
- **Apparel_Blindfold**: 눈가리개, 시야 차단

### Biotech DLC 전용

#### 메크 관련
- **Apparel_MechlordSuit**: 메크로드 슈트, 메크 대역폭 증가

#### 특수 의류
- **Apparel_Bandolier**: 중량 밴돌리어, 원거리 쿨다운 감소
- **Apparel_Sash**: 사시, 최소한의 보호

### Anomaly DLC 전용

#### 연구용
- **Apparel_LabCoat**: 실험복, 연구 속도 증가

### 주요 스탯 시스템

#### 방어력 (Armor Rating)
- **ArmorRating_Sharp**: 날카로운 공격 방어 (총알, 칼, 폭발)
- **ArmorRating_Blunt**: 둔기 공격 방어 (주먹, 몽둥이)
- **ArmorRating_Heat**: 열 공격 방어 (화상)

#### 보온성 (Insulation)
- **Insulation_Cold**: 추위 보호
- **Insulation_Heat**: 더위 보호

#### 특수 스탯
- **EnergyShieldEnergyMax**: 실드 최대 에너지
- **EnergyShieldRechargeRate**: 실드 재충전 속도
- **PackRadius**: 팩 효과 반경
- **EquipDelay**: 장착/해제 시간

#### 장착 시 스탯 변화 (equippedStatOffsets)
- **MoveSpeed**: 이동 속도
- **SocialImpact**: 사회적 영향력
- **SlaveSuppressionOffset**: 노예 억압
- **PsychicSensitivity**: 사이킥 감수성
- **ResearchSpeed**: 연구 속도
- **ShootingAccuracyPawn**: 사격 정확도
- **RangedCooldownFactor**: 원거리 쿨다운
- **VacuumResistance**: 진공 저항 (Odyssey DLC)

### 제작 시스템

#### 제작 시설
- **ElectricTailoringBench**: 전기 재봉대
- **HandTailoringBench**: 수동 재봉대
- **CraftingSpot**: 제작 지점
- **ElectricSmithy**: 전기 대장간
- **FueledSmithy**: 연료 대장간
- **TableMachining**: 기계 가공대
- **FabricationBench**: 제작대

#### 재료 카테고리
- **Fabric**: 직물
- **Leathery**: 가죽류
- **Metallic**: 금속류
- **Woody**: 목재류

#### 품질 시스템
- Awful (0.6x) → Poor (0.8x) → Normal (1.0x) → Good (1.15x) → Excellent (1.3x) → Masterwork (1.45x) → Legendary (1.8x)

### 태그 시스템

#### 기술 레벨 태그
- **Neolithic**: 신석기 시대
- **IndustrialBasic**: 산업 기본
- **IndustrialAdvanced**: 산업 고급
- **SpacerMilitary**: 우주군사

#### 용도 태그
- **Worker**: 작업자용
- **Soldier**: 군인용
- **Spacefarer**: 우주여행자용
- **Slave**: 노예용
- **Nudist**: 누디스트용

#### 로얄티어 태그
- **RoyalTier2** ~ **RoyalTier7**: 로얄티어 등급

이 보고서는 림월드의 복잡한 의류 시스템을 체계적으로 분석한 것으로, 새로운 의류 아이템 개발 시 참고할 수 있는 완전한 가이드입니다.
