# 방패 방향 고정 커맨드 기능 분석 보고서

> **태그**: Shield FaceDirection Command Rotation Gizmo Pawn Direction Lock Guard Stance  
> **작성일**: 2026-03-14  
> **목적**: 방패 착용 시 커맨드로 특정 방향을 바라보게 하는 기능 구현을 위한 기술 분석

---

## 1. 요구사항 정리

| 항목 | 내용 |
|------|------|
| **트리거** | 방패 착용 상태에서 커맨드(기즈모) 클릭 |
| **동작** | 맵 위 임의 지점 클릭 → 해당 방향으로 제자리에서 바라봄 |
| **그래픽** | 캐릭터의 모든 그래픽(몸, 머리, 장비, 의류 등)이 해당 방향으로 렌더링 |
| **해제 조건** | 이동, 근접 전투, 기타 행동 시 자동 해제 |
| **유지 조건** | 해제 조건이 발생하기 전까지 계속 해당 방향 유지 |

---

## 2. 핵심 시스템 분석

### 2.1 Pawn.Rotation — 모든 그래픽의 근본 방향값

**`Pawn.Rotation`을 바꾸면 모든 그래픽이 자동으로 반영됩니다.**

렌더링 흐름:
```
Pawn.Rotation (Rot4)
  → PawnRenderer.ParallelGetPreRenderResults()에서 bodyFacing으로 사용
    → PawnDrawParms.facing에 전달
      → 텍스처 선택 (MatNorth/East/South/West)
      → 메시 선택 (MeshAt)
      → 오프셋/피벗/레이어 (DrawData)
      → 노드 표시 여부 (visibleFacing)
```

- `PawnDrawParms.ShouldRecache()`에서 `facing`이 바뀌면 캐시 무효화 → 즉시 반영
- **결론**: `Pawn.Rotation`만 제어하면 그래픽 문제는 해결됨

### 2.2 Pawn_RotationTracker — 방향 업데이트 로직

**파일**: `RimworldSource/Verse/Pawn_RotationTracker.cs`

`UpdateRotation()`이 매 프레임 호출되며, 다음 우선순위로 방향을 결정:

```
1. kindDef.useFixedRotation → 고정 방향 (동물 등)
2. pawn.jobs.HandlingFacing == true → Job이 방향 처리 중이므로 스킵 ★핵심★
3. stunner.Stunned && DisableRotation → 스턴 중 회전 비활성화
4. Stance_Busy + focusTarg → 조준/시전 대상 방향
5. 이동 중 → pather.nextCell 방향
6. 대기 중 + Job → CurJob.GetTarget(rotateToFace) 방향
7. Drafted → Rot4.South 고정
```

**핵심 포인트**: `pawn.jobs.HandlingFacing == true`이면 `UpdateRotation()`이 **아무것도 하지 않고 리턴**합니다.

### 2.3 HandlingFacing 메커니즘

```csharp
// Toil 설정
toil.handlingFacing = true;  // 이 Toil이 방향을 직접 관리함

// JobDriver에서 확인
public bool HandlingFacing => curToil != null && curToil.handlingFacing;
```

- `handlingFacing = true`인 Toil이 실행 중이면, `UpdateRotation()`이 스킵됨
- Toil 내부에서 직접 `FaceTarget()`, `Face()` 등을 호출하여 방향 제어

### 2.4 Job.overrideFacing — 대기 중 방향 고정

```csharp
// JobDriver_Wait.MakeNewToils()
if (this.job.overrideFacing != Rot4.Invalid)
{
    toil.handlingFacing = true;  // UpdateRotation 스킵
    // tick마다 overrideFacing 방향으로 FaceTarget 호출
}
```

- `Job.overrideFacing`에 `Rot4` 값을 설정하면, `Wait` Job 중 해당 방향을 계속 바라봄
- **이것이 가장 간단한 방향 고정 방법**

---

## 3. 구현 전략

### 전략 A: Job.overrideFacing 활용 (권장)

**원리**: 커맨드로 방향을 선택하면, 현재 Wait_Combat Job의 `overrideFacing`을 설정

