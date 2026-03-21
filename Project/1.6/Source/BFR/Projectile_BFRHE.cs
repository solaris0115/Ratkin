using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// BFR HE탄 전용 Projectile 속성.
    /// preDetonationDistance: 목표로부터 기폭점까지 거리(0이면 적중 시 폭발).
    /// sectorAngle: 부채꼴 각도(도), sectorRadius: 반지름, damageAmountDirect/Explosion: 적중/폭발 피해.
    /// wallBreachRadius: 이 거리 이내는 벽 무시(최초 후폭발), 초과 시 벽에 막힘.
    /// </summary>
    public class ProjectileProperties_BFRHE : ProjectileProperties
    {
        public float preDetonationDistance = 0f;
        public float sectorAngle = 90f;
        public float sectorRadius = 1.7f;
        public float wallBreachRadius = 1.7f;
        public int damageAmountDirect = 40;
        public int damageAmountExplosion = 25;
        public DamageDef damageDefDirect;
        public DamageDef damageDefExplosion;
        public float armorPenetrationDirect = -1f;
        public float armorPenetrationExplosion = -1f;
        /// <summary>부채꼴 셀당 총탄 이펙트. null이면 ImpactSmallDustCloud 사용.</summary>
        public EffecterDef sectorCellEffecterDef;
    }

    /// <summary>
    /// BFR HE탄 - 적중 시 후면 부채꼴로 폭발.
    /// 부채꼴 각도·반지름, 적중 피해·폭발 피해 별도 설정 가능.
    /// wallBreachRadius 이내는 벽 무시, 초과 시 벽에 막혀 관통하지 않음.
    /// </summary>
    public class Projectile_BFRHE : Projectile_Explosive
    {
        private ProjectileProperties_BFRHE BFRProps => def.projectile as ProjectileProperties_BFRHE;
        private bool reachedDetonationPoint;
        private bool wasBlockedByShield;

        protected override int MaxTickIntervalRate => 1;

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            wasBlockedByShield = blockedByShield;
            base.Impact(hitThing, blockedByShield);
        }

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            float preDet = BFRProps?.preDetonationDistance ?? 0f;
            if (preDet > 0f)
            {
                Vector3 targetVec = usedTarget.Cell.ToVector3Shifted();
                Vector3 dir = (targetVec - origin).Yto0();
                float dist = dir.magnitude;
                if (dist > preDet + 1f)
                {
                    Vector3 detonationPoint = targetVec - dir.normalized * preDet;
                    IntVec3 detonationCell = detonationPoint.ToIntVec3();
                    usedTarget = new LocalTargetInfo(detonationCell);
                }
            }
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
        }

        protected override void ImpactSomething()
        {
            reachedDetonationPoint = true;
            base.ImpactSomething();
        }

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

            if (wasBlockedByShield)
            {
                Destroy(DestroyMode.Vanish);
                return;
            }

            // 가로막힌 경우: 직격 데미지만 (폭발 없음). damageAmountDirect는 가로막힐 때만 적용.
            if (!reachedDetonationPoint && props.preDetonationDistance > 0f)
            {
                ApplyDirectHitDamage(map, impactPos, props);
                Destroy(DestroyMode.Vanish);
                return;
            }

            List<IntVec3> shieldedCells;
            HashSet<CompProjectileInterceptor> hitShields;
            List<IntVec3> sectorCells = GetSectorCells(impactPos, map, props, out shieldedCells, out hitShields);
            if (sectorCells.Count > 0)
            {
                DoSectorExplosion(impactPos, map, sectorCells, props);
            }

            SpawnCenterExplosionVisual(impactPos, map);
            SpawnSectorCellEffects(map, sectorCells);
            SpawnShieldBlockEffects(map, shieldedCells, hitShields);

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

        private List<IntVec3> GetSectorCells(IntVec3 center, Map map, ProjectileProperties_BFRHE props,
            out List<IntVec3> shieldedCells, out HashSet<CompProjectileInterceptor> hitShields)
        {
            List<IntVec3> result = new List<IntVec3>();
            shieldedCells = new List<IntVec3>();
            hitShields = new HashSet<CompProjectileInterceptor>();
            float sectorRadius = props.sectorRadius > 0f ? props.sectorRadius : props.explosionRadius;
            float halfAngle = (props.sectorAngle > 0f ? props.sectorAngle : 90f) * 0.5f;
            float wallBreachRadius = props.wallBreachRadius > 0f ? props.wallBreachRadius : 1.7f;
            float wallBreachRadiusSq = wallBreachRadius * wallBreachRadius;

            var shieldZones = GetHostileShieldZones(map, center);

            Vector3 centerVec = center.ToVector3Shifted().Yto0();
            Vector3 dirToBack = (destination - origin).Yto0();
            if (dirToBack.sqrMagnitude < 1E-06f)
                return result;
            dirToBack.Normalize();
            float centerAngle = Vector3.SignedAngle(Vector3.right, dirToBack, Vector3.up);

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

                int shieldIdx = GetBlockingShieldIndex(cell, shieldZones);
                if (shieldIdx >= 0)
                {
                    shieldedCells.Add(cell);
                    hitShields.Add(shieldZones[shieldIdx].comp);
                    continue;
                }

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

            AddAdjacentWallCells(center, map, sectorRadius, result);

            return result;
        }

        private static int GetBlockingShieldIndex(IntVec3 cell, List<ShieldZone> zones)
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

        private struct ShieldZone
        {
            public Vector2 center;
            public float radiusSq;
            public CompProjectileInterceptor comp;
            public Thing parent;
        }

        private List<ShieldZone> GetHostileShieldZones(Map map, IntVec3 explosionCenter)
        {
            var zones = new List<ShieldZone>();
            Vector3 expVec = explosionCenter.ToVector3Shifted();
            Vector2 expPos = new Vector2(expVec.x, expVec.z);
            List<Thing> interceptors = map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor);
            for (int i = 0; i < interceptors.Count; i++)
            {
                var comp = interceptors[i].TryGetComp<CompProjectileInterceptor>();
                if (comp == null || !comp.Active)
                    continue;
                if (!comp.Props.interceptGroundProjectiles)
                    continue;
                Thing shieldThing = interceptors[i];
                if (launcher != null && shieldThing.Faction != null && !launcher.HostileTo(shieldThing))
                    continue;
                Vector3 pos = shieldThing.Position.ToVector3Shifted();
                float r = comp.Props.radius;
                float rSq = r * r;
                float dx = expPos.x - pos.x;
                float dz = expPos.y - pos.z;
                if (dx * dx + dz * dz <= rSq)
                    continue;
                zones.Add(new ShieldZone { center = new Vector2(pos.x, pos.z), radiusSq = rSq, comp = comp, parent = shieldThing });
            }
            return zones;
        }


        /// <summary>
        /// LOS 셀에 인접한 벽 셀을 result에 추가. 일반 폭발(ExplosionCellsToHit)과 동일한 로직.
        /// </summary>
        private void AddAdjacentWallCells(IntVec3 center, Map map, float radius, List<IntVec3> result)
        {
            HashSet<IntVec3> resultSet = new HashSet<IntVec3>(result);
            List<IntVec3> adjWalls = new List<IntVec3>();
            for (int j = 0; j < result.Count; j++)
            {
                IntVec3 cell = result[j];
                Building edifice = cell.GetEdifice(map);
                if (cell.Walkable(map))
                {
                    if (edifice != null && edifice.def.Fillage == FillCategory.Full)
                    {
                        Building_Door door = edifice as Building_Door;
                        if (door == null || !door.Open)
                            continue;
                    }
                    for (int k = 0; k < 4; k++)
                    {
                        IntVec3 adj = cell + GenAdj.CardinalDirections[k];
                        if (adj.InHorDistOf(center, radius) && adj.InBounds(map) && !adj.Standable(map) && adj.GetEdifice(map) != null && !resultSet.Contains(adj) && !adjWalls.Contains(adj))
                        {
                            adjWalls.Add(adj);
                        }
                    }
                }
            }
            result.AddRange(adjWalls);
        }

        /// <summary>기폭점: WyvernFire 이펙트 (잿빛, 알파 0.7, 1.2배 스케일, 0.7배 재생시간). 부채꼴은 폭발 방향을 향함. 사운드 Shockwave.</summary>
        private void SpawnCenterExplosionVisual(IntVec3 center, Map map)
        {
            List<IntVec3> overrideCells = new List<IntVec3> { center };
            SoundDef explosionSound = def.projectile.soundExplode;
            GenExplosion.DoExplosion(
                center,
                map,
                1f,
                DamageDefOf.Bomb,
                null,
                0,
                -1f,
                explosionSound,
                null,
                null,
                null,
                null,
                0f,
                1,
                null,
                null,
                255,
                false,
                null,
                0f,
                1,
                0f,
                false,
                null,
                null,
                null,
                false,
                1f,
                0f,
                true,
                null,
                1f,
                null,
                overrideCells,
                null,
                null);

            FleckDef wyvernFleck = DefDatabase<FleckDef>.GetNamedSilentFail("RK_WyvernFireExplosion");
            if (wyvernFleck != null)
            {
                Vector3 dir = (destination - origin).Yto0();
                float rot = dir.sqrMagnitude > 1E-06f ? dir.AngleFlat() : 0f;
                var data = FleckMaker.GetDataStatic(center.ToVector3Shifted(), map, wyvernFleck, 1f);
                data.exactScale = new Vector3?(new Vector3(3f, 1f, 2f)); // 가로 3배, 세로 2배
                data.rotation = rot;
                data.instanceColor = new Color(0.75f, 0.55f, 0.55f, 0.7f);
                map.flecks.CreateFleck(data);
            }
        }

        private void SpawnSectorCellEffects(Map map, List<IntVec3> sectorCells)
        {
            const int effectsPerCell = 2;
            const float noiseRange = 0.3f;

            foreach (IntVec3 cell in sectorCells)
            {
                Vector3 basePos = cell.ToVector3Shifted();
                for (int i = 0; i < effectsPerCell; i++)
                {
                    Vector3 noise = new Vector3(Rand.Range(-noiseRange, noiseRange), 0f, Rand.Range(-noiseRange, noiseRange));
                    Vector3 spawnPos = basePos + noise;
                    if (spawnPos.InBounds(map))
                    {
                        FleckMaker.Static(spawnPos, map, FleckDefOf.ShotHit_Dirt, 1f);
                    }
                }
            }
        }

        private void SpawnShieldBlockEffects(Map map, List<IntVec3> shieldedCells, HashSet<CompProjectileInterceptor> hitShields)
        {
            if (shieldedCells.Count == 0)
                return;

            foreach (var comp in hitShields)
            {
                Vector3 shieldPos = comp.parent.Position.ToVector3Shifted();
                Vector2 shieldCenter = new Vector2(shieldPos.x, shieldPos.z);
                float radius = comp.Props.radius;
                float radiusSq = radius * radius;
                float innerThreshold = (radius - 1.5f) * (radius - 1.5f);

                EffecterDef effecterDef = comp.Props.interceptEffect ?? EffecterDefOf.Interceptor_BlockedProjectile;
                HashSet<IntVec3> usedCells = new HashSet<IntVec3>();

                for (int i = 0; i < shieldedCells.Count; i++)
                {
                    Vector3 cv = shieldedCells[i].ToVector3Shifted();
                    Vector2 cp = new Vector2(cv.x, cv.z);
                    float dx = cp.x - shieldCenter.x;
                    float dy = cp.y - shieldCenter.y;
                    float dSq = dx * dx + dy * dy;
                    if (dSq >= innerThreshold && dSq <= radiusSq && !usedCells.Contains(shieldedCells[i]))
                    {
                        usedCells.Add(shieldedCells[i]);
                        Effecter effecter = new Effecter(effecterDef);
                        effecter.Trigger(new TargetInfo(shieldedCells[i], map, false), TargetInfo.Invalid);
                        effecter.Cleanup();
                    }
                }

                if (usedCells.Count == 0 && shieldedCells.Count > 0)
                {
                    IntVec3 fallback = shieldedCells[0];
                    Effecter effecter = new Effecter(effecterDef);
                    effecter.Trigger(new TargetInfo(fallback, map, false), TargetInfo.Invalid);
                    effecter.Cleanup();
                }
            }
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
                false,
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
