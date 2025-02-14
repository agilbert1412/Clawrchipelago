using HarmonyLib;
using System;
using Clawrchipelago.Extensions;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Gameplay;
using Gameplay.Enemies;
using Gameplay.Items.Data;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Items.Settings;
using Platforms;
using UI.TabbedPopup;

namespace Clawrchipelago.HarmonyPatches
{
    [HarmonyPatch(typeof(StartConfiguration))]
    [HarmonyPatch(nameof(StartConfiguration.GenerateDeck))]
    public class GenerateStartDeckPatch
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public List<PickupItemData> GenerateDeck()
        public static void Postfix(StartConfiguration __instance, ref List<PickupItemData> __result)
        {
            try
            {
                _logger.LogDebugPatchIsRunning(nameof(StartConfiguration), nameof(StartConfiguration.GenerateDeck), nameof(GenerateStartDeckPatch), nameof(Postfix));

                for (var i = __result.Count - 1; i >= 0; i--)
                {
                    var startingItem = __result[i];
                    var receivedCount = _archipelago.GetReceivedItemCount(startingItem.Setting.Name.ToEnglish());
                    if (receivedCount <= 0)
                    {
                        __result.RemoveAt(i);
                    }
                }


                foreach (var item in Runtime.Configuration.Items)
                {
                    if (item.Type != EPickupItemType.Combat)
                    {
                        continue;
                    }

                    var itemName = item.Name.ToEnglish();
                    var receivedCount = _archipelago.GetReceivedItemCount(itemName);

                    if (receivedCount <= 0)
                    {
                        continue;
                    }

                    if (__result.Any(x => x.Setting.Name.ToEnglish().Equals(itemName)))
                    {
                        continue;
                    }

                    for (int index = 0; index < receivedCount; ++index)
                    {
                        var data = item.GenerateData();
                        __result.Add(data);
                    }

                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(GenerateStartDeckPatch), nameof(Postfix), ex);
                return;
            }
        }
    }
}
