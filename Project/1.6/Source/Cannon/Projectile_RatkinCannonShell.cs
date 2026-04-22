using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NewRatkin
{
	/// <summary>
	/// 랫킨 포탄: 직격 시 폭발. 빈 지면 첫 착탄 시 <see cref="ProjectileProperties_RatkinCannonShell"/>의 반경·피해로 폭발 후 같은 방향으로 비행 거리 절반만 비행, 이후 첫 충돌 시 폭발.
	/// </summary>
	public class Projectile_RatkinCannonShell : Projectile_Explosive
	{
		private const float MinBounceLegCells = 18f;

		private ProjectileProperties_RatkinCannonShell ShellProps =>
			def.projectile as ProjectileProperties_RatkinCannonShell;

		private float GroundTouchExplosionRadiusFactor =>
			Mathf.Max(0.01f, ShellProps?.groundTouchExplosionRadiusFactor ?? 1.5f);

		private bool didGroundBounce;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref didGroundBounce, "rkCannonShellGroundBounce", false);
		}

		protected override void Impact(Thing hitThing, bool blockedByShield = false)
		{
			if (blockedByShield)
			{
				DetonateFromImpact();
				return;
			}

			if (hitThing == null)
			{
				if (!didGroundBounce)
				{
					didGroundBounce = true;
					PlayBounceImpactSound();
					DoGroundTouchExplosion();
					ContinueFlightHalfRemainingDistance();
					return;
				}

				DetonateFromImpact();
				return;
			}

			DetonateFromImpact();
		}

		private void DetonateFromImpact()
		{
			GenClamor.DoClamor(this, 12f, ClamorDefOf.Impact);
			if (def.projectile.landedEffecter != null)
			{
				def.projectile.landedEffecter.Spawn(Position, Map, 1f).Cleanup();
			}

			Explode();
		}

		private void PlayBounceImpactSound()
		{
			SoundDef snd = def.projectile.soundImpact;
			if (!snd.NullOrUndefined())
			{
				snd.PlayOneShot(SoundInfo.InMap(new TargetInfo(Position, Map), MaintenanceType.None));
			}
		}

		private int ResolveGroundTouchDamageAmount()
		{
			ProjectileProperties_RatkinCannonShell p = ShellProps;
			if (p != null && p.groundTouchDamageAmount >= 0)
			{
				return p.groundTouchDamageAmount;
			}

			return DamageAmount;
		}

		/// <summary>지면 도탄 순간: 본체는 유지. <see cref="Projectile_Explosive.Explode"/>와 동일 계열, 반경·피해는 Def 설정.</summary>
		private void DoGroundTouchExplosion()
		{
			Map map = Map;
			if (map == null)
			{
				return;
			}

			if (def.projectile.explosionEffect != null)
			{
				Effecter effecter = def.projectile.explosionEffect.Spawn();
				if (def.projectile.explosionEffectLifetimeTicks != 0)
				{
					map.effecterMaintainer.AddEffecterToMaintain(
						effecter,
						Position.ToVector3().ToIntVec3(),
						def.projectile.explosionEffectLifetimeTicks);
				}
				else
				{
					effecter.Trigger(new TargetInfo(Position, map), new TargetInfo(Position, map), -1);
					effecter.Cleanup();
				}
			}

			float explosionRadius = def.projectile.explosionRadius * GroundTouchExplosionRadiusFactor;
			DamageDef damageDef = DamageDef;
			Thing launcher = this.launcher;
			int damageAmount = ResolveGroundTouchDamageAmount();
			float armorPenetration = ArmorPenetration;
			SoundDef soundExplode = def.projectile.soundExplode;
			ThingDef equipmentDef = this.equipmentDef;
			ThingDef projectileDef = def;
			Thing intended = intendedTarget.Thing;
			ThingDef postExplosionSpawnThingDef = def.projectile.postExplosionSpawnThingDef
				?? (def.projectile.explosionSpawnsSingleFilth ? null : def.projectile.filth);
			ThingDef postExplosionSpawnThingDefWater = def.projectile.postExplosionSpawnThingDefWater;
			float postExplosionSpawnChance = def.projectile.postExplosionSpawnChance;
			int postExplosionSpawnThingCount = def.projectile.postExplosionSpawnThingCount;
			GasType? postExplosionGasType = def.projectile.postExplosionGasType;
			ThingDef preExplosionSpawnThingDef = def.projectile.preExplosionSpawnThingDef;
			float preExplosionSpawnChance = def.projectile.preExplosionSpawnChance;
			int preExplosionSpawnThingCount = def.projectile.preExplosionSpawnThingCount;
			bool applyDamageToExplosionCellsNeighbors = def.projectile.applyDamageToExplosionCellsNeighbors;
			float explosionChanceToStartFire = def.projectile.explosionChanceToStartFire;
			bool explosionDamageFalloff = def.projectile.explosionDamageFalloff;
			float? direction = new float?(origin.AngleToFlat(destination));
			float expolosionPropagationSpeed = damageDef.expolosionPropagationSpeed;
			float screenShakeFactor = def.projectile.screenShakeFactor;
			bool doExplosionVFX = def.projectile.doExplosionVFX;
			ThingDef preExplosionSpawnSingleThingDef = def.projectile.preExplosionSpawnSingleThingDef;
			ThingDef postExplosionSpawnSingleThingDef = def.projectile.postExplosionSpawnSingleThingDef;

			GenExplosion.DoExplosion(
				Position,
				map,
				explosionRadius,
				damageDef,
				launcher,
				damageAmount,
				armorPenetration,
				soundExplode,
				equipmentDef,
				projectileDef,
				intended,
				postExplosionSpawnThingDef,
				postExplosionSpawnChance,
				postExplosionSpawnThingCount,
				postExplosionGasType,
				null,
				255,
				applyDamageToExplosionCellsNeighbors,
				preExplosionSpawnThingDef,
				preExplosionSpawnChance,
				preExplosionSpawnThingCount,
				explosionChanceToStartFire,
				explosionDamageFalloff,
				direction,
				null,
				null,
				doExplosionVFX,
				expolosionPropagationSpeed,
				0f,
				true,
				postExplosionSpawnThingDefWater,
				screenShakeFactor,
				null,
				null,
				postExplosionSpawnSingleThingDef,
				preExplosionSpawnSingleThingDef);

			if (def.projectile.explosionSpawnsSingleFilth && def.projectile.filth != null && def.projectile.filthCount.TrueMax > 0
				&& Rand.Chance(def.projectile.filthChance) && !Position.Filled(map))
			{
				FilthMaker.TryMakeFilth(
					Position,
					map,
					def.projectile.filth,
					def.projectile.filthCount.RandomInRange,
					FilthSourceFlags.None,
					true);
			}
		}

		private void ContinueFlightHalfRemainingDistance()
		{
			Vector3 dir = (destination - origin).Yto0();
			if (dir.sqrMagnitude < 1E-6f)
			{
				dir = Vector3.forward;
			}
			else
			{
				dir.Normalize();
			}

			float fullLeg = Mathf.Max(MinBounceLegCells, (destination - origin).MagnitudeHorizontal());
			float leg = fullLeg * 0.5f;
			Vector3 pos = ExactPosition;
			origin = pos + dir * 0.06f;
			destination = origin + dir * Mathf.Max(0.25f, leg);
			ResetFlightAfterRedirect();
		}

		private void ResetFlightAfterRedirect()
		{
			ticksToImpact = Mathf.CeilToInt(StartingTicksToImpact);
			if (ticksToImpact < 1)
			{
				ticksToImpact = 1;
			}

			lifetime = ticksToImpact;
			landed = false;
		}
	}
}
