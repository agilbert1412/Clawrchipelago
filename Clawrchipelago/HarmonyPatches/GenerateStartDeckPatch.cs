using HarmonyLib;
using System;
using Clawrchipelago.Extensions;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Gameplay;
using Gameplay.Items.Data;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Items.Settings;
using Platforms;

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

                __result.Clear();

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

                    for (var index = 0; index < receivedCount; ++index)
                    {
                        var existing = __result.FirstOrDefault(x => x.Setting.Name.ToEnglish().Equals(itemName) && !x.HasBeenUpgraded);
                        if (existing != null && existing.CanBeUpgraded())
                        {
                            existing.HasBeenUpgraded = true;
                            continue;
                        }

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
