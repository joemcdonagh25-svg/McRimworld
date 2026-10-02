using System.Collections.Generic;
using System.Linq;
using RimWorld;
using VampireLord.Campaign;
using Verse;

namespace VampireLord.Scenario
{
    /// <summary>
    /// M2 playtest setup: place a simple walled courtyard at player start, reveal the map, auto-start Wave Director.
    /// Used only by the Vampire Lord scenario (not global).
    /// </summary>
    public class ScenPart_VampireLordPlaytestSetup : ScenPart
    {
        private const int CourtyardHalfSize = 7; // exterior ~15x15
        private const int GateHalfWidth = 1; // 3-cell open gate
        private const string LogPrefix = "[VampireLord]";

        // Static fallback: PostMapGenerate must still reveal even if the ScenPart instance was re-created.
        private static CellRect s_lastCourtyard = CellRect.Empty;
        private static IntVec3 s_lastGateCenter = IntVec3.Invalid;
        private static int s_lastMapId = -1;

        private CellRect lastCourtyard = CellRect.Empty;
        private IntVec3 lastGateCenter = IntVec3.Invalid;

        public override void GenerateIntoMap(Map map)
        {
            base.GenerateIntoMap(map);
            TryPlaceCourtyard(map);
        }

        public override void PostMapGenerate(Map map)
        {
            base.PostMapGenerate(map);
            // Fog is applied during map gen after ScenParts.GenerateIntoMap — unfog here so the wilds are visible/pathable.
            TryRevealMap(map, "PostMapGenerate");
        }

        public override void PostWorldGenerate()
        {
            base.PostWorldGenerate();
            TryEnsurePlayerIdeo();
        }

        public override void PostGameStart()
        {
            base.PostGameStart();
            TryEnsurePlayerIdeo();
            TryEnsureStartingPawnsAreColonists();
            TryEquipVampireLordLongsword();
            // Keep must be a real player Settlement — Camp/non-home parents feel like a caravan.
            VampireLordPlayerHome.TryEnsure("PostGameStart");
            // Belt-and-suspenders: if fog was reapplied after PostMapGenerate, clear it once pawns exist.
            Map map = Find.CurrentMap ?? Find.AnyPlayerHomeMap;
            if (map != null)
            {
                TryRevealMap(map, "PostGameStart");
            }

            TryAutoStartCampaign();
        }

        public override string Summary(RimWorld.Scenario scen)
        {
            return "Starts in a simple walled courtyard keep with an open gate; full map revealed; Wave Director auto-starts.";
        }

        public override bool HasNullDefs()
        {
            return false;
        }

        private static void TryEnsurePlayerIdeo()
        {
            if (!ModsConfig.IdeologyActive)
            {
                return;
            }

            Faction player = Faction.OfPlayer;
            if (player?.ideos == null)
            {
                Log.Warning($"{LogPrefix} Ideology active but player faction ideos tracker missing.");
                return;
            }

            // Harden cultures even when an ideo already exists (helps odd mid-game reloads).
            if (player.def != null)
            {
                VampireLordIdeoSettle.EnsureAllowedCultures(player.def, "PlaytestSetup");
            }

            if (player.ideos.PrimaryIdeo != null)
            {
                return;
            }

            try
            {
                var parms = new IdeoGenerationParms(player.def);
                player.ideos.ChooseOrGenerateIdeo(parms);
                if (player.ideos.PrimaryIdeo != null)
                {
                    Log.Message($"{LogPrefix} Generated starting ideoligion '{player.ideos.PrimaryIdeo.name}' for player faction.");
                }
                else
                {
                    Log.Warning($"{LogPrefix} ChooseOrGenerateIdeo returned without a PrimaryIdeo.");
                }
            }
            catch (System.Exception e)
            {
                Log.Warning($"{LogPrefix} Failed to generate starting ideoligion: {e.Message}");
            }
        }

        private static void TryEnsureStartingPawnsAreColonists()
        {
            Map map = Find.CurrentMap ?? Find.AnyPlayerHomeMap;
            if (map == null || Faction.OfPlayer == null)
            {
                return;
            }

            int fixedCount = 0;
            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned.ToList())
            {
                if (pawn?.RaceProps == null || !pawn.RaceProps.Humanlike)
                {
                    continue;
                }

                if (pawn.Faction != Faction.OfPlayer)
                {
                    pawn.SetFaction(Faction.OfPlayer);
                    fixedCount++;
                }
            }

            if (fixedCount > 0)
            {
                Log.Message($"{LogPrefix} Reassigned {fixedCount} humanlike pawn(s) to player faction.");
            }
        }

