using HarmonyLib;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// 1.7은 버스트 간격만 교체하여 게임의 발사·알림 처리를 그대로 사용한다.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class PulseRifleBurstPatch
    {
        static PulseRifleBurstPatch()
        {
            Harmony harmony = new Harmony("com.NewRatkin.rimworld.mod.pulserifle.burst");
#if RIMWORLD_1_7
            harmony.Patch(
                AccessTools.PropertyGetter(typeof(Verb), nameof(Verb.TicksBetweenBurstShots)),
                postfix: new HarmonyMethod(typeof(PulseRifleBurstPatch), nameof(TicksBetweenBurstShots_Postfix)));
#else
            harmony.Patch(
                AccessTools.Method(typeof(Verb), "TryCastNextBurstShot"),
                prefix: new HarmonyMethod(typeof(PulseRifleBurstPatch), nameof(TryCastNextBurstShot_Prefix)));
#endif
        }

#if RIMWORLD_1_7
        public static void TicksBetweenBurstShots_Postfix(Verb __instance, ref int __result)
        {
            if (__instance is Verb_PulseRifleShoot pulseVerb)
            {
                __result = pulseVerb.ModeTicksBetweenBurstShots;
            }
        }
#else
        public static bool TryCastNextBurstShot_Prefix(Verb __instance)
        {
            if (__instance is Verb_PulseRifleShoot pulseVerb)
            {
                pulseVerb.PulseTryCastNextBurstShot();
                return false;
            }

            return true;
        }
#endif
    }
}
