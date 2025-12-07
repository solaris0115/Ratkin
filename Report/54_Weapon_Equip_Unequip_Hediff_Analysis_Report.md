# 무기 장착/해제 시 Hediff 추가/제거 메커니즘 분석 보고서

**작성일**: 2025-01-XX  
**목적**: RimWorld에서 무기(Weapon) 장착/해제 시 호출되는 함수와 Hediff 추가/제거 구현 방법 조사

---

## 1. 무기 장착/해제 호출 흐름

### 1.1 장착 시 호출 흐름

```
사용자가 무기 장착
    ↓
Pawn_EquipmentTracker.AddEquipment(ThingWithComps newEq)
    ↓
equipment.TryAdd(newEq, true)  // ThingOwner에 추가
    ↓
Pawn_EquipmentTracker.Notify_EquipmentAdded(ThingWithComps eq)
    ↓
eq.GetComp<CompEquippable>().AllVerbs 설정
    ↓
eq.Notify_Equipped(this.pawn)  ⭐ 핵심 호출
    ↓
ThingWithComps.Notify_Equipped(Pawn pawn)
    ↓
base.Notify_Equipped(pawn)  // Thing.Notify_Equipped (virtual, 빈 구현)
    ↓
모든 Comp의 Notify_Equipped(pawn) 호출
    ↓
ThingComp.Notify_Equipped(Pawn pawn)  ⭐ 확장 가능한 지점
```

**코드 위치**: `RimworldSource/Verse/Pawn_EquipmentTracker.cs`

```342:358:RimworldSource/Verse/Pawn_EquipmentTracker.cs
public void Notify_EquipmentAdded(ThingWithComps eq)
{
	foreach (Verb verb in eq.GetComp<CompEquippable>().AllVerbs)
	{
		verb.caster = this.pawn;
		verb.Notify_PickedUp();
	}
	eq.Notify_Equipped(this.pawn);
	if (ModsConfig.RoyaltyActive && eq.def.equipmentType == EquipmentType.Primary && this.bondedWeapon != null && !this.bondedWeapon.Destroyed)
	{
		CompBladelinkWeapon compBladelinkWeapon = this.bondedWeapon.TryGetComp<CompBladelinkWeapon>();
		if (compBladelinkWeapon != null)
		{
			compBladelinkWeapon.Notify_WieldedOtherWeapon();
		}
	}
}
```

```883:893:RimworldSource/Verse/ThingWithComps.cs
public override void Notify_Equipped(Pawn pawn)
{
	base.Notify_Equipped(pawn);
	if (this.comps != null)
	{
		for (int i = 0; i < this.comps.Count; i++)
		{
			this.comps[i].Notify_Equipped(pawn);
		}
	}
}
```

### 1.2 해제 시 호출 흐름

```
무기 해제/드롭
    ↓
Pawn_EquipmentTracker.Notify_EquipmentRemoved(ThingWithComps eq)
    ↓
eq.Notify_Unequipped(this.pawn)  ⭐ 핵심 호출
    ↓
ThingWithComps.Notify_Unequipped(Pawn pawn)
    ↓
base.Notify_Unequipped(pawn)  // Thing.Notify_Unequipped (virtual, 빈 구현)
    ↓
모든 Comp의 Notify_Unequipped(pawn) 호출
    ↓
ThingComp.Notify_Unequipped(Pawn pawn)  ⭐ 확장 가능한 지점
```

**코드 위치**: `RimworldSource/Verse/Pawn_EquipmentTracker.cs`

