using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace NewRatkin
{
    public class HediffCompProperties_HawkEyeRange : HediffCompProperties
    {
        public int increaseTermTicks = 600;

        public float increaseFactor = 0.05f;

        public float maxRangeFactor = 1.5f;

        public bool resetOnWeaponChange = true;

        public TraitDef requiredTrait;

        public HediffCompProperties_HawkEyeRange()
        {
            this.compClass = typeof(HediffComp_HawkEyeRange);
        }

        public int IncreaseTermTicks
        {
            get
            {
                return Mathf.Max(1, this.increaseTermTicks);
            }
        }

        public float MaxRangeFactor
        {
            get
            {
                return Mathf.Max(1f, this.maxRangeFactor);
            }
        }
    }

    public class HediffComp_HawkEyeRange : HediffComp
    {
        private const float BaseRangeFactor = 1f;

        private float currentRangeFactor = BaseRangeFactor;

        private int ticksUntilNextIncrease;

        private IntVec3 lastCell = IntVec3.Invalid;

        private int lastPrimaryThingId = -1;

        public HediffCompProperties_HawkEyeRange Props
        {
            get
            {
                return (HediffCompProperties_HawkEyeRange)this.props;
            }
        }

        public float RangeFactor
        {
            get
            {
                return Mathf.Clamp(this.currentRangeFactor, BaseRangeFactor, this.Props.MaxRangeFactor);
            }
        }

        public override string CompLabelInBracketsExtra
        {
            get
            {
                return this.RangeFactor > BaseRangeFactor ? this.RangeFactor.ToStringPercent("F0") : null;
            }
        }

        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || (this.Props.requiredTrait != null && !HasTrait(this.Pawn, this.Props.requiredTrait));
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            this.ResetAndRememberCurrentState();
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            Pawn pawn = this.Pawn;
            if (pawn == null || pawn.Dead)
            {
                return;
            }

            ThingWithComps primary;
            if (!this.TryGetCurrentRangedPrimary(pawn, out primary) || !pawn.Spawned)
            {
                this.ResetAndRememberCurrentState();
                return;
            }

            int primaryThingId = primary.thingIDNumber;
            if (this.Props.resetOnWeaponChange && this.lastPrimaryThingId >= 0 && this.lastPrimaryThingId != primaryThingId)
            {
                this.ResetAndRememberCurrentState();
                return;
            }

            if (this.HasMovedSinceLastCheck(pawn))
            {
                this.ResetAndRememberCurrentState();
                return;
            }

            this.lastCell = pawn.Position;
            this.lastPrimaryThingId = primaryThingId;

            if (this.RangeFactor >= this.Props.MaxRangeFactor || this.Props.increaseFactor <= 0f)
            {
                this.ticksUntilNextIncrease = this.Props.IncreaseTermTicks;
                return;
            }

            this.ticksUntilNextIncrease -= delta;
            while (this.ticksUntilNextIncrease <= 0)
            {
                this.currentRangeFactor = Mathf.Min(this.Props.MaxRangeFactor, this.currentRangeFactor + this.Props.increaseFactor);
                if (this.RangeFactor >= this.Props.MaxRangeFactor)
                {
                    this.ticksUntilNextIncrease = this.Props.IncreaseTermTicks;
                    break;
                }
                this.ticksUntilNextIncrease += this.Props.IncreaseTermTicks;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref this.currentRangeFactor, "hawkEyeRangeFactor", BaseRangeFactor);
            Scribe_Values.Look(ref this.ticksUntilNextIncrease, "hawkEyeTicksUntilNextIncrease", 600);
            Scribe_Values.Look(ref this.lastCell, "hawkEyeLastCell", IntVec3.Invalid);
            Scribe_Values.Look(ref this.lastPrimaryThingId, "hawkEyeLastPrimaryThingId", -1);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.currentRangeFactor = Mathf.Clamp(this.currentRangeFactor, BaseRangeFactor, this.Props.MaxRangeFactor);
                if (this.ticksUntilNextIncrease <= 0)
                {
                    this.ticksUntilNextIncrease = this.Props.IncreaseTermTicks;
                }
            }
        }

        private bool TryGetCurrentRangedPrimary(Pawn pawn, out ThingWithComps primary)
        {
            primary = pawn.equipment?.Primary;
            if (primary == null || !primary.def.IsRangedWeapon)
            {
                return false;
            }

            CompEquippable primaryEq = pawn.equipment.PrimaryEq;
            Verb verb = primaryEq?.PrimaryVerb;
            return verb is Verb_LaunchProjectile && !verb.verbProps.IsMeleeAttack;
        }

        private bool HasMovedSinceLastCheck(Pawn pawn)
        {
            if (pawn.pather != null && pawn.pather.Moving)
            {
                return true;
            }
            return this.lastCell.IsValid && pawn.Position != this.lastCell;
        }

        private void ResetAndRememberCurrentState()
        {
            Pawn pawn = this.Pawn;
            this.currentRangeFactor = BaseRangeFactor;
            this.ticksUntilNextIncrease = this.Props.IncreaseTermTicks;
            this.lastCell = pawn != null && pawn.Spawned ? pawn.Position : IntVec3.Invalid;
            this.lastPrimaryThingId = -1;

            ThingWithComps primary;
            if (pawn != null && this.TryGetCurrentRangedPrimary(pawn, out primary))
            {
                this.lastPrimaryThingId = primary.thingIDNumber;
            }
        }

        private static bool HasTrait(Pawn pawn, TraitDef traitDef)
        {
            return pawn?.story?.traits != null && pawn.story.traits.HasTrait(traitDef);
        }
    }

    public class StatPart_HawkEyeRangeMultiplier : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            float factor;
            if (TryGetRangeFactor(req, out factor) && factor > 1f)
            {
                val *= factor;
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            float factor;
            if (!TryGetRangeFactor(req, out factor) || factor <= 1f)
            {
                return null;
            }

            string label = RatkinTraitDefOf.RK_Trait_HawkEye != null ? RatkinTraitDefOf.RK_Trait_HawkEye.LabelCap.ToString() : "Hawk Eye";
            return label + ": x" + factor.ToStringPercent("F0");
        }

        private static bool TryGetRangeFactor(StatRequest req, out float factor)
        {
            factor = 1f;
            if (!req.HasThing)
            {
                return false;
            }

            ThingWithComps weapon = req.Thing as ThingWithComps;
            if (weapon == null || !weapon.def.IsRangedWeapon)
            {
                return false;
            }

            Pawn pawn = GetPawnFromRequest(req);
            if (pawn == null || pawn.equipment?.Primary != weapon)
            {
                return false;
            }

            HediffComp_HawkEyeRange comp = GetHawkEyeComp(pawn);
            if (comp == null)
            {
                return false;
            }

            factor = comp.RangeFactor;
            return factor > 1f;
        }

        private static Pawn GetPawnFromRequest(StatRequest req)
        {
            if (req.Pawn != null)
            {
                return req.Pawn;
            }

            Pawn pawn = req.Thing as Pawn;
            if (pawn != null)
            {
                return pawn;
            }

            ThingWithComps weapon = req.Thing as ThingWithComps;
            if (weapon == null)
            {
                return null;
            }

            Pawn_EquipmentTracker equipmentTracker = weapon.ParentHolder as Pawn_EquipmentTracker;
            if (equipmentTracker != null)
            {
                return equipmentTracker.pawn;
            }

            if (weapon.holdingOwner != null)
            {
                equipmentTracker = weapon.holdingOwner.Owner as Pawn_EquipmentTracker;
                if (equipmentTracker != null)
                {
                    return equipmentTracker.pawn;
                }

                pawn = weapon.holdingOwner.Owner as Pawn;
                if (pawn != null)
                {
                    return pawn;
                }
            }

            CompEquippable compEquippable = weapon.GetComp<CompEquippable>();
            return compEquippable?.PrimaryVerb?.CasterPawn;
        }

        private static HediffComp_HawkEyeRange GetHawkEyeComp(Pawn pawn)
        {
            List<Hediff> hediffs = pawn?.health?.hediffSet?.hediffs;
            if (hediffs == null || RatkinHediffDefOf.RK_Hediff_HawkEyeRange == null)
            {
                return null;
            }

            for (int i = 0; i < hediffs.Count; i++)
            {
                HediffWithComps hediff = hediffs[i] as HediffWithComps;
                if (hediff != null && hediff.def == RatkinHediffDefOf.RK_Hediff_HawkEyeRange)
                {
                    return hediff.GetComp<HediffComp_HawkEyeRange>();
                }
            }

            return null;
        }
    }

    public class GameComponent_HawkEyeRangeBootstrap : GameComponent
    {
        public GameComponent_HawkEyeRangeBootstrap(Game game)
        {
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            ApplyToExistingTraitPawns();
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            ApplyToExistingTraitPawns();
        }

        private static void ApplyToExistingTraitPawns()
        {
            if (RatkinTraitDefOf.RK_Trait_HawkEye == null || RatkinHediffDefOf.RK_Hediff_HawkEyeRange == null)
            {
                return;
            }

            HashSet<Pawn> seen = new HashSet<Pawn>();
            ApplyToPawns(PawnsFinder.AllMapsWorldAndTemporary_Alive, seen);
            ApplyToPawns(PawnsFinder.AllCaravansAndTravellingTransporters_Alive, seen);
        }

        private static void ApplyToPawns(List<Pawn> pawns, HashSet<Pawn> seen)
        {
            if (pawns == null)
            {
                return;
            }

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (pawn != null && seen.Add(pawn))
                {
                    EnsureSingleHediffForPawn(pawn);
                }
            }
        }

        private static void EnsureSingleHediffForPawn(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null)
            {
                return;
            }

            if (pawn.story?.traits == null || !pawn.story.traits.HasTrait(RatkinTraitDefOf.RK_Trait_HawkEye))
            {
                RemoveAllHawkEyeHediffs(pawn);
                return;
            }

            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            Hediff kept = null;
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = hediffs[i];
                if (hediff.def != RatkinHediffDefOf.RK_Hediff_HawkEyeRange)
                {
                    continue;
                }

                if (kept == null)
                {
                    kept = hediff;
                }
                else
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }

            if (kept == null)
            {
                pawn.health.AddHediff(HediffMaker.MakeHediff(RatkinHediffDefOf.RK_Hediff_HawkEyeRange, pawn));
            }
        }

        private static void RemoveAllHawkEyeHediffs(Pawn pawn)
        {
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = hediffs[i];
                if (hediff.def == RatkinHediffDefOf.RK_Hediff_HawkEyeRange)
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }
        }
    }
}
