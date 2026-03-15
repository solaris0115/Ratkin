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
	/// 랫킨 유랑 상인 인시던트. hidden faction(RK_Faction_Caravan) 사용으로 공격해도 우호도 변화 없음.
	/// DevMode에서 수동 발동 가능 (baseChance=0).
	/// </summary>
	public class IncidentWorker_RatkinWanderingTrader : IncidentWorker
	{
		private static readonly PawnKindDef[] SalePawnKinds = new[]
		{
			RatkinPawnKindDefOf.RK_PawnKind_Nomad,
			RatkinPawnKindDefOf.RK_PawnKind_WanderingMercenary
		};

		private static readonly IntRange SalePawnCountRange = new IntRange(3, 6);

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

			// RK_Faction_Caravan 획득 또는 생성 (requiredCountAtGameStart=0이므로 없을 수 있음)
			Faction faction = Find.FactionManager.FirstFactionOfDef(RatkinFactionDefOf.RK_Faction_Caravan);
			if (faction == null)
			{
				FactionGeneratorParms fgParms = new FactionGeneratorParms(
					RatkinFactionDefOf.RK_Faction_Caravan,
					default(IdeoGenerationParms),
					true); // hidden
				faction = FactionGenerator.NewGeneratedFaction(fgParms);
				Find.FactionManager.Add(faction);
			}
			// 공격당하면 반격 (SetRelationDirect로 Hostile 설정, 우호도는 변하지 않음)
			faction.factionHostileOnHarmByPlayer = true;

			// 진입 셀
			if (!parms.spawnCenter.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, map, CellFinder.EdgeRoadChance_Neutral, false, null))
				return false;

			// 포인트 및 TraderKind
			parms.points = TraderCaravanUtility.GenerateGuardPoints();
			parms.faction = faction;
			parms.traderKind = DefDatabase<TraderKindDef>.GetNamed("RK_TraderKind_WanderingTrader", false);
			if (parms.traderKind == null)
				return false;

			// 상인 + 호위병 생성
			PawnGroupMakerParms groupParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Trader, parms, true);
			List<Pawn> caravanPawns = PawnGroupMakerUtility.GeneratePawns(groupParms, false).ToList();
			if (caravanPawns.NullOrEmpty())
				return false;

			// 판매용 pawn 생성 (3~6명, 4종류 중 랜덤)
			int saleCount = SalePawnCountRange.RandomInRange;
			List<Pawn> salePawns = new List<Pawn>();
			for (int i = 0; i < saleCount; i++)
			{
				PawnKindDef kind = SalePawnKinds.RandomElement();
				Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
					kind, faction, PawnGenerationContext.NonPlayer, map.Tile,
					false, false, false, true, kind.isFighter, 1f, true, true, false, true, true,
					false, false, false, false, 0f, 0f, null, 1f, null, null, null, null,
					null, null, null, null, null, null, null, null, false, false, false, false,
					null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null,
					false, false, false, -1, 0, false));
				if (pawn != null)
					salePawns.Add(pawn);
			}

			// 모든 pawn 스폰
			List<Pawn> allPawns = new List<Pawn>(caravanPawns);
			allPawns.AddRange(salePawns);

			foreach (Pawn p in allPawns)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 5, null);
				GenSpawn.Spawn(p, loc, map, WipeMode.Vanish);
				if (p.needs?.food != null)
					p.needs.food.CurLevel = p.needs.food.MaxLevel;
			}

			// chillSpot 및 LordJob
			IntVec3 chillSpot;
			if (!RCellFinder.TryFindRandomSpotJustOutsideColony(allPawns[0].Position, map, allPawns[0], out chillSpot, c =>
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

			LordJob_TradeWithColony lordJob = new LordJob_TradeWithColony(faction, chillSpot);
			LordMaker.MakeNewLord(faction, lordJob, map, allPawns);

			// 편지
			TaggedString label = "LetterLabelTraderCaravanArrival".Translate(faction.Name, parms.traderKind.label).CapitalizeFirst();
			TaggedString text = "LetterTraderCaravanArrival".Translate(faction.NameColored, parms.traderKind.label).CapitalizeFirst();
			text += "\n\n" + "LetterCaravanArrivalCommonWarning".Translate();
			PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(allPawns, ref label, ref text,
				"LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), true, true);
			SendStandardLetter(label, text, LetterDefOf.PositiveEvent, parms, allPawns[0], Array.Empty<NamedArgument>());

			return true;
		}
	}
}
