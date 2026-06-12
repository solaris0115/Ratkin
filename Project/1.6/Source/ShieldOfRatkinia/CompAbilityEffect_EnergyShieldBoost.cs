using RimWorld;
using Verse;

namespace NewRatkin
{
    public class CompProperties_EnergyShieldBoost : CompProperties_AbilityEffect
    {
        public HediffDef boostHediffDef;

        public int boostDurationTicks = 3600;

        /// <summary>즉시 충전(표시 단위) = 신경열 한계 × 이 배율 × 방패 품질 계수.</summary>
        public float boostFillNeuralHeatMult = 2f;

        public CompProperties_EnergyShieldBoost()
        {
            compClass = typeof(CompAbilityEffect_EnergyShieldBoost);
        }
    }

    public class CompAbilityEffect_EnergyShieldBoost : CompAbilityEffect
    {
        private new CompProperties_EnergyShieldBoost Props => (CompProperties_EnergyShieldBoost)props;

        private float PsyfocusCost => parent.def.PsyfocusCost;

        private float EntropyGain => parent.def.EntropyGain;

        public override bool CanCast
        {
            get
            {
                if (!HasRequiredPsycastResources(out _))
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
            if (!HasRequiredPsycastResources(out reason))
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

            if (EntropyGain > 0f)
                pawn.psychicEntropy.TryAddEntropy(EntropyGain, null, true, false);

            if (PsyfocusCost > 0f)
                pawn.psychicEntropy.OffsetPsyfocusDirectly(-PsyfocusCost);

            float qualityFactor = CompShieldDeflectEnergy.GetShieldEnergyQualityFactor(shieldComp.parent);
            float fillDisplay = pawn.psychicEntropy.MaxEntropy * Props.boostFillNeuralHeatMult * qualityFactor;
            shieldComp.ApplyEnergyBoost(pawn, Props.boostHediffDef, Props.boostDurationTicks, fillDisplay);
        }

        private bool HasRequiredPsycastResources(out string reason)
        {
            reason = null;
            Pawn pawn = parent.pawn;
            if (pawn == null) return false;

            if (pawn.psychicEntropy?.Psylink == null)
            {
                reason = "RK_EnergyShieldBoost_NoPsylink".Translate();
                return false;
            }

            if (PsyfocusCost > 0f && pawn.psychicEntropy.CurrentPsyfocus + 0.0005f < PsyfocusCost)
            {
                reason = "AbilityPsycastNoPsyfocus".Translate();
                return false;
            }

            if (EntropyGain > 0f && pawn.psychicEntropy.WouldOverflowEntropy(EntropyGain))
            {
                reason = "CommandPsycastWouldExceedEntropy".Translate();
                return false;
            }

            return true;
        }
    }
}
