# Creep Joiner(크립 조이너) 스폰 시 어빌리티 부여 보고서

**태그:** CreepJoiner CreepJoinerUtility CreepJoinerBenefitDef CreepJoinerDownsideDef Ability GainAbility Pawn_CreepJoinerTracker Anomaly 바닐라

**대상:** RimWorld 1.6(Anomaly DLC) 바닐라 `RimworldSource/RimWorld/CreepJoinerUtility.cs` 및 `RimworldData/Anomaly/Defs/CreepjoinerDefs/`

**작성 목적:** 이상·크립 조이너가 맵에 등장할 때 `AbilityDef`가 **어떤 경로로** 폰에 붙는지 정리한다. 랫킨 전용 C#이 아니라 **바닐라 Anomaly** 생성 파이프라인이다.

---

## 1. 한 줄 요약

스폰 직후 `CreepJoinerUtility.GenerateAndSpawn`이 선택된 **CreepJoinerBenefitDef** / **CreepJoinerDownsideDef**에 선언된 `abilities` 리스트를 순회하며 `pawn.abilities.GainAbility(def)`로 부여한다.

---

## 2. 호출 시점·순서

1. `PawnGenerator.GeneratePawn(..., IsCreepJoiner: true, ...)` 로 폰 생성  
2. `Pawn_CreepJoinerTracker`에 `form`, `benefit`, `downside`, `aggressive`, `rejection` 저장  
3. `ApplyExtraTraits` / `ApplyExtraHediffs` / `ApplySkillOverrides` (benefit·downside 각각)  
4. **`ApplyExtraAbilities(pawn, benefit.abilities)`**  
5. **`ApplyExtraAbilities(pawn, downside.abilities)`**  
6. 맵 스폰, `LordJob_CreepJoiner`, `Notify_Created` 등

즉, “어빌이 붙어서 스폰된다”는 **생성·트레이트/hediff/스킬 보정이 끝난 뒤**, 스폰 직전에 확정된다.

**코드 위치(레포):** `RimworldSource/RimWorld/CreepJoinerUtility.cs` — `GenerateAndSpawn` 본문 및 `ApplyExtraAbilities` private static 메서드.

---

## 3. 구현(능력 부여)

| 항목 | 내용 |
|------|------|
| **입력** | `List<AbilityDef>` (benefit 또는 downside의 `abilities`) |
| **처리** | `foreach` 로 각 `AbilityDef`에 대해 `pawn.abilities.GainAbility(def)` |
| **중복 방지** | `GainAbility` 쪽(바닐라) 동작에 따름 — 이 단계는 리스트에 있는 만큼 호출 |

`Pawn_AbilityTracker`는 인간형 등 조건에 따라 `PawnComponentsUtility`에서 생성된다. Creep joiner는 해당 경로로 트래커가 잡힌 뒤 위 호출이 실행된다.

---

## 4. benefit / downside는 어떻게 고르나

- **랜덤 이벤트 등:** `CreepJoinerUtility.GetCreepjoinerSpecifics` 또는 `GenerateAndSpawn`의 오버로드에서 `CreepJoinerFormKindDef`에 따른 `Requires` / `Excludes`, 전투력(`combatPoints`), `MinCombatPoints`, `CanOccurRandomly`, `Weight` 등을 반영해 **랜덤** 선택.  
- **퀘스트/스크립트:** `QuestNode` 계열에서 slate에 `benefit`/`downside`를 넣어 **고정**할 수 있음(파일: `QuestNode_Root_Creepjoiner_Arrival` 등 — 동일 `GenerateAndSpawn` 체인으로 수렴).

선택된 Def에 `abilities`가 비어 있으면 해당 루프는 사실상 아무것도 하지 않는다.

---

## 5. 바닐라 Def 예시 (Anomaly `Benefits.xml`)

일부 `CreepJoinerBenefitDef`만 `abilities`를 갖는다(예).

| defName | abilities (요지) |
|---------|-------------------|
| UnnaturalHealing | `UnnaturalHealing` |
| ShamblerOverlord | `ReleaseDeadlifeDust` |
| Fleshcrafter | `ShapeFlesh` |
| Alchemist | `TransmuteSteel` |
| PsychicButcher | `PsychicSlaughter` |

**바닐라 `Downsides.xml`** 은 `traits` / `hediffs` / `workerType` 위주이며, **기본 패키지에는 `abilities` 블록이 없다.** 다만 C#이 `downside.abilities`에 대해 동일하게 `ApplyExtraAbilities`를 호출하므로, **다른 모드가 `CreepJoinerDownsideDef`에 `abilities`를 추가**하면 downside에서도 능력이 붙을 수 있다.

---

## 6. 랫킨 프로젝트에서의 위치

- **팩트:** 위 로직은 **RimWorld 바닐라 소스/데이터**이며, 랫킨 `Project/1.6` C#이 Creep joiner `GainAbility`를 대체하는 전제는 없다(별도 훅을 두지 않은 한).  
- **검사 제안:** 랫킨 또는 로드 오더의 **Def 패치/추가** `CreepJoinerBenefitDef` / `CreepJoinerDownsideDef`에 `abilities`가 있으면 동일 경로로 적용되는지가 의심될 때 1차로 본다.

---

## 7. 관련 식별(플레이·디버그)

- `pawn.creepjoiner` 의 `benefit` / `downside` Def를 보면, 어떤 “패키지”에서 어빌이 왔는지(정의 기준) 역추적이 가능하다.  
- 실제 부여는 **Def의 `abilities` 필드**가 직접적 원인이다.

---

## 8. 참조 파일

| 경로 | 역할 |
|------|------|
| `RimworldSource/RimWorld/CreepJoinerUtility.cs` | `GenerateAndSpawn`, `ApplyExtraAbilities` |
| `RimworldData/Anomaly/Defs/CreepjoinerDefs/Benefits.xml` | `CreepJoinerBenefitDef` + `abilities` 예시 |
| `RimworldData/Anomaly/Defs/CreepjoinerDefs/Downsides.xml` | `CreepJoinerDownsideDef` (바닐라는 주로 hediff/trait) |
