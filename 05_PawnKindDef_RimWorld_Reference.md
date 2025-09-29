# PawnKindDef 림월드 데이터 참조 문서

## 개요

이 문서는 림월드 원본 데이터에서 PawnKindDef의 구조와 속성들을 분석한 참조 문서입니다. Ratkin 프로젝트에서 PawnKindDef를 생성하거나 수정할 때 참고할 수 있습니다.

## PawnKindDef 기본 구조

### 기본 태그
```xml
<PawnKindDef>
  <defName>DefName</defName>
  <label>표시명</label>
  <labelPlural>복수형 표시명</labelPlural>
  <labelMale>남성 표시명</labelMale>
  <labelFemale>여성 표시명</labelFemale>
  <labelFemalePlural>여성 복수형 표시명</labelFemalePlural>
</PawnKindDef>
```

### 상속 구조
```xml
<PawnKindDef ParentName="BasePawnKind" Name="CustomBase" Abstract="True">
  <!-- 상속받은 속성들을 오버라이드 -->
</PawnKindDef>
```

## 주요 속성 분석

### 1. 기본 정보
- **defName**: 고유 식별자
- **label**: 게임 내 표시명
- **race**: 종족 (Human, Ratkin 등)
- **defaultFactionDef**: 기본 소속 팩션
- **Abstract**: 추상 클래스 여부 (상속용)

### 2. 전투 관련
- **combatPower**: 전투력 (30~150 범위)
- **isFighter**: 전투원 여부
- **canBeSapper**: 사퍼 가능 여부
- **isGoodBreacher**: 우수한 돌파수 여부
- **factionLeader**: 팩션 리더 여부

### 3. 장비 및 아이템
- **apparelMoney**: 의류 예산 (50~9999999)
- **weaponMoney**: 무기 예산 (0~3500)
- **itemQuality**: 아이템 품질 (Poor, Normal, Good, Excellent)
- **gearHealthRange**: 장비 내구도 범위 (0.2~2.3)
- **forceNormalGearQuality**: 강제 일반 품질
- **forceWeaponQuality**: 강제 무기 품질

### 4. 의류 설정
- **apparelTags**: 의류 태그 목록
- **apparelRequired**: 필수 의류 목록
- **apparelDisallowTags**: 금지 의류 태그
- **apparelAllowHeadgearChance**: 헤드기어 착용 확률 (0~1)
- **apparelIgnoreSeasons**: 계절 무시
- **apparelIgnorePollution**: 오염 무시
- **apparelColor**: 의류 색상

### 5. 무기 설정
- **weaponTags**: 무기 태그 목록
- **biocodeWeaponChance**: 바이오코드 무기 확률 (0~1)

### 6. 기술적 개선 (TechHediffs)
- **techHediffsChance**: 기술적 개선 확률 (0~1)
- **techHediffsMoney**: 기술적 개선 예산 (0~4000)
- **techHediffsTags**: 기술적 개선 태그
- **techHediffsRequired**: 필수 기술적 개선
- **techHediffsDisallowTags**: 금지 기술적 개선 태그
- **techHediffsMaxAmount**: 최대 기술적 개선 수

### 7. 특성 및 작업
- **requiredWorkTags**: 필수 작업 태그
- **disallowedTraits**: 금지 특성
- **disallowedTraitsWithDegree**: 금지 특성 (정도별)
- **forcedTraits**: 강제 특성
- **requiredTraits**: 필수 특성

### 8. 나이 및 생물학
- **minGenerationAge**: 최소 생성 나이
- **maxGenerationAge**: 최대 생성 나이
- **chronologicalAgeRange**: 연령 범위
- **humanPregnancyChance**: 임신 확률 (0~1)

### 9. 정신 상태
- **initialWillRange**: 초기 의지력 범위
- **initialResistanceRange**: 초기 저항력 범위
- **chemicalAddictionChance**: 화학 중독 확률

### 10. 인벤토리 및 소지품
- **invNutrition**: 보유 영양분
- **invFoodDef**: 보유 음식 종류
- **inventoryOptions**: 인벤토리 옵션

### 11. 배경 스토리
- **backstoryFilters**: 배경 스토리 필터
- **backstoryFiltersOverride**: 배경 스토리 필터 오버라이드
- **backstoryCryptosleepCommonality**: 크립토슬립 배경 확률

