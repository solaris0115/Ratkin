using Verse;

namespace NewRatkin
{
    public class CompProperties_ShieldFaceDirection : CompProperties
    {
        public CompProperties_ShieldFaceDirection()
        {
            compClass = typeof(CompShieldFaceDirection);
        }

        public int cooldownTicks = 300;
    }
}
