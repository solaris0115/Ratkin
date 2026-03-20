using System.Collections.Generic;
using System.Linq;
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

            // LOS 기반 실제 피해 대상 수집 (중첩 수 포함)
            Dictionary<Pawn, SectorShotTargetInfo> damageTargets = CollectDamageTargetsWithOverlap(map, sectorCells);

            // 피해량·관통력 계산 (품질 반영)
            Thing weapon = EquipmentSource;
            float damageMultiplier = weapon != null ? weapon.GetStatValue(StatDefOf.RangedWeapon_DamageMultiplier, true, -1) : 1f;
            int baseDamage = GenMath.RoundRandom(sp.sectorDamageAmount * damageMultiplier);
            DamageDef damageDef = sp.sectorDamageDef ?? DamageDefOf.Bullet;

            // 각 피해 대상에 대해 명중률 기반 반복 피해 적용. 중첩 시 1회씩만 추가, 최대 hitRepeatOverlapCap
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

            // 사운드
            if (verbProps.soundCast != null)
                verbProps.soundCast.PlayOneShot(SoundInfo.InMap(new TargetInfo(caster.Position, map, false)));

            // 총구 이펙트
            SpawnWyvernFireExplosion(map, caster, vertexCell, sp.muzzleEffectOffset);

            // 지면 착탄 이펙트: LOS로 가려진 경우 실제 총탄이 맞는 위치(첫 Pawn 셀)에 스폰
            HashSet<IntVec3> effectCells = CollectEffectCells(map, sectorCells);
            SpawnSectorCellEffects(map, effectCells, sp);

            if (CasterIsPawn)
                CasterPawn.records.Increment(RecordDefOf.ShotsFired);

            return true;
        }

        /// <summary>지면 착탄 이펙트 셀 수집. LOS로 가려진 경우 첫 Pawn 셀, 아니면 부채꼴 셀.</summary>
        private HashSet<IntVec3> CollectEffectCells(Map map, List<IntVec3> sectorCells)
        {
            var cells = new HashSet<IntVec3>();
            IntVec3 casterPos = caster.Position;

            foreach (IntVec3 sectorCell in sectorCells)
            {
                bool foundPawnOnThisLine = false;
                foreach (IntVec3 pathCell in GenSight.PointsOnLineOfSight(casterPos, sectorCell))
                {
                    if (foundPawnOnThisLine)
                        break;
                    if (!pathCell.InBounds(map))
                        continue;
                    foreach (Thing t in pathCell.GetThingList(map))
                    {
                        if (t is Pawn p && p != caster)
                        {
                            cells.Add(pathCell);
                            foundPawnOnThisLine = true;
                            break;
                        }
                    }
                }
                if (!foundPawnOnThisLine)
                    cells.Add(sectorCell);
            }
            return cells;
        }

        /// <summary>부채꼴 내 각 셀에 대해 사수->셀 LOS 경로상의 첫 번째 Pawn 수집. LOS로 다른 Pawn을 가리는(뒤에 Pawn이 있는) 경우에만 중첩 수 카운트.</summary>
        private Dictionary<Pawn, SectorShotTargetInfo> CollectDamageTargetsWithOverlap(Map map, List<IntVec3> sectorCells)
        {
            var result = new Dictionary<Pawn, SectorShotTargetInfo>();
            IntVec3 casterPos = caster.Position;

            foreach (IntVec3 sectorCell in sectorCells)
            {
                Pawn firstPawn = null;
                bool hasPawnBehind = false;
                foreach (IntVec3 pathCell in GenSight.PointsOnLineOfSight(casterPos, sectorCell))
                {
                    if (!pathCell.InBounds(map))
                        continue;
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
