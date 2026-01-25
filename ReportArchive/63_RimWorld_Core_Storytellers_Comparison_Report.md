# RimWorld 코어 스토리텔러 비교 분석 보고서

## 분석 대상
- **Cassandra Classic** (카산드라 클래식)
- **Phoebe Chillax** (피비 칼락스)
- **Randy Random** (랜디 랜덤)

## 공통점

### 1. 기본 구조
- 모두 `BaseStoryteller`를 상속받음
- 동일한 기본 인구 의도 곡선 및 적응 일수 시스템 공유
- 엔드게임 퀘스트, DLC 관련 이벤트 등 기본 컴포넌트 동일

### 2. 공통 컴포넌트
- **ThreatsGenerator**: RaidBeacon 대상 습격 생성 (동일 설정)
- **FactionInteraction**: RaidFriendly 이벤트 (동일 설정)
- **CategoryIndividualMTBByBiome**: 캐러밴/임시 맵 이벤트 (동일 설정)
- **RandomQuest**: 퀘스트 제공 시스템 (동일 곡선, 간격만 다름)
- **CategoryMTB**: 월드 Misc 이벤트 (동일 설정)
- **Triggered**: StrangerInBlackJoin 이벤트 (동일 설정)

### 3. 공통 설정값
- **RaidBeacon 습격**: `onDays=1.0`, `offDays=0.5`, `minSpacingDays=0.04`
- **RaidFriendly**: `baseIncidentsPerYear=15` (카산드라/피비), `10` (랜디)
- **퀘스트 수락 곡선**: `(8일, 0%) → (15일, 100%)` (카산드라/피비), 랜디도 동일
- **월드 Misc**: `mtbDays=15`, `minDaysPassed=15` (카산드라/피비), `1` (랜디)

## 차이점

### 1. 이벤트 발생 시스템

#### 카산드라 & 피비 (OnOffCycle 기반)
- **ThreatBig**: 주기적 On/Off 사이클 사용
  - 카산드라: `onDays=4.6`, `offDays=6.0`, `minDaysPassed=11.0`
  - 피비: `onDays=8.0`, `offDays=8.0`, `minDaysPassed=13.0`
- **ThreatSmall**: On/Off 사이클 사용
  - 카산드라: `onDays=4.6`, `offDays=6.0`, `minDaysPassed=11.0`
  - 피비: `onDays=8.0`, `offDays=8.0`, `minDaysPassed=13.0`
- **Misc**: MTB 기반 (`mtbDays=4.8`, `minDaysPassed=5`)
- **OrbitalTrader**: On/Off 사이클 (`onDays=7`, `offDays=8`)

#### 랜디 (RandomMain 기반)
- **모든 이벤트**: 랜덤 기반 시스템 사용
  - `mtbDays=1.35` (매우 빠른 발생)
  - `randomPointsFactorRange=0.5~1.5` (변동성 높음)
  - `maxThreatBigIntervalDays=13` (최대 간격 제한)
- **카테고리 가중치**:
  - Misc: 3.5
  - ThreatBig: 1.4
  - ThreatSmall: 0.6
  - FactionArrival: 2.4
  - OrbitalVisitor: 1.1
  - ShipChunkDrop: 0.22

### 2. 위협 발생 빈도

| 항목 | 카산드라 | 피비 | 랜디 |
|------|----------|------|------|
| ThreatBig 시작일 | 11일 | 13일 | 1일 |
| ThreatBig 주기 | 4.6일 ON / 6일 OFF | 8일 ON / 8일 OFF | 랜덤 (최대 13일 간격) |
| ThreatSmall 시작일 | 11일 | 13일 | 1일 |
| ThreatSmall 주기 | 4.6일 ON / 6일 OFF | 8일 ON / 8일 OFF | 랜덤 |
| 이벤트 발생 빈도 | 중간 | 낮음 | 매우 높음 |

### 3. 질병 발생

