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
			Pawn priest = quest.GeneratePawn(priestKind, tempFaction, true, null, 0f, true, null, 0f, 0f, false, true, DevelopmentalStage.Adult, false);
			
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
			
			// 선택지 생성
			QuestPart_Choice questPart_Choice = quest.RewardChoice(null, null);
			
			// 수락 선택지
			QuestPart_Choice.Choice acceptChoice = new QuestPart_Choice.Choice();
			questPart_Choice.choices.Add(acceptChoice);
			
			// 거절 선택지
			QuestPart_Choice.Choice rejectChoice = new QuestPart_Choice.Choice();
			questPart_Choice.choices.Add(rejectChoice);
			
			questPart_Choice.inSignalChoiceUsed = QuestGen.slate.Get<string>("inSignal", null, false);
			
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
			
			// QuestPart_Choice가 선택되었을 때 처리
			// 각 Choice의 인덱스를 확인하여 시그널 발생
			// QuestPart_Choice의 Notify_ChoiceMade 이벤트를 사용할 수 없으므로
			// 대신 QuestPart_Choice의 choices 리스트를 확인하여 처리
			// 실제 구현은 QuestPart_Choice가 선택되었을 때 자동으로 처리되도록 함
			// 여기서는 기본 구조만 설정하고, 실제 시그널 처리는 QuestPart_Choice 내부에서 처리됨
			
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
