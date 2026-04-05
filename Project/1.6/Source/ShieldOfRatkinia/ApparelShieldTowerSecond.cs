using Verse;
using RimWorld;

namespace NewRatkin
{
    /// <summary>
    /// RK_TowerShield_Second 전용. deflect와 방향 고정은 별개 기능.
    /// deflect: 소집 상태 + 통제 가능 시, 바라보는 방향 기준 좌우 RK_Stat_DeflectAngle(반각, 도) 이내 근·원거리 공격을 동일 수치로 확률적으로 튕겨냄.
    /// 방향 고정(RK_Job_ShieldFaceDirection): 별도 기능 (대기 방향 지정 등).
    /// 통제 불가(기절/사망/불붙음/정신붕괴) 시 deflect 불가.
    /// 도탄 판정값 = 방패 방어(Rating)+ RK_Stat_ShieldHandling, 도탄률 = 판정값 − 관통. Rand.Value ≤ 도탄률이면 도탄.
    /// 방패 다루기 StatDef는 코어 MeleeHitChance와 같은 XML 구조(skill/capacity/postProcessCurve/StatPart_Age).
    /// DeflectAngle은 품질 무관, 폰의 melee 스킬로 반각에 배율 적용(0레벨 0.5× ~ 20레벨 1.2×, ShieldDeflectAngleMeleeCurve).
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

            // 입사/타격 방향이 허용 각도 안인지 확인 (근접·원거리 동일)
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

            float deflectAngleHalf = this.GetStatValue(RatkinStatDefOf.RK_Stat_DeflectAngle);
            if (deflectAngleHalf <= 0f)
            {
                deflectAngleHalf = FaceDirectionProps?.deflectAngleHalf ?? 70f;
            }
            float meleeLevel = pawn.skills?.GetSkill(SkillDefOf.Melee)?.Level ?? 0f;
            deflectAngleHalf *= ShieldDeflectAngleMeleeCurve.Evaluate(meleeLevel);
            if (angleDiff < -deflectAngleHalf || angleDiff > deflectAngleHalf)
            {
                return false;
            }

            float armorRating = GetArmorRatingForDamage(dinfo);
            string armorType = dinfo.Def.armorCategory?.defName ?? "None";
            float shieldHandling = pawn.GetStatValue(RatkinStatDefOf.RK_Stat_ShieldHandling);
            float deflectPower = armorRating + shieldHandling;
            float penetration = dinfo.ArmorPenetrationInt;
            float deflectRate = deflectPower - penetration;

            float roll = Rand.Value;
            bool deflected = roll <= deflectRate;

            if (Prefs.DevMode)
            {
                string attacker = dinfo.Instigator?.LabelShort ?? "?";
                Log.Message(
                    $"[RK-TowerShield2] {pawn.LabelShort} ← {attacker} ({dinfo.Def.defName}, {(dinfo.Def.isRanged ? "ranged" : "melee")})\n" +
                    $"  angle: diff={angleDiff:F1}° half={deflectAngleHalf:F1}° | dmgType={armorType}\n" +
                    $"  armor={armorRating:P0} + handling={shieldHandling:P0} = power={deflectPower:P0}\n" +
                    $"  power={deflectPower:P0} - AP={penetration:P0} = rate={deflectRate:P0}\n" +
                    $"  roll={roll:F3} {(deflected ? "<=" : ">")} rate={deflectRate:F3} → {(deflected ? "DEFLECT" : "HIT")}");
            }

            if (!deflected)
            {
                return false;
            }

            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "ShieldBlock".Translate(), 1.9f);
            EffecterDefOf.Deflect_Metal.Spawn().Trigger(pawn, dinfo.Instigator ?? pawn);
            return true;
        }

        private float GetArmorRatingForDamage(DamageInfo dinfo)
        {
            switch (dinfo.Def.armorCategory)
            {
                case DamageArmorCategoryDef d when d == DamageArmorCategoryDefOf.Sharp:
                    return this.GetStatValue(StatDefOf.ArmorRating_Sharp);
                case DamageArmorCategoryDef d when d == DamageArmorCategoryDefOf.Blunt:
                    return this.GetStatValue(StatDefOf.ArmorRating_Blunt);
                case DamageArmorCategoryDef d when d == DamageArmorCategoryDefOf.Heat:
                    return this.GetStatValue(StatDefOf.ArmorRating_Heat);
                default:
                    return 0f;
            }
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
