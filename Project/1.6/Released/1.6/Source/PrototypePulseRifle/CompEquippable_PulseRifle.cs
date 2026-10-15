using System.Collections.Generic;
using Verse;

namespace NewRatkin
{
    /// <summary>
    /// 파동 소총 전용 CompEquippable — 장착 시 발사 모드 토글 Gizmo 제공 (Harmony 없음).
    /// </summary>
    public class CompEquippable_PulseRifle : CompEquippable
    {
        public override IEnumerable<Gizmo> CompGetEquippedGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetEquippedGizmosExtra())
            {
                yield return gizmo;
            }

            Comp_PulseRifleFireMode fireMode = parent.GetComp<Comp_PulseRifleFireMode>();
            if (fireMode == null)
            {
                yield break;
            }

            foreach (Gizmo gizmo in fireMode.GetToggleGizmos())
            {
                yield return gizmo;
            }
        }
    }
}
