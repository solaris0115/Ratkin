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

			LordToil_Travel travel = new LordToil_Travel(chillSpot);
			stateGraph.StartingToil = travel; // setter가 이미 lordToils에 추가함. AddToil(travel) 중복 시 오류

			LordToil_WanderingCaravanIdle idle = new LordToil_WanderingCaravanIdle(chillSpot);
			stateGraph.AddToil(idle);

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

			// Idle: CaravanDismissed → Exit
			Transition toExitDismissed = new Transition(idle, exitMap, false, true);
			toExitDismissed.AddTrigger(new Trigger_Memo("CaravanDismissed"));
			toExitDismissed.AddPreAction(new TransitionAction_Custom(() => SaveCaravanToWorldPawns()));
			toExitDismissed.AddPreAction(new TransitionAction_Message("MessageTraderCaravanDismissed".Translate(faction.Name), null, 1f));
			toExitDismissed.AddPostAction(new TransitionAction_WakeAll());
			stateGraph.AddTransition(toExitDismissed, false);

			// Idle/Travel: BecamePlayerEnemy → ExitDefend
			Transition toExitEnemy = new Transition(idle, exitDefend, false, true);
			toExitEnemy.AddSource(travel);
			toExitEnemy.AddTrigger(new Trigger_BecamePlayerEnemy());
			toExitEnemy.AddPostAction(new TransitionAction_WakeAll());
			toExitEnemy.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(toExitEnemy, false);

			// Idle/Travel: PawnHarmed → ExitDefend (방어 모드)
			Transition toExitHarmed = new Transition(idle, exitDefend, false, true);
			toExitHarmed.AddSource(travel);
			toExitHarmed.AddTrigger(new Trigger_PawnHarmed(1f, false, null, null, null));
			toExitHarmed.AddPostAction(new TransitionAction_WakeAll());
			toExitHarmed.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(toExitHarmed, false);

			// Idle/Travel: DangerousTemperatures → Exit
			Transition toExitTemp = new Transition(idle, exitMap, false, true);
			toExitTemp.AddSource(travel);
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

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref leader, "leader");
			Scribe_Values.Look(ref chillSpot, "chillSpot");
			Scribe_References.Look(ref faction, "faction");
		}
	}
}
