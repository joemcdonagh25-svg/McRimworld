using TheArk.Campaign;
using UnityEngine;
using Verse;

namespace TheArk.Debug
{
    /// <summary>
    /// Crude development-only window for inspecting and editing Ark campaign state.
    /// Draft fields are discarded unless Apply is pressed — no second authoritative model.
    /// </summary>
    public class Dialog_ArkCampaignDebug : Window
    {
        private bool draftActive;
        private int draftDay;
        private int draftLanding;
        private int draftTier;
        private int draftPursuit;

        private string bufferDay;
        private string bufferLanding;
        private string bufferTier;
        private string bufferPursuit;

        public override Vector2 InitialSize => new Vector2(420f, 360f);

        public Dialog_ArkCampaignDebug()
        {
            optionalTitle = "[DEV] The Ark — Campaign Debug";
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            draggable = true;
            absorbInputAroundWindow = true;
            onlyDrawInDevMode = true;
            PullFromCampaign();
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (!Prefs.DevMode)
            {
                Widgets.Label(inRect, "Dev Mode required. Enable Dev Mode to use The Ark campaign debug UI.");
                return;
            }

            if (!ArkCampaignDebugOps.TryGet(out ArkCampaignGameComponent campaign))
            {
                Widgets.Label(inRect, "No ArkCampaignGameComponent on Current.Game.");
                return;
            }

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("DEVELOPMENT / DEBUG ONLY — not player UI.");
            listing.Label("Reads ArkCampaignGameComponent. Writes only via Apply / fixture.");
            listing.GapLine();

            listing.Label("Live: " + ArkCampaignDebugOps.FormatState(campaign));
            listing.Gap(6f);

            listing.CheckboxLabeled("CampaignActive", ref draftActive);
            listing.TextFieldNumericLabeled("CampaignDay", ref draftDay, ref bufferDay, 0f, 1_000_000f);
            listing.TextFieldNumericLabeled("LandingNumber", ref draftLanding, ref bufferLanding, 0f, 1_000_000f);
            listing.TextFieldNumericLabeled("ArkTier", ref draftTier, ref bufferTier, 0f, 100f);
            listing.TextFieldNumericLabeled("Pursuit", ref draftPursuit, ref bufferPursuit, 0f, 100f);

            listing.Gap(12f);

            if (listing.ButtonText("Refresh from campaign"))
            {
                PullFromCampaign();
            }

            if (listing.ButtonText("Apply edits to campaign"))
            {
                ApplyDraft(campaign);
            }

            if (listing.ButtonText("Apply M1 persistence fixture"))
            {
                ArkCampaignDebugOps.ApplyPersistenceFixture(campaign);
                PullFromCampaign();
            }

            if (listing.ButtonText("Log campaign state"))
            {
                ArkCampaignDebugOps.LogState("DebugWindow", campaign);
            }

            listing.End();
        }

        private void PullFromCampaign()
        {
            if (!ArkCampaignDebugOps.TryGet(out ArkCampaignGameComponent campaign))
            {
                return;
            }

            draftActive = campaign.CampaignActive;
            draftDay = campaign.CampaignDay;
            draftLanding = campaign.LandingNumber;
            draftTier = campaign.ArkTier;
            draftPursuit = campaign.Pursuit;

            bufferDay = draftDay.ToString();
            bufferLanding = draftLanding.ToString();
            bufferTier = draftTier.ToString();
            bufferPursuit = draftPursuit.ToString();
        }

        private void ApplyDraft(ArkCampaignGameComponent campaign)
        {
            ArkCampaignDebugOps.SetCampaignActive(campaign, draftActive);
            ArkCampaignDebugOps.SetCampaignDay(campaign, draftDay);
            ArkCampaignDebugOps.SetLandingNumber(campaign, draftLanding);
            ArkCampaignDebugOps.SetArkTier(campaign, draftTier);
            ArkCampaignDebugOps.SetPursuit(campaign, draftPursuit);
            ArkCampaignDebugOps.LogState("DebugWindow.Apply", campaign);
            PullFromCampaign();
        }
    }
}
