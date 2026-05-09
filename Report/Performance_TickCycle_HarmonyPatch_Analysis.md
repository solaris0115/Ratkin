# Performance Tick Cycle Harmony Patch Analysis
<!-- Tags: Performance Optimization Tick Update Harmony Patch Lag FPS TPS Slowdown MoraleBooster RatHolicGun ShieldPatch Projectile -->

## 개요

랫킨 모드 1.6 버전에서 유저들이 보고하는 성능 저하(느려짐) 원인을 파악하기 위해, **업데이트 사이클(Tick)에서 실행되는 코드**와 **Harmony 패치 중 업데이트 빈도에 영향을 주는 것**을 분석한 결과.

분석 범위: `Project/1.6/Source/` 내 144개 .cs 파일 전체

---

## 위험도 분류

### RED — 위험도 높음 (성능 저하 주범 후보)

#### 1. `HediffComp_GiveHediffsInRange.CompPostTick` (MoraleBooster)

| 항목 | 내용 |
|------|------|
| 파일 | `Source/MoraleBooster/HediffComp_GiveHediffsInRange.cs` |
| 호출 빈도 | **매 틱** (CompPostTick) |
| 동작 | mote 유지(매 틱) + **10틱마다** 맵 전체/같은 진영 폰 리스트 순회 → 범위 내 인간형 폰에 버프 Hediff 부여/갱신 |
| 비용 모델 | `O(맵 폰 수) × 해당 Hediff 보유 폰 수` (10틱 간격) |
| 문제점 | - `CompPostTick` 자체가 매 틱 호출되어 mote 유지만으로도 비용 발생<br>- 10틱마다 `SpawnedPawnsInFaction()` 또는 `AllPawnsSpawned` 전체 순회<br>- 각 폰에 대해 `FindExistingMoraleBuff` → Hediff 리스트 순회<br>- `TryGetComp<HediffComp_Disappears>()` 호출 |
| 개선안 | - `CompPostTickInterval`로 전환 (예: 30~60틱)<br>- mote 유지는 Draw 경로(`PostDraw`)로 이동<br>- 범위 내 폰 캐싱 (dirty flag 패턴) |

#### 2. `HediffComp_RatHolicGun.CompPostTick` (RatHolicGun)

| 항목 | 내용 |
|------|------|
| 파일 | `Source/RatHolicGun/Comp_RatHolicGun.cs` |
| 호출 빈도 | **매 틱** (CompPostTick) |
| 동작 | 무기 장착 확인 → stance 확인 → 타겟 비교 → 조건 충족 시 Hediff 제거 |
| 비용 모델 | `O(장비 수) × 해당 무기 사용 폰 수` (매 틱) |
| 문제점 | - `GetRatHolicGun()`: `AllEquipmentListForReading` 매 틱 순회<br>- `GetCurrentAimingTarget()`: stance 캐스팅 + `CompEquippable` 조회 + `AllVerbs` 순회<br>- 타겟 미변경 시에도 전체 로직 실행 |
| 개선안 | - `CompPostTickInterval` (5~10틱) 전환 — 타겟 변경은 프레임 단위 감지 불필요<br>- 무기 참조 캐싱 (장착/해제 시에만 갱신)<br>- early return 강화: `!Drafted && !InCombat`이면 스킵 |

#### 3. `ThingDef.IsShieldThatBlocksRanged` Postfix (ShieldPatch)

| 항목 | 내용 |
|------|------|
| 파일 | `Source/ShieldOfRatkinia/ShieldPatch.cs` |
| 패치 종류 | Harmony Postfix on property getter |
| 호출 빈도 | **높음** — Alert, WorkGiver_HunterHunt, 전투 판정 등에서 반복 호출 |
| 동작 | 바닐라 결과가 false일 때 `HasComp(typeof(CompRKShield))` + `GetCompProperties<>()` 실행 |
| 비용 모델 | 모든 ThingDef에 대해 호출됨 (CompRKShield 없는 아이템 포함) |
| 문제점 | - `HasComp(typeof(...))`: ThingDef.comps 리스트 순회 + 타입 비교<br>- `GetCompProperties<>()`: 동일 리스트 재순회<br>- 랫킨 방패가 아닌 모든 장비/아이템에도 불필요하게 실행 |
| 개선안 | - 게임 시작 시 `static HashSet<ThingDef>` 캐시 구축 → 패치 내에서 `Contains()` 체크만<br>- 또는 Def 로딩 단계에서 해당 ThingDef에 CompShield를 직접 추가하여 패치 자체 제거 |

---

### YELLOW — 위험도 중간

#### 4. `Projectile_ProximityBurst` — MaxTickIntervalRate = 1

