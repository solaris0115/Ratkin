# RK_Rifle_line Sector Shot 타겟팅 미동작 원인 분석

## 원인 1: `targetParams.canTargetLocations` 미설정 (가장 유력)

**문제**: `VerbProperties` 기본 `TargetingParameters`는 `canTargetLocations = false`입니다.

**영향**: 
- 부채꼴 샷은 **지면(빈 셀)**을 타겟해야 함
- `TargetingParameters.CanTarget()`에서 `targ.Thing == null`이면 `canTargetLocations`만 반환
- `false`이면 빈 셀을 타겟할 수 없어, 지면 타겟 시 유효 타겟이 없음

**참고**: 림월드 원본 무기들(AssaultRifle, Grenade 등)은 verb에 다음을 명시합니다:
```xml
<targetParams>
  <canTargetLocations>true</canTargetLocations>
</targetParams>
```

**해결**: RK_Rifle_line verb에 `targetParams.canTargetLocations` 추가

---

## 원인 2: 공격 커맨드 비활성화 조건

**조건**: `VerbTracker.CreateVerbTargetCommand`에서 다음이면 커맨드가 Disable 됨:
- `!verb.CasterPawn.Drafted` → **소집되지 않은 경우**
- `verb.CasterPawn.WorkTagIsDisabled(WorkTags.Violent)` → 폭력 불가
- `verb.caster.Faction != Faction.OfPlayer` → 플레이어 소속 아님

**확인**: 소집(Draft) 상태에서 B 키로 공격을 시도했는지 확인 필요.

---

## 원인 3: `verb.caster` null 가능성

**흐름**: 
- `VerbTracker` 생성 시 `verb.caster = ConstantCaster` (CompEquippable은 null)
- `Pawn_EquipmentTracker.Notify_EquipmentAdded`에서 `verb.caster = pawn` 설정

**가능성**: 세이브/로드 후 equipment 복원 시점에 `caster`가 null일 수 있음.  
이 경우 `CreateVerbTargetCommand`의 `verb.caster.Faction` 접근 시 NullReferenceException 발생 가능.

---

## 권장 수정

1. **Weapon_Range.xml** – verb에 `targetParams` 추가:
```xml
<targetParams>
  <canTargetLocations>true</canTargetLocations>
</targetParams>
```

2. **사용 확인**: 소집 상태에서 공격 커맨드(B) 사용 여부 확인
