using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace NewRatkin
{
    public class IncidentWorker_AfterRaid: IncidentWorker_Raid
    {
        public override bool FactionCanBeGroupSource(Faction f, IncidentParms parms, bool desperate = false)
        {
            return base.FactionCanBeGroupSource(f, parms, desperate) && f.HostileTo(Faction.OfPlayer) && (desperate || (float)GenDate.DaysPassed >= f.def.earliestRaidDays);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            // 1.7은 도착 방식 해결 실패도 false로 반환한다. 실패한 습격을 성공으로 기록하지 않는다.
            if (!base.TryExecuteWorker(parms))
            {
                return false;
            }
            if (!parms.silent)
                Find.TickManager.slower.SignalForceNormalSpeedShort();
            Find.StoryWatcher.statsRecord.numRaidsEnemy++;
            parms.target.StoryState.lastRaidFaction = parms.faction;
            return true;
        }

        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (parms.faction != null)
            {
                return true;
            }
            float num = parms.points;
            if (num <= 0f)
            {
                num = 999999f;
            }
            return PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(num, out parms.faction, (Faction f) => this.FactionCanBeGroupSource(f, parms, false), true, true, true, true) || PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(num, out parms.faction, (Faction f) => this.FactionCanBeGroupSource(f, parms, true), true, true, true, true);
        }

        protected override void ResolveRaidPoints(IncidentParms parms)
        {
            if (parms.points <= 0f)
            {
                RatkinLimitedLog.Error(RatkinLogKeys.AfterRaid_ResolveRaidPointsZero, "RaidEnemy is resolving raid points. They should always be set before initiating the incident.");
                parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target);
            }
        }

        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            if (parms.raidStrategy != null)
            {
                return;
            }
            Map map = (Map)parms.target;
            if (!(from d in DefDatabase<RaidStrategyDef>.AllDefs
                  where d.Worker.CanUseWith(parms, groupKind) && (parms.raidArrivalMode != null || (d.arriveModes != null && d.arriveModes.Any((PawnsArrivalModeDef x) => x.Worker.CanUseWith(parms))))
                  select d).TryRandomElementByWeight((RaidStrategyDef d) => d.Worker.SelectionWeightForFaction(map, parms.faction, parms.points), out parms.raidStrategy))
            {
                RatkinLimitedLog.Error(RatkinLogKeys.AfterRaid_NoRaidStrategy, string.Concat(new object[]
                {
                    "No raid stategy for ",
                    parms.faction,
                    " with points ",
                    parms.points,
                    ", groupKind=",
                    groupKind,
                    "\nparms=",
                    parms
                }));
                if (!Prefs.DevMode)
                {
                    parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                }
            }
        }

        protected override string GetLetterLabel(IncidentParms parms)
        {
            return parms.raidStrategy.letterLabelEnemy;
        }

        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            string text = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name);
            text += "\n\n";
            text += parms.raidStrategy.arrivalTextEnemy;
            Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
            if (pawn != null)
            {
                text += "\n\n";
                text += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
            }
            return text;
        }

        protected override LetterDef GetLetterDef()
        {
            return LetterDefOf.ThreatBig;
        }

        protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
        {
            return "LetterRelatedPawnsRaidEnemy".Translate(Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
        }
    }
}
