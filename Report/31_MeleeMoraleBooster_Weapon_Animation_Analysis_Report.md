# RK_Ability_MeleeMoraleBooster 무기 애니메이션 분석 리포트

## 개요
RK_Ability_MeleeMoraleBooster 사용 시 무기를 위로 치켜드는 애니메이션 설정 방법을 조사하고, RimWorld 소스 코드에서 관련 로직을 분석했습니다.

## 현재 상황

### RK_Ability_MeleeMoraleBooster 정의
```31:48:Project/1.6/Defs/AbilityDefs/AbilityDefs.xml
<AbilityDef ParentName="RoleAuraBuffBase">
	<defName>RK_Ability_MeleeMoraleBooster</defName>
	<label>morale booster</label>
	<description>Boost the morale of everyone nearby.</description>
	<iconPath>UI/Abilities/RK_TextureIcon_Banner</iconPath>
	<warmupMoteSocialSymbol>UI/Icons/RK_TextureIcon_Horn</warmupMoteSocialSymbol>
	<casterMustBeCapableOfViolence>false</casterMustBeCapableOfViolence>
	<groupDef>RK_AbilityGroup_MoraleBoost</groupDef>		<!--overrides MultiRole-->
	<comps>
		<li Class="CompProperties_AbilityGiveHediff">
			<compClass>CompAbilityEffect_GiveHediff</compClass>
			<hediffDef>RK_Hediff_MoraleBooster</hediffDef>
			<onlyBrain>True</onlyBrain>
			<onlyApplyToSelf>True</onlyApplyToSelf>
			<replaceExisting>true</replaceExisting>
		</li>
	</comps>
</AbilityDef>
```

### 부모 AbilityDef: RoleAuraBuffBase
```390:421:RimWorldData/Ideology/Defs/AbilityDefs/Abilities.xml
<AbilityDef Abstract="True" Name="RoleAuraBuffBase">
	<jobDef>CastAbilityOnThing</jobDef>
	<targetRequired>False</targetRequired>
	<canUseAoeToGetTargets>False</canUseAoeToGetTargets>
	<stunTargetWhileCasting>True</stunTargetWhileCasting>
	<showPsycastEffects>False</showPsycastEffects>
	<sendMessageOnCooldownComplete>true</sendMessageOnCooldownComplete>
	<displayGizmoWhileUndrafted>True</displayGizmoWhileUndrafted>
	<disableGizmoWhileUndrafted>False</disableGizmoWhileUndrafted>
	<groupDef>MultiRole</groupDef>
	<hotKey>Misc12</hotKey>
	<warmupStartSound>CombatCommand_Warmup</warmupStartSound>
	<statBases>
		<Ability_Duration>1000</Ability_Duration>
		<Ability_EffectRadius>9.9</Ability_EffectRadius>
	</statBases>
	<verbProperties>
		<verbClass>Verb_CastAbility</verbClass>
		<warmupTime>0.5</warmupTime>
		<range>9.9</range>
		<drawAimPie>False</drawAimPie>
		<requireLineOfSight>False</requireLineOfSight>
		<targetParams>
			<canTargetSelf>true</canTargetSelf>
			<canTargetPawns>false</canTargetPawns>
			<canTargetBuildings>false</canTargetBuildings>
			<canTargetAnimals>false</canTargetAnimals>
			<canTargetHumans>false</canTargetHumans>
			<canTargetMechs>false</canTargetMechs>
		</targetParams>
	</verbProperties>
</AbilityDef>
```

## 무기 애니메이션 메커니즘

### 무기 각도 계산 로직

**코드 위치:** `RimworldSource/Verse/PawnRenderUtility.cs:254-268`

무기 각도는 다음 순서로 계산됩니다:

1. **기본 각도 계산**: `stance_Busy.focusTarg`를 향한 각도 계산
2. **AimAngleOverride 확인**: `Verb.AimAngleOverride` 속성이 있으면 해당 값으로 오버라이드
3. **각도 적용**: 계산된 각도가 `DrawEquipmentAiming`에 전달됨

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

### Verb.AimAngleOverride 속성

**코드 위치:** `RimworldSource/Verse/Verb.cs:378-383`

```378:383:RimworldSource/Verse/Verb.cs
public virtual float? AimAngleOverride
{
	get
	{
		return null;
	}
}
```

- 기본값: `null` (기본 각도 계산 사용)
- 오버라이드 가능: 커스텀 Verb 클래스에서 오버라이드하여 각도 제어 가능

## 해결 방안

### 방법 1: 커스텀 Verb_CastAbility 클래스 생성 (권장)

무기를 위로 치켜들기 위해 커스텀 Verb 클래스를 만들어 `AimAngleOverride`를 오버라이드합니다.

#### 1. 커스텀 Verb 클래스 생성

**파일:** `Project/1.6/Source/Ability/Verb_CastAbilityRaiseWeapon.cs`

