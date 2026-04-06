using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// 방패 statBases 반각 × 근접 스킬 배율을 내부에서 계산한 뒤, UI에는 좌·우 합산 호(정면 기준 총 각도)로 노출.
    /// 실제 판정(ApparelShieldTowerSecond)은 반각을 그대로 사용.
    /// </summary>
    public class StatWorker_ShieldDeflectAngle : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            if (!base.ShouldShowFor(req))
                return false;

            if (req.Thing is Pawn pawn)
                return GetTowerShield(pawn) != null;

            return true;
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if (req.Thing is Pawn pawn)
            {
                var shield = GetTowerShield(pawn);
                if (shield == null)
                    return 0f;

                float baseHalf = base.GetBaseValueFor(StatRequest.For(shield));
                float melee = pawn.skills?.GetSkill(SkillDefOf.Melee)?.Level ?? 0f;
                float halfEffective = baseHalf * ShieldDeflectAngleMeleeCurve.Evaluate(melee);
                return 2f * halfEffective;
            }

            float half = base.GetValueUnfinalized(req, applyPostProcess);
            return 2f * half;
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            if (req.Thing is Pawn pawn)
            {
                var shield = GetTowerShield(pawn);
                if (shield == null)
                    return base.GetExplanationUnfinalized(req, numberSense);

                float baseHalf = base.GetBaseValueFor(StatRequest.For(shield));
                float melee = pawn.skills?.GetSkill(SkillDefOf.Melee)?.Level ?? 0f;
                float mult = ShieldDeflectAngleMeleeCurve.Evaluate(melee);
                float halfEffective = baseHalf * mult;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("shield: " + shield.LabelCap);
                sb.AppendLine("Base half-angle (item): " + baseHalf.ToStringByStyle(stat.ToStringStyleUnfinalized, numberSense));
                sb.AppendLine(string.Format("Melee ({0}): ×{1:F2}", melee, mult));
                sb.AppendLine("Effective half-angle (±): " + halfEffective.ToStringByStyle(stat.ToStringStyleUnfinalized, numberSense));
                sb.AppendLine("Total frontal arc (left+right): " + (2f * halfEffective).ToStringByStyle(stat.ToStringStyleUnfinalized, numberSense));
                return sb.ToString();
            }

            return base.GetExplanationUnfinalized(req, numberSense);
        }

        private static ApparelShieldTowerSecond GetTowerShield(Pawn pawn)
        {
            if (pawn?.apparel == null)
                return null;
            return pawn.apparel.WornApparel.OfType<ApparelShieldTowerSecond>().FirstOrDefault();
        }
    }
}
