using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VampireLord.Campaign
{
    /// <summary>
    /// M3 Blood Tithe V0: credit Keep Blood Reserve from fresh hostile corpses;
    /// spend a wave cost when raids launch (starvation raises raid points).
    /// No Harmony — corpse scan on the campaign tick.
    /// </summary>
    public static class VampireLordBloodTithe
    {
        private static readonly HashSet<int> CreditedCorpseIds = new HashSet<int>();

        public static int WaveBloodCost(VampireLordCampaignGameComponent campaign)
        {
            return VampireLordTuning.WaveBloodCostBase +
                   (campaign.ThreatLevel * VampireLordTuning.WaveBloodCostPerThreat);
        }

        public static void ResetSessionCredits()
        {
            CreditedCorpseIds.Clear();
        }

        public static void EnsureStartingReserve(VampireLordCampaignGameComponent campaign)
        {
            if (campaign.BloodReserve < VampireLordTuning.StartingBloodReserve)
            {
                campaign.BloodReserve = VampireLordTuning.StartingBloodReserve;
            }

            campaign.BloodGainedSinceWave = 0;
            campaign.LastWaveBloodStarved = false;
        }

        public static void AddBlood(VampireLordCampaignGameComponent campaign, int amount, string reason)
        {
            if (amount <= 0)
            {
                return;
            }

            campaign.BloodReserve += amount;
            campaign.BloodGainedSinceWave += amount;
            Log.Message($"[VampireLord] Blood Tithe +{amount} ({reason}). Reserve={campaign.BloodReserve}.");
        }

        public static int SpendBlood(VampireLordCampaignGameComponent campaign, int amount, string reason)
        {
            if (amount <= 0)
            {
                return 0;
            }

            int spent = amount;
            if (spent > campaign.BloodReserve)
            {
                spent = campaign.BloodReserve;
            }

            campaign.BloodReserve -= spent;
            Log.Message($"[VampireLord] Blood Tithe -{spent}/{amount} ({reason}). Reserve={campaign.BloodReserve}.");
            return spent;
        }

        /// <summary>
        /// Pay the wave tithe. Returns raid-points multiplier (1f if paid in full).
        /// </summary>
        public static float ApplyWaveCost(VampireLordCampaignGameComponent campaign)
        {
            int cost = WaveBloodCost(campaign);
            int spent = SpendBlood(campaign, cost, $"Wave {campaign.UpcomingWaveNumber} tithe");
            bool starved = spent < cost;
            campaign.LastWaveBloodStarved = starved;
            campaign.LastWaveBloodSpent = spent;
            campaign.LastWaveBloodCost = cost;

            VampireLordLetters.SendBloodTitheLetter(campaign, spent, cost, starved);

            if (starved)
            {
                Log.Message(
                    $"[VampireLord] Blood starved for Wave {campaign.UpcomingWaveNumber} " +
                    $"(paid {spent}/{cost}). Raid points x{VampireLordTuning.StarvedRaidPointsMultiplier:0.00}.");
                return VampireLordTuning.StarvedRaidPointsMultiplier;
            }

            return 1f;
        }

        public static void NotifyWaveHarvest(VampireLordCampaignGameComponent campaign)
        {
            int gained = campaign.BloodGainedSinceWave;
            if (gained > 0 && !campaign.PlaytestQuietLetters)
            {
                VampireLordLetters.SendBloodHarvestLetter(campaign, gained);
            }
            else if (gained > 0)
            {
                Log.Message(
                    $"[VampireLord] Harvest quiet: +{gained} blood since last wave " +
                    $"(Reserve={campaign.BloodReserve}).");
            }

            campaign.BloodGainedSinceWave = 0;
        }

        public static void ScanForFreshKills(VampireLordCampaignGameComponent campaign)
        {
            if (!campaign.CampaignActive || Current.Game == null)
            {
                return;
            }

            List<Map> maps = Find.Maps;
            if (maps == null)
            {
                return;
            }

            for (int m = 0; m < maps.Count; m++)
            {
                Map map = maps[m];
                if (map == null)
                {
                    continue;
                }

                List<Thing> corpses = map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
                if (corpses == null)
                {
                    continue;
                }

                for (int i = 0; i < corpses.Count; i++)
                {
                    if (corpses[i] is Corpse corpse)
                    {
                        TryCreditCorpse(campaign, corpse);
                    }
                }
            }
        }

        private static void TryCreditCorpse(VampireLordCampaignGameComponent campaign, Corpse corpse)
        {
            if (corpse == null || CreditedCorpseIds.Contains(corpse.thingIDNumber))
            {
                return;
            }

            if (corpse.Age > VampireLordTuning.FreshCorpseMaxAgeTicks)
            {
                CreditedCorpseIds.Add(corpse.thingIDNumber);
                return;
            }

            Pawn inner = corpse.InnerPawn;
            if (inner?.RaceProps == null || !inner.RaceProps.Humanlike)
            {
                CreditedCorpseIds.Add(corpse.thingIDNumber);
                return;
            }

            if (!IsTitheEligible(inner))
            {
                CreditedCorpseIds.Add(corpse.thingIDNumber);
                return;
            }

            CreditedCorpseIds.Add(corpse.thingIDNumber);
            AddBlood(campaign, VampireLordTuning.BloodPerHumanlikeKill, $"kill:{inner.LabelShort}");
        }

        private static bool IsTitheEligible(Pawn pawn)
        {
            if (pawn.Faction == Faction.OfPlayer || pawn.IsColonist)
            {
                return false;
            }

            Faction player = Faction.OfPlayer;
            if (player == null)
            {
                return false;
            }

            if (pawn.Faction != null && pawn.Faction.HostileTo(player))
            {
                return true;
            }

            // Factionless hostiles / temporary raid pawns often count as enemies via HostileTo null checks —
            // also accept any non-player humanlike that died while not of the player faction.
            return pawn.Faction == null || pawn.HostileTo(player);
        }

        public static void ForceLowBlood(VampireLordCampaignGameComponent campaign)
        {
            campaign.BloodReserve = 0;
            Log.Message("[VampireLord] Blood Tithe forced to 0 (debug).");
        }
    }
}
