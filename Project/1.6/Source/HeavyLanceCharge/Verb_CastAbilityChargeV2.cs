using System.Linq;
using RimWorld;
using Verse;

namespace NewRatkin
{
	/// <summary>
	/// LOS 기반 목적지 선정 + 최소/최대 거리(3~5칸) 적용.
	/// 직선 경로 상 LOS가 가로막히는 타일 직전까지만 돌진.
	/// </summary>
	public class Verb_CastAbilityChargeV2 : Verb_CastAbilityCharge
	{
		/// <summary>
		/// 무기에 JumpRange 스탯이 없으면 verbProps.range 사용.
		/// </summary>
		public override float EffectiveRange
		{
			get
			{
				float r = base.EffectiveRange;
				if (r <= 0f)
					return verbProps.range;
				return r;
			}
		}

		/// <summary>
		/// 최소 거리(3칸) 미만이면 타겟 불가.
		/// </summary>
		public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
		{
			float distSq = (float)root.DistanceToSquared(targ.Cell);
			float minRangeSq = verbProps.minRange * verbProps.minRange;
			if (distSq < minRangeSq)
				return false;
			return base.CanHitTargetFrom(root, targ);
		}

		/// <summary>
		/// LOS 기반 목적지: caster→target 직선 경로 상, LOS 차단 또는 경로 상 Pawn 직전까지 돌진.
		/// 경로에 적대 Pawn이 있으면 그 Pawn을 effectiveTarget으로 변경하여 멈춤 (뚫고 지나가지 않음).
		/// 최대 EffectiveRange(5칸) 이내로 제한.
		/// </summary>
		protected override LocalTargetInfo ResolveChargeDestination(out LocalTargetInfo effectiveTarget)
		{
			effectiveTarget = currentTarget;
			LocalTargetInfo targ = currentTarget;
			if (!targ.IsValid || CasterPawn?.Map == null)
				return LocalTargetInfo.Invalid;

			// 대상이 이동 중이면 돌진 직전 시점의 대상 위치로 점프 (currentTarget.Cell = Thing의 최신 위치)
			if (targ.Pawn != null && targ.Pawn.pather.MovingNow)
			{
				effectiveTarget = targ;
				LocalTargetInfo dest = FindAdjacentLanding(targ.Cell);
				return dest.IsValid ? dest : new LocalTargetInfo(targ.Cell);
			}

			IntVec3 targetCell = targ.Cell;
			IntVec3 casterPos = CasterPawn.Position;
			Map map = CasterPawn.Map;

			IntVec3 lastValid = IntVec3.Invalid;
			float maxRangeSq = EffectiveRange * EffectiveRange;

			foreach (IntVec3 cell in GenSight.PointsOnLineOfSight(casterPos, targetCell))
			{
				if (cell == casterPos)
					continue;
				if ((float)casterPos.DistanceToSquared(cell) > maxRangeSq)
					break;
				if (cell == targetCell)
					break;

				// 경로에 Pawn이 있으면 그 Pawn을 타겟으로 변경하고 직전 셀에서 멈춤
				Pawn pathPawn = cell.GetFirstPawn(map);
				if (pathPawn != null && pathPawn != targ.Pawn && IsValidChargeTarget(pathPawn))
				{
					effectiveTarget = new LocalTargetInfo(pathPawn);
					if (lastValid.IsValid)
						return new LocalTargetInfo(lastValid);
					// Pawn이 caster 바로 옆이면 8인접 중 유효 셀 탐색
					return FindAdjacentLanding(pathPawn.Position);
				}

				if (!cell.CanBeSeenOverFast(map))
					break;
				if (JumpUtility.ValidJumpTarget(CasterPawn, map, cell))
					lastValid = cell;
			}

			if (lastValid.IsValid)
				return new LocalTargetInfo(lastValid);

			return base.ResolveChargeDestination(out effectiveTarget);
		}

		/// <summary>
		/// targetCell 주변 8셀 중 caster에 가장 가까운 유효 착지 셀.
		/// </summary>
		private LocalTargetInfo FindAdjacentLanding(IntVec3 targetCell)
		{
			IntVec3 casterPos = CasterPawn.Position;
			Map map = CasterPawn.Map;
			IntVec3 best = IntVec3.Invalid;
			int bestDistSq = int.MaxValue;
			for (int i = 0; i < GenAdj.AdjacentCells.Length; i++)
			{
				IntVec3 cell = targetCell + GenAdj.AdjacentCells[i];
				if (!JumpUtility.ValidJumpTarget(CasterPawn, map, cell))
					continue;
				int d = cell.DistanceToSquared(casterPos);
				if (d < bestDistSq)
				{
					bestDistSq = d;
					best = cell;
				}
			}
			return best.IsValid ? new LocalTargetInfo(best) : LocalTargetInfo.Invalid;
		}

		/// <summary>
		/// 돌진 타겟으로 유효한 Pawn인지 (적대, 살아있음 등).
		/// </summary>
		private bool IsValidChargeTarget(Pawn p)
		{
			if (p == null || p.Destroyed || !p.Spawned)
				return false;
			CompAbilityEffect_ChargeOnJump chargeComp = ability?.comps?.OfType<CompAbilityEffect_ChargeOnJump>().FirstOrDefault();
			CompProperties_ChargeOnJump props = chargeComp?.props as CompProperties_ChargeOnJump;
			if (props != null && props.onlyHostilePawns && !p.HostileTo(CasterPawn))
				return false;
			return true;
		}
	}
}
