using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TestMod
{
    public class Verb_MeleeExplosiveAttack : Verb_MeleeAttackDamage
    {
        private readonly List<IntVec3> tmpCells = new List<IntVec3>();

        protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
        {
            // 기본 피해 적용
            DamageWorker.DamageResult result = base.ApplyMeleeDamageToTarget(target);

            // 폭발 처리
            Thing equipmentSource = this.EquipmentSource;
            if (equipmentSource != null)
            {
                CompMeleeExplosive comp = equipmentSource.TryGetComp<CompMeleeExplosive>();
                if (comp != null)
                {
                    if (Rand.Chance(comp.Props.chance))
                    {
                        DoExplosion(comp.Props, target.Cell);
                    }
                }
            }

            return result;
        }

        private void DoExplosion(CompProperties_MeleeExplosive props, IntVec3 targetCell)
        {
            Map map = this.CasterPawn.MapHeld;
            if (map == null) return;

            // AffectedCells 계산
            List<IntVec3> affectedCells = CalculateAffectedCells(props, targetCell);

            // GenExplosion 호출
            int damAmount = props.damAmount;
            float armorPenetration = props.armorPenetration;
            if (armorPenetration < 0f)
            {
                armorPenetration = damAmount * 0.015f;
            }

            GenExplosion.DoExplosion(
                center: targetCell,
                map: map,
                radius: 0f, // overrideCells 사용하므로 0으로 설정
                damType: props.damageDef,
                instigator: this.CasterPawn,
                damAmount: damAmount,
                armorPenetration: armorPenetration,
                explosionSound: null,
                weapon: this.EquipmentSource?.def,
                projectile: null,
                intendedTarget: null,
                postExplosionSpawnThingDef: null,
                postExplosionSpawnChance: 0f,
                postExplosionSpawnThingCount: 1,
                postExplosionGasType: null,
                postExplosionGasRadiusOverride: null,
                postExplosionGasAmount: 255,
                applyDamageToExplosionCellsNeighbors: false,
                preExplosionSpawnThingDef: null,
                preExplosionSpawnChance: 0f,
                preExplosionSpawnThingCount: 1,
                chanceToStartFire: 0f,
                damageFalloff: false,
                direction: null,
                ignoredThings: null,
                affectedAngle: null,
                doVisualEffects: true,
                propagationSpeed: 1f,
                excludeRadius: 0f,
                doSoundEffects: true,
                postExplosionSpawnThingDefWater: null,
                screenShakeFactor: 1f,
                flammabilityChanceCurve: null,
                overrideCells: affectedCells, // 원뿔형 범위 셀 목록
                postExplosionSpawnSingleThingDef: null,
                preExplosionSpawnSingleThingDef: null
            );
        }

        private List<IntVec3> CalculateAffectedCells(CompProperties_MeleeExplosive props, IntVec3 targetCell)
        {
            tmpCells.Clear();

            Vector3 casterPos = this.CasterPawn.Position.ToVector3Shifted().Yto0();
            IntVec3 endCell = targetCell.ClampInsideMap(this.CasterPawn.Map);

            // 공격자가 같은 셀에 있으면 무효
            if (this.CasterPawn.Position == endCell)
            {
                return tmpCells;
            }

            // 거리와 방향 계산
            float lengthHorizontal = (endCell - this.CasterPawn.Position).LengthHorizontal;
            if (lengthHorizontal <= 0f)
            {
                return tmpCells;
            }

            float dx = (endCell.x - this.CasterPawn.Position.x) / lengthHorizontal;
            float dz = (endCell.z - this.CasterPawn.Position.z) / lengthHorizontal;

            // props.range까지의 끝점 계산
            endCell.x = Mathf.RoundToInt(this.CasterPawn.Position.x + dx * props.range);
            endCell.z = Mathf.RoundToInt(this.CasterPawn.Position.z + dz * props.range);

            // 원뿔 각도 계산
            float targetAngle = Vector3.SignedAngle(
                endCell.ToVector3Shifted().Yto0() - casterPos, 
                Vector3.right, 
                Vector3.up
            );

            float halfWidth = props.lineWidthEnd / 2f;
            float coneLength = Mathf.Sqrt(
                Mathf.Pow((endCell - this.CasterPawn.Position).LengthHorizontal, 2f) + 
                Mathf.Pow(halfWidth, 2f)
            );
            float coneAngle = 57.29578f * Mathf.Asin(halfWidth / coneLength);

            // 범위 내 모든 셀 확인
            int cellCount = GenRadial.NumCellsInRadius(props.range);
            for (int i = 0; i < cellCount; i++)
            {
                IntVec3 cell = this.CasterPawn.Position + GenRadial.RadialPattern[i];
                if (CanUseCell(cell, props))
                {
                    float cellAngle = Vector3.SignedAngle(
                        cell.ToVector3Shifted().Yto0() - casterPos,
                        Vector3.right,
                        Vector3.up
                    );

                    if (Mathf.Abs(Mathf.DeltaAngle(cellAngle, targetAngle)) <= coneAngle)
                    {
                        tmpCells.Add(cell);
                    }
                }
            }

            // Bresenham 라인을 추가 (빈 공간 처리)
            List<IntVec3> lineCells = GenSight.BresenhamCellsBetween(this.CasterPawn.Position, endCell);
            for (int j = 0; j < lineCells.Count; j++)
            {
                IntVec3 cell = lineCells[j];
                if (!tmpCells.Contains(cell) && CanUseCell(cell, props))
                {
                    tmpCells.Add(cell);
                }
            }

            return tmpCells;
        }

        private bool CanUseCell(IntVec3 c, CompProperties_MeleeExplosive props)
        {
            if (!c.InBounds(this.CasterPawn.Map)) return false;
            if (c == this.CasterPawn.Position) return false;

            if (!props.canHitFilledCells && c.Filled(this.CasterPawn.Map)) return false;
            if (!c.InHorDistOf(this.CasterPawn.Position, props.range)) return false;

            ShootLine shootLine;
            return this.TryFindShootLineFromTo(this.CasterPawn.Position, c, out shootLine, false);
        }
    }
}

