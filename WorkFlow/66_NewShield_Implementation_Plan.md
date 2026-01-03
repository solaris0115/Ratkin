# NewShield 구현 작업 계획서

## 작업 개요
- 요청 내용: `NewRatkin.NewShield` 클래스 구현
- 목표: RimWorld 코어 쉴드 시스템을 참고하여 새로운 쉴드 시스템 구현
- 참고: RimWorld 코어 `CompShield` 분석 보고서 (Report/65_RimWorld_Core_Shield_System_Analysis.md)

## RimWorld 코어 쉴드 핵심 기능

### 1. 공격 차단 메커니즘
- `PostPreApplyDamage`: **근접/원거리 공격만 차단 가능**
- 폭발 공격: 차단 불가능 (통과시킴, 스태미나 감소 없음)
- EMP 공격: 차단 불가능 (통과시킴, 스태미나 감소 없음)
- `ignoreShields` 속성: 무시하고 차단 (근접/원거리는 차단)
- 근접 공격: 차단 가능
- 원거리 공격: 차단 가능

### 2. 스태미나 관리
- 스태미나 소모: 공격 유형에 따라 다른 비율 적용
  - 근접 공격: `stamina -= dinfo.Amount * staminaLossPerDamageMelee`
  - 원거리 공격: `stamina -= dinfo.Amount * staminaLossPerDamageRanged`
  - 폭발 공격: `stamina -= dinfo.Amount * staminaLossPerDamageExplosive` (통과하므로 사용 안 됨)
- 스태미나 재충전: `CompTick()`에서 `stamina += StaminaGainPerTick` (Active 상태에서만)
- 최대 스태미나: `StatDefOf.EnergyShieldEnergyMax` (명칭은 유지하되 스태미나로 사용)
- 재충전 속도: `StatDefOf.EnergyShieldRechargeRate / 60f` (틱당)

### 3. 쉴드 상태
- `ShieldState.Active`: 활성 상태, 공격 차단 및 재충전 가능 (스태미나 > 0)
- `ShieldState.Resetting`: 재충전 대기 중, 공격 차단 불가능
- `ShieldState.Disabled`: 비활성 상태 (스태미나 고갈 시에만, EMP나 기타 이유로는 비활성화 안 됨)

### 4. 쉴드 파괴 및 재충전
- `Break()`: 스태미나 0, `ticksToReset = startingTicksToReset` 설정
- `Reset()`: 재충전 완료, `stamina = staminaOnReset` 설정
- 재충전 대기 시간: `startingTicksToReset` (기본값: 3200틱)
- **스태미나 고갈인 경우만 쉴드 불가 상태** (EMP나 기타 이유로는 스태미나 감소 없음)

### 5. 시각 효과
- 쉴드 버블: `Draw()` 메서드로 렌더링, 에너지 비율에 따라 크기 변화
- 데미지 흡수: `AbsorbedDamage()` - 사운드, 플래시, 먼지 파티클
- 쉴드 파괴: `Break()` - `Shield_Break` 이펙터, 폭발 플래시
- 재충전 완료: `Reset()` - 사운드, 번개 글로우

### 6. 원거리 무기 차단
- `CompAllowVerbCast()`: `blocksRangedWeapons`가 true면 `Verb_LaunchProjectile` 차단
- `Apparel.AllowVerbCast()` 체인을 통해 Verb 발사 방지

### 7. UI 표시
- `Gizmo_StaminaShieldStatus`: 스태미나 바 및 현재/최대 스태미나 표시
- `CompGetWornGizmosExtra()`: 착용 시 Gizmo 추가

### 8. 설정 속성 (CompProperties_NewShield)
- `startingTicksToReset`: 재충전 대기 시간
- `minDrawSize`/`maxDrawSize`: 쉴드 버블 크기 범위
- `staminaLossPerDamageMelee`: 근접 공격당 스태미나 손실 비율 (XML 설정 가능)
- `staminaLossPerDamageRanged`: 원거리 공격당 스태미나 손실 비율 (XML 설정 가능)
- `staminaLossPerDamageExplosive`: 폭발 공격당 스태미나 손실 비율 (XML 설정 가능, 통과하므로 사용 안 됨)
- `staminaOnReset`: 재충전 완료 시 초기 스태미나 비율
- `blocksRangedWeapons`: 원거리 무기 발사 차단 여부

