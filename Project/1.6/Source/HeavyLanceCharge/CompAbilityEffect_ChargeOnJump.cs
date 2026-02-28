using RimWorld;
using Verse;

namespace NewRatkin
{
	public class CompAbilityEffect_ChargeOnJump : CompAbilityEffect, ICompAbilityEffectOnJumpCompleted
	{
		private new CompProperties_ChargeOnJump Props => (CompProperties_ChargeOnJump)props;

		public void OnJumpCompleted(IntVec3 origin, LocalTargetInfo target)
		{
			Pawn pawn = parent.pawn;
			if (pawn == null) return;

			if (Props.exhaustionHediffDef != null)
			{
				Hediff exhaustion = HediffMaker.MakeHediff(Props.exhaustionHediffDef, pawn, null);
				Verse.HediffComp_Disappears compExhaustion = exhaustion.TryGetComp<Verse.HediffComp_Disappears>();
				if (compExhaustion != null)
					compExhaustion.SetDuration(Props.exhaustionDurationTicks);
				pawn.health.AddHediff(exhaustion, null, null, null);
			}

			if (Props.focusHediffDef != null)
			{
				Hediff focus = HediffMaker.MakeHediff(Props.focusHediffDef, pawn, null);
				Verse.HediffComp_Disappears compFocus = focus.TryGetComp<Verse.HediffComp_Disappears>();
				if (compFocus != null)
					compFocus.SetDuration(Props.focusDurationTicks);
				pawn.health.AddHediff(focus, null, null, null);
			}
		}

		public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			if (target.Pawn == null)
				return false;
			if (Props.onlyHostilePawns && !target.Pawn.HostileTo(parent.pawn))
				return false;
			return base.Valid(target, throwMessages);
		}

		public override bool AICanTargetNow(LocalTargetInfo target)
		{
			if (target.Pawn == null)
				return false;
			if (Props.onlyHostilePawns && !target.Pawn.HostileTo(parent.pawn))
				return false;
			return true;
		}
	}
}
