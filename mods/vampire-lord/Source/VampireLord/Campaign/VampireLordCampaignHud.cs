using RimWorld;
using UnityEngine;
using Verse;

namespace VampireLord.Campaign
{
    /// <summary>
    /// Always-on Blood / Wave / Threat readout while the campaign is active.
    /// Drawn from <see cref="VampireLordCampaignGameComponent.GameComponentOnGUI"/> — no Harmony.
    /// </summary>
    public static class VampireLordCampaignHud
    {
        private const float PadX = 12f;
        private const float Width = 460f;
        private const float Height = 52f;
        private const float BottomPad = 58f;

        public static void Draw(VampireLordCampaignGameComponent campaign)
        {
            if (campaign == null || !campaign.CampaignActive || !campaign.ShowCampaignHud)
            {
                return;
            }

            // Avoid painting over world/setup menus.
            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            Text.Font = GameFont.Tiny;
            string line1 =
                $"Black Keep   Blood {campaign.BloodReserve}" +
                $" (tithe {VampireLordBloodTithe.WaveBloodCost(campaign)})" +
                $"   Wave {campaign.UpcomingWaveNumber} ({campaign.PendingWaveType})" +
                $"   Threat {campaign.ThreatLevel}";

            string line2 = StatusLine(campaign);

            float y = UI.screenHeight - BottomPad - Height;
            Rect box = new Rect(PadX, y, Width, Height);

            // Light scrim so text stays readable on bright/dark biomes.
            Widgets.DrawBoxSolid(box, new Color(0f, 0f, 0f, 0.45f));
            GUI.color = Color.white;
            Widgets.Label(new Rect(box.x + 6f, box.y + 4f, box.width - 12f, 22f), line1);
            Widgets.Label(new Rect(box.x + 6f, box.y + 26f, box.width - 12f, 22f), line2);

            Text.Font = GameFont.Small;
        }

        private static string StatusLine(VampireLordCampaignGameComponent campaign)
        {
            if (!campaign.WavePending)
            {
                return "Status: idle";
            }

            if (!campaign.WarningIssued)
            {
                string eta = FormatEta(campaign.TicksUntilWarning);
                string fortify = VampireLordFortify.IsPrepWindow(campaign)
                    ? (campaign.AutoFortifyPlaytest ? "Auto-fortify ready" : "Fortify available")
                    : "Prep";
                return $"Status: prep — warning in {eta}   {fortify}";
            }

            return $"Status: host inbound — arrives in {FormatEta(campaign.TicksUntilWave)}";
        }

        private static string FormatEta(int ticks)
        {
            if (ticks < 0)
            {
                return "?";
            }

            if (ticks < 60)
            {
                return "now";
            }

            float days = ticks / (float)GenDate.TicksPerDay;
            if (days >= 1f)
            {
                return $"{days:0.0}d";
            }

            float hours = days * 24f;
            return $"{hours:0.0}h";
        }
    }
}
