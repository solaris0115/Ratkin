using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using RimWorld;

namespace NewRatkin
{
	public class CompAbilityEffect_WyvernFire : CompAbilityEffect
	{
		private readonly List<IntVec3> tmpCells = new List<IntVec3>();

		private new CompProperties_AbilityWyvernFire Props
		{
			get
			{
				return (CompProperties_AbilityWyvernFire)this.props;
			}
		}

		private Pawn Pawn
		{
			get
			{
				return this.parent.pawn;
			}
		}

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			IntVec3 cell = target.Cell;
			Map mapHeld = this.parent.pawn.MapHeld;
			float radius = 0f;
			
			// Use configured damageDef instead of hardcoded Flame
			DamageDef damageDef = this.Props.damageDef ?? DamageDefOf.Bomb;
			
			Thing pawn = this.Pawn;
			int damAmount = this.Props.damAmount;
			if (damAmount == -1)
			{
				damAmount = damageDef.defaultDamage;
			}
			
			float armorPenetration = this.Props.armorPenetration;
			if (Mathf.Approximately(armorPenetration, -1f))
			{
				armorPenetration = damageDef.defaultArmorPenetration;
			}
			
			SoundDef explosionSound = null;
			ThingDef weapon = null;
			ThingDef projectile = null;
			Thing intendedTarget = null;
			ThingDef postExplosionSpawnThingDef = null;
			float postExplosionSpawnChance = 0f;
			int postExplosionSpawnThingCount = 0;
			SimpleCurve flammabilityAttachFireChanceCurve = null;
			List<IntVec3> overrideCells = this.AffectedCells(target);
			
			GenExplosion.DoExplosion(
				cell, mapHeld, radius, damageDef, pawn, 
				damAmount, armorPenetration, explosionSound, weapon, projectile, 
				intendedTarget, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, 
				null, null, 255, false, null, 0f, 1, 1f, false, 
				null, null, null, false, 0f, 0f, false, 
				null, 1f, flammabilityAttachFireChanceCurve, overrideCells, null, null);
			
			base.Apply(target, dest);
		}

		public override IEnumerable<PreCastAction> GetPreCastActions()
		{
			if (this.Props.effecterDef != null)
			{
				yield return new PreCastAction
				{
					action = delegate(LocalTargetInfo a, LocalTargetInfo b)
					{
						this.parent.AddEffecterToMaintain(this.Props.effecterDef.Spawn(this.parent.pawn.Position, a.Cell, this.parent.pawn.Map, 1f), this.Pawn.Position, a.Cell, 17, this.Pawn.MapHeld);
					},
					ticksAwayFromCast = 17
				};
			}
			yield break;
		}

		public override void DrawEffectPreview(LocalTargetInfo target)
		{
			GenDraw.DrawFieldEdges(this.AffectedCells(target), 2900);
		}

		public override bool AICanTargetNow(LocalTargetInfo target)
		{
			if (this.Pawn.Faction != null)
			{
				foreach (IntVec3 c in this.AffectedCells(target))
				{
					List<Thing> thingList = c.GetThingList(this.Pawn.Map);
					for (int i = 0; i < thingList.Count; i++)
					{
						if (thingList[i].Faction == this.Pawn.Faction)
						{
							return false;
						}
					}
				}
				return true;
			}
			return true;
		}

		private List<IntVec3> AffectedCells(LocalTargetInfo target)
		{
			this.tmpCells.Clear();
			Vector3 b = this.Pawn.Position.ToVector3Shifted().Yto0();
			IntVec3 intVec = target.Cell.ClampInsideMap(this.Pawn.Map);
			if (this.Pawn.Position == intVec)
			{
				return this.tmpCells;
			}
			float lengthHorizontal = (intVec - this.Pawn.Position).LengthHorizontal;
			float num = (float)(intVec.x - this.Pawn.Position.x) / lengthHorizontal;
			float num2 = (float)(intVec.z - this.Pawn.Position.z) / lengthHorizontal;
			intVec.x = Mathf.RoundToInt((float)this.Pawn.Position.x + num * this.Props.range);
			intVec.z = Mathf.RoundToInt((float)this.Pawn.Position.z + num2 * this.Props.range);
			float target2 = Vector3.SignedAngle(intVec.ToVector3Shifted().Yto0() - b, Vector3.right, Vector3.up);
			float num3 = this.Props.lineWidthEnd / 2f;
			float num4 = Mathf.Sqrt(Mathf.Pow((intVec - this.Pawn.Position).LengthHorizontal, 2f) + Mathf.Pow(num3, 2f));
			float num5 = 57.29578f * Mathf.Asin(num3 / num4);
			int num6 = GenRadial.NumCellsInRadius(this.Props.range);
			for (int i = 0; i < num6; i++)
			{
				IntVec3 intVec2 = this.Pawn.Position + GenRadial.RadialPattern[i];
				if (this.CanUseCell(intVec2) && Mathf.Abs(Mathf.DeltaAngle(Vector3.SignedAngle(intVec2.ToVector3Shifted().Yto0() - b, Vector3.right, Vector3.up), target2)) <= num5)
				{
					this.tmpCells.Add(intVec2);
				}
			}
			List<IntVec3> list = GenSight.BresenhamCellsBetween(this.Pawn.Position, intVec);
			for (int j = 0; j < list.Count; j++)
			{
				IntVec3 intVec3 = list[j];
				if (!this.tmpCells.Contains(intVec3) && this.CanUseCell(intVec3))
				{
					this.tmpCells.Add(intVec3);
				}
			}
			return this.tmpCells;
		}

		private bool CanUseCell(IntVec3 c)
		{
			ShootLine shootLine;
			return c.InBounds(this.Pawn.Map) && 
				!(c == this.Pawn.Position) && 
				(this.Props.canHitFilledCells || !c.Filled(this.Pawn.Map)) && 
				c.InHorDistOf(this.Pawn.Position, this.Props.range) && 
				this.parent.verb.TryFindShootLineFromTo(this.parent.pawn.Position, c, out shootLine, false);
		}
	}
}

