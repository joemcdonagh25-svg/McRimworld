using System.Collections.Generic;
using Verse;

namespace VampireLord.Campaign
{
    /// <summary>
    /// Accept / decline offer to spend Keep Blood on gate sandbags.
    /// </summary>
    public class ChoiceLetter_VampireLordFortify : ChoiceLetter
    {
        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly)
                {
                    yield return Option_Close;
                    yield break;
                }

                DiaOption accept = new DiaOption("AcceptButton".Translate());
                DiaOption reject = new DiaOption("RejectLetter".Translate());

                accept.action = delegate
                {
                    if (VampireLordCampaign.TryGet(out VampireLordCampaignGameComponent campaign))
                    {
                        VampireLordFortify.TryPurchase(campaign, forced: false, reason: "letter");
                    }

                    Find.LetterStack.RemoveLetter(this);
                };
                accept.resolveTree = true;

                string blockReason = "Unavailable";
                if (!VampireLordCampaign.TryGet(out VampireLordCampaignGameComponent live) ||
                    !VampireLordFortify.CanPurchase(live, forced: false, out blockReason))
                {
                    accept.Disable(blockReason ?? "Unavailable");
                }

                reject.action = delegate
                {
                    Find.LetterStack.RemoveLetter(this);
                };
                reject.resolveTree = true;

                yield return accept;
                yield return reject;
                yield return Option_Postpone;
            }
        }
    }
}
