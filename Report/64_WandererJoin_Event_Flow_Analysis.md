# WandererJoin 이벤트 플로우 분석 보고서

**작성일**: 2025-12-15  
**분석 대상**: 림월드 WandererJoin 이벤트 시스템  
**목적**: 이벤트 트리거 및 선택지 제공 플로우 분석

---

## 1. 개요

WandererJoin은 림월드에서 무작위로 발생하는 이벤트로, 홀로 떠돌던 사람이 플레이어의 정착지에 합류하고자 하는 상황을 나타냅니다. 이 이벤트는 플레이어에게 수락/거절 선택권을 제공합니다.

---

## 2. 이벤트 정의 구조 (IncidentDef)

### 2.1 기본 구조

```xml
<IncidentDef>
  <defName>WandererJoin</defName>
  <label>방랑자 합류</label>
  <category>ThreatBig</category> <!-- 또는 Misc -->
  <workerClass>IncidentWorker_WandererJoin</workerClass>
  
  <!-- 발생 조건 -->
  <baseChance>1.5</baseChance>
  <minPopulation>1</minPopulation>
  
  <!-- 타겟 -->
  <targetTags>
    <li>Map_PlayerHome</li>
  </targetTags>
</IncidentDef>
```

### 2.2 주요 속성

- **defName**: 이벤트 고유 식별자
- **workerClass**: 이벤트 로직을 처리하는 C# 클래스
- **category**: 이벤트 카테고리 (Storyteller가 참조)
- **baseChance**: 기본 발생 확률
- **minPopulation**: 최소 정착민 수 조건

---

## 3. 이벤트 플로우

### 3.1 전체 플로우 다이어그램

```
[Storyteller] 
    ↓
[이벤트 풀에서 WandererJoin 선택]
    ↓
[IncidentWorker_WandererJoin.TryExecuteWorker() 호출]
    ↓
[조건 검증]
    ├─ 맵 존재 여부
    ├─ 플레이어 세력 존재
    └─ 기타 전제 조건
    ↓
[방랑자 Pawn 생성]
    ├─ PawnKindDef 선택
    ├─ 백스토리, 스킬, 트레이트 생성
    └─ 장비 및 건강 상태 설정
    ↓
[Letter (편지) 생성 및 전송]
    ├─ LetterDef: PositiveEvent 또는 NeutralEvent
    ├─ 제목: "[방랑자 이름]이(가) 합류하려 합니다"
    └─ 내용: 방랑자 정보 및 배경 설명
    ↓
[ChoiceLetter 또는 DiaNode를 통한 선택지 제공]
    ├─ 수락 → Pawn을 플레이어 세력에 추가
    ├─ 거절 → Pawn 제거 또는 맵에서 떠남
    └─ (미루기는 일반적으로 없음, Letter를 닫으면 자동 수락)
    ↓
[결과 처리]
```

---

## 4. 핵심 구성 요소

### 4.1 IncidentWorker_WandererJoin 클래스

**위치**: `RimWorld.IncidentWorker_WandererJoin`

#### 주요 메서드

```csharp
public class IncidentWorker_WandererJoin : IncidentWorker
{
    // 이벤트 실행 메인 메서드
    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        
        // 1. 방랑자 생성
        Pawn wanderer = GenerateWanderer(map);
        
        // 2. 맵에 스폰
        IntVec3 spawnLoc = CellFinder.RandomClosewalkCellNear(
            map.Center, map, 5);
        GenSpawn.Spawn(wanderer, spawnLoc, map);
        
        // 3. Letter 전송
        SendStandardLetter(parms, wanderer);
        
        return true;
    }
    
    // 방랑자 생성 로직
    private Pawn GenerateWanderer(Map map)
    {
        PawnKindDef pawnKind = DefDatabase<PawnKindDef>
            .AllDefs.RandomElement();
        
        Pawn pawn = PawnGenerator.GeneratePawn(
            new PawnGenerationRequest(
                kind: pawnKind,
                faction: Faction.OfPlayer,
                context: PawnGenerationContext.NonPlayer
            ));
        
        return pawn;
    }
}
```

### 4.2 Letter 시스템

#### 기본 Letter (정보만 제공)

```csharp
// 단순 알림 Letter
Find.LetterStack.ReceiveLetter(
    label: "방랑자 합류",
    text: wanderer.NameFullColored + "이(가) 합류했습니다.",
    def: LetterDefOf.PositiveEvent,
    lookTargets: wanderer
);
```

이 경우 **자동 수락**되며, 플레이어는 정보만 받습니다.

#### ChoiceLetter (선택지 제공)

일부 모드나 특수 상황에서는 `ChoiceLetter_WandererJoin`을 사용할 수 있습니다:

```csharp
ChoiceLetter_WandererJoin letter = (ChoiceLetter_WandererJoin)
    LetterMaker.MakeLetter(LetterDefOf.PositiveEvent);

letter.title = "방랑자가 찾아왔습니다";
letter.label = wanderer.NameFullColored;
letter.wanderer = wanderer;

letter.Choices.Add(new DiaOption("수락") {
    action = delegate {
        // 수락 로직
        wanderer.SetFaction(Faction.OfPlayer);
    },
    resolveTree = true
});

letter.Choices.Add(new DiaOption("거절") {
    action = delegate {
        // 거절 로직
        wanderer.Destroy();
    },
    resolveTree = true
});

Find.LetterStack.ReceiveLetter(letter);
```

