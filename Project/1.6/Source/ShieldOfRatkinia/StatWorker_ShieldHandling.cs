using RimWorld;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// 방패 다루기: 중간 계산은 raw, 툴팁 최종값만 퍼센트.
    /// </summary>
    public class StatWorker_ShieldHandling : StatWorker
    {
        public override string ValueToString(float val, bool finalized, ToStringNumberSense numberSense = ToStringNumberSense.Absolute)
        {
            if (!finalized)
            {
                return val.ToStringByStyle(ToStringStyle.FloatOne, numberSense);
            }
            return base.ValueToString(val, finalized, numberSense);
        }
    }
}
