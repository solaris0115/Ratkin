using UnityEngine;
using Verse;
using RimWorld;

namespace NewRatkin
{
    /// <summary>
    /// RK_TowerShield_Second 전용. 방향 고정(RK_Job_ShieldFaceDirection) 상태일 때
    /// 캐릭터가 바라보는 방향 기준 좌우 70도 이내의 원거리 공격을 100% 튕겨냅니다.
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

            // 방향 고정 Job 수행 중일 때만 차단
            if (pawn.CurJob?.def != RatkinJobDefOf.RK_Job_ShieldFaceDirection)
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
    }
}
