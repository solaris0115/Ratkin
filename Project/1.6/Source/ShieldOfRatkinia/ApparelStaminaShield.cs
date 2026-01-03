using Verse;
using RimWorld;

namespace NewRatkin
{
    [StaticConstructorOnStartup]
    public class StaminaShield : Apparel
    {
        public CompStaminaShield CompStaminaShield
        {
            get
            {
                return this.GetComp<CompStaminaShield>();
            }
        }

        public override bool AllowVerbCast(Verb verb)
        {
            CompStaminaShield comp = this.CompStaminaShield;
            if (comp != null)
            {
                return comp.CompAllowVerbCast(verb);
            }
            return base.AllowVerbCast(verb);
        }
    }
}

