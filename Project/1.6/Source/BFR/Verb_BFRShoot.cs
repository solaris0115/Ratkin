using Verse;

namespace NewRatkin
{
    /// <summary>
    /// BFR 3000 전용 Verb - Comp_BFRAmmoToggle에서 현재 탄종을 가져와 발사
    /// </summary>
    public class Verb_BFRShoot : Verb_Shoot
    {
        public override ThingDef Projectile
        {
            get
            {
                ThingWithComps equipmentSource = EquipmentSource;
                Comp_BFRAmmoToggle comp = equipmentSource?.GetComp<Comp_BFRAmmoToggle>();
                if (comp != null)
                {
                    return comp.CurrentProjectile;
                }
                return verbProps.defaultProjectile;
            }
        }
    }
}
