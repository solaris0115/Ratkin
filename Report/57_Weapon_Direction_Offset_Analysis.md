# 무기 드로잉 방향별 오프셋 조정 분석 리포트

## 개요
RimWorld에서 `Verb_Shoot`에 의한 발사 시 pawn draw에 대해서 무기를 그리는 부분의 오프셋을 동서남북 방향별로 조정 가능한지 분석했습니다.

## 무기 드로잉 메커니즘 분석

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

**방향별 오프셋 상수:**
```18:24:RimworldSource/Verse/PawnRenderUtility.cs
private static readonly Vector3 EqLocNorth = new Vector3(0f, 0f, -0.11f);
private static readonly Vector3 EqLocEast = new Vector3(0.22f, 0f, -0.22f);
private static readonly Vector3 EqLocSouth = new Vector3(0f, 0f, -0.22f);
private static readonly Vector3 EqLocWest = new Vector3(-0.22f, 0f, -0.22f);
```

**특징:**
- 방향별 오프셋이 하드코딩되어 있음
- `equipmentDrawDistanceFactor`로 크기 조정만 가능
- 무기별로 다른 오프셋 적용 불가능

### 2. 발사 시 조준 (Aiming Weapon)

**코드 위치:** `RimworldSource/Verse/PawnRenderUtility.cs:254-268`

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

**특징:**
- 조준 시에는 타겟을 향한 각도에 따라 오프셋이 회전됨
- `equippedDistanceOffset`으로 거리만 조정 가능
- 방향별 고정 오프셋 적용 불가능

### 3. 무기 드로잉 실행 (DrawEquipmentAiming)

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

**drawSize 사용:**
- 111번째 줄: `Vector3 s = new Vector3(eq.Graphic.drawSize.x, 0f, eq.Graphic.drawSize.y);`
- 무기의 크기(스케일) 결정에만 사용됨
- 오프셋 위치 조정에는 사용되지 않음

## 현재 제한사항

1. **방향별 오프셋 하드코딩**: `EqLocNorth`, `EqLocEast`, `EqLocSouth`, `EqLocWest`가 정적 상수로 하드코딩되어 있음
2. **무기별 커스터마이징 불가**: 모든 무기가 동일한 오프셋 사용
3. **조준 시 방향 고정 불가**: 조준 시에는 각도에 따라 회전된 오프셋만 사용됨

## 해결 방안

### 방안 1: Harmony 패치를 통한 오프셋 오버라이드

**구현 방법:**
1. `CompProperties_WeaponDirectionOffset` 컴포넌트 생성
2. 방향별 오프셋 속성 정의 (`offsetNorth`, `offsetEast`, `offsetSouth`, `offsetWest`)
3. `DrawCarriedWeapon`과 `DrawEquipmentAndApparelExtras`를 Harmony 패치
4. 무기에 Comp가 있으면 해당 오프셋 사용, 없으면 기본값 사용

**장점:**
- 기존 코드 수정 없이 확장 가능
- XML에서 간단하게 설정 가능
- 무기별로 다른 오프셋 적용 가능

**단점:**
- Harmony 패치 필요
- 조준 시 방향 고정은 추가 로직 필요

### 방안 2: ThingDef 확장 속성 추가

**구현 방법:**
1. ThingDef에 방향별 오프셋 속성 추가
2. Harmony 패치로 해당 속성 읽어서 적용

**장점:**
- Comp 없이 직접 속성으로 설정 가능
- 간단한 구조

**단점:**
- ThingDef 확장 필요
- 조준 시 방향 고정은 추가 로직 필요

## 결론

**방향별 오프셋 조정은 가능합니다.**

1. **손에 들고 있을 때**: Harmony 패치로 `DrawCarriedWeapon`의 오프셋을 무기별로 오버라이드 가능
2. **조준 시**: `DrawEquipmentAndApparelExtras`를 패치하여 조준 시에도 방향별 오프셋 적용 가능
3. **drawSize**: 오프셋 위치 조정에는 사용되지 않지만, 무기 크기 조정에 사용됨

**권장 구현:**
- `CompProperties_WeaponDirectionOffset` 컴포넌트 생성
- 방향별 오프셋 속성 정의
- Harmony 패치로 `DrawCarriedWeapon`과 `DrawEquipmentAndApparelExtras` 수정
- XML에서 무기별로 방향별 오프셋 설정 가능하도록 구현

## 참고 코드 위치

- 무기 드로잉 메서드: `RimworldSource/Verse/PawnRenderUtility.cs`
- 방향별 오프셋 상수: `RimworldSource/Verse/PawnRenderUtility.cs:18-24`
- 조준 시 오프셋 계산: `RimworldSource/Verse/PawnRenderUtility.cs:254-268`
- drawSize 사용: `RimworldSource/Verse/PawnRenderUtility.cs:111`




