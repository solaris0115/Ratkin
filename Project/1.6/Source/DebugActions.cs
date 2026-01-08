using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using LudeonTK;
using RimWorld;

namespace NewRatkin
{
    public static class DebugActions
    {
        [DebugAction("Ratkin", "All Apparel Test", 
            allowedGameStates = AllowedGameStates.PlayingOnMap,
            displayPriority = 999)]
        private static void AllApparelTest()
        {
            if (Find.CurrentMap == null)
            {
                Log.Error("No current map found.");
                return;
            }

            // 1. Clear all things on map except terrain
            ClearMapExceptTerrain();

            // 2. Get all RK_ apparel
            var allApparels = DefDatabase<ThingDef>.AllDefs
                .Where(def => def.defName.StartsWith("RK_") && def.IsApparel)
                .OrderBy(def => def.defName)
                .ToList();

            if (allApparels.Count == 0)
            {
                Log.Error("No RK_ apparel found.");
                return;
            }

            // 3. Get Ratkin PawnKindDef and XenotypeDef
            PawnKindDef ratkinKind = DefDatabase<PawnKindDef>.GetNamedSilentFail("RatkinColonist");
            if (ratkinKind == null)
            {
                Log.Error("RatkinColonist PawnKindDef not found.");
                return;
            }

            XenotypeDef ratkinXenotype = DefDatabase<XenotypeDef>.GetNamedSilentFail("RK_XenoType_Ratkin");

            // 4. Spawn pawns with apparel
            Map map = Find.CurrentMap;
            IntVec3 startPos = new IntVec3(30, 0, map.Size.z - 30); // Top-right offset by 30,30
            int gridX = 0;
            int gridZ = 0;
            int spawnedCount = 0;
            
            // Calculate grid size for square layout (each apparel spawns twice, so double the count)
            int gridSize = (int)System.Math.Ceiling(System.Math.Sqrt(allApparels.Count * 2));

            List<Pawn> spawnedPawns = new List<Pawn>();

            foreach (ThingDef apparelDef in allApparels)
            {
                // Spawn twice: once for age 10, once for age 20
                int[] ages = { 10, 20 };
                
                foreach (int age in ages)
                {
                    // Calculate spawn position (1 cell spacing)
                    IntVec3 spawnPos = new IntVec3(
                        startPos.x + gridX,
                        0,
                        startPos.z - gridZ
                    );

                    // Find valid spawn position
                    if (!spawnPos.InBounds(map) || !spawnPos.Standable(map))
                    {
                        spawnPos = CellFinder.RandomSpawnCellForPawnNear(spawnPos, map);
                    }

                    // Generate Ratkin pawn
                    PawnGenerationRequest request = new PawnGenerationRequest(
                        kind: ratkinKind,
                        faction: Faction.OfPlayer,
                        forceGenerateNewPawn: true,
                        allowFood: false,
                        allowAddictions: false,
                        relationWithExtraPawnChanceFactor: 0f,
                        fixedBiologicalAge: age,
                        fixedChronologicalAge: age
                    );

                    // Set Ratkin xenotype if available
                    if (ratkinXenotype != null && ModsConfig.BiotechActive)
                    {
                        request.ForcedXenotype = ratkinXenotype;
                    }

                    Pawn pawn = PawnGenerator.GeneratePawn(request);

                    // Spawn pawn
                    GenSpawn.Spawn(pawn, spawnPos, map);

                    // Make colonist
                    if (pawn.Faction != Faction.OfPlayer)
                    {
                        pawn.SetFaction(Faction.OfPlayer);
                    }

                    // Strip all existing apparel after spawn
                    if (pawn.apparel != null)
                    {
                        List<Apparel> wornApparel = pawn.apparel.WornApparel.ToList();
                        foreach (Apparel app in wornApparel)
                        {
                            pawn.apparel.Remove(app);
                            app.Destroy();
                        }
                    }

                    // Wear only the target apparel
                    Apparel apparel = (Apparel)ThingMaker.MakeThing(apparelDef, GenStuff.DefaultStuffFor(apparelDef));
                    pawn.apparel.Wear(apparel, false);

                    spawnedPawns.Add(pawn);
                    spawnedCount++;

                    // Update grid position (square layout)
                    gridX++;
                    if (gridX >= gridSize)
                    {
                        gridX = 0;
                        gridZ++;
                    }
                }
            }

            // Draft all spawned pawns
            foreach (Pawn pawn in spawnedPawns)
            {
                if (pawn.drafter != null)
                {
                    pawn.drafter.Drafted = true;
                }
            }

            Messages.Message($"Spawned {spawnedCount} Ratkin colonists (age 10 & 20) with {allApparels.Count} different RK_ apparels.", MessageTypeDefOf.TaskCompletion);
        }

