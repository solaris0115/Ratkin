using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;

namespace NewRatkin
{
	public class Verb_CastAbilityCharge : Verb_CastAbilityJump
	{
		/// <summary>
		/// 돌진 목적지 계산: 대상이 움직이면 현재 위치로, 정지 시 주변 8셀 중 나와 가장 가까운 유효 셀로.
		/// currentTarget은 LocalTargetInfo로 Thing 참조 시 TryCastShot 시점의 최신 위치를 반영(추적됨).
		/// effectiveTarget: 실제 돌진 대상 (경로에 다른 Pawn이 있으면 그 Pawn으로 변경 가능).
		/// </summary>
		protected virtual LocalTargetInfo ResolveChargeDestination(out LocalTargetInfo effectiveTarget)
		{
			effectiveTarget = currentTarget;
			LocalTargetInfo targ = currentTarget;
			if (!targ.IsValid || CasterPawn?.Map == null)
				return LocalTargetInfo.Invalid;

			IntVec3 targetCell = targ.Cell; // Thing이면 PositionHeld(현재 위치) 반환 → 돌진 직전까지 추적

			// 대상이 Pawn이고 이동 중이면 → 대상 현재 위치로
			if (targ.Pawn != null && targ.Pawn.pather.MovingNow)
				return new LocalTargetInfo(targetCell);

			// 대상이 정지 상태 → 대상 주변 8셀 중 나와 가장 가까운 유효 셀
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

			if (best.IsValid)
				return new LocalTargetInfo(best);

			// 유효한 주변 셀이 없으면 대상 셀 시도(PawnFlyer.CheckDestination이 대체 셀 탐색)
			return new LocalTargetInfo(targetCell);
		}

		protected override bool TryCastShot()
		{
			// 돌진 직전 시야 검사: 대상이 보이지 않으면 취소
			if (!CanHitTarget(currentTarget))
				return false;

			CompAbilityEffect_ChargeOnJump chargeComp = ability?.comps?.OfType<CompAbilityEffect_ChargeOnJump>().FirstOrDefault();
			chargeComp?.ApplyHediffsImmediately(CasterPawn);

			LocalTargetInfo effectiveTarget;
			LocalTargetInfo dest = ResolveChargeDestination(out effectiveTarget);
			if (!dest.IsValid)
				return false;

			// 돌진 시작 시점에 사운드 재생
			if (verbProps.soundCast != null && CasterPawn?.MapHeld != null)
				verbProps.soundCast.PlayOneShot(new TargetInfo(CasterPawn.Position, CasterPawn.MapHeld, false));

			// ability.Activate만 호출 후, 계산된 목적지로 DoJump (base 호출 시 currentTarget으로 중복 점프됨)
			return (ability?.Activate(effectiveTarget, currentDestination) ?? false)
				&& JumpUtility.DoJump(CasterPawn, dest, ReloadableCompSource, verbProps, ability, effectiveTarget, JumpFlyerDef);
		}

		public override ThingDef JumpFlyerDef
		{
			get
			{
				CompAbilityEffect_ChargeOnJump chargeComp = ability?.comps?.OfType<CompAbilityEffect_ChargeOnJump>().FirstOrDefault();
				CompProperties_ChargeOnJump props = chargeComp?.props as CompProperties_ChargeOnJump;
				return props?.pawnFlyerDef ?? ThingDefOf.PawnFlyer;
			}
		}

		/// <summary>
		/// 대상이 시야에 없으면(은신, 장애물, LOS 차단) 돌진 불가.
		/// Verb_CastAbilityJump는 JumpUtility만 사용해 IsPsychologicallyInvisible 등을 검사하지 않으므로 여기서 보강.
		/// </summary>
		public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
		{
			if (targ.Thing != null && targ.Thing == caster)
				return targetParams.canTargetSelf;
			// 은신한 적대 pawn은 타겟 불가
			if (targ.Pawn != null && targ.Pawn.IsPsychologicallyInvisible() && caster.HostileTo(targ.Pawn))
				return false;
			if (ApparelPreventsShooting())
				return false;
			return JumpUtility.CanHitTargetFrom(CasterPawn, root, targ, EffectiveRange);
		}

		/// <summary>
		/// OrderJump 대신 QueueCastingJob 사용 → job.targetA = Pawn → warmup 시 focusTarg가 Thing 참조로
		/// 매 프레임 대상의 현재 위치를 추적하여 조준 연출이 이동 중인 대상을 따라감.
		/// </summary>
		public override void OrderForceTarget(LocalTargetInfo target)
		{
			if (ability != null && target.IsValid)
			{
				ability.QueueCastingJob(target, null);
				return;
			}
			base.OrderForceTarget(target);
		}

		/// <summary>
		/// Verb_CastAbilityJump는 ValidateTarget을 재정의하여 EffectComps.Valid() 검사를 건너뜀.
		/// 돌진은 적대 pawn만 타겟 가능하므로, 여기서 EffectComps 검사를 추가함.
		/// </summary>
		public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
		{
			if (!base.ValidateTarget(target, showMessages))
				return false;
			if (ability?.EffectComps == null)
				return true;
			for (int i = 0; i < ability.EffectComps.Count; i++)
			{
				if (!ability.EffectComps[i].Valid(target, showMessages))
					return false;
			}
			return true;
		}
	}
}
