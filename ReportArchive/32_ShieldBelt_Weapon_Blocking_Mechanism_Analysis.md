# ShieldBelt 무기 발사 방지 메커니즘 분석 보고서

## 개요

이 보고서는 RimWorld의 ShieldBelt가 착용 시 무기 발사를 어떻게 방지하는지 분석한 내용입니다. 이 메커니즘을 참고하여 방패와 특정 무기의 동시 착용을 방지하는 기능을 구현할 수 있습니다.

## 1. ShieldBelt 무기 발사 방지 메커니즘

### 1.1 핵심 메커니즘: `AllowVerbCast()` 체인

ShieldBelt는 **Apparel의 `AllowVerbCast()` 메서드**를 통해 무기 발사를 방지합니다.

#### 호출 흐름
```
Verb.TryCastShot() 또는 Verb.CanHitTarget()
  └─> Verb.FirstApparelPreventingShooting()
        └─> Apparel.AllowVerbCast(verb)
              └─> CompShield.CompAllowVerbCast(verb)
```

### 1.2 구현 세부사항

#### 1.2.1 CompShield.CompAllowVerbCast() 메서드

```368:371:RimworldSource/RimWorld/CompShield.cs
public override bool CompAllowVerbCast(Verb verb)
{
    return !this.Props.blocksRangedWeapons || !(verb is Verb_LaunchProjectile);
}
```

**동작 방식:**
- `CompProperties_Shield.blocksRangedWeapons`가 `true`이고
- `verb`가 `Verb_LaunchProjectile` 타입이면
- `false`를 반환하여 발사를 차단

#### 1.2.2 CompProperties_Shield 설정

```6:24:RimworldSource/RimWorld/CompProperties_Shield.cs
public class CompProperties_Shield : CompProperties
{
    public int startingTicksToReset = 3200;
    public float minDrawSize = 1.2f;
    public float maxDrawSize = 1.55f;
    public float energyLossPerDamage = 0.033f;
    public float energyOnReset = 0.2f;
    public bool blocksRangedWeapons = true;  // ← 핵심 설정

    public CompProperties_Shield()
    {
        this.compClass = typeof(CompShield);
    }
}
```

**설정:**
- `blocksRangedWeapons = true`: 원거리 무기 발사 차단 활성화
- XML에서 이 값을 설정하여 동작 제어 가능

#### 1.2.3 Apparel.AllowVerbCast() 메서드

```285:296:RimworldSource/RimWorld/Apparel.cs
public virtual bool AllowVerbCast(Verb verb)
{
    List<ThingComp> allComps = base.AllComps;
    for (int i = 0; i < allComps.Count; i++)
    {
        if (!allComps[i].CompAllowVerbCast(verb))
        {
            return false;
        }
    }
    return true;
}
```

**동작 방식:**
- 모든 Comp의 `CompAllowVerbCast()`를 순회
- 하나라도 `false`를 반환하면 전체적으로 `false` 반환
- **모든 Comp가 허용해야만 Verb 사용 가능**

#### 1.2.4 Verb.FirstApparelPreventingShooting() 메서드

```900:914:RimworldSource/Verse/Verb.cs
public Apparel FirstApparelPreventingShooting()
{
    if (this.CasterIsPawn && this.CasterPawn.apparel != null)
    {
        List<Apparel> wornApparel = this.CasterPawn.apparel.WornApparel;
        for (int i = 0; i < wornApparel.Count; i++)
        {
            if (!wornApparel[i].AllowVerbCast(this))
            {
                return wornApparel[i];
            }
        }
    }
    return null;
}
```

**동작 방식:**
- 착용한 모든 Apparel의 `AllowVerbCast()` 확인
- 차단하는 Apparel이 있으면 해당 Apparel 반환
- 없으면 `null` 반환

## 2. 프로젝트 내 Shield 클래스의 현재 구현

### 2.1 현재 구현 상태

```62:65:Project/1.6/Source/ShieldOfRatkinia/ApparelShield.cs
public override bool AllowVerbCast(Verb verb)
{
    return !(verb is Verb_LaunchProjectile);
}
```

**현재 동작:**
- `Shield` 클래스가 이미 `AllowVerbCast()`를 오버라이드하여 원거리 무기 발사를 차단
- ShieldBelt와 동일한 방식으로 구현됨

**차이점:**
- ShieldBelt: `CompShield.CompAllowVerbCast()`를 통해 Comp 기반으로 처리
- 현재 Shield: `Apparel.AllowVerbCast()`를 직접 오버라이드하여 처리

