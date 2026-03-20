using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace NewRatkin
{
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

            // 피해량·관통력 계산 (품질 반영)
            float damageMultiplier = 1f;
            Thing weapon = EquipmentSource;
            if (weapon != null)
                damageMultiplier = weapon.GetStatValue(StatDefOf.RangedWeapon_DamageMultiplier, true, -1);

            int finalDamage = GenMath.RoundRandom(sp.sectorDamageAmount * damageMultiplier);
            float armorPen = sp.sectorArmorPenetration >= 0f
                ? sp.sectorArmorPenetration
                : finalDamage * 0.015f;

            DamageDef damageDef = sp.sectorDamageDef ?? DamageDefOf.Bullet;

            // GenExplosion으로 부채꼴 피해 적용 (꼭지점 = 사수 방향으로 당긴 위치)
            GenExplosion.DoExplosion(
                center: vertexCell,
                map: map,
                radius: 0f,
                damType: damageDef,
                instigator: caster,
                damAmount: finalDamage,
                armorPenetration: armorPen,
                explosionSound: verbProps.soundCast,
                weapon: weapon?.def,
                intendedTarget: currentTarget.Thing,
                overrideCells: sectorCells);

            // 총구: BFR HE와 동일한 와이번 파이어 폭발 이펙트 (총구에서 발사, DrawPos+방향오프셋)
            SpawnWyvernFireExplosion(map, caster, vertexCell, sp.muzzleEffectOffset);

            // 부채꼴 셀당 지면 착탄 이펙트 (1~3개)
            SpawnSectorCellEffects(map, sectorCells, sp);

            if (CasterIsPawn)
                CasterPawn.records.Increment(RecordDefOf.ShotsFired);

            return true;
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
            FleckCreationData data = FleckMaker.GetDataStatic(muzzlePos, map, wyvernFleck, 1f);
            data.exactScale = new Vector3?(new Vector3(3f, 1f, 2f));
            data.rotation = rot;
            data.instanceColor = new Color(0.75f, 0.55f, 0.55f, 0.7f);
            map.flecks.CreateFleck(data);
        }

        private void SpawnSectorCellEffects(Map map, List<IntVec3> sectorCells, VerbProperties_SectorShot sp)
        {
            const float noiseRange = 0.3f;

            foreach (IntVec3 cell in sectorCells)
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
