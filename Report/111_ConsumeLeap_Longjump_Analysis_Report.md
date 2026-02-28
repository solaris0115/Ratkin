# ConsumeLeap & Longjump 비행/돌진 시스템 분석 보고서

> **태그**: ConsumeLeap Longjump PawnFlyer Jump Leap Devourer Ability Verb Charge Rush Dash RatkinLance  
> **목적**: 랫킨 랜스에 "대상에게 돌진" 기능 구현을 위한 사전 분석  
> **분석 대상**: Anomaly DLC `ConsumeLeap_Devourer`, Biotech DLC `Longjump`

---

## 1. 시스템 아키텍처 개요

두 능력 모두 **동일한 PawnFlyer 기반 점프 시스템**을 공유하며, 차이는 "도착 후 효과"와 "타겟 유형"에 있다.

```
[Ability 사용]
    │
    ▼
[Verb_CastAbilityJump / Verb_CastAbilityConsumeLeap]
    │
    ▼
[JumpUtility.DoJump()]
    │  - PawnFlyer 생성
    │  - Pawn DeSpawn (맵에서 제거)
    │  - PawnFlyer를 목적지 셀에 Spawn
    ▼
[PawnFlyer.TickInterval()] ← 매 틱 실행
    │  - 포물선 궤적 계산
    │  - 비행 시간 체크
    │  - 목적지 유효성 검증 (15틱 간격)
    ▼
[PawnFlyer.RespawnPawn()] ← 비행 완료 시
    │  - Pawn 재스폰
    │  - Draft/Job 상태 복원
    │  - 스턴 적용 (설정 시)
    │  - ICompAbilityEffectOnJumpCompleted 콜백
    ▼
[착지 후 효과]
    ├─ Longjump: 없음 (단순 이동)
    └─ ConsumeLeap: CompDevourer.StartDigesting() → 잡아먹기
```

---

## 2. Longjump (원거리 점프) 상세 분석

### 2.1 Def 정의

```xml
<AbilityDef Name="LongJump">
  <defName>Longjump</defName>
  <cooldownTicksRange>60</cooldownTicksRange>           <!-- 1초 쿨다운 -->
  <hostile>false</hostile>                               <!-- 비적대 행동 -->
  <verbProperties>
    <verbClass>Verb_CastAbilityJump</verbClass>
    <warmupTime>0.5</warmupTime>                         <!-- 0.5초 준비 -->
    <range>19.9</range>                                  <!-- 약 20칸 사거리 -->
    <requireLineOfSight>true</requireLineOfSight>
    <targetParams>
      <canTargetLocations>true</canTargetLocations>      <!-- 지면 타겟팅 -->
      <canTargetPawns>false</canTargetPawns>             <!-- Pawn 타겟 불가 -->
      <canTargetBuildings>false</canTargetBuildings>     <!-- 건물 타겟 불가 -->
    </targetParams>
  </verbProperties>
  <comps>
    <li Class="CompProperties_AbilityHemogenCost">
      <hemogenCost>0.05</hemogenCost>                    <!-- 헤모겐 5% 소모 -->
    </li>
  </comps>
</AbilityDef>
```

### 2.2 사용하는 PawnFlyer

기본 `PawnFlyer` (ThingDefOf.PawnFlyer) 사용:

| 속성 | 값 | 설명 |
|------|-----|------|
| flightDurationMin | 0.5초 | 최소 비행 시간 |
| flightSpeed | 12 | 비행 속도 (칸/초) |
| heightFactor | 2 | 포물선 높이 계수 |
| stunDurationTicksRange | 0 | 착지 스턴 없음 |

### 2.3 타겟 필터

- **지면(Location)만** 타겟 가능
- Pawn, 건물 타겟 불가
- 시야(Line of Sight) 필요
- 유효한 착지 지점: 걸을 수 있고, 안개 없고, 장애물 없는 셀

### 2.4 핵심 특징

- **순수 이동 능력**: 착지 후 추가 효과 없음
- **비적대적**: hostile=false, violent=false
- **짧은 쿨다운**: 60틱(1초) → 거의 연속 사용 가능
- **헤모겐 소모**: 5%로 저렴

---

## 3. ConsumeLeap_Devourer (포식 도약) 상세 분석

### 3.1 Def 정의

