using System;
using RimWorld;
using Verse;

namespace NewRatkin
{
	public class HediffCompProperties_GiveHediffsInRange : HediffCompProperties
	{
		public float range;

		public TargetingParameters targetingParameters;

		public HediffDef hediff;

		public ThingDef mote;

		public bool hideMoteWhenNotDrafted;

		public float initialSeverity = 1f;

		public bool onlyPawnsInSameFaction = true;

		public HediffCompProperties_GiveHediffsInRange()
		{
			this.compClass = typeof(HediffComp_GiveHediffsInRange);
		}
	}
}

