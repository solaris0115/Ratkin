# Apparel-Weapon Tag 기반 동시 착용 제한 구현 - 2025-11-07

## 작업 개요
- 요청 내용: Primary weapon과 특정 apparel의 동시 착용 제한 (Apparel의 Comp에서 허용 weaponTag 리스트 관리)
- 목표: Harmony 패치 없이 구현 (가능하면), 불가능하면 Harmony 사용

## 계획 (AI가 결정한 계획)
1. WorkFlow 파일 생성
2. 기존 CompShieldWeaponIncompatible 구현 분석
3. RimWorld 원본의 Apparel/Weapon 착용 Hook 포인트 찾기
4. Harmony 없는 구현 방법 탐색
5. 최종 구현 방법 제시

## 작업 세부 진행
1. [WorkFlow 파일 생성] [✓]
2. [기존 Comp 분석] [✓]
3. [Hook 포인트 탐색] [✓]
4. [구현 방법 결정] [✓]
5. [Report 파일 작성] [✓]

## 진행 상황

### 1. WorkFlow 파일 생성
- 내용: 작업 추적 파일 생성
- 결과: 완료

### 2. 기존 Comp 분석
- 내용: CompShieldWeaponIncompatible 구현 확인
- 결과: allowedWeaponTags 리스트와 TryIsWeaponAllowed() 메서드 이미 구현됨

### 3. Hook 포인트 분석
- 내용: Apparel/Weapon 착용 시 호출되는 Notify 메서드 확인
- 결과: ThingComp.Notify_Equipped(pawn) 발견 - Harmony 없이 구현 가능!

### 4. 구현 방법 결정
- 내용: Harmony 없는 방법과 Harmony 사용 방법 비교 분석
- 결과: ThingComp.Notify_Equipped(pawn) 오버라이드로 Harmony 없이 구현 가능!

### 5. Report 파일 작성
- 내용: Hook 분석 및 구현 방법 정리
- 결과: 완료

## 최종 작업 결과
✅ 완료 - **Harmony 없이 구현 가능함을 확인!**

## 핵심 발견사항

### 1. Harmony 없이 구현 가능! ✅

**ThingComp.Notify_Equipped(pawn)** 메서드를 오버라이드하면 됩니다:

```csharp
public class CompShieldWeaponIncompatible : ThingComp
{
    // ⭐ 이 메서드만 추가하면 Harmony 불필요!
    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        
        if (pawn?.equipment?.Primary == null) return;
        
        string reason;
        if (!TryIsWeaponAllowed(pawn.equipment.Primary.def, out reason))
        {
            // 호환되지 않는 무기를 바닥에 떨어뜨림
            DropIncompatibleWeapon(pawn, pawn.equipment.Primary, reason);
        }
    }
    
    private void DropIncompatibleWeapon(Pawn pawn, ThingWithComps weapon, string reason)
    {
        ThingWithComps droppedWeapon;
        if (pawn.equipment.TryDropEquipment(weapon, out droppedWeapon, pawn.Position, false))
        {
            // 메시지 표시
            Messages.Message(
                "RK_ApparelWeaponIncompatible".Translate(
                    parent.Label, weapon.Label, reason
                ),
                pawn, MessageTypeDefOf.CautionInput, false
            );
        }
    }
}
```

### 2. 착용 시스템 Hook 포인트

**Apparel 착용 흐름**:
```
Pawn_ApparelTracker.Wear()
    → Notify_ApparelAdded()
        → Apparel.Notify_Equipped(pawn)
            → ThingWithComps.Notify_Equipped(pawn)
                → CompShieldWeaponIncompatible.Notify_Equipped(pawn) ⭐
```

**Weapon 착용 흐름**:
```
Pawn_EquipmentTracker.AddEquipment()
    → Notify_EquipmentAdded()
        → ThingWithComps.Notify_Equipped(pawn)
            → (Weapon의 Comp들).Notify_Equipped(pawn)
```

### 3. 구현 방법 비교