```
[커맨드 클릭] → [맵 지점 선택] → [방향 계산(Rot4)] → [Wait_Combat Job에 overrideFacing 설정]
```

**장점**:
- 바닐라 시스템을 그대로 활용
- `handlingFacing = true`가 자동으로 설정되어 `UpdateRotation()` 간섭 없음
- 이동/새 Job 시작 시 자동으로 해제 (새 Job이 시작되면 이전 Job의 overrideFacing은 사라짐)

**단점**:
- `Wait_Combat` Job이 아닌 상태에서는 동작하지 않음 (징병 상태에서만 유효)
- 기존 Job을 교체해야 하므로 약간의 오버헤드

**구현 흐름**:
```csharp
// 1. Command_Target으로 방향 선택
Command_Target cmd = new Command_Target
{
    targetingParams = new TargetingParameters { canTargetLocations = true },
    action = delegate(LocalTargetInfo target)
    {
        // 2. 방향 계산
        IntVec3 pawnPos = pawn.Position;
        float angle = (target.Cell - pawnPos).ToVector3().AngleFlat();
        Rot4 facing = Pawn_RotationTracker.RotFromAngleBiased(angle);
        
        // 3. 새 Wait_Combat Job 생성 + overrideFacing 설정
        Job job = JobMaker.MakeJob(JobDefOf.Wait_Combat, pawnPos);
        job.overrideFacing = facing;
        pawn.jobs.TryTakeOrderedJob(job);
    }
};
```

### 전략 B: Comp 기반 방향 고정 + Harmony 패치

**원리**: CompStaminaShield에 방향 고정 상태를 저장하고, `Pawn_RotationTracker.UpdateRotation()`을 패치

```
[커맨드 클릭] → [맵 지점 선택] → [Comp에 lockedRotation 저장]
→ [UpdateRotation Prefix에서 lockedRotation 적용]
→ [이동/행동 감지 시 lockedRotation 해제]
```

**장점**:
- 징병 여부와 무관하게 동작 가능
- 더 세밀한 제어 가능 (해제 조건 커스터마이즈)
- 기존 Job 시스템에 간섭하지 않음

**단점**:
- Harmony 패치 필요 (UpdateRotation Prefix)
- 해제 조건을 직접 구현해야 함

**구현 흐름**:
```csharp
// CompStaminaShield에 추가
private Rot4 lockedFacing = Rot4.Invalid;
private bool isFacingLocked = false;

// Harmony Prefix
[HarmonyPatch(typeof(Pawn_RotationTracker), "UpdateRotation")]
static bool Prefix(Pawn_RotationTracker __instance, Pawn ___pawn)
{
    var shield = GetShieldComp(___pawn);
    if (shield != null && shield.isFacingLocked)
    {
        // 해제 조건 확인
        if (___pawn.pather.Moving || HasNewAction(___pawn))
        {
            shield.UnlockFacing();
            return true; // 원래 로직 실행
        }
        ___pawn.Rotation = shield.lockedFacing;
        return false; // 원래 로직 스킵
    }
    return true;
}
```

### 전략 비교

| 항목 | 전략 A (overrideFacing) | 전략 B (Harmony 패치) |
|------|------------------------|----------------------|
| 복잡도 | 낮음 | 중간 |
| 바닐라 호환성 | 높음 | 중간 (패치 의존) |
| 징병 전용 | O | X (범용 가능) |
| 해제 자동화 | 자동 (Job 교체 시) | 수동 구현 필요 |
| 그래픽 반영 | 자동 | 자동 |
| 다른 모드 충돌 | 낮음 | 중간 |

---

## 4. 커맨드(기즈모) 구현 패턴

### Command_Target 사용

```csharp
// CompGetWornGizmosExtra()에서 반환
yield return new Command_Target
{
    defaultLabel = "방패 방향 고정",
    defaultDesc = "클릭한 방향으로 방패를 들고 바라봅니다.",
    icon = /* 방패 아이콘 텍스처 */,
    targetingParams = new TargetingParameters
    {
        canTargetLocations = true,
        canTargetPawns = false,
        canTargetBuildings = false,
    },
    action = delegate(LocalTargetInfo target)
    {
        // 방향 계산 및 적용
    }
};
```

