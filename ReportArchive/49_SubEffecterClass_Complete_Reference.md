# SubEffecterClass 종합 참조 가이드

## 개요

RimWorld에서 `EffecterDef`의 `children` 요소에 사용되는 `subEffecterClass`의 모든 종류와 기능을 정리한 참조 가이드입니다.

## SubEffecter 기본 구조

```xml
<EffecterDef>
    <defName>EffecterName</defName>
    <children>
        <li>
            <subEffecterClass>SubEffecter_XXX</subEffecterClass>
            <!-- 서브 이펙터별 설정 -->
        </li>
    </children>
</EffecterDef>
```

## SubEffecter 클래스 계층 구조

```
SubEffecter (기본 클래스)
├── SubEffecter_Sprayer (추상 클래스)
│   ├── SubEffecter_SprayerTriggered
│   │   └── SubEffecter_SprayerTriggeredDelayed
│   ├── SubEffecter_SprayerTriggeredChance
│   ├── SubEffecter_SprayerContinuous
│   └── SubEffecter_SprayerChance
├── SubEffecter_CameraShake (추상 클래스)
│   ├── SubEffecter_CameraShakeSingle
│   └── SubEffecter_CameraShakeChance
├── SubEffecter_DrifterEmote (추상 클래스)
│   ├── SubEffecter_DrifterEmoteTriggered
│   ├── SubEffecter_DrifterEmoteChance
│   └── SubEffecter_DrifterEmoteContinuous
├── SubEffecter_SoundTriggered
├── SubEffecter_Sustainer
├── SubEffecter_SoundIntermittent
├── SubEffecter_InteractSymbol
├── SubEffecter_ProgressBar
├── SubEffecter_Random
└── SubEffecter_GroupedChance
```

## 주요 SubEffecter 클래스 상세 설명

### 1. 스프레이 계열 (Sprayer)

#### SubEffecter_Sprayer (추상 클래스)
- **기본 클래스**: 모든 스프레이 효과의 기본
- **기능**: Mote/Fleck 생성 및 위치 계산
- **직접 사용 불가**: 하위 클래스 사용

#### SubEffecter_SprayerTriggered
- **설명**: 즉시 발동되는 스프레이 효과
- **동작**: `SubTrigger()` 호출 시 즉시 Mote/Fleck 생성
- **사용 예시**: 타격 시 즉시 피 분산, 폭발 시 즉시 파편 분산
- **주요 속성**:
  - `fleckDef` / `moteDef`: 생성할 Fleck/Mote 정의
  - `burstCount`: 한 번에 생성할 개수
  - `spawnLocType`: 생성 위치 (OnSource, OnTarget, BetweenPositions 등)

#### SubEffecter_SprayerTriggeredDelayed
- **설명**: 지연 후 발동되는 스프레이 효과
- **동작**: `SubTrigger()` 호출 시 `initialDelayTicks`만큼 대기 후 생성
- **사용 예시**: 발사 후 약간의 지연을 두고 연기/불꽃 생성
- **주요 속성**:
  - `initialDelayTicks`: 지연 시간 (틱 단위)
  - 기타 속성은 `SubEffecter_SprayerTriggered`와 동일
- **현재 프로젝트 사용**: `RK_Effecter_BurnerUsed`에서 사용 중

#### SubEffecter_SprayerTriggeredChance
- **설명**: 확률적으로 발동되는 스프레이 효과
- **동작**: `SubTrigger()` 호출 시 `chancePerTick` 확률로 생성
- **사용 예시**: 특정 확률로만 발생하는 특수 효과
- **주요 속성**:
  - `chancePerTick`: 발동 확률 (0.0 ~ 1.0)

#### SubEffecter_SprayerContinuous
- **설명**: 지속적으로 생성되는 스프레이 효과
- **동작**: `SubEffectTick()`에서 일정 간격으로 계속 생성
- **사용 예시**: 지속적인 연기, 지속적인 불꽃
- **주요 속성**:
  - `initialDelayTicks`: 초기 지연 시간
  - `ticksBetweenMotes`: 생성 간격 (틱 단위)
  - `maxMoteCount`: 최대 생성 개수

#### SubEffecter_SprayerChance
- **설명**: 확률적으로 지속 생성되는 스프레이 효과
- **동작**: `SubEffectTick()`에서 확률적으로 생성
- **사용 예시**: 간헐적인 스파크, 간헐적인 연기
- **주요 속성**:
  - `chancePerTick`: 틱당 확률
  - `chancePeriodTicks`: 확률 체크 주기
  - `lifespanMaxTicks`: 최대 지속 시간
  - `initialDelayTicks`: 초기 지연 시간

### 2. 사운드 계열 (Sound)

