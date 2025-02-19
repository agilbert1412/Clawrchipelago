using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using Clawrchipelago.Extensions;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Gameplay;
using Gameplay.Items.Settings;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using UI;
using Gameplay.Items.Data;
using System.Collections.Generic;
using TMPro;
using Platforms;

namespace Clawrchipelago.HarmonyPatches
{
    public class RecycleUIPatches
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            RecycleUIInitPatch.Initialize(logger, archipelago, locationChecker);
            RecycleUIUpdatePatch.Initialize(logger, archipelago, locationChecker);
            RecyclerItemClickedPatch.Initialize(logger, archipelago, locationChecker);
            DeckItemClickedPatch.Initialize(logger, archipelago, locationChecker);
            RecycleButtonClickedPatch.Initialize(logger, archipelago, locationChecker);
        }

        public static int GetMaxStartingCombatItems() => ClawrchipelagoMod.Instance.Config.StartingCombatInventorySize + _archipelago.GetReceivedItemCount("Combat Inventory Size");

        public static int GetMaxPerks() => ClawrchipelagoMod.Instance.Config.StartingPerkInventorySize + _archipelago.GetReceivedItemCount("Perk Inventory Size");

        public static int IsSpecialShredder()
        {
            var mapData = Game.Instance?.Data?.MapData;
            if (mapData == null)
            {
                return 0;
            }

            if (mapData.Floor != 1)
            {
                return 0;
            }

            if (mapData.CurrentMapPosition == InitFreshGamePatch.CombatShredderPosition)
            {
                return 2;
            }

            if (mapData.CurrentMapPosition == InitFreshGamePatch.PerksShredderPosition)
            {
                return 1;
            }

            return 0;
        }

        public static void RemovePerkItemFromInventory(ItemDeckDisplay inventory, PickupItemData item)
        {
            ItemDisplay itemDisplay;
            if (!inventory.GetItemDisplay(item, out itemDisplay))
                return;
            inventory.Items().Remove(itemDisplay);
            inventory.ItemDetails().Remove(itemDisplay);
            UnityEngine.Object.Destroy(itemDisplay.gameObject);
        }
    }

    [HarmonyPatch(typeof(RecycleUI))]
    [HarmonyPatch(nameof(RecycleUI.Init))]
    public class RecycleUIInitPatch
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

        // public void Init()
        public static void Postfix(RecycleUI __instance)
        {
            try
            {
                if (RecycleUIPatches.IsSpecialShredder() <= 0)
                {
                    return;
                }

                _logger.LogDebugPatchIsRunning(nameof(RecycleUI), nameof(RecycleUI.Init), nameof(RecycleUIInitPatch), nameof(Postfix));

                if (RecycleUIPatches.IsSpecialShredder() == 2)
                {
                    InitStartingCombatItemsShredder(__instance);
                }
                else if (RecycleUIPatches.IsSpecialShredder() == 1)
                {
                    InitPerksShredder(__instance);
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(RecycleUIInitPatch), nameof(Postfix), ex);
                return;
            }
        }

        private static void InitStartingCombatItemsShredder(RecycleUI recycleUI)
        {
            // private int _shredderCoupons;
            var shredderCouponsField = typeof(RecycleUI).GetField("_shredderCoupons", BindingFlags.NonPublic | BindingFlags.Instance);
            shredderCouponsField.SetValue(recycleUI, Game.Instance.Data.Items.Count * 10);

            recycleUI.Inventory.Init(new List<PickupItemData>());
            recycleUI.Recycler.Init(Game.Instance.Data.Items.ToList());
        }

        private static void InitPerksShredder(RecycleUI recycleUI)
        {
            // private int _shredderCoupons;
            var shredderCouponsField = typeof(RecycleUI).GetField("_shredderCoupons", BindingFlags.NonPublic | BindingFlags.Instance);
            shredderCouponsField.SetValue(recycleUI, Game.Instance.Data.Perks.Count * 10);

            recycleUI.Inventory.Init(new List<PickupItemData>());
            var perks = Game.Instance.Data.Perks;
            var perksToShow = perks.Where(x => x.Setting.Type == EPickupItemType.Perk && x.Setting.Rarity != EItemRarity.DontDrop)
                .ToList();
            recycleUI.Recycler.Init(perksToShow);
        }
    }

    [HarmonyPatch(typeof(RecycleUI))]
    [HarmonyPatch(nameof(RecycleUI.Update))]
    public class RecycleUIUpdatePatch
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static string _deckOriginalText;
        private static string _finishButtonOriginalText;
        private static string _recycleButtonOriginalText;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _deckOriginalText = "";
            _finishButtonOriginalText = "";
            _recycleButtonOriginalText = "";
        }

        // public void Update()
        public static void Postfix(RecycleUI __instance)
        {
            try
            {
                if (RecycleUIPatches.IsSpecialShredder() <= 0)
                {
                    return;
                }

                // _logger.LogDebugPatchIsRunning(nameof(RecycleUI), nameof(RecycleUI.Update), nameof(RecycleUIUpdatePatch), nameof(Postfix));


                var inventoryItems = GetItemCount(__instance.Inventory);
                var recyclerItems = GetItemCount(__instance.Recycler);

                var recyclerIsEmpty = recyclerItems <= 0;

                if (RecycleUIPatches.IsSpecialShredder() == 2)
                {
                    var maxCombatItems = RecycleUIPatches.GetMaxStartingCombatItems();
                    var inventoryIsNotTooFull = inventoryItems <= maxCombatItems;
                    __instance.FinishButton.interactable = recyclerIsEmpty && inventoryIsNotTooFull;
                    __instance.RecycleButton.Button.interactable = inventoryItems >= 1 && recyclerItems >= 1;

                    UpdateDeckLabel(__instance, maxCombatItems, "Deck");
                    UpdateFinishButtonLabel(__instance, inventoryIsNotTooFull, inventoryItems, maxCombatItems, recyclerIsEmpty, "Item");
                    UpdateRecycleButtonLabel(__instance, inventoryItems);
                }
                else if (RecycleUIPatches.IsSpecialShredder() == 1)
                {
                    var maxPerks = RecycleUIPatches.GetMaxPerks();
                    var inventoryIsNotTooFull = inventoryItems <= maxPerks;
                    __instance.FinishButton.interactable = recyclerIsEmpty && inventoryIsNotTooFull;
                    __instance.RecycleButton.Button.interactable = inventoryItems >= 1 && recyclerItems >= 1;

                    UpdateDeckLabel(__instance, maxPerks, "Perks");
                    UpdateFinishButtonLabel(__instance, inventoryIsNotTooFull, inventoryItems, maxPerks, recyclerIsEmpty, "Perk");
                    UpdateRecycleButtonLabel(__instance, inventoryItems);
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(RecycleUIUpdatePatch), nameof(Postfix), ex);
                return;
            }
        }

        private static void UpdateDeckLabel(RecycleUI recycleUI, int maxItems, string itemType)
        {
            var labels = recycleUI.GetComponentsInChildren<TextMeshProUGUI>();
            var deckLabel = labels.FirstOrDefault(x => x.name == "Title");
            if (string.IsNullOrWhiteSpace(_deckOriginalText))
            {
                _deckOriginalText = deckLabel.text;
            }

            deckLabel.text = $"Your {itemType} (Max: {maxItems})";
        }

        private static void UpdateFinishButtonLabel(RecycleUI recycleUI, bool inventoryIsNotTooFull, int inventoryItems,
            int maxItems, bool recyclerIsEmpty, string itemType)
        {
            var finishButtonLabel = recycleUI.FinishButton.GetComponentsInChildren<TextMeshProUGUI>().First();
            if (string.IsNullOrWhiteSpace(_finishButtonOriginalText))
            {
                _finishButtonOriginalText = finishButtonLabel.text;
            }

            if (!inventoryIsNotTooFull)
            {
                var needToShred = inventoryItems - maxItems;
                var itemTypeText = itemType + (needToShred > 1 ? "s" : "");
                finishButtonLabel.text = $"Shred {needToShred} More {itemTypeText} To Proceed";
            }
            else if (!recyclerIsEmpty)
            {
                finishButtonLabel.text = $"Empty Shredder To Proceed";
            }
            else
            {
                finishButtonLabel.text = _finishButtonOriginalText;
            }
        }

        private static void UpdateRecycleButtonLabel(RecycleUI recycleUI, int inventoryItems)
        {
            var recycleButtonLabel = recycleUI.RecycleButton.Button.GetComponentsInChildren<TextMeshProUGUI>().First();
            if (string.IsNullOrWhiteSpace(_recycleButtonOriginalText))
            {
                _recycleButtonOriginalText = recycleButtonLabel.text;
            }

            if (inventoryItems < 1)
            {
                recycleButtonLabel.text = "Don't Shred Everything";
            }
            else
            {
                recycleButtonLabel.text = _recycleButtonOriginalText;
            }
        }

        private static int GetItemCount(ItemDeckDisplay inventory)
        {
            var items = inventory.Items();
            var recycleItemCount = 0;
            if (items == null)
            {
                return 0;
            }

            foreach (var itemDisplay in items)
            {
                // _logger.LogDebug($"{itemDisplay.Item.Setting.Name.ToEnglish()} ({itemDisplay.Count})");
                recycleItemCount += itemDisplay.Count;
                if (itemDisplay.Item.PerkCount > 1)
                {
                    recycleItemCount -= itemDisplay.Item.PerkCount-1;
                }
            }

            return recycleItemCount;
        }
    }

    [HarmonyPatch(typeof(RecycleUI))]
    [HarmonyPatch("RecyclerItemClicked")]
    public class RecyclerItemClickedPatch
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

        // private void RecyclerItemClicked(UI.ItemDisplay item)
        public static bool Prefix(RecycleUI __instance, ItemDisplay item)
        {
            try
            {
                if (RecycleUIPatches.IsSpecialShredder() != 1)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                _logger.LogDebugPatchIsRunning(nameof(RecycleUI), "RecyclerItemClicked", nameof(RecyclerItemClickedPatch), nameof(Prefix));

                var firstItem = __instance.Recycler.GetFirstItem(item);
                __instance.Inventory.AddItem(firstItem);
                RecycleUIPatches.RemovePerkItemFromInventory(__instance.Recycler, firstItem);


                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(RecyclerItemClickedPatch), nameof(Prefix), ex);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }

    [HarmonyPatch(typeof(RecycleUI))]
    [HarmonyPatch("DeckItemClicked")]
    public class DeckItemClickedPatch
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

        // private void DeckItemClicked(UI.ItemDisplay item)
        public static bool Prefix(RecycleUI __instance, ItemDisplay item)
        {
            try
            {
                if (RecycleUIPatches.IsSpecialShredder() != 1)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                _logger.LogDebugPatchIsRunning(nameof(RecycleUI), "DeckItemClicked", nameof(DeckItemClickedPatch), nameof(Prefix));

                var firstItem = __instance.Inventory.GetFirstItem(item);
                __instance.Recycler.AddItem(firstItem);
                RecycleUIPatches.RemovePerkItemFromInventory(__instance.Inventory, firstItem);


                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(DeckItemClickedPatch), nameof(Prefix), ex);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }

    [HarmonyPatch(typeof(RecycleUI))]
    [HarmonyPatch("RecycleButtonClicked")]
    public class RecycleButtonClickedPatch
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

        // private void RecycleButtonClicked()
        public static bool Prefix(RecycleUI __instance)
        {
            try
            {
                if (RecycleUIPatches.IsSpecialShredder() != 1)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                _logger.LogDebugPatchIsRunning(nameof(RecycleUI), "RecycleButtonClicked", nameof(RecycleButtonClickedPatch), nameof(Prefix));

                __instance.Recycler.RemoveAllItems();
                Game.Instance.Data.Perks = Game.Instance.Data.Perks.Where(x => x.Setting.Type == EPickupItemType.LuckyPaw || x.Setting.DontDrop || x.Setting.Rarity == EItemRarity.DontDrop).Union(__instance.Inventory.GetAllItems()).ToList();
                AudioSystem.PlaySoundEffect(Runtime.Configuration.AudioSettings.ShredderSound);

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(RecycleButtonClickedPatch), nameof(Prefix), ex);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
