using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace NewRatkin
{
	public class QuestNode_Root_WandererJoin_Priest : QuestNode_Root_WandererJoin_WalkIn
	{
		public override Pawn GeneratePawn()
		{
			Slate slate = QuestGen.slate;
			Gender? gender = null;
			PawnGenerationRequest request;
			
			// RatkinPriest 명시적 사용
			PawnKindDef priestKind = PawnKindDef.Named("RatkinPriest");
			Faction faction = null;
			PawnGenerationContext context = PawnGenerationContext.NonPlayer;
			Gender? fixedGender = gender;
			
			request = new PawnGenerationRequest(
				priestKind, 
				faction, 
				context, 
				null, 
				true, 
				false, 
				false, 
				true, 
				false, 
				20f, 
				false, 
				true, 
				true, 
				true, 
				true, 
				false, 
				false, 
				false, 
				false, 
				0f, 
				0f, 
				null, 
				1f, 
				null, 
				null, 
				null, 
				null, 
				null, 
				null, 
				null, 
				fixedGender, 
				null, 
				null, 
				null, 
				null, 
				false, 
				false, 
				false, 
				false, 
				null, 
				null, 
				null, 
				null, 
				null, 
				0f, 
				DevelopmentalStage.Adult, 
				null, 
				null, 
				null, 
				true, 
				false, 
				false, 
				-1, 
				0, 
				false
			);
			
			if (Find.Storyteller.difficulty.ChildrenAllowed)
			{
				request.AllowedDevelopmentalStages |= DevelopmentalStage.Child;
			}
			
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			if (!pawn.IsWorldPawn())
			{
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
			}
			return pawn;
		}
	}
}

