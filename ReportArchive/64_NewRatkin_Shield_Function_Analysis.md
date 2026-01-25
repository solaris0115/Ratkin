# NewRatkin.Shield 기능 분석 보고서

## 개요

`NewRatkin.Shield`는 RimWorld의 `Apparel` 클래스를 상속받아 구현된 방패 장비 시스템입니다. 방패 착용 시 공격을 방어하는 메커니즘과 무기 호환성 제한 기능을 제공합니다.

## 주요 구성 요소

### 1. Shield 클래스 (ApparelShield.cs)
- **기본 클래스**: `Apparel`
- **주요 기능**: 
  - 공격 방어 메커니즘 (`CheckPreAbsorbDamage`)
  - 원거리 무기 발사 차단 (`AllowVerbCast`)
  - 방어 확률 통계 표시 (`SpecialDisplayStats`)

### 2. CompShieldWeaponIncompatible 컴포넌트
- **기능**: 방패와 특정 무기의 동시 착용 제한
- **동작 방식**: 화이트리스트 기반 무기 태그 검증

### 3. ShieldPatch (Harmony Patch)
- **기능**: `ThingDef.IsShieldThatBlocksRanged` 속성 패치
- **목적**: `CompRKShield`를 가진 방패를 원거리 무기 차단 방패로 인식

## 핵심 기능 플로우

### 1. 공격 방어 메커니즘 (CheckPreAbsorbDamage)

#### 실행 조건
- 착용자가 살아있고(`!pawn.Dead`)
- 기절하지 않은 상태(`!pawn.Downed`)

#### 방어 각도 계산
1. **공격자 각도 계산**
   - `attackerAngle = dinfo.Angle + 180`
   - 360도 이상이면 -360 조정

2. **방어자 각도**
   - `defenderAngle = pawn.Rotation.AsAngle` (착용자가 바라보는 방향)

3. **방어 범위 검증**
   - 방어 가능 범위: 착용자 시야 기준 ±70도 (총 140도)
   - 조건: `defenderAngle - attackerAngle >= -70 && defenderAngle - attackerAngle <= 70`
   - 범위 밖 공격은 방어 불가

#### 방어 확률 계산

**1단계: 스킬 기반 방어 확률**
- 공식: `blockRateBySkill = MeleeSkillLevel × 0.02`
- 최대값: 스킬 20레벨 기준 40%

**2단계: 소재 기반 방어 확률**
- 공격 타입별 방어력 확인:
  - `Sharp` → `ArmorRating_Sharp`
  - `Blunt` → `ArmorRating_Blunt`
  - `Heat` → `ArmorRating_Heat`
- 소재 방어력 제한 공식: `Clamp01(armorRate / 2) / 2`
  - 최대 50%로 제한 (괴물 소재 대비)
  - 예: 방어력 200% → 50%로 제한

**3단계: 최종 방어 확률**
- `totalDeflectChance = blockRateBySkill + clampedBlockRateFromStuff`
- 랜덤 값(`Rand.Value`)과 비교하여 방어 성공 여부 결정

#### 방어 성공 시 처리
1. 개발 모드에서 로그 출력 (방어 확률 표시)
2. 텍스트 모트 생성: "ShieldBlock" 번역 키
3. 이펙트 생성: `EffecterDefOf.Deflect_Metal`
4. `true` 반환하여 데미지 완전 차단

### 2. 원거리 무기 발사 차단 (AllowVerbCast)

#### 동작 방식
- 모든 `Verb_LaunchProjectile` 타입의 Verb 차단
- `false` 반환 시 무기 발사 불가

#### 호출 체인
```
Verb.TryCastShot() / Verb.CanHitTarget()
  └─> Verb.FirstApparelPreventingShooting()
        └─> Apparel.AllowVerbCast(verb)
              └─> Shield.AllowVerbCast(verb)
                    └─> return !(verb is Verb_LaunchProjectile)
```

#### 결과
- 방패 착용 시 원거리 무기 발사 불가
- 근접 무기만 사용 가능

### 3. 무기 호환성 제한 (CompShieldWeaponIncompatible)

#### 화이트리스트 방식
- `allowedWeaponTags`에 명시된 태그를 가진 무기만 허용
- 기본 설정: `RK_WeaponTag_OneHand`, `RK_WeaponTag_LightShieldCompatible`

#### 검증 플로우

**1차 방어선: FloatMenu 차단 (Harmony Patch)**
```
1. 사용자가 방패 우클릭
2. FloatMenuOptionProvider_Wear.GetSingleOptionFor() 호출
3. Harmony Patch (FloatMenuPatch_ShieldWeaponCheck.Postfix) 실행
4. CompShieldWeaponIncompatible.TryIsWeaponAllowed() 호출
5. 무기 태그 검증
   - 무기의 weaponTags 중 하나라도 allowedWeaponTags에 있으면 허용
   - 없으면 차단
6. 차단 시 FloatMenuOption을 "착용 불가"로 대체
```

