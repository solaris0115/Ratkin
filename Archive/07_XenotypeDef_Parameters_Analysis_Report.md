# XenotypeDef 파라미터 분석 보고서

## 개요
- **작업 일자**: 2024년 12월 19일
- **목적**: XenotypeDef에서 사용할 수 있는 모든 파라미터 분석
- **분석 범위**: 
  - 현재 프로젝트 XenotypeDef 파일
  - RimWorld 원본 XenotypeDef 파일들 (Biotech, Odyssey)
  - RimWorld 소스코드 (XenotypeDef.cs)

## XenotypeDef 기본 구조

XenotypeDef는 RimWorld의 Biotech DLC에서 도입된 제노타입(인공 종족)을 정의하는 클래스입니다. 기본 Def 클래스를 상속받아 게임 내 제노타입의 모든 특성을 정의합니다.

## 사용 가능한 모든 파라미터

### 1. 기본 Def 파라미터 (상속)
- `defName`: 제노타입의 고유 식별자
- `label`: 표시되는 이름
- `description`: 상세 설명
- `descriptionShort`: 짧은 설명

### 2. XenotypeDef 전용 파라미터

#### 2.1 유전자 관련
- **`genes`** (List<GeneDef>)
  - 제노타입이 가진 유전자 목록
  - 필수 파라미터
  - 예시: `<li>DarkVision</li>`, `<li>MeleeDamage_Strong</li>`

#### 2.2 상속 및 생성 관련
- **`inheritable`** (bool, 기본값: false)
  - 자식에게 유전될 수 있는지 여부
  - true: 상속 가능, false: 상속 불가

- **`factionlessGenerationWeight`** (float, 기본값: 1f)
  - 세력이 없는 폰 생성 시 가중치
  - 0: 절대 생성되지 않음, 높을수록 더 자주 생성

- **`canGenerateAsCombatant`** (bool, 기본값: true)
  - 전투원으로 생성될 수 있는지 여부
  - false: 전투원으로는 생성되지 않음 (예: Highmate)

#### 2.3 이름 생성 관련
- **`nameMaker`** (RulePackDef)
  - 남성용 이름 생성 규칙팩

- **`nameMakerFemale`** (RulePackDef)
  - 여성용 이름 생성 규칙팩 (선택사항)

- **`chanceToUseNameMaker`** (float, 범위: 0.0~1.0)
  - 이름 생성기를 사용할 확률
  - 0: 절대 사용 안함, 1: 항상 사용

#### 2.4 전투 관련
- **`combatPowerFactor`** (float, 기본값: 1f)
  - 전투력 배수
  - 1.0: 기본, 1.5: 강함, 0.8: 약함

- **`forbiddenWeaponClasses`** (List<WeaponClassDef>)
  - 사용 금지된 무기 클래스 목록
  - 예시: `<li>LongShots</li>` (Pigskin의 경우)

#### 2.5 제노저머 관련 (Sanguophage 전용)
- **`generateWithXenogermReplicatingHediffChance`** (float)
  - 제노저머 복제 헤딥과 함께 생성될 확률

- **`xenogermReplicatingDurationLeftDaysRange`** (FloatRange)
  - 제노저머 복제 지속 시간 범위
  - 예시: `<xenogermReplicatingDurationLeftDaysRange>0.1~140</xenogermReplicatingDurationLeftDaysRange>`

- **`soundDefOnImplant`** (SoundDef)
  - 제노저머 이식 시 재생될 사운드
  - 예시: `<soundDefOnImplant>PawnBecameSanguophage</soundDefOnImplant>`

#### 2.6 혼합 제노타입 관련
- **`doubleXenotypeChances`** (List<XenotypeChance>)
  - 다른 제노타입과 혼합될 확률
  - 구조: `<XenotypeName>확률값</XenotypeName>`
  - 예시: `<Pigskin>0.02</Pigskin>`

#### 2.7 UI 관련
- **`iconPath`** (string, [NoTranslate])
  - 제노타입 아이콘 경로
  - 필수 파라미터
  - 예시: `<iconPath>UI/Icons/Xenotypes/Dirtmole</iconPath>`

- **`displayPriority`** (float)
  - UI에서의 표시 우선순위
  - 높을수록 먼저 표시됨

## 파라미터 사용 예시 분석

### 1. Baseliner (기본 제노타입)
```xml
<XenotypeDef>
    <defName>Baseliner</defName>
    <label>baseliner</label>
    <description>A naturally-evolved human...</description>
    <iconPath>UI/Icons/Xenotypes/Baseliner</iconPath>
    <displayPriority>1000</displayPriority>
    <factionlessGenerationWeight>40</factionlessGenerationWeight>
</XenotypeDef>
```

### 2. Dirtmole (복잡한 제노타입)
```xml
<XenotypeDef>
    <defName>Dirtmole</defName>
    <label>dirtmole</label>
    <description>With gray skin adapted...</description>
    <descriptionShort>With gray skin adapted...</descriptionShort>
    <iconPath>UI/Icons/Xenotypes/Dirtmole</iconPath>
    <inheritable>true</inheritable>
    <nameMaker>NamerPersonDirtmole_Male</nameMaker>
    <nameMakerFemale>NamerPersonDirtmole_Female</nameMakerFemale>
    <chanceToUseNameMaker>1</chanceToUseNameMaker>
    <genes>
        <li>Eyes_Gray</li>
        <li>Skin_LightGray</li>
        <li>AptitudeRemarkable_Mining</li>
        <!-- ... 더 많은 유전자들 -->
    </genes>
</XenotypeDef>
```

