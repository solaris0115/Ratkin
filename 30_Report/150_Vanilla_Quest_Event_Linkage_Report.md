# Vanilla Quest Event Linkage Report

**태그:** Quest, Event, Signal, Subquest, HistoryEvent, UniquePawn, 고유 캐릭터, 퀘스트 연계

**Agent:** Codex  
**기준:** RimWorld 1.6 바닐라 + DLC (`RimworldSource/`, `RimworldData/`)  
**작성일:** 2026-06-17  
**수정일:** 2026-06-18

---

## 표기

- `[]`: 복수 항목 가능
- 구조: 객체 구성
- 플로우: 처리 순서

---

## 비교군. 기본 랜덤

연계 유형이 아니다.  
바닐라 기본 발생 방식이다.

### 구조

주요 클래스:

- `StorytellerComp_RandomQuest`
- `IncidentWorker_GiveQuest`
- `NaturalRandomQuestChooser`
- `QuestScriptDef`
- `QuestManager`

```text
StorytellerComp
├─ IncidentWorker
├─ QuestScriptDef[]
└─ QuestManager
   └─ Quest[]
```

### 간결 플로우

```text
주기 호출
-> 후보[] 추림
-> 조건[] 확인
-> 퀘스트 생성
```

---

## 1. 트리거 연계

아이템, 책, 상인 등 특정 행위가 퀘스트를 여는 방식.

### 구조

주요 클래스:

- `CompUseEffect_GiveQuest`
- `BookOutcomeDoer_GiveQuest`
- `TradeUtility`
- `QuestGiverTag`
- `QuestScriptDef`

```text
트리거
├─ QuestScriptDef
├─ QuestScriptDef[]
│  └─ giverTag
└─ QuestManager
   └─ Quest
```

### 간결 플로우

```text
사용/거래
-> 퀘스트 선택
-> 조건[] 확인
-> 퀘스트 생성
```

---

## 2. 신호 연계

대상 사건을 신호로 바꿔 퀘스트가 반응하는 방식.

### 구조

주요 클래스:

- `Quest`
- `QuestPart`
- `QuestNode_Signal`
- `QuestNode_SendSignals`
- `QuestUtility`

```text
Quest
├─ target[]
│  └─ questTags[]
├─ signal[]
└─ QuestPart[]
```

### 간결 플로우

```text
대상 사건
-> 신호 전송
-> 파트[] 반응
-> 결과 처리
```

예:

```text
루미 구출
-> Rescued
-> 성공 처리
```

```text
루미 사망
-> Killed
-> 실패 처리
```

---

## 3. 지연 연계

시간 경과 뒤 사건을 실행하는 방식.

### 구조

주요 클래스:

- `QuestNode_CreateIncidents`
- `QuestNode_Incident`
- `QuestPart_Delay`
- `QuestPart_Incident`
- `IncidentDef`

```text
Quest
├─ Delay[]
│  └─ outSignal
└─ IncidentPart[]
   ├─ IncidentDef
   └─ IncidentParms
```

### 간결 플로우

```text
지연[] 등록
-> 시간 경과
-> 신호 발생
-> 사건[] 실행
```

예:

```text
호위 시작
-> 2일 대기
-> 습격 발생
```

---

## 4. 하위 연계

부모 퀘스트가 하위 퀘스트를 여는 방식.

### 구조

주요 클래스:

- `QuestPart_SubquestGenerator`
- `QuestScriptDef`
- `Quest`
- `QuestUtility`

```text
부모 Quest
├─ SubquestGen
│  └─ QuestScriptDef[]
└─ 자식 Quest[]
   └─ parent
```

### 간결 플로우

```text
부모 진행
-> 하위[] 생성
-> 하위[] 완료
-> 부모 갱신
```

예:

```text
루미 과거
├─ 흔적 조사
├─ 동료 구출
└─ 최종 선택
```

---

## 5. 후속 연계

퀘스트 A의 신호가 퀘스트 B를 여는 방식.

### 구조

주요 클래스:

- `QuestPart_AddQuest`
- `QuestPart_AddGiverQuest`
- `QuestScriptDef`
- `QuestUtility`

```text
Quest A
└─ AddQuest[]
   ├─ inSignal
   ├─ QuestScriptDef B
   └─ parent
```

### 간결 플로우

```text
A 신호
-> B 생성
-> parent 연결
-> 표시/수락
```

