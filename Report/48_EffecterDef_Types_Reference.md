# EffecterDef 종류 참조 가이드

## 개요
RimWorld에서 사용되는 EffecterDef의 주요 종류들을 카테고리별로 정리한 참조 가이드입니다.

## EffecterDef 기본 구조

```xml
<EffecterDef>
    <defName>EffecterName</defName>
    <maintainTicks>60</maintainTicks>  <!-- 유지 시간 (선택) -->
    <children>
        <li>
            <subEffecterClass>SubEffecter_XXX</subEffecterClass>
            <!-- 서브 이펙터 설정 -->
        </li>
    </children>
</EffecterDef>
```

## 주요 SubEffecter 클래스

1. **SubEffecter_SprayerTriggered**: 즉시 발동되는 스프레이 효과
2. **SubEffecter_SprayerTriggeredDelayed**: 지연 후 발동되는 스프레이 효과
3. **SubEffecter_SprayerContinuous**: 지속적으로 생성되는 스프레이 효과
4. **SubEffecter_SprayerChance**: 확률적으로 생성되는 스프레이 효과
5. **SubEffecter_SoundTriggered**: 즉시 재생되는 사운드
6. **SubEffecter_Sustainer**: 지속 재생되는 사운드
7. **SubEffecter_SoundIntermittent**: 간헐적으로 재생되는 사운드
8. **SubEffecter_InteractSymbol**: 상호작용 심볼
9. **SubEffecter_CameraShakeSingle**: 카메라 흔들림
10. **SubEffecter_DrifterEmoteChance**: 표정 이모트

## 카테고리별 EffecterDef 목록

### 1. 무기/전투 관련

#### 데미지/타격 효과
- **Damage_HitFlesh**: 살점 타격 효과 (피, 공기 분산)
- **Damage_HitInsect**: 곤충 타격 효과 (노란색 피)
- **Damage_HitMechanoid**: 기계족 타격 효과 (전기 스파크, 파란색)
- **DamageDiminished_Metal**: 금속 감소 데미지 (스파크)
- **DamageDiminished_General**: 일반 감소 데미지 (스파크)
- **Impact_Toxic**: 독성 타격 효과

#### 방어/반격 효과
- **Deflect_Metal**: 금속 방어 효과 (스파크, 공기 분산)
- **Deflect_Metal_Bullet**: 금속 총알 방어 효과
- **Deflect_General**: 일반 방어 효과 (노란색 스파크)
- **Deflect_General_Bullet**: 일반 총알 방어 효과
- **Shield_Break**: 실드 파괴 효과
- **Interceptor_BlockedProjectile**: 투사체 차단 효과

#### 폭발/화염 관련
- **GiantExplosion**: 거대 폭발 효과
- **ExtinguisherExplosion**: 소화기 폭발 효과
- **ExtinguisherPuffSmall**: 작은 소화기 분사 효과
- **BurnerUsed**: 버너 사용 효과 (연기, 불꽃) - **Anomaly DLC**
- **Fire_SpewBioferrite**: 바이오페라이트 화염 분사 - **Anomaly DLC**

### 2. 건물/구조물 관련

#### 전력/에너지
- **Power_Cell_Burning**: 전력 셀 연소 효과
- **Power_Cell_Sparks**: 전력 셀 스파크 효과
- **DisabledByEMP**: EMP로 인한 비활성화 효과
- **DisabledByEMPLarge**: 대형 EMP 비활성화 효과

#### 방어 구조물
- **BulletShieldGenerator_Reactivate**: 총알 방어막 재활성화
- **MortarShieldGenerator_Reactivate**: 박격포 방어막 재활성화

#### 구조물 파괴
- **Bridge_Collapse**: 다리 붕괴 효과
- **Bridge_CollapseWater**: 물 위 다리 붕괴 효과
- **RaisedRock_Collapse**: 바위 붕괴 효과

### 3. 특수 능력/이동 관련

#### 텔레포트/이동
- **Skip_Entry**: 텔레포트 진입 효과
- **Skip_EntryNoDelay**: 지연 없는 텔레포트 진입
- **Skip_Exit**: 텔레포트 탈출 효과
- **Skip_ExitNoDelay**: 지연 없는 텔레포트 탈출

#### 등장/출현
- **EmergencePointSustained8X8**: 8x8 지속 등장 효과
- **EmergencePointComplete8X8**: 8x8 완전 등장 효과
- **EmergencePointSustained3X3**: 3x3 지속 등장 효과
- **EmergencePointComplete3X3**: 3x3 완전 등장 효과
- **EmergencePointSustained2X2**: 2x2 지속 등장 효과
- **EmergencePointComplete2X2**: 2x2 완전 등장 효과