| 항목 | 내용 |
|------|------|
| 파일 | `Source/BFR/Projectile_ProximityBurst.cs` |
| 문제점 | 틱 스킵 금지 → 동시 다수 발사체 존재 시 프레임 드롭 |
| 영향 범위 | BFR 무기 대규모 전투 시 |

#### 5. `Projectile_BallistaBoltAP` — UpdateRateTicks = 1

| 항목 | 내용 |
|------|------|
| 파일 | `Source/ShieldOfRatkinia/Ballista.cs` |
| 문제점 | 매 틱 `Tick()` + 서스테이너 `Maintain()` + 1틱 간격 갱신 |
| 영향 범위 | 발리스타 연사 시 |

#### 6. `CompPowerPlantHamsterWheel.CompTick`

| 항목 | 내용 |
|------|------|
| 파일 | `Source/HamsterWheel/PowerComp.cs` |
| 문제점 | 매 틱 스핀/파워 계산. 건물 수 비례 |
| 영향 범위 | 햄스터 휠 다수 배치 시 |

#### 7. `GuerrillaTunnelSpawner.Tick`

| 항목 | 내용 |
|------|------|
| 파일 | `Source/RatkinGuerrilla/TunnelSpawner.cs` |
| 문제점 | 매 틱 `Rand.MTBEventOccurs` + `CellFinder.TryFindRandomReachableNearbyCell` |
| 영향 범위 | 게릴라 터널 스폰 이벤트 중에만 → 지속 부하 아님 |

#### 8. `Verb_MeleeAttack.TryCastShot` Postfix (LanceCharge)

| 항목 | 내용 |
|------|------|
| 파일 | `Source/HeavyLanceCharge/LanceChargeMomentumPatch.cs` |
| 문제점 | **모든** 근접 공격에 postfix 적용 (랫킨 무관 폰 포함) |
| 비용 | hediff 1개 조회 수준이라 단건은 가볍지만 대규모 전투 시 누적 |
| 개선안 | Prefix로 전환 후 해당 hediff 없으면 즉시 return, 또는 race 체크 추가 |

#### 9. `NegativeInteractionUtility.NegativeInteractionChanceFactor` Prefix (Priest)

| 항목 | 내용 |
|------|------|
| 파일 | `Source/Priest/PriestPatch.cs` |
| 문제점 | 모든 부정적 사교 상호작용 평가에 prefix 실행 |
| 비용 | `initiator.story?.Adulthood` 비교 1회 → 매우 가벼움 |
| 판단 | 실질 영향 미미하지만 식민지 대규모화 시 호출 빈도 증가 |

---

### GREEN — 위험도 낮음 / 개발 전용

| # | 코드 | 비고 |
|---|------|------|
| 10 | `Comp_DPSDisplay.CompTick` | Def 참조 없음 → 실제 게임에 미적용. Debug 빌드 주의 |
| 11 | `Patch_InfoCard_*` | `RATKIN_DEV_FEATURES` 조건부 컴파일 → Release 미포함 |
| 12 | `SPDestory.cs` HarmonyPatch | `PatchAll` 미호출 → 실제 적용 안 됨 |
| 13 | WanderingTrader 패치들 | 이벤트/UI 시점에만 호출 → 틱 무관 |
| 14 | `RaceSettingsUniversalBodyAddonCanonicalRestore` | `Game.FinalizeInit` 1회 → 무관 |

---

## Harmony 패치 전체 목록 (1.6 활성)

| # | 타깃 | 종류 | 파일 | 틱 영향 |
|---|------|------|------|---------|
| 1 | `EquipmentUtility.CanEquip` | Postfix | StaminaShield/ | 낮음 |
| 2 | `ThingDef.IsShieldThatBlocksRanged` | Postfix | ShieldOfRatkinia/ | **높음** |
| 3 | `WorldPawns.PassToWorld` | Prefix | WanderingTrader/ | 낮음 |
| 4 | `FloatMenuOptionProvider_Trade.GetOptionsFor` | Prefix | WanderingTrader/ | 낮음 |
| 5 | `Faction.Notify_MemberCaptured` | Postfix | WanderingTrader/ | 낮음 |
| 6 | `PawnGenerator.GeneratePawn` | Postfix | Priest/ | 낮음 |
| 7 | `Pawn_AbilityTracker.ExposeData` | Postfix | Priest/ | 낮음 |
| 8 | `NegativeInteractionUtility.NegativeInteractionChanceFactor` | Prefix | Priest/ | 중간 |
| 9 | `Verb_MeleeAttack.TryCastShot` | Postfix | HeavyLanceCharge/ | 중간 |
| 10 | `CompEquippable.CompGetEquippedGizmosExtra` | Postfix ×2 | BFR/, PulseRifle/ | 낮음 (UI) |
| 11 | `Game.FinalizeInit` | Postfix | AlienRaceCompat/ | 낮음 (1회) |
| 12 | `MainTabsRoot.HandleLowPriorityShortcuts` | Postfix | (Dev only) | - |
| 13 | `WindowStack.Add` / `Selector.Select` | Prefix/Postfix | (Dev only) | - |

