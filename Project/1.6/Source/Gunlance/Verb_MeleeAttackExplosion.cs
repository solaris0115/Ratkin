using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// 근접 공격 시 전방에 원뿔형 폭발을 일으키는 Verb
    /// Verb_SpewFire와 Verb_MeleeAttack를 결합한 구조
    /// </summary>
    public class Verb_MeleeAttackExplosion : Verb_MeleeAttack
    {
        private VerbProperties_MeleeExplosion ExplosionProps
        {
            get
            {
                return this.verbProps as VerbProperties_MeleeExplosion;
            }
        }

        protected override bool TryCastShot()
        {
            // 기본 근접 공격 실행
            bool hitSuccess = base.TryCastShot();

            // 명중 시에만 폭발 발생
            if (hitSuccess)
            {
                TriggerExplosion();
            }

            return hitSuccess;
        }

        /// <summary>
        /// 타겟 방향으로 원뿔형 폭발 발생
        /// </summary>
        private void TriggerExplosion()
        {
            if (ExplosionProps == null)
            {
                Log.Error("Verb_MeleeAttackExplosion requires VerbProperties_MeleeExplosion!");
                return;
            }

            Pawn casterPawn = CasterPawn;
            if (casterPawn == null || !casterPawn.Spawned)
            {
                return;
            }

            Thing targetThing = currentTarget.Thing;
            if (targetThing == null)
            {
                return;
            }

            // 타겟 방향 각도 계산 (Verb_SpewFire 방식)
            IntVec3 casterPos = casterPawn.Position;
            IntVec3 targetPos = targetThing.Position;
            
            float angleToTarget = Mathf.Atan2(
                -(targetPos.z - casterPos.z),
                targetPos.x - casterPos.x
            ) * Mathf.Rad2Deg;

            // 원뿔 각도 범위 계산
            float halfAngle = ExplosionProps.explosionAngle / 2f;
            FloatRange affectedAngle = new FloatRange(
                angleToTarget - halfAngle,
                angleToTarget + halfAngle
            );

            // 폭발 발생 (디폴트 파라미터 활용)
            GenExplosion.DoExplosion(
                center: casterPos,
                map: casterPawn.Map,
                radius: ExplosionProps.explosionRadius,
                damType: ExplosionProps.explosionDamageDef ?? DamageDefOf.Bomb,
                instigator: casterPawn,
                damAmount: ExplosionProps.explosionDamageAmount,
                armorPenetration: ExplosionProps.explosionArmorPenetration,
                explosionSound: ExplosionProps.explosionSound,
                weapon: EquipmentSource?.def,
                intendedTarget: targetThing,
                postExplosionSpawnThingDef: ExplosionProps.postExplosionFilth,
                postExplosionSpawnChance: ExplosionProps.postExplosionFilth != null ? 1f : 0f,
                chanceToStartFire: ExplosionProps.chanceToStartFire,
                damageFalloff: true,
                affectedAngle: affectedAngle,
                screenShakeFactor: ExplosionProps.screenShakeFactor
            );
        }

        protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
        {
            // 기본 근접 데미지 적용 (Verb_MeleeAttackDamage 방식)
            DamageWorker.DamageResult result = new DamageWorker.DamageResult();
            
            foreach (DamageInfo dinfo in DamageInfosToApply(target))
            {
                if (target.ThingDestroyed)
                {
                    break;
                }
                result = target.Thing.TakeDamage(dinfo);
            }
            
            return result;
        }

        /// <summary>
        /// 적용할 데미지 정보 생성 (Verb_MeleeAttackDamage에서 가져옴)
        /// </summary>
        private IEnumerable<DamageInfo> DamageInfosToApply(LocalTargetInfo target)
        {
            float num = this.verbProps.AdjustedMeleeDamageAmount(this, this.CasterPawn);
            float armorPenetration = this.verbProps.AdjustedArmorPenetration(this, this.CasterPawn);
            DamageDef def = this.verbProps.meleeDamageDef;
            BodyPartGroupDef bodyPartGroupDef = null;
            HediffDef hediffDef = null;
            
            num = Rand.Range(num * 0.8f, num * 1.2f);
            
            if (this.CasterIsPawn)
            {
                bodyPartGroupDef = this.verbProps.AdjustedLinkedBodyPartsGroup(this.tool);
                if (num >= 1f)
                {
                    if (base.HediffCompSource != null)
                    {
                        hediffDef = base.HediffCompSource.Def;
                    }
                }
                else
                {
                    num = 1f;
                    def = DamageDefOf.Blunt;
                }
            }
            
            ThingDef source;
            if (base.EquipmentSource != null)
            {
                source = base.EquipmentSource.def;
            }
            else
            {
                source = this.CasterPawn.def;
            }
            
            Vector3 direction = (target.Thing.Position - this.CasterPawn.Position).ToVector3();
            
            DamageInfo damageInfo = new DamageInfo(
                def, num, armorPenetration, -1f, this.caster, null, source,
                DamageInfo.SourceCategory.ThingOrUnknown, null, true, true
            );
            damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
            damageInfo.SetWeaponBodyPartGroup(bodyPartGroupDef);
            damageInfo.SetWeaponHediff(hediffDef);
            damageInfo.SetAngle(direction);
            
            yield return damageInfo;
            
            // 추가 데미지
            if (this.tool != null && this.tool.extraMeleeDamages != null)
            {
                foreach (ExtraDamage extraDamage in this.tool.extraMeleeDamages)
                {
                    if (Rand.Chance(extraDamage.chance))
                    {
                        num = extraDamage.amount;
                        num = Rand.Range(num * 0.8f, num * 1.2f);
                        damageInfo = new DamageInfo(
                            extraDamage.def, num,
                            extraDamage.AdjustedArmorPenetration(this, this.CasterPawn),
                            -1f, this.caster, null, source,
                            DamageInfo.SourceCategory.ThingOrUnknown, null, true, true
                        );
                        damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                        damageInfo.SetWeaponBodyPartGroup(bodyPartGroupDef);
                        damageInfo.SetWeaponHediff(hediffDef);
                        damageInfo.SetAngle(direction);
                        yield return damageInfo;
                    }
                }
            }
            
            yield break;
        }
    }
}

