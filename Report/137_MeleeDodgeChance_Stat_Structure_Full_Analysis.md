# MeleeDodgeChance 스탯 구조 완전 분석 보고서

> **Tags**: MeleeDodgeChance StatDef StatWorker SkillNeed capacityOffsets postProcessCurve StatDefOf Verb_MeleeAttack 근접회피 스탯구조 새스탯생성참조  
> **작성일**: 2026-03-30  
> **목적**: 새로운 유사 스탯을 만들기 위한 구조 레퍼런스

---

## 1. 개요 — 새 스탯을 만들 때 건드려야 할 모든 곳

새로운 Pawn 전투 스탯을 `MeleeDodgeChance`처럼 만들려면 다음 **7개 영역**을 모두 처리해야 한다.

| # | 영역 | 파일 예시 | 설명 |
|---|------|-----------|------|
| 1 | **StatDef XML** | `Stats_Pawns_Combat.xml` | 스탯 정의 본체 |
| 2 | **StatDefOf C#** | `StatDefOf.cs` | 코드에서 참조할 정적 필드 |
| 3 | **판정 로직 C#** | `Verb_MeleeAttack.cs` | 실제 사용처 (계산·판정) |
| 4 | **장비/종족/특성 Def** | 각종 ThingDef, TraitDef 등 | `statBases`, `equippedStatOffsets` 등 |
| 5 | **전투 로그** | `ManeuverDef`, `RulePackDef` | 결과 문자열 표시 |
| 6 | **사운드** | `SoundDef`, `RaceProperties` | 결과 효과음 |
| 7 | **UI 표시** | `StatDef` 자체 필드 | `displayPriorityInCategory`, `showOnPawns` 등 |

---

## 2. StatDef XML 구조 상세

### 2.1 MeleeDodgeChance 원본 (Core)

**파일**: `RimworldData/Core/Defs/Stats/Stats_Pawns_Combat.xml` (122~160행)

```xml
<StatDef>
  <defName>MeleeDodgeChance</defName>
  <label>melee dodge chance</label>
  <description>Chance to dodge a melee attack ...</description>
  <category>PawnCombat</category>
  <neverDisabled>true</neverDisabled>
  <defaultBaseValue>0</defaultBaseValue>
  <minValue>0</minValue>
  <toStringStyle>PercentZero</toStringStyle>
  <toStringStyleUnfinalized>FloatOne</toStringStyleUnfinalized>
  <noSkillOffset>0</noSkillOffset>
  <skillNeedOffsets>
    <li Class="SkillNeed_BaseBonus">
      <skill>Melee</skill>
      <baseValue>0</baseValue>
      <bonusPerLevel>1</bonusPerLevel>
    </li>
  </skillNeedOffsets>
  <capacityOffsets>
    <li>
      <capacity>Moving</capacity>
      <scale>18</scale>
    </li>
    <li>
      <capacity>Sight</capacity>
      <scale>8</scale>
      <max>1.4</max>
    </li>
  </capacityOffsets>
  <postProcessCurve>
    <points>
      <li>(5, 0)</li>
      <li>(20, 0.30)</li>
      <li>(60, 0.50)</li>
    </points>
  </postProcessCurve>
  <displayPriorityInCategory>4100</displayPriorityInCategory>
  <showDevelopmentalStageFilter>Child, Adult</showDevelopmentalStageFilter>
</StatDef>
```

### 2.2 각 필드 의미 및 계산 파이프라인 매핑

