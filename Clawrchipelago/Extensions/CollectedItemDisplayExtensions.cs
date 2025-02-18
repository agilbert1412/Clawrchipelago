using System.Reflection;
using UI;

namespace Clawrchipelago.Extensions
{
    public static class CollectedItemDisplayExtensions
    {
        public static bool HasItemsLeft(this CollectedItemDisplay collectedItemDisplay)
        {
            // internal bool HasItemsLeft()
            var hasItemsLeftMethod = typeof(CollectedItemDisplay).GetMethod("HasItemsLeft", BindingFlags.NonPublic | BindingFlags.Instance);
            var hasItemsLeft = (bool)(hasItemsLeftMethod?.Invoke(collectedItemDisplay, []));
            return hasItemsLeft;
        }
    }
}
