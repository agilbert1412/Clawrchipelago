using System.Collections.Generic;
using System.Reflection;
using Gameplay.Items.Data;
using UI;

namespace Clawrchipelago.Extensions
{
    public static class ItemDeckDisplayExtensions
    {
        public static List<ItemDisplay> Items(this ItemDeckDisplay inventory)
        {
            // internal List<ItemDisplay> Items;
            var itemsField = typeof(ItemDeckDisplay).GetField("Items", BindingFlags.NonPublic | BindingFlags.Instance);
            var items = itemsField?.GetValue(inventory) as List<ItemDisplay>;
            return items;
        }

        public static Dictionary<ItemDisplay, List<PickupItemData>> ItemDetails(this ItemDeckDisplay inventory)
        {
            // internal Dictionary<ItemDisplay, List<PickupItemData>> ItemDetails;
            var itemsDetailsField = typeof(ItemDeckDisplay).GetField("ItemDetails", BindingFlags.NonPublic | BindingFlags.Instance);
            var itemsDetails = itemsDetailsField?.GetValue(inventory) as Dictionary<ItemDisplay, List<PickupItemData>>;
            return itemsDetails;
        }
    }
}
