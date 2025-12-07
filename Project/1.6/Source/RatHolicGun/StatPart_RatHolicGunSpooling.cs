using RimWorld;
using System;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// RatHolic Gun Spooling 효과를 위한 StatPart
    /// Hediff의 중첩 수에 따라 정확히 cooldown 감소
    /// </summary>
    public class StatPart_RatHolicGunSpooling : StatPart
    {
        /// <summary>
        /// Spooling 효과를 위한 HediffDef
        /// XML에서 설정 가능: &lt;hediffDef&gt;RK_Hediff_RatHolicGunSpooling&lt;/hediffDef&gt;
        /// </summary>
        public HediffDef hediffDef;
        
        /// <summary>
        /// 중첩당 감소할 cooldown 시간 (초 단위)
        /// XML에서 설정 가능: &lt;reductionPerStack&gt;0.3&lt;/reductionPerStack&gt;
        /// </summary>
        public float reductionPerStack = 0.3f;
        
        /// <summary>
        /// 최대 중첩 수
        /// XML에서 설정 가능: &lt;maxStacks&gt;5&lt;/maxStacks&gt;
        /// </summary>
        public int maxStacks = 5;

        public override void TransformValue(StatRequest req, ref float val)
        {
            // 최적화: HediffDef가 없으면 즉시 반환
            if (hediffDef == null)
            {
                return;
            }

            // Pawn이 아니면 무시
            if (!req.HasThing || !(req.Thing is Pawn pawn))
            {
                return;
            }

            // Hediff가 없으면 무시 (최적화)
            Hediff hediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(hediffDef);
            if (hediff == null)
            {
                return;
            }

            // 무기 확인
            ThingWithComps weapon = pawn.equipment?.Primary;
            if (weapon == null)
            {
                return;
            }

            // 무기의 실제 cooldown 가져오기
            float weaponCooldown = weapon.GetStatValue(StatDefOf.RangedWeapon_Cooldown);
            if (weaponCooldown <= 0f)
            {
                return; // 무효한 cooldown
            }

            // 현재 스택 수 계산 (Severity: 0.2 = 1스택, 0.4 = 2스택, ..., 1.0 = maxStacks)
            int stackCount = System.Math.Min(maxStacks, (int)System.Math.Ceiling(hediff.Severity * maxStacks));
            if (stackCount <= 0)
            {
                return; // 스택이 없으면 무시
            }

            // factor 감소량 계산: (감소할 시간) / (무기 cooldown)
            // 예: 무기 cooldown 1.6초, 1스택(0.3초 감소) → factor 감소량 = 0.3 / 1.6 = 0.1875
            float reductionAmount = (reductionPerStack * stackCount) / weaponCooldown;

            // factor에서 감소 (최소값 0.01로 제한)
            val = System.Math.Max(0.01f, val - reductionAmount);
        }

        public override string ExplanationPart(StatRequest req)
        {
            // 최적화: HediffDef가 없으면 설명 없음
            if (hediffDef == null)
            {
                return null;
            }

            if (!req.HasThing || !(req.Thing is Pawn pawn))
            {
                return null;
            }

            Hediff hediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(hediffDef);
            if (hediff == null)
            {
                return null;
            }

            ThingWithComps weapon = pawn.equipment?.Primary;
            if (weapon == null)
            {
                return null;
            }

            float weaponCooldown = weapon.GetStatValue(StatDefOf.RangedWeapon_Cooldown);
            if (weaponCooldown <= 0f)
            {
                return null;
            }

            int stackCount = System.Math.Min(maxStacks, (int)System.Math.Ceiling(hediff.Severity * maxStacks));
            if (stackCount <= 0)
            {
                return null;
            }

            float reduction = reductionPerStack * stackCount;
            float reductionPercent = (reduction / weaponCooldown) * 100f;

            return $"RatHolic Gun Spooling ({stackCount}/{maxStacks}): -{reduction:F1}s cooldown ({reductionPercent:F1}% faster)";
        }
    }
}

