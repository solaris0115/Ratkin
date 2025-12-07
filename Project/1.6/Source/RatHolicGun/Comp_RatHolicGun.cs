using RimWorld;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// RatHolic Gun용 Comp 속성
    /// </summary>
    public class CompProperties_RatHolicGun : CompProperties
    {
        /// <summary>
        /// Spooling 효과를 위한 HediffDef
        /// XML에서 설정 가능: &lt;hediffDef&gt;RK_Hediff_RatHolicGunSpooling&lt;/hediffDef&gt;
        /// </summary>
        public HediffDef hediffDef;

        /// <summary>
        /// 최대 중첩 수
        /// XML에서 설정 가능: &lt;maxStacks&gt;5&lt;/maxStacks&gt;
        /// </summary>
        public int maxStacks = 5;

        public CompProperties_RatHolicGun()
        {
            this.compClass = typeof(Comp_RatHolicGun);
        }
    }

    /// <summary>
    /// RatHolic Gun용 Comp - 장착/해제 시 Spooling Hediff 관리
    /// </summary>
    public class Comp_RatHolicGun : ThingComp
    {
        public CompProperties_RatHolicGun Props => 
            (CompProperties_RatHolicGun)this.props;

        /// <summary>
        /// 무기 장착 시 기존 Spooling Hediff 제거
        /// </summary>
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            RemoveAllSpoolingHediffs(pawn);
        }

        /// <summary>
        /// 무기 해제 시 Spooling Hediff 제거
        /// </summary>
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            RemoveAllSpoolingHediffs(pawn);
        }

        /// <summary>
        /// Pawn의 Spooling Hediff 제거 (하나만 존재)
        /// </summary>
        private void RemoveAllSpoolingHediffs(Pawn pawn)
        {
            if (Props?.hediffDef == null || pawn?.health?.hediffSet == null)
            {
                return;
            }

            // HediffSet에서 해당 Hediff를 찾아서 제거
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }
    }
}

