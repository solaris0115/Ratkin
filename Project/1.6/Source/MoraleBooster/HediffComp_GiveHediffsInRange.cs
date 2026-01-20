using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace NewRatkin
{
	public class HediffComp_GiveHediffsInRange : HediffComp
	{
		private Mote mote;

		public HediffCompProperties_GiveHediffsInRange Props
		{
			get
			{
				return (HediffCompProperties_GiveHediffsInRange)this.props;
			}
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			Pawn caster = this.parent.pawn;
			
			if (!caster.Awake() || caster.health == null || caster.health.InPainShock || !caster.Spawned)
			{
				return;
			}
			if (!this.Props.hideMoteWhenNotDrafted || caster.Drafted)
			{
				if (this.Props.mote != null && (this.mote == null || this.mote.Destroyed))
				{
					this.mote = MoteMaker.MakeAttachedOverlay(caster, this.Props.mote, Vector3.zero, 1f, -1f);
				}
				if (this.mote != null)
				{
					this.mote.Maintain();
				}
			}
			IReadOnlyList<Pawn> readOnlyList;
			if (this.Props.onlyPawnsInSameFaction && caster.Faction != null)
			{
				readOnlyList = caster.Map.mapPawns.SpawnedPawnsInFaction(caster.Faction);
			}
			else
			{
				readOnlyList = caster.Map.mapPawns.AllPawnsSpawned;
			}
			foreach (Pawn pawn in readOnlyList)
			{
				if (!pawn.RaceProps.Humanlike || pawn.Dead || pawn.health == null || pawn == caster)
				{
					continue;
				}
				if (pawn.Position.DistanceTo(caster.Position) > this.Props.range)
				{
					continue;
				}
				if (this.Props.targetingParameters != null && !this.Props.targetingParameters.CanTarget(pawn, null))
				{
					continue;
				}
				Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(this.Props.hediff, false);
				if (hediff == null)
				{
					hediff = pawn.health.AddHediff(this.Props.hediff, pawn.health.hediffSet.GetBrain(), null, null);
					hediff.Severity = this.Props.initialSeverity;
					
					// HediffComp_Link의 other 설정 (필수: other가 null이면 CompShouldRemove가 true가 되어 Hediff가 즉시 제거됨)
					HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
					if (hediffComp_Link != null)
					{
						hediffComp_Link.other = caster;
						hediffComp_Link.drawConnection = false;
					}
				}
				else
				{
					// 기존 Hediff의 other도 확인 및 업데이트
					HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
					if (hediffComp_Link != null && hediffComp_Link.other != caster)
					{
						hediffComp_Link.other = caster;
					}
				}
				HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
				if (hediffComp_Disappears == null)
				{
					Log.Error("HediffComp_GiveHediffsInRange has a hediff in props which does not have a HediffComp_Disappears");
				}
				else
				{
					hediffComp_Disappears.ticksToDisappear = 5;
				}
			}
		}
	}
}

