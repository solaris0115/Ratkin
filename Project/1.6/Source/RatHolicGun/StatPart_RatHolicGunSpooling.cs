using RimWorld;
using System;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// RatHolic Gun Spooling 효과를 위한 StatPart
    /// RK_Stat_RangeCoolDown 스탯 값을 가져와서 쿨다운에서 직접 감소
    /// </summary>
    public class StatPart_RatHolicGunSpooling : StatPart
    {
        /// <summary>
        /// RK_Stat_RangeCoolDown 스탯 정의
        /// </summary>
        private static StatDef RK_Stat_RangeCoolDown;

        static StatPart_RatHolicGunSpooling()
        {
            // 스탯 정의를 한 번만 로드
            RK_Stat_RangeCoolDown = DefDatabase<StatDef>.GetNamedSilentFail("RK_Stat_RangeCoolDown");
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            // RK_Stat_RangeCoolDown 스탯이 없으면 무시
            if (RK_Stat_RangeCoolDown == null)
            {
                return;
            }

            // Pawn이 아니면 무시
            if (!req.HasThing || !(req.Thing is Pawn pawn))
            {
                return;
            }

            // RK_Stat_RangeCoolDown 스탯 값 가져오기 (Hediff의 statOffsets에서 설정된 값)
            float cooldownReduction = pawn.GetStatValue(RK_Stat_RangeCoolDown, true);
            
            // 값이 0이면 무시
            if (cooldownReduction == 0f)
            {
                return;
            }

            // RangedCooldownFactor는 factor를 곱하는 방식이므로,
            // offset을 적용하려면 무기 쿨다운을 가져와서 계산해야 함
            ThingWithComps weapon = pawn.equipment?.Primary;
            if (weapon == null)
            {
                return;
            }

            float weaponCooldown = weapon.GetStatValue(StatDefOf.RangedWeapon_Cooldown, true);
            if (weaponCooldown <= 0f)
            {
                return;
            }

            // factor 감소량 계산: (감소할 시간) / (무기 cooldown)
            // 예: 무기 cooldown 1.5초, 감소량 1.0초 → factor 감소량 = 1.0 / 1.5 = 0.667
            float reductionAmount = cooldownReduction / weaponCooldown;

            // factor에서 감소 (최소값 0.01로 제한)
            val = System.Math.Max(0.01f, val - reductionAmount);
        }

        public override string ExplanationPart(StatRequest req)
        {
            // RK_Stat_RangeCoolDown 스탯이 없으면 설명 없음
            if (RK_Stat_RangeCoolDown == null)
            {
                return null;
            }

            if (!req.HasThing || !(req.Thing is Pawn pawn))
            {
                return null;
            }

            // RK_Stat_RangeCoolDown 스탯 값 가져오기
            float cooldownReduction = pawn.GetStatValue(RK_Stat_RangeCoolDown, true);
            
            if (cooldownReduction == 0f)
            {
                return null;
            }

            ThingWithComps weapon = pawn.equipment?.Primary;
            if (weapon == null)
            {
                return null;
            }

            float weaponCooldown = weapon.GetStatValue(StatDefOf.RangedWeapon_Cooldown, true);
            if (weaponCooldown <= 0f)
            {
                return null;
            }

            float reductionPercent = (cooldownReduction / weaponCooldown) * 100f;

            return $"RatHolic Gun Spooling: -{cooldownReduction:F2}s cooldown ({reductionPercent:F1}% faster)";
        }
    }
}

