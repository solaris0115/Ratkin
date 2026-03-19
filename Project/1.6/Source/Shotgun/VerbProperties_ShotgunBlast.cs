using Verse;

namespace NewRatkin
{
    /// <summary>
    /// 산탄총 동시 발사용 Verb 속성
    /// </summary>
    public class VerbProperties_ShotgunBlast : VerbProperties
    {
        /// <summary>
        /// 산탄 수 (동시 발사할 탄환 개수)
        /// XML에서 설정 가능: &lt;pelletCount&gt;4&lt;/pelletCount&gt;
        /// </summary>
        public int pelletCount = 4;

        /// <summary>
        /// 분산각 (도 단위). 최대 거리 기준 분산 범위
        /// XML에서 설정 가능: &lt;spreadAngleDeg&gt;5&lt;/spreadAngleDeg&gt;
        /// </summary>
        public float spreadAngleDeg = 5f;

        /// <summary>
        /// 비목표(경로상 다른 대상) 적중 허용 확률. 1.0=항상, 0.5=50%
        /// XML에서 설정 가능: &lt;nonTargetHitChance&gt;1&lt;/nonTargetHitChance&gt;
        /// </summary>
        public float nonTargetHitChance = 1f;
    }
}
