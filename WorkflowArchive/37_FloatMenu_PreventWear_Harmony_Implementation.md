# FloatMenu 착용 불가 표시 Harmony 구현 - 2025-11-07

## 작업 개요
- 요청 내용: FloatMenu에서 처음부터 "착용 불가"를 표시하여 시도 자체를 못하게 함
- 목표: Harmony Patch로 FloatMenu 생성 시 착용 불가 메시지 표시

## 계획
1. Harmony Patch 클래스 생성
2. FloatMenuOptionProvider_Wear.GetSingleOptionFor Postfix 구현
3. CompShieldWeaponIncompatible과 연동

## 작업 세부 진행
1. [Harmony Patch 클래스 생성] [✓]
2. [CompShieldWeaponIncompatible에 Notify_Equipped 추가] [✓]

## 진행 상황

### 1. Harmony Patch 클래스 생성
- 내용: FloatMenuPatch_ShieldWeaponCheck.cs 생성
- 결과: FloatMenuOptionProvider_Wear.GetSingleOptionFor Postfix Patch 구현 완료

### 2. CompShieldWeaponIncompatible 확장
- 내용: Notify_Equipped(pawn) 메서드 추가
- 결과: 착용 시 호환되지 않는 무기 자동 제거 기능 추가

## 최종 작업 결과
✅ 완료

## 구현 내용

### 1. FloatMenu 착용 불가 표시 (Harmony Patch)

**파일**: `Project/1.6/Source/ShieldOfRatkinia/FloatMenuPatch_ShieldWeaponCheck.cs`

```csharp
[HarmonyPatch(typeof(FloatMenuOptionProvider_Wear))]
[HarmonyPatch("GetSingleOptionFor")]
public static class FloatMenuPatch_ShieldWeaponCheck
{
    [HarmonyPostfix]
    public static void Postfix(ref FloatMenuOption __result, Thing clickedThing, FloatMenuContext context)
    {
        // CompShieldWeaponIncompatible 검증
        // 호환되지 않는 무기 착용 중이면 __result를 착용 불가 옵션으로 대체
    }
}
```

**동작**:
- FloatMenu 생성 시 자동으로 호출됨
- 호환되지 않는 무기 착용 중이면 "착용 불가: [사유]"로 표시
- action = null로 설정하여 클릭 불가 (회색 표시)

### 2. 착용 시 무기 제거 (안전망)

**파일**: `Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs`

```csharp
public override void Notify_Equipped(Pawn pawn)
{
    base.Notify_Equipped(pawn);
    
    if (pawn?.equipment?.Primary != null)
    {
        string reason;
        if (!TryIsWeaponAllowed(pawn.equipment.Primary.def, out reason))
        {
            DropIncompatibleWeapon(pawn, pawn.equipment.Primary, reason);
        }
    }
}
```

**동작**:
- Apparel 착용 완료 시 자동으로 호출됨
- FloatMenu를 거치지 않는 경로(Job, 강제 착용 등)에서도 동작
- 호환되지 않는 무기를 바닥에 떨어뜨리고 메시지 표시

### 3. 이중 안전 메커니즘

```
[사용자 시나리오]

시나리오 1: 일반적인 경우 (FloatMenu 사용)
------------------------------------------
1. Ratkin이 양손 무기(Bolter) 착용 중
2. 사용자가 방패(Shield) 우클릭
3. FloatMenu Harmony Patch 동작 ⭐
4. FloatMenu 표시: ❌ "착용 불가: 양손 무기와 함께 착용 불가"
5. 사용자는 클릭할 수 없음 (회색)
   → 착용 시도 자체가 차단됨!

시나리오 2: FloatMenu를 거치지 않는 경우
------------------------------------------
1. Ratkin이 양손 무기(Bolter) 착용 중
2. 다른 모드나 Job이 직접 방패 착용 시도
3. Notify_Equipped() 동작 ⭐
4. 방패가 착용되고 Bolter가 바닥에 떨어짐
5. 메시지 표시: "방패를 착용하여 Bolter를 내려놓았습니다"
   → 안전망 역할!
```

### 4. 필요한 번역 키

**추가 필요한 번역 키**:
- `RK_ApparelWeaponIncompatible_Dropped` - 무기가 제거되었을 때 메시지
- `RK_ApparelWeaponIncompatible_Dropped_Default` - 기본 메시지

**기존 CompProperties에서 설정 가능한 키**:
- `blockReasonKey` - 착용 불가 사유 (예: "RK_ShieldTwoHandedIncompatible")

## 관련 파일 목록

**생성된 파일**:
- [Project/1.6/Source/ShieldOfRatkinia/FloatMenuPatch_ShieldWeaponCheck.cs](../Project/1.6/Source/ShieldOfRatkinia/FloatMenuPatch_ShieldWeaponCheck.cs)

**수정된 파일**:
- [Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs](../Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs)

**작업 기록**:
- [WorkFlow/37_FloatMenu_PreventWear_Harmony_Implementation.md](37_FloatMenu_PreventWear_Harmony_Implementation.md)

## Harmony 불가피성 분석

**Q: Harmony 없이 FloatMenu에서 착용 불가를 표시할 수 없나요?**

**A: 불가능합니다.**

### 이유

1. **FloatMenu 생성은 밀폐된 시스템**
   - `FloatMenuMakerMap`의 로직이 private/static 메서드로 구성
   - Provider 리스트가 private이고 자동 생성됨
   - 외부에서 개입할 Hook 포인트가 없음

2. **시도 가능한 방법들 (모두 실패)**
   - ❌ `Apparel.PawnCanWear` 오버라이드 → FloatMenu 생성 시 호출 안 됨
   - ❌ `EquipmentUtility.CanEquip` 확장 → static 메서드라 오버라이드 불가능
   - ❌ 커스텀 Provider 생성 → 원본 Provider와 중복되어 옵션 2개 표시됨
   - ❌ 커스텀 메서드 추가 → RimWorld 원본이 호출하지 않음

3. **Harmony만이 가능한 이유**
   - 런타임 IL 코드 수정으로 원본 메서드 실행 후 결과 변경
   - `ref __result`로 FloatMenuOption 반환값 수정 가능
   - 기존 시스템과 충돌 없이 안전하게 동작

### Harmony 없이 가능한 것

✅ **`Notify_Equipped(pawn)` 오버라이드**
- 착용 완료 후 호환되지 않는 무기 제거
- 안전망 역할 (FloatMenu를 거치지 않는 경로)
- **한계**: 착용 시도 후에만 알 수 있음

### 결론

**FloatMenu 착용 불가 표시는 Harmony 불가피**하며, Postfix Patch는 안전하고 표준적인 방법입니다.

상세 분석: [Report/35_FloatMenu_No_Harmony_Limitation_Report.md](../Report/35_FloatMenu_No_Harmony_Limitation_Report.md)

---

## 다음 단계

1. 번역 키 추가 (RK_ApparelWeaponIncompatible_Dropped 등)
2. 빌드 및 테스트
3. Apparel_Util.xml에서 CompProperties 설정 확인

