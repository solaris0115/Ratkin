using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// 산탄총 동시 발사 Verb - 여러 산탄을 한 틱에 동시 발사, 거리별 분산 적용
    /// </summary>
    public class Verb_ShotgunBlast : Verb_Shoot
    {
        private VerbProperties_ShotgunBlast ShotgunProps => verbProps as VerbProperties_ShotgunBlast;

        protected override int ShotsPerBurst => 1;

        protected override bool TryCastShot()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }

            ThingDef projectileDef = Projectile;
            if (projectileDef == null)
            {
                return false;
            }

            ShootLine shootLine;
            if (!TryFindShootLineFromTo(caster.Position, currentTarget, out shootLine, false) && verbProps.stopBurstWithoutLos)
            {
                return false;
            }

            if (EquipmentSource != null)
            {
                CompChangeableProjectile comp = EquipmentSource.GetComp<CompChangeableProjectile>();
                if (comp != null)
                {
                    comp.Notify_ProjectileLaunched();
                }
                CompApparelVerbOwner_Charged compCharged = EquipmentSource.GetComp<CompApparelVerbOwner_Charged>();
                if (compCharged != null)
                {
                    compCharged.UsedOnce();
                }
            }

            lastShotTick = Find.TickManager.TicksGame;

            Thing launcher = caster;
            Thing equipment = EquipmentSource;
            CompMannable compMannable = caster.TryGetComp<CompMannable>();
            if (compMannable?.ManningPawn != null)
            {
                launcher = compMannable.ManningPawn;
                equipment = caster;
            }

            Vector3 drawPos = caster.DrawPos;
            VerbProperties_ShotgunBlast props = ShotgunProps;
            int pelletCount = props?.pelletCount ?? 4;
            float spreadAngleDeg = props?.spreadAngleDeg ?? 5f;

            Vector3 origin = drawPos;
            Vector3 targetCenter = currentTarget.Cell.ToVector3Shifted();
            Vector3 direction = (targetCenter - origin).normalized;
            float distance = (targetCenter - origin).magnitude;
            if (distance < 0.01f)
            {
                distance = EffectiveRange;
            }

            for (int i = 0; i < pelletCount; i++)
            {
                float angleOffset = Rand.Range(-spreadAngleDeg / 2f, spreadAngleDeg / 2f);
                Vector3 rotated = Quaternion.Euler(0f, angleOffset, 0f) * direction;
                Vector3 pelletDestVec = origin + rotated * distance;
                IntVec3 pelletDest = pelletDestVec.ToIntVec3();

                ShootLine pelletShootLine = new ShootLine(shootLine.Source, pelletDest);
                ShotReport shotReport = ShotReport.HitReportFor(caster, this, currentTarget);
                Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
                ThingDef targetCoverDef = randomCoverToMissInto?.def;

                Projectile projectile = (Projectile)GenSpawn.Spawn(projectileDef, shootLine.Source, caster.Map, WipeMode.Vanish);
                ApplyCompUniqueWeapon(projectile, equipment);

                ProjectileHitFlags hitFlags;
                LocalTargetInfo launchTarget;

                if (verbProps.canGoWild && !Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
                {
                    pelletShootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget,
                        projectileDef.projectile?.flyOverhead ?? false, caster.Map);
                    launchTarget = pelletShootLine.Dest;
                    hitFlags = ProjectileHitFlags.NonTargetWorld;
                    if (Rand.Chance(0.5f) && canHitNonTargetPawnsNow)
                    {
                        hitFlags |= ProjectileHitFlags.NonTargetPawns;
                    }
                }
                else if (currentTarget.Thing != null && currentTarget.Thing.def.CanBenefitFromCover && randomCoverToMissInto != null && !Rand.Chance(shotReport.PassCoverChance))
                {
                    launchTarget = randomCoverToMissInto;
                    hitFlags = ProjectileHitFlags.NonTargetWorld;
                    if (canHitNonTargetPawnsNow)
                    {
                        hitFlags |= ProjectileHitFlags.NonTargetPawns;
                    }
                }
                else
                {
                    hitFlags = ProjectileHitFlags.IntendedTarget;
                    if (canHitNonTargetPawnsNow)
                    {
                        hitFlags |= ProjectileHitFlags.NonTargetPawns;
                    }
                    if (!currentTarget.HasThing || currentTarget.Thing.def.Fillage == FillCategory.Full)
                    {
                        hitFlags |= ProjectileHitFlags.NonTargetWorld;
                    }
                    launchTarget = pelletDest;
                }

                projectile.Launch(launcher, drawPos, launchTarget, currentTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            }

            if (CasterIsPawn)
            {
                CasterPawn.records.Increment(RecordDefOf.ShotsFired);
            }

            return true;
        }

        private void ApplyCompUniqueWeapon(Projectile projectile, Thing equipment)
        {
            if (equipment == null)
            {
                return;
            }

            CompUniqueWeapon compUniqueWeapon = equipment.TryGetComp<CompUniqueWeapon>();
            if (compUniqueWeapon == null)
            {
                return;
            }

            foreach (WeaponTraitDef weaponTraitDef in compUniqueWeapon.TraitsListForReading)
            {
                if (weaponTraitDef.damageDefOverride != null)
                {
                    projectile.damageDefOverride = weaponTraitDef.damageDefOverride;
                }
                if (!weaponTraitDef.extraDamages.NullOrEmpty())
                {
                    if (projectile.extraDamages == null)
                    {
                        projectile.extraDamages = new List<ExtraDamage>();
                    }
                    projectile.extraDamages.AddRange(weaponTraitDef.extraDamages);
                }
            }
        }
    }
}
