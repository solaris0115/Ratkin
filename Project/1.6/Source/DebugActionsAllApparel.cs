using System.Collections.Generic;
using System.Linq;
using Verse;
using LudeonTK;
using RimWorld;

namespace DebugTools
{
    public static class DebugActionsAllApparel
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
                // Spawn twice: once for age 15, once for age 20
                int[] ages = { 15, 20 };
                
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
                pawn.drafter.Drafted = true;
            }

            Messages.Message($"Spawned {spawnedCount} Ratkin colonists (age 15 & 20) with {allApparels.Count} different RK_ apparels.", MessageTypeDefOf.TaskCompletion);
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
    }
}

