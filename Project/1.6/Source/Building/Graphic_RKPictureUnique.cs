using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace NewRatkin
{
    public class Graphic_RKPictureUniqueRandom : Graphic_Random
    {
        private static readonly string[] DirectionSuffixes =
        {
            Graphic_Multi.NorthSuffix,
            Graphic_Multi.EastSuffix,
            Graphic_Multi.SouthSuffix,
            Graphic_Multi.WestSuffix
        };

        public override void Init(GraphicRequest req)
        {
            data = req.graphicData;
            path = req.path;
            maskPath = req.maskPath;
            color = req.color;
            colorTwo = req.colorTwo;
            drawSize = req.drawSize;

            List<string> variantPaths = VariantPathsFor(req.path).ToList();
            if (variantPaths.Count == 0)
            {
                Log.Error("RK picture graphic cannot init: No textures found for " + req.path);
                subGraphics = new Graphic[] { BaseContent.BadGraphic };
                return;
            }

            subGraphics = variantPaths
                .Select(variantPath => GraphicDatabase.Get(
                    typeof(Graphic_RKPictureUniqueVariant),
                    variantPath,
                    req.shader,
                    drawSize,
                    color,
                    colorTwo,
                    data,
                    req.shaderParameters,
                    maskPath))
                .ToArray();
        }

        public static int VariantCountFor(string texPath)
        {
            return VariantPathsFor(texPath).Count();
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_RKPictureUniqueRandom>(path, newShader, drawSize, newColor, newColorTwo, data, maskPath);
        }

        private static IEnumerable<string> VariantPathsFor(string texPath)
        {
            if (texPath.NullOrEmpty())
            {
                yield break;
            }

            SplitTexPath(texPath, out string folderPath, out string prefix);
            SortedSet<string> variantNames = new SortedSet<string>(StringComparer.Ordinal);

            foreach (Texture2D texture in ContentFinder<Texture2D>.GetAllInFolder(folderPath))
            {
                string name = texture.name;
                if (name.EndsWith(Graphic_Single.MaskSuffix, StringComparison.Ordinal))
                {
                    continue;
                }

                string variantName = StripDirectionSuffix(name);
                if (variantName.StartsWith(prefix, StringComparison.Ordinal))
                {
                    variantNames.Add(variantName);
                }
            }

            foreach (string variantName in variantNames)
            {
                yield return folderPath + "/" + variantName;
            }
        }

        private static void SplitTexPath(string texPath, out string folderPath, out string prefix)
        {
            int slashIndex = texPath.LastIndexOf('/');
            if (slashIndex < 0)
            {
                folderPath = texPath;
                prefix = string.Empty;
                return;
            }

            folderPath = texPath.Substring(0, slashIndex);
            prefix = texPath.Substring(slashIndex + 1);
        }

        private static string StripDirectionSuffix(string name)
        {
            foreach (string suffix in DirectionSuffixes)
            {
                if (name.EndsWith(suffix, StringComparison.Ordinal))
                {
                    return name.Substring(0, name.Length - suffix.Length);
                }
            }

            return name;
        }
    }

    public class Graphic_RKPictureUniqueVariant : Graphic_Multi
    {
        private Material itemMat;

        public override Material MatSingle => itemMat ?? base.MatSingle;

        public override void Init(GraphicRequest req)
        {
            base.Init(req);

            Texture2D itemTexture = ContentFinder<Texture2D>.Get(req.path, reportFailure: false);
            if (itemTexture == null)
            {
                return;
            }

            Texture2D maskTexture = null;
            if (req.shader.SupportsMaskTex())
            {
                string itemMaskPath = req.maskPath.NullOrEmpty() ? req.path + Graphic_Single.MaskSuffix : req.maskPath;
                maskTexture = ContentFinder<Texture2D>.Get(itemMaskPath, reportFailure: false);
            }

            itemMat = MaterialPool.MatFrom(new MaterialRequest
            {
                mainTex = itemTexture,
                shader = req.shader,
                color = color,
                colorTwo = colorTwo,
                maskTex = maskTexture,
                renderQueue = req.renderQueue,
                shaderParameters = req.shaderParameters
            });
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            if (thing is MinifiedThing && itemMat != null)
            {
                return itemMat;
            }

            return base.MatAt(rot, thing);
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_RKPictureUniqueVariant>(path, newShader, drawSize, newColor, newColorTwo, data, maskPath);
        }

        public override Graphic GetCopy(Vector2 newDrawSize, Shader overrideShader)
        {
            return GraphicDatabase.Get<Graphic_RKPictureUniqueVariant>(path, overrideShader ?? Shader, newDrawSize, color, colorTwo, data, maskPath);
        }

        public override void TryInsertIntoAtlas(TextureAtlasGroup groupKey)
        {
            base.TryInsertIntoAtlas(groupKey);
            if (itemMat?.mainTexture is Texture2D texture)
            {
                Texture2D mask = null;
                if (itemMat.HasProperty(ShaderPropertyIDs.MaskTex))
                {
                    mask = itemMat.GetTexture(ShaderPropertyIDs.MaskTex) as Texture2D;
                }

                GlobalTextureAtlasManager.TryInsertStatic(groupKey, texture, mask);
            }
        }
    }

    public class CompProperties_RKRandomGraphicIndex : CompProperties
    {
        public int variantCount;

        public CompProperties_RKRandomGraphicIndex()
        {
            compClass = typeof(CompRKRandomGraphicIndex);
        }
    }

    public class CompRKRandomGraphicIndex : ThingComp
    {
        private CompProperties_RKRandomGraphicIndex Props => (CompProperties_RKRandomGraphicIndex)props;

        public override void PostPostMake()
        {
            base.PostPostMake();
            AssignGraphicIndexIfMissing();
        }

        private void AssignGraphicIndexIfMissing()
        {
            if (parent.overrideGraphicIndex.HasValue)
            {
                return;
            }

            int count = Props.variantCount > 0
                ? Props.variantCount
                : Graphic_RKPictureUniqueRandom.VariantCountFor(parent.def.graphicData?.texPath);

            if (count > 1)
            {
                parent.overrideGraphicIndex = Rand.Range(0, count);
            }
        }
    }
}