| XML 필드 | C# 대응 | 계산 단계 | 설명 |
|----------|---------|-----------|------|
| `defName` | `StatDef.defName` | — | 고유 ID. 다른 Def에서 이 이름으로 참조 |
| `label` / `description` | — | UI | 정보 창 표시 텍스트 |
| `category` | `StatCategoryDef` | UI | 스탯 카드의 분류 탭 (`PawnCombat`) |
| `defaultBaseValue` | `StatDef.defaultBaseValue` | ①기본값 | Pawn의 `statBases`에 없으면 이 값 사용. 여기선 `0` |
| `minValue` | `StatDef.minValue` | ⑥클램프 | 최종값 하한 |
| `neverDisabled` | `StatDef.neverDisabled` | — | true면 관련 스킬 비활성이어도 스탯 작동 |
| `noSkillOffset` | `StatDef.noSkillOffset` | ②스킬 | 스킬 시스템 없는 폰(동물 등)에 적용하는 대체 오프셋 |
| `skillNeedOffsets` | `List<SkillNeed>` | ②스킬 | 스킬 레벨 → **가산** 성분 |
| `capacityOffsets` | `List<PawnCapacityOffset>` | ③신체능력 | 신체 능력치 → **가산** 성분 |
| `postProcessCurve` | `SimpleCurve` | ⑤후처리 | Raw합계 → 커브 매핑 → 최종값 |
| `toStringStyle` | — | UI | 최종값 표시 형식 (`PercentZero` = "30%") |
| `toStringStyleUnfinalized` | — | UI | 커브 전 Raw값 표시 형식 (`FloatOne` = "26.0") |
| `displayPriorityInCategory` | — | UI | 같은 카테고리 내 정렬 순서 (큰 값 = 위) |
| `showDevelopmentalStageFilter` | — | UI | 표시 대상 필터 (Baby 제외) |

### 2.3 StatDef에서 사용 가능하지만 MeleeDodgeChance는 안 쓰는 필드들

새 스탯에서 필요할 수 있는 필드 목록:

| 필드 | 용도 | 사용 예시 |
|------|------|-----------|
| `skillNeedFactors` | 스킬 레벨 → **곱연산** | `MiningSpeed` 등 작업 스탯 |
| `capacityFactors` | 신체 능력치 → **곱연산** | `MoveSpeed` (Moving weight 1) |
| `statFactors` | 다른 StatDef의 최종값을 **곱** | 범용 배율 체인 |
| `postProcessStatFactors` | 후처리 단계에서 다른 스탯의 최종값을 **곱** | `ShootingAccuracyPawn`→`ShootingAccuracyChildFactor` |
| `parts` | `StatPart` 서브클래스로 조건부 보정 | `StatPart_Age` (나이 보정) |
| `workerClass` | 커스텀 StatWorker | `StatWorker_MeleeDPS` 등 |
| `showOnPawns` | Pawn 정보창 표시 여부 | `MeleeHitChance`는 `false` |
| `maxValue` | 최종값 상한 | 기본 9999999 |
| `noSkillFactor` | 스킬 없을 때 곱연산 대체값 | 기본 1 |
| `finalizeEquippedStatOffset` | 장비 오프셋도 후처리 대상? | `MeleeHitChance`는 `false` |

---

## 3. 계산 파이프라인 (StatWorker)

`MeleeDodgeChance`는 `workerClass` 미지정 → 기본 **`StatWorker`** 사용.  
소스: `RimworldSource/RimWorld/StatWorker.cs`

### 3.1 전체 흐름

