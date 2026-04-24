# 히든 팩션 생성 - 2025-11-30

## 작업 개요
- 요청 내용: 히든 팩션1 (랫킨 캐러반) 생성
- 목표: 상인 전용 비전투 히든 팩션 추가

## 계획 (AI가 결정한 계획)
1. 워크플로우 파일 생성
2. 랫킨 캐러반 히든 팩션 Def 생성
3. 불명확한 파라미터는 ??? 또는 주석으로 표시

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. 워크플로우 파일 생성 [v]
2. 랫킨 캐러반 히든 팩션 Def 생성 [v]
3. 작업 완료 [v]

## 작업 세부 진행
1. 워크플로우 파일 생성 [v]
2. FactionDef 생성 [v]

## 진행 상황
### 1. 워크플로우 파일 생성
- 내용: 54_Hidden_Faction_Caravan_Creation.md 생성
- 결과: 완료

### 2. FactionDef 생성
- 내용: 히든 팩션1 (랫킨 캐러반) 추가
- 결과: 완료
  - defName: RK_Faction_Caravan
  - hidden=true
  - canSiege=false, canStageAttacks=false
  - 전투 그룹 없음 (Trader 그룹만 존재)
  - caravanTraderKinds: RK_TraderKind_Caravan
  - ??? 표시 항목:
    1. 공격 당한 경우 우호도 하락 없음 파라미터 (주석으로 안내)
    2. backstory categories
    3. PawnKindDef (상인, 가드, 운반동물)

## 최종 작업 결과
작업 완료되었습니다.

### 생성된 FactionDef
- **defName**: RK_Faction_Caravan
- **특징**:
  - 히든 팩션 (hidden=true) → **공격받아도 적대로 변하지 않음**
  - 침략 불가 (canSiege=false, canStageAttacks=false)
  - 전투 그룹 없음
  - Trader 그룹만 존재
  - TraderKind: RK_TraderKind_Caravan 사용
  - Xenotype: RK_XenoType_Ratkin
  - Culture: RK_Culture_Kingdom

### 적대 방지 메커니즘
`hidden=true` 설정으로 인해:
- `HasGoodwill = false` → 우호도 시스템 비활성화
- 공격, 사망, 체포 등의 이벤트가 우호도에 영향 없음
- **영구적으로 중립 상태 유지**

### 채워진 항목들
1. **backstoryFilters**: `Offworld` 사용 (기존 랫킨 팩션과 동일)
   
2. **PawnKindDef**:
   - traders: `RatkinMerchant`, `RatkinNoble`
   - carriers: `Ratkin_KingHamster`
   - guards: `RatkinMercenaryLight`, `RatkinSoldier`, `RatkinEliteGuardener`, `RatkinDefender`
   
   (기존 Rakinia 팩션의 Trader pawnGroupMaker와 동일한 구성)

## 관련 파일 목록
- [Factions_Misc.xml](../Project/1.6/Defs/FactionDefs/Factions_Misc.xml)
- [TraderKinds_Caravan_Ratkin.xml](../Project/1.6/Defs/TraderDefs/TraderKinds_Caravan_Ratkin.xml)

## 추가 조사: 적대 상단 거래 및 이벤트

### 2.1 거래 시 적대 팩션 패널티
**결론: 적대 팩션과는 거래 불가능**

#### 거래 불가 이유
1. **CanTradeNow 조건** (Pawn_TraderTracker.cs:83)
```csharp
(this.pawn.Faction == null || !this.pawn.Faction.HostileTo(Faction.OfPlayer))
```
- 적대 팩션이면 `CanTradeNow = false`
- 거래 자체가 불가능

2. **상단 도착 이벤트 차단** (IncidentWorker_TraderCaravanArrival.cs:91-94)
```csharp
if (parms.faction.HostileTo(Faction.OfPlayer))
{
    return false;
}
```
- 적대 팩션은 일반 상단 이벤트로 오지 않음

#### 거래 가격 패널티
**적대 여부와 무관**하게 거래 가격은 다음 요소만 영향:
- priceGain_PlayerNegotiator: 협상가 능력
- priceGain_Leader: 리더 보너스
- priceGain_Settlement: 정착지 거래 보너스
- priceGain_DrugBonus: 마약 거래 보너스
- priceGain_AnimalProduceBonus: 동물 생산물 보너스

**우호도나 적대 여부는 거래 가격에 영향 없음**

### 해결 방안

#### Option 1: 일반 팩션으로 변경 (권장)
```xml
<hidden>false</hidden>
<mustStartOneEnemy>false</mustStartOneEnemy>
```
- 공격하면 적대가 됨
- 적대 상태에서는 자동으로 상단이 오지 않음
- 우호도 관리 가능

#### Option 2: 적대 상태에서도 거래 가능하게 (C# 수정 필요)
- Custom IncidentWorker 작성
- CanTradeNow 로직 오버라이드
- Harmony Patch 필요

## 참고사항
- 기존 RK_Faction_Pilgrims 팩션 구조 참조
- 히든 팩션2 (순례자들)은 이미 RK_Faction_Pilgrims (187~231 라인)로 구현되어 있음