예:

```text
루미 구출
└─ 은신처 조사
   └─ 추적전
```

---

## 6. 이력 연계

퀘스트 결과를 이력으로 남기는 방식.

### 구조

주요 클래스:

- `HistoryEventDef`
- `HistoryEventsManager`
- `QuestNode_RecordHistoryEvent`
- `QuestPart_RecordHistoryEvent`
- `QuestScriptDef`

```text
QuestScriptDef
├─ successHistoryEvent
└─ failedOrExpiredHistoryEvent

Quest
└─ RecordHistoryEvent[]

HistoryEvents
└─ HistoryEvent[]
```

### 간결 플로우

```text
성공/실패
-> 이력[] 기록
-> 반응[] 처리
```

한계:

```text
이력 확인
-> 다음 해금
```

이 해금 조건은 기본 XML만으로 약하다.  
커스텀 조건 노드가 적합하다.

---

## 7. 특수 일정 연계

스토리텔러 전용 로직이 과거 퀘스트 상태를 보고 다시 여는 방식.

### 구조

주요 클래스:

- `StorytellerComp_ImportantQuest`
- `StorytellerComp_RefiringUniqueQuest`
- `StorytellerComp_RandomEpicQuest`
- `StorytellerComp_MechanitorComplexQuest`
- `QuestManager`

```text
StorytellerComp_*
├─ IncidentDef
│  └─ QuestScriptDef
└─ QuestManager
   └─ Quest[]
      └─ state
```

### 간결 플로우

```text
일수 확인
-> 이력[] 확인
-> 상태[] 확인
-> 퀘스트 발생
```

---

## 보조 제어

연계 유형은 아니다.  
연계가 꼬이지 않게 막는 장치다.

### 구조

주요 클래스:

- `QuestNode_QuestUnique`
- `QuestScriptDef`
- `QuestManager`
- `Quest`

```text
QuestScriptDef
└─ QuestUnique
   └─ tag

QuestManager
└─ Quest[]
   └─ tag[]
```

### 간결 플로우

```text
태그 작성
-> 진행중 검색
-> 있으면 차단
-> 없으면 생성
```

---

## 고유 캐릭터 예시

### 구조

```text
GameComponent
└─ storyStage

Quest[]
├─ unique tag[]
├─ target pawn
│  └─ questTags[]
├─ signal[]
└─ followup[]
```

### 간결 플로우

```text
첫 만남
-> 상태: 만남
-> 구출 퀘스트
-> 상태: 구출
-> 최종 퀘스트
-> 상태: 종료
```

단계 예:

```text
첫 만남
├─ 유니크 제한
├─ 폰 태그[] 부여
└─ 성공
   └─ 상태: 만남
```

```text
구출 퀘스트
├─ 조건[]: 만남
├─ 지연[] 습격
├─ 구출 신호[]
└─ 성공
   └─ 후속[]
```

```text
최종 퀘스트
├─ 조건[]: 구출
├─ 선택[]
└─ 완료
   └─ 상태: 종료
```

장기 상태는 바닐라 XML보다 커스텀 저장이 적합하다.

---

## 참고 파일

- `RimworldSource/RimWorld/QuestScriptDef.cs`
- `RimworldSource/RimWorld/NaturalRandomQuestChooser.cs`
- `RimworldSource/RimWorld/IncidentWorker_GiveQuest.cs`
- `RimworldSource/RimWorld/QuestUtility.cs`
- `RimworldSource/RimWorld/QuestPart_SubquestGenerator.cs`
- `RimworldSource/RimWorld/QuestPart_AddQuest.cs`
- `RimworldSource/RimWorld/QuestPart_Incident.cs`
- `RimworldSource/RimWorld/QuestGen/QuestNode_QuestUnique.cs`
- `RimworldSource/RimWorld/QuestGen/QuestNode_Signal.cs`
- `RimworldSource/RimWorld/QuestGen/QuestNode_CreateIncidents.cs`
- `RimworldSource/RimWorld/QuestGen/QuestNode_RecordHistoryEvent.cs`
- `RimworldSource/RimWorld/StorytellerComp_ImportantQuest.cs`
- `RimworldSource/RimWorld/StorytellerComp_RefiringUniqueQuest.cs`
- `RimworldSource/RimWorld/StorytellerComp_MechanitorComplexQuest.cs`
