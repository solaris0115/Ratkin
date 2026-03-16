using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace NewRatkin
{
	/// <summary>
	/// 유랑 상인(RK_Faction_Caravan + RK_TraderKind_WanderingTrader) 우클릭 시
	/// "정착 희망 유랑민 보기", "용병 물색" 옵션 추가.
	/// </summary>
	public class FloatMenuOptionProvider_WanderingTrader : FloatMenuOptionProvider
	{
		private static readonly HashSet<PawnKindDef> SalePawnKinds = new HashSet<PawnKindDef>
		{
			RatkinPawnKindDefOf.RK_PawnKind_Nomad,
			RatkinPawnKindDefOf.RK_PawnKind_WanderingMercenary
		};

		protected override bool Drafted => true;
		protected override bool Undrafted => true;
		protected override bool Multiselect => false;

		private static bool IsWanderingTrader(Pawn pawn)
		{
			if (pawn?.Faction == null || pawn.TraderKind == null)
				return false;
			return pawn.Faction.def == RatkinFactionDefOf.RK_Faction_Caravan
				&& pawn.TraderKind.defName == "RK_TraderKind_WanderingTrader";
		}

		private static List<Pawn> GetSalePawnsForTrader(Pawn trader)
		{
			List<Pawn> result = new List<Pawn>();
			Lord lord = trader.GetLord();
			if (lord == null)
				return result;
			foreach (Pawn p in lord.ownedPawns)
			{
				if (p != trader && p.Spawned && !p.Dead && SalePawnKinds.Contains(p.kindDef))
					result.Add(p);
			}
			return result;
		}

		public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
		{
			if (!IsWanderingTrader(clickedPawn))
				yield break;

			Pawn colonist = context.FirstSelectedPawn;
			if (colonist == null || !colonist.IsColonist)
				yield break;

			// 정착 희망 유랑민 보기
			List<Pawn> salePawns = GetSalePawnsForTrader(clickedPawn);
			if (salePawns.Count > 0)
			{
				if (!colonist.CanReach(clickedPawn, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
				{
					yield return new FloatMenuOption("RK_WanderingTrader_ViewSettlers".Translate() + ": " + "NoPath".Translate().CapitalizeFirst(),
						null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
				}
				else
				{
					Action action = () =>
					{
						Find.WindowStack.Add(new Dialog_WanderingTraderSettlers(clickedPawn, salePawns));
					};
					yield return FloatMenuUtility.DecoratePrioritizedTask(
						new FloatMenuOption("RK_WanderingTrader_ViewSettlers".Translate(), action,
							MenuOptionPriority.InitiateSocial, null, clickedPawn, 0f, null, null, true, 0),
						colonist, clickedPawn, "ReservedBy", null);
				}
			}

			// 용병 물색 (플레이스홀더)
			Action mercAction = () =>
			{
				Log.Message("조건에 맞는 대상을 찾기 시작했습니다.");
			};
			yield return new FloatMenuOption("RK_WanderingTrader_MercenarySearch".Translate(), mercAction,
				MenuOptionPriority.Default, null, clickedPawn, 0f, null, null, true, 0);
		}
	}
}