---

## 5. 선택지 제공 메커니즘

### 5.1 DiaNode 시스템

림월드는 대화 트리를 `DiaNode`와 `DiaOption`으로 구성합니다.

```csharp
public class DiaNode
{
    public string text;                    // 표시 텍스트
    public List<DiaOption> options;        // 선택지 목록
}

public class DiaOption
{
    public string text;                    // 버튼 텍스트
    public Action action;                  // 선택 시 실행할 액션
    public bool resolveTree;               // 대화 종료 여부
}
```

### 5.2 Dialog_NodeTree

선택지를 화면에 표시하는 UI 클래스:

```csharp
Dialog_NodeTree dialog = new Dialog_NodeTree(rootNode);
Find.WindowStack.Add(dialog);
```

### 5.3 플로우 예시

```
[Letter 수신]
    ↓
[플레이어가 Letter 클릭]
    ↓
[Dialog_NodeTree 팝업]
    ↓
    ├─ [수락 버튼]
    │   ↓
    │   └─ wanderer.SetFaction(Faction.OfPlayer)
    │
    ├─ [거절 버튼]
    │   ↓
    │   └─ wanderer.Destroy() 또는 맵에서 퇴장
    │
    └─ [미루기 버튼] (선택적)
        ↓
        └─ 대화창 닫기, 방랑자는 맵에 남음
```

---

## 6. 바닐라 WandererJoin의 실제 동작

**중요**: 바닐라 림월드의 `WandererJoin` 이벤트는 **선택지를 제공하지 않습니다**.

### 6.1 바닐라 동작

1. 이벤트 발생
2. 방랑자 Pawn 생성 및 스폰
3. **자동으로 플레이어 세력에 합류**
4. PositiveEvent Letter로 알림

### 6.2 선택지가 있는 유사 이벤트

선택지를 제공하는 유사 이벤트들:

- **RefugeePodCrash**: 조난자 구출 (치료 여부 선택)
- **RefugeeChased**: 쫓기는 난민 (받아들일지 여부 선택)
- **QuestPawnJoins**: 퀘스트 완료 시 합류 제안

---

## 7. 커스텀 선택지 구현 방법

Ratkin 모드에서 선택지가 있는 방랑자 합류 이벤트를 만들려면:

### 7.1 커스텀 IncidentWorker 작성

```csharp
public class IncidentWorker_WandererJoinWithChoice : IncidentWorker
{
    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        
        // 방랑자 생성
        Pawn wanderer = GenerateWanderer(map);
        
        // 맵 가장자리에 스폰
        IntVec3 spawnLoc = CellFinder.RandomEdgeCell(map);
        GenSpawn.Spawn(wanderer, spawnLoc, map);
        
        // 선택지가 있는 Dialog 생성
        DiaNode rootNode = CreateDialogNode(wanderer, map);
        Dialog_NodeTree dialog = new Dialog_NodeTree(rootNode, 
            delayInteractivity: true);
        
        // Letter와 함께 전송
        Find.LetterStack.ReceiveLetter(
            label: "WandererJoinChoice_Label".Translate(),
            text: "WandererJoinChoice_Text".Translate(
                wanderer.NameFullColored),
            def: LetterDefOf.NeutralEvent,
            lookTargets: wanderer,
            relatedFaction: null,
            quest: null,
            showInLetterTab: true,
            customText: null
        );
        
        // Dialog 표시
        Find.WindowStack.Add(dialog);
        
        return true;
    }
    
    private DiaNode CreateDialogNode(Pawn wanderer, Map map)
    {
        DiaNode node = new DiaNode("WandererJoinChoice_Dialog".Translate(
            wanderer.NameFullColored));
        
        // 수락 옵션
        DiaOption acceptOption = new DiaOption("Accept".Translate());
        acceptOption.action = delegate
        {
            wanderer.SetFaction(Faction.OfPlayer);
            Messages.Message("WandererJoinChoice_Accepted".Translate(
                wanderer.NameShortColored), 
                wanderer, MessageTypeDefOf.PositiveEvent);
        };
        acceptOption.resolveTree = true;
        node.options.Add(acceptOption);
        
        // 거절 옵션
        DiaOption rejectOption = new DiaOption("Reject".Translate());
        rejectOption.action = delegate
        {
            // 방랑자를 맵에서 퇴장시킴
            wanderer.DeSpawn();
            Find.WorldPawns.PassToWorld(wanderer, 
                PawnDiscardDecideMode.Decide);
            
            Messages.Message("WandererJoinChoice_Rejected".Translate(), 
                MessageTypeDefOf.NeutralEvent);
        };
        rejectOption.resolveTree = true;
        node.options.Add(rejectOption);
        
        // 미루기 옵션 (선택적)
        DiaOption delayOption = new DiaOption("Postpone".Translate());
        delayOption.action = delegate
        {
            Messages.Message("WandererJoinChoice_Postponed".Translate(
                wanderer.NameShortColored), 
                wanderer, MessageTypeDefOf.NeutralEvent);
            // 방랑자는 맵에 남아있음
        };
        delayOption.resolveTree = true;
        node.options.Add(delayOption);
        
        return node;
    }
}
```

