using RimWorld;
using UnityEngine;
using Verse;

namespace VampireLord.Scenario
{
    /// <summary>
    /// Drop-in replacement for <c>Page_ChooseIdeoPreset</c> on the Vampire Lord scenario.
    /// Applies a classic ideoligion in <see cref="PostOpen"/> and advances immediately
    /// (same pattern as vanilla tutorialMode auto-next), avoiding the CultureDef NRE UI.
    /// </summary>
    public class Page_VampireLordAutoIdeo : Page
    {
        public override string PageTitle => "Vampire Lord";

        public override void PostOpen()
        {
            base.PostOpen();

            VampireLordIdeoSettle.TryApplyClassicAndNotify("AutoIdeoPage");

            // Back from the next page should return to the starting-site picker, not flash this page.
            if (next != null)
            {
                next.prev = prev;
            }

            DoNext();
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Intentionally empty — PostOpen always advances before the player interacts.
        }
    }
}