```360:380:RimworldSource/Verse/Pawn_EquipmentTracker.cs
public void Notify_EquipmentRemoved(ThingWithComps eq)
{
	eq.Notify_Unequipped(this.pawn);
	if (ModsConfig.RoyaltyActive)
	{
		CompBladelinkWeapon compBladelinkWeapon = eq.TryGetComp<CompBladelinkWeapon>();
		if (compBladelinkWeapon != null)
		{
			compBladelinkWeapon.Notify_EquipmentLost(this.pawn);
		}
	}
	if (ModsConfig.OdysseyActive)
	{
		CompUniqueWeapon compUniqueWeapon = eq.TryGetComp<CompUniqueWeapon>();
		if (compUniqueWeapon == null)
		{
			return;
		}
		compUniqueWeapon.Notify_EquipmentLost(this.pawn);
	}
}
```

```895:905:RimworldSource/Verse/ThingWithComps.cs
public override void Notify_Unequipped(Pawn pawn)
{
	base.Notify_Unequipped(pawn);
	if (this.comps != null)
	{
		for (int i = 0; i < this.comps.Count; i++)
		{
			this.comps[i].Notify_Unequipped(pawn);
		}
	}
}
```

---

## 2. 확장 가능한 메서드

### 2.1 ThingComp 기반 확장 (권장)

**장점**:
- RimWorld 표준 패턴
- 여러 무기에 재사용 가능
- XML에서 CompProperties로 설정 가능

**기본 구조**:

```csharp
public class CompCauseHediff_Weapon : ThingComp
{
    private CompProperties_CauseHediff_Weapon Props
    {
        get => (CompProperties_CauseHediff_Weapon)this.props;
    }

    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        
        // Hediff가 이미 있는지 확인
        if (pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff, false) == null)
        {
            // Hediff 추가
            pawn.health.AddHediff(Props.hediff, null, null, null);
        }
    }

    public override void Notify_Unequipped(Pawn pawn)
    {
        base.Notify_Unequipped(pawn);
        
        // Hediff 제거
        Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff, false);
        if (hediff != null)
        {
            pawn.health.RemoveHediff(hediff);
        }
    }
}

public class CompProperties_CauseHediff_Weapon : CompProperties
{
    public HediffDef hediff;

    public CompProperties_CauseHediff_Weapon()
    {
        this.compClass = typeof(CompCauseHediff_Weapon);
    }
}
```

**참고 예제**: `RimworldSource/RimWorld/CompCauseHediff_Apparel.cs` (Apparel용이지만 동일한 패턴)

```16:26:RimworldSource/RimWorld/CompCauseHediff_Apparel.cs
public override void Notify_Equipped(Pawn pawn)
{
	if (pawn.health.hediffSet.GetFirstHediffOfDef(this.Props.hediff, false) == null)
	{
		HediffComp_RemoveIfApparelDropped hediffComp_RemoveIfApparelDropped = pawn.health.AddHediff(this.Props.hediff, pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).FirstOrFallback((BodyPartRecord p) => p.def == this.Props.part, null), null, null).TryGetComp<HediffComp_RemoveIfApparelDropped>();
		if (hediffComp_RemoveIfApparelDropped != null)
		{
			hediffComp_RemoveIfApparelDropped.wornApparel = (Apparel)this.parent;
		}
	}
}
```

### 2.2 ThingWithComps 상속 확장

**장점**:
- 특정 무기 타입에만 적용 가능
- 더 복잡한 로직 구현 가능

**기본 구조**:

```csharp
public class CustomWeapon : ThingWithComps
{
    public override void Notify_Equipped(Pawn pawn)
    {
        base.Notify_Equipped(pawn);
        
        // Hediff 추가
        if (pawn.health.hediffSet.GetFirstHediffOfDef(YourHediffDef, false) == null)
        {
            pawn.health.AddHediff(YourHediffDef, null, null, null);
        }
    }

    public override void Notify_Unequipped(Pawn pawn)
    {
        base.Notify_Unequipped(pawn);
        
        // Hediff 제거
        Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(YourHediffDef, false);
        if (hediff != null)
        {
            pawn.health.RemoveHediff(hediff);
        }
    }
}
```

