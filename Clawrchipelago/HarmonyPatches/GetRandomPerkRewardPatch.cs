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

namespace Clawrchipelago.HarmonyPatches
{
    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch(nameof(Game.GetRandomPerkRewards))]
    public class GetRandomPerkRewardPatch
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

        // public List<PickupItemData> GetRandomPerkRewards(int count, List<PickupItemData> previousItems = null, System.Random rnd = null)
        public static bool Prefix(Game __instance, int count, List<PickupItemData> previousItems, Random rnd, ref List<PickupItemData> __result)
        {
            try
            {
                _logger.LogDebugPatchIsRunning(nameof(Game), nameof(Game.GetRandomPerkRewards), nameof(GetRandomPerkRewardPatch), nameof(Prefix));

                rnd ??= new Random(__instance.Data.CurrentRoomSeed);

                var rewards = new List<PickupItemData>();
                var allItems = Runtime.Configuration.Items;
                var missingLocations = _locationChecker.GetAllMissingLocationNames();
                var allPerks = allItems.Where(item => item.CanDrop() && item.Type == EPickupItemType.Perk);
                var itemsWithALocationToCheck = allPerks.Where(x =>
                    missingLocations.Any(location => location.StartsWith($"{x.Name.ToEnglish()} - Level ")));
                var itemLocations = itemsWithALocationToCheck.ToDictionary(x => x,
                    y => missingLocations.Where(location => location.StartsWith($"{y.Name.ToEnglish()} - Level ")).ToList());

                var itemsInOrder = itemsWithALocationToCheck.OrderByDescending(x => _archipelago.GetReceivedItemCount(x.Name.ToEnglish())).ThenByDescending(x => itemLocations[x].Count).ToList();

                if (!itemsInOrder.Any())
                {
                    __result = rewards;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var individualChance = Math.Min(1.0, (1.0 / itemsInOrder.Count) * 1.25);
                for (var i = 0; i < count; i++)
                {
                    var pickedItem = itemsInOrder.First();
                    foreach (var pickupItemSetting in itemsInOrder)
                    {
                        if (rnd.NextDouble() > individualChance)
                        {
                            continue;
                        }

                        pickedItem = pickupItemSetting;
                        break;
                    }

                    rewards.Add(pickedItem.GenerateData());
                    itemsInOrder.Remove(pickedItem);

                    if (!itemsInOrder.Any())
                    {
                        __result = rewards;
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }
                }

                __result = rewards;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(GetRandomPerkRewardPatch), nameof(Prefix), ex);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
        private static bool CanGetOneMoreStackOfPerk(PickupItemData ownedPerk, PickupItemSetting candidatePerk)
        {
            if (ownedPerk.Setting != candidatePerk)
            {
                return true;
            }

            if (!ownedPerk.Setting.IsStackable)
            {
                return false;
            }

            if (!ownedPerk.Setting.HasStackLimit)
            {
                return true;
            }

            return ownedPerk.PerkCount < ownedPerk.Setting.StackLimit;
        }
    }
}
