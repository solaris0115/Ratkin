# 방패-무기 호환성 제한 기능 구현 제안서

**작성일**: 2025-01-XX  
**작성 목적**: 방패와 특정 무기 태그를 가진 무기의 동시 착용/사용을 제한하는 기능 구현 방안 제안  
**참고 파일**: 
- `Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs`
- `Project/1.6/Source/ShieldOfRatkinia/ApparelShield.cs`
- `Project/1.6/Defs/ThingsDefs/Apparel_Util.xml` (344-452줄)

---

## 1. 요구사항 분석

### 1.1 목표
방패(`RK_HeavyShield`)와 특정 무기 태그(`RK_WeaponTag_OneHand` 등)를 가진 무기만 허용하고, 그 외 무기와의 동시 착용/사용을 불가능하게 함.

### 1.2 현재 구현 상태
- ✅ `CompShieldWeaponIncompatible`: 방패 착용 시 호환되지 않는 무기 자동 제거 기능 구현됨
- ✅ `Shield.AllowVerbCast`: 현재 항상 `true` 반환 (모든 Verb 허용)
- ❌ 무기 착용 시 방패 제거 기능 미구현
- ❌ FloatMenu에서 착용 차단 기능 미구현
- ❌ Verb 차단 기능 미구현

### 1.3 제약사항
- **RimWorld 소스코드는 수정 불가능** (디컴파일된 참고용)
- RimWorld 소스코드 수정이 필요한 경우 **Harmony 패치 필수**
- 가능하면 Harmony 패치 없이 구현하는 것을 선호

---

## 2. 구현 방법 분석

### 방법 1: FloatMenu에서 착용 자체를 막기

#### 설명
방패를 착용한 상태에서 호환되지 않는 무기를 착용하려 할 때, FloatMenu에서 "착용 불가능" 옵션으로 표시

#### 구현 방식
1. **`EquipmentUtility.CanEquip` 패치** (Harmony 필요)
   - `EquipmentUtility.CanEquip`은 `static` 메서드이므로 Harmony 패치 필요
   - 방패 착용 상태에서 호환되지 않는 무기 착용 시도 시 `false` 반환

2. **`Apparel.PawnCanWear` 오버라이드** (확인 필요)
   - `Apparel` 클래스에 `PawnCanWear` 메서드가 있는지, virtual인지 확인 필요
   - virtual이면 오버라이드 가능 (Harmony 불필요)

#### 장점
- 착용 시도 자체를 막아 사용자 혼란 방지
- 명확한 UI 피드백 제공

#### 단점
- Harmony 패치 필요 (RimWorld 소스 수정 불가)
- `EquipmentUtility.CanEquip`은 static이므로 패치 필수

#### Harmony 패치 필요 여부
- ✅ **필요** (`EquipmentUtility.CanEquip` 패치)
- 또는 `Apparel.PawnCanWear` 오버라이드 가능 여부 확인 필요

---

### 방법 2: Verb만 막기 (ShieldBelt 방식) ⭐ **권장**

#### 설명
착용은 가능하지만, 방패에서 지정된 태그가 없는 무기의 Verb만 차단. ShieldBelt와 동일한 방식.

#### 구현 방식
`Shield.AllowVerbCast` 오버라이드 수정:
```csharp
public override bool AllowVerbCast(Verb verb)
{
    // 1. Verb의 소유자가 무기(CompEquippable)인지 확인
    // 2. 무기 태그에 허용 태그가 있는지 확인
    // 3. 무기 Verb만 차단, Apparel/Gene/Hediff Verb는 허용
}
```

#### 로직 흐름
1. `verb.verbOwner`가 `CompEquippable`인지 확인
2. `CompEquippable.parent`가 무기(`ThingWithComps`)인지 확인
3. 무기의 `weaponTags`에 허용 태그가 있는지 확인
4. 허용 태그가 없으면 `false` 반환 (Verb 차단)
5. 허용 태그가 있거나 무기가 아니면 `true` 반환 (Verb 허용)

