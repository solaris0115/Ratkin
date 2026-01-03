using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace NewRatkin
{
    [StaticConstructorOnStartup]
    public class CompStaminaShield : ThingComp
    {
        protected float energy;

        protected int ticksToReset = -1;

        protected int lastKeepDisplayTick = -9999;

        private Vector3 impactAngleVect;

        private int lastAbsorbDamageTick = -9999;

        private const float MaxDamagedJitterDist = 0.05f;

        private const int JitterDurationTicks = 8;

        private int KeepDisplayingTicks = 1000;

        private float ApparelScorePerEnergyMax = 0.25f;

        private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

        public CompProperties_StaminaShield Props
        {
            get
            {
                return (CompProperties_StaminaShield)this.props;
            }
        }

        private float EnergyMax
        {
            get
            {
                return this.parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true, -1);
            }
        }

        private float EnergyGainPerTick
        {
            get
            {
                return this.parent.GetStatValue(StatDefOf.EnergyShieldRechargeRate, true, -1) / 60f;
            }
        }

        public float Energy
        {
            get
            {
                return this.energy;
            }
        }

        public ShieldState ShieldState
        {
            get
            {
                Pawn pawn = this.parent as Pawn;
                if (pawn != null && (pawn.IsCharging() || pawn.IsSelfShutdown()))
                {
                    return ShieldState.Disabled;
                }
                CompCanBeDormant comp = this.parent.GetComp<CompCanBeDormant>();
                if (comp != null && !comp.Awake)
                {
                    return ShieldState.Disabled;
                }
                if (this.ticksToReset <= 0)
                {
                    return ShieldState.Active;
                }
                return ShieldState.Resetting;
            }
        }

        protected bool ShouldDisplay
        {
            get
            {
                Pawn pawnOwner = this.PawnOwner;
                return pawnOwner.Spawned && !pawnOwner.Dead && !pawnOwner.Downed && (pawnOwner.InAggroMentalState || pawnOwner.Drafted || (pawnOwner.Faction.HostileTo(Faction.OfPlayer) && !pawnOwner.IsPrisoner) || Find.TickManager.TicksGame < this.lastKeepDisplayTick + this.KeepDisplayingTicks || (ModsConfig.BiotechActive && pawnOwner.IsColonyMech && Find.Selector.SingleSelectedThing == pawnOwner));
            }
        }

        protected Pawn PawnOwner
        {
            get
            {
                Apparel apparel = this.parent as Apparel;
                if (apparel != null)
                {
                    return apparel.Wearer;
                }
                Pawn pawn = this.parent as Pawn;
                if (pawn != null)
                {
                    return pawn;
                }
                return null;
            }
        }

        public bool IsApparel
        {
            get
            {
                return this.parent is Apparel;
            }
        }

        private bool IsBuiltIn
        {
            get
            {
                return !this.IsApparel;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.energy, "energy", 0f, false);
            Scribe_Values.Look<int>(ref this.ticksToReset, "ticksToReset", -1, false);
            Scribe_Values.Look<int>(ref this.lastKeepDisplayTick, "lastKeepDisplayTick", 0, false);
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
            {
                yield return gizmo;
            }
            IEnumerator<Gizmo> enumerator = null;
            if (this.IsApparel)
            {
                foreach (Gizmo gizmo2 in this.GetGizmos())
                {
                    yield return gizmo2;
                }
                enumerator = null;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Break",
                    action = new Action(this.Break)
                };
                if (this.ticksToReset > 0)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Clear reset",
                        action = delegate()
                        {
                            this.ticksToReset = 0;
                        }
                    };
                }
            }
            yield break;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            IEnumerator<Gizmo> enumerator = null;
            if (this.IsBuiltIn)
            {
                foreach (Gizmo gizmo2 in this.GetGizmos())
                {
                    yield return gizmo2;
                }
                enumerator = null;
            }
            yield break;
        }

        private IEnumerable<Gizmo> GetGizmos()
        {
            if (this.PawnOwner.Faction != Faction.OfPlayer)
            {
                Pawn pawn = this.parent as Pawn;
                if (pawn == null || !pawn.RaceProps.IsMechanoid)
                {
                    goto IL_82;
                }
            }
            if (Find.Selector.SingleSelectedThing == this.PawnOwner)
            {
                yield return new Gizmo_StaminaShieldStatus
                {
                    shield = this
                };
            }
            IL_82:
            yield break;
        }

        public override float CompGetSpecialApparelScoreOffset()
        {
            return this.EnergyMax * this.ApparelScorePerEnergyMax;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.PawnOwner == null)
            {
                this.energy = 0f;
                return;
            }
            if (this.ShieldState == ShieldState.Resetting)
            {
                this.ticksToReset--;
                if (this.ticksToReset <= 0)
                {
                    this.Reset();
                    return;
                }
            }
            else if (this.ShieldState == ShieldState.Active)
            {
                this.energy += this.EnergyGainPerTick;
                if (this.energy > this.EnergyMax)
                {
                    this.energy = this.EnergyMax;
                }
            }
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (this.ShieldState != ShieldState.Active || this.PawnOwner == null)
            {
                return;
            }
            if (dinfo.Def == DamageDefOf.EMP)
            {
                this.energy = 0f;
                this.Break();
                return;
            }
            if (dinfo.Def.ignoreShields)
            {
                return;
            }
            if (dinfo.Def.isRanged || dinfo.Def.isExplosive)
            {
                this.energy -= dinfo.Amount * this.Props.energyLossPerDamage;
                if (this.energy < 0f)
                {
                    this.Break();
                }
                else
                {
                    this.AbsorbedDamage(dinfo);
                }
                absorbed = true;
                return;
            }
        }

        public void KeepDisplaying()
        {
            this.lastKeepDisplayTick = Find.TickManager.TicksGame;
        }

        private void AbsorbedDamage(DamageInfo dinfo)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(this.PawnOwner.Position, this.PawnOwner.Map, false));
            this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = this.PawnOwner.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f;
            float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
            FleckMaker.Static(loc, this.PawnOwner.Map, FleckDefOf.ExplosionFlash, num);
            int num2 = (int)num;
            for (int i = 0; i < num2; i++)
            {
                FleckMaker.ThrowDustPuff(loc, this.PawnOwner.Map, Rand.Range(0.8f, 1.2f));
            }
            this.lastAbsorbDamageTick = Find.TickManager.TicksGame;
            this.KeepDisplaying();
        }

        private void Break()
        {
            if (this.parent.Spawned)
            {
                float scale = Mathf.Lerp(this.Props.minDrawSize, this.Props.maxDrawSize, this.energy);
                EffecterDefOf.Shield_Break.SpawnAttached(this.parent, this.parent.MapHeld, scale);
                FleckMaker.Static(this.PawnOwner.TrueCenter(), this.PawnOwner.Map, FleckDefOf.ExplosionFlash, 12f);
                for (int i = 0; i < 6; i++)
                {
                    FleckMaker.ThrowDustPuff(this.PawnOwner.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), this.PawnOwner.Map, Rand.Range(0.8f, 1.2f));
                }
            }
            this.energy = 0f;
            this.ticksToReset = this.Props.startingTicksToReset;
        }

        private void Reset()
        {
            if (this.PawnOwner.Spawned)
            {
                SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(this.PawnOwner.Position, this.PawnOwner.Map, false));
                FleckMaker.ThrowLightningGlow(this.PawnOwner.TrueCenter(), this.PawnOwner.Map, 3f);
            }
            this.ticksToReset = -1;
            this.energy = this.Props.energyOnReset;
        }

        public override void CompDrawWornExtras()
        {
            base.CompDrawWornExtras();
            if (this.IsApparel)
            {
                this.Draw();
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (this.IsBuiltIn)
            {
                this.Draw();
            }
        }

        private void Draw()
        {
            if (this.ShieldState == ShieldState.Active && this.ShouldDisplay)
            {
                float num = Mathf.Lerp(this.Props.minDrawSize, this.Props.maxDrawSize, this.energy);
                Vector3 vector = this.PawnOwner.Drawer.DrawPos;
                vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int num2 = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    float num3 = (float)(8 - num2) / 8f * 0.05f;
                    vector += this.impactAngleVect * num3;
                    num -= num3;
                }
                float angle = (float)Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, CompStaminaShield.BubbleMat, 0);
            }
        }

        public override bool CompAllowVerbCast(Verb verb)
        {
            return !this.Props.blocksRangedWeapons || !(verb is Verb_LaunchProjectile);
        }
    }
}

