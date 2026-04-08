using System.Linq;
using RimWorld;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// DeflectAngle은 Pawn 스탯. 방패 착용 시에만 표시.
    /// 실제 값 계산(skillNeedOffsets + postProcessCurve)은 기본 StatWorker가 처리.
    /// </summary>
    public class StatWorker_ShieldDeflectAngle : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            if (!base.ShouldShowFor(req))
                return false;

            if (req.Thing is Pawn pawn)
                return pawn.apparel?.WornApparel.OfType<ApparelShieldTowerSecond>().Any() == true;

            return false;
        }
    }
}