## 3. 착용 방지 메커니즘과의 차이점

### 3.1 무기 발사 방지 vs 착용 방지

| 구분 | 무기 발사 방지 | 착용 방지 |
|------|---------------|----------|
| **시점** | Verb 사용 시점 | 착용 시도 시점 |
| **메서드** | `AllowVerbCast()` | `PawnCanWear()` |
| **효과** | 발사는 막지만 착용은 가능 | 착용 자체를 막음 |
| **UI 표시** | 발사 시 경고 메시지 | 우클릭 메뉴에서 비활성화 |

### 3.2 착용 방지를 위한 메서드

#### Apparel.PawnCanWear() 메서드

```176:179:RimworldSource/RimWorld/Apparel.cs
public bool PawnCanWear(Pawn pawn, bool ignoreGender = false)
{
    return this.def.IsApparel && this.def.apparel.PawnCanWear(pawn, ignoreGender);
}
```

**특징:**
- `virtual` 메서드가 아니지만, `ApparelProperties.PawnCanWear()`를 호출
- `Apparel` 클래스를 상속받아 `PawnCanWear()`를 오버라이드 가능
- 착용 가능 여부를 반환하여 우클릭 메뉴에서 비활성화

#### FloatMenuOptionProvider_Wear에서의 사용

```68:76:RimworldSource/RimWorld/FloatMenuOptionProvider_Wear.cs
if (!ApparelUtility.HasPartsToWear(context.FirstSelectedPawn, apparel.def))
{
    return new FloatMenuOption(key.Translate(apparel.Label, apparel) + ": " + "CannotWearBecauseOfMissingBodyParts".Translate().CapitalizeFirst(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
}
string t;
if (!EquipmentUtility.CanEquip(apparel, context.FirstSelectedPawn, out t, true))
{
    return new FloatMenuOption(key.Translate(apparel.Label, apparel) + ": " + t, null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
}
```

**동작:**
- `EquipmentUtility.CanEquip()`가 `false`를 반환하면
- 우클릭 메뉴에서 비활성화된 옵션으로 표시

## 4. 방패와 무기 동시 착용 방지 구현 방안

### 4.1 방법 1: Apparel.PawnCanWear() 오버라이드 (권장)

**장점:**
- Harmony patch 불필요
- 우클릭 메뉴에서 자동으로 비활성화
- ShieldBelt와 유사한 패턴 사용 가능

**구현 예시:**
```csharp
public class Shield : Apparel
{
    public override bool PawnCanWear(Pawn pawn, bool ignoreGender = false)
    {
        // 기본 착용 가능 여부 확인
        if (!base.PawnCanWear(pawn, ignoreGender))
        {
            return false;
        }
        
        // 현재 착용 중인 무기 확인
        if (pawn.equipment?.Primary != null)
        {
            ThingDef weaponDef = pawn.equipment.Primary.def;
            
            // 특정 무기와 충돌하는지 확인
            if (IsIncompatibleWeapon(weaponDef))
            {
                return false;
            }
        }
        
        return true;
    }
    
    private bool IsIncompatibleWeapon(ThingDef weaponDef)
    {
        // weaponTag 기반 확인
        if (weaponDef.weaponTags != null && weaponDef.weaponTags.Contains("RK_TwoHanded"))
        {
            return true;
        }
        
        // 또는 defName 기반 확인
        // return incompatibleWeaponDefNames.Contains(weaponDef.defName);
        
        return false;
    }
}
```

### 4.2 방법 2: Comp 기반 구현 (ShieldBelt 방식)

**장점:**
- CompProperties로 XML에서 설정 가능
- 더 유연한 확장성

**구현 예시:**
```csharp
public class CompProperties_ShieldIncompatible : CompProperties
{
    public List<string> incompatibleWeaponTags;
    public List<ThingDef> incompatibleWeaponDefs;
    
    public CompProperties_ShieldIncompatible()
    {
        this.compClass = typeof(CompShieldIncompatible);
    }
}

public class CompShieldIncompatible : ThingComp
{
    public CompProperties_ShieldIncompatible Props => (CompProperties_ShieldIncompatible)this.props;
    
    public bool IsIncompatibleWeapon(ThingDef weaponDef)
    {
        if (Props.incompatibleWeaponTags != null)
        {
            if (weaponDef.weaponTags != null && 
                weaponDef.weaponTags.Any(tag => Props.incompatibleWeaponTags.Contains(tag)))
            {
                return true;
            }
        }
        
        if (Props.incompatibleWeaponDefs != null && 
            Props.incompatibleWeaponDefs.Contains(weaponDef))
        {
            return true;
        }
        
        return false;
    }
}
```