**2차 방어선: 착용 후 제거 (Notify_Equipped)**
```
1. 방패 착용 완료
2. CompShieldWeaponIncompatible.Notify_Equipped() 호출
3. 현재 착용 중인 무기 확인
4. TryIsWeaponAllowed()로 호환성 검증
5. 호환되지 않으면 DropIncompatibleWeapon() 실행
   - 무기를 바닥에 떨어뜨림
   - 경고 메시지 표시
```

#### 무기 태그 검증 로직
- 무기의 `weaponTags` 리스트 순회
- `allowedWeaponTags`에 하나라도 일치하면 허용
- 일치하는 태그가 없으면 차단

### 4. 방어 확률 통계 표시 (SpecialDisplayStats)

#### 표시 항목
- `BlockChance_Sharp`: 예리한 공격 방어 확률
- `BlockChance_Blunt`: 둔기 공격 방어 확률
- `BlockChance_Heat`: 열 공격 방어 확률

#### 계산 방식
- 각 타입별로 `스킬 기반 확률 + 소재 기반 확률` 합산
- 개발 모드에서 상세 정보 표시:
  - 근접 스킬 레벨 및 기여도
  - 방어력 및 기여도
  - 최종 방어 확률

## ShieldPatch (Harmony Patch)

### 목적
RimWorld의 `ThingDef.IsShieldThatBlocksRanged` 속성을 확장하여 `CompRKShield`를 가진 방패도 원거리 무기 차단 방패로 인식

### 패치 대상
- `ThingDef.IsShieldThatBlocksRanged` 속성 Getter

### 동작 방식
1. 원본 결과가 `true`면 즉시 반환
2. `ThingDef`가 `null`이면 반환
3. `CompRKShield` 컴포넌트가 있고 `CompProperties_RK_Shield`가 설정되어 있으면 `true` 반환

### 결과
- 게임 시스템이 `CompRKShield`를 가진 방패를 원거리 무기 차단 방패로 인식
- 다른 시스템과의 호환성 확보

## Def 설정 구조

### 추상 부모 (RK_ApparelAttr_ShieldBase)
- `thingClass`: `NewRatkin.Shield`
- `apparel/layers`: `OffHand`
- `apparel/defaultOutfitTags`: `Soldier`

### 구체적 방패 종류
- **RK_WoodenShield**: 목제 방패
- **RK_HeavyShield**: 철제 방패
- **RK_TowerShield**: 대형 철제 방패

### Comp 설정
- `CompShieldWeaponIncompatible`: 무기 호환성 제한
- `CompProperties_RK_Shield`: 원거리 무기 차단 설정 (현재 미사용)

## 작동 시나리오

### 시나리오 1: 정면 공격 방어
```
1. 적이 정면에서 공격
2. 공격 각도가 착용자 시야 ±70도 범위 내
3. 방어 확률 계산 (스킬 + 소재)
4. 랜덤 값과 비교
5. 성공 시 데미지 차단 + 이펙트 표시
```

### 시나리오 2: 측면/후면 공격
```
1. 적이 측면/후면에서 공격
2. 공격 각도가 착용자 시야 ±70도 범위 밖
3. 방어 메커니즘 작동 안 함
4. 데미지 정상 적용
```

### 시나리오 3: 원거리 무기 발사 시도
```
1. 방패 착용 중 원거리 무기 발사 시도
2. Verb.FirstApparelPreventingShooting() 호출
3. Shield.AllowVerbCast() → false 반환
4. 발사 차단
5. UI에 차단 메시지 표시
```

### 시나리오 4: 양손 무기와 방패 동시 착용 시도
```
1. 양손 무기 착용 중 방패 우클릭
2. FloatMenu 생성
3. Harmony Patch가 무기 태그 검증
4. 양손 무기 태그가 allowedWeaponTags에 없음
5. FloatMenuOption을 "착용 불가"로 변경
6. 사용자가 클릭 불가 상태 확인
```

## 특징 요약

### 방어 메커니즘
- **방향성 방어**: 정면 140도 범위만 방어
- **이중 확률 시스템**: 스킬 + 소재 방어력
- **소재 방어력 제한**: 최대 50%로 제한하여 밸런스 유지

### 무기 제한 시스템
- **화이트리스트 방식**: 허용된 무기만 사용 가능
- **이중 안전망**: FloatMenu 차단 + 착용 후 제거
- **사용자 친화적**: 명확한 차단 메시지 제공

### 시스템 통합
- **Harmony Patch**: 게임 시스템과의 호환성 확보
- **Comp 기반**: 확장 가능한 구조
- **Def 기반 설정**: XML로 쉽게 조정 가능

## 관련 파일

### 소스 코드
- `Project/1.6/Source/ShieldOfRatkinia/ApparelShield.cs`
- `Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs`
- `Project/1.6/Source/ShieldOfRatkinia/ShieldPatch.cs`

### Def 파일
- `Project/1.6/Defs/ThingsDefs/Apparel_Util.xml`

### 워크플로우 문서
- `WorkFlow/38_Shield_Weapon_Restriction_Complete.md`
- `WorkFlow/40_Shield_Abstract_Base.md`