#### 물 관련
- **PawnEmergeFromWater**: 물에서 등장 효과
- **PawnEmergeFromWaterLarge**: 대형 물 등장 효과
- **WaterMist**: 물 안개 효과

### 4. 환경/상태 효과

#### 가스/독성
- **ToxGasReleasing**: 독가스 방출 효과
- **Vaporize_Heatwave**: 열파 증발 효과

#### 화재/연소
- **Power_Cell_Burning**: 전력 셀 연소 (위 참조)
- **Power_Cell_Sparks**: 전력 셀 스파크 (위 참조)

### 5. 상호작용/작업 관련

#### 작업 효과
- **PlayPoker**: 포커 플레이 효과
- **WatchingTelevision**: TV 시청 효과
- **FillingInCrater**: 크레이터 매우기 효과
- **HackingTerminal**: 해킹 터미널 효과

#### 상호작용
- **HermeticCrateOpened**: 밀폐 상자 개봉 효과
- **ForcedVisible**: 강제 가시화 효과

### 6. 특수 상태 효과

#### 폰 상태
- **Drunk**: 술 취함 효과
- **Berserk**: 광란 효과
- **Vomit**: 구토 효과

#### 특수 객체
- **CocoonDestroyed**: 고치 파괴 효과
- **CocoonWakingUp**: 고치 깨어남 효과

### 7. 분사/스프레이 효과

- **AcidSpray_Directional**: 산성 분사 (방향성)
- **FoamSpray_Directional**: 거품 분사 (방향성)

### 8. 기타

- **ImpactSmallDustCloud**: 작은 먼지 구름 충격
- **UndercaveMapExitLightshafts**: 지하 동굴 출구 광선

## Anomaly DLC 추가 EffecterDef

### 무기/능력 관련
- **BurnerUsed**: 버너 사용 효과 (HellcatBurner, IncineratorBurner에서 사용)
  - 연기, 불꽃 파편 효과
  - `CompProperties_AbilityBurner`의 `effecterDef`로 사용

### 폭발 관련
- **MeatExplosionTiny**: 작은 고기 폭발
- **MeatExplosionSmall**: 작은 고기 폭발
- **MeatExplosion**: 고기 폭발
- **MeatExplosionLarge**: 큰 고기 폭발
- **MeatExplosionExtraLarge**: 초대형 고기 폭발
- **AgonyPulseExplosion**: 고통 펄스 폭발
- **ObeliskExplosionWarmup**: 오벨리스크 폭발 워밍업

## 사용 예시

### 무기 능력에서 사용 (HellcatBurner)
```xml
<AbilityDef>
    <comps>
        <li Class="CompProperties_AbilityBurner">
            <effecterDef>BurnerUsed</effecterDef>
            <!-- 기타 설정 -->
        </li>
    </comps>
</AbilityDef>
```

### 건물에서 사용
```xml
<ThingDef>
    <comps>
        <li Class="CompProperties_Effecter">
            <effecterDef>Power_Cell_Burning</effecterDef>
        </li>
    </comps>
</ThingDef>
```

### 데미지 효과에서 사용
```xml
<DamageDef>
    <effecterDef>Damage_HitFlesh</effecterDef>
</DamageDef>
```

## 참고 파일 위치

- **Core**: `RimworldData/Core/Defs/Effects/`
  - `Effecter_Misc.xml`
  - `Effecter_Damage.xml`
  - `Effecter_PawnStates.xml`
  - `Effecter_Construction.xml`
  - `Effecter_Ingest.xml`
  - `Effecter_Recipes.xml`
  - `Effecter_WorkGeneral.xml`

- **Anomaly DLC**: `RimworldData/Anomaly/Defs/Effects/`
  - `Effecter_Misc.xml` (추가 effecterDef 포함)

## 주요 특징

1. **maintainTicks**: 효과가 유지되는 시간 (틱 단위)
2. **children**: 여러 서브 이펙터를 조합하여 복합 효과 생성 가능
3. **spawnLocType**: 효과 생성 위치
   - `OnSource`: 소스 위치
   - `OnTarget`: 타겟 위치
   - `BetweenPositions`: 중간 위치
   - `RandomCellOnTarget`: 타겟의 랜덤 셀

## 건랜스 관련 권장 EffecterDef

현재 프로젝트에서 사용 중인 `BurnerUsed` 외에도 다음을 고려할 수 있습니다:

- **GiantExplosion**: 강력한 폭발 효과 (용격포용)
- **Damage_HitFlesh**: 타격 효과 (일반 포격용)
- **Power_Cell_Sparks**: 스파크 효과 (과열/충전용)
- **ExtinguisherExplosion**: 폭발 효과 (발사용)







