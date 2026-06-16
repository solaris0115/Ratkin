using Verse;

namespace NewRatkin
{
    public class CompProperties_RandomGraphicIndex : CompProperties
    {
        public int variantCount;

        public CompProperties_RandomGraphicIndex()
        {
            compClass = typeof(CompRandomGraphicIndex);
        }
    }

    public class CompRandomGraphicIndex : ThingComp
    {
        private CompProperties_RandomGraphicIndex Props => (CompProperties_RandomGraphicIndex)props;

        public override void PostPostMake()
        {
            base.PostPostMake();
            AssignIndexIfMissing();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            AssignIndexIfMissing();
        }

        private void AssignIndexIfMissing()
        {
            if (parent.overrideGraphicIndex.HasValue)
            {
                return;
            }

            int count = Props.variantCount;
            if (count <= 0 && parent.def.graphicData?.Graphic is Graphic_Random randomGraphic)
            {
                count = randomGraphic.SubGraphicsCount;
            }

            if (count > 1)
            {
                parent.overrideGraphicIndex = Rand.Range(0, count);
            }
        }
    }
}
