using System.Collections.Generic;
using Clawrchipelago.HarmonyPatches;
using Gameplay.Items.Data;
using Gameplay;
using Platforms;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using Clawrchipelago.Archipelago;
using Clawrchipelago.Extensions;
using Clawrchipelago.Serialization;
using Gameplay.Items.Settings;
using Clawrchipelago.UI;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace Clawrchipelago.Items
{
    public class ClawrchipelagoItemManager : ItemManager
    {
        private ILogger _logger;
        private TrapExecutor _trapExecutor;
        private RecentItemsAndLocations _recentItemsAndLocations;

        public ClawrchipelagoItemManager(ILogger logger, DungeonClawlerArchipelagoClient archipelago, TrapExecutor trapExecutor,
            IEnumerable<ReceivedItem> itemsAlreadyProcessed, RecentItemsAndLocations recentItemsAndLocations) : base(archipelago, itemsAlreadyProcessed)
        {
            _logger = logger;
            _trapExecutor = trapExecutor;
            _recentItemsAndLocations = recentItemsAndLocations;
        }

        protected override void ProcessItem(ReceivedItem receivedItem, bool immediatelyIfPossible)
        {
            if (TryHandleReceivedPerk(receivedItem))
            {
                return;
            }

            if (_trapExecutor.TryHandleReceivedTrap(receivedItem))
            {
                return;
            }
        }

        public bool TryHandleReceivedPerk(ReceivedItem item)
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
