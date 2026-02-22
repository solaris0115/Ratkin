# RimWorld 코어 쉴드 시스템 분석 보고서

## 개요

이 보고서는 RimWorld 코어의 에너지 쉴드 시스템(`CompShield`)에 대한 상세 분석입니다. ShieldBelt와 같은 에너지 쉴드 장비의 작동 원리, 차단 가능한 공격 유형, 시각 효과, 재충전 메커니즘을 다룹니다.

## 1. 어떤 공격을 차단 가능한지

### 1.1 차단 조건

쉴드는 `PostPreApplyDamage` 메서드에서 다음 조건을 순차적으로 확인합니다:

#### 조건 1: 쉴드 상태 확인
- `ShieldState`가 `Active` 상태여야 함
- `PawnOwner`가 존재해야 함
- 조건 불만족 시 차단하지 않음

#### 조건 2: EMP 공격 특수 처리
- `DamageDef == DamageDefOf.EMP`인 경우
- 에너지를 즉시 0으로 설정
- 쉴드를 파괴(`Break()`)하고 차단하지 않음
- **EMP는 쉴드를 완전히 무력화**

#### 조건 3: 쉴드 무시 속성 확인
- `dinfo.Def.ignoreShields == true`인 경우
- 차단하지 않고 통과시킴
- 예: `BeamBypassShields` (Odyssey DLC)

#### 조건 4: 공격 타입 확인
- `dinfo.Def.isRanged == true` 또는 `dinfo.Def.isExplosive == true`인 경우만 차단
- 조건 만족 시 데미지를 흡수하고 `absorbed = true` 반환

### 1.2 차단 가능한 공격 유형

#### ✅ 차단 가능 (isRanged = true)
- **Bullet**: 총알 데미지
- **Arrow**: 화살 데미지
- **ArrowHighVelocity**: 고속 화살
- **RangedStab**: 원거리 찌르기
- **BulletToxic**: 독성 총알
- **Bullet_TraitTox**: 독성 특성 총알
- **Bullet_TraitIncendiary**: 소이 특성 총알
- **Beam**: 빔 데미지 (단, `ignoreShields`가 없어야 함)

#### ✅ 차단 가능 (isExplosive = true)
- **Bomb**: 폭발 데미지
- **BombSuper**: 초강력 폭발
- **Thump**: 충격 데미지
- **Vaporize**: 증발 데미지
- **MiningBomb**: 채굴 폭탄
- **NociosphereVaporize**: 노시오스피어 증발 (Anomaly DLC)

#### ❌ 차단 불가능
- **근접 공격**: `isRanged = false`이고 `isExplosive = false`인 모든 근접 데미지
  - Cut, Crush, Blunt, Stab, Scratch, Bite 등
- **EMP 공격**: 즉시 쉴드 파괴
- **ignoreShields = true**: 쉴드 무시 속성을 가진 데미지
  - `BeamBypassShields` (Odyssey DLC)
- **환경 데미지**: Flame, Burn, Frostbite 등 (isRanged/isExplosive 없음)

### 1.3 차단 불가능한 공격 예시

| 데미지 타입 | 이유 |
|------------|------|
| Cut, Stab, Scratch | 근접 공격 (`isRanged = false`) |
| EMP | 쉴드 즉시 파괴 |
| BeamBypassShields | `ignoreShields = true` |
| Flame, Burn | 환경 데미지 (isRanged/isExplosive 없음) |

## 2. 작동 방식

### 2.1 전체 플로우

```
[공격 발생]
    ↓
PostPreApplyDamage() 호출
    ↓
[조건 확인]
    ├─ ShieldState != Active? → 차단 안 함
    ├─ EMP 공격? → 쉴드 파괴, 차단 안 함
    ├─ ignoreShields? → 차단 안 함
    └─ isRanged || isExplosive? → 차단 시도
         ↓
    [에너지 소모 계산]
    energy -= dinfo.Amount * Props.energyLossPerDamage
         ↓
    [에너지 확인]
    ├─ energy < 0? → Break() 호출, 쉴드 파괴
    └─ energy >= 0? → AbsorbedDamage() 호출, 데미지 차단
         ↓
    absorbed = true 반환
```

### 2.2 에너지 관리

#### 에너지 소모
- 공식: `energy -= dinfo.Amount * Props.energyLossPerDamage`
- 기본값: `energyLossPerDamage = 0.033`
- 예시: 데미지 30 → 에너지 0.99 소모

#### 에너지 재충전
- `CompTick()`에서 매 틱마다 실행
- 공식: `energy += EnergyGainPerTick`
- `EnergyGainPerTick = EnergyShieldRechargeRate / 60f`
- 최대 에너지: `EnergyShieldEnergyMax` (StatDef)

### 2.3 쉴드 상태 (ShieldState)

#### Active (활성)
- `ticksToReset <= 0`
- 에너지 재충전 가능
- 공격 차단 가능

#### Resetting (재충전 중)
- `ticksToReset > 0`
- 에너지 재충전 불가능
- 공격 차단 불가능
- 재충전 시간 경과 후 `Reset()` 호출