#### 장점
- ✅ **Harmony 패치 불필요** (`Apparel.AllowVerbCast`는 virtual)
- ✅ ShieldBelt와 동일한 방식으로 일관성 유지
- ✅ 착용은 가능하지만 사용만 제한 (사용자 편의성)
- ✅ 무기 Verb만 차단, 다른 Verb(능력, 유전자 등)는 허용

#### 단점
- 착용은 되지만 사용 불가능한 상태가 발생 (UI 피드백 부족 가능)

#### Harmony 패치 필요 여부
- ❌ **불필요** (`Apparel.AllowVerbCast`는 virtual 메서드)

---

### 방법 3: 착용 시 호환 안되는 장비 벗기기

#### 설명
- 방패 착용 시: 호환되지 않는 무기 자동 제거 (✅ 이미 구현됨)
- 무기 착용 시: 호환되지 않는 방패 자동 제거 (❌ 미구현)

#### 구현 방식
1. **방패 착용 시** (이미 구현됨)
   - `CompShieldWeaponIncompatible.Notify_Equipped`에서 처리
   - 호환되지 않는 무기 자동 제거

2. **무기 착용 시** (구현 필요)
   - `ThingWithComps.Notify_Equipped` 패치 또는 `CompEquippable` 패치 필요
   - 무기 착용 시 착용 중인 방패 중 호환되지 않는 것 제거

#### Harmony 패치 대상
- `ThingWithComps.Notify_Equipped` (Postfix)
- 또는 `CompEquippable.Notify_Equipped` (Postfix)

#### 로직 흐름
1. 무기 착용 시 `Notify_Equipped` 호출
2. 착용한 Pawn의 `apparel.WornApparel` 순회
3. 각 Apparel에 `CompShieldWeaponIncompatible` 컴포넌트가 있는지 확인
4. 있으면 `TryIsWeaponAllowed`로 호환성 확인
5. 호환되지 않으면 방패 제거

#### 장점
- 자동으로 호환되지 않는 장비 제거 (사용자 편의성)
- 양방향 제거 지원 (방패↔무기)

#### 단점
- Harmony 패치 필요
- 무기 착용 시점 감지 필요

#### Harmony 패치 필요 여부
- ✅ **필요** (무기 착용 시 방패 제거)

---

## 3. 구현 우선순위 제안

### 우선순위 1: 방법 2 (Verb만 막기) ⭐ **즉시 구현 가능**

**이유**:
- Harmony 패치 불필요
- ShieldBelt와 동일한 방식으로 일관성 유지
- 가장 간단하고 안정적인 구현

**구현 내용**:
- `Shield.AllowVerbCast` 메서드 수정
- 무기 Verb만 차단하는 로직 추가

---

### 우선순위 2: 방법 3 완성 (무기 착용 시 방패 제거)

**이유**:
- 사용자 편의성 향상
- 양방향 자동 제거 완성

**구현 내용**:
- Harmony 패치로 `ThingWithComps.Notify_Equipped` 또는 `CompEquippable.Notify_Equipped` 후킹
- 무기 착용 시 호환되지 않는 방패 자동 제거

---

### 우선순위 3: 방법 1 (FloatMenu 차단) - 선택사항

**이유**:
- UI 피드백 향상
- 착용 시도 자체를 막아 혼란 방지

**구현 내용**:
- Harmony 패치로 `EquipmentUtility.CanEquip` 수정
- 또는 `Apparel.PawnCanWear` 오버라이드 가능 여부 확인 후 구현

---

## 4. 권장 구현 계획

### 단계 1: 방법 2 구현 (즉시 적용 가능)
- `Shield.AllowVerbCast` 수정
- 무기 Verb 차단 로직 구현
- 테스트 및 검증

### 단계 2: 방법 3 완성 (Harmony 패치)
- Harmony 패치 클래스 생성
- `ThingWithComps.Notify_Equipped` 또는 `CompEquippable.Notify_Equipped` Postfix 패치
- 무기 착용 시 방패 제거 로직 구현
- 테스트 및 검증

