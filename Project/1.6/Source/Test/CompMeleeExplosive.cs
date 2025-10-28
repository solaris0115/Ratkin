using Verse;

namespace TestMod
{
    public class CompMeleeExplosive : ThingComp
    {
        public CompProperties_MeleeExplosive Props => 
            (CompProperties_MeleeExplosive)this.props;
    }
}

