# 소방거품 이펙트 분석 및 적용 보고서

## 태그
Firefoam Effect Explosion StrawberryBeer Projectile

## 개요
RimWorld의 소방거품(Firefoam) 관련 이펙트 시스템을 분석하고, `RK_StrawberryBeer_Bullet`의 폭발 이펙트를 소방거품 이펙트로 변경했습니다.

## 소방거품 이펙트 시스템 분석

### 1. EffecterDef 분석

#### ExtinguisherExplosion
- **위치**: `RimworldData/Core/Defs/Effects/Effecter_Misc.xml`
- **용도**: 소방거품 폭발 시 시각적 이펙트
- **구성**:
  ```xml
  <EffecterDef>
    <defName>ExtinguisherExplosion</defName>
    <children>
      <li>
        <subEffecterClass>SubEffecter_SprayerTriggered</subEffecterClass>
        <positionRadius>0.2</positionRadius>
        <moteDef>Mote_ExtinguisherPuff</moteDef>
        <burstCount>10~15</burstCount>
        <speed>4.8~8.4</speed>
        <scale>4~5</scale>
      </li>
    </children>
  </EffecterDef>
  ```
- **특징**: 
  - 즉시 발동되는 스프레이 효과 (`SubEffecter_SprayerTriggered`)
  - 10~15개의 거품 파편 생성
  - 빠른 속도(4.8~8.4)와 큰 스케일(4~5)

#### ExtinguisherPuffSmall
- **위치**: `RimworldData/Core/Defs/Effects/Effecter_Misc.xml`
- **용도**: 작은 소방거품 분사 효과 (투사체 착지 시)
- **구성**:
  ```xml
  <EffecterDef>
    <defName>ExtinguisherPuffSmall</defName>
    <children>
      <li>
        <subEffecterClass>SubEffecter_SprayerTriggered</subEffecterClass>
        <positionRadius>0.2</positionRadius>
        <moteDef>Mote_ExtinguisherPuff</moteDef>
        <burstCount>5~10</burstCount>
        <speed>0.8~1.5</speed>
        <scale>0.5~0.8</scale>
      </li>
    </children>
  </EffecterDef>
  ```
- **특징**: 
  - ExtinguisherExplosion보다 작은 규모
  - 5~10개의 거품 파편
  - 느린 속도(0.8~1.5)와 작은 스케일(0.5~0.8)

#### FoamSpray_Directional
- **위치**: `RimworldData/Core/Defs/Effects/Effecter_Misc.xml`
- **용도**: 방향성 거품 분사 효과
- **구성**:
  ```xml
  <EffecterDef>
    <defName>FoamSpray_Directional</defName>
    <children>
      <li>
        <subEffecterClass>SubEffecter_SprayerTriggered</subEffecterClass>
        <positionRadius>0.15</positionRadius>
        <fleckDef>FoamSpray</fleckDef>
        <burstCount>2~4</burstCount>
        <speed>0.6~3.3</speed>
        <scale>0.8~1.25</scale>
        <angle>-24~24</angle>
        <positionLerpFactor>0.85</positionLerpFactor>
        <fleckUsesAngleForVelocity>true</fleckUsesAngleForVelocity>
      </li>
    </children>
  </EffecterDef>
  ```
- **특징**: 
  - 방향성 분사 효과
  - 각도 제한(-24~24도)
  - 속도와 각도를 사용한 물리 시뮬레이션

### 2. 원본 RimWorld 소방거품 사용 예시

#### FirefoamPopper (건물)
- **위치**: `RimworldData/Core/Defs/ThingDefs_Buildings/Buildings_Misc.xml`
- **설정**:
  ```xml
  <compProperties_Explosive>
    <explosiveRadius>9.9</explosiveRadius>
    <explosiveDamageType>Extinguish</explosiveDamageType>
    <postExplosionSpawnThingDef>Filth_FireFoam</postExplosionSpawnThingDef>
    <postExplosionSpawnChance>1</postExplosionSpawnChance>
    <postExplosionSpawnThingCount>1</postExplosionSpawnThingCount>
    <explosionEffect>ExtinguisherExplosion</explosionEffect>
    <explosionSound>Explosion_FirefoamPopper</explosionSound>
  </compProperties_Explosive>
  ```