**ThingDef 설정**:
```xml
<ThingDef Name="YourWeapon">
    <thingClass>YourNamespace.CustomWeapon</thingClass>
    <!-- 기타 설정 -->
</ThingDef>
```

---

## 3. Hediff 추가/제거 API

### 3.1 Hediff 추가

**메서드 시그니처**: `RimworldSource/Verse/Pawn_HealthTracker.cs`

```176:181:RimworldSource/Verse/Pawn_HealthTracker.cs
public Hediff AddHediff(HediffDef def, BodyPartRecord part = null, DamageInfo? dinfo = null, DamageWorker.DamageResult result = null)
{
	Hediff hediff = HediffMaker.MakeHediff(def, this.pawn, part);
	this.AddHediff(hediff, part, dinfo, result);
	return hediff;
}
```

**사용 예시**:
```csharp
// 기본 사용 (BodyPartRecord 없음)
pawn.health.AddHediff(HediffDefOf.YourHediff, null, null, null);

// 특정 부위에 추가
BodyPartRecord part = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault(p => p.def == BodyPartDefOf.Torso);
pawn.health.AddHediff(HediffDefOf.YourHediff, part, null, null);
```

### 3.2 Hediff 제거

**메서드 시그니처**: `RimworldSource/Verse/Pawn_HealthTracker.cs`

```236:237:RimworldSource/Verse/Pawn_HealthTracker.cs
public void RemoveHediff(Hediff hediff)
{
	hediff.PreRemoved();
	this.hediffSet.hediffs.Remove(hediff);
```

**사용 예시**:
```csharp
// Hediff 찾기
Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.YourHediff, false);
if (hediff != null)
{
    pawn.health.RemoveHediff(hediff);
}
```

### 3.3 Hediff 확인

```csharp
// 특정 Hediff가 있는지 확인
bool hasHediff = pawn.health.hediffSet.HasHediff(HediffDefOf.YourHediff, false);

// 첫 번째 Hediff 가져오기
Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.YourHediff, false);
```

---

## 4. 구현 예제

### 4.1 Comp 기반 구현 (권장)

**C# 코드**: `Project/1.6/Source/WeaponHediff/CompCauseHediff_Weapon.cs`

```csharp
using RimWorld;
using Verse;

namespace NewRatkin
{
    public class CompCauseHediff_Weapon : ThingComp
    {
        private CompProperties_CauseHediff_Weapon Props
        {
            get => (CompProperties_CauseHediff_Weapon)this.props;
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            
            if (pawn == null || pawn.health == null || Props?.hediff == null)
                return;

            // 이미 Hediff가 있는지 확인
            if (pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff, false) == null)
            {
                // Hediff 추가
                pawn.health.AddHediff(Props.hediff, null, null, null);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            
            if (pawn == null || pawn.health == null || Props?.hediff == null)
                return;

            // Hediff 제거
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff, false);
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }
    }

    public class CompProperties_CauseHediff_Weapon : CompProperties
    {
        public HediffDef hediff;

        public CompProperties_CauseHediff_Weapon()
        {
            this.compClass = typeof(CompCauseHediff_Weapon);
        }
    }
}
```

**XML 설정**: `Project/1.6/Defs/ThingsDefs/Weapon_Example.xml`

```xml
<ThingDef Name="RK_ExampleWeapon">
    <!-- 기타 설정 -->
    
    <comps>
        <li Class="NewRatkin.CompProperties_CauseHediff_Weapon">
            <hediff>RK_ExampleHediff</hediff>
        </li>
    </comps>
</ThingDef>
```

### 4.2 ThingWithComps 상속 구현

**C# 코드**: `Project/1.6/Source/WeaponHediff/CustomWeapon.cs`

