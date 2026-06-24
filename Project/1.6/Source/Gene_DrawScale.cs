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
				ApplyScaleToPawn(pawn, DrawScale, rebuildTree: true, dirtyCaches: true);
			}
		}

		public override void PostRemove()
		{
			base.PostRemove();
			ApplyScaleToPawn(pawn, DrawScaleForPawn(pawn, this), rebuildTree: true, dirtyCaches: true);
		}

		private static float DrawScaleForPawn(Pawn pawn, Gene ignoredGene)
		{
			if (pawn == null || pawn.genes == null)
			{
				return 1f;
			}
			foreach (Gene gene in pawn.genes.GenesListForReading)
			{
				Gene_DrawScale drawScaleGene = gene as Gene_DrawScale;
				if (drawScaleGene != null && gene != ignoredGene && gene.Active)
				{
					return drawScaleGene.DrawScale;
				}
			}
			return 1f;
		}

		internal static void ApplyScaleToRenderTree(PawnRenderTree renderTree, float scale)
		{
			PawnRenderNode rootNode = renderTree != null ? renderTree.rootNode : null;
			if (rootNode != null && !Mathf.Approximately(rootNode.debugScale, scale))
			{
				rootNode.debugScale = scale;
			}
		}

		private static void ApplyScaleToPawn(Pawn pawn, float scale, bool rebuildTree, bool dirtyCaches)
		{
			if (pawn == null || pawn.Drawer == null || pawn.Drawer.renderer == null)
			{
				return;
			}

			PawnRenderer renderer = pawn.Drawer.renderer;
			PawnRenderTree renderTree = renderer.renderTree;
			if (renderTree == null)
			{
				return;
			}

			if (rebuildTree)
			{
				renderTree.SetDirty();
			}
			renderer.EnsureGraphicsInitialized();

			float scaleBefore = renderTree.rootNode != null ? renderTree.rootNode.debugScale : 1f;
			ApplyScaleToRenderTree(renderTree, scale);
			if (dirtyCaches && (rebuildTree || !Mathf.Approximately(scaleBefore, scale)))
			{
				DirtyVisualCaches(pawn);
			}
		}

		private static void DirtyVisualCaches(Pawn pawn)
		{
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
			Gene_DrawScale drawScaleGene = node.gene as Gene_DrawScale;
			if (drawScaleGene != null && drawScaleGene.Active)
			{
				Gene_DrawScale.ApplyScaleToRenderTree(node.tree, drawScaleGene.DrawScale);
			}
			return false;
		}
	}
}
