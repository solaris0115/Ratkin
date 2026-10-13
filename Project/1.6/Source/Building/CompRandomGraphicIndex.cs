// [보류] 벽화: 등장 경로 미완료로 전용 그림 선택 코드를 임시 비활성화. 벽화 정의와 함께 복구.
#if false
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
#endif
