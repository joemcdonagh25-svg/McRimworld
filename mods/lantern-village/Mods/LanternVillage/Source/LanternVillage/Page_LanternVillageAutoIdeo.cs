using RimWorld;
using UnityEngine;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Drop-in replacement for Page_ChooseIdeoPreset. Applies The Hearthbound in PostOpen
    /// and advances immediately so the CultureDef NRE UI never opens.
    /// </summary>
    public class Page_LanternVillageAutoIdeo : Page
    {
        public override string PageTitle => "The Lantern Village";

        public override void PostOpen()
        {
            base.PostOpen();

            LanternVillageIdeoSettle.TryApplyAndNotify("AutoIdeoPage");

            if (next != null)
            {
                next.prev = prev;
            }

            DoNext();
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Intentionally empty — PostOpen always advances.
        }
    }
}
