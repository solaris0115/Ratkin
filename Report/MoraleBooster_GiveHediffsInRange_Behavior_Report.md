Tags: MoraleBooster HediffComp_GiveHediffsInRange 부여 조건 제거 조건 범위 버프 Disappears 성능

# MoraleBooster — `HediffComp_GiveHediffsInRange` 동작·제거·개선 여지

대상: `Project/1.6/Source/MoraleBooster/HediffComp_GiveHediffsInRange.cs`, 시전자 Hediff `RK_Hediff_MoraleBoosterCaster`, 버프 Hediff들(`RK_Hediff_MoraleBoosterBuff*`).

---

## 1. 부여 조건 (실제 게임 흐름 기준)

시전 직후가 아니라, **시전자에게 붙은 `RK_Hediff_MoraleBoosterCaster`가 유지되는 동안**, 컴프가 **주기적으로 스윕**하면서 범위 안 대상에게 버프를 넣거나 타이머를 연장한다.

### 1.1 스윕이 돌아가려면 (시전자 쪽 게이트)

`CompPostTick` 초반에 아래면 **모트·스윕 모두 스킵**(버프 갱신 없음).

- 시전자(`caster`)가 `Awake()`가 아님 (기절·수면 등).
- `caster.health == null` 또는 `InPainShock`.
- `!caster.Spawned` (비스폰·월드 폰 등 맵에 없음).

즉 **시전 중 유지**는 능력이 준 시전자 Hediff가 살아 있고, 위 조건을 통과할 때만 주변에게 버프를 다시 줄 수 있다.

### 1.2 한 번의 스윕 주기

- `BuffApplyIntervalTicks`(현재 **15**)마다 폰 리스트를 순회한다.
- 그 사이 틱에서는 카운터만 감소하고 **대상 목록을 보지 않는다**.

### 1.3 후보 폰 풀

- `onlyPawnsInSameFaction`(기본 `true`, 모랄 부스트 XML에서 명시 안 함 → **같은 진영 스폰 폰만**): `SpawnedPawnsInFaction(caster.Faction)`.
- 아니면 `AllPawnsSpawned`(전체 맵 스폰 폰).

### 1.4 한 명의 대상에게 버프를 “줄 자격”이 있을 때 (루프 안 조건)

아래를 **연속 필터**로 통과해야 한다.

| 단계 | 의미 |
|------|------|
| 종족·상태 | Humanlike, 살아 있음, `health` 있음, **시전자 본인 제외** |
| 거리 | `pawn.Position`과 `caster.Position`의 거리 ≤ `Props.range`(모랄 부스트 **9.9**) |
| 타겟 규칙 | `targetingParameters != null`이면 `CanTarget(pawn, null)` — 모랄 부스트는 건물/동물/메크 타겟 불가 등 |
| 버프 Def | `ResolveBuffHediffForCaster(caster)`가 null이 아님 — Social 티어로 Weak/Default/Strong 중 하나, 티어 없으면 `Props.hediff` |

**“범위에 막 들어온 폰”**에 대한 별도 이벤트는 없다. **들어온 뒤 다음 스윕(최대 ~15틱 이내)** 에 걸리면 그때부터 조건을 만족하는 한 추가·갱신된다.

### 1.5 이미 버프 Hediff를 가진 대상을 어떻게 보는가

- `FindExistingMoraleBuff(pawn)`: 그 폰의 **전체 `hediffs` 리스트**를 선형 스캔.
- `IsConfiguredBuffDef(def)`: (1) `Props.hediff`와 동일, 또는 (2) `socialSkillHediffTiers`에 등록된 Def면 “이 오라가 주는 버프”로 인정.
- **첫 번째로 매칭되는 Hediff 하나**만 사용한다.

이후 분기:

- 이미 있는 버프의 Def가 **현재 시전자 Social로 결정된 `buffDef`와 다르면** → 기존 제거 후 새 Def로 `AddHediff`.
- 없으면 → `AddHediff`, `Severity = initialSeverity`(기본 1).
- 있으면(같은 Def) → 추가 호출 없이 그 인스턴스에 **`HediffComp_Disappears.ticksToDisappear = DisappearTicksAfterRefresh`(15)** 로 **만료 타이머만 리셋**.

