using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace NewRatkin
{
    /// <summary>
    /// ThingDef에 붙이면 <c>allowedBodyTypes</c> 중 하나인 체형의 폰만 착용·장착 가능 (Biotech + XML 패치로 race restriction 해제 시 사용).
    /// </summary>
    public class BodyTypeEquipRestriction : DefModExtension
    {
        public List<BodyTypeDef> allowedBodyTypes;

        public bool Configured => !allowedBodyTypes.NullOrEmpty();
    }

    /// <summary>
    /// 로드 시 한 번 구축하는 조회용 캐시 (런타임에 GetModExtension 반복 호출 방지).
    /// </summary>
    [StaticConstructorOnStartup]
    public static class BodyTypeEquipRestrictionRegistry
    {
        private static readonly Dictionary<ThingDef, List<BodyTypeDef>> allowedBodyTypesByThingDef =
            new Dictionary<ThingDef, List<BodyTypeDef>>();

        private static readonly Dictionary<ApparelProperties, ThingDef> thingDefByApparelProps =
            new Dictionary<ApparelProperties, ThingDef>();

        static BodyTypeEquipRestrictionRegistry()
        {
            Rebuild();
            ApplyHarmonyPatches();
        }

        public static void Rebuild()
        {
            allowedBodyTypesByThingDef.Clear();
            thingDefByApparelProps.Clear();

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.apparel != null)
                {
                    thingDefByApparelProps[def.apparel] = def;
                }

                BodyTypeEquipRestriction ext = def.GetModExtension<BodyTypeEquipRestriction>();
                if (ext == null || !ext.Configured)
                {
                    continue;
                }

                List<BodyTypeDef> list = new List<BodyTypeDef>();
                for (int i = 0; i < ext.allowedBodyTypes.Count; i++)
                {
                    BodyTypeDef bt = ext.allowedBodyTypes[i];
                    if (bt != null)
                    {
                        list.Add(bt);
                    }
                }

                if (list.Count > 0)
                {
                    allowedBodyTypesByThingDef[def] = list;
                }
            }
        }

        public static bool TryGetAllowedBodyTypes(ThingDef def, out List<BodyTypeDef> bodyTypes)
        {
            return allowedBodyTypesByThingDef.TryGetValue(def, out bodyTypes);
        }

        public static bool TryGetThingDefForApparelProps(ApparelProperties props, out ThingDef def)
        {
            return thingDefByApparelProps.TryGetValue(props, out def);
        }

        public static bool PawnMeetsBodyTypeRequirement(Pawn pawn, List<BodyTypeDef> allowed)
        {
            if (pawn?.story?.bodyType == null || allowed.NullOrEmpty())
            {
                return false;
            }

            BodyTypeDef pawnBodyType = pawn.story.bodyType;
            for (int i = 0; i < allowed.Count; i++)
            {
                if (allowed[i] == pawnBodyType)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool BlocksPawn(ThingDef def, Pawn pawn, out string cantReason)
        {
            cantReason = null;
            if (!TryGetAllowedBodyTypes(def, out List<BodyTypeDef> allowed))
            {
                return false;
            }

            if (PawnMeetsBodyTypeRequirement(pawn, allowed))
            {
                return false;
            }

            cantReason = "RK_BodyTypeEquipRequirementFailed".Translate();
            return true;
        }

        private static void ApplyHarmonyPatches()
        {
            Harmony harmony = new Harmony("com.NewRatkin.bodyTypeEquip");

            harmony.Patch(
                AccessTools.Method(typeof(JobGiver_OptimizeApparel), nameof(JobGiver_OptimizeApparel.ApparelScoreGain)),
                postfix: new HarmonyMethod(typeof(BodyTypeEquipRestrictionRegistry), nameof(ApparelScoreGain_Postfix)));

            MethodInfo pawnCanWear = AccessTools.Method(
                typeof(ApparelProperties),
                nameof(ApparelProperties.PawnCanWear),
                new[] { typeof(Pawn), typeof(bool) });
            if (pawnCanWear != null)
            {
                harmony.Patch(
                    pawnCanWear,
                    postfix: new HarmonyMethod(typeof(BodyTypeEquipRestrictionRegistry), nameof(PawnCanWear_Postfix)));
            }
        }

        private static void ApparelScoreGain_Postfix(Pawn pawn, Apparel ap, List<float> wornScoresCache, ref float __result)
        {
            if (__result < 0f || ap == null)
            {
                return;
            }

            if (!TryGetAllowedBodyTypes(ap.def, out List<BodyTypeDef> allowed))
            {
                return;
            }

            if (!PawnMeetsBodyTypeRequirement(pawn, allowed))
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

            if (TryGetAllowedBodyTypes(apparelDef, out List<BodyTypeDef> allowed) && !PawnMeetsBodyTypeRequirement(pawn, allowed))
            {
                __result = false;
            }
        }
    }
}