```csharp
using RimWorld;
using Verse;

namespace NewRatkin
{
    public class CustomWeapon : ThingWithComps
    {
        // ThingDef에서 설정할 HediffDef 참조
        // 또는 하드코딩: public static HediffDef HediffToAdd = HediffDefOf.RK_ExampleHediff;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            
            if (pawn == null || pawn.health == null)
                return;

            // HediffDef 가져오기 (ThingDef의 compProperties에서 가져오거나 하드코딩)
            HediffDef hediffDef = HediffDefOf.RK_ExampleHediff; // 또는 this.def에서 가져오기
            
            if (pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef, false) == null)
            {
                pawn.health.AddHediff(hediffDef, null, null, null);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            
            if (pawn == null || pawn.health == null)
                return;

            HediffDef hediffDef = HediffDefOf.RK_ExampleHediff;
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef, false);
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }
    }
}
```

**XML 설정**:
```xml
<ThingDef Name="RK_ExampleWeapon">
    <thingClass>NewRatkin.CustomWeapon</thingClass>
    <!-- 기타 설정 -->
</ThingDef>
```

---

## 5. 주의사항

### 5.1 중복 Hediff 방지

장착 시 Hediff가 이미 있는지 확인하는 것이 중요합니다:

```csharp
if (pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff, false) == null)
{
    pawn.health.AddHediff(Props.hediff, null, null, null);
}
```

### 5.2 Null 체크

Pawn이 null이거나 health가 null일 수 있으므로 항상 체크:

```csharp
if (pawn == null || pawn.health == null || Props?.hediff == null)
    return;
```

### 5.3 저장/불러오기 호환성

Hediff가 저장/불러오기 시 자동으로 처리되므로 별도 작업 불필요. 다만, `HediffComp_RemoveIfApparelDropped`와 유사한 컴포넌트를 만들어 자동 제거를 보장할 수 있습니다.

### 5.4 CompEquippable의 Notify_Unequipped

`CompEquippable`에도 `Notify_Unequipped`가 있지만, 이것은 Verb 관련 처리용입니다. Hediff 처리는 `ThingComp`의 `Notify_Unequipped`를 사용해야 합니다.

```115:122:RimworldSource/Verse/CompEquippable.cs
public override void Notify_Unequipped(Pawn p)
{
	List<Verb> allVerbs = this.AllVerbs;
	for (int i = 0; i < allVerbs.Count; i++)
	{
		allVerbs[i].Notify_EquipmentLost();
	}
}
```

---

## 6. 결론

### 확장 가능 여부

**✅ 완전히 확장 가능합니다!**

1. **ThingComp 패턴 (권장)**: `Notify_Equipped`/`Notify_Unequipped`를 오버라이드하여 Hediff 추가/제거 가능
2. **ThingWithComps 상속**: 커스텀 무기 클래스에서 직접 구현 가능

### 구현 방법 선택 가이드

- **여러 무기에 재사용**: ThingComp 패턴 사용
- **특정 무기 전용 로직**: ThingWithComps 상속 사용
- **XML 설정 필요**: ThingComp 패턴 사용 (CompProperties 활용)

### 참고 파일

- `RimworldSource/Verse/Pawn_EquipmentTracker.cs` - 장착/해제 트래커
- `RimworldSource/Verse/ThingWithComps.cs` - Comp 호출 메커니즘
- `RimworldSource/Verse/ThingComp.cs` - Comp 기본 클래스
- `RimworldSource/RimWorld/CompCauseHediff_Apparel.cs` - Apparel용 참고 예제
- `RimworldSource/Verse/Pawn_HealthTracker.cs` - Hediff 추가/제거 API

---

## 7. 관련 파일 목록

- `RimworldSource/Verse/Pawn_EquipmentTracker.cs`
- `RimworldSource/Verse/ThingWithComps.cs`
- `RimworldSource/Verse/ThingComp.cs`
- `RimworldSource/Verse/CompEquippable.cs`
- `RimworldSource/RimWorld/CompCauseHediff_Apparel.cs`
- `RimworldSource/Verse/Pawn_HealthTracker.cs`