## 구현 계획

### 1단계: 기본 구조 설계
- [ ] `NewShield` 클래스 설계 (Apparel 상속)
- [ ] `CompProperties_NewShield` 클래스 생성
- [ ] `CompNewShield` 컴포넌트 생성
- [ ] 기존 `Shield` 및 `StaminaShield`와의 차이점 정의

### 2단계: 핵심 기능 구현
- [ ] `PostPreApplyDamage()` 메서드 구현 (근접/원거리 공격 차단 로직)
  - [ ] EMP 공격은 차단하지 않고 통과시킴 (스태미나 감소 없음)
  - [ ] 폭발 공격은 차단하지 않고 통과시킴 (스태미나 감소 없음)
  - [ ] 원거리/근접 공격만 차단
  - [ ] `ignoreShields` 속성 무시하고 차단
  - [ ] 공격 유형에 따라 다른 스태미나 소모량 적용:
    - 근접 공격: `staminaLossPerDamageMelee` 사용
    - 원거리 공격: `staminaLossPerDamageRanged` 사용
- [ ] 스태미나 관리 시스템 구현 (`CompTick()`, 스태미나 소모/재충전)
- [ ] 쉴드 상태 관리 (`ShieldState` enum, 상태 전환 로직)
  - [ ] 스태미나 고갈(<= 0)인 경우만 `Resetting` 상태로 전환
  - [ ] EMP나 기타 이유로는 스태미나 감소 없음
- [ ] 쉴드 파괴 및 재충전 메커니즘 (`Break()`, `Reset()`)

### 3단계: 시각 효과 구현
- [ ] 쉴드 버블 렌더링 (`Draw()`, `CompDrawWornExtras()`)
- [ ] 데미지 흡수 효과 (`AbsorbedDamage()` - 사운드, 파티클)
- [ ] 쉴드 파괴 효과 (`Break()` - 이펙터, 파티클)
- [ ] 재충전 완료 효과 (`Reset()` - 사운드, 글로우)

### 4단계: 부가 기능 구현
- [ ] 원거리 무기 발사 차단 (`CompAllowVerbCast()`)
- [ ] UI Gizmo 구현 (`Gizmo_NewShieldStatus`)
- [ ] Apparel 점수 보정 (`CompGetSpecialApparelScoreOffset()`)
- [ ] 개발 모드 기능 (DEV Gizmo)

### 5단계: XML Def 설정
- [ ] `CompProperties_NewShield` 기본값 설정
  - [ ] `staminaLossPerDamageMelee` 기본값 설정 (예: 0.033f)
  - [ ] `staminaLossPerDamageRanged` 기본값 설정 (예: 0.033f)
  - [ ] `staminaLossPerDamageExplosive` 기본값 설정 (예: 0.033f, 사용 안 됨)
- [ ] StatDef 설정 (`EnergyShieldEnergyMax`, `EnergyShieldRechargeRate` - 명칭은 유지하되 스태미나로 사용)
- [ ] 테스트용 ThingDef 생성 (선택사항)
  - [ ] XML에서 공격 유형별 스태미나 소모량 설정 예시 작성

### 6단계: 테스트 및 검증
- [ ] 빌드 오류 확인
- [ ] 게임 내 동작 테스트
- [ ] 스태미나 재충전 테스트
- [ ] 근접/원거리 공격 차단 테스트
- [ ] 근접 공격 시 `staminaLossPerDamageMelee` 적용 확인
- [ ] 원거리 공격 시 `staminaLossPerDamageRanged` 적용 확인
- [ ] XML에서 공격 유형별 스태미나 소모량 설정 테스트
- [ ] 폭발 공격 차단 불가능 확인 (통과, 스태미나 감소 없음)
- [ ] EMP 공격 차단 불가능 확인 (통과, 스태미나 감소 없음)
- [ ] 스태미나 고갈 시에만 쉴드 불가 상태 확인
- [ ] 시각 효과 확인

