using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Scenario subclass that replaces Ideology's Page_ChooseIdeoPreset with
    /// Page_LanternVillageAutoIdeo (Vampire Lord pattern). Avoids the root-level
    /// OnGUI NRE Joe hit when pressing Next on the starting-site page.
    /// </summary>
    public class LanternVillageScenario : Scenario
    {
        public override Page GetFirstConfigPage()
        {
            List<Page> list = new List<Page>();
            list.Add(new Page_SelectStoryteller());
            list.Add(new Page_CreateWorldParams());

            if (!AllParts.Any(p => p is ScenPart_ForcedMap))
            {
                list.Add(new Page_SelectStartingSite());
            }

            if (ModsConfig.IdeologyActive)
            {
                list.Add(new Page_LanternVillageAutoIdeo());
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