```
GetValue()
  ├─ GetValueUnfinalized()    ... Raw값 산출
  │    ├─ ① GetBaseValueFor()        statBases or defaultBaseValue
  │    ├─ ② skillNeedOffsets          스킬 레벨 × bonusPerLevel (가산)
  │    ├─ ③ capacityOffsets           (능력치% - 1) × scale (가산)
  │    ├─ ④ trait.OffsetOfStat()      특성 오프셋 (가산)
  │    ├─ ④ hediff.StatOffset()       헤디프 오프셋 (가산)
  │    ├─ ④ ideo.statOffsets          이데올로기 교리 오프셋 (가산)
  │    ├─ ④ gene.statOffsets          유전자 오프셋 (가산)
  │    ├─ ④ equippedStatOffsets       장비 오프셋 (가산)
  │    ├─ ④ inventory offsets         소지품 오프셋 (가산)
  │    ├─ ──── 가산 완료 ────
  │    ├─ ⑤ statFactors              다른 스탯 곱연산
  │    ├─ ⑤ skillNeedFactors         스킬 곱연산
  │    ├─ ⑤ capacityFactors          능력치 곱연산
  │    ├─ ⑤ trait.StatFactor          특성 곱연산
  │    ├─ ⑤ hediff.StatFactor         헤디프 곱연산
  │    ├─ ⑤ ideo.statFactors         교리 곱연산
  │    ├─ ⑤ gene.statFactors         유전자 곱연산
  │    ├─ ⑤ equippedStatFactors      장비 곱연산
  │    └─ ⑤ inspiration offsets/factors 영감
  │
  └─ FinalizeValue()           ... 후처리
       ├─ ⑥ parts[].TransformValue()  StatPart 변환
       ├─ ⑦ postProcessCurve          SimpleCurve.Evaluate(raw)
       ├─ ⑧ postProcessStatFactors    다른 스탯 최종값 곱
       ├─ ⑧ Scenario.GetStatFactor    시나리오 배율
       └─ ⑨ Clamp(minValue, maxValue) 최종 클램프
```

### 3.2 MeleeDodgeChance에서 실제 활성화되는 단계

| 단계 | 활성 | 내용 |
|------|------|------|
| ① baseValue | O | `defaultBaseValue=0`. 종족 `statBases`로 오버라이드 가능 |
| ② skillNeedOffsets | O | `Melee × 1` (base=0, bonus=1) |
| ③ capacityOffsets | O | Moving×18, Sight×8(max 1.4) |
| ④ 가산 보정 | O | trait, hediff, 장비 등의 `statOffsets` / `equippedStatOffsets` |
| ⑤ 곱연산 보정 | O | trait, hediff, 장비 등의 `statFactors` (XML에 별도 지정 없음) |
| ⑥ StatPart | X | `parts` 없음 |
| ⑦ postProcessCurve | O | (5→0), (20→0.30), (60→0.50) |
| ⑧ postProcessStatFactors | X | 없음 |
| ⑨ Clamp | O | min=0 |

### 3.3 핵심 클래스 소스

#### SkillNeed_BaseBonus

소스: `RimworldSource/RimWorld/SkillNeed_BaseBonus.cs`

```csharp
public override float ValueFor(Pawn pawn)
{
    int level = pawn.skills.GetSkill(this.skill).Level;
    return this.baseValue + this.bonusPerLevel * (float)level;
}
```

MeleeDodgeChance: `baseValue=0`, `bonusPerLevel=1` → **레벨 × 1** 반환

#### PawnCapacityOffset

소스: `RimworldSource/RimWorld/PawnCapacityOffset.cs`

```csharp
public float GetOffset(float capacityEfficiency)
{
    return (Mathf.Min(capacityEfficiency, this.max) - 1f) * this.scale;
}
```

- Moving: `(Moving% - 1) × 18`. 이동 100% → +0, 50% → -9
- Sight: `(min(Sight%, 1.4) - 1) × 8`. 시력 100% → +0, 140%+ → +3.2

**정상 인간형(100%)**: 두 capacity 기여 = 0. 따라서 **Raw = 0 + Melee×1 + 0 + 0 = Melee레벨**
...실제로는 capacity 100% = 1.0이므로 `(1.0-1.0)×scale = 0`. 즉 정상이면 capacity는 Raw에 0을 더한다.

> **주의**: 정상 인간형에서 `Raw = Melee레벨 + 26`이라는 기존 보고서의 수치는 **종족 `statBases`나 다른 보정이 포함된 경우**를 가정한 것일 수 있음. 순수 바닐라 인간형에서 capacity 100%면 offset은 0이므로 **Raw ≈ Melee레벨 + 0(base) + 0(capacity) = Melee레벨** → 이를 커브에 넣으면 매우 낮은 값이 됨.

> **수정**: capacity 100%는 `GetLevel()` 반환값이 **1.0**. `(1.0 - 1.0) × 18 = 0`. 맞음. 하지만 **Moving capacity = 100% (이동 불가 부상 없음) → GetLevel = 1.0**이므로, 사실상 capacity offset은 base에서 벗어났을 때만 효과가 있다. 기존 보고서의 "+26"은 **재검증 필요**.