### 타겟팅 흐름
```
Command_Target 클릭
  → Targeter.BeginTargeting(params, action, onUpdate)
  → 마우스 커서가 타겟팅 모드로 변경
  → 맵 위 클릭
    → action(LocalTargetInfo) 호출
    → 방향 계산 및 적용
```

---

## 5. 해제 조건 분석

### 자동 해제가 필요한 상황

| 상황 | 감지 방법 | 전략 A | 전략 B |
|------|-----------|--------|--------|
| **이동 시작** | `pawn.pather.Moving` | 자동 (새 Job) | `UpdateRotation`에서 체크 |
| **근접 전투** | `Stance_Busy` 또는 새 Job | 자동 (새 Job) | `UpdateRotation`에서 체크 |
| **원거리 공격** | `Stance_Busy` | 자동 (새 Job) | `UpdateRotation`에서 체크 |
| **기타 행동** | 새 Job 시작 | 자동 (새 Job) | `pawn.CurJob` 변경 감지 |
| **징병 해제** | `!pawn.Drafted` | 자동 (새 Job) | `CompTick`에서 체크 |

**전략 A의 장점**: `TryTakeOrderedJob`으로 새 Job이 시작되면 이전 `overrideFacing` Job이 자동으로 교체되므로, 별도 해제 로직이 불필요합니다.

---

## 6. 기존 프로젝트 방패 코드 현황

### ApparelShield (ShieldOfRatkinia)
- `pawn.Rotation.AsAngle`을 사용하여 **바라보는 방향 기준 ±70도 범위**에서만 방어
- 이미 방향 기반 방어 로직이 존재 → 방향 고정 기능과 시너지

### CompStaminaShield (StaminaShield)
- 스태미나 기반 피해 감소 시스템
- `CompGetWornGizmosExtra()`에서 기즈모 반환 구조 이미 존재
- **여기에 Command_Target 기즈모를 추가하면 됨**

---

## 7. 권장 구현 방향

### 추천: 전략 A (overrideFacing) + 전략 B 하이브리드

1. **기본 동작**: `Command_Target`으로 방향 선택 → `Wait_Combat` Job에 `overrideFacing` 설정
2. **보조 안전장치**: `CompTick()`에서 방향 고정 상태 추적, 비정상 해제 시 정리
3. **커맨드 위치**: `CompStaminaShield.CompGetWornGizmosExtra()`에 추가

### 구현 시 고려사항

1. **징병 상태 확인**: 커맨드는 징병 상태에서만 활성화
2. **방패 상태 확인**: `ShieldState.Active`일 때만 커맨드 사용 가능
3. **시각적 피드백**: 방향 고정 중임을 나타내는 UI (아이콘 변경 또는 이펙트)
4. **사운드**: 방패를 드는 사운드 효과
5. **ApparelShield 시너지**: 방향 고정 시 해당 방향의 방어 범위가 명확해짐

---

## 8. 참조 파일 목록

| 파일 | 역할 |
|------|------|
| `RimworldSource/Verse/Pawn_RotationTracker.cs` | 방향 업데이트 핵심 로직 |
| `RimworldSource/Verse/Rot4.cs` | 4방향 구조체 |
| `RimworldSource/Verse/Thing.cs` | `Rotation` 프로퍼티 |
| `RimworldSource/Verse/PawnRenderer.cs` | 렌더링에서 Rotation 사용 |
| `RimworldSource/Verse/PawnDrawParms.cs` | 렌더 파라미터 (facing) |
| `RimworldSource/Verse/AI/Pawn_PathFollower.cs` | 이동 시스템 |
| `RimworldSource/Verse/AI/JobDriver_Wait.cs` | 대기 Job (overrideFacing 처리) |
| `RimworldSource/Verse/AI/Pawn_JobTracker.cs` | Job 관리 |
| `Project/1.6/Source/StaminaShield/CompStaminaShield.cs` | 기존 방패 Comp |
| `Project/1.6/Source/ShieldOfRatkinia/ApparelShield.cs` | 기존 방향 기반 방어 |
