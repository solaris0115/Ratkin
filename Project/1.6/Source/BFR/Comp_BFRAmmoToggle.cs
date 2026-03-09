using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NewRatkin
{
    /// <summary>
    /// BFR 3000 탄종 토글 Comp 속성
    /// </summary>
    public class CompProperties_BFRAmmoToggle : CompProperties
    {
        public ThingDef projectileAP;
        public ThingDef projectileHE;
        public string iconPathAP = "UI/BFR/BFR_AP";
        public string iconPathHE = "UI/BFR/BFR_HE";

        public CompProperties_BFRAmmoToggle()
        {
            compClass = typeof(Comp_BFRAmmoToggle);
        }
    }

    /// <summary>
    /// BFR 3000 AP탄/폭발탄 토글 Comp
    /// 커맨드창 Gizmo로 탄종 전환, 세이브/로드 시 마지막 선택 기억
    /// </summary>
    public class Comp_BFRAmmoToggle : ThingComp
    {
        private bool isHEMode;

        public CompProperties_BFRAmmoToggle Props => (CompProperties_BFRAmmoToggle)props;

        /// <summary>
        /// 현재 선택된 발사체 ThingDef
        /// </summary>
        public ThingDef CurrentProjectile => isHEMode ? Props.projectileHE : Props.projectileAP;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isHEMode, "isHEMode", false);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (parent.Faction != Faction.OfPlayer)
            {
                yield break;
            }

            bool showGizmo = false;
            if (parent.Spawned)
            {
                showGizmo = parent.Map.IsPlayerHome;
            }
            else if (parent.ParentHolder is Pawn holderPawn)
            {
                showGizmo = holderPawn.Faction == Faction.OfPlayer;
            }
            else if (parent.ParentHolder is IThingHolder holder)
            {
                Thing holderThing = holder as Thing;
                showGizmo = holderThing?.Map?.IsPlayerHome ?? false;
            }

            if (!showGizmo)
            {
                yield break;
            }

            string label = isHEMode ? "HE" : "AP";
            string iconPath = isHEMode ? Props.iconPathHE : Props.iconPathAP;
            Texture2D icon = ContentFinder<Texture2D>.Get(iconPath, false);

            yield return new Command_Action
            {
                defaultLabel = label,
                defaultDesc = isHEMode ? "BFR: Switch to AP round" : "BFR: Switch to HE round",
                icon = icon,
                action = () =>
                {
                    isHEMode = !isHEMode;
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
                }
            };
        }
    }
}
