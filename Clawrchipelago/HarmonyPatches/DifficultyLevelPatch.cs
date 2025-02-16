using HarmonyLib;
using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using UI;
using Gameplay.Items.Data;
using System.Collections.Generic;
using System.Linq;
using Clawrchipelago.Archipelago;
using Clawrchipelago.Extensions;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Platforms;

namespace Clawrchipelago.HarmonyPatches
{
    [HarmonyPatch(typeof(DifficultyLevel))]
    [HarmonyPatch(nameof(DifficultyLevel.Init))]
    public class DifficultyLevelPatch
    {
        private static ILogger _logger;
        private static DungeonClawlerArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, DungeonClawlerArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public void Init(ItemSelectionPopup itemSelectionPopup, List<PickupItemData> allPaws)
        public static bool Prefix(DifficultyLevel __instance, ItemSelectionPopup itemSelectionPopup, List<PickupItemData> allPaws)
        {
            try
            {
                _logger.LogDebugPatchIsRunning(nameof(DifficultyLevel), nameof(DifficultyLevel.Init), nameof(DifficultyLevelPatch), nameof(Prefix));

                var fighters = Runtime.Configuration.Fighters;
                IEnumerable<PickupItemData> unlockedPaws;
                if (_archipelago.SlotData.ShuffleFighters == ShuffleFighters.FightersAndPaws)
                {
                    unlockedPaws = fighters.Select(i => i.RewardPaw.GenerateData()).Where(x => _archipelago.HasReceivedItem(x.Setting.Name.ToEnglish()));
                }
                else if (_archipelago.SlotData.ShuffleFighters == ShuffleFighters.Fighters)
                {
                    unlockedPaws = fighters.Where(x => _archipelago.HasReceivedItem(x.Name.ToEnglish())).Select(i => i.RewardPaw.GenerateData());
                }
                else
                {
                    unlockedPaws = fighters.Select(i => i.RewardPaw.GenerateData());
                }

                var paws = unlockedPaws.ToList();
                __instance.DifficultyPanel.Init(itemSelectionPopup, __instance.DifficultyButton, paws);
                __instance.DifficultyButton.interactable = paws.Count >= (int)__instance.Level;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(DifficultyLevelPatch), nameof(Prefix), ex);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