#### Disabled (비활성)
- Pawn이 충전 중이거나 셧다운 상태
- `CompCanBeDormant`가 비활성 상태
- 공격 차단 불가능

### 2.4 원거리 무기 발사 차단

#### 메커니즘
- `CompAllowVerbCast(Verb verb)` 메서드 사용
- `Props.blocksRangedWeapons == true`이고 `verb is Verb_LaunchProjectile`이면 `false` 반환
- `Apparel.AllowVerbCast()`를 통해 호출됨

#### 호출 체인
```
Verb.TryCastShot() / Verb.CanHitTarget()
  └─> Verb.FirstApparelPreventingShooting()
        └─> Apparel.AllowVerbCast(verb)
              └─> CompShield.CompAllowVerbCast(verb)
                    └─> return !Props.blocksRangedWeapons || !(verb is Verb_LaunchProjectile)
```

## 3. 시각 효과

### 3.1 쉴드 버블 렌더링

#### 표시 조건
- `ShieldState == Active`
- `ShouldDisplay == true` (다음 조건 중 하나):
  - Pawn이 공격적 정신 상태 (`InAggroMentalState`)
  - Pawn이 징집됨 (`Drafted`)
  - Pawn이 적대 세력이고 포로가 아님
  - 최근 1000틱 내에 데미지 흡수 (`lastKeepDisplayTick`)
  - Biotech DLC 활성화 시 메카닉이 선택됨

#### 렌더링 방식
- 위치: `PawnOwner.Drawer.DrawPos` (고도: `MoteOverhead`)
- 크기: `Lerp(minDrawSize, maxDrawSize, energy)`
  - 최소: 1.2 (기본값)
  - 최대: 1.55 (기본값)
  - 에너지 비율에 따라 크기 변화
- 회전: 매 프레임 랜덤 각도 (0~360도)
- 재질: `MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent)`

#### 데미지 흡수 시 시각 효과
- 최근 8틱 내 데미지 흡수 시:
  - 버블이 공격 방향으로 약간 이동 (`impactAngleVect * 0.05f`)
  - 버블 크기 약간 감소

### 3.2 데미지 흡수 효과 (AbsorbedDamage)

#### 사운드
- `SoundDefOf.EnergyShield_AbsorbDamage` 재생
- 위치: `PawnOwner.Position`

#### 파티클 효과
- 위치: `PawnOwner.TrueCenter() + impactAngleVect.RotatedBy(180f) * 0.5f`
- `FleckDefOf.ExplosionFlash` 생성
  - 크기: `Min(10f, 2f + dinfo.Amount / 10f)`
  - 데미지가 클수록 큰 플래시
- `FleckMaker.ThrowDustPuff` 생성
  - 개수: 플래시 크기와 동일
  - 크기: 0.8~1.2 랜덤

#### 표시 시간 연장
- `KeepDisplaying()` 호출
- `lastKeepDisplayTick` 업데이트
- 쉴드 버블이 추가로 1000틱 동안 표시됨

### 3.3 쉴드 파괴 효과 (Break)

#### 이펙터
- `EffecterDefOf.Shield_Break` 생성
- 스케일: `Lerp(minDrawSize, maxDrawSize, energy)`
- 파괴 시점의 에너지에 따라 크기 결정

#### 파티클 효과
- `FleckDefOf.ExplosionFlash` 생성 (크기: 12f)
- `FleckMaker.ThrowDustPuff` 6개 생성
  - 위치: `PawnOwner.TrueCenter()` 주변 랜덤
  - 거리: 0.3~0.6 랜덤
  - 각도: 0~360도 랜덤

#### 상태 변경
- `energy = 0f`
- `ticksToReset = Props.startingTicksToReset` (기본값: 3200틱)

### 3.4 쉴드 재충전 완료 효과 (Reset)

#### 사운드
- `SoundDefOf.EnergyShield_Reset` 재생
- 위치: `PawnOwner.Position`

#### 파티클 효과
- `FleckMaker.ThrowLightningGlow` 생성
- 위치: `PawnOwner.TrueCenter()`
- 크기: 3f

#### 상태 변경
- `ticksToReset = -1` (Active 상태로 전환)
- `energy = Props.energyOnReset` (기본값: 0.2, 최대 에너지의 20%)

## 4. 재충전과 기타 기능

### 4.1 재충전 메커니즘

#### 재충전 속도
- StatDef: `EnergyShieldRechargeRate`
- 틱당 증가량: `EnergyShieldRechargeRate / 60f`
- 예: 재충전 속도 60 → 틱당 1.0 증가

#### 재충전 조건
- `ShieldState == Active` 상태에서만 재충전
- `CompTick()`에서 매 틱마다 실행
- 최대 에너지 제한: `energy = Min(energy, EnergyMax)`

#### 재충전 시간 계산
- 최대 에너지에서 0으로 떨어진 경우:
  - 재충전 대기: `startingTicksToReset` (기본값: 3200틱 = 약 53초)
  - 재충전 시작: `energyOnReset` (기본값: 0.2)로 시작
  - 완전 충전까지: `(EnergyMax - energyOnReset) / EnergyGainPerTick` 틱

