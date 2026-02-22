# 필그림 이벤트 조사 보고서

## 개요

림월드 Ideology DLC의 필그림(순례자) 이벤트에 대한 상세 조사 결과입니다. 이 이벤트는 플레이어의 성물함(Reliquary)에 설치된 성물(Relic)을 보러 순례자들이 방문하는 퀘스트 이벤트입니다.

## 이벤트 발생 조건

### 1. 기본 요구사항
- **DLC**: Ideology DLC 필수
- **성물함(Reliquary)**: 맵에 설치되어 있어야 함
- **성물(Relic)**: 성물함에 설치되어 있어야 함
- **발생 주기**: Storyteller에 의해 40일 주기로 발생 가능
- **최소 간격**: 20일

### 2. Storyteller 설정
```xml
<!-- RimworldData/Core/Defs/Storyteller/Storytellers.xml -->
<li Class="StorytellerCompProperties_OnOffCycle" MayRequire="Ludeon.RimWorld.Ideology">
  <incident MayRequire="Ludeon.RimWorld.Ideology">GiveQuest_ReliquaryPilgrims</incident>
  <onDays>40</onDays>
  <offDays>0</offDays>
  <minSpacingDays>20</minSpacingDays>
</li>
```

## 이벤트 구조

### 1. IncidentDef
**파일**: `RimworldData/Ideology/Defs/IncidentDefs/Incidents_Map_Special.xml`

```xml
<IncidentDef>
  <defName>GiveQuest_ReliquaryPilgrims</defName>
  <category>GiveQuest</category>
  <label>pilgrims arrive</label>
  <targetTags>
    <li>Map_PlayerHome</li>
  </targetTags>
  <letterLabel>pilgrims arrive</letterLabel>
  <questScriptDef>ReliquaryPilgrims</questScriptDef>
  <workerClass>IncidentWorker_GiveQuest</workerClass>
  <baseChance>0</baseChance> <!-- Storyteller comp에 의해 제어됨 -->
  <requireColonistsPresent>True</requireColonistsPresent>
</IncidentDef>
```

### 2. QuestScriptDef
**파일**: `RimworldData/Ideology/Defs/QuestScriptDefs/Script_ReliquaryPilgrims.xml`

- **defName**: `ReliquaryPilgrims`
- **isRootSpecial**: `true`
- **autoAccept**: `true` (자동 수락)
- **hideInvolvedFactionsInfo**: `true`
- **sendAvailableLetter**: `false`
- **successHistoryEvent**: `ReliquaryPilgrimsSuccess`

### 3. QuestNode 구현
**파일**: `RimworldSource/RimWorld/QuestGen/QuestNode_Root_ReliquaryPilgrims.cs`

## 이벤트 동작 흐름

### 1. 필그림 생성
- **인원 수**: 1~4명 (랜덤)
- **아이 포함 가능**: 난이도 설정에 따라 아이 포함 가능
- **팩션 선택**:
  - 50% 확률: `FactionDefOf.Pilgrims` + `PawnKindDefOf.PovertyPilgrim`
  - 50% 확률: `FactionDefOf.OutlanderCivil` + `PawnKindDefOf.WellEquippedTraveler`
- **팩션 특성**: 임시 팩션으로 생성 (`faction.temporary = true`)
- **이데올로기**: 성물의 이데올로기로 설정

### 2. 필그림 행동
- **도착**: 맵 가장자리에서 도착
- **목적**: 성물함으로 이동하여 성물을 숭배
- **숭배 시간**: 5,000~10,000 틱 (약 1.4~2.8일)
- **퇴장**: 숭배 완료 후 맵을 떠남

### 3. 보상 시스템
- **보상 확률**: 50%
- **보상 가치**: 1,000~2,000 실버
- **보상 지연**: 5~10일 후 드롭포드로 전달
- **보상 종류**: 
  - 표준 품질 이상의 아이템
  - 낮은 빈도의 특수 아이템

### 4. 실패 조건
- 필그림 체포 시
- 필그림 사망 시
- 성물 파괴 시
- 성물함 파괴 시
- 맵 제거 시
- 팩션이 플레이어와 적대 관계가 될 시

