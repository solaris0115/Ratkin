using HarmonyLib;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// Verb.TryCastNextBurstShot는 override 불가 — 모드별 버스트 간격만 Prefix로 처리.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class PulseRifleBurstPatch
    {
        static PulseRifleBurstPatch()
        {
            Harmony harmony = new Harmony("com.NewRatkin.rimworld.mod.pulserifle.burst");
            harmony.Patch(
                AccessTools.Method(typeof(Verb), "TryCastNextBurstShot"),
                prefix: new HarmonyMethod(typeof(PulseRifleBurstPatch), nameof(TryCastNextBurstShot_Prefix)));
        }

        public static bool TryCastNextBurstShot_Prefix(Verb __instance)
        {
            if (__instance is Verb_PulseRifleShoot pulseVerb)
            {
                pulseVerb.PulseTryCastNextBurstShot();
                return false;
            }

            return true;
        }
    }
}