#### SubEffecter_SoundTriggered
- **설명**: 즉시 재생되는 사운드
- **동작**: `SubTrigger()` 호출 시 즉시 사운드 재생
- **사용 예시**: 타격 사운드, 폭발 사운드
- **주요 속성**:
  - `soundDef`: 재생할 사운드 정의

#### SubEffecter_Sustainer
- **설명**: 지속 재생되는 사운드 (루프)
- **동작**: `SubEffectTick()`에서 지속적으로 사운드 유지
- **사용 예시**: 지속적인 기계음, 지속적인 화재음
- **주요 속성**:
  - `soundDef`: 재생할 사운드 정의 (Sustainer 타입)
  - `ticksBeforeSustainerStart`: 시작 전 지연 시간

#### SubEffecter_SoundIntermittent
- **설명**: 간헐적으로 재생되는 사운드
- **동작**: `SubEffectTick()`에서 일정 간격으로 재생
- **사용 예시**: 간헐적인 경고음, 간헐적인 기계음
- **주요 속성**:
  - `soundDef`: 재생할 사운드 정의
  - `intermittentSoundInterval`: 재생 간격 (틱 단위)

### 3. 카메라 흔들림 계열 (CameraShake)

#### SubEffecter_CameraShake (추상 클래스)
- **기본 클래스**: 카메라 흔들림 효과의 기본
- **직접 사용 불가**: 하위 클래스 사용

#### SubEffecter_CameraShakeSingle
- **설명**: 한 번만 발생하는 카메라 흔들림
- **동작**: `SubTrigger()` 호출 시 즉시 카메라 흔들림
- **사용 예시**: 폭발 시 카메라 흔들림, 강한 타격 시 흔들림
- **주요 속성**:
  - `cameraShake`: 흔들림 강도 범위
  - `distanceAttenuationMax`: 거리 감쇠 최대 거리
  - `distanceAttenuationScale`: 거리 감쇠 스케일
  - `soundDef`: (선택) 동시 재생할 사운드

#### SubEffecter_CameraShakeChance
- **설명**: 확률적으로 발생하는 카메라 흔들림
- **동작**: `SubTrigger()` 호출 시 확률적으로 흔들림
- **사용 예시**: 확률적 지진 효과

### 4. 드리프터 이모트 계열 (DrifterEmote)

#### SubEffecter_DrifterEmote (추상 클래스)
- **기본 클래스**: 드리프터 이모트 효과의 기본
- **직접 사용 불가**: 하위 클래스 사용

#### SubEffecter_DrifterEmoteTriggered
- **설명**: 즉시 발동되는 이모트
- **동작**: `SubTrigger()` 호출 시 즉시 이모트 생성
- **사용 예시**: 특정 행동 시 표정 이모트

#### SubEffecter_DrifterEmoteChance
- **설명**: 확률적으로 발생하는 이모트
- **동작**: `SubEffectTick()`에서 확률적으로 생성
- **사용 예시**: 랜덤 표정 이모트

#### SubEffecter_DrifterEmoteContinuous
- **설명**: 지속적으로 생성되는 이모트
- **동작**: `SubEffectTick()`에서 지속적으로 생성
- **사용 예시**: 지속적인 표정 이모트

### 5. 상호작용 계열

#### SubEffecter_InteractSymbol
- **설명**: 상호작용 심볼 표시
- **동작**: `SubEffectTick()`에서 상호작용 오버레이 Mote 유지
- **사용 예시**: 작업 중 상호작용 심볼 표시
- **주요 속성**:
  - `moteDef`: 표시할 Mote 정의

#### SubEffecter_ProgressBar
- **설명**: 진행 바 표시
- **동작**: `SubEffectTick()`에서 진행 바 Mote 유지
- **사용 예시**: 작업 진행률 표시
- **주요 속성**:
  - `moteDef`: 진행 바 Mote 정의 (MoteProgressBar 타입)

### 6. 특수 계열

#### SubEffecter_Random
- **설명**: 랜덤으로 자식 SubEffecter 선택
- **동작**: 초기화 시 `children` 중 하나를 랜덤 선택하여 사용
- **사용 예시**: 여러 효과 중 하나를 랜덤으로 선택
- **주요 속성**:
  - `children`: 선택 가능한 SubEffecterDef 목록
  - `randomWeight`: 각 자식의 가중치

#### SubEffecter_GroupedChance
- **설명**: 확률적으로 여러 자식 SubEffecter를 동시에 발동
- **동작**: `SubEffectTick()`에서 확률적으로 모든 자식을 `SubTrigger()` 호출
- **사용 예시**: 확률적으로 복합 효과 발동
- **주요 속성**:
  - `children`: 발동할 SubEffecterDef 목록
  - `chancePerTick`: 발동 확률
  - `chancePeriodTicks`: 확률 체크 주기
  - `lifespanMaxTicks`: 최대 지속 시간
  - `initialDelayTicks`: 초기 지연 시간
  - `subTriggerOnSpawn`: 스폰 시 즉시 발동 여부

