using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace NewRatkin
{
	public class PawnFlyer_LanceCharge : PawnFlyer
	{
		private static readonly FieldInfo targetField =
			typeof(PawnFlyer).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);

		private LocalTargetInfo chargeTarget;

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad && targetField != null)
			{
				chargeTarget = (LocalTargetInfo)targetField.GetValue(this);
			}
		}

		protected override void TickInterval(int delta)
		{
			Pawn pawn = FlyingPawn;
			if (pawn != null && chargeTarget.IsValid)
			{
				Vector3 targetPos = chargeTarget.HasThing
					? chargeTarget.Thing.DrawPos
					: chargeTarget.Cell.ToVector3Shifted();
				Vector3 direction = targetPos - DrawPos;
				if (direction.MagnitudeHorizontalSquared() > 0.001f)
				{
					pawn.Rotation = Pawn_RotationTracker.RotFromAngleBiased(direction.AngleFlat());
				}
			}
			base.TickInterval(delta);
		}

		protected override void RespawnPawn()
		{
			Pawn pawn = FlyingPawn;
			LocalTargetInfo savedTarget = chargeTarget;

			if (savedTarget.IsValid)
			{
				Vector3 targetPos = savedTarget.HasThing
					? savedTarget.Thing.DrawPos
					: savedTarget.Cell.ToVector3Shifted();
				Vector3 dir = targetPos - DestinationPos;
				if (dir.MagnitudeHorizontalSquared() > 0.001f)
				{
					base.Rotation = Pawn_RotationTracker.RotFromAngleBiased(dir.AngleFlat());
				}
			}

			base.RespawnPawn();

			if (pawn != null && pawn.Spawned && savedTarget.IsValid)
			{
				pawn.stances.SetStance(new Stance_Cooldown(30, savedTarget, null));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_TargetInfo.Look(ref chargeTarget, "chargeTarget");
		}
	}
}