#### Bullet_Shell_Firefoam (투사체)
- **위치**: `RimworldData/Core/Defs/ThingDefs_Items/Items_Resource_Shell.xml`
- **설정**:
  ```xml
  <projectile>
    <damageDef>Extinguish</damageDef>
    <explosionRadius>5</explosionRadius>
    <soundExplode>Explosion_EMP</soundExplode>
    <postExplosionSpawnThingDef>Filth_FireFoam</postExplosionSpawnThingDef>
    <postExplosionSpawnChance>1</postExplosionSpawnChance>
    <postExplosionSpawnThingCount>3</postExplosionSpawnThingCount>
    <explosionEffect>ExtinguisherExplosion</explosionEffect>
  </projectile>
  ```

#### Bullet_FoamSprayer (소방거품 터렛 투사체)
- **위치**: `RimworldData/Core/Defs/ThingDefs_Buildings/Buildings_Security_Turrets.xml`
- **설정**:
  ```xml
  <projectile>
    <filth>Filth_FireFoam</filth>
    <filthCount>1</filthCount>
    <landedEffecter>ExtinguisherPuffSmall</landedEffecter>
    <soundImpact>Foam_Impact</soundImpact>
  </projectile>
  ```

### 3. 소방거품 관련 리소스

#### Filth_FireFoam
- **타입**: FilthDef (더러움)
- **용도**: 폭발 후 지면에 남는 소방거품 더러움
- **특징**: 화재 진압 효과

#### SoundDef
- **Explosion_FirefoamPopper**: 소방거품 폭발 사운드
- **Foam_Impact**: 소방거품 착지 사운드

#### MoteDef
- **Mote_ExtinguisherPuff**: 소방거품 파편 모트

## 적용 내용

### 변경 전 설정
```xml
<projectile>
  <soundExplode>Explosion_FirefoamPopper</soundExplode>
  <preExplosionSpawnThingDef>Filth_SpentAcid</preExplosionSpawnThingDef>
  <preExplosionSpawnChance>0</preExplosionSpawnChance>
</projectile>
```

### 변경 후 설정
```xml
<projectile>
  <soundExplode>Explosion_FirefoamPopper</soundExplode>
  <postExplosionSpawnThingDef>Filth_FireFoam</postExplosionSpawnThingDef>
  <postExplosionSpawnChance>1</postExplosionSpawnChance>
  <postExplosionSpawnThingCount>1</postExplosionSpawnThingCount>
  <explosionEffect>ExtinguisherExplosion</explosionEffect>
</projectile>
```

### 변경 사항 요약

1. **explosionEffect 추가**: `ExtinguisherExplosion` - 소방거품 폭발 시각 효과
2. **preExplosionSpawnThingDef 제거**: `Filth_SpentAcid` 제거 (산성 더러움)
3. **postExplosionSpawnThingDef 추가**: `Filth_FireFoam` - 소방거품 더러움 생성
4. **postExplosionSpawnChance 설정**: 1 (100% 확률로 생성)
5. **postExplosionSpawnThingCount 설정**: 1 (1개 생성)

## 효과

### 시각적 효과
- 폭발 시 `ExtinguisherExplosion` 이펙트가 발동되어 10~15개의 거품 파편이 생성됨
- 거품 파편이 빠른 속도(4.8~8.4)로 분산됨
- 큰 스케일(4~5)로 시각적 임팩트 제공

### 게임플레이 효과
- 폭발 지점에 `Filth_FireFoam` 더러움이 생성됨
- 소방거품 더러움은 화재 진압 효과를 가짐
- 기존 산성 더러움 대신 소방거품 더러움으로 변경

### 사운드 효과
- `Explosion_FirefoamPopper` 사운드가 재생됨 (이미 적용되어 있었음)

## 참고 사항

### preExplosion vs postExplosion
- **preExplosionSpawnThingDef**: 폭발 전에 생성되는 더러움 (현재 사용 안 함)
- **postExplosionSpawnThingDef**: 폭발 후에 생성되는 더러움 (소방거품에 적합)

### 원본 RimWorld 패턴
- 대부분의 소방거품 관련 투사체는 `postExplosionSpawnThingDef`를 사용
- `preExplosionSpawnThingDef`는 주로 산성/독성 투사체에서 사용

## 결론

`RK_StrawberryBeer_Bullet`의 폭발 이펙트를 소방거품 이펙트로 성공적으로 변경했습니다. 이제 맥주병이 폭발할 때 소방거품 이펙트와 더러움이 생성되어, 화재 진압 효과를 가진 소방거품 더러움이 남게 됩니다.