## 공통 속성 (모든 SubEffecter)

### 위치 관련
- `spawnLocType`: 생성 위치 타입
  - `OnSource`: 소스 위치
  - `OnTarget`: 타겟 위치
  - `BetweenPositions`: 중간 위치
  - `BetweenTouchingCells`: 접촉 셀 사이
  - `RandomCellOnTarget`: 타겟의 랜덤 셀
  - `RandomDrawPosOnTarget`: 타겟의 랜덤 드로우 위치

### 시각 효과 관련
- `moteDef`: 생성할 Mote 정의
- `fleckDef`: 생성할 Fleck 정의
- `color`: 색상 (또는 `colorOverride`)
- `scale`: 크기 범위
- `rotation`: 회전 각도 범위
- `rotationRate`: 회전 속도 범위
- `speed`: 속도 범위
- `angle`: 각도 범위
- `airTime`: 공중 체류 시간 범위

### 위치 오프셋 관련
- `positionOffset`: 위치 오프셋
- `positionRadius`: 위치 반경
- `positionRadiusMin`: 최소 위치 반경
- `positionDimensions`: 위치 차원
- `positionLerpFactor`: 위치 보간 계수 (BetweenPositions용)

### 기타
- `burstCount`: 한 번에 생성할 개수 범위
- `attachPoint`: 부착 지점 (AttachPointType)
- `attachToSpawnThing`: 스폰 대상에 부착 여부
- `absoluteAngle`: 절대 각도 사용 여부
- `useTargetAInitialRotation`: 타겟 A의 초기 회전 사용
- `useTargetBInitialRotation`: 타겟 B의 초기 회전 사용
- `useTargetABodyAngle`: 타겟 A의 몸체 각도 사용
- `useTargetBBodyAngle`: 타겟 B의 몸체 각도 사용
- `rotateTowardsTargetCenter`: 타겟 중심을 향해 회전
- `fleckUsesAngleForVelocity`: Fleck 속도에 각도 사용
- `orbitOrigin`: 궤도 원점 사용
- `orbitSpeed`: 궤도 속도 범위
- `orbitSnapStrength`: 궤도 스냅 강도

## 사용 예시

### 예시 1: 즉시 발동 스프레이 (타격 효과)
```xml
<EffecterDef>
    <defName>Damage_HitFlesh</defName>
    <children>
        <li>
            <subEffecterClass>SubEffecter_SprayerTriggered</subEffecterClass>
            <fleckDef>Fleck_Blood</fleckDef>
            <burstCount>3~6</burstCount>
            <spawnLocType>OnTarget</spawnLocType>
            <scale>0.5~1.0</scale>
            <speed>0.5~2.0</speed>
        </li>
    </children>
</EffecterDef>
```

### 예시 2: 지연 발동 스프레이 (발사 효과)
```xml
<EffecterDef>
    <defName>RK_Effecter_BurnerUsed</defName>
    <children>
        <li>
            <subEffecterClass>SubEffecter_SprayerTriggeredDelayed</subEffecterClass>
            <fleckDef>Fleck_BurnerUsedSmoke</fleckDef>
            <initialDelayTicks>5</initialDelayTicks>
            <burstCount>7~10</burstCount>
            <spawnLocType>OnSource</spawnLocType>
            <scale>0.1~1.0</scale>
        </li>
    </children>
</EffecterDef>
```

### 예시 3: 지속 스프레이 (연기 효과)
```xml
<EffecterDef>
    <defName>Smoke_Continuous</defName>
    <maintainTicks>300</maintainTicks>
    <children>
        <li>
            <subEffecterClass>SubEffecter_SprayerContinuous</subEffecterClass>
            <fleckDef>Fleck_Smoke</fleckDef>
            <initialDelayTicks>10</initialDelayTicks>
            <ticksBetweenMotes>15</ticksBetweenMotes>
            <maxMoteCount>20</maxMoteCount>
            <spawnLocType>OnSource</spawnLocType>
        </li>
    </children>
</EffecterDef>
```

### 예시 4: 사운드 + 스프레이 조합
```xml
<EffecterDef>
    <defName>Explosion_Effect</defName>
    <children>
        <li>
            <subEffecterClass>SubEffecter_SoundTriggered</subEffecterClass>
            <soundDef>Explosion</soundDef>
        </li>
        <li>
            <subEffecterClass>SubEffecter_SprayerTriggered</subEffecterClass>
            <fleckDef>Fleck_Explosion</fleckDef>
            <burstCount>20~30</burstCount>
            <spawnLocType>OnSource</spawnLocType>
        </li>
        <li>
            <subEffecterClass>SubEffecter_CameraShakeSingle</subEffecterClass>
            <cameraShake>0.5~1.0</cameraShake>
        </li>
    </children>
</EffecterDef>
```