        /// <summary>
        /// Scenario lists a masterwork longsword as a starting thing; put it in the Vampire Lord's hand
        /// so New Game is fight-ready without scavenging the keep floor.
        /// </summary>
        private static void TryEquipVampireLordLongsword()
        {
            Map map = Find.CurrentMap ?? Find.AnyPlayerHomeMap;
            if (map?.mapPawns == null)
            {
                return;
            }

            ThingDef swordDef = DefDatabase<ThingDef>.GetNamedSilentFail("MeleeWeapon_LongSword");
            if (swordDef == null)
            {
                Log.Warning($"{LogPrefix} Longsword equip skipped: MeleeWeapon_LongSword missing.");
                return;
            }

            Pawn lord = PickVampireLord(map);
            if (lord?.equipment == null)
            {
                Log.Warning($"{LogPrefix} Longsword equip skipped: no Vampire Lord pawn.");
                return;
            }

            if (lord.equipment.Primary != null && lord.equipment.Primary.def == swordDef)
            {
                EnsureMasterworkQuality(lord.equipment.Primary);
                return;
            }

            ThingWithComps sword = FindLooseLongsword(map, swordDef) ?? MakeMasterworkLongsword(swordDef);
            if (sword == null)
            {
                return;
            }

            if (sword.Spawned)
            {
                sword.DeSpawn();
            }

            if (lord.equipment.Primary != null)
            {
                lord.equipment.TryDropEquipment(lord.equipment.Primary, out _, lord.Position, false);
            }

            lord.equipment.AddEquipment(sword);
            Log.Message($"{LogPrefix} Equipped masterwork longsword on {lord.LabelShort}.");
        }

        private static Pawn PickVampireLord(Map map)
        {
            Pawn sanguophage = null;
            Pawn fallback = null;
            foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
            {
                if (pawn == null)
                {
                    continue;
                }

                fallback ??= pawn;
                if (pawn.genes?.Xenotype != null && pawn.genes.Xenotype == XenotypeDefOf.Sanguophage)
                {
                    sanguophage = pawn;
                    break;
                }
            }

            return sanguophage ?? fallback;
        }

