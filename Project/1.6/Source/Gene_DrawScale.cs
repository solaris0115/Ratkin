using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace NewRatkin
{
	public class GeneDrawScaleDef : GeneDef
	{
		public float drawScale = 1f;
	}

	public class Gene_DrawScale : Gene
	{
		private const float MinDrawScale = 0.01f;

		private GeneDrawScaleDef DrawScaleDef
		{
			get
			{
				return def as GeneDrawScaleDef;
			}
		}

		internal float DrawScale
		{
			get
			{
				GeneDrawScaleDef drawScaleDef = DrawScaleDef;
				return Mathf.Max(MinDrawScale, drawScaleDef != null ? drawScaleDef.drawScale : 1f);
			}
		}

		public override void PostAdd()
		{
			base.PostAdd();
			if (Active)
			{
				DirtyPawnDrawCaches(pawn, rebuildTree: true);
			}
		}

		public override void PostRemove()
		{
			base.PostRemove();
			DirtyPawnDrawCaches(pawn, rebuildTree: true);
		}

		internal static float DrawScaleForPawn(Pawn pawn, Gene ignoredGene = null)
		{
			if (pawn == null || pawn.genes == null)
			{
				return 1f;
			}
			foreach (Gene gene in pawn.genes.GenesListForReading)
			{
				if (gene == ignoredGene || !gene.Active)
				{
					continue;
				}
				GeneDrawScaleDef drawScaleDef = gene.def as GeneDrawScaleDef;
				if (drawScaleDef != null)
				{
					return Mathf.Max(MinDrawScale, drawScaleDef.drawScale);
				}
			}
			return 1f;
		}

		internal static Vector3 ScaleDrawLocFromRoot(Vector3 drawLoc, Vector3 rootLoc, float scale)
		{
			if (Mathf.Approximately(scale, 1f))
			{
				return drawLoc;
			}
			drawLoc.x = rootLoc.x + (drawLoc.x - rootLoc.x) * scale;
			drawLoc.z = rootLoc.z + (drawLoc.z - rootLoc.z) * scale;
			return drawLoc;
		}

		private static void DirtyPawnDrawCaches(Pawn pawn, bool rebuildTree)
		{
			if (pawn == null || pawn.Drawer == null || pawn.Drawer.renderer == null)
			{
				return;
			}

			PawnRenderer renderer = pawn.Drawer.renderer;
			PawnRenderTree renderTree = renderer.renderTree;
			if (rebuildTree && renderTree != null)
			{
				renderTree.SetDirty();
			}
			SilhouetteUtility.NotifyGraphicDirty(pawn);
			PortraitsCache.SetDirty(pawn);
			GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
		}
	}

	public class PawnRenderNodeWorker_DrawScale : PawnRenderNodeWorker
	{
		public override bool ShouldListOnGraph(PawnRenderNode node, PawnDrawParms parms)
		{
			return false;
		}

		public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
		{
			GeneDrawScaleDef drawScaleDef = node != null && node.gene != null ? node.gene.def as GeneDrawScaleDef : null;
			if (drawScaleDef != null && node.gene.Active)
			{
				ApplyScaleToRootVisualNodes(node.tree, Mathf.Max(0.01f, drawScaleDef.drawScale));
			}
			return false;
		}

		private static void ApplyScaleToRootVisualNodes(PawnRenderTree tree, float drawScale)
		{
			PawnRenderNode rootNode = tree != null ? tree.rootNode : null;
			if (rootNode == null || rootNode.children == null)
			{
				return;
			}

			for (int i = 0; i < rootNode.children.Length; i++)
			{
				PawnRenderNode child = rootNode.children[i];
				if (child != null && child.Props != null && child.Props.useGraphic && !Mathf.Approximately(child.debugScale, drawScale))
				{
					child.debugScale = drawScale;
				}
			}
		}
	}

	[StaticConstructorOnStartup]
	internal static class GeneDrawScaleRenderPatches
	{
		private const string HarmonyId = "com.NewRatkin.rimworld.mod.genedrawscale";

		[System.ThreadStatic]
		private static List<EquipmentDrawContext> equipmentDrawContexts;

		static GeneDrawScaleRenderPatches()
		{
			Harmony harmony = new Harmony(HarmonyId);
			harmony.Patch(
				AccessTools.Method(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAndApparelExtras)),
				prefix: new HarmonyMethod(typeof(GeneDrawScaleRenderPatches), nameof(DrawEquipmentAndApparelExtras_Prefix)),
				postfix: new HarmonyMethod(typeof(GeneDrawScaleRenderPatches), nameof(DrawEquipmentAndApparelExtras_Postfix)));
			harmony.Patch(
				AccessTools.Method(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming)),
				prefix: new HarmonyMethod(typeof(GeneDrawScaleRenderPatches), nameof(DrawEquipmentAiming_Prefix)));
		}

		private static void DrawEquipmentAndApparelExtras_Prefix(Pawn pawn)
		{
			if (equipmentDrawContexts == null)
			{
				equipmentDrawContexts = new List<EquipmentDrawContext>();
			}
			equipmentDrawContexts.Add(new EquipmentDrawContext(Gene_DrawScale.DrawScaleForPawn(pawn), pawn != null ? pawn.DrawPos : Vector3.zero));
		}

		private static void DrawEquipmentAndApparelExtras_Postfix()
		{
			if (equipmentDrawContexts == null || equipmentDrawContexts.Count == 0)
			{
				return;
			}
			equipmentDrawContexts.RemoveAt(equipmentDrawContexts.Count - 1);
		}

		private static bool DrawEquipmentAiming_Prefix(Thing eq, Vector3 drawLoc, float aimAngle)
		{
			EquipmentDrawContext context;
			if (eq == null || !TryGetEquipmentDrawContext(eq, out context) || Mathf.Approximately(context.Scale, 1f))
			{
				return true;
			}

			DrawEquipmentAimingScaled(eq, Gene_DrawScale.ScaleDrawLocFromRoot(drawLoc, context.RootLoc, context.Scale), aimAngle, context.Scale);
			return false;
		}

		private static bool TryGetEquipmentDrawContext(Thing eq, out EquipmentDrawContext context)
		{
			Pawn_EquipmentTracker equipmentTracker = eq.ParentHolder as Pawn_EquipmentTracker;
			Pawn pawn = equipmentTracker != null ? equipmentTracker.pawn : null;
			if (pawn != null && pawn.equipment != null && pawn.equipment.Primary == eq)
			{
				context = new EquipmentDrawContext(Gene_DrawScale.DrawScaleForPawn(pawn), pawn.DrawPos);
				return true;
			}

			if (equipmentDrawContexts != null && equipmentDrawContexts.Count != 0)
			{
				context = equipmentDrawContexts[equipmentDrawContexts.Count - 1];
				return true;
			}

			context = EquipmentDrawContext.Default;
			return false;
		}

		private static void DrawEquipmentAimingScaled(Thing eq, Vector3 drawLoc, float aimAngle, float drawScale)
		{
			float num = aimAngle - 90f;
			Mesh mesh;
			if (aimAngle > 20f && aimAngle < 160f)
			{
				mesh = MeshPool.plane10;
				num += eq.def.equippedAngleOffset;
			}
			else if (aimAngle > 200f && aimAngle < 340f)
			{
				mesh = MeshPool.plane10Flip;
				num -= 180f;
				num -= eq.def.equippedAngleOffset;
			}
			else
			{
				mesh = MeshPool.plane10;
				num += eq.def.equippedAngleOffset;
			}
			num %= 360f;
			CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
			if (compEquippable != null)
			{
				Vector3 recoilOffset;
				float recoilAngle;
				EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out recoilOffset, out recoilAngle, aimAngle);
				drawLoc += recoilOffset;
				num += recoilAngle;
			}
			Graphic_StackCount graphicStackCount = eq.Graphic as Graphic_StackCount;
			Material material = graphicStackCount != null ? graphicStackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq) : eq.Graphic.MatSingleFor(eq);
			Vector3 s = new Vector3(eq.Graphic.drawSize.x * drawScale, 0f, eq.Graphic.drawSize.y * drawScale);
			Matrix4x4 matrix = Matrix4x4.TRS(drawLoc, Quaternion.AngleAxis(num, Vector3.up), s);
			Graphics.DrawMesh(mesh, matrix, material, 0);
		}

		private struct EquipmentDrawContext
		{
			internal static readonly EquipmentDrawContext Default = new EquipmentDrawContext(1f, Vector3.zero);

			internal readonly float Scale;

			internal readonly Vector3 RootLoc;

			internal EquipmentDrawContext(float scale, Vector3 rootLoc)
			{
				Scale = scale;
				RootLoc = rootLoc;
			}
		}
	}
}
