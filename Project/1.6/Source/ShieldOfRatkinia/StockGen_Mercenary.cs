using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld.Planet;


namespace NewRatkin
{
    public class StockGenerator_Mercenary : StockGenerator
    {
        private bool respectPopulationIntent = false;

        public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
        {
            if (this.respectPopulationIntent && Rand.Value > StorytellerUtilityPopulation.PopulationIntent)
            {
                yield break;
            }
            int count = this.countRange.RandomInRange;
            for (int i = 0; i < count; i++)
            {
                Faction mercenaryFaction;
                if (!(from fac in Find.FactionManager.AllFactionsVisible
                      where fac != Faction.OfPlayer && fac.def.humanlikeFaction
                      select fac).TryRandomElement(out mercenaryFaction))
                {
                    yield break;
                }
                var mercenaryPawnKindDef = RatkinPawnKindDefOf.RatkinMercenary;

                // 빈 구조체는 성장 단계·나이 특성 등 생성 기본값까지 0으로 만든다.
                PawnGenerationRequest request = new PawnGenerationRequest(
                    mercenaryPawnKindDef, mercenaryFaction, tile: forTile,
                    forceAddFreeWarmLayerIfNeeded: !this.trader.orbital);

                yield return PawnGenerator.GeneratePawn(request);
            }
            yield break;
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return thingDef.category == ThingCategory.Pawn && thingDef.race.Humanlike && thingDef.tradeability != Tradeability.None;
        }
    }
}
