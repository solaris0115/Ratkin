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
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector3.forward;
            }

            // 항상 최대사거리까지 비행 (가까운 대상 조준해도 탄환은 최대사거리까지 날아감)
            float distance = EffectiveRange;

            ShotReport shotReport = ShotReport.HitReportFor(caster, this, currentTarget);
            Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
            ThingDef targetCoverDef = randomCoverToMissInto?.def;
            float aimChance = shotReport.AimOnTargetChance_IgnoringPosture;
            float nonTargetChance = Mathf.Clamp01(props?.nonTargetHitChance ?? 1f);

            for (int i = 0; i < pelletCount; i++)
            {
                // 발당 명중 판정: 각 탄환이 목표에 히트 가능한지
                bool pelletHitsTarget = !verbProps.canGoWild || Rand.Chance(aimChance);
                bool pelletHitsCover = pelletHitsTarget
                    && currentTarget.Thing != null && currentTarget.Thing.def.CanBenefitFromCover
                    && randomCoverToMissInto != null && !Rand.Chance(shotReport.PassCoverChance);

                Vector3 rotated;
                if (pelletHitsCover)
                {
                    Vector3 toCover = randomCoverToMissInto.Position.ToVector3Shifted() - origin;
                    rotated = (toCover.sqrMagnitude < 0.0001f ? direction : toCover.normalized);
                }
                else if (pelletHitsTarget)
                {
                    // 적중: 목표 통과 방향으로 최대사거리까지 (작은 분산으로 시각적 구분)
                    float smallAngle = pelletCount <= 1 ? 0f : -0.5f + 1f * (float)i / Mathf.Max(1, pelletCount - 1);
                    rotated = Quaternion.Euler(0f, smallAngle, 0f) * direction;
                }
                else
                {
                    // 빗나감: 분산 각도로 목표 이탈
                    float angleOffset = pelletCount <= 1 ? 0f
                        : -spreadAngleDeg / 2f + spreadAngleDeg * (float)i / (pelletCount - 1);
                    rotated = Quaternion.Euler(0f, angleOffset, 0f) * direction;
                }

                Vector3 pelletDestVec = origin + rotated * distance;
                IntVec3 pelletDest = pelletDestVec.ToIntVec3();

                Projectile projectile = (Projectile)GenSpawn.Spawn(projectileDef, shootLine.Source, caster.Map, WipeMode.Vanish);
                ApplyCompUniqueWeapon(projectile, equipment);

                ProjectileHitFlags hitFlags;
                LocalTargetInfo launchTarget;

                if (pelletHitsCover)
                {
                    launchTarget = randomCoverToMissInto;
                    hitFlags = ProjectileHitFlags.NonTargetWorld;
                }
                else if (!pelletHitsTarget)
                {
                    launchTarget = pelletDest;
                    hitFlags = ProjectileHitFlags.NonTargetWorld;
                }
                else
                {
                    launchTarget = pelletDest;
                    hitFlags = ProjectileHitFlags.IntendedTarget;
                    if (!currentTarget.HasThing || currentTarget.Thing.def.Fillage == FillCategory.Full)
                    {
                        hitFlags |= ProjectileHitFlags.NonTargetWorld;
                    }
                }

                // 비목표(경로상 다른 대상) 적중 허용 확률
                if (canHitNonTargetPawnsNow && Rand.Chance(nonTargetChance))
                {
                    hitFlags |= ProjectileHitFlags.NonTargetPawns;
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
