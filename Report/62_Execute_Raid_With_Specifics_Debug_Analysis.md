# Execute Raid With Specifics 디버깅 기능 분석

**작성일**: 2024-12-30  
**분석 목적**: RimWorld의 "Execute raid with specifics" 디버그 액션 동작 방식 및 플로우 분석

---

## 1. 개요

**위치**: `RimworldSource/Verse/DebugActionsIncidents.cs`  
**메서드**: `ExecuteRaidWithSpecifics()` (165-273줄)  
**디버그 액션 경로**: `Incidents` → `Execute raid with specifics...`

개발자 모드에서 레이드를 세부 설정으로 테스트할 수 있는 디버깅 기능입니다.

---

## 2. 전체 플로우

```
디버그 메뉴 실행
    ↓
ExecuteRaidWithSpecifics() 호출
    ↓
StorytellerComp에서 기본 파라미터 생성
    ↓
[1단계] Faction 선택 메뉴 표시
    ↓
[2단계] Points 선택 메뉴 표시
    ↓
[3단계] RaidStrategy 선택 메뉴 표시
    ↓
[4단계] PawnsArrivalMode 선택 메뉴 표시
    ↓
[5단계] (Biotech 활성 시) RaidAgeRestriction 선택 메뉴 표시
    ↓
DoRaid(parms) 호출
    ↓
IncidentDefOf.RaidEnemy/RaidFriendly.Worker.TryExecute(parms)
```

### 선택 가능한 옵션

#### RaidStrategy 및 PawnsArrivalMode

**ImmediateAttack**: 즉시 공격
- 등장 후 지연 없이 바로 공격을 시작
- 가장 기본적인 레이드 전략으로, 모든 세력이 사용 가능
- 포인트 보정 없음 (1.0 배율)
- 도착 방식: EdgeDrop, EdgeWalkIn, CenterDrop, RandomDrop, EdgeDropGroups, EdgeWalkInGroups, EdgeWalkInDarkness (Anomaly DLC)

**ImmediateAttackFriendly**: 즉시 도움 제공 (우호 세력)
- ImmediateAttack을 상속받은 우호 세력용 전략
- 파운들이 음식을 가져올 수 있음 (`pawnsCanBringFood`)
- 플레이어를 도와주는 목적으로 사용
- 도착 방식: EdgeDrop, EdgeWalkIn, CenterDrop, RandomDrop, EdgeDropGroups, EdgeWalkInGroups, EdgeWalkInDarkness (Anomaly DLC)

**ImmediateAttackSmart**: 즉시 공격 (똑똑한 전술 사용)
- 등장 후 즉시 공격하지만, AI가 더 똑똑하게 행동
- 터렛의 사격 범위를 피하고 일부 함정을 감지함
- 포인트 1000 이상에서만 선택 가능 (선택 가중치 0.5)
- 포인트 보정: 0.95 배율 (약간 낮은 위협도)
- 도착 방식: EdgeDrop, EdgeWalkIn, CenterDrop, RandomDrop, EdgeDropGroups, EdgeWalkInGroups

**StageThenAttack**: 준비 후 공격
- 등장 후 일정 시간 준비 단계를 거친 후 공격 시작
- 플레이어는 방어 준비를 하거나 선제 공격 가능
- 포인트 보정 없음 (1.0 배율)
- 도착 방식: EdgeDrop, EdgeWalkIn, EdgeDropGroups, EdgeWalkInGroups

**EmergeFromWater**: 물에서 등장
- 물 타일에서 등장하는 특수 전략
- 포인트 보정: 0.8 배율 (낮은 위협도)
- 메카노이드는 포인트 500 이상에서만 사용 가능
- 도착 방식: EmergeFromWater (고정)

**Siege**: 포격 공성전
- 등장 후 모르타르를 설치하고 원거리 포격으로 공성
- 최소 4명의 파운 필요
- 포인트 보정: 0.80~0.65 배율 (포인트에 따라 감소)
- 포인트 500 이상에서만 선택 가능
- 표면 레이어에서만 사용 가능
- 도착 방식: EdgeDrop, EdgeWalkIn

**ImmediateAttackSappers**: 공병을 이용한 터널 공격
- 등장 후 즉시 공격하지만, 공병(Sapper)을 포함하여 방어선을 우회
- 공병이 벽을 뚫고 터널을 만들어 진입 경로 확보
- 최소 2명의 파운 필요
- 포인트 보정: 0.85~0.60 배율 (포인트에 따라 감소)
- 포인트 700 이상에서만 선택 가능
- 도착 방식: EdgeDrop, EdgeWalkIn, EdgeDropGroups, EdgeWalkInGroups

**ImmediateAttackBreaching**: 벽 돌파 공격
- 등장 후 즉시 공격하며, 벽을 직접 파괴하며 진입
- 자신만의 경로를 결정하여 벽을 부수고 들어옴
- 포인트 보정: 1.0~0.5 배율 (포인트에 따라 감소)
- 포인트 700 이상에서만 선택 가능
- 도착 방식: EdgeWalkIn (고정)

**ImmediateAttackBreachingSmart**: 벽 돌파 공격 (똑똑한 전술 사용)
- ImmediateAttackBreaching과 동일하지만 AI가 더 똑똑함
- 터렛의 사격 범위를 피하고 함정을 감지함
- 벽을 부수며 진입하되 더 효율적인 경로 선택
- 도착 방식: EdgeWalkIn (고정)

**PsychicRitualSiege** (Anomaly DLC): 사이킥 의식 공성전
- 등장 후 사이킥 의식을 수행하며 공성전 전개
- 의식이 완료되면 여러 번 반복 수행 가능
- 파운들이 음식을 가져올 수 있음
- 포인트 보정: 0.7~0.5 배율 (포인트에 따라 감소)
- 도착 방식: EdgeWalkIn (고정)