| 방법 | Harmony 필요 | UX | 구현 복잡도 | 호환성 |
|------|--------------|-----|-------------|--------|
| **ThingComp.Notify_Equipped 오버라이드** | ❌ 불필요 | 착용 후 무기 제거 | 낮음 | ⭐⭐⭐⭐⭐ |
| **FloatMenu Postfix Patch** | ✅ 필요 | 착용 전 경고 | 중간 | ⭐⭐⭐ |
| **EquipmentTracker Prefix Patch** | ✅ 필요 | 무기 장착 시 방패 제거 | 높음 | ⭐⭐ |

### 4. 추천 구현 순서

**1단계 (필수, Harmony 불필요)**:
- `CompShieldWeaponIncompatible.Notify_Equipped(pawn)` 오버라이드
- Apparel 착용 시 호환되지 않는 Weapon 자동 제거
- 사용자에게 메시지 표시

**2단계 (선택, Harmony 필요)**:
- `FloatMenuOptionProvider_Wear.GetSingleOptionFor` Postfix Patch
- FloatMenu에서 미리 경고 표시 ("착용 불가: 양손 무기와 함께 착용 불가")
- UX 개선용

### 5. 동작 예시

```
[시나리오 1: Harmony 없는 구현]
1. Ratkin이 양손 무기(Bolter) 착용 중
2. 사용자가 방패(Shield) 착용 시도
3. 방패가 착용됨
4. CompShieldWeaponIncompatible.Notify_Equipped(pawn) 호출
5. TryIsWeaponAllowed(Bolter.def) → false
6. Bolter를 바닥에 떨어뜨림
7. 메시지 표시: "방패를 착용하여 Bolter를 내려놓았습니다"

[시나리오 2: Harmony 추가 시]
1. Ratkin이 양손 무기(Bolter) 착용 중
2. 사용자가 방패(Shield) 우클릭
3. FloatMenu 표시: ❌ "착용 불가: 양손 무기와 함께 착용 불가"
4. 사용자는 착용 시도를 하지 않음
```

### 6. 기존 구현 확인사항

현재 `CompShieldWeaponIncompatible.cs`에는:
- ✅ `allowedWeaponTags` 리스트 (CompProperties)
- ✅ `TryIsWeaponAllowed(ThingDef weaponDef, out string reason)` 메서드
- ❌ `Notify_Equipped(pawn)` 오버라이드 **없음** → 추가 필요!

## 관련 파일 목록

**RimWorld 원본 소스**:
- [RimworldSource/RimWorld/Pawn_ApparelTracker.cs](../RimworldSource/RimWorld/Pawn_ApparelTracker.cs)
- [RimworldSource/Verse/Pawn_EquipmentTracker.cs](../RimworldSource/Verse/Pawn_EquipmentTracker.cs)
- [RimworldSource/RimWorld/Apparel.cs](../RimworldSource/RimWorld/Apparel.cs)
- [RimworldSource/Verse/ThingWithComps.cs](../RimworldSource/Verse/ThingWithComps.cs)
- [RimworldSource/Verse/ThingComp.cs](../RimworldSource/Verse/ThingComp.cs)

**Ratkin 프로젝트**:
- [Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs](../Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs)
- [Project/1.6/Source/ShieldOfRatkinia/ApparelShield.cs](../Project/1.6/Source/ShieldOfRatkinia/ApparelShield.cs)

**작업 결과**:
- [WorkFlow/36_Apparel_Weapon_Tag_Restriction_Implementation.md](36_Apparel_Weapon_Tag_Restriction_Implementation.md)
- [Report/34_Apparel_Weapon_Restriction_Hook_Analysis_Report.md](../Report/34_Apparel_Weapon_Restriction_Hook_Analysis_Report.md)

## 다음 단계

1. `CompShieldWeaponIncompatible.cs`에 `Notify_Equipped(pawn)` 메서드 추가
2. `DropIncompatibleWeapon()` 헬퍼 메서드 구현
3. 번역 키 추가 (`RK_ApparelWeaponIncompatible`)
4. 테스트
5. (선택) FloatMenu Harmony Patch 추가

