using System.Collections.Generic;
using RimWorld;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// Prototype Pulse Rifle 발사 모드 토글 Comp 속성
    /// 연사/단발별 탄환·사거리·버스트·조준·stat offset (Verb override로 적용, verbProps 교체 없음)
    /// </summary>
    public class CompProperties_PulseRifleFireMode : CompProperties
    {
        public ThingDef projectileBurst;
        public ThingDef projectileSingle;

        public float rangeBurst = 31f;
        public float rangeSingle = 31f;
        public int burstShotCountBurst = 3;
        public int burstShotCountSingle = 1;
        public int ticksBetweenBurstShotsBurst = 10;
        public int ticksBetweenBurstShotsSingle = 0;
        public float warmupTimeBurst = 1.5f;
        public float warmupTimeSingle = 2f;

        public List<StatModifier> statOffsetsBurst;
        public List<StatModifier> statOffsetsSingle;

        public string iconPathBurst = "UI/Commands/RK_Icon_BurstShot";
        public string iconPathSingle = "UI/Commands/RK_Icon_Snipe";

        public CompProperties_PulseRifleFireMode()
        {
            compClass = typeof(Comp_PulseRifleFireMode);
        }
    }
}