**ShamblerAssault** (Anomaly DLC): 좀비 시체 무리 공격
- 휘청이며 썩어가는 시체들이 무리로 공격
- 이상한 에너지로 움직이며, 시간이 지나면 스스로 무너짐
- 포인트 보정 없음 (1.0 배율)
- 도착 방식: EdgeWalkInDistributedGroups (고정)

---

## 3. 상세 단계별 동작

### 3.1 초기화
- StorytellerComp에서 기본 `IncidentParms` 생성
- `parms.forced = true` 설정으로 강제 실행

### 3.2 Faction 선택
- 모든 Faction 목록 표시
- `FactionCanBeGroupSource()` 체크로 사용 불가능한 Faction에 `[NO]` 표시

### 3.3 Points 선택
- Extended 모드: 20~5000 (세밀한 간격) + 6000~10000
- 20~90: 10 간격, 100~475: 25 간격, 500~1450: 50 간격, 1500~5000: 100 간격

### 3.4 RaidStrategy 선택
- 모든 `RaidStrategyDef` 목록 표시
- `CanUseWith()` 체크로 사용 불가능한 전략에 `[NO]` 표시

### 3.5 PawnsArrivalMode 선택
- 모든 `PawnsArrivalModeDef` 목록 표시
- 선택한 `RaidStrategy`의 `arriveModes`와 호환성 체크
- `-Random-` 옵션 제공 (선택 시 랜덤)

### 3.6 RaidAgeRestriction 선택 (Biotech DLC)
- Biotech DLC 활성 시에만 표시
- `-Random-` 옵션 제공

### 3.7 최종 실행
- Faction의 적대 여부에 따라 `RaidEnemy` 또는 `RaidFriendly` 선택
- `IncidentWorker.TryExecute()` 호출

---

## 4. 설정 가능한 파라미터

| 파라미터 | 타입 | 설명 |
|---------|------|------|
| `faction` | `Faction` | 레이드를 수행할 세력 |
| `points` | `float` | 위협 포인트 (20~10000) |
| `raidStrategy` | `RaidStrategyDef` | 레이드 전략 (ImmediateAttack, Siege 등) |
| `raidArrivalMode` | `PawnsArrivalModeDef` | 등장 방식 (EdgeWalkIn, DropPod 등) |
| `raidAgeRestriction` | `RaidAgeRestrictionDef` | 연령 제한 (Biotech DLC) |

---

## 5. 유효성 검사

각 단계에서 다음 검증 수행:

1. **Faction**: `FactionCanBeGroupSource()` 체크
2. **RaidStrategy**: `CanUseWith(parms, PawnGroupKindDefOf.Combat)` 체크
3. **PawnsArrivalMode**: 
   - `CanUseWith(parms)` 체크
   - 선택한 `RaidStrategy.arriveModes` 포함 여부 체크
4. **RaidAgeRestriction**: `CanUseWith(parms)` 체크

검증 실패 시 메뉴 항목에 `[NO]` 표시

---

## 6. PointsOptions 상세

**Extended 모드 (true)**:
- 20~90: 10 간격
- 100~475: 25 간격
- 500~1450: 50 간격
- 1500~5000: 100 간격
- 6000~10000: 1000 간격

---

## 7. 사용 예시

### 7.1 기본 사용법

1. 개발자 모드 활성화 (`Prefs.DevMode = true`)
2. 게임 내 맵에서 플레이 중
3. 디버그 메뉴 열기
4. `Incidents` → `Execute raid with specifics...` 선택
5. 단계별로 선택:
   - Faction 선택
   - Points 선택
   - RaidStrategy 선택
   - PawnsArrivalMode 선택
   - (Biotech) RaidAgeRestriction 선택

### 7.2 테스트 시나리오

**시나리오**: Ratkin 세력의 500 포인트 즉시 공격 레이드 테스트
- Faction: Rakinia
- Points: 500
- RaidStrategy: ImmediateAttack
- PawnsArrivalMode: EdgeWalkIn

---

## 8. 관련 클래스

| 클래스 | 용도 |
|--------|------|
| `DebugActionsIncidents` | 디버그 액션 정의 |
| `DebugActionsUtility` | Points 옵션 등 유틸리티 |
| `IncidentParms` | 이벤트 파라미터 |
| `IncidentWorker_Raid` | 레이드 실행 로직 |
| `Dialog_DebugOptionListLister` | 디버그 메뉴 UI |

---

## 9. 주의사항

1. **개발자 모드 필수**: `Prefs.DevMode` 활성화 필요
2. **맵 플레이 중**: `AllowedGameStates.PlayingOnMap` 상태에서만 사용 가능
3. **유효성 검사**: `[NO]` 표시된 옵션은 선택 불가능
4. **강제 실행**: `parms.forced = true`로 설정되어 일반 발생 조건 무시

---

## 10. 활용 방안

### 10.1 Ratkin 프로젝트 적용

- **커스텀 RaidStrategy 테스트**: Ratkin 전용 전략 검증
- **Faction별 레이드 밸런스 테스트**: Rakinia 세력 레이드 포인트 조정
- **PawnKindDef 검증**: 특정 PawnKindDef가 레이드에 포함되는지 확인
- **ArrivalMode 테스트**: 커스텀 등장 방식 검증

### 10.2 디버깅 팁

- Points를 낮게 설정하여 소규모 레이드로 빠른 테스트
- `[NO]` 표시 원인 파악을 위해 관련 Def 설정 확인
- 로그 확인: `Log.Message()`로 `IncidentParms` 내용 출력

---

## 참고 파일

- `RimworldSource/Verse/DebugActionsIncidents.cs`
- `RimworldSource/Verse/DebugActionsUtility.cs`