### 7.2 커스텀 IncidentDef 정의

```xml
<IncidentDef>
  <defName>Ratkin_WandererJoinChoice</defName>
  <label>방랑자 합류 제안</label>
  <category>Misc</category>
  <workerClass>YourNamespace.IncidentWorker_WandererJoinWithChoice</workerClass>
  
  <baseChance>1.5</baseChance>
  <minPopulation>1</minPopulation>
  
  <letterLabel>방랑자가 찾아왔습니다</letterLabel>
  <letterText>{0}이(가) 정착지 근처를 배회하고 있습니다. 합류를 허락하시겠습니까?</letterText>
  <letterDef>NeutralEvent</letterDef>
  
  <targetTags>
    <li>Map_PlayerHome</li>
  </targetTags>
</IncidentDef>
```

---

## 8. 주요 참조 클래스

### 8.1 코어 클래스

| 클래스 | 용도 |
|--------|------|
| `IncidentWorker` | 이벤트 처리 기본 클래스 |
| `IncidentDef` | 이벤트 정의 |
| `PawnGenerator` | Pawn 생성 유틸리티 |
| `LetterStack` | Letter 관리 |
| `Dialog_NodeTree` | 선택지 대화창 UI |
| `DiaNode` | 대화 노드 |
| `DiaOption` | 선택지 옵션 |

### 8.2 유틸리티

| 유틸리티 | 메서드 | 용도 |
|----------|--------|------|
| `GenSpawn` | `Spawn()` | 맵에 객체 생성 |
| `CellFinder` | `RandomEdgeCell()` | 가장자리 셀 찾기 |
| `Find` | `LetterStack` | Letter 시스템 접근 |
| `Find` | `WindowStack` | UI 윈도우 스택 접근 |

---

## 9. 트리거 메커니즘

### 9.1 Storyteller의 역할

```csharp
// Storyteller가 주기적으로 호출
public void TryFireIncident()
{
    // 1. 발생 가능한 이벤트 목록 필터링
    List<IncidentDef> availableIncidents = 
        GetAvailableIncidents(currentThreatPoints);
    
    // 2. 가중치 기반 랜덤 선택
    IncidentDef chosenIncident = 
        availableIncidents.RandomElementByWeight(
            i => i.baseChance * GetChanceModifiers(i));
    
    // 3. 이벤트 실행
    chosenIncident.Worker.TryExecute(CreateParms());
}
```

### 9.2 발생 조건

```csharp
public override bool CanFireNow(IncidentParms parms)
{
    Map map = (Map)parms.target;
    
    // 조건 검증
    if (map == null) return false;
    if (map.mapPawns.FreeColonistsCount < def.minPopulation) 
        return false;
    if (!map.IsPlayerHome) return false;
    
    return true;
}
```

---

## 10. 요약

### 10.1 핵심 플로우

1. **Storyteller가 이벤트 선택** → 가중치 기반 랜덤
2. **IncidentWorker.TryExecuteWorker() 호출**
3. **조건 검증** → CanFireNow()
4. **방랑자 Pawn 생성** → PawnGenerator
5. **맵에 스폰** → GenSpawn.Spawn()
6. **Letter 전송** → Find.LetterStack.ReceiveLetter()
7. **(선택적) Dialog 표시** → Dialog_NodeTree
8. **플레이어 선택 처리**
   - 수락 → SetFaction(Faction.OfPlayer)
   - 거절 → Destroy() 또는 PassToWorld()
   - 미루기 → 아무 작업 없음

### 10.2 바닐라 vs 커스텀

| 항목 | 바닐라 WandererJoin | 커스텀 구현 |
|------|---------------------|-------------|
| 선택지 | 없음 (자동 수락) | 수락/거절/미루기 |
| Letter | PositiveEvent | NeutralEvent |
| Dialog | 없음 | Dialog_NodeTree |
| 스폰 위치 | 맵 중앙 근처 | 맵 가장자리 |

---

## 11. 다음 단계 제안

Ratkin 모드에서 방랑자 합류 이벤트를 커스터마이즈하려면:

1. **커스텀 IncidentWorker 클래스 작성**
   - 선택지가 있는 Dialog 구현
   - 수락/거절/미루기 로직

2. **IncidentDef 정의**
   - Ratkin 종족용 PawnKindDef 참조
   - 적절한 발생 확률 설정

3. **번역 문자열 추가**
   - WandererJoinChoice_Label
   - WandererJoinChoice_Text
   - WandererJoinChoice_Dialog
   - WandererJoinChoice_Accepted
   - WandererJoinChoice_Rejected
   - WandererJoinChoice_Postponed

4. **테스트**
   - 개발 모드에서 강제 발생
   - 각 선택지 동작 확인

---

**분석 완료**