        private static ThingWithComps FindLooseLongsword(Map map, ThingDef swordDef)
        {
            List<Thing> things = map.listerThings?.ThingsOfDef(swordDef);
            if (things == null)
            {
                return null;
            }

            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is ThingWithComps twc && twc.Spawned && !twc.IsForbidden(Faction.OfPlayer))
                {
                    EnsureMasterworkQuality(twc);
                    return twc;
                }
            }

            return null;
        }

        private static ThingWithComps MakeMasterworkLongsword(ThingDef swordDef)
        {
            ThingDef stuff = swordDef.MadeFromStuff ? ThingDefOf.Steel : null;
            Thing made = ThingMaker.MakeThing(swordDef, stuff);
            if (made is not ThingWithComps sword)
            {
                made?.Destroy(DestroyMode.Vanish);
                return null;
            }

            EnsureMasterworkQuality(sword);
            return sword;
        }

        private static void EnsureMasterworkQuality(Thing thing)
        {
            CompQuality quality = thing?.TryGetComp<CompQuality>();
            if (quality == null)
            {
                return;
            }

            if (quality.Quality < QualityCategory.Masterwork)
            {
                quality.SetQuality(QualityCategory.Masterwork, ArtGenerationContext.Outsider);
            }
        }

        private static void TryAutoStartCampaign()
        {
            if (!VampireLordCampaign.TryGet(out VampireLordCampaignGameComponent campaign))
            {
                Log.Warning($"{LogPrefix} Playtest setup: no campaign component — cannot auto-start.");
                return;
            }

            if (campaign.CampaignActive)
            {
                Log.Message($"{LogPrefix} Playtest setup: campaign already active.");
                return;
            }

            VampireLordWaveDirector.ActivateCampaign(campaign);
            Log.Message($"{LogPrefix} Playtest setup: campaign auto-started for Vampire Lord scenario.");
        }

        private void TryPlaceCourtyard(Map map)
        {
            if (map == null)
            {
                return;
            }

            IntVec3 center = MapGenerator.PlayerStartSpotValid
                ? MapGenerator.PlayerStartSpot
                : map.Center;

            if (!center.InBounds(map))
            {
                center = map.Center;
            }

            CellRect exterior = CellRect.CenteredOn(center, CourtyardHalfSize).ClipInsideMap(map);
            CellRect interior = exterior.ContractedBy(1);
            lastCourtyard = exterior;
            s_lastCourtyard = exterior;
            s_lastMapId = map.uniqueID;

            ThingDef wallDef = ThingDefOf.Wall;
            ThingDef stuff = ThingDefOf.BlocksGranite;
            TerrainDef floor = TerrainDefOf.FlagstoneSandstone;
            TerrainDef pathTerrain = TerrainDefOf.PackedDirt;
            Faction player = Faction.OfPlayer;

            foreach (IntVec3 cell in exterior.Cells)
            {
                ClearCellForKeep(map, cell, clearItems: false);
            }

            foreach (IntVec3 cell in interior.Cells)
            {
                if (cell.InBounds(map))
                {
                    map.terrainGrid.SetTerrain(cell, floor);
                }
            }

            IntVec3 gateCenter = exterior.GetCenterCellOnEdge(Rot4.South);
            if (!gateCenter.InBounds(map))
            {
                gateCenter = new IntVec3(center.x, 0, exterior.minZ);
            }

            lastGateCenter = gateCenter;
            s_lastGateCenter = gateCenter;
            VampireLordKeepLayout.Remember(map, exterior, gateCenter);

            foreach (IntVec3 cell in exterior.EdgeCells)
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                // Open south gate (no door) so pathing to the wilds is never blocked by a closed door.
                if (IsGateCell(cell, gateCenter))
                {
                    ClearCellForKeep(map, cell, clearItems: true);
                    map.terrainGrid.SetTerrain(cell, pathTerrain);
                    continue;
                }

                ClearCellForKeep(map, cell, clearItems: true);
                SpawnPlayerBuilding(ThingMaker.MakeThing(wallDef, stuff), cell, map, player);
            }

            // Clear + pave a short approach path south of the gate so the exit is walkable and obvious.
            ClearApproachLane(map, gateCenter, pathTerrain);

            MapGenerator.PlayerStartSpot = center;

            Log.Message(
                $"{LogPrefix} Playtest keep courtyard placed at {center} " +
                $"(size {exterior.Width}x{exterior.Height}, open gate {gateCenter}, faction={player?.Name}).");
        }

        private void TryRevealMap(Map map, string phase)
        {
            if (map?.fogGrid == null)
            {
                Log.Warning($"{LogPrefix} Reveal skipped ({phase}): map or fogGrid null.");
                return;
            }

            RestoreCourtyardMemory(map);

            // Playtest: unfog the whole map so "outside the keep" is never an empty black void.
            int unfogged = 0;
            foreach (IntVec3 cell in map.AllCells)
            {
                if (map.fogGrid.IsFogged(cell))
                {
                    map.fogGrid.Unfog(cell);
                    unfogged++;
                }
            }

            IntVec3 gate = lastGateCenter.IsValid ? lastGateCenter : s_lastGateCenter;
            if (gate.IsValid && gate.InBounds(map))
            {
                map.fogGrid.FloodUnfogAdjacent(gate, false);
            }

            IntVec3 start = MapGenerator.PlayerStartSpotValid ? MapGenerator.PlayerStartSpot : map.Center;
            if (start.InBounds(map))
            {
                map.fogGrid.FloodUnfogAdjacent(start, false);
            }

            Log.Message($"{LogPrefix} Revealed map ({phase}): unfogged {unfogged}/{map.Area} cells; gate={gate}.");
        }

        private void RestoreCourtyardMemory(Map map)
        {
            if (!lastCourtyard.IsEmpty)
            {
                s_lastCourtyard = lastCourtyard;
                s_lastGateCenter = lastGateCenter;
                s_lastMapId = map.uniqueID;
                return;
            }

            if (s_lastMapId == map.uniqueID && !s_lastCourtyard.IsEmpty)
            {
                lastCourtyard = s_lastCourtyard;
                lastGateCenter = s_lastGateCenter;
                VampireLordKeepLayout.Remember(map, lastCourtyard, lastGateCenter);
            }
        }

        private static bool IsGateCell(IntVec3 cell, IntVec3 gateCenter)
        {
            return cell.z == gateCenter.z
                && cell.x >= gateCenter.x - GateHalfWidth
                && cell.x <= gateCenter.x + GateHalfWidth;
        }

        private static void ClearApproachLane(Map map, IntVec3 gateCenter, TerrainDef pathTerrain)
        {
            for (int dz = 1; dz <= 8; dz++)
            {
                for (int dx = -GateHalfWidth; dx <= GateHalfWidth; dx++)
                {
                    IntVec3 cell = new IntVec3(gateCenter.x + dx, 0, gateCenter.z - dz);
                    if (!cell.InBounds(map))
                    {
                        continue;
                    }

                    ClearCellForKeep(map, cell, clearItems: false);
                    // Don't pave water / impassable under-terrain; only replace if currently walkable or rock-like.
                    if (cell.GetTerrain(map) != null && cell.Walkable(map))
                    {
                        map.terrainGrid.SetTerrain(cell, pathTerrain);
                    }
                    else if (cell.GetEdifice(map) == null)
                    {
                        // If plants/rocks were cleared but cell was non-walkable rubble, force a path.
                        map.terrainGrid.SetTerrain(cell, pathTerrain);
                    }
                }
            }
        }

        private static void ClearCellForKeep(Map map, IntVec3 cell, bool clearItems)
        {
            List<Thing> things = cell.GetThingList(map).ToList();
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (thing?.def == null)
                {
                    continue;
                }

                bool remove =
                    thing.def.category == ThingCategory.Plant
                    || thing.def.category == ThingCategory.Building
                    || thing.def.IsFilth
                    || (clearItems && thing.def.category == ThingCategory.Item);

                if (remove)
                {
                    thing.Destroy(DestroyMode.Vanish);
                }
            }
        }

        private static void SpawnPlayerBuilding(Thing thing, IntVec3 cell, Map map, Faction player)
        {
            if (thing == null || !cell.InBounds(map))
            {
                return;
            }

            if (player != null)
            {
                thing.SetFactionDirect(player);
            }

            GenSpawn.Spawn(thing, cell, map, WipeMode.Vanish);

            if (player != null && thing.Faction != player)
            {
                thing.SetFaction(player);
            }
        }
    }
}
