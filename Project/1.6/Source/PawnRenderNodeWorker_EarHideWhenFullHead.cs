using RimWorld;
using UnityEngine;

namespace Verse
{
	public class PawnRenderNodeWorker_EarHideWhenFullHead : PawnRenderNodeWorker_FlipWhenCrawling
	{
		public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
		{
			// FullHead 타입의 장비를 착용했는지 확인
			if (parms.pawn.apparel != null)
			{
				foreach (Apparel apparel in parms.pawn.apparel.WornApparel)
				{
					if (apparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead))
					{
						return false; // FullHead 착용 시 귀 숨김
					}
				}
			}
			
			return base.CanDrawNow(node, parms);
		}
	}
}