## 구현 세부사항

### NewShield 클래스 구조
```
NewRatkin.NewShield : Apparel
  - CompNewShield 컴포넌트 사용
  - AllowVerbCast() 오버라이드 (원거리 무기 차단)
```

### CompNewShield 컴포넌트 구조
```
CompNewShield : ThingComp
  - 스태미나 관리 (stamina, StaminaMax, StaminaGainPerTick)
  - 쉴드 상태 관리 (ShieldState)
  - PostPreApplyDamage() - 근접/원거리 공격만 차단 (폭발/EMP 제외)
  - CompTick() - 재충전
  - Draw() - 쉴드 버블 렌더링
  - AbsorbedDamage() - 흡수 효과
  - Break() - 파괴 효과 (스태미나 고갈 시에만)
  - Reset() - 재충전 완료 효과
  - CompAllowVerbCast() - 원거리 무기 차단
```

### CompProperties_NewShield 설정
- RimWorld 코어 `CompProperties_Shield` 기반
- `energyLossPerDamage` → 공격 유형별 스태미나 소모량으로 분리:
  - `staminaLossPerDamageMelee`: 근접 공격당 스태미나 손실 비율 (기본값: 0.033f)
  - `staminaLossPerDamageRanged`: 원거리 공격당 스태미나 손실 비율 (기본값: 0.033f)
  - `staminaLossPerDamageExplosive`: 폭발 공격당 스태미나 손실 비율 (기본값: 0.033f, 통과하므로 사용 안 됨)
- `energyOnReset` → `staminaOnReset`로 변경
- XML에서 각 공격 유형별 스태미나 소모량 개별 설정 가능

#### XML 설정 예시
```xml
<comps>
  <li Class="NewRatkin.CompProperties_NewShield">
    <startingTicksToReset>3200</startingTicksToReset>
    <minDrawSize>1.2</minDrawSize>
    <maxDrawSize>1.55</maxDrawSize>
    <staminaLossPerDamageMelee>0.05</staminaLossPerDamageMelee>
    <staminaLossPerDamageRanged>0.033</staminaLossPerDamageRanged>
    <staminaLossPerDamageExplosive>0.1</staminaLossPerDamageExplosive>
    <staminaOnReset>0.2</staminaOnReset>
    <blocksRangedWeapons>true</blocksRangedWeapons>
  </li>
</comps>
```

- 필요시 추가 속성 확장 가능

### PostPreApplyDamage 로직 (핵심 변경사항)
```
1. ShieldState가 Active이고 PawnOwner가 존재하는지 확인
2. EMP 공격인 경우 차단하지 않고 통과시킴 (스태미나 감소 없음)
3. 폭발 공격(isExplosive == true)인 경우 차단하지 않고 통과시킴 (스태미나 감소 없음)
4. 근접/원거리 공격만 차단:
   - isRanged == true 또는 근접 공격 (isExplosive == false)
5. ignoreShields 속성 무시하고 차단
6. 공격 유형에 따라 다른 스태미나 소모량 적용:
   - 근접 공격: stamina -= dinfo.Amount * Props.staminaLossPerDamageMelee
   - 원거리 공격: stamina -= dinfo.Amount * Props.staminaLossPerDamageRanged
7. 스태미나가 0 이하가 되면 Break() 호출
8. 스태미나가 남아있으면 AbsorbedDamage() 호출
9. absorbed = true 반환하여 데미지 차단
```

### ShieldState 로직 (핵심 변경사항)
```
- Active: 스태미나 > 0이고 ticksToReset <= 0
- Resetting: 스태미나 <= 0 (고갈)이고 ticksToReset > 0
- Disabled: Pawn 충전/셧다운, Dormant 상태 (스태미나와 무관)
- EMP나 기타 이유로는 스태미나 감소 없음 (스태미나 고갈인 경우만 Resetting)
```

## RimWorld 코어 쉴드 vs NewShield 비교

### 공격 차단 메커니즘 비교

