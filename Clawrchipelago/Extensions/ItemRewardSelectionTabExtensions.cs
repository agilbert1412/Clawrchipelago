using System.Collections.Generic;
using System.Reflection;
using UI.TabbedPopup;

namespace Clawrchipelago.Extensions
{
    public static class ItemRewardSelectionTabExtensions
    {
        public static List<ItemRewardDisplay> RewardsButtonList(this ItemRewardSelectionTab itemRewardSelectionTab)
        {
            // private List<ItemRewardDisplay> _rewardButtonList;
            var rewardButtonListField = typeof(ItemRewardSelectionTab).GetField("_rewardButtonList", BindingFlags.NonPublic | BindingFlags.Instance);
            var rewardButtonList = rewardButtonListField?.GetValue(itemRewardSelectionTab) as List<ItemRewardDisplay>;
            return rewardButtonList;
        }
    }
}
