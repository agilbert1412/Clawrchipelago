using System.Collections;
using System.Reflection;
using Gameplay;
using Gameplay.Items.Data;
using UI;

namespace Clawrchipelago.Extensions
{
    public static class BigItemDisplayExtensions
    {
        public static PickupItemData Item(this BigItemDisplay bigItemDisplay)
        {
            // internal PickupItemData Item;
            var itemField = typeof(BigItemDisplay).GetField("Item", BindingFlags.NonPublic | BindingFlags.Instance);
            var item = (PickupItemData)(itemField.GetValue(bigItemDisplay));
            return item;
        }
    }
}
