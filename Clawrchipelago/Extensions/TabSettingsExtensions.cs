using System.Reflection;
using UI.TabbedPopup;

namespace Clawrchipelago.Extensions
{
    public static class TabSettingsExtensions
    {
        public static TabbedPopupWindowContent GetContent(this TabSettings combatant)
        {
            // internal TabbedPopupWindowContent Content;
            var contentField = typeof(TabSettings).GetField("Content", BindingFlags.NonPublic | BindingFlags.Instance);
            var content = contentField?.GetValue(combatant) as TabbedPopupWindowContent;
            return content;
        }
    }
}