## Ratkin 프로젝트의 필그림 관련 정의

### 1. FactionDef
**파일**: `Project/1.6/Defs/FactionDefs/Factions_Misc.xml`

```xml
<FactionDef ParentName="FactionBase">
  <defName>RK_Faction_Pilgrims</defName>
  <label>pilgrims</label>
  <categoryTag>Pilgrims</categoryTag>
  <canSiege>false</canSiege>
  <canStageAttacks>false</canStageAttacks>
  <hidden>true</hidden>
  <description>A group of pilgrims seeking to venerate an ancient relic.</description>
  <techLevel>Neolithic</techLevel>
  <basicMemberKind>RK_PawnKind_Pilgrim</basicMemberKind>
  <xenotypeSet>
    <xenotypeChances>
      <RK_XenoType_Ratkin MayRequire="Ludeon.RimWorld.Biotech">1</RK_XenoType_Ratkin>
    </xenotypeChances>
  </xenotypeSet>
  <backstoryFilters>
    <li>
      <categories>
        <li>RK_Backstory_Pilgrim</li>
      </categories>
    </li>
  </backstoryFilters>
</FactionDef>
```

**특징**:
- `hidden=true`: 공격받아도 우호도 변화 없음
- 공격 불가능 (`canSiege=false`, `canStageAttacks=false`)
- Ratkin 제노타입 사용
- 순례자 백스토리 필터 적용

### 2. PawnKindDef
**파일**: `Project/1.6/Defs/PawnKindDef_Ratkin/PawnKinds_Pilgrim.xml`

#### 기본 필그림
```xml
<PawnKindDef Name="RK_PawnKindAttr_PilgrimBase" ParentName="RKBasePawnKind">
  <defName>RK_PawnKind_Pilgrim</defName>
  <label>pilgrim</label>
  <defaultFactionDef>RK_Faction_Pilgrims</defaultFactionDef>
  <combatPower>35</combatPower>
  <gearHealthRange>0.1~0.3</gearHealthRange>
  <itemQuality>Awful</itemQuality>
  <apparelMoney>80~250</apparelMoney>
  <apparelAllowHeadgearChance>0.2</apparelAllowHeadgearChance>
  <apparelTags>
    <li>RK_ApparelTag_Pilgrim</li>
  </apparelTags>
  <weaponMoney>0~0</weaponMoney>
  <techHediffsChance>0</techHediffsChance>
  <inventoryOptions>
    <skipChance>0.9</skipChance>
    <subOptionsChooseOne>
      <li>
        <thingDef>Silver</thingDef>
        <countRange>10~25</countRange>
      </li>
      <li>
        <thingDef>MedicineHerbal</thingDef>
        <countRange>1</countRange>
      </li>
    </subOptionsChooseOne>
  </inventoryOptions>
  <initialWillRange>0~1</initialWillRange>
  <initialResistanceRange>4~8</initialResistanceRange>
</PawnKindDef>
```

**특징**:
- 무기 없음 (`weaponMoney>0~0`)
- 허름한 장비 (`itemQuality>Awful`, `gearHealthRange>0.1~0.3`)
- 소량의 실버 또는 약초 의약품 소지
- 낮은 전투력 (35)

#### 사제 필그림
```xml
<PawnKindDef ParentName="RK_PawnKindAttr_PilgrimBase">
  <defName>RK_PawnKind_Priest</defName>
  <label>pilgrim</label>
  <backstoryFiltersOverride>
    <li>
      <categories>
        <li>RK_Backstory_Pilgrim</li>
      </categories>
    </li>
  </backstoryFiltersOverride>
  <apparelRequired>
    <li>RK_SistersVeil</li>
    <li>RK_SistersDerss</li>
  </apparelRequired>
</PawnKindDef>
```

**특징**:
- 순례자 백스토리 필수
- 수녀 베일과 드레스 착용 필수

