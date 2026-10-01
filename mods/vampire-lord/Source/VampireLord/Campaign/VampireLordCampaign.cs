using Verse;

namespace VampireLord.Campaign
{
    /// <summary>
    /// Thin retrieval helper for the authoritative <see cref="VampireLordCampaignGameComponent"/>.
    /// Does not cache campaign state — always reads from <see cref="Current.Game"/>.
    /// </summary>
    public static class VampireLordCampaign
    {
        public static bool TryGet(out VampireLordCampaignGameComponent campaign)
        {
            Game game = Current.Game;
            if (game == null)
            {
                campaign = null;
                return false;
            }

            campaign = game.GetComponent<VampireLordCampaignGameComponent>();
            return campaign != null;
        }

        public static VampireLordCampaignGameComponent Get()
        {
            return TryGet(out VampireLordCampaignGameComponent campaign) ? campaign : null;
        }
    }
}
