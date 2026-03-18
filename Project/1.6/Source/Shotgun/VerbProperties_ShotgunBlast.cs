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
    }
}
