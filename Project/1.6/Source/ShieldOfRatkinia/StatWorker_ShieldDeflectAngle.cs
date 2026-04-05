using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// 방패 statBases 반각 × 근접 스킬 배율. 폰 창에만 타워실드 착용 시 표시 (방패 미착용 폰은 숨김).
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
                return baseHalf * ShieldDeflectAngleMeleeCurve.Evaluate(melee);
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

                float baseHalf = base.GetBaseValueFor(StatRequest.For(shield));
                float melee = pawn.skills?.GetSkill(SkillDefOf.Melee)?.Level ?? 0f;
                float mult = ShieldDeflectAngleMeleeCurve.Evaluate(melee);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("shield: " + shield.LabelCap);
                sb.AppendLine("Base half-angle (item): " + baseHalf.ToStringByStyle(stat.ToStringStyleUnfinalized, numberSense));
                sb.AppendLine(string.Format("Melee ({0}): ×{1:F2}", melee, mult));
                sb.AppendLine("= " + (baseHalf * mult).ToStringByStyle(stat.ToStringStyleUnfinalized, numberSense));
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