> **재확인**: 실제 게임에서 `pawn.health.capacities.GetLevel(Moving)`은 건강한 인간이면 **정확히 1.0**을 반환. 따라서 offset = 0. **정상 인간의 Raw = Melee레벨 + defaultBaseValue(0) = Melee레벨**. 스킬 0이면 Raw=0 → 커브에서 0%(raw≤5). 이 경우 Melee 20이면 Raw=20 → 30%.

> **결론**: 기존 보고서의 "Raw = Melee + 26"은 착오. 올바른 값은 **Raw = Melee레벨** (정상 능력치, 장비 보정 없음). 단, `statBases`에 base가 지정된 종족이면 그만큼 더해짐 (Ratkin은 `statBases`에서 `MeleeDodgeChance`를 1.15로? → 이건 `statFactors` 배율일 수 있음, 확인 필요).

---

## 4. Pawn 정보 창 표시 구조

### 4.1 일반 스탯 목록

StatDef 자체 필드만으로 자동 표시됨. 별도 코드 불필요.

| StatDef 필드 | 효과 |
|-------------|------|
| `category: PawnCombat` | "전투" 카테고리에 배치 |
| `displayPriorityInCategory: 4100` | 카테고리 내 정렬 위치 |
| `showOnPawns: (기본 true)` | Pawn 정보창에 표시 |
| `toStringStyle: PercentZero` | 최종값을 "30%" 형태로 표시 |
| `toStringStyleUnfinalized: FloatOne` | Raw값을 "26.0" 형태로 표시 |
| `showDevelopmentalStageFilter: Child, Adult` | Baby에겐 표시 안 함 |

### 4.2 이데올로기 어둠 보정 (추가 표시 행)

별도 `StatDrawEntry`로 추가 — `Pawn.SpecialDisplayStats()` → `DarknessCombatUtility.GetStatEntriesForPawn()`.

**소스**: `RimworldSource/DarknessCombatUtility.cs`

교리의 `statOffsets`에서 `MeleeDodgeChanceIndoorsDarkOffset` 등 4종을 모아 min~max 범위 문자열로 표시. `displayPriority`는 `StatDisplayOrder.Pawn_DarknessMeleeDodgeChance = 4101`.

→ **새 스탯에도 이데올로기 연동이 필요하면** 이 패턴을 참고하여 오프셋용 StatDef 4종 + DarknessCombatUtility 분기 추가.

### 4.3 스탯 설명(Explanation) 패널

`StatWorker.GetExplanationUnfinalized()`가 자동으로 각 보정 소스를 줄별로 나열:
- Base value: 0
- Melee skill: +10 (레벨 10일 때)
- Moving: +0.0 (100%)
- 장비: -0.2 (방패 등)
- Trait: +15 (nimble 등)
- ...
- Raw: 24.8
- Final (커브 후): 32.4%

이 설명은 StatDef 구조만으로 **자동 생성**됨. 커스텀 StatWorker 없이도 동작.

---

## 5. 스킬 레벨에 따른 성장 제어

### 5.1 구조

```
skillNeedOffsets → SkillNeed_BaseBonus
  ├─ skill: Melee
  ├─ baseValue: 0        ← 스킬 0일 때 기여값
  └─ bonusPerLevel: 1    ← 레벨당 증가분
```

계산: `0 + 1 × level` → 레벨당 Raw +1

### 5.2 postProcessCurve와의 상호작용

Raw가 커브에 들어가므로, **같은 +1 Raw라도 커브 구간에 따라 최종값 변화가 다름**:

| 구간 | Raw 범위 | 기울기 | 레벨 +1 → 최종값 변화 |
|------|----------|--------|----------------------|
| 불능 | ≤5 | 0 | 0%p |
| 급상승 | 5~20 | 2%p/raw | +2.0%p |
| 완만 | 20~60 | 0.5%p/raw | +0.5%p |