### 예시 5: 확률적 복합 효과
```xml
<EffecterDef>
    <defName>Random_Spark_Effect</defName>
    <maintainTicks>180</maintainTicks>
    <children>
        <li>
            <subEffecterClass>SubEffecter_GroupedChance</subEffecterClass>
            <chancePerTick>0.05</chancePerTick>
            <chancePeriodTicks>10</chancePeriodTicks>
            <lifespanMaxTicks>180</lifespanMaxTicks>
            <children>
                <li>
                    <subEffecterClass>SubEffecter_SprayerTriggered</subEffecterClass>
                    <fleckDef>Fleck_Spark</fleckDef>
                    <burstCount>1~3</burstCount>
                </li>
                <li>
                    <subEffecterClass>SubEffecter_SoundTriggered</subEffecterClass>
                    <soundDef>Spark</soundDef>
                </li>
            </children>
        </li>
    </children>
</EffecterDef>
```

## 선택 가이드

### 즉시 효과가 필요한 경우
- `SubEffecter_SprayerTriggered`: 즉시 스프레이
- `SubEffecter_SoundTriggered`: 즉시 사운드
- `SubEffecter_CameraShakeSingle`: 즉시 카메라 흔들림

### 지연 효과가 필요한 경우
- `SubEffecter_SprayerTriggeredDelayed`: 지연 스프레이

### 지속 효과가 필요한 경우
- `SubEffecter_SprayerContinuous`: 지속 스프레이
- `SubEffecter_Sustainer`: 지속 사운드
- `SubEffecter_DrifterEmoteContinuous`: 지속 이모트

### 확률적 효과가 필요한 경우
- `SubEffecter_SprayerTriggeredChance`: 확률적 즉시 스프레이
- `SubEffecter_SprayerChance`: 확률적 지속 스프레이
- `SubEffecter_SoundIntermittent`: 간헐적 사운드
- `SubEffecter_DrifterEmoteChance`: 확률적 이모트
- `SubEffecter_GroupedChance`: 확률적 복합 효과

### 랜덤 선택이 필요한 경우
- `SubEffecter_Random`: 여러 효과 중 랜덤 선택

### UI 표시가 필요한 경우
- `SubEffecter_InteractSymbol`: 상호작용 심볼
- `SubEffecter_ProgressBar`: 진행 바

## 참고 파일 위치

- **소스 코드**: `RimworldSource/Verse/SubEffecter*.cs`
- **기본 클래스**: `RimworldSource/Verse/SubEffecter.cs`
- **스프레이 계열**: `RimworldSource/Verse/SubEffecter_Sprayer*.cs`
- **사운드 계열**: `RimworldSource/Verse/SubEffecter_Sound*.cs`
- **카메라 계열**: `RimworldSource/Verse/SubEffecter_CameraShake*.cs`

## 현재 프로젝트 사용 예시

### Weapon_HighTech.xml의 RK_Effecter_BurnerUsed
```xml
<EffecterDef>
    <defName>RK_Effecter_BurnerUsed</defName>
    <children>
        <li>
            <subEffecterClass>SubEffecter_SprayerTriggeredDelayed</subEffecterClass>
            <fleckDef>Fleck_BurnerUsedSmoke</fleckDef>
            <initialDelayTicks>5</initialDelayTicks>
            <scale>0.1~1.0</scale>
            <burstCount>7~10</burstCount>
            <spawnLocType>OnSource</spawnLocType>
            <absoluteAngle>True</absoluteAngle>
            <positionRadius>2.5</positionRadius>
            <positionRadiusMin>0</positionRadiusMin>
            <angle>0</angle>
            <speed>0~0.6</speed>
            <rotation>0~360</rotation>
            <fleckUsesAngleForVelocity>True</fleckUsesAngleForVelocity>
        </li>
        <!-- 추가 효과들... -->
    </children>
</EffecterDef>
```

## 요약

총 **17개의 SubEffecter 클래스**가 있으며, 크게 다음과 같이 분류됩니다:

1. **스프레이 계열** (5개): 시각 효과 생성
2. **사운드 계열** (3개): 오디오 효과
3. **카메라 계열** (2개): 카메라 흔들림
4. **이모트 계열** (3개): 표정 이모트
5. **UI 계열** (2개): 상호작용 심볼, 진행 바
6. **특수 계열** (2개): 랜덤 선택, 복합 효과

각 클래스는 특정 상황에 최적화되어 있으므로, 원하는 효과에 맞는 클래스를 선택하여 사용하면 됩니다.









