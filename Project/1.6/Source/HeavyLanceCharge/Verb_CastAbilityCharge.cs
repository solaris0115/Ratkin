using System.Linq;
using RimWorld;
using Verse;

namespace NewRatkin
{
	public class Verb_CastAbilityCharge : Verb_CastAbilityJump
	{
		protected override bool TryCastShot()
		{
			CompAbilityEffect_ChargeOnJump chargeComp = ability?.comps?.OfType<CompAbilityEffect_ChargeOnJump>().FirstOrDefault();
			chargeComp?.ApplyHediffsImmediately(CasterPawn);
			return base.TryCastShot();
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
	}
}