### 5.3 실전 수치 (정상 인간형, 장비/특성 없음)

base=0, capacity offsets=0이므로 **Raw = Melee레벨**:

| Melee Lv | Raw | 최종 회피율 | 비고 |
|----------|-----|-----------|------|
| 0~5 | 0~5 | 0% | 불능 구간 |
| 6 | 6 | 2% | 회피 시작 |
| 10 | 10 | 10% | |
| 15 | 15 | 20% | |
| 20 | 20 | 30% | 커브 변곡점 |

장비 `equippedStatOffsets`나 종족 `statBases` 보정이 들어오면 Raw가 더 높아져 다른 구간에 진입.

### 5.4 새 스탯에서의 성장 커스터마이징

| 조절 대상 | 방법 |
|-----------|------|
| **레벨당 증가 속도** | `bonusPerLevel` 값 변경 |
| **기본 기여값** | `baseValue` 변경 (0 아닌 값이면 스킬 0에서도 기여) |
| **성장 곡선** | `postProcessCurve` 포인트 변경 |
| **성장 상한** | 커브 마지막 포인트의 y값 |
| **성장 시작점** | 커브 첫 포인트의 x값 (5 = Raw≤5면 0%) |
| **능력치 의존도** | `capacityOffsets`의 `scale`/`max` 변경 |

---

## 6. 판정 위치 — 코드에서 스탯을 읽는 곳

### 6.1 바닐라: Verb_MeleeAttack.GetDodgeChance()

**파일**: `RimworldSource/RimWorld/Verb_MeleeAttack.cs` (191~232행)

```
TryCastShot()
  ├─ Rand.Chance(GetNonMissChance) → 빗맞춤 판정
  └─ 명중 시 → Rand.Chance(GetDodgeChance) → 회피 판정
       ├─ 회피 성공 → SoundDodge(), TextMote_Dodge, combatLogRulesDodge
       └─ 회피 실패 → ApplyMeleeDamageToTarget()
```

**GetDodgeChance 로직**:

```csharp
private float GetDodgeChance(LocalTargetInfo target)
{
    if (surpriseAttack) return 0f;           // 기습
    if (IsTargetImmobile(target)) return 0f; // 쓰러짐/건물
    Pawn pawn = target.Thing as Pawn;
    if (pawn == null) return 0f;             // Pawn 아님
    // 원거리 무기 조준/발사 중이면 회피 불가
    Stance_Busy sb = pawn.stances.curStance as Stance_Busy;
    if (sb != null && sb.verb != null && !sb.verb.verbProps.IsMeleeAttack)
        return 0f;

    float num = pawn.GetStatValue(StatDefOf.MeleeDodgeChance);

    // 이데올로기: 위치별 4종 오프셋 합산
    if (ModsConfig.IdeologyActive)
    {
        // OutdoorsLit, OutdoorsDark, IndoorsDark, IndoorsLit 중 하나
        num += pawn.GetStatValue(해당 오프셋 StatDef);
    }
    return num;
}
```

### 6.2 Ratkin 모드: 커스텀 Verb

| 파일 | 클래스 | 차이점 |
|------|--------|--------|
| `Project/1.6/Source/RatkinGuerrilla/Verb_MeleeExplosion.cs` | `Verb_MeleeExplosion` | 이데올로기 오프셋 미적용 |
| `Project/1.6/Source/Gunlance/Verb_GunlanceFiring.cs` | `Verb_GunlanceFiring` | 이데올로기 오프셋 미적용 |

두 커스텀 Verb 모두 `pawn.GetStatValue(StatDefOf.MeleeDodgeChance)`만 읽고 이데올로기 4종 오프셋은 합산하지 않음.

→ **새 스탯이 dodge와 비슷한 판정이면**, 이 커스텀 Verb들에도 추가 분기를 넣어야 할 수 있음.

---

## 7. 연관 시스템 전체 맵

### 7.1 데이터 (XML/Def)

