using UnityEngine;
using Verse;
using RimWorld;

namespace NewRatkin
{
    /// <summary>
    /// RK_TowerShield_Second 전용. deflect와 방향 고정은 별개 기능.
    /// deflect: 소집 상태 + 통제 가능 시, 바라보는 방향 기준 좌우 RK_Stat_DeflectAngle(풀각, 도)/2 이내 근·원거리 공격을 Gumbel형 도탄 확률로 튕겨냄.
    /// 방향 고정(RK_Job_ShieldFaceDirection): 별도 기능 (대기 방향 지정 등).
    /// 통제 불가(기절/사망/불붙음/정신붕괴) 시 deflect 불가.
    /// 도탄: D = S − P, z = D + a·M − b, BlockChance = Cmin + (Cmax − Cmin)·exp(−exp(−k·z)); M은 근접 스킬 선형·지수 블렌드. Rand.Value &lt; BlockChance 이면 도탄.
    /// DeflectAngle은 풀각(좌+우) 기준 세팅. 내부에서 /2 하여 반각으로 변환 후 melee 스킬 배율 적용(0레벨 0.5× ~ 20레벨 1.2×, ShieldDeflectAngleMeleeCurve).
    /// </summary>
    public class ApparelShieldTowerSecond : Apparel
    {
        /// <summary>v2 도탄 고정 상수 (TempData 쉴드 샘플 HTML과 동일).</summary>
        public const float DeflectV2_Mmax = 0.40f;
        public const float DeflectV2_Lcap = 20f;
        public const float DeflectV2_Cmin = 0.05f;
        public const float DeflectV2_Cmax = 0.90f;
        public const float DeflectV2_k = 1.50f;
        public const float DeflectV2_a = 2.10f;
        public const float DeflectV2_b = 0.47f;
        public const float DeflectV2_Kappa = 2.5f;

        private CompProperties_ShieldFaceDirection FaceDirectionProps =>
            this.GetComp<CompShieldFaceDirection>()?.Props as CompProperties_ShieldFaceDirection;

        /// <summary>근접 스킬 L → M (선형·지수 중간 블렌드). L은 0~Lcap로 클램프.</summary>
        public static float ComputeM(float meleeLevel)
        {
            float Lc = Mathf.Clamp(meleeLevel, 0f, DeflectV2_Lcap);
            float t = Lc / DeflectV2_Lcap;
            float mLinear = t;
            float denom = Mathf.Exp(DeflectV2_Kappa) - 1f;
            float mExp = denom > 0f
                ? (Mathf.Exp(DeflectV2_Kappa * t) - 1f) / denom
                : 0f;
            return DeflectV2_Mmax * (mLinear + mExp) * 0.5f;
        }

        /// <summary>Gumbel형 이중 지수: z = D + a·M − b, BlockChance = Cmin + (Cmax − Cmin)·exp(−exp(−k·z)).</summary>
        public static float ComputeBlockChance(float D, float M)
        {
            float z = D + DeflectV2_a * M - DeflectV2_b;
            float gumbel = Mathf.Exp(-Mathf.Exp(-DeflectV2_k * z));
            return DeflectV2_Cmin + (DeflectV2_Cmax - DeflectV2_Cmin) * gumbel;
        }

        /// <summary>AP·방어력·근접 레벨로 도탄 확률 (UI·실전 공통).</summary>
        public static float ComputeBlockChanceForArmorAndMelee(float armorRating, float meleeLevel, float armorPenetration)
        {
            float D = armorRating - armorPenetration;
            float M = ComputeM(meleeLevel);
            return ComputeBlockChance(D, M);
        }

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

            float deflectAngleFull = this.GetStatValue(RatkinStatDefOf.RK_Stat_DeflectAngle);
            if (deflectAngleFull <= 0f)
            {
                deflectAngleFull = FaceDirectionProps?.deflectAngleHalf ?? 140f;
            }
            float deflectAngleHalf = deflectAngleFull * 0.5f;
            float meleeLevel = pawn.skills?.GetSkill(SkillDefOf.Melee)?.Level ?? 0f;
            deflectAngleHalf *= ShieldDeflectAngleMeleeCurve.Evaluate(meleeLevel);
            if (angleDiff < -deflectAngleHalf || angleDiff > deflectAngleHalf)
            {
                return false;
            }

            float armorRating = GetArmorRatingForDamage(dinfo);
            string armorType = dinfo.Def.armorCategory?.defName ?? "None";
            float penetration = dinfo.ArmorPenetrationInt;
            float D = armorRating - penetration;
            float M = ComputeM(meleeLevel);
            float z = D + DeflectV2_a * M - DeflectV2_b;
            float blockChance = ComputeBlockChance(D, M);

            float roll = Rand.Value;
            bool deflected = roll < blockChance;

            if (Prefs.DevMode)
            {
                string attacker = dinfo.Instigator?.LabelShort ?? "?";
                Log.Message(
                    $"[RK-TowerShield2] {pawn.LabelShort} ← {attacker} ({dinfo.Def.defName}, {(dinfo.Def.isRanged ? "ranged" : "melee")})\n" +
                    $"  angle: diff={angleDiff:F1}° half={deflectAngleHalf:F1}° | dmgType={armorType}\n" +
                    $"  S={armorRating:F3} P={penetration:F3} → D=S-P={D:F3} | melee L={meleeLevel:F0} → M={M:F3}\n" +
                    $"  z=D+a·M-b={z:F3} → BlockChance={blockChance:P1}\n" +
                    $"  roll={roll:F3} {(deflected ? "<" : ">=")} {blockChance:F3} → {(deflected ? "DEFLECT" : "HIT")}");
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
                    return this.GetStatValue(RatkinStatDefOf.RK_Stat_Shield_Sharp);
                case DamageArmorCategoryDef d when d == DamageArmorCategoryDefOf.Blunt:
                    return this.GetStatValue(RatkinStatDefOf.RK_Stat_Shield_Blunt);
                case DamageArmorCategoryDef d when d == DamageArmorCategoryDefOf.Heat:
                    return this.GetStatValue(RatkinStatDefOf.RK_Stat_Shield_Heat);
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
