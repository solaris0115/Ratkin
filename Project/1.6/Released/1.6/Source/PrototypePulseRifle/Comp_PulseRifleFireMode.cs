using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NewRatkin
{
    /// <summary>
    /// Prototype Pulse Rifle 연사/단발 모드 — verbProps 참조는 Def 공유 유지, 수치는 Comp + Verb override로 적용.
    /// </summary>
    public class Comp_PulseRifleFireMode : ThingComp
    {
        private bool isBurstMode = true;

        public CompProperties_PulseRifleFireMode Props => (CompProperties_PulseRifleFireMode)props;

        public bool IsBurstMode => isBurstMode;

        public ThingDef CurrentProjectile => isBurstMode ? Props.projectileBurst : Props.projectileSingle;

        public float CurrentRange => isBurstMode ? Props.rangeBurst : Props.rangeSingle;

        public int CurrentBurstShotCount => isBurstMode ? Props.burstShotCountBurst : Props.burstShotCountSingle;

        public int CurrentTicksBetweenBurstShots =>
            isBurstMode ? Props.ticksBetweenBurstShotsBurst : Props.ticksBetweenBurstShotsSingle;

        public float CurrentWarmupTime => isBurstMode ? Props.warmupTimeBurst : Props.warmupTimeSingle;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isBurstMode, "isBurstMode", true);
        }

        public override float GetStatOffset(StatDef stat)
        {
            if (stat == null)
            {
                return 0f;
            }

            List<StatModifier> offsets = isBurstMode ? Props.statOffsetsBurst : Props.statOffsetsSingle;
            if (offsets == null)
            {
                return 0f;
            }

            for (int i = 0; i < offsets.Count; i++)
            {
                if (offsets[i].stat == stat)
                {
                    return offsets[i].value;
                }
            }

            return 0f;
        }

        /// <summary>
        /// 모드별 사거리 + 날씨 상한 (VerbProperties.AdjustedRange와 동일 보정).
        /// </summary>
        public float GetAdjustedRange(Verb ownerVerb, Thing attacker)
        {
            float num = CurrentRange;
            if (attacker == null)
            {
                return num;
            }

            ThingWithComps equipmentSource = ownerVerb?.EquipmentSource;
            CompUniqueWeapon compUniqueWeapon;
            if (equipmentSource == null
                || !equipmentSource.TryGetComp(out compUniqueWeapon)
                || !compUniqueWeapon.IgnoreAccuracyMaluses)
            {
                Map mapHeld = attacker.MapHeld;
                if (mapHeld != null && mapHeld.weatherManager.CurWeatherMaxRangeCap >= 0f)
                {
                    num = Mathf.Min(num, mapHeld.weatherManager.CurWeatherMaxRangeCap);
                }
            }

            return num;
        }

        /// <summary>
        /// 발사 모드 토글 Gizmo. 장착 시 CompEquippable_PulseRifle에서 호출.
        /// </summary>
        public IEnumerable<Gizmo> GetToggleGizmos()
        {
            string iconPath = isBurstMode ? Props.iconPathBurst : Props.iconPathSingle;
            Texture2D icon = ContentFinder<Texture2D>.Get(iconPath, false);

            yield return new Command_Action
            {
                defaultLabel = (isBurstMode
                    ? "RK_PulseRifleFireMode_LabelBurst"
                    : "RK_PulseRifleFireMode_LabelSingle").Translate().ToString(),
                defaultDesc = (isBurstMode
                    ? "RK_PulseRifleFireMode_DescToSingle"
                    : "RK_PulseRifleFireMode_DescToBurst").Translate().ToString(),
                icon = icon,
                action = () =>
                {
                    isBurstMode = !isBurstMode;
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
                    Pawn holder = GetHolderPawn();
                    if (holder != null && holder.stances.curStance is Stance_Warmup)
                    {
                        holder.stances.CancelBusyStanceSoft();
                    }
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
