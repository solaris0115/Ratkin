using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// v2: 관통 0 기준 표시값 = Sharp/Blunt/Heat 각각 BlockChance의 산술 평균 (Gumbel + M 커브).
    /// </summary>
    public class StatWorker_ShieldDeflectChance : StatWorker
    {
        private const float ReferenceArmorPenetration = 0f;

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

            return AverageBlockChanceAtApZero(shield, pawn);
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

            float meleeLevel = pawn.skills?.GetSkill(SkillDefOf.Melee)?.Level ?? 0f;
            float M = ApparelShieldTowerSecond.ComputeM(meleeLevel);

            float armorSharp = shield.GetStatValue(RatkinStatDefOf.RK_Stat_Shield_Sharp);
            float armorBlunt = shield.GetStatValue(RatkinStatDefOf.RK_Stat_Shield_Blunt);
            float armorHeat = shield.GetStatValue(RatkinStatDefOf.RK_Stat_Shield_Heat);

            float bcSharp = ApparelShieldTowerSecond.ComputeBlockChanceForArmorAndMelee(
                armorSharp, meleeLevel, ReferenceArmorPenetration);
            float bcBlunt = ApparelShieldTowerSecond.ComputeBlockChanceForArmorAndMelee(
                armorBlunt, meleeLevel, ReferenceArmorPenetration);
            float bcHeat = ApparelShieldTowerSecond.ComputeBlockChanceForArmorAndMelee(
                armorHeat, meleeLevel, ReferenceArmorPenetration);
            float avg = (bcSharp + bcBlunt + bcHeat) / 3f;

            float dSharp = armorSharp - ReferenceArmorPenetration;
            float dBlunt = armorBlunt - ReferenceArmorPenetration;
            float dHeat = armorHeat - ReferenceArmorPenetration;
            float zSharp = dSharp + ApparelShieldTowerSecond.DeflectV2_a * M - ApparelShieldTowerSecond.DeflectV2_b;
            float zBlunt = dBlunt + ApparelShieldTowerSecond.DeflectV2_a * M - ApparelShieldTowerSecond.DeflectV2_b;
            float zHeat = dHeat + ApparelShieldTowerSecond.DeflectV2_a * M - ApparelShieldTowerSecond.DeflectV2_b;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("방패 방어력 (S, 타입별)");
            sb.AppendLine(shield.LabelCap);
            if (shield.TryGetQuality(out QualityCategory qc))
            {
                sb.AppendLine("QualityIs".Translate(qc.GetLabel()));
            }
            sb.AppendLine("방어도 - 날카로움: " + FmtLikeStatTable(armorSharp, numberSense));
            sb.AppendLine("방어도 - 둔탁함: " + FmtLikeStatTable(armorBlunt, numberSense));
            sb.AppendLine("방어도 - 열기: " + FmtLikeStatTable(armorHeat, numberSense));
            sb.AppendLine();

            sb.AppendLine("근접 스킬 → M (선형·지수 블렌드)");
            sb.AppendLine(SkillDefOf.Melee.LabelCap + ": " + meleeLevel.ToString("F0") + " → M=" + M.ToString("F3"));
            sb.AppendLine();

            sb.AppendLine("참조: 관통 P=0");
            sb.AppendLine(
                "D = S − P, z = D + " + ApparelShieldTowerSecond.DeflectV2_a.ToString("F2") + "·M − " +
                ApparelShieldTowerSecond.DeflectV2_b.ToString("F2"));
            sb.AppendLine(
                "BlockChance = " + ApparelShieldTowerSecond.DeflectV2_Cmin.ToString("F2") + " + (" +
                ApparelShieldTowerSecond.DeflectV2_Cmax.ToString("F2") + " − " +
                ApparelShieldTowerSecond.DeflectV2_Cmin.ToString("F2") + ")·exp(−exp(−" +
                ApparelShieldTowerSecond.DeflectV2_k.ToString("F2") + "·z))");
            sb.AppendLine();
            sb.AppendLine("날카로움: D=" + dSharp.ToString("F3") + ", z=" + zSharp.ToString("F3") + " → " + FmtLikeStatTable(bcSharp, numberSense));
            sb.AppendLine("둔탁함: D=" + dBlunt.ToString("F3") + ", z=" + zBlunt.ToString("F3") + " → " + FmtLikeStatTable(bcBlunt, numberSense));
            sb.AppendLine("열기: D=" + dHeat.ToString("F3") + ", z=" + zHeat.ToString("F3") + " → " + FmtLikeStatTable(bcHeat, numberSense));
            sb.AppendLine();
            sb.AppendLine("Average".Translate() + " (AP " + ReferenceArmorPenetration.ToString("F0") + "): " + FmtLikeStatTable(avg, numberSense));

            return sb.ToString();
        }

        /// <summary>코어가 붙이는 "최종값" 줄 생략 — 위에 평균 한 줄로 충분.</summary>
        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            return string.Empty;
        }

        /// <summary>관통 0일 때 Sharp/Blunt/Heat 각 BlockChance의 산술 평균.</summary>
        private static float AverageBlockChanceAtApZero(ApparelShieldTowerSecond shield, Pawn pawn)
        {
            float meleeLevel = pawn.skills?.GetSkill(SkillDefOf.Melee)?.Level ?? 0f;
            float aS = shield.GetStatValue(RatkinStatDefOf.RK_Stat_Shield_Sharp);
            float aB = shield.GetStatValue(RatkinStatDefOf.RK_Stat_Shield_Blunt);
            float aH = shield.GetStatValue(RatkinStatDefOf.RK_Stat_Shield_Heat);
            float bcS = ApparelShieldTowerSecond.ComputeBlockChanceForArmorAndMelee(aS, meleeLevel, ReferenceArmorPenetration);
            float bcB = ApparelShieldTowerSecond.ComputeBlockChanceForArmorAndMelee(aB, meleeLevel, ReferenceArmorPenetration);
            float bcH = ApparelShieldTowerSecond.ComputeBlockChanceForArmorAndMelee(aH, meleeLevel, ReferenceArmorPenetration);
            return (bcS + bcB + bcH) / 3f;
        }

        private static ApparelShieldTowerSecond GetShield(Pawn pawn)
        {
            if (pawn?.apparel == null)
                return null;
            return pawn.apparel.WornApparel.OfType<ApparelShieldTowerSecond>().FirstOrDefault();
        }
    }
}
