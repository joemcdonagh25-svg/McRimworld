using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VampireLord.Scenario
{
    /// <summary>
    /// Scenario subclass so we can replace Ideology's <c>Page_ChooseIdeoPreset</c>
    /// without Harmony. Uses <see cref="Page_VampireLordAutoIdeo"/> instead.
    /// </summary>
    public class VampireLordScenario : RimWorld.Scenario
    {
        public override Page GetFirstConfigPage()
        {
            List<Page> list = new List<Page>();
            list.Add(new Page_SelectStoryteller());
            list.Add(new Page_CreateWorldParams());

            // parts is internal to Assembly-CSharp — AllParts is the public surface.
            // playerFaction / surfaceLayer do not contribute config pages or ForcedMap.
            if (!AllParts.Any(p => p is ScenPart_ForcedMap))
            {
                list.Add(new Page_SelectStartingSite());
            }

            if (ModsConfig.IdeologyActive)
            {
                list.Add(new Page_VampireLordAutoIdeo());
            }

            foreach (Page item in AllParts.SelectMany(p => p.GetConfigPages()))
            {
                list.Add(item);
            }

            Page page = PageUtility.StitchedPages(list);
            if (page != null)
            {
                Page last = page;
                while (last.next != null)
                {
                    last = last.next;
                }

                last.nextAct = PageUtility.InitGameStart;
            }

            return page;
        }
    }
}
