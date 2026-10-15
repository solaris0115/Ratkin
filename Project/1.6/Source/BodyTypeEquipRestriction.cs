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
#if RIMWORLD_1_7
            // 의상 체형 제한은 1.7 전용. 1.6은 마지막 배포 당시 HAR 의상 제한을 유지한다.
            ApplyHarmonyPatches();
#endif
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
#if !RIMWORLD_1_7
            // 1.6에는 의상 체형 제한을 적용하지 않는다. 무기 조건은 별도로 유지한다.
            if (def.IsApparel)
            {
                return false;
            }
#endif
            if (!TryGetAllowedBodyTypes(def, out List<BodyTypeDef> allowed))
            {
                return false;
            }

            if (PawnMeetsBodyTypeRequirement(pawn, allowed))
            {
                return false;
            }

            // 어린이는 성인 체형 유전자와 무관하게 Child 체형을 사용한다.
            // Thin용 의류 중 원래 어린이 착용이 가능한 것만 허용하며, 무기 제한은 유지한다.
            // Pawn 오버로드는 아래 Harmony 패치를 다시 호출하므로 성별·성장 단계 오버로드를 사용한다.
            if (def.IsApparel && pawn?.story?.bodyType == BodyTypeDefOf.Child &&
                pawn.DevelopmentalStage.Child() && allowed.Contains(BodyTypeDefOf.Thin) &&
                def.apparel.PawnCanWear(pawn.gender, pawn.DevelopmentalStage))
            {
                return false;
            }

            cantReason = "RK_BodyTypeEquipRequirementFailed".Translate();
            return true;
        }

#if RIMWORLD_1_7
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

            if (BlocksPawn(ap.def, pawn, out _))
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

            if (BlocksPawn(apparelDef, pawn, out _))
            {
                __result = false;
            }
        }
#endif
    }
}
