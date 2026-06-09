using UnityEngine;
using Verse;
using RimWorld;

namespace NewRatkin
{
    public class CompProperties_ShieldDeflectEnergy : CompProperties_ShieldDeflect
    {
        /// <summary>기본 에너지 풀 최대치(부스트 없을 때).</summary>
        public float energyBase = 60f;

        /// <summary>부스트 시 최대치 배율 가산(0.5 = +50%).</summary>
        public float boostPct = 0.5f;

        /// <summary>충전 간격(틱)마다 회복량.</summary>
        public float energyRechargePerTick = 2f;

        /// <summary>충전 간격(틱).</summary>
        public int rechargeTickInterval = 60;

        /// <summary>피격 후 충전 정지 시간(틱).</summary>
        public int rechargeDelayAfterHitTicks = 300;

        /// <summary>에너지 흡수 시 블록 시도 게이트(ShieldBlockChance)를 스킵.</summary>
        public bool guaranteedBlockAttempt = true;

        public CompProperties_ShieldDeflectEnergy()
        {
            compClass = typeof(CompShieldDeflectEnergy);
        }
    }

    /// <summary>
    /// 에너지 코팅 흡수 + 기존 방패 아머 판정(에너지 고갈 시).
    /// 부스트 Hediff 활성 시 effectiveMax = energyBase × (1 + boostPct).
    /// </summary>
    public class CompShieldDeflectEnergy : CompShieldDeflect
    {
        public new CompProperties_ShieldDeflectEnergy Props => (CompProperties_ShieldDeflectEnergy)props;

        private float energy;

        private int ticksUntilRechargeAllowed;

        public float Energy => energy;

        public static CompShieldDeflectEnergy GetWornEnergyShield(Pawn pawn)
        {
            if (pawn?.apparel == null) return null;
            var worn = pawn.apparel.WornApparel;
            for (int i = 0; i < worn.Count; i++)
            {
                var comp = worn[i].GetComp<CompShieldDeflectEnergy>();
                if (comp != null) return comp;
            }
            return null;
        }

        public float EffectiveMax(Pawn pawn)
        {
            float mult = IsBoostActive(pawn) ? 1f + Props.boostPct : 1f;
            return Props.energyBase * mult;
        }

        public bool IsBoostActive(Pawn pawn)
        {
            if (pawn == null || RatkinHediffDefOf.RK_Hediff_EnergyShieldBoost == null)
                return false;
            return pawn.health.hediffSet.HasHediff(RatkinHediffDefOf.RK_Hediff_EnergyShieldBoost);
        }

        public void ClampEnergy(Pawn pawn)
        {
            energy = Mathf.Clamp(energy, 0f, EffectiveMax(pawn));
        }

        /// <summary>부스트 어빌리티: 에너지·최대치 증폭 + Hediff 부여.</summary>
        public void ApplyEnergyBoost(Pawn pawn, HediffDef boostHediffDef, int durationTicks)
        {
            if (pawn == null) return;

            float boostAmount = Props.energyBase * Props.boostPct;
            energy += boostAmount;
            ClampEnergy(pawn);

            if (boostHediffDef == null) return;

            Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(boostHediffDef);
            if (existing != null)
                pawn.health.RemoveHediff(existing);

            Hediff hediff = HediffMaker.MakeHediff(boostHediffDef, pawn);
            HediffComp_Disappears disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (disappears != null)
                disappears.SetDuration(durationTicks);
            pawn.health.AddHediff(hediff);
            ClampEnergy(pawn);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref energy, "energy", 0f);
            Scribe_Values.Look(ref ticksUntilRechargeAllowed, "ticksUntilRechargeAllowed", 0);
        }

        public override void CompTick()
        {
            base.CompTick();
            Pawn pawn = (parent as Apparel)?.Wearer;
            if (pawn == null)
            {
                energy = 0f;
                return;
            }

            ClampEnergy(pawn);

            if (ticksUntilRechargeAllowed > 0)
                ticksUntilRechargeAllowed--;

            if (ticksUntilRechargeAllowed > 0) return;
            if (!parent.IsHashIntervalTick(Props.rechargeTickInterval)) return;

            float max = EffectiveMax(pawn);
            if (energy < max)
                energy = Mathf.Min(energy + Props.energyRechargePerTick, max);
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (_processingDamage) return;

            Pawn pawn = (parent as Apparel)?.Wearer;
            if (pawn == null) return;

            var shield = parent as ApparelShieldTowerSecond;
            if (shield == null) return;

            if (pawn.Dead || pawn.Downed) return;
            if (!pawn.Drafted) return;
            if (!ApparelShieldTowerSecond.PawnCanDeflectWithShield(pawn)) return;
            if (dinfo.Def == null) return;
            if (dinfo.Def.ignoreShields || dinfo.Def == DamageDefOf.EMP) return;

            if (!ApparelShieldTowerSecond.IsAngleWithinDeflectRange(shield, pawn, dinfo)) return;

            ClampEnergy(pawn);

            if (energy > 0f)
            {
                if (!Props.guaranteedBlockAttempt)
                {
                    float blockChance = pawn.GetStatValue(RatkinStatDefOf.RK_Stat_ShieldBlockChance);
                    if (Rand.Value >= blockChance) return;
                }

                energy -= dinfo.Amount;
                if (energy < 0f) energy = 0f;
                ticksUntilRechargeAllowed = Props.rechargeDelayAfterHitTicks;
                ClampEnergy(pawn);

                absorbed = true;
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "ShieldBlock".Translate(), 1.9f);
                EffecterDefOf.Deflect_Metal.Spawn().Trigger(pawn, dinfo.Instigator ?? pawn);
                return;
            }

            base.PostPreApplyDamage(ref dinfo, out absorbed);
        }
    }
}
