using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace NewRatkin
{
	// 랫킨 사제 합류 퀘스트 노드
	public class QuestNode_Root_PriestJoin : QuestNode
	{
		protected override void RunInt()
		{
			Quest quest = QuestGen.quest;
			Slate slate = QuestGen.slate;
			Map map = QuestGen_Get.GetMap(false, null, false);
			
			if (map == null)
			{
				return;
			}
			
			// 랫킨 사제 생성
			PawnKindDef priestKind = RatkinPawnKindDefOf.RatkinPriest;
			if (priestKind == null)
			{
				return;
			}
			
			// 중립 팩션 생성 (임시)
			FactionDef factionDef = FactionDefOf.OutlanderCivil;
			List<FactionRelation> relations = new List<FactionRelation>();
			foreach (Faction faction in Find.FactionManager.AllFactionsListForReading)
			{
				if (!faction.def.PermanentlyHostileTo(factionDef))
				{
					relations.Add(new FactionRelation
					{
						other = faction,
						kind = FactionRelationKind.Neutral
					});
				}
			}
			Faction tempFaction = FactionGenerator.NewGeneratedFactionWithRelations(factionDef, relations, true);
			tempFaction.temporary = true;
			Find.FactionManager.Add(tempFaction);
			
			// 사제 Pawn 생성
			Pawn priest = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
				priestKind,
				tempFaction,
				PawnGenerationContext.NonPlayer,
				null,
				true,
				false,
				false,
				true,
				false, // pawnMustBeCapableOfViolence = false (사제는 폭력 불가능 가능)
				20f,
				false,
				true,
				false,
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
				null,
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
				false,
				false,
				false,
				-1,
				0,
				false
			));
			
			quest.SetFactionHidden(tempFaction, false, null);
			
			// 사제 도착
			quest.PawnsArrive(
				new List<Pawn> { priest },
				null,
				map.Parent,
				null,
				false,
				null,
				"[priestArrivedLetterLabel]",
				"[priestArrivedLetterText]",
				null,
				null,
				false,
				false,
				true
			);
			
			// 선택지 처리 시그널
			string acceptSignal = QuestGen.GenerateNewSignal("PriestAccepted", true);
			string rejectSignal = QuestGen.GenerateNewSignal("PriestRejected", true);
			string postponeSignal = QuestGen.GenerateNewSignal("PriestPostponed", true);
			
			// 선택지 생성 (보상 선택이 아닌 플레이어 선택을 위한 구조)
			// QuestPart_Choice는 보상 선택용이므로, 여기서는 시그널 기반으로 처리
			// 실제 선택은 Interaction을 통해 처리되거나 다른 QuestPart를 사용해야 함
			
			// 수락 시: 사제를 플레이어 세력에 추가
			quest.Signal(acceptSignal, delegate
			{
				priest.SetFaction(Faction.OfPlayer);
				quest.Message("[priestAcceptedMessage]", MessageTypeDefOf.PositiveEvent, false, null, new List<Pawn> { priest }, null);
			});
			
			// 거절 시: 사제를 맵에서 제거
			quest.Signal(rejectSignal, delegate
			{
				if (priest.Spawned)
				{
					priest.DeSpawn();
				}
				Find.WorldPawns.PassToWorld(priest);
				quest.Message("[priestRejectedMessage]", MessageTypeDefOf.NeutralEvent, false, null, null, null);
			});
			
			// 미루기 시: 사제는 맵에 남아있음
			quest.Signal(postponeSignal, delegate
			{
				quest.Message("[priestPostponedMessage]", MessageTypeDefOf.NeutralEvent, false, null, new List<Pawn> { priest }, null);
			});
			
			// 퀘스트 종료
			quest.End(QuestEndOutcome.Success, 0, null, acceptSignal, QuestPart.SignalListenMode.OngoingOnly, false, false);
			quest.End(QuestEndOutcome.Fail, 0, null, rejectSignal, QuestPart.SignalListenMode.OngoingOnly, false, false);
			quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("map.MapRemoved"), QuestPart.SignalListenMode.OngoingOnly, true, false);
			
			// Slate에 데이터 저장
			slate.Set<Map>("map", map, false);
			slate.Set<List<Pawn>>("pawns", new List<Pawn> { priest }, false);
			slate.Set<Pawn>("pawns0", priest, false);
			slate.Set<Faction>("faction", tempFaction, false);
		}
		
		protected override bool TestRunInt(Slate slate)
		{
			Map map = QuestGen_Get.GetMap(false, null, false);
			return map != null && RatkinPawnKindDefOf.RatkinPriest != null;
		}
	}
}
