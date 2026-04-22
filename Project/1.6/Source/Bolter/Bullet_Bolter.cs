using RimWorld;
using UnityEngine;
using Verse;

namespace NewRatkin
{
	/// <summary>
	/// 볼터 대구경 탄환: 직접 적중 시 30% 확률로 짧은 스턴.
	/// 기계(메카노이드) 직격 또는 실드(투사체 방어)에 막힌 경우 추가 EMP.
	/// </summary>
	public class Bullet_Bolter : Bullet
	{
		private const float StunOnHitChance = 0.3f;
		private const float StunDurationSeconds = 2f;
		/// <summary>물리 피해와 별도의 보너스 EMP 수치(기계·실드에 동일 적용).</summary>
		private const float ExtraEmpDamage = 6f;

		protected override void Impact(Thing hitThing, bool blockedByShield = false)
		{
			// 실드에 막힌 경우, Destroy 전에 투사체·경로 기준으로 어떤 Comp가 막았는지 식별 (CheckIntercept 재호출 금지: 부수효과)
			CompProjectileInterceptor shieldAtIntercept = null;
			if (blockedByShield)
			{
				shieldAtIntercept = FindFirstInterceptorMatchingShieldBlock();
			}

			base.Impact(hitThing, blockedByShield);

			// 직접 맞은 메카노이드: 보너스 EMP
			if (hitThing is Pawn mechPawn
				&& mechPawn.RaceProps.IsMechanoid
				&& !mechPawn.Dead
				&& !blockedByShield)
			{
				ApplyExtraEmpTo(mechPawn);
			}

			// 투사체 인터셉트(에너지 실드 등)에 막힌 경우: 실드(부착 틱)에 보너스 EMP
			if (shieldAtIntercept != null)
			{
				ApplyExtraEmpTo(shieldAtIntercept.parent);
			}

			// 30% 스턴 — 실드/헛방에는 적용하지 않음
			if (blockedByShield)
			{
				return;
			}

			if (hitThing == null)
			{
				return;
			}

			if (!Rand.Chance(StunOnHitChance))
			{
				return;
			}

			if (!(hitThing is Pawn target) || target.Dead)
			{
				return;
			}

			StunHandler stunner = target.stances?.stunner;
			if (stunner == null)
			{
				return;
			}

			int ticks = StunDurationSeconds.SecondsToTicks();
			stunner.StunFor(ticks, launcher, addBattleLog: true, showMote: true, disableRotation: false);
		}

		private void ApplyExtraEmpTo(Thing target)
		{
			if (target is null || target.Destroyed || !target.Spawned)
			{
				return;
			}

			Pawn instigatorPawn = launcher as Pawn;
			bool instigatorGuilty = instigatorPawn == null || !instigatorPawn.Drafted;
			DamageInfo dinfo = new DamageInfo(
				DamageDefOf.EMP,
				ExtraEmpDamage,
				0.5f,
				-1f,
				launcher,
				null,
				equipmentDef,
				DamageInfo.SourceCategory.ThingOrUnknown,
				intendedTarget.Thing,
				instigatorGuilty,
				true,
				QualityCategory.Normal,
				true,
				false);
			target.TakeDamage(dinfo);
		}

		/// <summary>CompProjectileInterceptor.CheckIntercept(…)의 조건만 복제 (부수효과 없음). 실제로 막은 첫 방패와 동일한 순서로 탐지.</summary>
		private CompProjectileInterceptor FindFirstInterceptorMatchingShieldBlock()
		{
			if (Map is null)
			{
				return null;
			}

			Vector3 newExactPos = ExactPosition;
			Vector3 v = (destination - origin).Yto0();
			if (v.sqrMagnitude < 1E-4f)
			{
				return null;
			}

			float step = Mathf.Max(0.1f, def.projectile.SpeedTilesPerTick);
			Vector3 lastExactPos = newExactPos - v.normalized * step;

			foreach (Thing t in Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor))
			{
				CompProjectileInterceptor comp = t == null ? null : t.TryGetComp<CompProjectileInterceptor>();
				if (comp == null)
				{
					continue;
				}

				if (ShieldInterceptConditionsMatch(this, comp, lastExactPos, newExactPos))
				{
					return comp;
				}
			}

			return null;
		}

		private static bool ShieldInterceptConditionsMatch(Projectile projectile, CompProjectileInterceptor comp, Vector3 lastExactPos, Vector3 newExactPos)
		{
			CompProperties_ProjectileInterceptor props = comp.Props;
			Thing parent = comp.parent;
			Vector3 vector = parent.Position.ToVector3Shifted();
			float horiz = (newExactPos.x - vector.x) * (newExactPos.x - vector.x) + (newExactPos.z - vector.z) * (newExactPos.z - vector.z);
			float maxDist = props.radius + projectile.def.projectile.SpeedTilesPerTick + 0.1f;
			if (horiz > maxDist * maxDist)
			{
				return false;
			}

			if (!comp.Active)
			{
				return false;
			}

			if (!CompProjectileInterceptor.InterceptsProjectile(props, projectile))
			{
				return false;
			}

			// private debug 플래그는 접근 불가: 일반 플레이는 아래로 충분
			if (projectile.Launcher == null && !props.interceptNonHostileProjectiles)
			{
				return false;
			}

			if (parent.Faction != null)
			{
				if (projectile.Launcher != null && projectile.Launcher.Spawned
					&& !projectile.Launcher.HostileTo(parent.Faction))
				{
					return false;
				}

				if (projectile.Launcher != null && !projectile.Launcher.Spawned
					&& !projectile.Launcher.Faction.HostileTo(parent.Faction))
				{
					return false;
				}
			}

			if (!props.interceptOutgoingProjectiles)
			{
				if ((new Vector2(vector.x, vector.z) - new Vector2(lastExactPos.x, lastExactPos.z)).sqrMagnitude
					<= props.radius * props.radius)
				{
					return false;
				}
			}

			return GenGeo.IntersectLineCircleOutline(
				new Vector2(vector.x, vector.z),
				props.radius,
				new Vector2(lastExactPos.x, lastExactPos.z),
				new Vector2(newExactPos.x, newExactPos.z));
		}
	}
}
