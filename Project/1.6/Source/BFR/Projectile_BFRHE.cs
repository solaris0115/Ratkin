using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// BFR HE탄 전용 Projectile 속성.
    /// sectorAngle: 부채꼴 각도(도), sectorRadius: 반지름, damageAmountDirect/Explosion: 적중/폭발 피해.
    /// wallBreachRadius: 이 거리 이내는 벽 무시(최초 후폭발), 초과 시 벽에 막힘.
    /// </summary>
    public class ProjectileProperties_BFRHE : ProjectileProperties
    {
        public float sectorAngle = 90f;
        public float sectorRadius = 1.7f;
        public float wallBreachRadius = 1.7f;
        public int damageAmountDirect = 40;
        public int damageAmountExplosion = 25;
        public DamageDef damageDefDirect;
        public DamageDef damageDefExplosion;
        public float armorPenetrationDirect = -1f;
        public float armorPenetrationExplosion = -1f;
    }

    /// <summary>
    /// BFR HE탄 - 적중 시 후면 부채꼴로 폭발.
    /// 부채꼴 각도·반지름, 적중 피해·폭발 피해 별도 설정 가능.
    /// wallBreachRadius 이내는 벽 무시, 초과 시 벽에 막혀 관통하지 않음.
    /// </summary>
    public class Projectile_BFRHE : Projectile_Explosive
    {
        private ProjectileProperties_BFRHE BFRProps => def.projectile as ProjectileProperties_BFRHE;

        protected override void Explode()
        {
            Map map = Map;
            IntVec3 impactPos = Position;
            if (map == null)
            {
                base.Explode();
                return;
            }

            ProjectileProperties_BFRHE props = BFRProps;
            if (props == null)
            {
                base.Explode();
                return;
            }

            // 디버그: 발사/적중/폭발 위치 로그
            string targetStr = intendedTarget.HasThing ? $"{intendedTarget.Thing?.LabelShort} @ {intendedTarget.Thing?.Position}" : $"셀 {intendedTarget.Cell}";
            Log.Message($"[BFR_HE] 발사위치(origin): {origin} -> 셀 {origin.ToIntVec3()}");
            Log.Message($"[BFR_HE] 목표위치(destination): {destination} -> 셀 {destination.ToIntVec3()}");
            Log.Message($"[BFR_HE] 폭발위치(impactPos): {impactPos}");
            Log.Message($"[BFR_HE] 적중대상(intendedTarget): {targetStr}");

            // 1. 적중 피해 (직격)
            ApplyDirectHitDamage(map, impactPos, props);

            // 2. 부채꼴 폭발 (후면, 벽 관통 제한)
            List<IntVec3> sectorCells = GetSectorCells(impactPos, map, props);
            Log.Message($"[BFR_HE] 부채꼴 셀 수: {sectorCells.Count}");
            if (sectorCells.Count > 0)
            {
                DoSectorExplosion(impactPos, map, sectorCells, props);
            }

            // 이펙트
            if (def.projectile.explosionEffect != null)
            {
                Effecter effecter = def.projectile.explosionEffect.Spawn();
                if (def.projectile.explosionEffectLifetimeTicks != 0)
                {
                    map.effecterMaintainer.AddEffecterToMaintain(effecter, impactPos.ToVector3().ToIntVec3(), def.projectile.explosionEffectLifetimeTicks);
                }
                else
                {
                    effecter.Trigger(new TargetInfo(impactPos, map, false), new TargetInfo(impactPos, map, false), -1);
                    effecter.Cleanup();
                }
            }

            Destroy(DestroyMode.Vanish);
        }

        private void ApplyDirectHitDamage(Map map, IntVec3 impactPos, ProjectileProperties_BFRHE props)
        {
            int damAmount = props.damageAmountDirect > 0 ? props.damageAmountDirect : DamageAmount;
            DamageDef damageDef = props.damageDefDirect ?? DamageDef;
            float armorPen = props.armorPenetrationDirect >= 0f ? props.armorPenetrationDirect : ArmorPenetration;

            foreach (Thing thing in impactPos.GetThingList(map).ToList())
            {
                if (thing == launcher || thing == this)
                    continue;
                if (thing.Destroyed)
                    continue;

                Pawn pawn = launcher as Pawn;
                bool instigatorGuilty = pawn == null || !pawn.Drafted;
                DamageInfo dinfo = new DamageInfo(
                    damageDef,
                    damAmount,
                    armorPen,
                    ExactRotation.eulerAngles.y,
                    launcher,
                    null,
                    equipmentDef,
                    DamageInfo.SourceCategory.ThingOrUnknown,
                    intendedTarget.Thing,
                    instigatorGuilty,
                    true,
                    QualityCategory.Normal,
                    true,
                    false);
                dinfo.SetWeaponQuality(equipmentQuality);
                thing.TakeDamage(dinfo);
            }
        }

        /// <summary>
        /// 적중 지점 기준 후면 부채꼴 셀 목록 (적중 셀 제외).
        /// wallBreachRadius 이내: 벽 무시(최초 후폭발). 초과 시: GenSight.LineOfSight로 벽 차단 확인.
        /// </summary>
        private List<IntVec3> GetSectorCells(IntVec3 center, Map map, ProjectileProperties_BFRHE props)
        {
            List<IntVec3> result = new List<IntVec3>();
            float sectorRadius = props.sectorRadius > 0f ? props.sectorRadius : props.explosionRadius;
            float halfAngle = (props.sectorAngle > 0f ? props.sectorAngle : 90f) * 0.5f;
            float wallBreachRadius = props.wallBreachRadius > 0f ? props.wallBreachRadius : 1.7f;
            float wallBreachRadiusSq = wallBreachRadius * wallBreachRadius;

            Vector3 centerVec = center.ToVector3Shifted().Yto0();
            // 후면 = 대상 뒷편 = 탄이 날아가는 방향(destination - origin). 공격자 방향(origin - destination)이 아님.
            Vector3 dirToBack = (destination - origin).Yto0();
            if (dirToBack.sqrMagnitude < 1E-06f)
                return result;
            dirToBack.Normalize();
            float centerAngle = Vector3.SignedAngle(Vector3.right, dirToBack, Vector3.up);
            Log.Message($"[BFR_HE] 부채꼴 중심각(후면방향): {centerAngle:F1}°, 반지름: {sectorRadius}, halfAngle: {halfAngle}°, wallBreach: {wallBreachRadius}");

            int numCells = GenRadial.NumCellsInRadius(sectorRadius);
            for (int i = 1; i < numCells; i++)
            {
                IntVec3 offset = GenRadial.RadialPattern[i];
                IntVec3 cell = center + offset;
                if (!cell.InBounds(map))
                    continue;
                if (cell == center)
                    continue;

                Vector3 cellVec = cell.ToVector3Shifted().Yto0();
                float cellAngle = Vector3.SignedAngle(Vector3.right, (cellVec - centerVec).normalized, Vector3.up);
                if (Mathf.Abs(Mathf.DeltaAngle(cellAngle, centerAngle)) > halfAngle)
                    continue;

                // wallBreachRadius 이내: 벽 무시. 초과: 벽에 막히면 제외.
                float distSq = (cell - center).LengthHorizontalSquared;
                if (distSq <= wallBreachRadiusSq)
                {
                    result.Add(cell);
                }
                else if (GenSight.LineOfSight(center, cell, map, true, null, 0, 0))
                {
                    result.Add(cell);
                }
            }

            return result;
        }

        private void DoSectorExplosion(IntVec3 center, Map map, List<IntVec3> sectorCells, ProjectileProperties_BFRHE props)
        {
            DamageDef damageDef = props.damageDefExplosion ?? DamageDef;
            int damAmount = props.damageAmountExplosion > 0 ? props.damageAmountExplosion : DamageAmount;
            float armorPen = props.armorPenetrationExplosion >= 0f ? props.armorPenetrationExplosion : ArmorPenetration;

            GenExplosion.DoExplosion(
                center,
                map,
                0f,
                damageDef,
                launcher,
                damAmount,
                armorPen,
                def.projectile.soundExplode,
                equipmentDef,
                def,
                intendedTarget.Thing,
                def.projectile.postExplosionSpawnThingDef,
                def.projectile.postExplosionSpawnChance,
                def.projectile.postExplosionSpawnThingCount,
                null,
                null,
                255,
                def.projectile.applyDamageToExplosionCellsNeighbors,
                def.projectile.preExplosionSpawnThingDef,
                def.projectile.preExplosionSpawnChance,
                def.projectile.preExplosionSpawnThingCount,
                def.projectile.explosionChanceToStartFire,
                def.projectile.explosionDamageFalloff,
                null,
                null,
                null,
                def.projectile.doExplosionVFX,
                damageDef.expolosionPropagationSpeed,
                0f,
                true,
                def.projectile.postExplosionSpawnThingDefWater,
                def.projectile.screenShakeFactor,
                null,
                sectorCells,
                null,
                null);
        }
    }
}
