using RimWorld;
using Verse;

namespace TestMod
{
    public class CompProperties_MeleeExplosive : CompProperties
    {
        public float range = 3f;
        public float lineWidthEnd = 1.5f;
        public int damAmount = 15;
        public DamageDef damageDef = DamageDefOf.Bomb;
        public float armorPenetration = -1f;
        public float chance = 1.0f;
        public bool canHitFilledCells = false;

        public CompProperties_MeleeExplosive()
        {
            this.compClass = typeof(CompMeleeExplosive);
        }
    }
}

