using System;
using Verse;
using RimWorld;

namespace NewRatkin
{
	public class CompProperties_AbilityWyvernFire : CompProperties_AbilityEffect
	{
		public float range;

		public float lineWidthEnd;

		public DamageDef damageDef;

		public int damAmount = -1;

		public float armorPenetration = -1f;

		public ThingDef filthDef;

		public EffecterDef effecterDef;

		public bool canHitFilledCells;

		public CompProperties_AbilityWyvernFire()
		{
			this.compClass = typeof(CompAbilityEffect_WyvernFire);
		}
	}
}