        private static void ClearMapExceptTerrain()
        {
            Map map = Find.CurrentMap;
            
            // Remove all pawns
            List<Pawn> pawns = map.mapPawns.AllPawnsSpawned.ToList();
            foreach (Pawn pawn in pawns)
            {
                pawn.Destroy();
            }

            // Remove all things (buildings, items, plants, etc.)
            List<Thing> things = map.listerThings.AllThings.ToList();
            foreach (Thing thing in things)
            {
                if (!thing.Destroyed && thing.def.category != ThingCategory.Mote)
                {
                    thing.Destroy();
                }
            }

            // Remove all plants
            var plants = map.listerThings.ThingsInGroup(ThingRequestGroup.Plant).ToList();
            foreach (Thing plant in plants)
            {
                plant.Destroy();
            }

            Messages.Message("Map cleared (terrain preserved).", MessageTypeDefOf.TaskCompletion);
        }

        [DebugAction("Ratkin", "Fill All Needs", 
            allowedGameStates = AllowedGameStates.PlayingOnMap,
            displayPriority = 998)]
        private static void FillAllNeeds()
        {
            if (Find.CurrentMap == null)
            {
                Log.Error("No current map found.");
                return;
            }

            // 맵의 모든 플레이어 소속 pawn 가져오기 (식민지 주민, 죄수, 노예 포함)
            List<Pawn> allColonyPawns = new List<Pawn>();
            allColonyPawns.AddRange(Find.CurrentMap.mapPawns.FreeColonistsSpawned);
            allColonyPawns.AddRange(Find.CurrentMap.mapPawns.PrisonersOfColonySpawned);
            allColonyPawns.AddRange(Find.CurrentMap.mapPawns.SlavesOfColonySpawned);

            if (allColonyPawns.Count == 0)
            {
                Messages.Message("No pawns found to fill needs.", MessageTypeDefOf.RejectInput);
                return;
            }

            int filledCount = 0;
            foreach (Pawn pawn in allColonyPawns)
            {
                if (pawn.needs == null)
                    continue;

                // 모든 need를 최대치로 채우기
                foreach (Need need in pawn.needs.AllNeeds)
                {
                    if (need != null)
                    {
                        need.CurLevel = need.MaxLevel;
                    }
                }

                filledCount++;
            }

            Messages.Message($"Filled all needs for {filledCount} pawn(s).", MessageTypeDefOf.TaskCompletion);
        }

        [DebugAction("Ratkin", "Remove Body Part", 
            allowedGameStates = AllowedGameStates.PlayingOnMap,
            actionType = DebugActionType.ToolMapForPawns,
            displayPriority = 997)]
        private static void RemoveBodyPart(Pawn p)
        {
            if (p == null)
            {
                Log.Error("RemoveBodyPart: Pawn is null.");
                return;
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(Options_RemovePart(p), null));
        }