| 위치 | 역할 | 내용 |
|------|------|------|
| **Core StatDef** | 스탯 정의 | `Stats_Pawns_Combat.xml` → `MeleeDodgeChance` |
| **Ideology StatDef** | 조명 오프셋 | `Stats_Pawns_Ideo.xml` → 4종 `MeleeDodgeChance*Offset` |
| **TraitDef** | 특성 보정 | `Traits_Singular.xml` → `statOffsets: MeleeDodgeChance 15` (Nimble 등) |
| **HediffDef** | 헤디프 보정 | Ideology `Hediffs_Casts.xml` → `statOffsets: MeleeDodgeChance 8` |
| **ThingDef (장비)** | 장비 보정 | `equippedStatOffsets` → 방패 -0.2, 갑옷 -0.1 등 |
| **ThingDef (종족)** | 종족 기본값 | `statBases` → Ratkin `MeleeDodgeChance 1.15` (확인 필요) |
| **PreceptDef** | 이데올로기 교리 | `Precepts_Lighting.xml` → 4종 오프셋 수치 |
| **AbilityDef** | 능력 보정 | Ratkin `AbilityDefs.xml` → `statOffsets 0.15` |
| **ManeuverDef** | 전투 로그 규칙 | `Maneuvers.xml` → `combatLogRulesDodge` |
| **RulePackDef** | 전투 텍스트 | `RulePacks_Maneuvers.xml` → `Maneuver_*_MeleeDodge` |
| **SoundDef** | 사운드 | `Interact_Oneshots_Melee.xml` → `Pawn_MeleeDodge` |
| **RaceProperties** | 종족 사운드 | `soundMeleeDodge` 필드 |

### 7.2 코드 (C#)

| 파일 | 역할 |
|------|------|
| **StatDefOf.cs** | `public static StatDef MeleeDodgeChance;` 정적 참조 |
| **StatWorker.cs** | 범용 스탯 계산 엔진 (skillNeed, capacity, curve 등) |
| **StatDisplayOrder.cs** | UI 정렬 상수 `Pawn_DarknessMeleeDodgeChance = 4101` |
| **Verb_MeleeAttack.cs** | `GetDodgeChance()` — 판정 + 사용 |
| **DarknessCombatUtility.cs** | 이데올로기 어둠 보정 UI `StatDrawEntry` 생성 |
| **Pawn.cs** | `SpecialDisplayStats()` → DarknessCombatUtility 호출 |
| **ManeuverDef.cs** | `combatLogRulesDodge` 필드 |
| **RaceProperties.cs** | `soundMeleeDodge` 필드 |
| **BattleLogEntry_MeleeCombat.cs** | 전투 로그 항목 (dodge도 이 클래스 사용) |

### 7.3 Ratkin 모드 코드

| 파일 | 역할 |
|------|------|
| `Verb_MeleeExplosion.cs` | 폭발 근접 공격의 dodge 판정 |
| `Verb_GunlanceFiring.cs` | 건랜스의 dodge 판정 |

### 7.4 Ratkin 모드 Def에서 MeleeDodgeChance 참조

| 파일 | 내용 |
|------|------|
| `Apparel_Shield.xml` | `equippedStatOffsets: -0.2` |
| `Apparel_Util.xml` | `-0.15`, `-0.1`, `-0.2` (아이템별) |
| `Apparel_Various.xml` | `+0.05`, `+2`, `+0.05` |
| `Apparel_Armor.xml` | `-0.1` |
| `Races_Rakinlike.xml` | `statBases` 또는 `statFactors` |
| `AbilityDefs.xml` | `statOffsets: +0.15` |

---

## 8. 비교: MeleeHitChance vs MeleeDodgeChance