그리고 `Shield` 클래스에서:
```csharp
public override bool PawnCanWear(Pawn pawn, bool ignoreGender = false)
{
    if (!base.PawnCanWear(pawn, ignoreGender))
    {
        return false;
    }
    
    if (pawn.equipment?.Primary != null)
    {
        CompShieldIncompatible comp = this.GetComp<CompShieldIncompatible>();
        if (comp != null && comp.IsIncompatibleWeapon(pawn.equipment.Primary.def))
        {
            return false;
        }
    }
    
    return true;
}
```

## 5. 무기 착용 시 방패 확인 (양방향 검사)

### 5.1 EquipmentUtility.CanEquip() 확장

`EquipmentUtility.CanEquip()`는 static 메서드이므로 직접 오버라이드 불가능합니다. 대신 Comp 기반으로 확장할 수 있습니다.

**구현 예시:**
```csharp
public class CompProperties_WeaponShieldIncompatible : CompProperties
{
    public List<string> incompatibleShieldTags;
    
    public CompProperties_WeaponShieldIncompatible()
    {
        this.compClass = typeof(CompWeaponShieldIncompatible);
    }
}

public class CompWeaponShieldIncompatible : ThingComp
{
    public CompProperties_WeaponShieldIncompatible Props => 
        (CompProperties_WeaponShieldIncompatible)this.props;
    
    public override void PostPostMake()
    {
        base.PostPostMake();
        // 무기 착용 시 방패 확인 로직
    }
}
```

하지만 이 방법은 착용 시점 검사가 어렵습니다. 대신 **방패 착용 시 무기 확인**만으로도 충분할 수 있습니다.

## 6. 결론 및 권장사항

### 6.1 ShieldBelt의 핵심 메커니즘

1. **`CompShield.CompAllowVerbCast()`**: Verb 사용 시점에 차단
2. **`Apparel.AllowVerbCast()`**: 모든 Comp의 허용 여부 확인
3. **`Verb.FirstApparelPreventingShooting()`**: 착용한 Apparel 중 차단하는 것 확인

### 6.2 방패와 무기 동시 착용 방지 구현 권장사항

**방법 1: `Apparel.PawnCanWear()` 오버라이드 (단순하고 효과적)**
- ✅ Harmony patch 불필요
- ✅ 우클릭 메뉴에서 자동 비활성화
- ✅ 구현이 간단

**방법 2: Comp 기반 (확장성 좋음)**
- ✅ XML에서 설정 가능
- ✅ 여러 방패/무기 조합에 유연하게 대응
- ⚠️ 약간 더 복잡한 구현 필요

### 6.3 양방향 검사 필요성

**방패 착용 시 무기 확인만으로도 충분:**
- 방패 착용 시: `PawnCanWear()`에서 무기 확인 → 우클릭 메뉴 비활성화
- 무기 착용 시: 이미 착용한 방패가 있으면 `AllowVerbCast()`로 발사 차단 (현재 구현됨)

**추가 검사가 필요한 경우:**
- 무기 착용 시 방패를 자동으로 벗기고 싶은 경우
- 무기 착용 시점에 명확한 경고 메시지를 표시하고 싶은 경우

## 관련 파일 목록

- `RimworldSource/RimWorld/CompShield.cs` - CompShield 구현
- `RimworldSource/RimWorld/CompProperties_Shield.cs` - CompProperties 설정
- `RimworldSource/RimWorld/Apparel.cs` - Apparel.AllowVerbCast() 메서드
- `RimworldSource/Verse/Verb.cs` - Verb.FirstApparelPreventingShooting() 메서드
- `RimworldSource/RimWorld/FloatMenuOptionProvider_Wear.cs` - 의류 착용 메뉴 생성
- `Project/1.6/Source/ShieldOfRatkinia/ApparelShield.cs` - 현재 Shield 클래스 구현

## 참고사항

- ShieldBelt는 **발사 시점**에 차단하지만, **착용은 가능**합니다
- 착용을 완전히 막으려면 **`PawnCanWear()` 오버라이드**가 필요합니다
- 현재 Shield 클래스는 이미 `AllowVerbCast()`를 오버라이드하여 원거리 무기 발사를 차단하고 있습니다
- 동시 착용 방지를 위해서는 `PawnCanWear()` 오버라이드가 추가로 필요합니다

