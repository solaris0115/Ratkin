using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// Pawn 전투 스탯 — 방패 블록 시도 성공률 (1단계 게이트).
    /// MeleeHitChance와 동일한 구조: 근접 스킬 + 조작 + 시야 → postProcessCurve → 0~1.
    /// 방패 아머 스탯(2단계)과 무관하며, "이 공격을 블락 시도하는가"만 결정한다.
    /// </summary>
    public class StatWorker_ShieldBlockChance : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            if (!base.ShouldShowFor(req))
                return false;

            if (req.Thing is Pawn pawn)
                return GetShield(pawn) != null;

            return false;
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if (req.Thing is Pawn pawn)
            {
                if (GetShield(pawn) == null) return 0f;
                // 순수 스킬/조작/시야 팩터. 방패 아머 미포함.
                return base.GetValueUnfinalized(req, applyPostProcess);
            }

            return base.GetValueUnfinalized(req, applyPostProcess);
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            if (req.Thing is Pawn pawn)
            {
                var shield = GetShield(pawn);
                if (shield == null)
                    return base.GetExplanationUnfinalized(req, numberSense);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(base.GetExplanationUnfinalized(req, numberSense).TrimEnd());
                sb.AppendLine("---");
                sb.AppendLine("Shield: " + shield.LabelCap);
                sb.AppendLine($"  Sharp  armor: {shield.GetStatValue(RatkinStatDefOf.RK_Stat_Shield_Sharp).ToStringPercent()}");
                sb.AppendLine($"  Blunt  armor: {shield.GetStatValue(RatkinStatDefOf.RK_Stat_Shield_Blunt).ToStringPercent()}");
                sb.AppendLine($"  Heat   armor: {shield.GetStatValue(RatkinStatDefOf.RK_Stat_Shield_Heat).ToStringPercent()}");
                sb.Append("(Shield armor used in stage-2 armor roll, not here)");
                return sb.ToString();
            }

            return base.GetExplanationUnfinalized(req, numberSense);
        }

        private static ApparelShieldTowerSecond GetShield(Pawn pawn)
        {
            if (pawn?.apparel == null) return null;
            return pawn.apparel.WornApparel.OfType<ApparelShieldTowerSecond>().FirstOrDefault();
        }
    }
}
