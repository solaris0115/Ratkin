using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace NewRatkin
{
    public class MinifiedThing_PictureUnique : MinifiedThing
    {
        public override Graphic Graphic
        {
            get
            {
                if (cachedGraphic == null)
                {
                    Thing innerThing = InnerThing;
                    string path = ItemGraphicPathFor(innerThing);
                    if (path.NullOrEmpty())
                    {
                        return base.Graphic;
                    }

                    cachedGraphic = GraphicDatabase.Get<Graphic_Single>(
                        path,
                        ShaderDatabase.Cutout,
                        MinifiedGraphicDrawSize(innerThing),
                        innerThing.DrawColor,
                        innerThing.DrawColorTwo);
                }

                return cachedGraphic;
            }
        }

        private static string ItemGraphicPathFor(Thing innerThing)
        {
            string basePath = innerThing?.def?.uiIconPath;
            if (basePath.NullOrEmpty())
            {
                return null;
            }

            int graphicIndex = innerThing.OverrideGraphicIndex ?? 0;
            if (graphicIndex <= 0)
            {
                return basePath;
            }

            string variantPath = basePath + (graphicIndex + 1);
            return ContentFinder<Texture2D>.Get(variantPath, reportFailure: false) != null ? variantPath : basePath;
        }

        private Vector2 MinifiedGraphicDrawSize(Thing innerThing)
        {
            Vector2 drawSize = Vector2.one;
            if ((float)innerThing.def.size.x > MaxMinifiedGraphicSize || (float)innerThing.def.size.z > MaxMinifiedGraphicSize)
            {
                Vector2 minifiedDrawSize = GetMinifiedDrawSize(innerThing.def.size.ToVector2(), MaxMinifiedGraphicSize);
                drawSize = new Vector2(
                    minifiedDrawSize.x / (float)innerThing.def.size.x * drawSize.x,
                    minifiedDrawSize.y / (float)innerThing.def.size.z * drawSize.y);
            }

            if (Math.Abs(innerThing.def.minifiedDrawScale - 1f) > float.Epsilon)
            {
                drawSize = new Vector2(
                    innerThing.def.minifiedDrawScale * drawSize.x,
                    innerThing.def.minifiedDrawScale * drawSize.y);
            }

            return drawSize;
        }
    }
}
