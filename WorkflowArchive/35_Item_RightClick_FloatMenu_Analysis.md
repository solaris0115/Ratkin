# 아이템 우클릭 FloatMenu UI 로직 분석 - 2025-11-07

## 작업 개요
- 요청 내용: 장비 착용을 위해서 아이템 우클릭시 UI를 띄우는 로직 분석
- 목표: RimWorld의 FloatMenu 생성 및 장비 착용 제한 로직 파악

## 계획 (AI가 결정한 계획)
1. WorkFlow 파일 생성
2. FloatMenu 생성 로직 분석 (FloatMenuMakerMap)
3. Apparel 착용 관련 FloatMenuOption 생성 로직 분석
4. 장비 착용 제한 조건 확인 로직 분석
5. Report 파일로 결과 정리

## 최종 계획 (컨펌을 통해서 최종적으로 결정된 목표)
1. WorkFlow 파일 생성 ✓
2. FloatMenu 생성 로직 분석 (FloatMenuMakerMap)
3. Apparel 착용 관련 FloatMenuOption 생성 로직 분석
4. 장비 착용 제한 조건 확인 로직 분석
5. Report 파일로 결과 정리

## 작업 세부 진행
1. [WorkFlow 파일 생성] [✓]
2. [FloatMenuMakerMap 분석] [✓]
3. [Apparel 착용 FloatMenuOption 분석] [✓]
4. [착용 제한 로직 분석] [✓]
5. [Report 파일 작성] [✓]

## 진행 상황

### 1. WorkFlow 파일 생성
- 내용: 작업 추적을 위한 WorkFlow 파일 생성
- 결과: 완료

### 2. FloatMenuMakerMap 분석
- 내용: Provider 패턴 기반으로 FloatMenu 생성
- 결과: 완료

### 3. FloatMenuOptionProvider_Wear 분석
- 내용: Apparel 착용 메뉴 옵션 생성 로직 분석
- 결과: 완료

### 4. EquipmentUtility.CanEquip 분석
- 내용: 장비 착용 가능 여부 판단 로직 분석
- 결과: 완료

### 5. Report 파일 작성
- 내용: FloatMenu 생성 및 착용 제한 로직 분석 결과 Report 작성
- 결과: 완료

## 최종 작업 결과
✅ 완료

## 분석 결과 요약

### 1. FloatMenu 생성 구조
- **Provider 패턴 사용**: 각 상황별로 독립적인 Provider가 메뉴 옵션 생성
- **FloatMenuMakerMap**: 모든 Provider를 순회하며 옵션 수집
- **50여 개의 Provider**: Wear, Equip, PickUpItem, Ingest 등

### 2. Apparel 착용 검증 흐름
```
FloatMenuOptionProvider_Wear.GetSingleOptionFor()
├─ CanReach (경로 확인)
├─ IsBurning (불타는지 확인)
├─ WouldReplaceLockedApparel (잠긴 의복 대체)
├─ IsMutant && disableApparel (Mutant 제한)
├─ HasPartsToWear (착용 가능 신체 부위)
└─ EquipmentUtility.CanEquip ⭐ (최종 검증)
    ├─ Bladelink 연결 확인
    ├─ 생체코딩 확인
    ├─ 기존 Bladelink 무기 확인
    ├─ Role 제한 확인
    └─ DevelopmentalStage 확인
```

### 3. CompShieldWeaponIncompatible 통합 방법

**✅ 추천: FloatMenuOptionProvider_Wear.GetSingleOptionFor Postfix Patch**

```csharp
[HarmonyPatch(typeof(FloatMenuOptionProvider_Wear), "GetSingleOptionFor")]
public static class FloatMenuOptionProvider_Wear_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ref FloatMenuOption __result, Thing clickedThing, FloatMenuContext context)
    {
        // 추가 검증 로직
        CompShieldWeaponIncompatible comp = apparel.TryGetComp<CompShieldWeaponIncompatible>();
        if (comp != null && !comp.AllowEquipmentWith(pawn.equipment.Primary, out failReason))
        {
            // FloatMenuOption을 착용 불가로 대체
            __result = new FloatMenuOption("CannotWear: " + failReason, null, ...);
        }
    }
}
```

**장점**:
- 모든 기본 검증 완료 후 실행
- 다른 모드와 호환성 높음
- 착용 불가 사유 명확히 표시 가능

### 4. 핵심 발견사항

1. **EquipmentUtility.CanEquip**: 최종 착용 가능 여부 판단 (static 메서드)
2. **Apparel.PawnCanWear**: 기본 속성 검증 (성별, 발달 단계)
3. **FloatMenu 생성 시점**: Provider가 각 Thing에 대해 옵션을 독립적으로 생성
4. **확장 포인트**: Postfix Patch로 FloatMenuOption을 수정/대체 가능

## 관련 파일 목록

**RimWorld 원본 소스**:
- [RimworldSource/RimWorld/FloatMenuMakerMap.cs](../RimworldSource/RimWorld/FloatMenuMakerMap.cs)
- [RimworldSource/RimWorld/FloatMenuOptionProvider.cs](../RimworldSource/RimWorld/FloatMenuOptionProvider.cs)
- [RimworldSource/RimWorld/FloatMenuOptionProvider_Wear.cs](../RimworldSource/RimWorld/FloatMenuOptionProvider_Wear.cs)
- [RimworldSource/RimWorld/FloatMenuOptionProvider_Equip.cs](../RimworldSource/RimWorld/FloatMenuOptionProvider_Equip.cs)
- [RimworldSource/RimWorld/EquipmentUtility.cs](../RimworldSource/RimWorld/EquipmentUtility.cs)
- [RimworldSource/RimWorld/Apparel.cs](../RimworldSource/RimWorld/Apparel.cs)
- [RimworldSource/RimWorld/ApparelProperties.cs](../RimworldSource/RimWorld/ApparelProperties.cs)

**Ratkin 프로젝트**:
- [Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs](../Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs)

**작업 결과**:
- [WorkFlow/35_Item_RightClick_FloatMenu_Analysis.md](35_Item_RightClick_FloatMenu_Analysis.md)
- [Report/33_Item_RightClick_FloatMenu_Analysis_Report.md](../Report/33_Item_RightClick_FloatMenu_Analysis_Report.md)

