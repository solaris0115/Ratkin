using Verse;

namespace NewRatkin
{
    /// <summary>
    /// 근접 공격 시 폭발을 일으키는 Verb용 프로퍼티
    /// </summary>
    public class VerbProperties_MeleeExplosion : VerbProperties
    {
        /// <summary>
        /// 폭발 반경 (셀 단위)
        /// </summary>
        public float explosionRadius = 1f;

        /// <summary>
        /// 폭발 원뿔 각도 (degrees)
        /// 예: 30이면 ±15도 원뿔
        /// </summary>
        public float explosionAngle = 26f;

        /// <summary>
        /// 폭발 데미지 타입
        /// </summary>
        public DamageDef explosionDamageDef;

        /// <summary>
        /// 폭발 데미지량 (-1이면 기본값 사용)
        /// </summary>
        public int explosionDamageAmount = -1;

        /// <summary>
        /// 폭발 관통력 (-1이면 기본값 사용)
        /// </summary>
        public float explosionArmorPenetration = -1f;

        /// <summary>
        /// 폭발 사운드
        /// </summary>
        public SoundDef explosionSound;

        /// <summary>
        /// 폭발 후 생성할 filth (null이면 없음)
        /// </summary>
        public ThingDef postExplosionFilth;

        /// <summary>
        /// 화염 발화 확률 (0~1)
        /// </summary>
        public float chanceToStartFire = 0f;

        /// <summary>
        /// 화면 흔들림 강도 (0 = 화면 흔들림 없음, 1 = 기본값)
        /// </summary>
        public float screenShakeFactor = 0f;
    }
}

