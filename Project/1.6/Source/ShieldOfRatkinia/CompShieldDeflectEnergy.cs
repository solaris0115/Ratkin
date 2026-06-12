using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace NewRatkin
{
    public class CompProperties_ShieldDeflectEnergy : CompProperties_ShieldDeflect
    {
        /// <summary>부스트 시 최대치 배율 가산(0.5 = +50%). 어빌리티 전용.</summary>
        public float boostPct = 0.5f;

        /// <summary>바닐라 CompShield와 동일 — 피해량당 에너지 손실 계수.</summary>
        public float energyLossPerDamage = 0.033f;

        /// <summary>에너지 흡수 시 블록 시도 게이트(ShieldBlockChance)를 스킵.</summary>
        public bool guaranteedBlockAttempt = true;

        /// <summary>바닐라 CompProperties_Shield — 고갈 후 재가동 대기 틱(3200 ≈ 53초).</summary>
        public int startingTicksToReset = 3200;

        /// <summary>바닐라 CompProperties_Shield — 재가동 직후 초기 에너지(표시 20).</summary>
        public float energyOnReset = 0.2f;

        public CompProperties_ShieldDeflectEnergy()
        {
            compClass = typeof(CompShieldDeflectEnergy);
        }
    }

    /// <summary>
    /// 에너지 코팅 흡수 + 기존 방패 아머 판정(에너지 고갈 시).
    /// 에너지 풀·충전·표시는 바닐라 CompShield(쉴드벨트)와 동일 스탯/틱 로직.
    /// 부스트 Hediff 활성 시 effectiveMax = EnergyShieldEnergyMax × (1 + boostPct).
    /// </summary>
    public class CompShieldDeflectEnergy : CompShieldDeflect
    {
        public new CompProperties_ShieldDeflectEnergy Props => (CompProperties_ShieldDeflectEnergy)props;

        private float energy;

        private int ticksToReset = -1;

        public float Energy => energy;

        public ShieldState ShieldState =>
            ticksToReset <= 0 ? ShieldState.Active : ShieldState.Resetting;

        private Pawn PawnOwner => (parent as Apparel)?.Wearer;

        /// <summary>바닐라 EnergyMax — EnergyShieldEnergyMax 스탯.</summary>
        public float EnergyMax =>
            parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true, -1);

        /// <summary>바닐라 EnergyGainPerTick — EnergyShieldRechargeRate / 60.</summary>
        private float EnergyGainPerTick =>
            parent.GetStatValue(StatDefOf.EnergyShieldRechargeRate, true, -1) / 60f;

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

        /// <summary>EnergyShieldEnergyMax 스탯의 품질 배율(쉴드벨트 StatPart_Quality와 동일).</summary>
        public static float GetShieldEnergyQualityFactor(Thing apparel)
        {
            if (apparel?.def == null) return 1f;
            float abstractVal = apparel.def.GetStatValueAbstract(StatDefOf.EnergyShieldEnergyMax);
            if (abstractVal <= 0f) return 1f;
            return apparel.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true, -1) / abstractVal;
        }

        public float EffectiveMax(Pawn pawn)
        {
            float mult = IsBoostActive(pawn) ? 1f + Props.boostPct : 1f;
            return EnergyMax * mult;
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

        /// <summary>부스트 어빌리티: 최대치 증폭(Hediff) 후 fillDisplayAmount(표시 단위)만큼 충전.</summary>
        public void ApplyEnergyBoost(Pawn pawn, HediffDef boostHediffDef, int durationTicks, float fillDisplayAmount)
        {
            if (pawn == null) return;

            if (boostHediffDef != null)
            {
                Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(boostHediffDef);
                if (existing != null)
                    pawn.health.RemoveHediff(existing);

                Hediff hediff = HediffMaker.MakeHediff(boostHediffDef, pawn);
                HediffComp_Disappears disappears = hediff.TryGetComp<HediffComp_Disappears>();
                if (disappears != null)
                    disappears.SetDuration(durationTicks);
                pawn.health.AddHediff(hediff);
            }

            if (fillDisplayAmount > 0f)
            {
                energy += fillDisplayAmount / 100f;
                ClampEnergy(pawn);
                ticksToReset = -1;
            }
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
                yield return gizmo;

            Pawn pawn = PawnOwner;
            if (pawn == null) yield break;
            if (pawn.Faction != Faction.OfPlayer) yield break;
            if (Find.Selector.SingleSelectedThing != pawn) yield break;

            yield return new Gizmo_EnergyShieldDeflectStatus { shield = this };
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref energy, "energy", 0f);
            Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
        }

        public override void CompTick()
        {
            base.CompTick();
            Pawn pawn = PawnOwner;
            if (pawn == null)
            {
                energy = 0f;
                return;
            }

            ClampEnergy(pawn);

            if (ShieldState == ShieldState.Resetting)
            {
                ticksToReset--;
                if (ticksToReset <= 0)
                    Reset(pawn);
                return;
            }

            energy += EnergyGainPerTick;
            float max = EffectiveMax(pawn);
            if (energy > max)
                energy = max;
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (_processingDamage) return;

            Pawn pawn = PawnOwner;
            if (pawn == null) return;

            var shield = parent as ApparelShieldTowerSecond;
            if (shield == null) return;

            if (pawn.Dead || pawn.Downed) return;
            if (dinfo.Def == DamageDefOf.EMP)
            {
                energy = 0f;
                Break(pawn);
                return;
            }
            if (!pawn.Drafted) return;
            if (!ApparelShieldTowerSecond.PawnCanDeflectWithShield(pawn)) return;
            if (dinfo.Def == null) return;
            if (dinfo.Def.ignoreShields) return;

            if (!ApparelShieldTowerSecond.IsAngleWithinDeflectRange(shield, pawn, dinfo)) return;

            ClampEnergy(pawn);

            if (energy > 0f && ShieldState == ShieldState.Active)
            {
                if (!Props.guaranteedBlockAttempt)
                {
                    float blockChance = pawn.GetStatValue(RatkinStatDefOf.RK_Stat_ShieldBlockChance);
                    if (Rand.Value >= blockChance) return;
                }

                energy -= dinfo.Amount * Props.energyLossPerDamage;
                if (energy < 0f)
                    Break(pawn);
                else
                    PlayEnergyAbsorbFeedback(pawn, dinfo);

                absorbed = true;
                return;
            }

            base.PostPreApplyDamage(ref dinfo, out absorbed);
        }

        /// <summary>바닐라 CompShield.Break — 고갈 시 재가동 대기 시작.</summary>
        private void Break(Pawn pawn)
        {
            if (parent.Spawned && pawn.Spawned)
            {
                float scale = Mathf.Lerp(1.2f, 1.55f, Mathf.Max(0f, energy));
                EffecterDefOf.Shield_Break.SpawnAttached(parent, parent.MapHeld, scale);
                FleckMaker.Static(pawn.TrueCenter(), pawn.Map, FleckDefOf.ExplosionFlash, 12f);
                for (int i = 0; i < 6; i++)
                {
                    FleckMaker.ThrowDustPuff(
                        pawn.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f),
                        pawn.Map, Rand.Range(0.8f, 1.2f));
                }
            }

            energy = 0f;
            ticksToReset = Props.startingTicksToReset;
        }

        /// <summary>바닐라 CompShield.Reset — 대기 종료 후 소량 충전 재개.</summary>
        private void Reset(Pawn pawn)
        {
            if (pawn.Spawned)
            {
                SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
                FleckMaker.ThrowLightningGlow(pawn.TrueCenter(), pawn.Map, 3f);
            }

            ticksToReset = -1;
            energy = Props.energyOnReset;
            ClampEnergy(pawn);
        }

        /// <summary>바닐라 CompShield.AbsorbedDamage — 텍스트 없음, 쉴드벨트 흡수음·이펙트.</summary>
        private static void PlayEnergyAbsorbFeedback(Pawn pawn, DamageInfo dinfo)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map));
            Vector3 impactVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = pawn.TrueCenter() + impactVect.RotatedBy(180f) * 0.5f;
            float flashSize = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
            FleckMaker.Static(loc, pawn.Map, FleckDefOf.ExplosionFlash, flashSize);
            int puffCount = (int)flashSize;
            for (int i = 0; i < puffCount; i++)
                FleckMaker.ThrowDustPuff(loc, pawn.Map, Rand.Range(0.8f, 1.2f));
        }
    }
}

