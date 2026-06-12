---
name: Energy Greatshield Design
overview: 에너지 대방패(RK_EnergyShield) 전용. 후보1만 구현 — 방향성(각도) 에너지 흡수 풀 + 자연충전 + 부스트 어빌리티(초집중 소모, 쿨 5시간, base x50% 증폭). 자원은 초집중만 사용(신경과열 보류). 이번 단계는 설계 확정까지.
todos:
  - id: f1-comp
    content: CompShieldDeflectEnergy 파생 — 각도 통과 시 관통 무시 흡수, 자연충전(간격 틱), effectiveMax 클램프
    status: completed
  - id: f1-boost
    content: 부스트 어빌리티 + Hediff — 초집중 소모, 쿨 5시간, base x50% (max/현재 둘다 +), 만료 시 max만 감소(클램프)
    status: completed
  - id: common-bool
    content: guaranteedBlockAttempt props — 에너지 활성 중 ShieldBlockChance 시도게이트 스킵(각도는 필수)
    status: completed
  - id: defs-xml
    content: Apparel_Shield.xml comp 교체 + AbilityDef/HediffDef + 한영 번역
    status: completed
  - id: balance
    content: energyBase/충전율/충전딜레이/초집중 소모량/부스트 지속 수치 밸런싱
    status: completed
isProject: false
---

## 에너지 대방패: 방향성 에너지 풀 + 부스트 어빌리티 (후보1)

검색 태그: EnergyShield Greatshield Psyfocus Deflect Absorb Ability Boost Design

### 확정 사항

- 대상: `RK_EnergyShield` **한 아이템만** (기존 목/중/타워 방패는 그대로).
- **후보1만 구현** (후보2/100% 흡수 어빌리티는 폐기).
- 자원: **초집중(사이포커스)만** 사용. **신경과열(엔트로피)은 보류**.
- **매프레임(틱) 체크 불필요**:
  - 데미지 흡수 판정 = `PostPreApplyDamage` (피격 **이벤트** 시에만 호출)
  - 어빌리티 지속 = `HediffComp_Disappears` 자동 만료
  - 어빌리티 사용/초집중 소모 = 사용 순간 1회
  - 유일한 틱 = 에너지 자연충전(매프레임 아님, 가벼운 간격 틱 — 바닐라 실드벨트와 동일)
- 부스트 어빌리티는 사실상 **사이링크 보유 폰 전용**(초집중 필요). 비사이커스터는 패시브 풀만 사용 가능, 부스트 사용 불가.

### 현재 판정 구조 (전제)

`CompShieldDeflect.PostPreApplyDamage`는 피해 1건마다: 1) 기본조건 → 2) **각도** → 3-1) **블록 시도 게이트**(`RK_Stat_ShieldBlockChance`, 근접스킬) → 3-2) **아머 vs 관통 롤** → 블록/관통.

### 패시브 — 방향성 에너지 흡수 풀

- 신규 컴포넌트 `CompShieldDeflectEnergy : CompShieldDeflect` (분기 대신 파생).
- 판정: **각도 통과 후** 에너지 잔량 > 0이면 관통 롤 없이 100% 흡수, `energy` 차감, 고갈 시 통과. → **각도 메커니즘 유지**(전방향 아님).
- `effectiveMax = energyBase × (부스트 Hediff 있으면 1.5, 없으면 1.0)`. `energy`는 접근/충전 시 항상 `effectiveMax`로 클램프.
- 자연충전: `CompTickInterval`(가벼운 간격)로 회복, 피격 후 `rechargeDelayAfterHitTicks` 동안 정지.
- 비사이커스터: 패시브 풀(`energyBase`)만 작동.

### 부스트 어빌리티 (`RK_Ability_EnergyShieldBoost`)

- `RK_EnergyShield` 착용 시 바닐라 `CompProperties_EquippableAbility`로 부여.
- **쿨다운: 5시간 = 12,500틱** (`cooldownTicksRange 12500~12500`).
- 사용 시 **초집중 소모**(`OffsetPsyfocusDirectly(-cost)`, 부족 시 시전 불가).
- 효과 (퍼센트, `boostPct = 0.5`):
  - 시전 즉시 `energy += energyBase × 0.5` (실 에너지 증가, 새 max로 클램프).
  - `RK_Hediff_EnergyShieldBoost`(지속 D) 부여 → 존재하는 동안 `effectiveMax = energyBase × 1.5` (최대치 증가).
  - **만료 시**: `effectiveMax`가 `energyBase`로 복귀 → 클램프에 의해 초과분 자동 증발, 현재 에너지가 base 이하면 max만 감소(현재값 유지). **별도 만료 코드 불필요**(클램프로 자연 처리).

### 공통 bool — 시도 무조건 성공

- props `guaranteedBlockAttempt`: 에너지 잔량으로 흡수하는 경로에서 **3-1 시도 게이트(`ShieldBlockChance`)를 스킵**(= 무조건 시도 성공). 각도(2)는 그대로 필수.
- `false`면 기존처럼 근접스킬 기반 시도 게이트 유지.

### 영향 파일 (구현 단계 기준)

- 신규 C#: `CompShieldDeflectEnergy.cs`, 초집중 소모 `CompAbilityEffect_*`(또는 부스트 적용 효과) (`Project/1.6/Source/ShieldOfRatkinia/`).
- `NewRatkin.csproj`: 신규 `.cs` `Compile Include` 항목 추가.
- `Apparel_Shield.xml`: `RK_EnergyShield`의 `CompProperties_ShieldDeflect` → `CompProperties_ShieldDeflectEnergy` 교체 + `CompProperties_EquippableAbility` 추가.
- 신규 Def: `AbilityDef RK_Ability_EnergyShieldBoost`, `HediffDef RK_Hediff_EnergyShieldBoost` (`Project/1.6/Defs/AbilityDefs/` 등) + 한/영 DefInjected 번역.

### 제안 기본값 (튜닝 가능)

- `energyBase` = 60 (흡수 가능 피해량).
- 자연충전: `CompTickInterval(60)`마다 +2 (≈ 0초→완충 30초), 피격 후 `rechargeDelayAfterHitTicks` = 300틱(5초) 정지.
- 초집중 소모량 = 0.35 (3등급 초집중 스킬 수준).
- **부스트 지속시간 D = 3600틱(≈60초)**, `boostPct` = 0.5.
- 쿨다운 = 12,500틱(5시간) 고정.

### 보류 항목

- 신경과열(엔트로피) 비용 연동.
