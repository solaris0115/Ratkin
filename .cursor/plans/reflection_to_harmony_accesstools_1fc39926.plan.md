---
name: Reflection to Harmony AccessTools
overview: Verb_SectorShot의 Reflection(FieldInfo) 코드를 Harmony의 AccessTools/Traverse로 대체하여 일관성 있고 간결한 코드로 변경한다.
todos:
  - id: replace-reflection
    content: FieldInfo 3개를 AccessTools.FieldRefAccess로 교체하고, using 변경, TriggerForceFieldCone 내 SetValue를 ref 대입으로 변경
    status: completed
isProject: false
---

# Reflection을 Harmony AccessTools로 대체

## 현재 상태

[Verb_SectorShot.cs](Project/1.6/Source/SectorShot/Verb_SectorShot.cs) 23~25행에서 `System.Reflection`의 `FieldInfo`를 직접 사용:

```csharp
private static readonly FieldInfo _lastInterceptAngle = typeof(CompProjectileInterceptor).GetField("lastInterceptAngle", BindingFlags.Instance | BindingFlags.NonPublic);
private static readonly FieldInfo _lastInterceptTicks = typeof(CompProjectileInterceptor).GetField("lastInterceptTicks", BindingFlags.Instance | BindingFlags.NonPublic);
private static readonly FieldInfo _drawInterceptCone = typeof(CompProjectileInterceptor).GetField("drawInterceptCone", BindingFlags.Instance | BindingFlags.NonPublic);
```

`TriggerForceFieldCone`에서 `FieldInfo.SetValue`로 값 설정.

## 대체 방안: AccessTools.FieldRefAccess

프로젝트에서 이미 `AccessTools.Field`, `AccessTools.Method`, `Traverse` 등을 사용 중 ([Comp_PulseRifleFireMode.cs](Project/1.6/Source/PrototypePulseRifle/Comp_PulseRifleFireMode.cs), [HamsterWheel/Job.cs](Project/1.6/Source/HamsterWheel/Job.cs) 등).

Harmony의 `AccessTools.FieldRefAccess<T, F>`를 사용하면 boxing 없이 ref로 직접 접근 가능:

```csharp
private static readonly AccessTools.FieldRef<CompProjectileInterceptor, float> _lastInterceptAngle =
    AccessTools.FieldRefAccess<CompProjectileInterceptor, float>("lastInterceptAngle");
private static readonly AccessTools.FieldRef<CompProjectileInterceptor, int> _lastInterceptTicks =
    AccessTools.FieldRefAccess<CompProjectileInterceptor, int>("lastInterceptTicks");
private static readonly AccessTools.FieldRef<CompProjectileInterceptor, bool> _drawInterceptCone =
    AccessTools.FieldRefAccess<CompProjectileInterceptor, bool>("drawInterceptCone");
```

사용 시:

```csharp
_lastInterceptAngle(comp) = angle;
_lastInterceptTicks(comp) = Find.TickManager.TicksGame;
_drawInterceptCone(comp) = true;
```

## 변경 사항

- `using System.Reflection;` 제거, `using HarmonyLib;` 추가
- static FieldInfo 3개 -> `AccessTools.FieldRef` 3개로 교체
- `TriggerForceFieldCone` 내 `SetValue` 호출 -> ref 대입으로 변경
- null 체크 불필요 (FieldRefAccess는 필드 못 찾으면 초기화 시 예외)

## 장점

- 프로젝트 내 다른 파일들과 일관된 패턴
- boxing/unboxing 없음 (FieldInfo.SetValue는 값 타입에 boxing 발생)
- 코드가 더 간결