```xml
<AbilityDef>
  <defName>ConsumeLeap_Devourer</defName>
  <cooldownTicksRange>3600</cooldownTicksRange>          <!-- 1분 쿨다운 -->
  <ai_IsOffensive>true</ai_IsOffensive>
  <aiCanUse>true</aiCanUse>
  <verbProperties>
    <verbClass>Verb_CastAbilityConsumeLeap</verbClass>
    <warmupTime>0.25</warmupTime>                        <!-- 0.25초 준비 -->
    <range>9.9</range>                                   <!-- 약 10칸 사거리 -->
    <requireLineOfSight>true</requireLineOfSight>
    <targetParams>
      <canTargetBuildings>false</canTargetBuildings>     <!-- 건물 불가, Pawn 가능 -->
    </targetParams>
  </verbProperties>
  <comps>
    <li Class="CompProperties_ConsumeLeap">
      <maxBodySize>2.5</maxBodySize>                     <!-- 체형 2.5 이하만 -->
    </li>
  </comps>
</AbilityDef>
```

### 3.2 사용하는 PawnFlyer

`PawnFlyer_ConsumeLeap` 사용 (PawnFlyerBase 상속):

| 속성 | 값 | 설명 |
|------|-----|------|
| flightDurationMin | 0.5초 | 상속 |
| flightSpeed | 12 | 상속 |
| **heightFactor** | **0.5** | **낮은 궤적** (기본 2 대비 1/4) |
| stunDurationTicksRange | 0 | 착지 스턴 없음 |

> **heightFactor 0.5**: 높이 뛰지 않고 낮게 날아감 → "도약해서 덮치는" 연출

### 3.3 타겟 필터 (다중 레이어)

**레이어 1 - Verb targetParams (XML)**
- `canTargetBuildings=false` → 건물 불가
- Pawn 타겟 가능 (기본값 true)
- Location 타겟 가능 (기본값 true)

**레이어 2 - CompAbilityEffect_ConsumeLeap.Valid()**
```csharp
public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
{
    return target.Pawn.BodySize <= this.Props.maxBodySize  // BodySize ≤ 2.5
        && base.Valid(target, throwMessages);
}
```

**레이어 3 - AI 전용 필터 (AICanTargetNow)**
```csharp
public override bool AICanTargetNow(LocalTargetInfo target)
{
    Pawn pawn = target.Pawn;
    return pawn != null && pawn.BodySize <= this.Props.maxBodySize;
}
```

### 3.4 착지 후 효과 - 소화 시스템

착지 시 `ICompAbilityEffectOnJumpCompleted` 인터페이스를 통해 콜백:

```
PawnFlyer.RespawnPawn()
    └─ CompAbilityEffect_ConsumeLeap.OnJumpCompleted(origin, target)
        └─ CompDevourer.StartDigesting(origin, target)
```

**StartDigesting 상세 플로우:**

1. 타겟 Pawn에 산성 데미지 99 적용 (Lord에 피해 알림)
2. 타겟 Pawn을 **DeSpawn** (맵에서 제거)
3. 타겟을 `innerContainer`에 보관 (삼킨 상태)
4. `JobDefOf.DevourerDigest` 작업 시작 (소화 대기)
5. 소화 애니메이션 재생 (`DevourerDigesting`)
6. 전투 로그 기록

**소화 시간 (BodySize → 초):**

| BodySize | 소화 시간 |
|----------|----------|
| 0.2 | 10초 |
| 1.0 | 60초 |
| 3.5 | 90초 |

**소화 중단 조건:**
- Devourer가 기절(Downed)하면 → 뱉어냄 + 산성 데미지
- Devourer가 죽으면 → 뱉어냄 + 산성 데미지
- 시간 데미지 커브: 0초→5dmg, 60초→35dmg

**소화 완료:**
- 125 산성 데미지 (SetApplyAllDamage) → 사실상 즉사

---

## 4. 핵심 비교표

