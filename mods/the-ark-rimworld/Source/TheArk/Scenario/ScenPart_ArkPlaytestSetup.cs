using System.Linq;
using RimWorld;
using TheArk.Campaign;
using TheArk.Debug;
using Verse;

namespace TheArk.Scenario
{
    /// <summary>
    /// Playtest-only: harden new-game start (Ideology/colonists) and activate Ark campaign.
    /// Does not place a gravship yet — Odyssey wreckage/start is a later milestone.
    /// </summary>
    public class ScenPart_ArkPlaytestSetup : ScenPart
    {
        private const string LogPrefix = "[The Ark]";

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
            TryActivateCampaign();
        }

        public override string Summary(RimWorld.Scenario scen)
        {
            return "Activates Ark campaign state for playtest; ensures player ideo/colonists when Ideology is on.";
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
                    Log.Message(
                        $"{LogPrefix} Generated starting ideoligion '{player.ideos.PrimaryIdeo.name}' for player faction.");
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

        private static void TryActivateCampaign()
        {
            if (!ArkCampaignDebugOps.TryGet(out ArkCampaignGameComponent campaign))
            {
                Log.Warning($"{LogPrefix} Playtest setup: no ArkCampaignGameComponent — cannot activate campaign.");
                return;
            }

            if (!campaign.CampaignActive)
            {
                ArkCampaignDebugOps.SetCampaignActive(campaign, true);
            }

            ArkCampaignDebugOps.LogState("Scenario.PostGameStart", campaign);
            Log.Message(
                $"{LogPrefix} Playtest scenario ready. Dev Mode → The Ark (DEV) → Apply M1 Persistence Fixture " +
                "for the save/quit/load proof.");
        }
    }
}