#### 귀족 필그림
```xml
<PawnKindDef ParentName="RK_PawnKindAttr_PilgrimBase">
  <defName>RK_PawnKind_NoblePilgrim</defName>
  <label>pilgrim</label>
  <defaultFactionDef>Rakinia</defaultFactionDef>
  <backstoryFiltersOverride>
    <li>
      <categories>
        <li>RK_Backstory_Noble</li>
      </categories>
    </li>
  </backstoryFiltersOverride>
  <apparelTags inherit="false">
    <li>RK_ApparelTag_Noble</li>
  </apparelTags>
</PawnKindDef>
```

**특징**:
- 귀족 백스토리 필수
- Rakinia 팩션 소속
- 귀족 의상 착용 (허름하게)

## 이벤트 트리거 메커니즘

### 1. 성물함 찾기
```csharp
// QuestNode_Root_ReliquaryPilgrims.cs
private static bool TryFindReliquaryWithRelic(Map map, out Precept_Relic relic, out Building reliquary, out Thing relicThing)
{
    foreach (Thing thing in map.listerThings.ThingsOfDef(ThingDefOf.Reliquary).InRandomOrder(null))
    {
        CompThingContainer compThingContainer = thing.TryGetComp<CompThingContainer>();
        if (compThingContainer != null)
        {
            foreach (Thing thing2 in compThingContainer.GetDirectlyHeldThings())
            {
                Precept_Relic precept_Relic = thing2.StyleSourcePrecept as Precept_Relic;
                if (precept_Relic != null)
                {
                    reliquary = (Building)thing;
                    relic = precept_Relic;
                    relicThing = thing2;
                    return true;
                }
            }
        }
    }
    return false;
}
```

**동작**:
1. 맵의 모든 Reliquary 건물 검색
2. 각 Reliquary의 컨테이너 확인
3. 컨테이너 내부의 아이템 중 Precept_Relic 타입 찾기
4. 성물이 있으면 true 반환

### 2. 팩션 및 PawnKind 선택
```csharp
private void GetFactionAndPawnKind(out FactionDef factionDef, out PawnKindDef pawnKind)
{
    if (Rand.Bool)  // 50% 확률
    {
        factionDef = FactionDefOf.Pilgrims;
        pawnKind = PawnKindDefOf.PovertyPilgrim;
        return;
    }
    factionDef = FactionDefOf.OutlanderCivil;
    pawnKind = PawnKindDefOf.WellEquippedTraveler;
}
```

**중요**: 필그림 이벤트에서 PawnKind 선택은 **명시적으로 하드코딩**되어 있습니다.

#### 기본 필그림 팩션의 basicMemberKind
`FactionDefOf.Pilgrims`에는 `basicMemberKind` 속성이 정의되어 있습니다:
```xml
<!-- RimworldData/Ideology/Defs/FactionDefs/Factions_Misc.xml -->
<FactionDef ParentName="FactionBase">
  <defName>Pilgrims</defName>
  <basicMemberKind>PovertyPilgrim</basicMemberKind>
  ...
</FactionDef>
```

하지만 필그림 이벤트에서는:
- **`basicMemberKind`를 사용하지 않음**
- **명시적으로 `PawnKindDefOf.PovertyPilgrim`을 지정**
- 카테고리나 자동 선택 메커니즘이 아닌 **코드에서 직접 지정**

#### 선택 방식 요약
1. **명시적 지정**: `QuestNode_Root_ReliquaryPilgrims.GetFactionAndPawnKind()` 메서드에서 하드코딩
2. **랜덤 선택**: 50% 확률로 두 가지 조합 중 선택
   - `Pilgrims` 팩션 + `PovertyPilgrim` PawnKind
   - `OutlanderCivil` 팩션 + `WellEquippedTraveler` PawnKind
3. **basicMemberKind의 역할**: 다른 시스템(예: 방랑자 합류 이벤트)에서 사용될 수 있지만, 필그림 이벤트에서는 무시됨