| 항목 | Longjump | ConsumeLeap |
|------|----------|-------------|
| **Verb 클래스** | Verb_CastAbilityJump | Verb_CastAbilityConsumeLeap |
| **PawnFlyer** | PawnFlyer (기본) | PawnFlyer_ConsumeLeap |
| **비행 높이** | heightFactor=2 (높은 포물선) | heightFactor=0.5 (낮은 도약) |
| **비행 속도** | 12칸/초 | 12칸/초 (동일) |
| **사거리** | 19.9칸 | 9.9칸 |
| **쿨다운** | 60틱 (1초) | 3600틱 (1분) |
| **준비 시간** | 0.5초 | 0.25초 |
| **타겟 유형** | 지면만 | Pawn만 (BodySize ≤ 2.5) |
| **적대성** | 비적대 | 적대 |
| **착지 효과** | 없음 | 잡아먹기 (소화 시작) |
| **비행 중 상태** | DeSpawn (무적/타겟불가) | DeSpawn (무적/타겟불가) |
| **착지 스턴** | 없음 | 없음 (대신 소화 시작) |
| **비용** | 헤모겐 5% | 없음 |

---

## 5. 비행 중 상태 상세 분석 (PawnFlyer)

### 5.1 무적/타겟불가 메커니즘

```csharp
// PawnFlyer.MakeFlyer() 내부
pawn.DeSpawn(DestroyMode.WillReplace);  // ← 핵심!
pawnFlyer.innerContainer.TryAdd(pawn, true);
```

**Pawn이 맵에서 완전히 제거(DeSpawn)됨:**
- 맵에 존재하지 않으므로 **타겟팅 불가**
- 공격 대상으로 선택 불가
- 사실상 **무적 상태**
- PawnFlyer Thing이 대신 맵에 존재하지만, 이것은 공격 대상이 아님

### 5.2 비행 시간 계산

```csharp
float num = Mathf.Max(flightDistance, 1f) / flightSpeed;  // 거리/속도
num = Mathf.Max(num, flightDurationMin);                   // 최소 시간 보장
ticksFlightTime = num.SecondsToTicks();                    // 초 → 틱 변환
```

예시 (flightSpeed=12):
- 6칸 거리: 0.5초 (최소값 적용)
- 12칸 거리: 1.0초
- 20칸 거리: 1.67초

### 5.3 궤적 계산

```csharp
float t = ticksFlying / ticksFlightTime;           // 진행률 0~1
float t2 = Worker.AdjustedProgress(t);             // 커브 적용
float height = Worker.GetHeight(t2);               // 역포물선: 4t(1-t)
Vector3 groundPos = Lerp(startVec, destPos, t2);   // 지면 위치 보간
Vector3 effectivePos = groundPos + height * heightFactor; // 최종 위치
```

**역포물선 높이 함수**: `GenMath.InverseParabola(t) = 4 * t * (1 - t)`
- t=0: 높이 0 (출발)
- t=0.5: 높이 1 (최고점)
- t=1: 높이 0 (착지)

heightFactor에 의해 최종 높이가 결정됨:
- Longjump: 최고점 높이 = 1 × 2 = **2** (높이 뜀)
- ConsumeLeap: 최고점 높이 = 1 × 0.5 = **0.5** (낮게 도약)

### 5.4 비행 중 목적지 변경

15틱마다 목적지 유효성 검사. 무효하면 3.9칸 반경에서 대체 착지점 탐색:

```csharp
private void CheckDestination()
{
    if (JumpUtility.ValidJumpTarget(FlyingThing, Map, destCell)) return;
    // 반경 3.9칸 내 유효한 셀 탐색
    for (int i = 0; i < GenRadial.NumCellsInRadius(3.9f); i++) { ... }
}
```

---

## 6. 랫킨 랜스 돌진 기능 구현을 위한 분석

### 6.1 기존 시스템의 한계 (사용자 요구사항 대비)

| 사용자 요구 | PawnFlyer 시스템 | 차이 |
|------------|-----------------|------|
| 대상에게 빠르게 걸어가기 | 공중 비행 (DeSpawn) | **맵에서 사라짐** |
| 타겟팅 가능 상태 유지 | 타겟팅 불가 (DeSpawn) | **무적 상태** |
| 짧은 거리 빠른 이동 | 포물선 궤적 비행 | **연출이 다름** |
| 이동 속도 보너스 형태 | 순간이동에 가까움 | **이동이 아닌 비행** |

### 6.2 구현 접근법 제안

**PawnFlyer 시스템은 부적합** → 다른 접근이 필요

#### 접근법 A: 이동속도 버프 + 강제 이동 (추천)