### 12. 로열티 관련
- **royalTitleChance**: 로열 타이틀 확률
- **titleRequired**: 필수 타이틀
- **titleSelectOne**: 선택 가능한 타이틀
- **minTitleRequired**: 최소 필요 타이틀
- **allowRoyalRoomRequirements**: 로열 방 요구사항 허용
- **allowRoyalApparelRequirements**: 로열 의류 요구사항 허용

### 13. 제노타입 (Biotech DLC)
- **xenotypeSet**: 제노타입 세트
- **useFactionXenotypes**: 팩션 제노타입 사용

### 14. 기타
- **trader**: 상인 여부
- **acceptArrestChanceFactor**: 체포 수락 확률 배수
- **factionHostileOnDeath**: 사망 시 팩션 적대화
- **pawnGroupDevelopmentStage**: 개발 단계 (Child 등)

## 주요 PawnKindDef 유형별 분석

### 1. 플레이어 관련
- **Colonist**: 기본 식민지 주민
- **Tribesperson**: 부족 주민
- **SpaceRefugee**: 우주 난민
- **Mechanitor**: 메카니터 (Biotech DLC)

### 2. 부족 (Tribal)
- **Tribal_Penitent**: 참회자 (빈곤층)
- **Tribal_Archer**: 궁수 (저층)
- **Tribal_Warrior**: 전사 (중층)
- **Tribal_Hunter**: 사냥꾼 (중층)
- **Tribal_Berserker**: 광전사 (고층)
- **Tribal_HeavyArcher**: 중궁수 (고층)
- **Tribal_ChiefMelee**: 족장 (근접)
- **Tribal_ChiefRanged**: 족장 (원거리)

### 3. 외지인 (Outlander)
- **Villager**: 마을 주민
- **Town_Guard**: 마을 경비
- **Town_Trader**: 마을 상인
- **Town_Councilman**: 마을 의원

### 4. 해적 (Pirate)
- **Drifter**: 표류자 (빈곤층)
- **Scavenger**: 청소부 (저층)
- **Thrasher**: 난타자 (저층)
- **Pirate**: 해적 (중층)

### 5. 제국 (Empire)
- **Empire_Common_Lodger**: 제국 시민
- **Empire_Common_Trader**: 제국 상인
- **Empire_Common_Laborer**: 제국 노동자
- **Empire_Fighter_Trooper**: 제국 병사
- **Empire_Fighter_Grenadier**: 제국 수류탄병
- **Empire_Fighter_Janissary**: 제국 친위대
- **Empire_Fighter_Champion**: 제국 챔피언
- **Empire_Fighter_Cataphract**: 제국 기갑병
- **Empire_Fighter_StellicGuardRanged**: 스텔릭 수호자 (원거리)
- **Empire_Fighter_StellicGuardMelee**: 스텔릭 수호자 (근접)

### 6. 로열티
- **Empire_Royal_NobleWimp**: 귀족 (약함)
- **Empire_Royal_Yeoman**: 신병
- **Empire_Royal_Acolyte**: 수행사제
- **Empire_Royal_Knight**: 기사
- **Empire_Royal_Praetor**: 집정관
- **Empire_Royal_Baron**: 남작
- **Empire_Royal_Count**: 백작
- **Empire_Royal_Duke**: 공작
- **Empire_Royal_Consul**: 집정관
- **Empire_Royal_Stellarch**: 스텔라크
- **Empire_Royal_Bestower**: 수여자

### 7. 특수
- **Slave**: 노예
- **WildMan**: 야생인
- **StrangerInBlack**: 검은 옷의 낯선 사람
- **AncientSoldier**: 고대 병사
- **Sanguophage**: 상귀포지 (Biotech DLC)
- **SanguophageThrall**: 상귀포지 노예

## 주요 태그 시스템

### 의류 태그
- **Neolithic**: 신석기 시대
- **IndustrialBasic**: 산업 기본
- **IndustrialAdvanced**: 산업 고급
- **IndustrialMilitaryBasic**: 산업 군사 기본
- **IndustrialMilitaryAdvanced**: 산업 군사 고급
- **SpacerMilitary**: 우주군
- **RoyalTier2~7**: 로열티 등급
- **BeltDefensePop**: 방어 벨트
- **Western**: 서부 스타일
- **Cape**: 망토
- **Psychic**: 사이킥
- **Gunlink**: 건링크
- **Bladelink**: 블레이드링크

