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
        public string iconPathAP = "UI/Commands/RK_Icon_ArmorPiercing";
        public string iconPathHE = "UI/Commands/RK_Icon_ShapedCharge";

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

        /// <summary>
        /// 탄종 토글 Gizmo 반환. 장비 시 CompGetEquippedGizmosExtra 패치에서 호출됨.
        /// </summary>
        public IEnumerable<Gizmo> GetToggleGizmos()
        {
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

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (!ShouldShowGizmo())
            {
                yield break;
            }

            foreach (Gizmo gizmo in GetToggleGizmos())
            {
                yield return gizmo;
            }
        }

        private bool ShouldShowGizmo()
        {
            if (parent.Faction != null && parent.Faction != Faction.OfPlayer)
            {
                return false;
            }

            if (parent.Spawned)
            {
                return parent.Map?.IsPlayerHome ?? false;
            }

            Pawn holderPawn = GetHolderPawn();
            if (holderPawn != null)
            {
                return holderPawn.Faction == Faction.OfPlayer;
            }

            if (parent.ParentHolder is Thing holderThing)
            {
                return holderThing.Map?.IsPlayerHome ?? false;
            }

            return false;
        }

        private Pawn GetHolderPawn()
        {
            if (parent.ParentHolder is Pawn_EquipmentTracker tracker)
            {
                return tracker.pawn;
            }
            return parent.ParentHolder as Pawn;
        }
    }
}
