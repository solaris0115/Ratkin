using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace NewRatkin
{
    /// <summary>
    /// ThingDef에 붙이면 <c>requiredGenes</c>를 모두 활성 보유한 폰만 착용·장착 가능 (Biotech + XML 패치로 race restriction 해제 시 사용).
    /// </summary>
    public class GeneEquipRestriction : DefModExtension
    {
        public List<GeneDef> requiredGenes;

        public bool Configured => !requiredGenes.NullOrEmpty();
    }

    /// <summary>
    /// 로드 시 한 번 구축하는 조회용 캐시 (런타임에 GetModExtension 반복 호출 방지).
    /// </summary>
    [StaticConstructorOnStartup]
    public static class GeneEquipRestrictionRegistry
    {
        private static readonly Dictionary<ThingDef, List<GeneDef>> requiredGenesByThingDef = new Dictionary<ThingDef, List<GeneDef>>();

        private static readonly Dictionary<ApparelProperties, ThingDef> thingDefByApparelProps =
            new Dictionary<ApparelProperties, ThingDef>();

        static GeneEquipRestrictionRegistry()
        {
            Rebuild();
            ApplyHarmonyPatches();
        }

        public static void Rebuild()
        {
            requiredGenesByThingDef.Clear();
            thingDefByApparelProps.Clear();

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.apparel != null)
                {
                    thingDefByApparelProps[def.apparel] = def;
                }

                GeneEquipRestriction ext = def.GetModExtension<GeneEquipRestriction>();
                if (ext == null || !ext.Configured)
                {
                    continue;
                }

                List<GeneDef> list = new List<GeneDef>();
                for (int i = 0; i < ext.requiredGenes.Count; i++)
                {
                    GeneDef g = ext.requiredGenes[i];
                    if (g != null)
                    {
                        list.Add(g);
                    }
                }

                if (list.Count > 0)
                {
                    requiredGenesByThingDef[def] = list;
                }
            }
        }

        public static bool TryGetRequiredGenes(ThingDef def, out List<GeneDef> genes)
        {
            return requiredGenesByThingDef.TryGetValue(def, out genes);
        }

        public static bool TryGetThingDefForApparelProps(ApparelProperties props, out ThingDef def)
        {
            return thingDefByApparelProps.TryGetValue(props, out def);
        }

        public static bool PawnMeetsAllRequiredGenes(Pawn pawn, List<GeneDef> required)
        {
            if (pawn?.genes == null || required.NullOrEmpty())
            {
                return false;
            }

            for (int i = 0; i < required.Count; i++)
            {
                if (!pawn.genes.HasActiveGene(required[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool BlocksPawn(ThingDef def, Pawn pawn, out string cantReason)
        {
            cantReason = null;
            if (!TryGetRequiredGenes(def, out List<GeneDef> req))
            {
                return false;
            }

            if (PawnMeetsAllRequiredGenes(pawn, req))
            {
                return false;
            }

            cantReason = "RK_GeneEquipRequirementFailed".Translate();
            return true;
        }

        private static void ApplyHarmonyPatches()
        {
            Harmony harmony = new Harmony("com.NewRatkin.genEquip");

            harmony.Patch(
                AccessTools.Method(typeof(JobGiver_OptimizeApparel), nameof(JobGiver_OptimizeApparel.ApparelScoreGain)),
                postfix: new HarmonyMethod(typeof(GeneEquipRestrictionRegistry), nameof(ApparelScoreGain_Postfix)));

            MethodInfo pawnCanWear = AccessTools.Method(
                typeof(ApparelProperties),
                nameof(ApparelProperties.PawnCanWear),
                new[] { typeof(Pawn), typeof(bool) });
            if (pawnCanWear != null)
            {
                harmony.Patch(
                    pawnCanWear,
                    postfix: new HarmonyMethod(typeof(GeneEquipRestrictionRegistry), nameof(PawnCanWear_Postfix)));
            }
        }

        private static void ApparelScoreGain_Postfix(Pawn pawn, Apparel ap, List<float> wornScoresCache, ref float __result)
        {
            if (__result < 0f || ap == null)
            {
                return;
            }

            if (!TryGetRequiredGenes(ap.def, out List<GeneDef> req))
            {
                return;
            }

            if (!PawnMeetsAllRequiredGenes(pawn, req))
            {
                __result = -1001f;
            }
        }

        private static void PawnCanWear_Postfix(ApparelProperties __instance, Pawn pawn, ref bool __result)
        {
            if (!__result || pawn == null || !TryGetThingDefForApparelProps(__instance, out ThingDef apparelDef))
            {
                return;
            }

            if (TryGetRequiredGenes(apparelDef, out List<GeneDef> req) && !PawnMeetsAllRequiredGenes(pawn, req))
            {
                __result = false;
            }
        }
    }
}
