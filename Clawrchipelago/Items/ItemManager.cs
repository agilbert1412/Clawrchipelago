using Clawrchipelago.HarmonyPatches;
using Gameplay.Items.Data;
using Gameplay;
using Platforms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Archipelago.MultiClient.Net.Models;
using Clawrchipelago.Archipelago;
using Clawrchipelago.Extensions;
using Gameplay.Items.Settings;
using UI;
using Gameplay.Values;
using System.Collections;
using Gameplay.Liquid.Settings;
using Clawrchipelago.UI;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace Clawrchipelago.Items
{
    public class ItemManager
    {
        private ILogger _logger;
        private DungeonClawlerArchipelagoClient _archipelago;
        private RecentItemsAndLocations _recentItemsAndLocations;

        public ItemManager(ILogger logger, DungeonClawlerArchipelagoClient archipelago, RecentItemsAndLocations recentItemsAndLocations)
        {
            _logger = logger;
            _archipelago = archipelago;
            _recentItemsAndLocations = recentItemsAndLocations;
        }

        public void OnItemReceived()
        {
            _recentItemsAndLocations.UpdateItems();
            if (_archipelago == null || !_archipelago.IsConnected) // || _itemManager == null)
            {
                return;
            }

            var allReceivedItems = _archipelago.GetSession().Items.AllItemsReceived;
            if (allReceivedItems == null)
            {
                return;
            }


            var lastItem = allReceivedItems.LastOrDefault();
            if (TryHandleReceivedPerk(lastItem))
            {
                return;
            }

            if (TryHandleReceivedTrap(lastItem))
            {
                return;
            }
        }

        public bool TryHandleReceivedPerk(ItemInfo item)
        {
            if (item == null)
            {
                return false;
            }

            var allItems = Runtime.Configuration?.Items;
            var perksDeck = Game.Instance?.Data?.Perks;
            if (allItems == null || perksDeck == null)
            {
                return false;
            }

            var perk = allItems.FirstOrDefault(x =>
                x.Type == EPickupItemType.Perk && x.Rarity != EItemRarity.DontDrop &&
                x.Name.ToEnglish() == item.ItemName);

            if (perk == null)
            {
                return false;
            }

            var perksCount = perksDeck.CountRealPerks();
            bool IsThisItem(PickupItemData x) => x.Setting.Name.ToEnglish().Equals(item.ItemName);
            var receivedCount = _archipelago.GetReceivedItemCount(item.ItemName);
            if (perksDeck.Any(IsThisItem))
            {
                perksDeck.First(IsThisItem).PerkCount = receivedCount;
            }
            else if (perksCount < RecycleUIPatches.GetMaxPerks())
            {
                perksDeck.Add(perk.GenerateData());
                perksDeck.First(IsThisItem).PerkCount = receivedCount;
            }
            return true;
        }

        public bool TryHandleReceivedTrap(ItemInfo item)
        {
            _logger.LogDebug($"TryHandleReceivedTrap: {item.ItemName}");
            var allItems = Runtime.Configuration?.Items;
            var allLiquids = Runtime.Configuration?.Liquids;
            var clawMachine = Game.Instance?.ClawMachine;
            if (allItems == null || clawMachine == null || allLiquids == null)
            {
                return false;
            }

            var liquidToAdd = GetTrapLiquidToAdd(item, allLiquids);
            if (liquidToAdd != null)
            {
                _logger.LogDebug($"Adding Liquid: {liquidToAdd.Name.ToEnglish()}");
                Game.Instance.StartCoroutine(FillMachineWithLiquid(clawMachine, liquidToAdd));
                return true;
            }

            var itemToAdd = GetTrapItemToAdd(item, allItems);
            if (itemToAdd != null)
            {
                var amount = _archipelago.SlotData.TrapDifficulty switch
                {
                    TrapDifficulty.NoTraps => 0,
                    TrapDifficulty.Easy => 1,
                    TrapDifficulty.Medium => 2,
                    TrapDifficulty.Hard => 4,
                    TrapDifficulty.Hell => 8,
                    TrapDifficulty.Nightmare => 16,
                    _ => throw new ArgumentOutOfRangeException()
                };

                _logger.LogDebug($"Adding Item: {itemToAdd.Name.ToEnglish()} ({amount})");

                Game.Instance.StartCoroutine(clawMachine.AddItems(itemToAdd, amount));
                return true;
            }

            _logger.LogDebug($"Not a trap :(");
            return false;
        }

        private LiquidSetting GetTrapLiquidToAdd(ItemInfo item, LiquidSetting[] allLiquids)
        {
            foreach (var potentialLiquid in allLiquids)
            {
                var name = potentialLiquid.Name.ToEnglish();
                if ($"{name} Trap" == item.ItemName)
                {
                    return potentialLiquid;
                }
            }

            return null;
        }

        private PickupItemSetting GetTrapItemToAdd(ItemInfo item, PickupItemSetting[] allItems)
        {
            foreach (var potentialItem in allItems)
            {
                var name = potentialItem.Name.ToEnglish();
                if ($"{name} Trap" == item.ItemName)
                {
                    return potentialItem;
                }
            }

            return null;
        }

        private IEnumerator FillMachineWithLiquid(ClawMachine clawMachine, LiquidSetting liquid)
        {
            yield return clawMachine.ClawMachineBox.WaterLevel.ChangeLiquid(liquid);
            yield return clawMachine.ClawMachineBox.WaterLevel.ChangeWaterLevel(null, 1.0f, null);
        }
    }
}
