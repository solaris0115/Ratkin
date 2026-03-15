using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;

namespace NewRatkin
{
	/// <summary>
	/// 유랑 상인의 정착 희망 유랑민 목록을 보여주고 구매(colonist 합류)할 수 있는 창.
	/// </summary>
	public class Dialog_WanderingTraderSettlers : Window
	{
		private readonly Pawn trader;
		private List<Pawn> salePawns;
		private Vector2 scrollPosition;
		private float scrollViewHeight;

		private static WanderingTraderSettings GetSettings()
		{
			var def = DefDatabase<TraderKindDef>.GetNamed("RK_TraderKind_WanderingTrader", false);
			return def?.GetModExtension<WanderingTraderSettings>() ?? new WanderingTraderSettings();
		}

		public Dialog_WanderingTraderSettlers(Pawn trader, List<Pawn> salePawns)
		{
			this.trader = trader;
			this.salePawns = new List<Pawn>(salePawns);
			optionalTitle = "RK_WanderingTrader_ViewSettlers".Translate();
			doCloseButton = true;
			doCloseX = true;
			absorbInputAroundWindow = true;
			forcePause = false;
		}

		public override Vector2 InitialSize => new Vector2(800f, 900f);

		public override void DoWindowContents(Rect inRect)
		{
			WanderingTraderSettings settings = GetSettings();

			Rect scrollRect = new Rect(0f, 0f, inRect.width - 20f, inRect.height - 40f);
			Rect viewRect = new Rect(0f, 0f, scrollRect.width - 20f, scrollViewHeight);

			Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect, true);

			float y = 0f;
			List<Pawn> toRemove = new List<Pawn>();

			float leftPadding = 5f;
			float portraitSize = 140f;
			float rowHeight = 150f;

			foreach (Pawn pawn in salePawns)
			{
				if (pawn.DestroyedOrNull() || pawn.Dead)
				{
					toRemove.Add(pawn);
					continue;
				}

				Rect rowRect = new Rect(0f, y, viewRect.width, rowHeight);
				Widgets.DrawHighlightIfMouseover(rowRect);

				// 전신 포트레이트 (왼쪽, 남쪽 방향)
				Rect portraitRect = new Rect(leftPadding, rowRect.y, portraitSize, portraitSize);
				RenderTexture portrait = PortraitsCache.Get(pawn, new Vector2(portraitSize, portraitSize), Rot4.South, default(Vector3), 1f, true, true, true, true, null, null, false, null);
				GUI.DrawTexture(portraitRect, portrait);

				// Pawn 정보 (포트레이트 오른쪽)
				Rect labelRect = new Rect(leftPadding + portraitSize + 10f, rowRect.y, rowRect.width - leftPadding - portraitSize - 240f, 36f);
				Widgets.Label(labelRect, pawn.LabelShortCap + " - " + (pawn.kindDef?.label ?? "?"));

				int price = settings.GetPriceForPawn(pawn);
				string priceStr = price + " " + ThingDefOf.Silver.label;

				// 구매 버튼
				Rect buyRect = new Rect(rowRect.width - 220f, rowRect.y + 5f, 200f, 40f);
				bool canAfford = GetSilverCount(Find.CurrentMap) >= price;
				if (Widgets.ButtonText(buyRect, "RK_WanderingTrader_Buy".Translate(priceStr)))
				{
					if (canAfford)
					{
						BuyPawn(pawn, price);
						toRemove.Add(pawn);
						SoundDefOf.ExecuteTrade.PlayOneShotOnCamera(null);
					}
					else
					{
						Messages.Message("RK_WanderingTrader_NotEnoughSilver".Translate(), MessageTypeDefOf.RejectInput);
					}
				}
				if (!canAfford)
				{
					GUI.color = Color.gray;
					Widgets.DrawHighlightIfMouseover(buyRect);
					GUI.color = Color.white;
				}

				// Info 버튼
				Rect infoRect = new Rect(rowRect.width - 220f, rowRect.y + 50f, 200f, 35f);
				if (Widgets.ButtonText(infoRect, "RK_WanderingTrader_Info".Translate()))
				{
					Find.WindowStack.Add(new Dialog_InfoCard(pawn));
				}

				y += rowHeight + 5f;
			}

			foreach (Pawn p in toRemove)
				salePawns.Remove(p);

			scrollViewHeight = y;
			Widgets.EndScrollView();
		}

		private void BuyPawn(Pawn pawn, int price)
		{
			Map map = Find.CurrentMap;
			if (map == null)
				return;

			// 은화 차감 (맵 내 은화에서 제거)
			RemoveSilverFromMap(map, price);

			// Lord에서 제거 후 플레이어 faction으로 합류 (colonist, PawnKind 유지)
			Lord lord = pawn.GetLord();
			lord?.Notify_PawnLost(pawn, PawnLostCondition.LeftVoluntarily);
			pawn.SetFaction(Faction.OfPlayer, null);

			Messages.Message("RK_WanderingTrader_Recruited".Translate(pawn.LabelShortCap), pawn, MessageTypeDefOf.PositiveEvent);
		}

		private static int GetSilverCount(Map map)
		{
			if (map?.resourceCounter == null)
				return 0;
			return map.resourceCounter.GetCount(ThingDefOf.Silver);
		}

		private static void RemoveSilverFromMap(Map map, int amount)
		{
			int remaining = amount;
			foreach (Thing t in map.listerThings.ThingsOfDef(ThingDefOf.Silver))
			{
				if (remaining <= 0)
					break;
				int toTake = Mathf.Min(remaining, t.stackCount);
				if (toTake >= t.stackCount)
					t.Destroy(DestroyMode.Vanish);
				else
					t.SplitOff(toTake).Destroy(DestroyMode.Vanish);
				remaining -= toTake;
			}
		}
	}
}
