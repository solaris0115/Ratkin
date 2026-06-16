using RimWorld;
using UnityEngine;
using Verse;

namespace NewRatkin
{
	public class GeneDrawScaleDef : GeneDef
	{
		public float drawScale = 1f;
		public int reapplyIntervalTicks = 60;
	}

	public class Gene_DrawScale : Gene
	{
		private const float MinDrawScale = 0.01f;
		private const int DefaultReapplyIntervalTicks = 60;

		private GeneDrawScaleDef DrawScaleDef
		{
			get
			{
				return def as GeneDrawScaleDef;
			}
		}

		private float DrawScale
		{
			get
			{
				GeneDrawScaleDef drawScaleDef = DrawScaleDef;
				return Mathf.Max(MinDrawScale, drawScaleDef != null ? drawScaleDef.drawScale : 1f);
			}
		}

		private int ReapplyIntervalTicks
		{
			get
			{
				GeneDrawScaleDef drawScaleDef = DrawScaleDef;
				return Mathf.Max(1, drawScaleDef != null ? drawScaleDef.reapplyIntervalTicks : DefaultReapplyIntervalTicks);
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

		public override void TickInterval(int delta)
		{
			base.TickInterval(delta);
			if (pawn != null && pawn.IsHashIntervalTick(ReapplyIntervalTicks, delta))
			{
				ApplyScaleToPawn(pawn, DrawScale, rebuildTree: false, dirtyCaches: false);
			}
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

			PawnRenderNode rootNode = renderTree.rootNode;
			if (rootNode == null)
			{
				return;
			}

			bool scaleChanged = !Mathf.Approximately(rootNode.debugScale, scale);
			if (scaleChanged)
			{
				rootNode.debugScale = scale;
			}
			if (dirtyCaches && (scaleChanged || rebuildTree))
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
}
