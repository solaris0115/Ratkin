<!-- Verb LoadID CompEquippable CompEquippableAbility RK_HeavyLance 세이브 로드 -->

# Verb LoadID 중복 버그 리포트

## 에러 메시지

```
Cannot register RimWorld.Verb_MeleeAttackDamage RimWorld.Verb_MeleeAttackDamage(null), 
(id=Verb_CompEquippable_RK_HeavyLance76514_0_Stab in loaded object directory. 
Id already used by RimWorld.Verb_MeleeAttackDamage RimWorld.Verb_MeleeAttackDamage(null).
```

## 발생 상황

- **시점**: RK_HeavyLance에 CompProperties_EquippableAbility(돌진 능력) 추가 후
- **조건**: 기존 세이브 파일 로드 시

## 원인

Def 상속으로 **CompEquippable**과 **CompEquippableAbility**가 동시에 존재:

- `BaseWeapon` → `CompEquippable` (verbTracker)
- RK_HeavyLance에 `CompProperties_EquippableAbility` 추가 → `CompEquippableAbility` (verbTracker)

동일 LoadID(`Verb_CompEquippable_RK_HeavyLance76514_0_Stab`)로 두 verbTracker가 등록되어 충돌.

## 해결

`Weapon_Melee.xml` RK_HeavyLance에 `comps Inherit="False"` 적용 후 필요한 comps를 명시적으로 나열. `CompEquippable` 대신 `CompProperties_EquippableAbility`만 사용.

## 수정 파일

- [Project/1.6/Defs/ThingsDefs/Weapon_Melee.xml](Project/1.6/Defs/ThingsDefs/Weapon_Melee.xml)

## 참고

- [WorkFlow/45_Verb_DuplicateID_CompEquippableAbility_Analysis.md](WorkFlow/45_Verb_DuplicateID_CompEquippableAbility_Analysis.md) - Gunlance 동일 이슈 분석
- [Report/45_CompEquippable_VerbTracker_Duplication_Analysis_Report.md](Report/45_CompEquippable_VerbTracker_Duplication_Analysis_Report.md) - 상세 분석
- RimWorld Odyssey `Weapons_Unique.xml`: `comps Inherit="False"` 패턴 사용

## 기존 세이브

이미 저장된 세이브는 verbTracker 구조가 꼬여 있어 오류가 계속 발생할 수 있음. **새 게임** 시작 시 정상 동작.
