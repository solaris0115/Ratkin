using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// statBases에 풀각(좌+우 합산, 도)으로 세팅.
    /// 내부 판정(ApparelShieldTowerSecond)은 /2 하여 반각 사용.
    /// UI에는 풀각을 그대로 표시하고, 근접 스킬 배율을 적용한 유효 풀각을 보여줌.
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

                float baseFull = base.GetBaseValueFor(StatRequest.For(shield));
                float melee = pawn.skills?.GetSkill(SkillDefOf.Melee)?.Level ?? 0f;
                float effectiveFull = baseFull * ShieldDeflectAngleMeleeCurve.Evaluate(melee);
                return effectiveFull;
            }

            return base.GetValueUnfinalized(req, applyPostProcess);
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            if (req.Thing is Pawn pawn)
            {
                var shield = GetTowerShield(pawn);
                if (shield == null)
                    return base.GetExplanationUnfinalized(req, numberSense);

                float baseFull = base.GetBaseValueFor(StatRequest.For(shield));
                float melee = pawn.skills?.GetSkill(SkillDefOf.Melee)?.Level ?? 0f;
                float mult = ShieldDeflectAngleMeleeCurve.Evaluate(melee);
                float effectiveFull = baseFull * mult;
                float effectiveHalf = effectiveFull * 0.5f;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("shield: " + shield.LabelCap);
                sb.AppendLine("Base deflect angle: " + baseFull.ToStringByStyle(stat.ToStringStyleUnfinalized, numberSense) + "°");
                sb.AppendLine(string.Format("Melee ({0}): ×{1:F2}", melee, mult));
                sb.AppendLine("Effective deflect angle: " + effectiveFull.ToStringByStyle(stat.ToStringStyleUnfinalized, numberSense) + "°");
                sb.AppendLine("(±" + effectiveHalf.ToStringByStyle(stat.ToStringStyleUnfinalized, numberSense) + "° each side)");
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