### 4.2 에너지 관련 StatDef

#### EnergyShieldEnergyMax
- 최대 에너지량
- 기본값: ShieldBelt 기준 1.0 (100%)
- XML에서 `statBases`로 설정 가능

#### EnergyShieldRechargeRate
- 재충전 속도 (초당)
- 기본값: ShieldBelt 기준 60 (초당 60%)
- XML에서 `statBases`로 설정 가능

### 4.3 기타 기능

#### Apparel 점수 보정
- `CompGetSpecialApparelScoreOffset()` 메서드
- 공식: `EnergyMax * 0.25f`
- 최대 에너지가 높을수록 착용 우선순위 증가

#### Gizmo 표시
- `Gizmo_EnergyShieldStatus` 생성
- 조건:
  - 플레이어 세력이거나 메카닉인 경우
  - 선택된 Pawn인 경우
- 표시 내용:
  - 쉴드 이름
  - 에너지 바 (현재/최대)
  - 에너지 퍼센트

#### 개발 모드 기능
- `DebugSettings.ShowDevGizmos` 활성화 시:
  - "DEV: Break" 버튼: 쉴드 즉시 파괴
  - "DEV: Clear reset" 버튼: 재충전 시간 즉시 완료

### 4.4 CompProperties_Shield 설정

#### startingTicksToReset
- 쉴드 파괴 후 재충전 시작까지 걸리는 틱 수
- 기본값: 3200틱 (약 53초)

#### minDrawSize / maxDrawSize
- 쉴드 버블 최소/최대 크기
- 기본값: 1.2 / 1.55

#### energyLossPerDamage
- 데미지당 에너지 손실 비율
- 기본값: 0.033
- 예: 데미지 30 → 에너지 0.99 소모

#### energyOnReset
- 재충전 완료 시 초기 에너지 비율
- 기본값: 0.2 (최대 에너지의 20%)

#### blocksRangedWeapons
- 원거리 무기 발사 차단 여부
- 기본값: true

## 5. 작동 시나리오

### 시나리오 1: 총알 공격 차단
```
1. Bullet 데미지 발생 (isRanged = true)
2. PostPreApplyDamage() 호출
3. ShieldState == Active 확인
4. EMP 아님 확인
5. ignoreShields 아님 확인
6. isRanged == true 확인
7. 에너지 소모: energy -= 30 * 0.033 = 0.99
8. energy >= 0 확인
9. AbsorbedDamage() 호출
   - 사운드 재생
   - 플래시 및 먼지 파티클 생성
10. absorbed = true 반환
11. 데미지 차단 완료
```

### 시나리오 2: 쉴드 파괴
```
1. Bomb 데미지 발생 (isExplosive = true)
2. PostPreApplyDamage() 호출
3. 조건 확인 통과
4. 에너지 소모: energy -= 50 * 0.033 = 1.65
5. energy < 0 확인
6. Break() 호출
   - Shield_Break 이펙터 생성
   - 폭발 플래시 및 먼지 생성
7. energy = 0, ticksToReset = 3200 설정
8. ShieldState = Resetting으로 변경
9. absorbed = true 반환 (하지만 쉴드 파괴됨)
```

### 시나리오 3: EMP 공격
```
1. EMP 데미지 발생
2. PostPreApplyDamage() 호출
3. ShieldState == Active 확인
4. EMP 공격 확인
5. energy = 0 설정
6. Break() 호출
7. absorbed = false 반환 (차단 안 함)
8. EMP 데미지가 Pawn에게 적용됨
```

### 시나리오 4: 근접 공격
```
1. Cut 데미지 발생 (isRanged = false, isExplosive = false)
2. PostPreApplyDamage() 호출
3. ShieldState == Active 확인
4. EMP 아님 확인
5. ignoreShields 아님 확인
6. isRanged == false, isExplosive == false 확인
7. 조건 불만족으로 차단 안 함
8. absorbed = false 반환
9. 근접 데미지가 정상적으로 적용됨
```

## 6. 요약

### 차단 가능한 공격
- ✅ 원거리 공격 (`isRanged = true`)
- ✅ 폭발 공격 (`isExplosive = true`)
- ❌ 근접 공격
- ❌ EMP 공격 (쉴드 파괴)
- ❌ `ignoreShields = true` 공격

### 주요 특징
- 에너지 기반 방어 시스템
- 자동 재충전 메커니즘
- 시각적 피드백 (버블, 이펙트, 사운드)
- 원거리 무기 발사 차단 기능
- EMP에 취약

### 설정 가능한 값
- 최대 에너지 (`EnergyShieldEnergyMax`)
- 재충전 속도 (`EnergyShieldRechargeRate`)
- 재충전 대기 시간 (`startingTicksToReset`)
- 데미지당 에너지 손실 (`energyLossPerDamage`)
- 재충전 완료 시 초기 에너지 (`energyOnReset`)
- 원거리 무기 차단 여부 (`blocksRangedWeapons`)

