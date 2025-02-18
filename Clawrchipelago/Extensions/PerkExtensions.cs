using System.Collections.Generic;
using System.Linq;
using Gameplay.Items.Data;
using Gameplay.Items.Settings;

namespace Clawrchipelago.Extensions
{
    public static class PerkExtensions
    {
        public static int CountRealPerks(this List<PickupItemData> perks)
        {
            if (perks == null)
            {
                return 0;
            }
            return perks.Count(x => x.Setting.Type == EPickupItemType.Perk && x.Setting.Rarity != EItemRarity.DontDrop);
        }
    }
}
