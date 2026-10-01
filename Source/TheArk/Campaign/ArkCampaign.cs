using Verse;

namespace TheArk.Campaign
{
    /// <summary>
    /// Single access path to the authoritative <see cref="ArkCampaignGameComponent"/>.
    /// Does not cache state — always reads from <see cref="Current.Game"/>.
    /// </summary>
    public static class ArkCampaign
    {
        public static bool TryGet(out ArkCampaignGameComponent campaign)
        {
            Game game = Current.Game;
            if (game == null)
            {
                campaign = null;
                return false;
            }

            campaign = game.GetComponent<ArkCampaignGameComponent>();
            return campaign != null;
        }

        public static ArkCampaignGameComponent Get()
        {
            if (!TryGet(out ArkCampaignGameComponent campaign))
            {
                return null;
            }

            return campaign;
        }
    }
}
