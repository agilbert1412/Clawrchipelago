using HarmonyLib;
using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Gameplay;
using Gameplay.Items.Data;
using Gameplay.Items.Settings;
using System.Linq;
using Clawrchipelago.Archipelago;
using Clawrchipelago.Extensions;
using UI;
using UI.TabbedPopup;
using Clawrchipelago.HarmonyPatches.PerkPatches;

namespace Clawrchipelago.HarmonyPatches.SavePatches
{
    public class PerkDisplayPatches
    {
        private static ILogger _logger;
        private static DungeonClawlerArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, DungeonClawlerArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            BigItemDisplayPatch.Initialize(logger, archipelago, locationChecker);
            TabbedPopupWindowAddTabPatch.Initialize(logger, archipelago, locationChecker);
        }

        public static void SetItemDescriptionToScoutedItem(BigItemDisplay itemDisplay, PickupItemData item)
        {
            var firstMissingLocation = AddNewPerkPatch.GetNextPerkLocationName(item);
            if (string.IsNullOrEmpty(firstMissingLocation))
            {
                _logger.LogWarning($"Skipping the scouting because we couldn't find a missing location for {item.Setting.Name.ToEnglish()}");
                return;
            }

            var scouted = _archipelago.ScoutSingleLocation(firstMissingLocation);
            itemDisplay.DescriptionLabel.text = $"{scouted.PlayerName}'s {scouted.ItemName}";
        }
    }

    [HarmonyPatch(typeof(BigItemDisplay))]
    [HarmonyPatch(nameof(BigItemDisplay.Init))]
    public class BigItemDisplayPatch
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

        // public void Init(PickupItemData item, bool hideUpgradeDescription = false)
        public static void Postfix(BigItemDisplay __instance, PickupItemData item, bool hideUpgradeDescription)
        {
            try
            {
                if (!_archipelago.SlotData.ShufflePerks)
                {
                    return;
                }

                _logger.LogDebugPatchIsRunning(nameof(BigItemDisplay), nameof(BigItemDisplay.Init), nameof(PerkDisplayPatches), nameof(Postfix));

                if (__instance == null || item == null || item.Setting == null || item.Setting.Type != EPickupItemType.Perk)
                {
                    _logger.LogDebug($"Skipping the scouting because this is not a perk");
                    return;
                }

                if (!Game.Instance.PopupWindow.Tabs.Any(x => x.GetContent() is ItemRewardSelectionTab))
                {
                    _logger.LogDebug($"Skipping the scouting because this is not an ItemRewardSelectionTab, this is a [{string.Join(",", Game.Instance.PopupWindow.Tabs.Select(x => x.GetContent()?.GetType()))}]");
                    return;
                }

                PerkDisplayPatches.SetItemDescriptionToScoutedItem(__instance, item);

                return;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(PerkDisplayPatches), nameof(Postfix), ex);
                return;
            }
        }
    }

    [HarmonyPatch(typeof(TabbedPopupWindow))]
    [HarmonyPatch(nameof(TabbedPopupWindow.AddTab))]
    public class TabbedPopupWindowAddTabPatch
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

        // public void AddTab(TabbedPopupWindowContent content, bool select = false)
        public static void Postfix(TabbedPopupWindow __instance, TabbedPopupWindowContent content, bool select)
        {
            try
            {
                if (!_archipelago.SlotData.ShufflePerks)
                {
                    return;
                }

                _logger.LogDebugPatchIsRunning(nameof(TabbedPopupWindow), nameof(TabbedPopupWindow.AddTab), nameof(TabbedPopupWindowAddTabPatch), nameof(Postfix));

                if (__instance == null || content == null)
                {
                    _logger.LogDebug($"Skipping the scouting because this is not a perk");
                    return;
                }

                if (content is not ItemRewardSelectionTab itemRewardSelectionTab)
                {
                    // _logger.LogDebug($"Skipping the scouting because this is not an ItemRewardSelectionTab, this is a {content.GetType().ToString()}");
                    return;
                }

                var rewardButtons = itemRewardSelectionTab.RewardsButtonList();
                if (!rewardButtons.Any())
                {
                    _logger.LogDebug($"Skipping the scouting because there are no items in this tab");
                    return;
                }

                foreach (var rewardButton in rewardButtons)
                {
                    var itemDisplay = rewardButton.ItemDisplay;
                    if (itemDisplay == null)
                    {
                        continue;
                    }

                    var item = itemDisplay.Item();
                    if (item.Setting == null || item.Setting.Type != EPickupItemType.Perk)
                    {
                        continue;
                    }

                    PerkDisplayPatches.SetItemDescriptionToScoutedItem(itemDisplay, item);
                }


                return;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(TabbedPopupWindowAddTabPatch), nameof(Postfix), ex);
                return;
            }
        }
    }
}