즉 **“이미 갖고 있나?”**는 **전 Hediff 배열 스캔 + Def 화이트리스트**로만 판별한다. 별도 해시/캐시 없음.

---

## 2. 제거 조건

### 2.1 범위 이탈

이 컴프는 **범위 밖으로 나간 폰을 즉시 `RemoveHediff` 하지 않는다.**

- 범위 안에서 스윕에 걸릴 때만 `ticksToDisappear`가 15로 갱신된다.
- 범위 밖이면 스윕 루프에 안 잡히므로, 버프 Hediff의 **`HediffComp_Disappears`가 매 틱 `ticksToDisappear`를 깎다가 0 이하**가 되면 제거된다(`CompShouldRemove`).

그래서 **이탈 후 제거 지연**은 대략 **남은 타이머 틱 수**(최대 약 15틱 + 스윕 주기에 따른 오차)이다. “경계 나가자마자 끊김”이 아니다.

### 2.2 시전자 사망·시전자 Hediff 소멸

- 시전자용 `RK_Hediff_MoraleBoosterCaster`에는 **`HediffComp_Disappears`**, **`HediffComp_DisappearsOnDeath`**가 있다.
- 시전자가 죽거나 해당 Hediff가 제거되면 **`GiveHediffsInRange` 컴프 자체가 더 이상 틱하지 않는다** → 주변 버프에 대한 **갱신이 끊김**. 주변 버프는 위와 같이 **각자 Disappears 카운트다운으로 소멸**.

### 2.3 시전자 기절·페인쇼크·비스폰

위 1.1 게이트에 걸리면 **스윕 자체가 안 돈다** → 주변 버프 **갱신 중단**, 역시 **Disappears로만 자연 소멸**.

### 2.4 대상 사망

버프 Hediff들(`RK_HediffAttr_CombatRoleAuraBuff`)에 **`HediffComp_DisappearsOnDeath`**가 있어 대상 폰이 죽으면 정리된다.

### 2.5 티어 변경으로 인한 교체

같은 스윕 안에서 `FindExistingMoraleBuff`가 **다른 Def**를 반환하면(예: 시전자 Social 구간이 바뀐 경우는 드물고, 주로 설정된 티어와 불일치하는 잔여 버프) **`RemoveHediff` 후 새 Def 추가**.

---

## 3. 비용·개선 여지 (효율)

현재 구조의 특성:

- **주기마다** 진영 전체(또는 맵 전체) 스폰 폰 리스트를 순회하고, 후보마다 **거리·타겟 검사·Hediff 리스트 스캔**을 한다.
- 루프 **안에서** `ResolveBuffHediffForCaster(caster)`를 **매 폰마다 호출**한다 — 시전자 Social은 스윕당 **한 번**만 계산하면 되므로 **불필요한 중복 호출**이다.
- 범위 판정이 **전원 대상 거리**라, RimWorld 쪽 **셀/반경 기반 수집**(`GenRadial`, 맵 그리드 등)으로 후보를 줄이면 동일 의미를 유지하면서 순회 수를 줄일 여지가 있다.
- “범위 이탈 즉시 제거”가 디자인 목표가 아니라면(현재는 타이머 기반), **스윕 주기·Disappear 틱**을 설계 일관되게 두는 것이 중요하다. 반대로 즉시 끊고 싶다면 **이탈 시 Remove** 로직을 별도로 넣어야 한다(현재 없음).

---

## 4. 한 줄 요약

| 구분 | 방식 |
|------|------|
| 부여 | 주기 스윕 + 거리·타겟·Humanlike + Social 티어로 Def 선택; 기존 버프는 Hediff 배열에서 설정 Def 매칭 |
| 갱신 | 범위 안이면 `ticksToDisappear`를 15로 리셋 |
| 제거(주변 버프) | 주로 **Disappears 카운트다운**; 범위 밖·시전자 무효는 **갱신 끊김**으로 간접 제거 |
| 제거(시전자) | 능력 지속 Hediff + 사망 컴프 등 기존 Hediff 규칙 |
