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
        /// 쉴드 버블 최소 크기
        /// </summary>
        public float minDrawSize = 1.2f;

        /// <summary>
        /// 쉴드 버블 최대 크기
        /// </summary>
        public float maxDrawSize = 1.55f;

        /// <summary>
        /// 데미지당 에너지 손실 비율
        /// </summary>
        public float energyLossPerDamage = 0.033f;

        /// <summary>
        /// 재충전 완료 시 초기 에너지 비율
        /// </summary>
        public float energyOnReset = 0.2f;

        /// <summary>
        /// 원거리 무기 발사 차단 여부
        /// </summary>
        public bool blocksRangedWeapons = true;
    }
}