| 공격 유형 | RimWorld 코어 쉴드 | NewShield | 변경점 |
|---------|------------------|-----------|--------|
| **원거리 공격** (`isRanged == true`) | ✅ 차단 | ✅ 차단 | 동일 |
| **폭발 공격** (`isExplosive == true`) | ✅ 차단 | ❌ 통과 (차단 안 함) | **변경: 차단 → 통과** |
| **근접 공격** | ❌ 통과 (차단 안 함) | ✅ 차단 | **변경: 통과 → 차단** |
| **EMP 공격** | ❌ 통과 (즉시 쉴드 파괴) | ❌ 통과 (스태미나 감소 없음) | 변경: 즉시 파괴 → 통과만 |
| **ignoreShields 속성** | ❌ 통과 (차단 안 함) | ✅ 차단 | **변경: 통과 → 차단** |

### 에너지/스태미나 관리 비교

| 항목 | RimWorld 코어 쉴드 | NewShield | 변경점 |
|-----|------------------|-----------|--------|
| **명칭** | 에너지 (energy) | 스태미나 (stamina) | **변경: 명칭 통일** |
| **EMP 공격 시** | 즉시 에너지 0, 쉴드 파괴 | 스태미나 감소 없음, 통과만 | **변경: 파괴 → 통과** |
| **스태미나 소모량** | 단일 비율 (`energyLossPerDamage`) | 공격 유형별 분리 (근접/원거리/폭발) | **변경: 공격 유형별 설정 가능** |
| **스태미나 고갈 시** | Resetting 상태 | Resetting 상태 | 동일 |
| **기타 이유로 감소** | 없음 | 없음 | 동일 |

### 쉴드 상태 비교

| 상태 | RimWorld 코어 쉴드 | NewShield | 변경점 |
|-----|------------------|-----------|--------|
| **Active** | 에너지 > 0, ticksToReset <= 0 | 스태미나 > 0, ticksToReset <= 0 | 동일 (명칭만 변경) |
| **Resetting** | 에너지 0 또는 EMP 파괴 | 스태미나 <= 0 (고갈만) | **변경: EMP 파괴 제거** |
| **Disabled** | Pawn 충전/셧다운, Dormant | Pawn 충전/셧다운, Dormant | 동일 |

### 주요 변경점 요약

1. **근접 공격**: 코어는 통과 → NewShield는 차단
2. **폭발 공격**: 코어는 차단 → NewShield는 통과
3. **EMP 공격**: 코어는 즉시 파괴 → NewShield는 통과만 (스태미나 감소 없음)
4. **ignoreShields 속성**: 코어는 통과 → NewShield는 차단
5. **명칭**: 에너지 → 스태미나로 통일
6. **스태미나 소모량**: 단일 비율 → 공격 유형별 분리 (XML에서 개별 설정 가능)
7. **스태미나 고갈**: 스태미나 고갈인 경우만 쉴드 불가 상태 (EMP나 기타 이유로는 감소 없음)

## 참고사항
- 기존 `Shield` 클래스는 방향성 방어 + 확률 기반 차단 시스템
- 기존 `StaminaShield`는 에너지 기반 쉴드 시스템 (이미 구현됨)
- `NewShield`는 RimWorld 코어 쉴드 기반이지만 차별화된 특징:
  - **스태미나 기반** (에너지가 아닌 스태미나로 명칭 통일)
  - **근접/원거리 공격만 차단 가능** (폭발/EMP 공격은 통과)
  - **폭발 공격은 차단 불가능** (통과시킴, 스태미나 감소 없음)
  - **EMP 공격은 차단 불가능** (통과시킴, 스태미나 감소 없음)
  - **공격 유형별 스태미나 소모량 설정 가능** (XML에서 근접/원거리/폭발 각각 설정)
  - **스태미나 고갈인 경우만 쉴드 불가 상태** (EMP나 기타 이유로는 스태미나 감소 없음)

## 관련 파일
- 소스 코드: `Project/1.6/Source/ShieldOfRatkinia/`
- 분석 보고서: `Report/65_RimWorld_Core_Shield_System_Analysis.md`
- 기존 구현: `ApparelShield.cs`, `CompStaminaShield.cs`