#### PovertyPilgrim PawnKindDef 정의
```xml
<!-- RimworldData/Ideology/Defs/PawnKinds/PawnKinds_Special.xml -->
<PawnKindDef>
  <defName>PovertyPilgrim</defName>
  <label>pilgrim</label>
  <race>Human</race>
  <defaultFactionDef>Pilgrims</defaultFactionDef>
  <combatPower>35</combatPower>
  <gearHealthRange>0.1~0.3</gearHealthRange>
  <itemQuality>Awful</itemQuality>
  <apparelMoney>80~250</apparelMoney>
  <weaponMoney>80~250</weaponMoney>
  <weaponTags>
    <li>MedievalMeleeBasic</li>
    <li>NeolithicMeleeBasic</li>
  </weaponTags>
  ...
</PawnKindDef>
```

## 보상 시스템 상세

### 1. ThingSetMakerDef
**파일**: `RimworldData/Ideology/Defs/ThingSetMakerDefs/ThingSetMakers_Reward.xml`

```xml
<ThingSetMakerDef>
  <defName>Reward_ReliquaryPilgrims</defName>
  <root Class="ThingSetMaker_MarketValue">
    <fixedParams>
      <filter>
        <thingSetMakerTagsToAllow>
          <li>RewardStandardQualitySuper</li>
          <li>RewardStandardLowFreq</li>
        </thingSetMakerTagsToAllow>
      </filter>
    </fixedParams>
  </root>
</ThingSetMakerDef>
```

### 2. 보상 파라미터
- **총 가치 범위**: 1,000~2,000 실버
- **품질 생성기**: `QualityGenerator.Reward`
- **아이템 수**: 1개
- **보상 확률**: 50%
- **지연 시간**: 5~10일 (300,000~600,000 틱)

## HistoryEvent

### ReliquaryPilgrimsSuccess
**파일**: `RimworldData/Ideology/Defs/PreceptDefs/Precepts_Proselytizing.xml`

- **이벤트**: 필그림이 성물을 성공적으로 숭배했을 때 발생
- **개발 포인트**: 2점 (Proselytizing Precept의 경우)
- **용도**: 이데올로기 전파 관련 Precept의 개발 포인트 획득

## Ratkin 프로젝트와의 연계 가능성

### 현재 상태
- Ratkin 전용 필그림 팩션 정의됨 (`RK_Faction_Pilgrims`)
- Ratkin 전용 필그림 PawnKind 정의됨 (`RK_PawnKind_Pilgrim`)
- 하지만 기본 필그림 이벤트는 여전히 원본 팩션/PawnKind 사용

### 개선 방안
1. **QuestNode 패치**: `QuestNode_Root_ReliquaryPilgrims.GetFactionAndPawnKind()` 메서드를 패치하여 Ratkin 팩션/PawnKind 사용
2. **조건부 팩션 선택**: 플레이어가 Ratkin인 경우 Ratkin 필그림 사용
3. **커스텀 IncidentDef**: Ratkin 전용 필그림 이벤트 생성

## 관련 파일 목록

### RimworldData
- `Ideology/Defs/IncidentDefs/Incidents_Map_Special.xml`
- `Ideology/Defs/QuestScriptDefs/Script_ReliquaryPilgrims.xml`
- `Ideology/Defs/ThingSetMakerDefs/ThingSetMakers_Reward.xml`
- `Ideology/Defs/PreceptDefs/Precepts_Proselytizing.xml`
- `Core/Defs/Storyteller/Storytellers.xml`

### RimworldSource
- `RimWorld/QuestGen/QuestNode_Root_ReliquaryPilgrims.cs`

### Ratkin 프로젝트
- `Project/1.6/Defs/FactionDefs/Factions_Misc.xml` (RK_Faction_Pilgrims)
- `Project/1.6/Defs/PawnKindDef_Ratkin/PawnKinds_Pilgrim.xml` (RK_PawnKind_Pilgrim 등)

## 참고사항

1. **Ideology DLC 필수**: 이 이벤트는 Ideology DLC가 없으면 발생하지 않습니다.
2. **성물 필요**: 성물함에 성물이 설치되어 있어야만 이벤트가 발생합니다.
3. **임시 팩션**: 필그림 팩션은 임시 팩션이므로 이벤트 종료 후 사라집니다.
4. **자동 수락**: 퀘스트는 자동으로 수락되며 거부할 수 없습니다.
5. **보호 의무**: 필그림을 보호해야 보상을 받을 수 있습니다.

