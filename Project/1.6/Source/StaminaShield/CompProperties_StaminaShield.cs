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

        // 품질 영향 스탯들은 statBases에서 가져오도록 변경됨:
        // RK_Stat_ShieldStaminaOnReset, RK_Stat_ShieldStaminaLossMelee, 
        // RK_Stat_ShieldStaminaLossRanged, RK_Stat_ShieldStaminaLossExplosive,
        // RK_Stat_ShieldReduceDamageMelee, RK_Stat_ShieldReduceDamageRanged, RK_Stat_ShieldReduceDamageExplosive

        /// <summary>
        /// 방패가 흡수한 데미지량에 대한 내구도 손상 비율 (기본값 0.25 = 25%)
        /// 실제로 방패가 막아낸 데미지량(reducedDamage) × 이 비율만큼 내구도 손상 발생
        /// 0.0 = 내구도 손상 없음, 0.125 = 12.5%, 0.5 = 50% 등
        /// </summary>
        public float durabilityDamagePercent = 0.25f;

        /// <summary>
        /// 스태미나 브레이크 시 스턴 기능 활성화 여부 (기본값: false)
        /// true = 스턴 부여, false = 스턴 부여 안 함
        /// </summary>
        public bool enableStun = false;

        /// <summary>
        /// 스태미나 브레이크 시 pawn에게 부여할 스턴 시간 (틱 단위)
        /// enableStun이 true일 때만 적용됨
        /// 60 = 1초, 300 = 5초 등
        /// </summary>
        public int stunDurationTicks = 0;
    }
}

