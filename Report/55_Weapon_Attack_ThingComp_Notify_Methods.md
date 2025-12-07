# 무기 공격 시 ThingComp Notify 메서드 분석 보고서

**작성일**: 2025-01-XX  
**목적**: 무기 공격 시 ThingComp에 알림되는 메서드 조사

---

## 무기 공격 시 ThingComp Notify 메서드

### 1. `Notify_UsedVerb(Pawn pawn, Verb verb)` ⭐

**호출 시점**: 공격 시도 성공 시 (`TryCastShot()` 성공 후)

**호출 흐름**:
```
Verb.TryCastNextBurstShot()
  → TryCastShot() 성공
    → CasterPawn.Notify_UsedVerb(this.CasterPawn, this)
      → ThingWithComps.Notify_UsedVerb()
        → 모든 Comp의 Notify_UsedVerb() 호출
```

**코드 위치**: `RimworldSource/Verse/Verb.cs:634`

```634:634:RimworldSource/Verse/Verb.cs
this.CasterPawn.Notify_UsedVerb(this.CasterPawn, this);
```

**ThingWithComps에서 Comp 호출**:
```907:916:RimworldSource/Verse/ThingWithComps.cs
public override void Notify_UsedVerb(Pawn pawn, Verb verb)
{
	base.Notify_UsedVerb(pawn, verb);
	if (this.comps != null)
	{
		for (int i = 0; i < this.comps.Count; i++)
		{
			this.comps[i].Notify_UsedVerb(pawn, verb);
		}
	}
}
```

**ThingComp 기본 구현**:
```201:203:RimworldSource/Verse/ThingComp.cs
public virtual void Notify_UsedVerb(Pawn pawn, Verb verb)
{
}
```

**용도**: 
- 공격 시도 시점에 처리 (데미지 적용 전)
- 공격 횟수 카운트, 효과 발동 등

---

### 2. `PostPreApplyDamage` / `PostPostApplyDamage` ❌

**주의**: 이 메서드들은 **무기가 공격할 때가 아니라, 무기 자체가 데미지를 받을 때** 호출됩니다.

```410:422:RimworldSource/Verse/ThingWithComps.cs
if (this.comps != null)
{
	for (int i = 0; i < this.comps.Count; i++)
	{
		this.comps[i].PostPreApplyDamage(ref dinfo, out absorbed);
		if (absorbed)
		{
			return;
		}
	}
}
```

**용도**: 무기 내구도 감소, 무기 파괴 방지 등

---

## 구현 예제

### Comp 기반 구현

```csharp
using RimWorld;
using Verse;

namespace NewRatkin
{
    public class CompWeaponAttackCounter : ThingComp
    {
        private int attackCount = 0;

        public override void Notify_UsedVerb(Pawn pawn, Verb verb)
        {
            base.Notify_UsedVerb(pawn, verb);
            
            // 공격 횟수 증가
            attackCount++;
            
            // 예: 10회 공격마다 특수 효과
            if (attackCount >= 10)
            {
                attackCount = 0;
                // 특수 효과 발동
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref attackCount, "attackCount", 0);
        }
    }

    public class CompProperties_WeaponAttackCounter : CompProperties
    {
        public CompProperties_WeaponAttackCounter()
        {
            this.compClass = typeof(CompWeaponAttackCounter);
        }
    }
}
```

**XML 설정**:
```xml
<ThingDef Name="RK_ExampleWeapon">
    <comps>
        <li Class="NewRatkin.CompProperties_WeaponAttackCounter" />
    </comps>
</ThingDef>
```

---

## 결론

**무기 공격 시 ThingComp에 notify되는 메서드**:
- ✅ **`Notify_UsedVerb`**: 공격 시도 성공 시 호출 (사용 가능)
- ❌ `PostPreApplyDamage` / `PostPostApplyDamage`: 무기가 데미지를 받을 때 호출 (공격 시 아님)

**확장 방법**: `ThingComp`를 상속받아 `Notify_UsedVerb`를 오버라이드하면 됩니다.

---

## 관련 파일 목록

- `RimworldSource/Verse/Verb.cs` - Notify_UsedVerb 호출
- `RimworldSource/Verse/ThingWithComps.cs` - Comp 호출 메커니즘
- `RimworldSource/Verse/ThingComp.cs` - Comp 기본 클래스

