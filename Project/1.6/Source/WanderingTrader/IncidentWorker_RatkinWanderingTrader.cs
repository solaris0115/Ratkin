using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;
using RimWorld.Planet;

namespace NewRatkin
{
	/// <summary>
	/// 랫킨 유랑단 캐러반 인시던트. hidden faction(RK_Faction_Caravan) 사용.
	/// 명부 기반 영속 인물 + 매년 충원. LordJob_WanderingCaravan 사용.
	/// </summary>
	public class IncidentWorker_RatkinWanderingTrader : IncidentWorker
	{
		private static readonly PawnKindDef[] SalePawnKinds = new[]
		{
			RatkinPawnKindDefOf.RK_PawnKind_Nomad,
			RatkinPawnKindDefOf.RK_PawnKind_Wanderer
		};

		private static readonly IntRange GuardCountRange = new IntRange(2, 4);

		private IncidentDefExtension_WanderingCaravan Ext => def.GetModExtension<IncidentDefExtension_WanderingCaravan>();

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
				return false;
			Map map = (Map)parms.target;
			foreach (GameCondition cond in map.GameConditionManager.ActiveConditions)
			{
				if (cond.def.preventNeutralVisitors)
					return false;
			}
			return true;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			var ext = Ext ?? new IncidentDefExtension_WanderingCaravan();
			int maxRoster = ext.maxRosterCount;
			IntRange yearlyRecruit = ext.yearlyRecruitRange;

			Faction faction = Find.FactionManager.FirstFactionOfDef(RatkinFactionDefOf.RK_Faction_Caravan);
			if (faction == null)
			{
				FactionGeneratorParms fgParms = new FactionGeneratorParms(
					RatkinFactionDefOf.RK_Faction_Caravan,
					default(IdeoGenerationParms),
					true);
				faction = FactionGenerator.NewGeneratedFaction(fgParms);
				Find.FactionManager.Add(faction);
			}
			faction.factionHostileOnHarmByPlayer = true;

			TraderKindDef traderKind = DefDatabase<TraderKindDef>.GetNamed("RK_TraderKind_WanderingTrader", false);
			if (traderKind == null)
				return false;

			if (!parms.spawnCenter.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, map, CellFinder.EdgeRoadChance_Neutral, false, null))
				return false;

			GameComponent_WanderingCaravan comp = Current.Game.GetComponent<GameComponent_WanderingCaravan>();
			if (comp == null)
				return false;

			Pawn leader;
			List<Pawn> guards;
			List<Pawn> salePawns;

			if (comp.IsRosterEmpty)
			{
				// 첫 방문: 전원 신규 생성, 유랑민은 풀에 추가 후 로스터 선택
				leader = CreateLeader(faction, map.Tile, traderKind);
				if (leader == null) return false;

				guards = CreateGuards(faction, map.Tile);
				var initialSettlers = CreateSettlers(faction, map.Tile, ext.initialSettlerCount);
				foreach (Pawn p in initialSettlers)
					comp.AddToPool(p, SettlementJoinRequirement.GenerateRandom());
				salePawns = comp.SelectRosterFromPool(maxRoster);
			}
			else
			{
				// 재방문: 사망/만료 정리, 풀 충원, 풀에서 로스터 선택
				comp.CleanupDeadPawns();
				if (comp.IsNewYearFor(map.Tile))
				{
					comp.RemoveExpiredFromPool(ext.expireAfterAppearances);
					comp.RefillPool(map, faction, yearlyRecruit.RandomInRange, ext.maxPoolSize);
					if (comp.PoolCount >= maxRoster)
						comp.RefillPool(map, faction, ext.overflowRecruitCount, ext.maxPoolSize);
				}
				comp.SelectRosterFromPool(maxRoster);
				comp.TakePawnsForSpawn(map, out leader, out guards, out salePawns);

				if (leader == null || leader.DestroyedOrNull() || leader.Dead)
				{
					leader = CreateLeader(faction, map.Tile, traderKind);
					if (leader == null) return false;
				}
				else
				{
					leader.mindState.wantsToTradeWithColony = true;
					PawnComponentsUtility.AddAndRemoveDynamicComponents(leader, true);
					leader.trader.traderKind = traderKind;
				}
				CleanupAndRefillGuards(guards, faction, map.Tile);
			}

			// 리더 인벤토리에 거래용 물품 추가
			ThingSetMakerParams stockParms = default(ThingSetMakerParams);
			stockParms.traderDef = traderKind;
			stockParms.tile = new PlanetTile?(map.Tile);
			stockParms.makingFaction = faction;
			foreach (Thing thing in ThingSetMakerDefOf.TraderStock.root.Generate(stockParms))
			{
				Pawn stockPawn = thing as Pawn;
				if (stockPawn != null)
				{
					if (stockPawn.Faction != faction)
						stockPawn.SetFaction(faction, null);
					salePawns.Add(stockPawn);
				}
				else if (!leader.inventory.innerContainer.TryAdd(thing, true))
				{
					thing.Destroy(DestroyMode.Vanish);
				}
			}
			PawnInventoryGenerator.GiveRandomFood(leader);

