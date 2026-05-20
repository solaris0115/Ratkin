using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace NewRatkin
{
    /// <summary>
    /// Prototype Pulse Rifle — verbProps는 Def 공유 참조 유지, 모드별 수치는 Comp에서 읽음 (다수 선택 공격 호환).
    /// </summary>
    public class Verb_PulseRifleShoot : Verb_Shoot
    {
        private Comp_PulseRifleFireMode FireModeComp
        {
            get
            {
                ThingWithComps equipmentSource = EquipmentSource;
                return equipmentSource?.GetComp<Comp_PulseRifleFireMode>();
            }
        }

        public override ThingDef Projectile
        {
            get
            {
                Comp_PulseRifleFireMode comp = FireModeComp;
                if (comp != null)
                {
                    return comp.CurrentProjectile;
                }

                return verbProps.defaultProjectile;
            }
        }

        public override float WarmupTime
        {
            get
            {
                Comp_PulseRifleFireMode comp = FireModeComp;
                if (comp != null)
                {
                    return comp.CurrentWarmupTime;
                }

                return base.WarmupTime;
            }
        }

        public override float EffectiveRange
        {
            get
            {
                Comp_PulseRifleFireMode comp = FireModeComp;
                if (comp == null)
                {
                    return base.EffectiveRange;
                }

                float range = comp.GetAdjustedRange(this, caster);
                ThingWithComps equipmentSource = EquipmentSource;
                float multiplier = equipmentSource != null
                    ? equipmentSource.GetStatValue(StatDefOf.RangedWeapon_RangeMultiplier, true, -1)
                    : 1f;
                return range * multiplier;
            }
        }

        protected override int ShotsPerBurst
        {
            get
            {
                Comp_PulseRifleFireMode comp = FireModeComp;
                if (comp != null)
                {
                    return comp.CurrentBurstShotCount;
                }

                return base.ShotsPerBurst;
            }
        }

        public override void WarmupComplete()
        {
            burstShotsLeft = ShotsPerBurst;
            state = VerbState.Bursting;
            TryCastNextBurstShot();

            Thing target = currentTarget.HasThing ? currentTarget.Thing : null;
            ThingWithComps equipmentSource = EquipmentSource;
            Find.BattleLog.Add(new BattleLogEntry_RangedFire(
                caster,
                target,
                equipmentSource != null ? equipmentSource.def : null,
                Projectile,
                ShotsPerBurst > 1));

            Pawn pawn = target as Pawn;
            if (pawn == null || pawn.Downed || pawn.IsColonyMech || !CasterIsPawn || CasterPawn.skills == null)
            {
                return;
            }

            float xpBase = pawn.HostileTo(caster) ? 170f : 20f;
            CasterPawn.skills.Learn(SkillDefOf.Shooting, xpBase * GetAdjustedFullCycleTimeForXp(), false, false);
        }

        /// <summary>
        /// Verb.TryCastNextBurstShot는 virtual이 아니므로 PulseRifleBurstPatch Prefix에서 호출.
        /// </summary>
        public void PulseTryCastNextBurstShot()
        {
            LocalTargetInfo localTargetInfo = currentTarget;
            if (Available() && TryCastShot())
            {
                if (verbProps.muzzleFlashScale > 0.01f)
                {
                    FleckMaker.Static(caster.Position, caster.Map, FleckDefOf.ShotFlash, verbProps.muzzleFlashScale);
                }

                if (verbProps.soundCast != null)
                {
                    verbProps.soundCast.PlayOneShot(new TargetInfo(caster.Position, caster.MapHeld, false));
                }

                if (verbProps.soundCastTail != null)
                {
                    verbProps.soundCastTail.PlayOneShotOnCamera(caster.Map);
                }

                if (CasterIsPawn)
                {
                    CasterPawn.Notify_UsedVerb(CasterPawn, this);

                    if (CasterPawn.MentalState != null)
                    {
                        CasterPawn.MentalState.Notify_AttackedTarget(localTargetInfo);
                    }

                    if (TerrainDefSource != null)
                    {
                        CasterPawn.meleeVerbs.Notify_UsedTerrainBasedVerb();
                    }

                    if (CasterPawn.health != null)
                    {
                        CasterPawn.health.Notify_UsedVerb(this, localTargetInfo);
                    }

                    if (EquipmentSource != null)
                    {
                        EquipmentSource.Notify_UsedWeapon(CasterPawn);
                    }

                    if (!CasterPawn.Spawned)
                    {
                        Reset();
                        return;
                    }
                }

                if (verbProps.consumeFuelPerShot > 0f)
                {
                    CompRefuelable compRefuelable = caster.TryGetComp<CompRefuelable>();
                    if (compRefuelable != null)
                    {
                        compRefuelable.ConsumeFuel(verbProps.consumeFuelPerShot);
                    }
                }

                burstShotsLeft--;
            }
            else
            {
                burstShotsLeft = 0;
            }

            int ticksBetween = GetTicksBetweenBurstShotsForMode();
            if (burstShotsLeft > 0)
            {
                ticksToNextBurstShot = ticksBetween;
                if (CasterIsPawn && !NonInterruptingSelfCast)
                {
                    CasterPawn.stances.SetStance(new Stance_Cooldown(ticksBetween + 1, currentTarget, this));
                    return;
                }
            }
            else
            {
                state = VerbState.Idle;
                if (CasterIsPawn && !NonInterruptingSelfCast)
                {
                    CasterPawn.stances.SetStance(new Stance_Cooldown(verbProps.AdjustedCooldownTicks(this, CasterPawn), currentTarget, this));
                }

                if (castCompleteCallback != null)
                {
                    castCompleteCallback();
                }

                if (verbProps.consumeFuelPerBurst > 0f)
                {
                    CompRefuelable compRefuelable2 = caster.TryGetComp<CompRefuelable>();
                    if (compRefuelable2 != null)
                    {
                        compRefuelable2.ConsumeFuel(verbProps.consumeFuelPerBurst);
                    }
                }
            }
        }

        private int GetTicksBetweenBurstShotsForMode()
        {
            Comp_PulseRifleFireMode comp = FireModeComp;
            if (comp != null)
            {
                return comp.CurrentTicksBetweenBurstShots;
            }

            return TicksBetweenBurstShots;
        }

        private float GetAdjustedFullCycleTimeForXp()
        {
            Comp_PulseRifleFireMode comp = FireModeComp;
            if (comp == null)
            {
                return verbProps.AdjustedFullCycleTime(this, CasterPawn);
            }

            int ticksBetween = comp.CurrentTicksBetweenBurstShots;
            return WarmupTime
                + verbProps.AdjustedCooldown(this, CasterPawn)
                + ((ShotsPerBurst - 1) * ticksBetween).TicksToSeconds();
        }
    }
}