### 3. Sanguophage (특수 제노타입)
```xml
<XenotypeDef>
    <defName>Sanguophage</defName>
    <!-- ... 기본 파라미터들 ... -->
    <soundDefOnImplant>PawnBecameSanguophage</soundDefOnImplant>
    <generateWithXenogermReplicatingHediffChance>0.5</generateWithXenogermReplicatingHediffChance>
    <xenogermReplicatingDurationLeftDaysRange>0.1~140</xenogermReplicatingDurationLeftDaysRange>
    <combatPowerFactor>2.5</combatPowerFactor>
    <displayPriority>-1000</displayPriority>
    <factionlessGenerationWeight>0</factionlessGenerationWeight>
    <doubleXenotypeChances>
        <Pigskin>0.02</Pigskin>
        <Impid>0.02</Impid>
        <!-- ... 다른 제노타입들 ... -->
    </doubleXenotypeChances>
    <genes>
        <!-- ... 많은 유전자들 ... -->
    </genes>
</XenotypeDef>
```

### 4. Pigskin (무기 제한 제노타입)
```xml
<XenotypeDef>
    <defName>Pigskin</defName>
    <!-- ... 기본 파라미터들 ... -->
    <inheritable>true</inheritable>
    <nameMaker>NamerPersonPigskin</nameMaker>
    <chanceToUseNameMaker>1</chanceToUseNameMaker>
    <genes>
        <!-- ... 유전자들 ... -->
    </genes>
    <forbiddenWeaponClasses>
        <li>LongShots</li>
    </forbiddenWeaponClasses>
</XenotypeDef>
```

### 5. Highmate (비전투 제노타입)
```xml
<XenotypeDef>
    <defName>Highmate</defName>
    <!-- ... 기본 파라미터들 ... -->
    <combatPowerFactor>0.8</combatPowerFactor>
    <canGenerateAsCombatant>false</canGenerateAsCombatant>
    <genes>
        <!-- ... 유전자들 ... -->
    </genes>
</XenotypeDef>
```

## 현재 프로젝트 (RK_XenoType_Ratkin) 분석

```xml
<XenotypeDef>
    <defName>RK_XenoType_Ratkin</defName>
    <label>ratkin</label>
    <description>기원을 알 수 없으나, </description>
    <descriptionShort>일반적인 랫킨.</descriptionShort>
    <iconPath>UI/Icon/RK_TextureIcon_XenotypeRatkin</iconPath>
    <inheritable>true</inheritable>
    <factionlessGenerationWeight>0</factionlessGenerationWeight>
    <genes>
        <li>DarkVision</li>
    </genes>
</XenotypeDef>
```

### 현재 상태
- **사용된 파라미터**: 7개 (기본 Def 4개 + XenotypeDef 3개)
- **미사용 파라미터**: 10개 이상
- **특이사항**: 
  - `factionlessGenerationWeight`가 0으로 설정되어 있어 세력 없는 폰으로는 생성되지 않음
  - `genes`에 DarkVision만 포함되어 있어 매우 기본적인 설정
  - 주석에 많은 계획된 유전자들이 있으나 실제로는 구현되지 않음

## 권장사항

### 1. 필수 파라미터 추가
- `displayPriority`: UI 표시 순서 설정
- `combatPowerFactor`: 전투력 설정 (랫킨 특성에 맞게)

### 2. 이름 생성 시스템 추가
- `nameMaker`: 랫킨 전용 이름 생성 규칙팩
- `chanceToUseNameMaker`: 이름 생성기 사용 확률

### 3. 유전자 시스템 확장
- 주석에 있는 계획된 유전자들을 실제로 구현
- 랫킨 특성에 맞는 유전자 조합 구성

### 4. 세력 연동 설정
- `factionlessGenerationWeight` 조정 (현재 0으로 설정됨)
- 필요시 `canGenerateAsCombatant` 설정

## 파라미터 우선순위

### 높은 우선순위 (즉시 구현 권장)
1. `genes` - 랫킨 특성 유전자 추가
2. `combatPowerFactor` - 전투력 설정
3. `displayPriority` - UI 표시 순서

### 중간 우선순위 (단계적 구현)
1. `nameMaker` - 이름 생성 시스템
2. `factionlessGenerationWeight` - 생성 가중치 조정
3. `description` - 상세 설명 완성

### 낮은 우선순위 (선택적 구현)
1. `forbiddenWeaponClasses` - 무기 제한 (필요시)
2. `doubleXenotypeChances` - 혼합 제노타입 (고급 기능)

## 결론

XenotypeDef는 총 17개의 파라미터를 제공하며, 현재 RK_XenoType_Ratkin은 7개만 사용하고 있습니다. 랫킨 제노타입을 완성하기 위해서는 특히 유전자 시스템과 전투력 설정이 우선적으로 필요하며, 이름 생성 시스템과 UI 표시 순서도 중요한 요소입니다.