			List<Pawn> allPawns = new List<Pawn> { leader };
			allPawns.AddRange(guards);
			allPawns.AddRange(salePawns);

			foreach (Pawn p in allPawns)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 5, null);
				GenSpawn.Spawn(p, loc, map, WipeMode.Vanish);
				if (p.needs?.food != null)
					p.needs.food.CurLevel = p.needs.food.MaxLevel;
			}

			IntVec3 chillSpot;
			if (!RCellFinder.TryFindRandomSpotJustOutsideColony(leader.Position, map, leader, out chillSpot, c =>
			{
				foreach (Pawn p in allPawns)
				{
					if (!p.CanReach(c, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
						return false;
				}
				return true;
			}))
			{
				return false;
			}

			comp.UpdateLastVisitYear(map.Tile);

			LordJob_WanderingCaravan lordJob = new LordJob_WanderingCaravan(leader, chillSpot, faction);
			LordMaker.MakeNewLord(faction, lordJob, map, allPawns);

			TaggedString label = "RK_WanderingCaravan_ArrivalLetterLabel".Translate();
			TaggedString text = "RK_WanderingCaravan_ArrivalLetter".Translate(faction.NameColored);
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(allPawns, ref label, ref text,
				"LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), true, true);
			SendStandardLetter(label, text, LetterDefOf.PositiveEvent, parms, leader, Array.Empty<NamedArgument>());

			return true;
		}

		private static Pawn CreateLeader(Faction faction, int tile, TraderKindDef traderKind)
		{
			Pawn leader = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
				RatkinPawnKindDefOf.RK_PawnKind_CaravanLeader, faction, PawnGenerationContext.NonPlayer, tile,
				false, false, false, true, false, 1f, true, true, false, true, true,
				false, false, false, false, 0f, 0f, null, 1f, null, null, null, null,
				null, null, null, null, null, null, null, null, false, false, false, false,
				null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null,
				false, false, false, -1, 0, false));
			if (leader == null) return null;

			leader.mindState.wantsToTradeWithColony = true;
			PawnComponentsUtility.AddAndRemoveDynamicComponents(leader, true);
			leader.trader.traderKind = traderKind;
			return leader;
		}

		private static List<Pawn> CreateGuards(Faction faction, int tile)
		{
			int guardCount = GuardCountRange.RandomInRange;
			var guards = new List<Pawn>();
			for (int i = 0; i < guardCount; i++)
			{
				Pawn guard = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
					RatkinPawnKindDefOf.RK_PawnKind_CaravanGuard, faction, PawnGenerationContext.NonPlayer, tile,
					false, false, false, true, true, 1f, true, true, false, true, true,
					false, false, false, false, 0f, 0f, null, 1f, null, null, null, null,
					null, null, null, null, null, null, null, null, false, false, false, false,
					null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null,
					false, false, false, -1, 0, false));
				if (guard != null) guards.Add(guard);
			}
			return guards;
		}

		private static List<Pawn> CreateSettlers(Faction faction, int tile, int count)
		{
			var list = new List<Pawn>();
			for (int i = 0; i < count; i++)
			{
				PawnKindDef kind = SalePawnKinds.RandomElement();
				Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
					kind, faction, PawnGenerationContext.NonPlayer, tile,
					false, false, false, true, kind.isFighter, 1f, true, true, false, true, true,
					false, false, false, false, 0f, 0f, null, 1f, null, null, null, null,
					null, null, null, null, null, null, null, null, false, false, false, false,
					null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null,
					false, false, false, -1, 0, false));
				if (pawn != null) list.Add(pawn);
			}
			return list;
		}

		private static void CleanupAndRefillGuards(List<Pawn> guards, Faction faction, int tile)
		{
			guards.RemoveAll(p => p == null || p.DestroyedOrNull() || p.Dead);
			int need = GuardCountRange.min - guards.Count;
			if (need > 0)
			{
				for (int i = 0; i < need; i++)
				{
					Pawn guard = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
						RatkinPawnKindDefOf.RK_PawnKind_CaravanGuard, faction, PawnGenerationContext.NonPlayer, tile,
						false, false, false, true, true, 1f, true, true, false, true, true,
						false, false, false, false, 0f, 0f, null, 1f, null, null, null, null,
						null, null, null, null, null, null, null, null, false, false, false, false,
						null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null,
						false, false, false, -1, 0, false));
					if (guard != null) guards.Add(guard);
				}
			}
		}
	}
}
