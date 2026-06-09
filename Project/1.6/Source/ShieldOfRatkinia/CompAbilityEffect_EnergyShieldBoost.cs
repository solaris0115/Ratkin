using RimWorld;
using Verse;

namespace NewRatkin
{
    public class CompProperties_EnergyShieldBoost : CompProperties_AbilityEffect
    {
        public HediffDef boostHediffDef;

        public int boostDurationTicks = 3600;

        public float psyfocusCost = 0.35f;

        public CompProperties_EnergyShieldBoost()
        {
            compClass = typeof(CompAbilityEffect_EnergyShieldBoost);
        }
    }

    public class CompAbilityEffect_EnergyShieldBoost : CompAbilityEffect
    {
        private new CompProperties_EnergyShieldBoost Props => (CompProperties_EnergyShieldBoost)props;

        public override bool CanCast
        {
            get
            {
                if (!HasRequiredPsyfocus(out _))
                    return false;
                return CompShieldDeflectEnergy.GetWornEnergyShield(parent.pawn) != null;
            }
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (CompShieldDeflectEnergy.GetWornEnergyShield(parent.pawn) == null)
            {
                reason = "RK_EnergyShieldBoost_NoShield".Translate();
                return true;
            }
            if (!HasRequiredPsyfocus(out reason))
                return true;
            return base.GizmoDisabled(out reason);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = parent.pawn;
            if (pawn == null) return;

            CompShieldDeflectEnergy shieldComp = CompShieldDeflectEnergy.GetWornEnergyShield(pawn);
            if (shieldComp == null) return;

            pawn.psychicEntropy.OffsetPsyfocusDirectly(-Props.psyfocusCost);
            shieldComp.ApplyEnergyBoost(pawn, Props.boostHediffDef, Props.boostDurationTicks);
        }

        private bool HasRequiredPsyfocus(out string reason)
        {
            reason = null;
            Pawn pawn = parent.pawn;
            if (pawn == null) return false;

            if (pawn.psychicEntropy?.Psylink == null)
            {
                reason = "RK_EnergyShieldBoost_NoPsylink".Translate();
                return false;
            }

            if (pawn.psychicEntropy.CurrentPsyfocus + 0.0005f < Props.psyfocusCost)
            {
                reason = "AbilityPsycastNoPsyfocus".Translate();
                return false;
            }

            return true;
        }
    }
}
