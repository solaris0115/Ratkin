using Verse;

namespace NewRatkin
{
    public class CompProperties_StaminaShield : CompProperties
    {
        public CompProperties_StaminaShield()
        {
            this.compClass = typeof(CompStaminaShield);
        }

        /// <summary>
        /// 쉴드가 깨진 후 재충전까지 걸리는 틱 수
        /// </summary>
        public int startingTicksToReset = 3200;

        /// <summary>
        /// 재충전 완료 시 초기 스태미나 비율
        /// </summary>
        public float staminaOnReset = 0.2f;

        /// <summary>
        /// 근접 공격당 스태미나 손실 비율 (감쇄된 데미지량 × 이 비율만큼 차감)
        /// </summary>
        public float staminaLossPerDamageMelee = 0.033f;

        /// <summary>
        /// 원거리 공격당 스태미나 손실 비율 (감쇄된 데미지량 × 이 비율만큼 차감)
        /// </summary>
        public float staminaLossPerDamageRanged = 0.033f;

        /// <summary>
        /// 폭발 공격당 스태미나 손실 비율 (감쇄된 데미지량 × 이 비율만큼 차감)
        /// </summary>
        public float staminaLossPerDamageExplosive = 0.033f;

        /// <summary>
        /// 근접 공격 피해 감소율 (0.0 ~ 1.0, 1.0 = 100% 차단)
        /// </summary>
        public float damageReductionPercentMelee = 1.0f;

        /// <summary>
        /// 원거리 공격 피해 감소율 (0.0 ~ 1.0, 1.0 = 100% 차단)
        /// </summary>
        public float damageReductionPercentRanged = 1.0f;

        /// <summary>
        /// 폭발 공격 피해 감소율 (0.0 ~ 1.0, 1.0 = 100% 차단)
        /// </summary>
        public float damageReductionPercentExplosive = 1.0f;

        /// <summary>
        /// 방패가 흡수한 데미지량에 대한 내구도 손상 비율 (기본값 0.25 = 25%)
        /// 실제로 방패가 막아낸 데미지량(reducedDamage) × 이 비율만큼 내구도 손상 발생
        /// 0.0 = 내구도 손상 없음, 0.125 = 12.5%, 0.5 = 50% 등
        /// </summary>
        public float durabilityDamagePercent = 0.25f;

        /// <summary>
        /// 스태미나 브레이크 시 pawn에게 부여할 스턴 시간 (틱 단위)
        /// 0 = 스턴 부여 안 함, 60 = 1초, 300 = 5초 등
        /// </summary>
        public int stunDurationTicks = 0;
    }
}

