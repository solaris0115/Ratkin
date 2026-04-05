using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// 스탯 값: 세 유형 (방어+숙련) 합의 평균. 상한 클램프 없음(AP 0 기준 표시, 실전은 타입별 방어+숙련−관통).
    /// </summary>
    public class StatWorker_ShieldDeflectChance : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            return base.ShouldShowFor(req) && req.Thing is Pawn pawn && GetShield(pawn) != null;
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn == null)
                return 0f;

            var shield = GetShield(pawn);
            if (shield == null)
                return 0f;

            return AverageOfArmorPlusHandling(shield, pawn);
        }

        /// <summary>스탯 패널과 동일: StatWorker.ValueToString(PercentZero), 상한 클램프 없음.</summary>
        private string FmtLikeStatTable(float v, ToStringNumberSense numberSense)
        {
            return this.ValueToString(v, true, numberSense);
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn == null)
                return base.GetExplanationUnfinalized(req, numberSense);

            var shield = GetShield(pawn);
            if (shield == null)
                return base.GetExplanationUnfinalized(req, numberSense);

            float handling = pawn.GetStatValue(RatkinStatDefOf.RK_Stat_ShieldHandling);

            float armorSharp = shield.GetStatValue(StatDefOf.ArmorRating_Sharp);
            float armorBlunt = shield.GetStatValue(StatDefOf.ArmorRating_Blunt);
            float armorHeat = shield.GetStatValue(StatDefOf.ArmorRating_Heat);

            float sumSharp = armorSharp + handling;
            float sumBlunt = armorBlunt + handling;
            float sumHeat = armorHeat + handling;
            float avg = (sumSharp + sumBlunt + sumHeat) / 3f;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("방패 방어력");
            sb.AppendLine(shield.LabelCap);
            if (shield.TryGetQuality(out QualityCategory qc))
            {
                sb.AppendLine("QualityIs".Translate(qc.GetLabel()));
            }
            sb.AppendLine("방어도 - 날카로움: " + FmtLikeStatTable(armorSharp, numberSense));
            sb.AppendLine("방어도 - 둔탁함: " + FmtLikeStatTable(armorBlunt, numberSense));
            sb.AppendLine("방어도 - 열기: " + FmtLikeStatTable(armorHeat, numberSense));
            sb.AppendLine();

            sb.AppendLine("방패 숙련");
            // 나이(StatPart_Age)·후처리 곡선 등은 코어에서 GetExplanationFinalizePart에만 붙음 → Full 사용.
            StatDef hStat = RatkinStatDefOf.RK_Stat_ShieldHandling;
            float hVal = pawn.GetStatValue(hStat);
            string handlingExplain = hStat.Worker
                .GetExplanationFull(StatRequest.For(pawn), hStat.toStringNumberSense, hVal)
                .TrimEndNewlines();
            if (!handlingExplain.NullOrEmpty())
            {
                sb.AppendLine(handlingExplain);
            }
            sb.AppendLine();

            sb.AppendLine("최종 확률");
            sb.AppendLine("날카로움: " + FmtLikeStatTable(sumSharp, numberSense));
            sb.AppendLine("둔탁함: " + FmtLikeStatTable(sumBlunt, numberSense));
            sb.AppendLine("열기: " + FmtLikeStatTable(sumHeat, numberSense));
            sb.AppendLine("Average".Translate() + ": " + FmtLikeStatTable(avg, numberSense));

            return sb.ToString();
        }

        /// <summary>코어가 붙이는 "최종값" 줄 생략 — 위에 평균 한 줄로 충분.</summary>
        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            return string.Empty;
        }

        /// <summary>세 피해 유형 각각 (방패 방어 + 숙련) 합의 산술 평균.</summary>
        private static float AverageOfArmorPlusHandling(ApparelShieldTowerSecond shield, Pawn pawn)
        {
            float h = pawn.GetStatValue(RatkinStatDefOf.RK_Stat_ShieldHandling);
            float aS = shield.GetStatValue(StatDefOf.ArmorRating_Sharp);
            float aB = shield.GetStatValue(StatDefOf.ArmorRating_Blunt);
            float aH = shield.GetStatValue(StatDefOf.ArmorRating_Heat);
            return ((aS + h) + (aB + h) + (aH + h)) / 3f;
        }

        private static ApparelShieldTowerSecond GetShield(Pawn pawn)
        {
            if (pawn?.apparel == null)
                return null;
            return pawn.apparel.WornApparel.OfType<ApparelShieldTowerSecond>().FirstOrDefault();
        }
    }
}