### 무기 태그
- **NeolithicMeleeBasic**: 신석기 근접 기본
- **NeolithicMeleeDecent**: 신석기 근접 양호
- **NeolithicMeleeAdvanced**: 신석기 근접 고급
- **NeolithicRangedBasic**: 신석기 원거리 기본
- **NeolithicRangedDecent**: 신석기 원거리 양호
- **NeolithicRangedHeavy**: 신석기 원거리 중화기
- **NeolithicRangedChief**: 신석기 원거리 족장용
- **NeolithicRangedFlame**: 신석기 원거리 화염
- **MedievalMeleeBasic**: 중세 근접 기본
- **MedievalMeleeDecent**: 중세 근접 양호
- **MedievalMeleeAdvanced**: 중세 근접 고급
- **Gun**: 총기
- **SimpleGun**: 간단한 총기
- **IndustrialGunAdvanced**: 산업 고급 총기
- **SpacerGun**: 우주 총기
- **SniperRifle**: 저격총
- **GunHeavy**: 중화기
- **Revolver**: 리볼버
- **Autopistol**: 자동권총
- **UltratechMelee**: 초고급 근접
- **Bladelink**: 블레이드링크
- **EltexStaff**: 엘텍스 지팡이
- **GrenadeFlame**: 화염 수류탄
- **EmpireGrenadeDestructive**: 제국 파괴 수류탄

### 기술적 개선 태그
- **Poor**: 빈곤
- **Simple**: 단순
- **Advanced**: 고급
- **AdvancedWeapon**: 고급 무기
- **ImplantEmpireCommon**: 제국 일반 임플란트
- **ImplantEmpireRoyal**: 제국 로열 임플란트
- **PainCauser**: 통증 유발

### 작업 태그
- **Violent**: 폭력적
- **Caring**: 돌봄
- **Intellectual**: 지적
- **Social**: 사회적
- **ManualDumb**: 단순 노동
- **ManualSkilled**: 숙련 노동
- **Cleaning**: 청소
- **Hauling**: 운반
- **Mining**: 채굴
- **Firefighting**: 소방

## 특수 설정 예시

### 색상 생성기
```xml
<colorGenerator Class="ColorGenerator_Options">
  <options>
    <li><only>(0.7, 0.7, 0.7)</only></li>
    <li><only>(104, 120, 119)</only></li>
  </options>
</colorGenerator>
```

### 특정 의류 요구사항
```xml
<specificApparelRequirements>
  <li>
    <bodyPartGroup>UpperHead</bodyPartGroup>
    <requiredTag>RoyalTier3</requiredTag>
    <stuff>Steel</stuff>
  </li>
  <li>
    <bodyPartGroup>Torso</bodyPartGroup>
    <apparelLayer>OnSkin</apparelLayer>
    <requiredTag>RoyalTier2</requiredTag>
  </li>
</specificApparelRequirements>
```

### 스킬 설정
```xml
<skills>
  <li>
    <skill>Shooting</skill>
    <range>4~18</range>
  </li>
  <li>
    <skill>Melee</skill>
    <range>8~13</range>
  </li>
</skills>
```

### 인벤토리 옵션
```xml
<inventoryOptions>
  <skipChance>0.9</skipChance>
  <subOptionsChooseOne>
    <li>
      <thingDef>MedicineIndustrial</thingDef>
      <countRange>1</countRange>
    </li>
  </subOptionsChooseOne>
</inventoryOptions>
```

## Ratkin 프로젝트 적용 가이드

### 1. 네이밍 규칙
- **DefName**: `RK_PawnKind_Xxx` 형식
- **Label**: 랫킨 특성에 맞는 표시명
- **Race**: `RK_Race_Ratkin` 참조

### 2. 기본 설정
- **combatPower**: 30~100 범위 권장
- **apparelMoney**: 200~800 범위 권장
- **weaponMoney**: 100~500 범위 권장
- **techHediffsChance**: 0.03~0.15 범위 권장

### 3. 의류 태그
- **IndustrialBasic**: 기본 산업 의류
- **IndustrialAdvanced**: 고급 산업 의류
- **BeltDefensePop**: 방어 벨트

### 4. 무기 태그
- **Gun**: 기본 총기
- **MedievalMeleeDecent**: 중세 근접 무기
- **IndustrialGunAdvanced**: 고급 산업 총기

### 5. 특성 설정
- **requiredWorkTags**: `Violent` (전투원인 경우)
- **disallowedTraits**: `Brawler` (원거리 전투원인 경우)

이 문서를 참고하여 Ratkin 프로젝트에 맞는 PawnKindDef를 생성하고 수정할 수 있습니다.
