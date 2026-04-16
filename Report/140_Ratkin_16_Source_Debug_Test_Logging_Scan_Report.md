# Ratkin 1.6 Source Debug Test Logging Scan Report

Tags: DebugAction DevMode Log.Message Log.Warning Log.Error AutoTests TestSetup StockGenerator_UniqueWeapon RKSpawnEquipmentConfig 미사용 NUnit 단위테스트 XML ThingDef Ethereal Dummy 벽

## 요약

- **정식 단위 테스트(NUnit/xUnit/MSTest 등)**: `Project/1.6/Source`에 **없음** (`NewRatkin.csproj`는 `**\*.cs` 컴파일만, 테스트 참조 없음).
- **테스트·셋업 성격 코드**: 림월드 **개발자 모드**의 `[DebugAction(...)]`로 노출되는 정적 메서드들이 전부. 일반 플레이 UI 경로와 무관.
- **상시 로그(디버깅 잔재 의심)**: `StockGenerator_UniqueWeapon.GenerateThings`가 **DevMode 분기 없이** `Log.Message` 다수 호출 → 상인/재고 생성 시 로그 스팸 가능.
- **미사용 C#**: XML Def와 “연결” 여부는 컴파일 타임에 검증되지 않음. 본 스캔에서는 **고아 클래스**까지 자동 추적하지 않음(별도 정적 분석 필요).

---

## 1. DebugAction(개발자 모드 전용)

| 파일 | 메뉴/이름(요지) |
|------|------------------|
| `DebugActions_TestSetup.cs` | Ratkin: `[Test1]` 의복+작업대+무기 스폰 / `[Test2]` 랫킨 인시던트 일괄 발동 |
| `DebugActions.cs` | Mods: 비판매 RK 장비 목록 로그 / Ratkin: 의복 테스트, 니즈 채우기, 식민지별 RK 장비 스폰, 부위 제거, 아기 생성 1000회 통계, 모든 PawnKind 25명씩 스폰 등 |
| `AutoTests/AutoTests.cs` | Mods: `Make RatkinTest (Full)` / `(ForEach)` — 맵 클리어 후 대량 랫킨 스폰 (`AutoTests_ColonyMaker` 동일 파일 내) |

개발자 모드가 꺼져 있으면 플레이어가 이 경로로 진입하기 어렵다.

---

## 2. Prefs.DevMode / 디버깅 분기

- `ShieldOfRatkinia/ApparelShield.cs`: `Prefs.DevMode`일 때만 방패 막기 확률 `Log.Message`.
- `ShieldOfRatkinia/CompShieldDeflect.cs`: `Prefs.DevMode`일 때만 관통/블록 상세 `Log.Message`.
- `RatkinGuerrilla/IncidentWorker_AfterRaid.cs`: `Prefs.DevMode`가 **아닐 때** 레이드 전략을 `ImmediateAttack`으로 고정(동작 차이 — 디버그 편의에 가까운 분기).

---

## 3. Log.Warning / Log.Error (게임 경고·오류 처리)

정상 플로우·데이터 검증용으로 보이는 것과 구분 어렵지만, **테스트 전용은 아님**:  
`Command_AbilityPrayService`, `HediffComp_GiveHediffsInRange`, `Building_Tunnel`, `Verb_MeleeExplosion`, `IncidentWorker_WandererJoin`, `EquipmentUtility_CanEquip_Patch`, `IncidentWorker_AfterRaid` 등.

---

## 4. 로깅 잔재(상시 `Log.Message`)

- **`ShieldOfRatkinia/StockGenerator_UniqueWeapon.cs`**: `GenerateThings` 전 구간에서 **조건 없는** `Log.Message` (Odyssey 상거래/재고 생성 시 반복 호출 가능). DevMode 가드 없음 → **일반 유저 로그에도 노출** 가능성이 큼.

그 외 `DebugActions*.cs`, `DebugActions_TestSetup`의 `[Test1]/[Test2]` 로그는 디버그 액션 실행 시에만.

---

## 5. 디버그 전용 설정·UI

- **`RKSpawnEquipmentConfig.cs`**: `GenFilePaths.ConfigFolderPath` 아래 `Ratkin_DebugSpawnConfig.xml` — “RK Equipment Spawn” 디버그 도구용 품질·체크리스트 저장.
- **`Dialog_RKSpawnEquipmentConfig.cs`**: 위 설정 편집 UI(모드 어셈블리 기준 ModContentPack 탐색).

---

## 6. 주석 처리된 로그

- `AutoTests/AutoTests.cs`: `//Log.Message(...)` 형태 주석 다수(과거 디버그 출력).

---

## 7. 범위 밖

- **`RimworldSource/`**: 바닐라 소스 내 `Autotests_*`, `PerfTest` 등은 **Ratkin 모드 코드가 아님**.
- **Def/XML 미참조 파일**: 전수 orphan 검사는 이번에 수행하지 않음.

---

## 8. XML(Def) 쪽 “개발자용 벽” 유사 항목 스캔 (2026-04 보강)

`Project/1.6/Defs`·`Patches`·`1.5/Defs`에서 `debug` / `dev` / `testwall` / `dummy` / `invisible` / `Wall` 전용 **개발자 벽 ThingDef**는 **발견되지 않음**.

- **에테리얼·실시간만 그리는 오브젝트** (맵 장애물 “벽”이 아니라 이벤트/연출용에 가까움): `RK_GuerrillaTunnelSpawner` (`ParentName="EtherealThingBase"`), `Mote_CountDown` (투명 텍스처 모트), `MinifiedEMP` (`drawerType` RealtimeOnly 등).
- **기술용 더미 이름**: `RK_Bullet_SectorShot_Dummy` — `Verb_SectorShot.cs`에서 `ThingDef.Named`로 참조. 저장소 내 **ThingDef 본문 XML은 검색되지 않음**(한글 `DefInjected` 라벨만 존재). 게임 내 다른 모드/누락 Def와 겹치면 로드 이슈 가능.
- **바닐라 스타일 메타 필드**: `AbilityDefs.xml`의 `<debugLabelExtra>` (능력 디버그 UI용, 콘텐츠 “벽” 아님).
- **패치의 `PatchOperationTest`**: xpath 조건용 클래스명이지 테스트 맵용 XML이 아님.
- **주석 TODO**: `Cultures.xml`, `ResearchProjects.xml`, `AlienRaceSettings.xml` 등 — 미완 작업 메모.
