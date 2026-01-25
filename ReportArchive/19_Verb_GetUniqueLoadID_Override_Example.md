# Custom Verb 클래스에서 GetUniqueLoadID() 오버라이드 방법

## 개요

RimWorld의 기본 `Verb` 클래스는 `GetUniqueLoadID()` 메서드를 다음과 같이 구현합니다:

```461:464:RimworldSource/Verse/Verb.cs
public string GetUniqueLoadID()
{
	return "Verb_" + this.loadID;
}
```

이 메서드를 Custom Verb 클래스에서 오버라이드하여 고유한 접두사를 추가하면, RimWorld의 기본 Verb와 LoadID 충돌을 피할 수 있습니다.

## 구현 방법

### 1. 기존 Verb 클래스 수정 예시

현재 프로젝트에 있는 `Verb_MeleeAttackDamage` 클래스를 수정하는 방법:

```csharp
using RimWorld;
using Verse;

namespace NewRatkin
{
    public class Verb_MeleeAttackDamage : Verb_MeleeAttack
    {
        // GetUniqueLoadID() 오버라이드
        public override string GetUniqueLoadID()
        {
            // 고유 접두사 추가로 RimWorld 기본 Verb와 구분
            return "RK_Verb_" + this.loadID;
        }

        // 기존 메서드들...
        protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
        {
            return null;
        }

        protected override bool TryCastShot()
        {
            // 기존 구현...
        }
    }
}
```

### 2. 동작 원리

**변경 전 (RimWorld 기본):**
- LoadID: `CompEquippable_RK_Gunlance_NormalType85822_0_Stab`
- GetUniqueLoadID(): `Verb_CompEquippable_RK_Gunlance_NormalType85822_0_Stab`

**변경 후 (Custom 오버라이드):**
- LoadID: `CompEquippable_RK_Gunlance_NormalType85822_0_Stab` (동일)
- GetUniqueLoadID(): `RK_Verb_CompEquippable_RK_Gunlance_NormalType85822_0_Stab` (고유 접두사 추가)

### 3. 장점

1. **간단한 구현**: 기존 클래스에 메서드 하나만 추가
2. **명확한 구분**: Custom Verb와 기본 Verb의 LoadID가 명확히 구분됨
3. **충돌 방지**: RimWorld 기본 `Verb_MeleeAttackDamage`와 LoadID가 달라 충돌 없음

### 4. 단점 및 주의사항

#### ⚠️ 주의 1: 저장된 게임 호환성

**문제:**
- 이미 저장된 게임에서는 `Verb_...` 접두사로 저장됨
- 새 버전으로 불러올 때 `RK_Verb_...`로 변경되면서 저장된 Verb를 찾지 못할 수 있음

**해결 방법:**
```csharp
public override string GetUniqueLoadID()
{
    // 저장된 게임 호환성을 위해 조건부 처리
    if (this.loadID != null && !this.loadID.StartsWith("RK_"))
    {
        // 기존 저장 파일에서는 기본 형식 유지
        return "Verb_" + this.loadID;
    }
    // 새로 생성되는 Verb는 고유 접두사 사용
    return "RK_Verb_" + this.loadID;
}
```

#### ⚠️ 주의 2: RimWorld 내부 참조

**문제:**
- RimWorld 내부에서 `GetUniqueLoadID()`를 사용하여 Verb를 찾는 경우
- 접두사가 달라지면 찾지 못할 수 있음

**확인 필요:**
- `LoadedObjectDirectory`에서 Verb를 조회할 때 `GetUniqueLoadID()` 사용 여부 확인
- 대부분의 경우 문제 없지만, 일부 특수한 경우 충돌 가능

#### ⚠️ 주의 3: 다른 모드와의 호환성

**문제:**
- 다른 모드도 같은 방식으로 접두사를 사용할 경우 충돌 가능
- RimWorld 기본 ManeuverDef의 verbClass를 변경하면 다른 모드에 영향

**권장:**
- 고유한 접두사 사용 (예: `RK_` 대신 `RK_NewRatkin_` 같은 더 구체적인 접두사)

### 5. 완전한 구현 예시 (호환성 고려)

```csharp
using RimWorld;
using Verse;

namespace NewRatkin
{
    public class Verb_MeleeAttackDamage : Verb_MeleeAttack
    {
        private const string LoadIDPrefix = "RK_Verb_";

        /// <summary>
        /// 고유 LoadID를 반환하여 RimWorld 기본 Verb와 구분
        /// </summary>
        public override string GetUniqueLoadID()
        {
            if (string.IsNullOrEmpty(this.loadID))
            {
                return LoadIDPrefix + "Unknown";
            }

            // 이미 고유 접두사가 있으면 그대로 반환
            if (this.loadID.StartsWith("RK_"))
            {
                return LoadIDPrefix + this.loadID;
            }

            // 저장된 게임 호환성: 기존 형식도 지원
            // 하지만 새로 생성되는 Verb는 고유 접두사 사용
            // 저장된 Verb는 InitVerbs에서 기존 loadID를 유지하므로
            // 이 경우도 고유 접두사를 추가
            return LoadIDPrefix + this.loadID;
        }

        // 기존 메서드들...
        protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
        {
            return null;
        }

        protected override bool TryCastShot()
        {
            // 기존 구현...
        }
    }
}
```

### 6. 대안: InitVerb 전에 처리

만약 `GetUniqueLoadID()` 오버라이드만으로 해결되지 않는다면, Harmony 패치로 `InitVerb` 메서드를 수정할 수도 있습니다:

```csharp
[HarmonyPatch(typeof(VerbTracker), "InitVerb")]
public static class VerbTracker_InitVerb_Patch
{
    static void Postfix(VerbTracker __instance, Verb verb, VerbProperties properties, Tool tool, ManeuverDef maneuver, string id)
    {
        if (verb is Verb_MeleeAttackDamage)
        {
            // LoadID를 설정하기 전에 기존 등록 해제
            // 또는 loadID에 고유 접두사 추가
            if (!id.StartsWith("RK_"))
            {
                verb.loadID = "RK_" + id;
            }
        }
    }
}
```

## 결론

**가장 간단한 해결책:**
1. `Verb_MeleeAttackDamage` 클래스에 `GetUniqueLoadID()` 오버라이드 추가
2. 고유 접두사 `"RK_Verb_"` 사용
3. 저장된 게임 호환성 확인 (필요시 조건부 처리)

**더 안전한 해결책:**
- Harmony 패치로 `InitVerb` 수정 (방안 1)
- 또는 `RK_Gunlance_NormalType`에서 `Stab` capacity 제거 (방안 2)
