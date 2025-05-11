using System.Collections.Generic;
using System.Reflection;
using Gameplay.Currencies;
using UI;

namespace DataExporter.Extensions
{
    public static class CurrencyListDisplayExtensions
    {
        public static Dictionary<CurrencySetting, CurrencyDisplay> CurrencyDisplays(this CurrencyListDisplay currencyListDisplay)
        {
            // private Dictionary<CurrencySetting, CurrencyDisplay> CurrencyDisplays;
            var currencyDisplaysField = typeof(CurrencyListDisplay).GetField("CurrencyDisplays", BindingFlags.NonPublic | BindingFlags.Instance);
            var currencyDisplays = currencyDisplaysField.GetValue(currencyListDisplay) as Dictionary<CurrencySetting, CurrencyDisplay>;
            return currencyDisplays;
        }
    }
}
