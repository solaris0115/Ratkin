using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace NewRatkin
{
	/// <summary>
	/// 랫킨 사제 합류 퀘스트 노드
	/// 바닐라 WalkIn 퀘스트의 버전별 생성 진입점을 랫킨 사제 생성으로 연결한다.
	/// </summary>
	public class QuestNode_Root_PriestJoin : QuestNode_Root_WandererJoin_WalkIn
	{
		public override Pawn GeneratePawn()
		{
			return GeneratePriestPawn(null);
		}

#if RIMWORLD_1_7
		// 1.7의 RunInt는 GeneratePawn 대신 맵을 전달하는 진입점을 호출한다.
		public override Pawn GeneratePawn_NewTemp(Map map)
		{
			return GeneratePriestPawn(map);
		}
#endif

		private Pawn GeneratePriestPawn(Map map)
		{
			// 랫킨 사제 PawnKindDef 가져오기
			PawnKindDef priestKind = DefDatabase<PawnKindDef>.GetNamed("RatkinPriest", false);
			if (priestKind == null)
			{
#if RIMWORLD_1_7
				return base.GeneratePawn_NewTemp(map);
#else
				return base.GeneratePawn();
#endif
			}

			PawnGenerationRequest request = new PawnGenerationRequest(
				priestKind,
				faction: null, // 수락 시 플레이어 팩션으로 설정
				tile: map?.Tile,
				forceGenerateNewPawn: true,
				colonistRelationChanceFactor: 20f,
				allowPregnant: true,
				forceRecruitable: true
			);

			// 아이 허용 설정 확인
			if (Find.Storyteller.difficulty.ChildrenAllowed)
			{
				request.AllowedDevelopmentalStages |= DevelopmentalStage.Child;
			}

			Pawn pawn = PawnGenerator.GeneratePawn(request);

			// PawnGenerator 패치가 적용되지 않는 경우 대비 - QuestNode에서 직접 기도회 능력 부여
			if (pawn.DevelopmentalStage.Adult() && pawn.abilities != null && pawn.abilities.GetAbility(RatkinAbilityDefOf.RK_PrayerService) == null)
			{
				pawn.abilities.GainAbility(RatkinAbilityDefOf.RK_PrayerService);
			}

			if (!pawn.IsWorldPawn())
			{
				Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
			}

			return pawn;
		}
	}
}
