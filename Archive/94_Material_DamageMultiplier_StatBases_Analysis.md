# 소재 배율(SharpDamageMultiplier/BluntDamageMultiplier) statBases 지정 가능 여부 분석

## 질문
**소재에 따른 배율은 statBases에 추가로 지정이 가능한가? 아니면 품질 배율만 적용 가능한가? (피해량 관련, 관통 말고)**

## 답변

**소재 배율(SharpDamageMultiplier, BluntDamageMultiplier)은 소재 ThingDef의 statBases에만 정의 가능합니다. 무기 ThingDef의 statBases에는 추가로 지정할 수 없습니다.**

---

## 코드 분석

### 소스코드 위치
`RimworldSource/Verse/Tool.cs` - `AdjustedBaseMeleeDamageAmount` 메서드

### 핵심 코드

```csharp
public float AdjustedBaseMeleeDamageAmount(Thing ownerEquipment, DamageDef damageDef)
{
    float num = this.power;
    if (ownerEquipment != null)
    {
        // 1. 무기 품질 배율 (무기 ThingDef의 statBases에서 가져옴)
        num *= ownerEquipment.GetStatValue(StatDefOf.MeleeWeapon_DamageMultiplier, true, -1);
        
        if (ownerEquipment.Stuff != null && damageDef != null)
        {
            // 2. 소재 배율 (소재 ThingDef의 statBases에서 가져옴)
            num *= ownerEquipment.Stuff.GetStatValueAbstract(damageDef.armorCategory.multStat, null);
        }
    }
    return num;
}
```

### 핵심 포인트

1. **품질 배율**: `ownerEquipment.GetStatValue()` → 무기 ThingDef의 statBases에서 가져옴
2. **소재 배율**: `ownerEquipment.Stuff.GetStatValueAbstract()` → 소재 ThingDef의 statBases에서 가져옴

---

## GetStatValueAbstract 동작 방식

### 메서드 시그니처
```csharp
public static float GetStatValueAbstract(this BuildableDef def, StatDef stat, ThingDef stuff = null)
{
    return stat.Worker.GetValueAbstract(def, stuff);
}
```

### 동작 원리
- `GetStatValueAbstract`는 **호출된 ThingDef의 statBases**에서 값을 가져옵니다
- `ownerEquipment.Stuff.GetStatValueAbstract()`는 **Stuff ThingDef의 statBases**를 참조합니다
- 무기 ThingDef의 statBases는 참조하지 않습니다

---

## 적용 가능한 배율

### 1. 품질 배율 (MeleeWeapon_DamageMultiplier)
**위치**: 무기 ThingDef의 statBases
**적용 방식**: 무기 품질(Quality)에 따라 자동 적용
**예시**:
```xml
<!-- 무기 ThingDef -->
<statBases>
    <!-- MeleeWeapon_DamageMultiplier는 품질에 따라 자동 적용됨 -->
    <!-- statBases에 명시할 필요 없음 -->
</statBases>
```

**품질별 배율** (StatDef에서 정의):
- Awful: 0.8
- Poor: 0.9
- Normal: 1.0
- Good: 1.1
- Excellent: 1.2
- Masterwork: 1.45
- Legendary: 1.65

### 2. 소재 배율 (SharpDamageMultiplier, BluntDamageMultiplier)
**위치**: 소재 ThingDef의 statBases
**적용 방식**: 소재 ThingDef의 statBases에 정의된 값 사용
**예시**:
```xml
<!-- 소재 ThingDef (Steel) -->
<statBases>
    <SharpDamageMultiplier>1</SharpDamageMultiplier>
    <BluntDamageMultiplier>1</BluntDamageMultiplier>
</statBases>
```

```xml
<!-- 소재 ThingDef (우라늄) -->
<statBases>
    <SharpDamageMultiplier>1.1</SharpDamageMultiplier>
    <BluntDamageMultiplier>1.5</BluntDamageMultiplier>
</statBases>
```

---

## 무기 ThingDef에서 소재 배율 지정 시도

