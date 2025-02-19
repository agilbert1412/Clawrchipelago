using Clawrchipelago.HarmonyPatches;
using Gameplay.Items.Data;
using Gameplay;
using Platforms;
using System;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using Clawrchipelago.Archipelago;
using Clawrchipelago.Extensions;
using Gameplay.Items.Settings;
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
        private TrapExecutor _trapExecutor;
        private RecentItemsAndLocations _recentItemsAndLocations;

        public ItemManager(ILogger logger, DungeonClawlerArchipelagoClient archipelago, TrapExecutor trapExecutor,
            RecentItemsAndLocations recentItemsAndLocations)
        {
            _logger = logger;
            _archipelago = archipelago;
            _trapExecutor = trapExecutor;
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

            if (_trapExecutor.TryHandleReceivedTrap(lastItem))
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
    }
}