```csharp
using RimWorld;
using Verse;

namespace NewRatkin
{
	public class Verb_CastAbilityRaiseWeapon : Verb_CastAbility
	{
		public override float? AimAngleOverride
		{
			get
			{
				// 위쪽 각도: 90도 (북쪽, 수직 위)
				// 또는 45도~135도 사이의 값으로 대각선 위 설정 가능
				return 90f;
			}
		}
	}
}
```

#### 2. AbilityDef 수정

**파일:** `Project/1.6/Defs/AbilityDefs/AbilityDefs.xml`

```xml
<AbilityDef ParentName="RoleAuraBuffBase">
	<defName>RK_Ability_MeleeMoraleBooster</defName>
	<label>morale booster</label>
	<description>Boost the morale of everyone nearby.</description>
	<iconPath>UI/Abilities/RK_TextureIcon_Banner</iconPath>
	<warmupMoteSocialSymbol>UI/Icons/RK_TextureIcon_Horn</warmupMoteSocialSymbol>
	<casterMustBeCapableOfViolence>false</casterMustBeCapableOfViolence>
	<groupDef>RK_AbilityGroup_MoraleBoost</groupDef>
	<verbProperties>
		<verbClass>NewRatkin.Verb_CastAbilityRaiseWeapon</verbClass>
		<warmupTime>0.5</warmupTime>
		<range>9.9</range>
		<drawAimPie>False</drawAimPie>
		<requireLineOfSight>False</requireLineOfSight>
		<targetParams>
			<canTargetSelf>true</canTargetSelf>
			<canTargetPawns>false</canTargetPawns>
			<canTargetBuildings>false</canTargetBuildings>
			<canTargetAnimals>false</canTargetAnimals>
			<canTargetHumans>false</canTargetHumans>
			<canTargetMechs>false</canTargetMechs>
		</targetParams>
	</verbProperties>
	<comps>
		<li Class="CompProperties_AbilityGiveHediff">
			<compClass>CompAbilityEffect_GiveHediff</compClass>
			<hediffDef>RK_Hediff_MoraleBooster</hediffDef>
			<onlyBrain>True</onlyBrain>
			<onlyApplyToSelf>True</onlyApplyToSelf>
			<replaceExisting>true</replaceExisting>
		</li>
	</comps>
</AbilityDef>
```

### 방법 2: 동적 각도 계산 (고급)

Pawn의 facing 방향에 따라 위쪽 각도를 계산하는 방법:

```csharp
public class Verb_CastAbilityRaiseWeapon : Verb_CastAbility
{
	public override float? AimAngleOverride
	{
		get
		{
			if (this.CasterPawn == null || !this.CasterPawn.Spawned)
			{
				return null;
			}
			
			// Pawn의 facing 방향에 따라 위쪽 각도 계산
			Rot4 facing = this.CasterPawn.Rotation;
			float baseAngle = 90f; // 기본 위쪽 각도
			
			// facing 방향에 따라 약간 조정 가능
			// 예: facing이 East(1)이면 90도, West(3)이면 90도 등
			return baseAngle;
		}
	}
}
```

### 각도 값 참고

RimWorld의 각도 시스템:
- **0도**: 동쪽 (East)
- **90도**: 북쪽 (North, 위쪽)
- **180도**: 서쪽 (West)
- **270도**: 남쪽 (South, 아래쪽)

무기를 위로 치켜들기 위한 권장 각도:
- **90도**: 수직 위 (가장 일반적)
- **45도~135도**: 대각선 위 (더 자연스러운 느낌)
- **Pawn facing + 90도**: Pawn이 바라보는 방향의 위쪽

## 구현 단계

1. **커스텀 Verb 클래스 생성**
   - `Project/1.6/Source/Ability/Verb_CastAbilityRaiseWeapon.cs` 생성
   - `AimAngleOverride` 속성 오버라이드

2. **AbilityDef 수정**
   - `verbProperties.verbClass`를 커스텀 클래스로 변경
   - `verbProperties` 전체를 명시적으로 지정 (부모에서 상속받지 않도록)

3. **빌드 및 테스트**
   - 프로젝트 빌드
   - 게임에서 ability 사용 시 무기가 위로 치켜드는지 확인

## 참고사항

- `AimAngleOverride`는 `Stance_Warmup` 중에만 적용됩니다 (warmupTime > 0)
- `neverAimWeapon` 속성이 true이면 무기 조준이 비활성화됩니다
- 무기가 없으면 애니메이션이 표시되지 않습니다
- 각도는 0~360도 범위 내에서 설정해야 합니다

## 관련 파일

- `RimworldSource/RimWorld/Verb_CastAbility.cs` - Verb_CastAbility 소스 코드
- `RimworldSource/Verse/PawnRenderUtility.cs` - 무기 드로잉 로직
- `RimworldSource/Verse/Verb.cs` - Verb 클래스 (AimAngleOverride)
- `Report/28_Weapon_Drawing_Angle_Analysis_Report.md` - 무기 각도 분석 리포트











