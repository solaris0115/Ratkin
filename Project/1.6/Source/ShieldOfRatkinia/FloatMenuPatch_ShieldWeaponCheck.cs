using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace NewRatkin
{
    /// <summary>
    /// FloatMenu에서 방패 착용 시 호환되지 않는 무기를 착용 중일 때 미리 경고를 표시하는 Patch
    /// </summary>
    [HarmonyPatch(typeof(FloatMenuOptionProvider_Wear))]
    [HarmonyPatch("GetSingleOptionFor")]
    public static class FloatMenuPatch_ShieldWeaponCheck
    {
        [HarmonyPostfix]
        public static void Postfix(ref FloatMenuOption __result, Thing clickedThing, FloatMenuContext context)
        {
            // 이미 착용 불가로 표시되었거나 null이면 무시
            if (__result == null || __result.Disabled)
            {
                return;
            }

            // Apparel이 아니면 무시
            Apparel apparel = clickedThing as Apparel;
            if (apparel == null)
            {
                return;
            }

            // Pawn이 없으면 무시
            Pawn pawn = context.FirstSelectedPawn;
            if (pawn == null)
            {
                return;
            }

            // CompShieldWeaponIncompatible이 없으면 무시
            CompShieldWeaponIncompatible comp = apparel.TryGetComp<CompShieldWeaponIncompatible>();
            if (comp == null)
            {
                return;
            }

            // Primary 무기가 없으면 착용 가능
            if (pawn.equipment?.Primary == null)
            {
                return;
            }

            // 무기 검증
            string reason;
            if (!comp.TryIsWeaponAllowed(pawn.equipment.Primary.def, out reason))
            {
                // 착용 불가 옵션으로 대체
                string labelKey = apparel.def.apparel.LastLayer.IsUtilityLayer 
                    ? "CannotEquipApparel" 
                    : "CannotWear";

                // 번역 키가 있으면 사용, 없으면 기본 메시지
                string failMessage;
                if (!comp.Props.blockReasonKey.NullOrEmpty())
                {
                    failMessage = comp.Props.blockReasonKey.Translate();
                }
                else
                {
                    failMessage = reason;
                }

                __result = new FloatMenuOption(
                    labelKey.Translate(apparel.Label, apparel) + ": " + failMessage,
                    null, // action을 null로 설정하면 클릭 불가 (회색으로 표시)
                    MenuOptionPriority.Default,
                    null, null, 0f, null, null, true, 0
                );
            }
        }
    }
}