        private static List<DebugMenuOption> Options_RemovePart(Pawn p)
        {
            if (p == null)
            {
                throw new System.ArgumentNullException("p");
            }

            List<DebugMenuOption> list = new List<DebugMenuOption>();
            
            // 현재 존재하는 body part 목록 가져오기 (이미 제거된 것은 제외)
            foreach (BodyPartRecord localPart2 in p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null))
            {
                BodyPartRecord localPart = localPart2;
                list.Add(new DebugMenuOption(localPart.LabelCap, DebugMenuOptionMode.Action, delegate()
                {
                    // Hediff_MissingPart 생성 및 추가
                    Hediff_MissingPart hediff_MissingPart = (Hediff_MissingPart)HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, p, null);
                    hediff_MissingPart.Part = localPart;
                    hediff_MissingPart.IsFresh = false;
                    hediff_MissingPart.lastInjury = HediffDefOf.Cut; // 기본 부상 타입 설정
                    
                    p.health.AddHediff(hediff_MissingPart, localPart, null, null);
                    
                    Messages.Message($"Removed {localPart.LabelCap} from {p.LabelShort}.", MessageTypeDefOf.NeutralEvent);
                }));
            }
            
            return list;
        }

        [DebugAction("Ratkin", "Create Baby From Parents (1000x)", 
            allowedGameStates = AllowedGameStates.PlayingOnMap,
            displayPriority = 996,
            requiresBiotech = true)]
        private static void CreateBabyFromParents1000x()
        {
            if (Find.CurrentMap == null)
            {
                Log.Error("No current map found.");
                return;
            }

            DebugTool tool = null;
            Pawn parent1 = null;
            tool = new DebugTool("First parent...", delegate()
            {
                parent1 = PawnAt(UI.MouseCell());
                if (parent1 != null)
                {
                    DebugTools.curTool = new DebugTool("Second parent...", delegate()
                    {
                        Pawn parent2 = PawnAt(UI.MouseCell());
                        if (parent2 != null)
                        {
                            // 결과 카운트 딕셔너리
                            Dictionary<int, int> resultCounts = new Dictionary<int, int>();
                            
                            // 1000회 반복 실행
                            for (int cycle = 1; cycle <= 1000; cycle++)
                            {
                                // 생성 전 아이 수 확인
                                int beforeCount = CountChildrenOnMap();
                                
                                // 아이 생성
                                PregnancyUtility.ApplyBirthOutcome(
                                    RitualOutcomeEffectDefOf.ChildBirth.BestOutcome, 
                                    1f, 
                                    null, 
                                    null, 
                                    parent1, 
                                    parent2, 
                                    parent2, 
                                    null, 
                                    null, 
                                    null, 
                                    false);
                                
                                // 생성 후 아이 수 확인
                                int afterCount = CountChildrenOnMap();
                                int createdCount = afterCount - beforeCount;
                                
                                // 결과 카운트
                                if (!resultCounts.ContainsKey(createdCount))
                                {
                                    resultCounts[createdCount] = 0;
                                }
                                resultCounts[createdCount]++;
                            }
                            
                            // 결과 요약 출력
                            Log.Message("=== Create Baby Test Results (1000 trials) ===");
                            foreach (var kvp in resultCounts.OrderBy(x => x.Key))
                            {
                                double percentage = (kvp.Value / 1000.0) * 100.0;
                                Log.Message($"{kvp.Key}명: {kvp.Value}회 ({percentage:F2}%)");
                            }
                            
                            Messages.Message("Created babies 1000 times. Check logs for probability summary.", MessageTypeDefOf.TaskCompletion);
                        }
                        DebugTools.curTool = tool;
                    }, (Action)null);
                }
            }, (Action)null);
            DebugTools.curTool = tool;
        }

        private static Pawn PawnAt(IntVec3 c)
        {
            foreach (Thing thing in Find.CurrentMap.thingGrid.ThingsAt(c))
            {
                Pawn pawn = thing as Pawn;
                if (pawn != null)
                {
                    return pawn;
                }
            }
            return null;
        }

        private static int CountChildrenOnMap()
        {
            int count = 0;
            foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned)
            {
                if (pawn.ageTracker != null && 
                    (pawn.ageTracker.CurLifeStage.developmentalStage == DevelopmentalStage.Newborn ||
                     pawn.ageTracker.CurLifeStage.developmentalStage == DevelopmentalStage.Baby ||
                     pawn.ageTracker.CurLifeStage.developmentalStage == DevelopmentalStage.Child))
                {
                    count++;
                }
            }
            return count;
        }
    }
}