### 시도 1: 무기 ThingDef의 statBases에 추가
```xml
<!-- 무기 ThingDef -->
<statBases>
    <SharpDamageMultiplier>1.2</SharpDamageMultiplier>  <!-- ❌ 작동 안 함 -->
    <BluntDamageMultiplier>1.3</BluntDamageMultiplier>  <!-- ❌ 작동 안 함 -->
</statBases>
```

**결과**: 무시됨. 코드에서 `ownerEquipment.Stuff.GetStatValueAbstract()`를 호출하므로 무기 ThingDef의 statBases는 참조하지 않습니다.

### 시도 2: 소재 ThingDef의 statBases에 추가
```xml
<!-- 소재 ThingDef -->
<statBases>
    <SharpDamageMultiplier>1.2</SharpDamageMultiplier>  <!-- ✅ 작동함 -->
    <BluntDamageMultiplier>1.3</BluntDamageMultiplier>  <!-- ✅ 작동함 -->
</statBases>
```

**결과**: 정상 작동. 소재 ThingDef의 statBases에서 값을 가져옵니다.

---

## 실제 적용 예시

### 예시 1: Steel 소재 롱소드
**소재 ThingDef (Steel)**:
```xml
<statBases>
    <SharpDamageMultiplier>1</SharpDamageMultiplier>
    <BluntDamageMultiplier>1</BluntDamageMultiplier>
</statBases>
```

**무기 ThingDef (롱소드)**:
```xml
<statBases>
    <!-- SharpDamageMultiplier, BluntDamageMultiplier 지정 불가 -->
    <!-- 소재 ThingDef에서만 정의 가능 -->
</statBases>
```

**계산 결과**:
- Stab 공격: SharpDamageMultiplier 1.0 적용
- Cut 공격: SharpDamageMultiplier 1.0 적용
- Blunt 공격: BluntDamageMultiplier 1.0 적용

### 예시 2: 우라늄 소재 롱소드
**소재 ThingDef (우라늄)**:
```xml
<statBases>
    <SharpDamageMultiplier>1.1</SharpDamageMultiplier>
    <BluntDamageMultiplier>1.5</BluntDamageMultiplier>
</statBases>
```

**무기 ThingDef (롱소드)**:
```xml
<statBases>
    <!-- 무기 ThingDef에서 소재 배율 지정 불가 -->
</statBases>
```

**계산 결과**:
- Stab 공격: SharpDamageMultiplier 1.1 적용 (소재 ThingDef에서 가져옴)
- Cut 공격: SharpDamageMultiplier 1.1 적용 (소재 ThingDef에서 가져옴)
- Blunt 공격: BluntDamageMultiplier 1.5 적용 (소재 ThingDef에서 가져옴)

---

## 결론

### 소재 배율 지정 가능 여부

| 배율 타입 | 무기 ThingDef statBases | 소재 ThingDef statBases | 비고 |
|-----------|------------------------|------------------------|------|
| **MeleeWeapon_DamageMultiplier** (품질 배율) | ❌ 불가 (품질에 따라 자동 적용) | ❌ 불가 | 품질에 따라 자동 적용됨 |
| **SharpDamageMultiplier** (소재 배율) | ❌ 불가 | ✅ 가능 | 소재 ThingDef에서만 정의 가능 |
| **BluntDamageMultiplier** (소재 배율) | ❌ 불가 | ✅ 가능 | 소재 ThingDef에서만 정의 가능 |

### 핵심 요약
1. **품질 배율**: 무기 품질에 따라 자동 적용 (statBases에 명시 불필요)
2. **소재 배율**: 소재 ThingDef의 statBases에만 정의 가능
3. **무기 ThingDef**: 소재 배율을 statBases에 추가로 지정할 수 없음

### 설계 의도
- 소재 배율은 **소재의 고유 속성**이므로 소재 ThingDef에서만 정의
- 무기 ThingDef는 소재 배율을 오버라이드할 수 없음
- 각 소재마다 고유한 피해량 배율을 가지도록 설계됨

---

## 관련 파일
- `RimworldSource/Verse/Tool.cs` (피해량 계산 로직)
- `RimworldSource/RimWorld/StatExtension.cs` (GetStatValueAbstract 구현)
- `RimWorldData/Core/Defs/ThingDefs_Items/Items_Resource_Stuff.xml` (소재 정의)
