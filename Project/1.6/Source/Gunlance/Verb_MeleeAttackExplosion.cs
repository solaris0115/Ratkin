using System.Collections.Generic;
using RimWorld;
using Verse.Sound;
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
            Pawn casterPawn = this.CasterPawn;
            if (!casterPawn.Spawned) return false;
            if (casterPawn.stances.FullBodyBusy) return false;

            Thing thing = this.currentTarget.Thing;
            if (!this.CanHitTarget(thing))
            {
                Pawn pawn = casterPawn;
                string str = (pawn != null) ? pawn.ToString() : null;
                string str2 = " meleed ";
                Thing thing2 = thing;
                Log.Warning(str + str2 + ((thing2 != null) ? thing2.ToString() : null) + " from out of melee position.");
            }

            casterPawn.rotationTracker.Face(thing.DrawPos);
            if (!this.IsTargetImmobile(this.currentTarget) && casterPawn.skills != null)
            {
                casterPawn.skills.Learn(SkillDefOf.Melee, 200f * this.verbProps.AdjustedFullCycleTime(this, casterPawn), false);
            }

            Pawn pawn2 = thing as Pawn;
            if (pawn2 != null && !pawn2.Dead && (casterPawn.MentalStateDef != MentalStateDefOf.SocialFighting || pawn2.MentalStateDef != MentalStateDefOf.SocialFighting) && (casterPawn.story == null || !casterPawn.story.traits.DisableHostilityFrom(pawn2)))
            {
                pawn2.mindState.meleeThreat = casterPawn;
                pawn2.mindState.lastMeleeThreatHarmTick = Find.TickManager.TicksGame;
            }

            Map map = thing.Map;
            Vector3 drawPos = thing.DrawPos;

            // 건랜스는 해당 위치에 폭발을 일으키면 되서 "명중"판정이 없다.
            // 애니메이션과 이펙트만 처리
            SoundDef soundDef = this.SoundHitPawn();
            if (this.verbProps.impactMote != null)
            {
                MoteMaker.MakeStaticMote(drawPos, map, this.verbProps.impactMote, 1f);
            }
            if (this.verbProps.impactFleck != null)
            {
                FleckMaker.Static(drawPos, map, this.verbProps.impactFleck, 1f);
            }

            if (soundDef != null)
            {
                soundDef.PlayOneShot(new TargetInfo(thing.Position, map));
            }

            if (casterPawn.Spawned)
            {
                casterPawn.Drawer.Notify_MeleeAttackOn(thing);
            }

            if (pawn2 != null && !pawn2.Dead && pawn2.Spawned)
            {
                pawn2.stances.stagger.StaggerFor(95, 0.17f);
            }

            if (casterPawn.Spawned)
            {
                casterPawn.rotationTracker.FaceCell(thing.Position);
            }

            if (casterPawn.caller != null)
            {
                casterPawn.caller.Notify_DidMeleeAttack();
            }

            // 건랜스는 위치 폭발을 일으키므로 항상 성공
            // 모든 경우에 폭발 발생
            TriggerExplosion();

            return true;
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
            // 근접 데미지는 적용하지 않음
            return new DamageWorker.DamageResult();
        }

        private bool IsTargetImmobile(LocalTargetInfo target)
        {
            Thing thing = target.Thing;
            Pawn pawn = thing as Pawn;
            return pawn == null || pawn.Downed || pawn.GetPosture() > PawnPosture.Standing;
        }

        private SoundDef SoundHitPawn()
        {
            if (base.EquipmentSource != null && !base.EquipmentSource.def.meleeHitSound.NullOrUndefined())
            {
                return base.EquipmentSource.def.meleeHitSound;
            }
            if (base.EquipmentSource != null && base.EquipmentSource.Stuff != null)
            {
                if (this.verbProps.meleeDamageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)
                {
                    if (!base.EquipmentSource.Stuff.stuffProps.soundMeleeHitSharp.NullOrUndefined())
                    {
                        return base.EquipmentSource.Stuff.stuffProps.soundMeleeHitSharp;
                    }
                }
                else if (!base.EquipmentSource.Stuff.stuffProps.soundMeleeHitBlunt.NullOrUndefined())
                {
                    return base.EquipmentSource.Stuff.stuffProps.soundMeleeHitBlunt;
                }
            }
            if (this.CasterPawn != null && !this.CasterPawn.def.race.soundMeleeHitPawn.NullOrUndefined())
            {
                return this.CasterPawn.def.race.soundMeleeHitPawn;
            }
            return SoundDefOf.Pawn_Melee_Punch_HitPawn;
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

