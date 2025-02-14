using HarmonyLib;
using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Gameplay;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Gameplay.Items.Data;
using Gameplay.Items.Settings;
using Platforms;
using System.Collections.Generic;
using System.Linq;
using Clawrchipelago.Extensions;
using UI;
using Utils;

namespace Clawrchipelago.HarmonyPatches
{
    [HarmonyPatch(typeof(BigItemDisplay))]
    [HarmonyPatch(nameof(BigItemDisplay.Init))]
    public class PerkDisplayPatch
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

        // public void Init(PickupItemData item, bool hideUpgradeDescription = false)
        public static void Postfix(BigItemDisplay __instance, PickupItemData item, bool hideUpgradeDescription)
        {
            try
            {
                _logger.LogDebugPatchIsRunning(nameof(BigItemDisplay), nameof(BigItemDisplay.Init), nameof(PerkDisplayPatch), nameof(Postfix));

                if (item.Setting.Type != EPickupItemType.Perk)
                {
                    return;
                }

                var firstMissingLocation = AddNewPerkPatch.GetNextPerkLocationName(item);
                var scouted = _archipelago.ScoutSingleLocation(firstMissingLocation);
                __instance.DescriptionLabel.text = $"{scouted.PlayerName}'s {scouted.ItemName}";
                
                return;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(PerkDisplayPatch), nameof(Postfix), ex);
                return;
            }
        }
    }
}