| 항목 | 카산드라 | 피비 | 랜디 |
|------|----------|------|------|
| Human Disease | 9일 후 | 12일 후 | 즉시 (0일) |
| Animal Disease | 9일 후 | 12일 후 | 즉시 (0일) |

### 4. 상인/방문자 이벤트

#### 카산드라 & 피비
- **TraderCaravanArrival**: `baseIncidentsPerYear=5`, `minSpacingDays=6`
- **VisitorGroup**: `baseIncidentsPerYear=4`, `minSpacingDays=5`
- **TravelerGroup**: `baseIncidentsPerYear=6`, `minSpacingDays=1`
- **OrbitalTraderArrival**: On/Off 사이클 (`onDays=7`, `offDays=8`)

#### 랜디
- **TraderCaravanArrival**: 없음
- **VisitorGroup**: 없음
- **TravelerGroup**: 없음
- **OrbitalTraderArrival**: 없음
- → FactionArrival 가중치 2.4로 대체 (랜덤 기반)

### 5. 퀘스트 시스템

| 항목 | 카산드라 | 피비 | 랜디 |
|------|----------|------|------|
| Non-Royalty 퀘스트 | `minSpacingDays=3` | `minSpacingDays=3` | `minSpacingDays=0.2` |
| Royalty 퀘스트 수 | `numIncidentsRange=2` | `numIncidentsRange=2` | `numIncidentsRange=1~3` |
| Royalty 퀘스트 간격 | `minSpacingDays=3` | `minSpacingDays=3` | `minSpacingDays=0.2` |

### 6. 특수 컴포넌트

#### 카산드라 & 피비만 보유
- **ClassicIntro**: 시작 인트로 이벤트
- **ShipChunkDrop**: 우주선 파편 낙하
- **TraderCaravanArrival**: 상인 캐러밴 도착
- **VisitorGroup**: 방문자 그룹
- **TravelerGroup**: 여행자 그룹
- **OrbitalTraderArrival**: 궤도 상인 도착

#### 랜디만 보유
- **RandomMain**: 랜덤 기반 메인 이벤트 시스템
- **spaceMtbDayFactor**: 2 (이벤트 간격 조정)
- **spaceMinSpacingDays**: 2 (최소 간격)

## 요약

### 카산드라 클래식
- **특징**: 점진적 난이도 증가, 규칙적 패턴
- **난이도**: 중간~높음
- **이벤트 빈도**: 규칙적 (4.6일 ON / 6일 OFF)
- **적합한 플레이어**: 클래식한 스토리텔링 선호, 예측 가능한 도전 원하는 플레이어

### 피비 칼락스
- **특징**: 긴 여유 시간, 건설 중심
- **난이도**: 낮음~중간 (높은 난이도에서도 강함)
- **이벤트 빈도**: 낮음 (8일 ON / 8일 OFF)
- **적합한 플레이어**: 건설/확장 중심 플레이어, 여유 있는 진행 선호

### 랜디 랜덤
- **특징**: 완전 랜덤, 예측 불가능
- **난이도**: 변동성 매우 높음
- **이벤트 빈도**: 매우 높음 (`mtbDays=1.35`)
- **적합한 플레이어**: 예측 불가능한 도전 원하는 플레이어, 드라마 선호
- **주의**: 매우 어렵거나 불공정한 이벤트 연속 발생 가능

## 핵심 차이 요약표

| 구분 | 카산드라 | 피비 | 랜디 |
|------|----------|------|------|
| **시스템** | OnOffCycle | OnOffCycle | RandomMain |
| **이벤트 빈도** | 중간 | 낮음 | 매우 높음 |
| **예측 가능성** | 높음 | 높음 | 없음 |
| **위협 시작** | 11일 | 13일 | 1일 |
| **질병 시작** | 9일 | 12일 | 즉시 |
| **상인 이벤트** | 있음 | 있음 | 없음 |
| **퀘스트 간격** | 3일 | 3일 | 0.2일 |

---

**분석 일자**: 2024년  
**참조 파일**: `RimworldData/Core/Defs/Storyteller/Storytellers.xml`

