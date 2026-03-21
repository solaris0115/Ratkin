using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NewRatkin
{
    /// <summary>피해 대상 정보: LOS 중첩 수 (가리는 경로 수).</summary>
    public struct SectorShotTargetInfo
    {
        public int overlapCount;
    }
    /// <summary>
    /// 부채꼴(Sector) 즉시 AOE 피해 Verb. 타겟에서 사수 방향으로 vertexOffset칸 당긴 위치를 부채꼴 중심으로 사용 (칸 단위 이동).
    /// </summary>
    public class Verb_SectorShot : Verb_LaunchProjectile
    {
        private VerbProperties_SectorShot SP => verbProps as VerbProperties_SectorShot;

        private static readonly FieldInfo _lastInterceptAngle = typeof(CompProjectileInterceptor).GetField("lastInterceptAngle", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _lastInterceptTicks = typeof(CompProjectileInterceptor).GetField("lastInterceptTicks", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _drawInterceptCone = typeof(CompProjectileInterceptor).GetField("drawInterceptCone", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// 투사체 없이 즉시 피해 적용하므로 Verb_LaunchProjectile.Available()의
        /// Projectile != null 체크를 우회. Verb.Available() 로직만 수행.
        /// </summary>
        public override bool Available()
        {
            if (verbProps.consumeFuelPerShot > 0f)
            {
                CompRefuelable comp = caster.TryGetComp<CompRefuelable>();
                if (comp != null && comp.Fuel < verbProps.consumeFuelPerShot)
                    return false;
            }

            ThingWithComps equipmentSource = EquipmentSource;
            CompApparelVerbOwner apparelVerbOwner = equipmentSource?.GetComp<CompApparelVerbOwner>();
            if (apparelVerbOwner != null && !apparelVerbOwner.CanBeUsed(out _))
                return false;

            if (CasterIsPawn && EquipmentSource != null && EquipmentUtility.RolePreventsFromUsing(CasterPawn, EquipmentSource, out _))
                return false;

            if (CasterIsPawn)
            {
                Pawn casterPawn = CasterPawn;
                if (casterPawn.Faction != Faction.OfPlayer && !verbProps.ai_ProjectileLaunchingIgnoresMeleeThreats
                    && casterPawn.mindState.MeleeThreatStillThreat
                    && casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>부채꼴이므로 기본 원형 하이라이트 비활성화.</summary>
        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return 0f;
        }

        /// <summary>조준 시 사거리 링 + 부채꼴 착탄 범위 하이라이트.</summary>
        public override void DrawHighlight(LocalTargetInfo target)
        {
            verbProps.DrawRadiusRing(caster.Position, this);

            if (target.IsValid)
            {
                GenDraw.DrawTargetHighlight(target);
                List<IntVec3> cells = GetSectorCells(target.Cell);
                if (cells != null && cells.Count > 0)
                    GenDraw.DrawFieldEdges(cells, new Color(1f, 0.5f, 0.3f, 0.35f));
            }
        }

        /// <summary>타겟에서 사수 방향으로 vertexOffset칸 당긴 꼭지점을 부채꼴 중심으로 사용. 칸 단위 이동으로 좌표 오차 없음.</summary>
        private List<IntVec3> GetSectorCells(IntVec3 targetCell)
        {
            VerbProperties_SectorShot sp = SP;
            if (sp == null) return null;

            Map map = caster.Map;
            if (map == null) return null;

            IntVec3 vertexCell = GetVertexCell(targetCell, sp.vertexOffset);
            if (!vertexCell.InBounds(map))
                return null;

            return TeleUtils.circularSectorCellsStartedTarget(
                caster.Position,
                map,
                vertexCell,
                sp.sectorRadius,
                sp.sectorAngle,
                true).ToList();
        }

        /// <summary>타겟에서 사수 방향으로 offsetCells칸 당긴 셀. 칸 단위로 이동하여 ToIntVec3 버림 오차 없음.</summary>
        private IntVec3 GetVertexCell(IntVec3 targetCell, int offsetCells)
        {
            if (offsetCells <= 0) return targetCell;

            IntVec3[] adj = { IntVec3.North, IntVec3.South, IntVec3.East, IntVec3.West,
                IntVec3.NorthEast, IntVec3.NorthWest, IntVec3.SouthEast, IntVec3.SouthWest };

            IntVec3 current = targetCell;
            for (int i = 0; i < offsetCells; i++)
            {
                IntVec3 best = current;
                int bestDistSq = (caster.Position - current).LengthHorizontalSquared;
                foreach (IntVec3 d in adj)
                {
                    IntVec3 next = current + d;
                    int distSq = (caster.Position - next).LengthHorizontalSquared;
                    if (distSq < bestDistSq)
                    {
                        bestDistSq = distSq;
                        best = next;
                    }
                }
                if (best == current) break;
                current = best;
            }
            return current;
        }

        protected override bool TryCastShot()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
                return false;

            VerbProperties_SectorShot sp = SP;
            if (sp == null)
                return false;

            ShootLine shootLine;
            if (!TryFindShootLineFromTo(caster.Position, currentTarget, out shootLine, false))
                return false;

            List<IntVec3> sectorCells = GetSectorCells(currentTarget.Cell);
            if (sectorCells == null || sectorCells.Count == 0)
                return false;

            Map map = caster.Map;
            IntVec3 targetCell = currentTarget.Cell;
            IntVec3 vertexCell = GetVertexCell(targetCell, sp.vertexOffset);

            List<IntVec3> shieldedCells;
            HashSet<CompProjectileInterceptor> hitShields;
            List<ShieldZoneInfo> shieldZones;
            FilterShieldedCells(map, sectorCells, vertexCell, out shieldedCells, out hitShields, out shieldZones);

            Dictionary<Pawn, SectorShotTargetInfo> damageTargets = CollectDamageTargetsWithOverlap(map, sectorCells, shieldZones);

            Thing weapon = EquipmentSource;
            float damageMultiplier = weapon != null ? weapon.GetStatValue(StatDefOf.RangedWeapon_DamageMultiplier, true, -1) : 1f;
            int baseDamage = GenMath.RoundRandom(sp.sectorDamageAmount * damageMultiplier);
            DamageDef damageDef = sp.sectorDamageDef ?? DamageDefOf.Bullet;

            foreach (var kv in damageTargets)
            {
                Pawn pawn = kv.Key;
                SectorShotTargetInfo info = kv.Value;
                if (pawn == null || pawn.Destroyed || !pawn.Spawned)
                    continue;

                float hitChance = GetHitChanceForTarget(pawn);
                int baseRepeat = Rand.RangeInclusive(sp.hitRepeatMin, sp.hitRepeatMax);
                int repeatCount = Mathf.Min(baseRepeat + info.overlapCount, sp.hitRepeatOverlapCap);

                for (int i = 0; i < repeatCount; i++)
                {
                    if (Rand.Chance(hitChance))
                        ApplyDamageToPawn(pawn, baseDamage, damageDef, sp, weapon);
                }
            }

            if (verbProps.soundCast != null)
                verbProps.soundCast.PlayOneShot(SoundInfo.InMap(new TargetInfo(caster.Position, map, false)));

            SpawnWyvernFireExplosion(map, caster, vertexCell, sp.muzzleEffectOffset);

            HashSet<IntVec3> effectCells = CollectEffectCells(map, sectorCells, shieldZones);
            SpawnSectorCellEffects(map, effectCells, sp);
            SpawnShieldBlockEffects(map, shieldedCells, hitShields);

            if (CasterIsPawn)
                CasterPawn.records.Increment(RecordDefOf.ShotsFired);

            return true;
        }

        private HashSet<IntVec3> CollectEffectCells(Map map, List<IntVec3> sectorCells, List<ShieldZoneInfo> shieldZones)
        {
            var cells = new HashSet<IntVec3>();
            IntVec3 casterPos = caster.Position;

            foreach (IntVec3 sectorCell in sectorCells)
            {
                bool blocked = false;
                foreach (IntVec3 pathCell in GenSight.PointsOnLineOfSight(casterPos, sectorCell))
                {
                    if (blocked)
                        break;
                    if (!pathCell.InBounds(map))
                        continue;
                    if (shieldZones.Count > 0 && GetBlockingShieldIndex(pathCell, shieldZones) >= 0)
                    {
                        blocked = true;
                        break;
                    }
                    foreach (Thing t in pathCell.GetThingList(map))
                    {
                        if (t is Pawn p && p != caster)
                        {
                            cells.Add(pathCell);
                            blocked = true;
                            break;
                        }
                    }
                }
                if (!blocked)
                    cells.Add(sectorCell);
            }
            return cells;
        }

        private Dictionary<Pawn, SectorShotTargetInfo> CollectDamageTargetsWithOverlap(Map map, List<IntVec3> sectorCells, List<ShieldZoneInfo> shieldZones)
        {
            var result = new Dictionary<Pawn, SectorShotTargetInfo>();
            IntVec3 casterPos = caster.Position;

            foreach (IntVec3 sectorCell in sectorCells)
            {
                Pawn firstPawn = null;
                bool hasPawnBehind = false;
                bool hitShield = false;
                foreach (IntVec3 pathCell in GenSight.PointsOnLineOfSight(casterPos, sectorCell))
                {
                    if (hitShield)
                        break;
                    if (!pathCell.InBounds(map))
                        continue;
                    if (shieldZones.Count > 0 && GetBlockingShieldIndex(pathCell, shieldZones) >= 0)
                    {
                        hitShield = true;
                        break;
                    }
                    foreach (Thing t in pathCell.GetThingList(map))
                    {
                        if (t is Pawn p && p != caster)
                        {
                            if (firstPawn == null)
                                firstPawn = p;
                            else
                                hasPawnBehind = true;
                        }
                    }
                }
                if (firstPawn != null)
                {
                    if (!result.ContainsKey(firstPawn))
                        result[firstPawn] = new SectorShotTargetInfo { overlapCount = 0 };
                    if (hasPawnBehind)
                    {
                        SectorShotTargetInfo info = result[firstPawn];
                        info.overlapCount++;
                        result[firstPawn] = info;
                    }
                }
            }
            return result;
        }

        /// <summary>대상별 명중률 (일반 사격과 동일: 사격 스킬, 무기 정확도, 거리, 타겟 크기, 포복, 엄폐 반영).</summary>
        private float GetHitChanceForTarget(Pawn target)
        {
            ShotReport report = ShotReport.HitReportFor(caster, this, target);
            return report.TotalEstimatedHitChance;
        }

        private void ApplyDamageToPawn(Pawn pawn, int baseDamage, DamageDef damageDef, VerbProperties_SectorShot sp, Thing weapon)
        {
            int finalDamage = GenMath.RoundRandom(baseDamage);
            float armorPen = sp.sectorArmorPenetration >= 0f
                ? sp.sectorArmorPenetration
                : finalDamage * 0.015f;

            QualityCategory quality = QualityCategory.Normal;
            if (weapon != null)
                weapon.TryGetQuality(out quality);

            DamageInfo dinfo = new DamageInfo(
                damageDef,
                finalDamage,
                armorPen,
                -1f,
                caster,
                null,
                weapon?.def,
                DamageInfo.SourceCategory.ThingOrUnknown,
                pawn,
                true,
                true,
                quality,
                true,
                false);

            pawn.TakeDamage(dinfo);
        }

        /// <summary>총구: BFR HE와 동일한 RK_WyvernFireExplosion 이펙트. DrawPos + 발사방향*muzzleOffset 위치에 스폰.</summary>
        private void SpawnWyvernFireExplosion(Map map, Thing caster, IntVec3 targetCell, float muzzleOffset)
        {
            FleckDef wyvernFleck = DefDatabase<FleckDef>.GetNamedSilentFail("RK_WyvernFireExplosion");
            if (wyvernFleck == null)
                return;

            Vector3 drawPos = caster.DrawPos;
            Vector3 dir = (targetCell.ToVector3Shifted() - drawPos).Yto0();
            if (dir.sqrMagnitude < 1E-06f)
                dir = Vector3.forward;
            else
                dir.Normalize();

            Vector3 muzzlePos = drawPos + dir * muzzleOffset;

            float rot = dir.AngleFlat();
            const float effectScale = 0.5f;
            FleckCreationData data = FleckMaker.GetDataStatic(muzzlePos, map, wyvernFleck, effectScale);
            data.exactScale = new Vector3?(new Vector3(3f, 1f, 2f) * effectScale);
            data.rotation = rot;
            data.instanceColor = new Color(0.75f, 0.55f, 0.55f, 0.7f);
            map.flecks.CreateFleck(data);
        }

        private void FilterShieldedCells(Map map, List<IntVec3> sectorCells, IntVec3 explosionCenter,
            out List<IntVec3> shieldedCells, out HashSet<CompProjectileInterceptor> hitShields,
            out List<ShieldZoneInfo> outZones)
        {
            shieldedCells = new List<IntVec3>();
            hitShields = new HashSet<CompProjectileInterceptor>();

            Vector3 casterVec = caster.Position.ToVector3Shifted();
            Vector2 casterPos = new Vector2(casterVec.x, casterVec.z);

            var zones = new List<ShieldZoneInfo>();
            List<Thing> interceptors = map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor);
            for (int i = 0; i < interceptors.Count; i++)
            {
                var comp = interceptors[i].TryGetComp<CompProjectileInterceptor>();
                if (comp == null || !comp.Active)
                    continue;
                if (!comp.Props.interceptGroundProjectiles)
                    continue;
                Thing shieldThing = interceptors[i];
                if (caster != null && shieldThing.Faction != null && !caster.HostileTo(shieldThing))
                    continue;
                Vector3 pos = shieldThing.Position.ToVector3Shifted();
                float r = comp.Props.radius;
                float rSq = r * r;
                float dx = casterPos.x - pos.x;
                float dz = casterPos.y - pos.z;
                if (dx * dx + dz * dz <= rSq)
                    continue;
                zones.Add(new ShieldZoneInfo { center = new Vector2(pos.x, pos.z), radiusSq = rSq, comp = comp });
            }
            outZones = zones;

            if (zones.Count == 0)
                return;

            for (int i = sectorCells.Count - 1; i >= 0; i--)
            {
                IntVec3 cell = sectorCells[i];
                int idx = GetBlockingShieldIndex(cell, zones);
                if (idx >= 0)
                {
                    shieldedCells.Add(cell);
                    hitShields.Add(zones[idx].comp);
                    sectorCells.RemoveAt(i);
                }
            }
        }

        private struct ShieldZoneInfo
        {
            public Vector2 center;
            public float radiusSq;
            public CompProjectileInterceptor comp;
        }

        private static int GetBlockingShieldIndex(IntVec3 cell, List<ShieldZoneInfo> zones)
        {
            Vector3 cellVec = cell.ToVector3Shifted();
            Vector2 cellPos = new Vector2(cellVec.x, cellVec.z);
            for (int i = 0; i < zones.Count; i++)
            {
                float dx = cellPos.x - zones[i].center.x;
                float dy = cellPos.y - zones[i].center.y;
                if (dx * dx + dy * dy <= zones[i].radiusSq)
                    return i;
            }
            return -1;
        }

        private void SpawnShieldBlockEffects(Map map, List<IntVec3> shieldedCells, HashSet<CompProjectileInterceptor> hitShields)
        {
            if (shieldedCells.Count == 0)
                return;

            Vector2 casterPos2 = new Vector2(caster.Position.ToVector3Shifted().x, caster.Position.ToVector3Shifted().z);

            foreach (var comp in hitShields)
            {
                Vector3 shieldPos = comp.parent.Position.ToVector3Shifted();
                Vector2 shieldCenter = new Vector2(shieldPos.x, shieldPos.z);
                float radius = comp.Props.radius;
                float radiusSq = radius * radius;
                float innerThreshold = (radius - 1.5f) * (radius - 1.5f);

                Vector2 toCaster = casterPos2 - shieldCenter;

                EffecterDef effecterDef = comp.Props.interceptEffect ?? EffecterDefOf.Interceptor_BlockedProjectile;
                HashSet<IntVec3> usedCells = new HashSet<IntVec3>();

                for (int i = 0; i < shieldedCells.Count; i++)
                {
                    IntVec3 cell = shieldedCells[i];
                    Vector3 cv = cell.ToVector3Shifted();
                    Vector2 cp = new Vector2(cv.x, cv.z);
                    float dx = cp.x - shieldCenter.x;
                    float dy = cp.y - shieldCenter.y;
                    float dSq = dx * dx + dy * dy;

                    float dot = dx * toCaster.x + dy * toCaster.y;
                    bool casterFacing = dot > 0f;

                    if (dSq >= innerThreshold && dSq <= radiusSq && casterFacing && !usedCells.Contains(cell))
                    {
                        usedCells.Add(cell);
                        Effecter effecter = new Effecter(effecterDef);
                        effecter.Trigger(new TargetInfo(cell, map, false), TargetInfo.Invalid);
                        effecter.Cleanup();
                    }
                }

                if (usedCells.Count == 0)
                {
                    IntVec3 fallback = shieldedCells[0];
                    Vector2 fp = new Vector2(fallback.ToVector3Shifted().x, fallback.ToVector3Shifted().z);
                    float fdx = fp.x - shieldCenter.x;
                    float fdy = fp.y - shieldCenter.y;
                    float fDot = fdx * toCaster.x + fdy * toCaster.y;
                    if (fDot > 0f)
                    {
                        Effecter effecter = new Effecter(effecterDef);
                        effecter.Trigger(new TargetInfo(fallback, map, false), TargetInfo.Invalid);
                        effecter.Cleanup();
                    }
                }

                if (usedCells.Count > 0)
                {
                    Vector3 avg = Vector3.zero;
                    foreach (IntVec3 c in usedCells)
                        avg += c.ToVector3Shifted();
                    avg /= usedCells.Count;
                    TriggerForceFieldCone(comp, avg);
                }
            }
        }

        private static void TriggerForceFieldCone(CompProjectileInterceptor comp, Vector3 hitPos)
        {
            if (_lastInterceptAngle == null || _lastInterceptTicks == null || _drawInterceptCone == null)
                return;

            float angle = hitPos.AngleToFlat(comp.parent.TrueCenter());

            _lastInterceptAngle.SetValue(comp, angle);
            _lastInterceptTicks.SetValue(comp, Find.TickManager.TicksGame);
            _drawInterceptCone.SetValue(comp, true);
        }

        private void SpawnSectorCellEffects(Map map, IEnumerable<IntVec3> effectCells, VerbProperties_SectorShot sp)
        {
            const float noiseRange = 0.3f;

            foreach (IntVec3 cell in effectCells)
            {
                int count = Rand.RangeInclusive(sp.effectsPerCellMin, sp.effectsPerCellMax);
                Vector3 basePos = cell.ToVector3Shifted();

                for (int i = 0; i < count; i++)
                {
                    Vector3 noise = new Vector3(
                        Rand.Range(-noiseRange, noiseRange),
                        0f,
                        Rand.Range(-noiseRange, noiseRange));
                    Vector3 spawnPos = basePos + noise;
                    if (spawnPos.InBounds(map))
                        FleckMaker.Static(spawnPos, map, FleckDefOf.ShotHit_Dirt, 1f);
                }
            }
        }
    }
}
