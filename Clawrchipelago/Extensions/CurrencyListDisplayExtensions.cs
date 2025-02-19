using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Gameplay;
using Gameplay.Currencies;
using UI;

namespace Clawrchipelago.Extensions
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
