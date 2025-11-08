# 무기 드로잉 및 각도 분석 리포트

## 개요
RimWorld 소스코드에서 무기 드로잉과 각도 관련 메커니즘을 분석하고, 커스터마이징 가능 여부를 확인했습니다.

## 무기 드로잉 메커니즘

### 1. 손에 들고 있을 때 (Carried Weapon)

**코드 위치:** `RimworldSource/Verse/PawnRenderUtility.cs:49-69`

```49:69:RimworldSource/Verse/PawnRenderUtility.cs
public static void DrawCarriedWeapon(ThingWithComps weapon, Vector3 drawPos, Rot4 facing, float equipmentDrawDistanceFactor)
{
	int num = 143;
	switch (facing.AsInt)
	{
	case 0:
		drawPos += PawnRenderUtility.EqLocNorth * equipmentDrawDistanceFactor;
		break;
	case 1:
		drawPos += PawnRenderUtility.EqLocEast * equipmentDrawDistanceFactor;
		break;
	case 2:
		drawPos += PawnRenderUtility.EqLocSouth * equipmentDrawDistanceFactor;
		break;
	case 3:
		drawPos += PawnRenderUtility.EqLocWest * equipmentDrawDistanceFactor;
		num = 217;
		break;
	}
	PawnRenderUtility.DrawEquipmentAiming(weapon, drawPos, (float)num);
}
```

**특징:**
- Pawn의 facing 방향에 따라 고정된 각도 사용
  - North, East, South: **143도**
  - West: **217도**
- 각도는 하드코딩되어 있어 직접 커스터마이징 불가
- `DrawEquipmentAiming` 메서드에 각도 전달

### 2. 발사 시 조준 각도 (Aiming Weapon)

**코드 위치:** `RimworldSource/Verse/PawnRenderUtility.cs:230-273`

```254:268:RimworldSource/Verse/PawnRenderUtility.cs
if (!flags.HasFlag(PawnRenderFlags.NeverAimWeapon) && stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
{
	Vector3 a = stance_Busy.focusTarg.HasThing ? stance_Busy.focusTarg.Thing.DrawPos : stance_Busy.focusTarg.Cell.ToVector3Shifted();
	if ((a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
	{
		num = (a - pawn.DrawPos).AngleFlat();
	}
	Verb currentEffectiveVerb = pawn.CurrentEffectiveVerb;
	if (currentEffectiveVerb != null && currentEffectiveVerb.AimAngleOverride != null)
	{
		num = currentEffectiveVerb.AimAngleOverride.Value;
	}
	drawPos += new Vector3(0f, 0f, 0.4f + pawn.equipment.Primary.def.equippedDistanceOffset).RotatedBy(num) * equipmentDrawDistanceFactor;
	PawnRenderUtility.DrawEquipmentAiming(pawn.equipment.Primary, drawPos, num);
}
```

**조준 각도 계산 순서:**
1. 타겟(`stance_Busy.focusTarg`)을 향한 각도 계산 (기본)
2. `Verb.AimAngleOverride` 속성이 있으면 해당 값으로 오버라이드
3. 계산된 각도가 `DrawEquipmentAiming`에 전달됨

### 3. 무기 각도 적용 (DrawEquipmentAiming)

**코드 위치:** `RimworldSource/Verse/PawnRenderUtility.cs:71-114`

```71:114:RimworldSource/Verse/PawnRenderUtility.cs
public static void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)
{
	float num = aimAngle - 90f;
	Mesh mesh;
	if (aimAngle > 20f && aimAngle < 160f)
	{
		mesh = MeshPool.plane10;
		num += eq.def.equippedAngleOffset;
	}
	else if (aimAngle > 200f && aimAngle < 340f)
	{
		mesh = MeshPool.plane10Flip;
		num -= 180f;
		num -= eq.def.equippedAngleOffset;
	}
	else
	{
		mesh = MeshPool.plane10;
		num += eq.def.equippedAngleOffset;
	}
	num %= 360f;
	CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
	if (compEquippable != null)
	{
		Vector3 b;
		float num2;
		EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out b, out num2, aimAngle);
		drawLoc += b;
		num += num2;
	}
	Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
	Material material;
	if (graphic_StackCount != null)
	{
		material = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq);
	}
	else
	{
		material = eq.Graphic.MatSingleFor(eq);
	}
	Vector3 s = new Vector3(eq.Graphic.drawSize.x, 0f, eq.Graphic.drawSize.y);
	Matrix4x4 matrix = Matrix4x4.TRS(drawLoc, Quaternion.AngleAxis(num, Vector3.up), s);
	Graphics.DrawMesh(mesh, matrix, material, 0);
}
```

**각도 처리 과정:**
1. `aimAngle - 90f`로 기본 각도 계산
2. 각도 범위에 따라 메시 선택 (뒤집기 여부 결정)
3. **`eq.def.equippedAngleOffset` 속성으로 각도 보정**
4. 리코일 각도 추가 (발사 시)
5. 최종 각도로 메시 회전

## 커스터마이징 가능한 속성

