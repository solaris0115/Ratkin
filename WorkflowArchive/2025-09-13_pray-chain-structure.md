# Pray 관련 항목 체인 구조 문서

## 🎯 **핵심 Pray 시스템 체인**

### **1. AbilityDef (능력 정의)**
```
RK_PrayerService
├── iconPath: UI/Abilities/PrayerService
├── gizmoClass: NewRatkin.Command_AbilityPrayService
└── comps: NewRatkin.CompProperties_AbilityPrayService
```

### **2. InteractionDef (상호작용 정의)**
```
RK_PriestPray
├── workerClass: NewRatkin.InteractionWorker_PriestPray
├── symbol: Things/Mote/SpeechSymbols/Pray
└── recipientThought: RK_PriestPray
```

### **3. ThoughtDef (생각 정의)**
```
RK_PriestPray
├── thoughtToMake: RK_PriestPrayMood
└── stages: "blessed by priest" (+10 opinion)

RK_PriestPrayMood
└── stages: "blessed by priest" (+5 mood)

RK_AttendPrayerMeetingMood
├── icon: Things/Mote/SpeechSymbols/Pray
└── stages: "attends prayer meeting" (+2 mood)
```

### **4. JobDef (작업 정의)**
```
RK_Job_PrayerService
├── driverClass: NewRatkin.JobDriver_PrayerService
└── reportString: "praying."

RK_Job_SpectatePray
├── driverClass: JobDriver_Spectate
└── reportString: "praying."
```

### **5. DutyDef (의무 정의)**
```
RK_JoinPrayerService
├── hook: MediumPriority
├── socialModeMax: SuperActive
└── thinkNode: JobGiver_GotoTravelDestination + ThinkNode_ConditionalAtDutyRoom

RK_SpectatePrayerService
├── hook: MediumPriority
├── socialModeMax: Quiet
└── thinkNode: NewRatkin.JobGiver_PrayDutySpectateRect

RK_OrganizePrayerService
├── socialModeMax: Off
├── thinkNode: NewRatkin.JobGiver_GotoTravelInteractionCell
└── subNode: NewRatkin.JobGiver_PrayerService
```

### **6. ThingDef (건물 정의)**
```
RK_Pulpit
├── description: "a small pulpit. ratkin priest can host prayer meeting in this place."
├── texPath: Things/Building/Furniture/RK_Pulpit
├── hasInteractionCell: True
└── interactionCellOffset: (0,0,-1)
```

## 🔗 **연결된 시스템들**

### **PawnKindDef 연결**
```
RatkinPriest
├── defName: RatkinPriest
├── backstoryFiltersOverride: RatkinPriest
└── apparelTags: RK_Priest
```

### **Apparel 연결**
```
RK_Priest 태그를 가진 장비들
├── RK_SistersVeil (필수 장비)
└── RK_Priest 태그가 있는 다른 장비들
```

### **BackStoryDef 연결**
```
RatkinPriest 카테고리 백스토리들
├── 다양한 RatkinPriest 백스토리들
└── AlienRaceSettings에서 RatkinPriest 참조
```

### **FactionDef 연결**
```
Factions_Misc.xml
├── RatkinPriest: 10 (일부 팩션)
└── RatkinPriest: 3 (일부 팩션)
```

## 📁 **파일별 분류**

### **직접 정의 파일들**
- `AbilityDefs/AbilityDefs.xml` - RK_PrayerService
- `InteractionDefs/InteractionDefs.xml` - RK_PriestPray
- `ThoughtDefs/ThoughtDefs.xml` - RK_PriestPray, RK_PriestPrayMood, RK_AttendPrayerMeetingMood
- `JobDefs/jobdef.xml` - RK_Job_PrayerService, RK_Job_SpectatePray
- `DutyDefs/DutyDefs.xml` - RK_JoinPrayerService, RK_SpectatePrayerService, RK_OrganizePrayerService
- `ThingDefs_Building/Buildings_Furniture.xml` - RK_Pulpit

### **참조 파일들**
- `PawnKindDef_Ratkin/PawnKinds_Player.xml` - RatkinPriest
- `ThingsDefs/RK_Apparel.xml` - RK_Priest 태그 장비들
- `BackStoryDefs/BackstoryDef.xml` - RatkinPriest 백스토리들
- `FactionDefs/Factions_Misc.xml` - RatkinPriest 팩션 설정
- `AlienRaceSettings/AlienRaceSettings.xml` - RatkinPriest 설정

## 🎨 **리소스 파일들**
- `UI/Abilities/PrayerService` (아이콘)
- `Things/Mote/SpeechSymbols/Pray` (심볼)
- `Things/Building/Furniture/RK_Pulpit` (건물 텍스처)

## 🧩 **C# 클래스들 (Source 폴더)**
- `NewRatkin.Command_AbilityPrayService`
- `NewRatkin.CompProperties_AbilityPrayService`
- `NewRatkin.InteractionWorker_PriestPray`
- `NewRatkin.JobDriver_PrayerService`
- `NewRatkin.JobGiver_PrayerService`
- `NewRatkin.JobGiver_PrayDutySpectateRect`
- `NewRatkin.JobGiver_GotoTravelInteractionCell`
- `NewRatkin.ThinkNode_ConditionalAtDutyRoom`
- `NewRatkin.ThinkNode_ConditionalAtDutyInteractionCell`

## ⚠️ **삭제 시 주의사항**
1. **순서**: 의존성 순서대로 삭제 (참조하는 것부터 먼저)
2. **체크리스트**: 모든 참조가 제거되었는지 확인
3. **백업**: 삭제 전 git stash 생성 필수
4. **테스트**: 삭제 후 게임 로딩 테스트 필요