### 단계 3: 방법 1 구현 (선택사항)
- `EquipmentUtility.CanEquip` 패치 또는 `Apparel.PawnCanWear` 오버라이드
- FloatMenu 차단 로직 구현
- 테스트 및 검증

---

## 5. 기술적 세부사항

### 5.1 Verb 소유자 확인 방법

```csharp
// Verb의 소유자가 무기인지 확인
IVerbOwner verbOwner = verb.verbOwner;
if (verbOwner is CompEquippable compEquippable)
{
    ThingWithComps weapon = compEquippable.parent as ThingWithComps;
    if (weapon != null && weapon.def.IsWeapon)
    {
        // 무기 Verb임
        // weapon.def.weaponTags 확인
    }
}
```

### 5.2 무기 태그 확인 방법

```csharp
// CompShieldWeaponIncompatible의 TryIsWeaponAllowed 메서드 활용
CompShieldWeaponIncompatible comp = GetComp<CompShieldWeaponIncompatible>();
if (comp != null)
{
    string reason;
    bool allowed = comp.TryIsWeaponAllowed(weapon.def, out reason);
    if (!allowed)
    {
        // Verb 차단
        return false;
    }
}
```

### 5.3 Harmony 패치 예시 (방법 3)

```csharp
[HarmonyPatch(typeof(ThingWithComps), "Notify_Equipped")]
public static class ThingWithComps_Notify_Equipped_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ThingWithComps __instance, Pawn pawn)
    {
        // 무기인지 확인
        if (!__instance.def.IsWeapon || pawn?.apparel == null)
            return;
        
        // 착용 중인 방패 중 호환되지 않는 것 제거
        List<Apparel> wornApparel = pawn.apparel.WornApparel;
        for (int i = wornApparel.Count - 1; i >= 0; i--)
        {
            Apparel apparel = wornApparel[i];
            CompShieldWeaponIncompatible comp = apparel.GetComp<CompShieldWeaponIncompatible>();
            if (comp != null)
            {
                string reason;
                if (!comp.TryIsWeaponAllowed(__instance.def, out reason))
                {
                    // 방패 제거
                    pawn.apparel.Remove(apparel);
                    // 메시지 표시
                }
            }
        }
    }
}
```

---

## 6. 참고 자료

### 관련 보고서
- `Report/33_Item_RightClick_FloatMenu_Analysis_Report.md`: FloatMenu 생성 흐름 분석
- `Report/32_ShieldBelt_Weapon_Blocking_Mechanism_Analysis.md`: ShieldBelt Verb 차단 메커니즘 분석
- `Report/25_TryCastShot_Call_Flow_Analysis.md`: Verb 호출 흐름 분석

### 관련 코드
- `Project/1.6/Source/ShieldOfRatkinia/CompShieldWeaponIncompatible.cs`: 현재 구현된 컴포넌트
- `Project/1.6/Source/ShieldOfRatkinia/ApparelShield.cs`: Shield 클래스
- `Project/1.6/Defs/ThingsDefs/Apparel_Util.xml`: 방패 Def 설정

---

## 7. 결론 및 권장사항

### 최종 권장 구현 순서
1. **방법 2 (Verb만 막기)** - 즉시 구현 가능, Harmony 불필요
2. **방법 3 완성 (무기 착용 시 방패 제거)** - Harmony 패치 필요
3. **방법 1 (FloatMenu 차단)** - 선택사항, Harmony 패치 필요

### 구현 시 주의사항
- RimWorld 소스코드는 수정 불가능하므로 Harmony 패치 필수
- `Apparel.AllowVerbCast`는 virtual이므로 오버라이드 가능 (Harmony 불필요)
- `EquipmentUtility.CanEquip`은 static이므로 Harmony 패치 필요
- 무기 Verb와 다른 Verb(Apparel/Gene/Hediff)를 구분해야 함

### 예상 결과
- 방법 2 구현 시: 착용은 가능하지만 호환되지 않는 무기 Verb만 차단
- 방법 3 완성 시: 양방향 자동 제거 완성 (방패↔무기)
- 방법 1 구현 시: FloatMenu에서 착용 차단 (UI 피드백 향상)