### 1. ThingDef 속성 (XML에서 설정 가능)

**위치:** `RimworldSource/Verse/ThingDef.cs:341-343`

```341:343:RimworldSource/Verse/ThingDef.cs
public float equippedAngleOffset;

public float equippedDistanceOffset;
```

**사용법:**
```xml
<ThingDef Name="MyWeapon">
  <!-- 각도 오프셋 (도 단위) -->
  <equippedAngleOffset>15.0</equippedAngleOffset>
  
  <!-- 거리 오프셋 (월드 단위) -->
  <equippedDistanceOffset>0.1</equippedDistanceOffset>
</ThingDef>
```

**효과:**
- `equippedAngleOffset`: 무기 장착 시 각도에 추가되는 오프셋 (도 단위)
- `equippedDistanceOffset`: 무기 장착 시 거리에 추가되는 오프셋 (월드 단위)

### 2. Verb 속성 (코드에서 오버라이드 가능)

**위치:** `RimworldSource/Verse/Verb.cs:378-383`

```378:383:RimworldSource/Verse/Verb.cs
public virtual float? AimAngleOverride
{
	get
	{
		return null;
	}
}
```

**사용법:**
커스텀 Verb 클래스에서 오버라이드하여 조준 각도를 완전히 제어 가능:

```csharp
public class Verb_MyCustomWeapon : Verb_Shoot
{
	public override float? AimAngleOverride
	{
		get
		{
			// 커스텀 각도 계산 로직
			return calculatedAngle;
		}
	}
}
```

**효과:**
- `null`이면 기본 각도 계산 사용
- 값이 있으면 해당 각도로 강제 설정

### 3. 리코일 속성 (선택적)

**위치:** `RimworldSource/RimWorld/EquipmentUtility.cs:168-193`

리코일은 무기 각도에 추가적인 영향을 줍니다:

```168:193:RimworldSource/RimWorld/EquipmentUtility.cs
public static void Recoil(ThingDef weaponDef, Verb_LaunchProjectile shootVerb, out Vector3 drawOffset, out float angleOffset, float aimAngle)
{
	drawOffset = Vector3.zero;
	angleOffset = 0f;
	if (weaponDef.recoilPower > 0f && shootVerb != null)
	{
		Rand.PushState(shootVerb.LastShotTick);
		try
		{
			int num = Find.TickManager.TicksGame - shootVerb.LastShotTick;
			if ((float)num < weaponDef.recoilRelaxation)
			{
				float num2 = Mathf.Clamp01((float)num / weaponDef.recoilRelaxation);
				float num3 = Mathf.Lerp(weaponDef.recoilPower, 0f, num2);
				drawOffset = new Vector3((float)Rand.Sign * EquipmentUtility.RecoilCurveAxisX.Evaluate(num2), 0f, -EquipmentUtility.RecoilCurveAxisY.Evaluate(num2)) * num3;
				angleOffset = (float)Rand.Sign * EquipmentUtility.RecoilCurveRotation.Evaluate(num2) * num3;
				aimAngle += angleOffset;
				drawOffset = drawOffset.RotatedBy(aimAngle);
			}
		}
		finally
		{
			Rand.PopState();
		}
	}
}
```

**ThingDef 속성:**
- `recoilPower`: 리코일 강도
- `recoilRelaxation`: 리코일 완화 시간 (틱)

## 커스터마이징 가능 여부 요약

| 항목 | 커스터마이징 가능 여부 | 방법 |
|------|---------------------|------|
| **손에 들고 있을 때 각도** | ❌ 불가능 | 하드코딩됨 (143도 또는 217도) |
| **조준 시 각도** | ✅ 가능 | `Verb.AimAngleOverride` 오버라이드 |
| **각도 오프셋** | ✅ 가능 | `ThingDef.equippedAngleOffset` (XML) |
| **거리 오프셋** | ✅ 가능 | `ThingDef.equippedDistanceOffset` (XML) |
| **리코일** | ✅ 가능 | `ThingDef.recoilPower`, `recoilRelaxation` (XML) |

## 결론

1. **손에 들고 있을 때 각도**는 하드코딩되어 있어 직접 커스터마이징 불가능합니다.
2. **발사 시 조준 각도**는 `Verb.AimAngleOverride`를 오버라이드하여 완전히 제어 가능합니다.
3. **각도 및 거리 오프셋**은 XML에서 `equippedAngleOffset`과 `equippedDistanceOffset`으로 설정 가능합니다.
4. 커스텀 무기를 만들 때는 커스텀 Verb 클래스를 만들어 `AimAngleOverride`를 구현하면 원하는 조준 각도를 설정할 수 있습니다.

## 참고 코드 위치

- 무기 드로잉 메서드: `RimworldSource/Verse/PawnRenderUtility.cs`
- 리코일 처리: `RimworldSource/RimWorld/EquipmentUtility.cs`
- Verb 클래스: `RimworldSource/Verse/Verb.cs`
- ThingDef 속성: `RimworldSource/Verse/ThingDef.cs`


