using System.Linq;
using RimWorld;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// DeflectAngle 표시 Worker.
    /// - 방패 아이템 창: equippedStatOffsets 원본 값(°) 표시
    /// - 폰 스탯 창: 방패 착용 시 표시 (pawn 계산값)
    /// </summary>
    public class StatWorker_ShieldDeflectAngle : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            if (req.Thing is ApparelShieldTowerSecond)
                return true;

            if (!base.ShouldShowFor(req))
                return false;

            if (req.Thing is Pawn pawn)
                return pawn.apparel?.WornApparel.OfType<ApparelShieldTowerSecond>().Any() == true;

            return false;
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if (req.Thing is ApparelShieldTowerSecond shield)
                return shield.def.equippedStatOffsets?.GetStatOffsetFromList(stat) ?? 0f;

            return base.GetValueUnfinalized(req, applyPostProcess);
        }
    }
}
