using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace NewRatkin
{
	/// <summary>
	/// 랫킨 유랑단 캐러반 전용 LordJob.
	/// 리더 중심 상호작용, Travel → Idle → Exit 흐름.
	/// </summary>
	public class LordJob_WanderingCaravan : LordJob
	{
		public Pawn leader;
		private IntVec3 chillSpot;
		private Faction faction;

		public LordJob_WanderingCaravan() { }

		public LordJob_WanderingCaravan(Pawn leader, IntVec3 chillSpot, Faction faction)
		{
			this.leader = leader;
			this.chillSpot = chillSpot;
			this.faction = faction;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();

			LordToil_WanderingCaravanTravel travel = new LordToil_WanderingCaravanTravel(chillSpot);
			stateGraph.StartingToil = travel; // setter가 이미 lordToils에 추가함. AddToil(travel) 중복 시 오류

			LordToil_WanderingCaravanIdle idle = new LordToil_WanderingCaravanIdle(chillSpot);
			stateGraph.AddToil(idle);

			LordToil_WanderingCaravanDefend defend = new LordToil_WanderingCaravanDefend();
			stateGraph.AddToil(defend);

			LordToil_ExitMap exitMap = new LordToil_ExitMap(LocomotionUrgency.None, false, false);
			stateGraph.AddToil(exitMap);

			LordToil_ExitMapAndDefendSelf exitDefend = new LordToil_ExitMapAndDefendSelf();
			stateGraph.AddToil(exitDefend);

			// TravelArrived → Idle
			Transition toIdle = new Transition(travel, idle, false, true);
			toIdle.AddTrigger(new Trigger_Memo("TravelArrived"));
			stateGraph.AddTransition(toIdle, false);

			// Idle: TicksPassed → Exit
			Transition toExitTime = new Transition(idle, exitMap, false, true);
			toExitTime.AddTrigger(new Trigger_TicksPassed(DebugSettings.instantVisitorsGift ? 0 : Rand.Range(27000, 45000)));
			toExitTime.AddPreAction(new TransitionAction_Custom(() => SaveCaravanToWorldPawns()));
			toExitTime.AddPreAction(new TransitionAction_Message("MessageTraderCaravanLeaving".Translate(faction.Name), null, 1f));
			toExitTime.AddPostAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(toExitTime, false);

			// Idle: CaravanDismissed → Exit (맵 끝까지 이동 후 퇴장, SaveCaravanToWorldPawns는 각 pawn ExitMap 시 Notify_PawnLost에서 처리)
			Transition toExitDismissed = new Transition(idle, exitMap, false, true);
			toExitDismissed.AddTrigger(new Trigger_Memo("CaravanDismissed"));
			toExitDismissed.AddPreAction(new TransitionAction_Message("MessageTraderCaravanDismissed".Translate(faction.Name), null, 1f));
			toExitDismissed.AddPostAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(toExitDismissed, false);

			// Idle/Travel: BecamePlayerEnemy → ExitDefend (플레이어 적대 시 즉시 퇴각)
			Transition toExitEnemy = new Transition(idle, exitDefend, false, true);
			toExitEnemy.AddSource(travel);
			toExitEnemy.AddSource(defend);
			toExitEnemy.AddTrigger(new Trigger_BecamePlayerEnemy());
			toExitEnemy.AddPostAction(new TransitionAction_WakeAll());
			toExitEnemy.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(toExitEnemy, false);

			// Idle/Travel: PawnHarmed → Defend (raid처럼 전투, 피해 누적 시 별도 전이로 퇴각)
			Transition toDefend = new Transition(idle, defend, false, true);
			toDefend.AddSource(travel);
			toDefend.AddPreAction(new TransitionAction_SetWanderingCaravanDefendPoint());
			toDefend.AddTrigger(new Trigger_PawnHarmed(1f, false, null, null, null));
			toDefend.AddPostAction(new TransitionAction_WakeAll());
			toDefend.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(toDefend, false);

			// Defend: 피해 20% 누적 → ExitDefend (퇴각)
			Transition defendToExit = new Transition(defend, exitDefend, false, true);
			defendToExit.AddTrigger(new Trigger_FractionPawnsLost(0.2f));
			defendToExit.AddPostAction(new TransitionAction_WakeAll());
			defendToExit.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(defendToExit, false);

			// Defend: 1200틱(약 20초) 무해 → Idle로 복귀 (Travel 중이었으면 Idle에서 chillSpot 근처 대기)
			Transition defendToIdle = new Transition(defend, idle, false, true);
			defendToIdle.AddTrigger(new Trigger_TicksPassedWithoutHarm(1200));
			stateGraph.AddTransition(defendToIdle, false);

			// Idle/Travel: DangerousTemperatures → Exit
			Transition toExitTemp = new Transition(idle, exitMap, false, true);
			toExitTemp.AddSource(travel);
			toExitTemp.AddSource(defend);
			toExitTemp.AddPreAction(new TransitionAction_Custom(() => SaveCaravanToWorldPawns()));
			toExitTemp.AddPreAction(new TransitionAction_Message("MessageVisitorsDangerousTemperature".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name), null, 1f));
			toExitTemp.AddPostAction(new TransitionAction_EndAllJobs());
			toExitTemp.AddTrigger(new Trigger_PawnExperiencingDangerousTemperatures());
			stateGraph.AddTransition(toExitTemp, false);

			return stateGraph;
		}

		private void SaveCaravanToWorldPawns()
		{
			Current.Game.GetComponent<GameComponent_WanderingCaravan>()?.OnCaravanExited(lord);
		}

		public override void Notify_PawnLost(Pawn p, PawnLostCondition cond)
		{
			base.Notify_PawnLost(p, cond);
			// 돌려보내기로 맵 끝 이동 후 퇴장 시, 각 pawn이 ExitMap할 때마다 roster에 추가 (WorldPawns KeepForever는 Pawn_ExitMap_Patch에서 처리)
			if (cond == PawnLostCondition.ExitedMap)
			{
				Current.Game.GetComponent<GameComponent_WanderingCaravan>()?.OnCaravanPawnExitedMap(p);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref leader, "leader");
			Scribe_Values.Look(ref chillSpot, "chillSpot");
			Scribe_References.Look(ref faction, "faction");
		}
	}
}