---

## 우선 개선 순위

1. **`HediffComp_GiveHediffsInRange`** → `CompPostTickInterval` 전환 + mote를 Draw로 분리
2. **`HediffComp_RatHolicGun`** → 틱 간격 늘리기 + 무기 참조 캐싱
3. **`IsShieldThatBlocksRanged` Postfix** → HashSet 캐싱 또는 Def 수정으로 패치 제거
4. **`Verb_MeleeAttack.TryCastShot` Postfix** → race/hediff 선행 체크 추가
5. **발사체 1틱 간격** → 정말 필요한지 재검토 (2~3틱도 시각적 차이 미미)

---

## 부록: Tick 구조 참고

- `CompPostTick`: 매 게임 틱(60Hz) 호출 — **가장 비싼 경로**
- `CompPostTickInterval`: Def에서 `compProperties.tickIntervalTicks` 설정 가능
- `TickRare`: 250틱마다
- `TickLong`: 2000틱마다
- 발사체 `TickInterval(delta)`: `UpdateRateTicks` 속성으로 간격 제어

---

---

## 유랑단(WanderingCaravan) 등장 시 성능 저하 분석

유저 보고: "유랑단만 등장하면 느려진다"

### 근본 원인: 과다한 인간형 폰 스폰

| | 바닐라 상인 | 랫킨 유랑단 |
|---|---|---|
| 리더/상인 | 1 | 1 |
| 호위 | 2-4 | 3-5 (포인트 기반) |
| 인간형 추가 (settlers) | 0 | **최대 5** |
| 짐꾼 동물 | 2-4 | 4-7 |
| **총 인간형** | **3-5** | **9-11** |
| **총합** | 7-12 | **13-18** |

인간형 폰의 사교 상호작용은 **N² 스케일** → 인간형 2~3배 = 체감 부하 4~9배

### 구조적 문제

#### 1. 무제한 풀 성장 (maxPoolSize=-1, expireAfterAppearances=-1)

- 풀에 만료 없이 유랑민 축적 → WorldPawns KeepForever 누적
- 장기 게임에서 풀 크기 무한 증가 → `HasRosterOrPoolPawnsSpawned()` 순회 비용 증가
- WorldPawns 자체 틱/메모리/세이브 부담 증가

#### 2. LINQ Concat 할당 (`HasRosterOrPoolPawnsSpawned`)

```csharp
// 매 호출마다 IEnumerator 4개 할당 → GC 압박
foreach (Pawn p in rosterLeader.Concat(rosterGuards).Concat(rosterSettlers).Concat(settlerPool))
```

- 스토리텔러 1000틱 간격 + PawnLost 이벤트마다 호출
- 풀이 클수록 순회 시간 + 할당 비용 증가

#### 3. `HasWanderingCaravanActiveAnywhere()` 전체 맵 스캔

- ALL maps → ALL lords + ALL spawned pawns 순회
- `Notify_PawnLost`에서 `TryResetCaravanFactionToNeutralIfCleared()` 호출 → 퇴장 시 폰별로 전체 스캔

#### 4. 인간형 폰 증가의 파급 효과 (N² 비용 영역)

- **사교 시스템**: 모든 인간형 쌍에 대해 상호작용 평가 → `NegativeInteractionChanceFactor` Prefix 호출 급증
- **Alert**: `Alert_ShieldUserHasRangedWeapon` 등이 추가 폰 apparel 순회 → `IsShieldThatBlocksRanged` Postfix 추가 실행
- **경로 탐색**: Travel→Idle 이동 시 10+명 동시 경로 요청
- **렌더링**: AlienRace 커스텀 렌더노드 + 방패 `CompDrawWornExtras` 프레임당 추가

### 개선 권장

| 우선순위 | 대상 | 방안 |
|----------|------|------|
| 1 | settler 스폰 수 | `maxRosterCount` 5→3 또는 동적 제한 (식민지 규모 비례) |
| 2 | 풀 만료 | `maxPoolSize=12`, `expireAfterAppearances=4` 설정 |
| 3 | `HasRosterOrPoolPawnsSpawned` | LINQ Concat → for 루프 4개 (GC 0) |
| 4 | `HasWanderingCaravanActiveAnywhere` | bool 캐시 + dirty flag (1000틱 유효, PawnLost에서 invalidate) |
| 5 | guard 수 조절 | points 상한 낮추기 → 총 인간형 폰 줄이기 |
| 6 | WorldPawns 정리 | 퇴장 시 roster에 없는 폰은 KeepForever 대신 Decide로 전환 |

---

*분석일: 2026-05-08*
*유랑단 분석 추가: 2026-05-09*