| 항목 | MeleeHitChance | MeleeDodgeChance |
|------|----------------|------------------|
| **적용 대상** | 공격자 | 방어자 |
| **showOnPawns** | `false` (숨김) | `true` (기본, 표시) |
| **noSkillOffset** | 4 | 0 |
| **skillNeedOffsets** | Melee ×1 | Melee ×1 |
| **capacityOffsets** | Manipulation×12(max1.5), Sight×12(max1.5) | Moving×18, Sight×8(max1.4) |
| **postProcessCurve** | 7포인트 (-20→5% ~ 60→98%) | 3포인트 (5→0% ~ 60→50%) |
| **parts** | `StatPart_Age` (나이 보정) | 없음 |
| **finalizeEquippedStatOffset** | `false` | (기본 true) |

---

## 9. 새 스탯 만들기 체크리스트

### Phase 1: 정의

- [ ] `StatDef` XML 작성 (Ratkin Defs 디렉토리에 새 파일 또는 기존 파일에 추가)
- [ ] `defName`, `label`, `description`, `category` 설정
- [ ] `skillNeedOffsets` 또는 `skillNeedFactors` 설정 (스킬 연동)
- [ ] `capacityOffsets` 설정 (신체 능력치 연동)
- [ ] `postProcessCurve` 설계 (성장 곡선)
- [ ] UI 필드: `toStringStyle`, `displayPriorityInCategory`, `showOnPawns` 등
- [ ] `defaultBaseValue`, `minValue`, `maxValue` 설정

### Phase 2: C# 코드

- [ ] (모드 내) `StatDefOf` 유사 클래스에 정적 참조 추가, 또는 `DefDatabase<StatDef>.GetNamed()` 사용
- [ ] 판정 로직 작성 — 어디서 이 스탯을 읽어서 쓸 것인가
- [ ] 기존 Verb에 영향이 있다면 해당 Verb 수정

### Phase 3: 데이터 연동

- [ ] 장비/갑옷에 `equippedStatOffsets` 추가
- [ ] 종족에 `statBases` 설정
- [ ] 필요시 특성/헤디프/유전자에 보정 추가

### Phase 4: 표시/로그

- [ ] 전투 결과 텍스트 (TextMote 등) — 필요시
- [ ] 전투 로그 (ManeuverDef, RulePackDef) — 필요시
- [ ] 사운드 — 필요시
- [ ] 번역 키 — 필요시

---

## 10. 참고 소스 파일 인덱스

| 파일 | 핵심 행 |
|------|---------|
| `RimworldData/Core/Defs/Stats/Stats_Pawns_Combat.xml` | 122~160 (MeleeDodgeChance), 63~120 (MeleeHitChance) |
| `RimworldSource/RimWorld/StatWorker.cs` | 109~180 (GetValueUnfinalized), 370~410 (곱연산), 922~958 (FinalizeValue) |
| `RimworldSource/RimWorld/StatDef.cs` | 전체 (필드 정의) |
| `RimworldSource/RimWorld/StatDefOf.cs` | 318~337 (MeleeDodgeChance 관련) |
| `RimworldSource/RimWorld/Verb_MeleeAttack.cs` | 44~114 (TryCastShot), 191~232 (GetDodgeChance) |
| `RimworldSource/RimWorld/SkillNeed_BaseBonus.cs` | 13~26 (ValueFor, ValueAtLevel) |
| `RimworldSource/RimWorld/PawnCapacityOffset.cs` | 15~18 (GetOffset) |
| `RimworldSource/DarknessCombatUtility.cs` | 120~151 (GetStatEntriesForPawn) |
| `Project/1.6/Source/RatkinGuerrilla/Verb_MeleeExplosion.cs` | 63~89, 234~255 |
| `Project/1.6/Source/Gunlance/Verb_GunlanceFiring.cs` | 268~289 |

---

## 기존 관련 보고서

- `Report/131_RimWorld_MeleeHitChance_DodgeChance_Rules.md` — 명중/회피 규칙 요약
- `Report/126_MeleeDodgeChance_postProcessCurve_Simulation_Report.md` — 커브 시뮬레이션 상세
- `Report/116_Melee_Skill_Level_Curve_Analysis_Report.md` — 스킬 레벨별 분석
- `Report/117_postProcessCurve_Concise_Report.md` — postProcessCurve 계산 플로우
