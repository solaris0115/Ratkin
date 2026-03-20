using UnityEngine;
using Verse;
using RimWorld;

namespace NewRatkin
{
    /// <summary>
    /// RK_TowerShield_Second 전용. deflect와 방향 고정은 별개 기능.
    /// deflect: 소집 상태 + 통제 가능 시, 바라보는 방향 기준 좌우 70도 이내 원거리 공격을 확률적으로 튕겨냄.
    /// 방향 고정(RK_Job_ShieldFaceDirection): 별도 기능 (대기 방향 지정 등).
    /// 통제 불가(기절/사망/불붙음/정신붕괴) 시 deflect 불가.
    /// </summary>
    public class ApparelShieldTowerSecond : Apparel
    {
        private CompProperties_ShieldFaceDirection FaceDirectionProps =>
            this.GetComp<CompShieldFaceDirection>()?.Props as CompProperties_ShieldFaceDirection;

        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            Pawn pawn = Wearer;
            if (pawn == null || pawn.Dead || pawn.Downed)
            {
                return false;
            }

            // deflect: 소집 상태에서만 적용 (방향 고정과 별개)
            if (!pawn.Drafted)
            {
                return false;
            }

            // 통제 불가 상황: 기절, 불붙음, 정신붕괴 → deflect 없음 (사망/쓰러짐은 위에서 이미 체크)
            if (!PawnCanDeflectWithShield(pawn))
            {
                return false;
            }

            // ignoreShields 또는 EMP는 차단하지 않음
            if (dinfo.Def.ignoreShields || dinfo.Def == DamageDefOf.EMP)
            {
                return false;
            }

            // 원거리 공격만 대상
            if (!dinfo.Def.isRanged)
            {
                return false;
            }

            // 입사각 ±70도 이내인지 확인 (ApparelShield와 동일한 계산)
            float attackerAngle = dinfo.Angle + 180f;
            if (attackerAngle >= 360f)
            {
                attackerAngle -= 360f;
            }
            if (attackerAngle < 0f)
            {
                attackerAngle += 360f;
            }

            float defenderAngle = pawn.Rotation.AsAngle;
            float angleDiff = defenderAngle - attackerAngle;

            // -180 ~ 180 범위로 정규화
            while (angleDiff > 180f) angleDiff -= 360f;
            while (angleDiff < -180f) angleDiff += 360f;

            float deflectAngleHalf = FaceDirectionProps?.deflectAngleHalf ?? 70f;
            if (angleDiff < -deflectAngleHalf || angleDiff > deflectAngleHalf)
            {
                return false;
            }

            // 흡수 확률 체크 (XML deflectChance, 기본 100%)
            float deflectChance = FaceDirectionProps?.deflectChance ?? 1f;
            if (Rand.Value > deflectChance)
            {
                return false;
            }

            // 차단: Deflect 텍스트 모트 + 이펙트
            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "ShieldBlock".Translate(), 1.9f);
            EffecterDefOf.Deflect_Metal.Spawn().Trigger(pawn, dinfo.Instigator ?? pawn);
            return true;
        }

        /// <summary>
        /// Pawn이 방패 deflect를 할 수 있는 상태인지 (통제 가능 여부).
        /// 기절, 불붙음, 정신붕괴 등 통제 불가 시 false.
        /// </summary>
        private static bool PawnCanDeflectWithShield(Pawn pawn)
        {
            if (pawn?.stances?.stunner == null)
                return false;
            if (pawn.stances.stunner.Stunned)
                return false;
            if (pawn.IsBurning())
                return false;
            if (pawn.InMentalState)
                return false;
            return true;
        }
    }
}