```
[Ability 사용] → 대상 Pawn 지정
    │
    ▼
[돌진 상태 시작]
    ├─ 대상 방향으로 이동 시 속도 보너스 (Hediff + StatModifier)
    ├─ 강제 이동 Job 부여 (대상 위치로 Goto)
    ├─ Pawn은 맵에 존재 (타겟팅 가능, 공격 받을 수 있음)
    └─ 도착 판정 (인접 셀 도달 시)
         └─ 효과 발동 (공격, 넉백 등)
```

핵심 구현 요소:
- **Hediff**: 돌진 상태를 나타내는 Hediff (이동속도 보너스 + 시각 효과)
- **JobDriver**: 대상을 향해 이동하는 커스텀 Job
- **어뷰징 방지**: 대상 방향으로 이동할 때만 속도 보너스 적용
  - 매 틱 현재 이동 방향과 대상 방향의 각도 차이 계산
  - 일정 각도 이내일 때만 보너스 유지
  - 대상에서 멀어지면 돌진 취소

#### 접근법 B: PawnFlyer 커스텀 (낮은 비행)

heightFactor를 0이나 극히 낮은 값으로 설정하면 "미끄러지듯 이동"하는 연출 가능.
단, **여전히 DeSpawn 상태**이므로 타겟팅 불가/무적 문제는 해결 안 됨.

#### 접근법 C: 하이브리드 (짧은 PawnFlyer + 도착 효과)

ConsumeLeap처럼 낮은 heightFactor(0.1~0.3)로 짧은 거리를 빠르게 이동.
- 짧은 거리(3~5칸)이므로 무적 시간이 매우 짧음 (0.3~0.5초)
- 도착 후 즉시 공격 효과 발동
- 어뷰징 여지가 적음 (짧은 거리 + 긴 쿨다운)

### 6.3 참고할 수 있는 기존 시스템

| 시스템 | 위치 | 특징 |
|--------|------|------|
| PawnFlyer_Stun | Core | 착지 후 스턴 부여 (60~180틱) |
| ConsumeLeap | Anomaly | 낮은 궤적(0.5) + 착지 후 효과 |
| JumpPack | Royalty | 장비 기반 점프, 충전식 |
| Verb_MeleeAttack | Core | 근접 공격 접근 로직 참고 |

---

## 7. 클래스 상속 구조

```
Verb (Base)
 └─ Verb_CastAbility
     └─ Verb_CastAbilityJump          ← Longjump 사용
         └─ Verb_CastAbilityConsumeLeap  ← ConsumeLeap 사용
              (JumpFlyerDef만 오버라이드)

Thing (Base)
 └─ PawnFlyer                          ← 비행 컨테이너
      ├─ PawnFlyer (기본)              ← Longjump용
      └─ PawnFlyer_ConsumeLeap         ← ConsumeLeap용 (heightFactor=0.5)

CompAbilityEffect (Base)
 └─ CompAbilityEffect_ConsumeLeap      ← 착지 후 소화 트리거
      implements ICompAbilityEffectOnJumpCompleted

ThingComp (Base)
 └─ CompDevourer                       ← 소화 시스템 (IThingHolder)
```

---

## 8. 결론 및 권장사항

### 핵심 발견

1. **PawnFlyer 비행 중에는 반드시 DeSpawn** → 타겟팅 불가 + 사실상 무적
2. **heightFactor로 궤적 높이 조절 가능** → 낮은 값이면 "도약" 연출
3. **ICompAbilityEffectOnJumpCompleted**로 착지 후 효과를 깔끔하게 분리 가능
4. **ConsumeLeap은 Longjump의 확장** → Verb만 상속하고 FlyerDef + Comp만 교체

### 랫킨 랜스 돌진 구현 권장

사용자의 의도("타겟팅 가능한 상태에서 빠르게 이동")를 고려하면:

- **짧은 거리(5~8칸) + 매우 낮은 heightFactor(0.1)의 PawnFlyer** 방식이 가장 현실적
- 무적 시간이 0.5초 미만으로 매우 짧아 게임플레이 영향 최소
- 기존 시스템을 최대한 활용하여 구현 복잡도 낮춤
- 착지 후 `ICompAbilityEffectOnJumpCompleted`로 공격/효과 발동
- 또는 완전히 새로운 **이동속도 버프 기반 돌진**을 구현하면 "타겟팅 가능" 요구사항을 완벽히 충족

---

*보고서 생성일: 2026-02-28*  
*분석 소스: RimWorld 1.6 Source Code (Anomaly DLC, Biotech DLC, Core)*
