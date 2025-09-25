# PawnKindDef 참조 가이드

## 림월드 데이터 파일
- Core: PawnKinds_Player.xml, PawnKinds_Tribal.xml, PawnKinds_Pirate.xml, PawnKinds_Outlander.xml, PawnKinds_Mercenary.xml, PawnKinds_Spacer.xml, PawnKinds_Special.xml, PawnKinds_Breach.xml
- Royalty: PawnKinds_Empire.xml, PawnKinds_Refugee.xml
- Ideology: PawnKinds_Special.xml, PawnKinds_NeutralCamps.xml
- Biotech: PawnKinds_Yttakin.xml, PawnKinds_Waster.xml, PawnKinds_Pigskin.xml, PawnKinds_Impid.xml, PawnKinds_Special.xml
- Anomaly: PawnKinds_Horaxian.xml, PawnKinds_Fleshbeasts.xml, PawnKinds_Mutants.xml, PawnKinds_Player.xml, PawnKinds_Entities.xml
- Odyssey: PawnKinds_Player.xml, PawnKinds_Drones.xml, PawnKinds_TradersGuild.xml, PawnKinds_Mechanoids_Medium.xml, PawnKinds_Spacer.xml, PawnKinds_Salvagers.xml

## 필수 필드
- defName: 고유 정의명
- label: 표시명
- race: 종족 정의

## 기본 설정
- Abstract: 추상 정의 여부
- ParentName: 부모 정의명 (상속)
- defaultFactionDef: 기본 팩션
- combatPower: 전투력
- isFighter: 전투원 여부

## 백스토리
- backstoryFilters: 백스토리 필터
- backstoryFiltersOverride: 백스토리 필터 오버라이드
- backstoryCategories: 백스토리 카테고리
- fixedChildBackstories: 고정 아동기 백스토리
- fixedAdultBackstories: 고정 성인기 백스토리
- backstoryCryptosleepCommonality: 크립토슬립 백스토리 확률

## 생애 단계 (lifeStages)
- label: 생애 단계 표시명
- bodyGraphicData: 몸체 그래픽
- femaleGraphicData: 여성 몸체 그래픽
- corpseGraphicData: 시체 그래픽
- swimmingGraphicData: 수영 그래픽
- flyingAnimationEast/North/South: 비행 애니메이션

## 외관
- alternateGraphics: 대체 그래픽
- alternateGraphicChance: 대체 그래픽 확률
- styleItemTags: 스타일 아이템 태그
- forcedHair: 강제 헤어
- forcedHairColor: 강제 헤어 색상
- apparelColor: 의류 색상
- skinColorOverride: 피부 색상 오버라이드

## 특성 및 능력
- forcedTraits: 강제 특성
- disallowedTraits: 금지 특성
- abilities: 능력 목록

## 생체공학
- xenotypeSet: 제노타입 세트
- useFactionXenotypes: 팩션 제노타입 사용
- techHediffsRequired: 필수 기술 헤딥
- techHediffsMoney: 기술 헤딥 비용
- techHediffsTags: 기술 헤딥 태그
- techHediffsChance: 기술 헤딥 확률

## 나이 및 성별
- chronologicalAgeRange: 연령 범위
- minGenerationAge: 최소 생성 나이
- maxGenerationAge: 최대 생성 나이
- fixedGender: 고정 성별
- humanPregnancyChance: 임신 확률

## 전투 및 AI
- canArriveManhunter: 맨헌터 도착 가능
- canBeSapper: 사퍼 가능
- isGoodBreacher: 좋은 브리처
- allowInMechClusters: 메크 클러스터 허용
- maxPerGroup: 그룹당 최대 수
- aiAvoidCover: AI 엄폐물 회피
- fleeHealthThresholdRange: 도주 체력 임계값
- acceptArrestChanceFactor: 체포 수락 확률

## 무기 및 장비
- itemQuality: 아이템 품질
- forceWeaponQuality: 강제 무기 품질
- gearHealthRange: 장비 내구도 범위
- weaponMoney: 무기 비용 범위
- weaponTags: 무기 태그
- weaponStuffOverride: 무기 재료 오버라이드
- biocodeWeaponChance: 바이오코드 무기 확률

## 의류
- apparelMoney: 의류 비용 범위
- apparelRequired: 필수 의류
- apparelTags: 의류 태그
- apparelDisallowTags: 금지 의류 태그
- apparelAllowHeadgearChance: 헤드기어 허용 확률
- apparelIgnoreSeasons: 계절 의류 무시
- apparelIgnorePollution: 오염 의류 무시
- minApparelQuality: 최소 의류 품질
- maxApparelQuality: 최대 의류 품질

## 인벤토리
- fixedInventory: 고정 인벤토리
- inventoryOptions: 인벤토리 옵션
- invNutrition: 인벤토리 영양분
- invFoodDef: 인벤토리 음식

## 화학물질
- chemicalAddictionChance: 화학물질 중독 확률
- combatEnhancingDrugsChance: 전투 강화 약물 확률
- forcedAddictions: 강제 중독

## 스킬 및 작업
- skills: 스킬 범위
- requiredWorkTags: 필수 작업 태그
- disabledWorkTags: 비활성화 작업 태그
- extraSkillLevels: 추가 스킬 레벨
- minTotalSkillLevels: 최소 총 스킬 레벨

## 로얄티
- royalTitleChance: 로얄 타이틀 확률
- titleRequired: 필수 로얄 타이틀
- minTitleRequired: 최소 필수 로얄 타이틀
- allowRoyalRoomRequirements: 로얄 방 요구사항 허용

## 저항 및 의지
- initialResistanceRange: 초기 저항 범위
- initialWillRange: 초기 의지 범위

## 행동
- destroyGearOnDrop: 드롭 시 장비 파괴
- canStrip: 벗기기 가능
- factionHostileOnKill: 살인 시 팩션 적대
- canMeleeAttack: 근접 공격 가능
- canOpenDoors: 문 열기 가능
- trader: 상인 여부

## 동물
- wildGroupSize: 야생 그룹 크기
- ecoSystemWeight: 생태계 가중치

## 기타
- isBoss: 보스 여부
- nakedChance: 나체 확률
- factionLeader: 팩션 리더 여부
- preventIdeo: 이데올로지 방지
