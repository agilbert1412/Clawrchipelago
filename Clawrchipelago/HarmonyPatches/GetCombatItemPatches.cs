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
using Utils;

namespace Clawrchipelago.HarmonyPatches
{
    public static class GetCombatItemPatches
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            GetRandomCombatItemRewardsPatch.Initialize(logger, archipelago, locationChecker);
            GetRandomCombatItemRewardPatch.Initialize(logger, archipelago, locationChecker);
        }

        public static List<PickupItemData> GetRandomUnlockedItems(int count, List<PickupItemData> previousItems, Random rnd, bool upgraded,
            EItemRarity minimumRarity)
        {
            var rewards = new List<PickupItemData>();
            var validItems = GetValidUnlockedItems(minimumRarity);
            for (var index = 0; index < count; ++index)
            {
                var pickupItemSetting = validItems.RandomItemProbability(newItem => rewards.Any(r => r.Setting == newItem) || previousItems != null && previousItems.Any(p => p.Setting == newItem) || newItem.Rarity < minimumRarity ? 0.001f : newItem.CalculateDropProbability(), rnd);
                rewards.Add(pickupItemSetting.GenerateData(upgraded));
            }

            return rewards;
        }

        public static PickupItemSetting GetRandomUnlockedItem(List<PickupItemData> previousItems, Random rnd, EItemRarity minimumRarity)
        {
            var validItems = GetValidUnlockedItems();
            var reward = validItems.RandomItemProbability(newItem => previousItems != null && previousItems.Any(p => p.Setting == newItem) || newItem.Rarity < minimumRarity ? 0.001f : newItem.CalculateDropProbability(), rnd);

            return reward;
        }

        private static List<PickupItemSetting> GetValidUnlockedItems(EItemRarity minimumRarity = EItemRarity.Normal)
        {
            var allItems = Runtime.Configuration.Items;
            var validItems = allItems.Where(i => i.CanDrop() && i.Type == EPickupItemType.Combat && i.Rarity >= minimumRarity && _archipelago.HasReceivedItem(i.Name.ToEnglish())).ToList();
            return validItems;
        }
    }

    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch(nameof(Game.GetRandomItemRewards))]
    public class GetRandomCombatItemRewardsPatch
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

        // public List<PickupItemData> GetRandomItemRewards(int count, List<PickupItemData> previousItems = null, System.Random rnd = null, bool upgraded = false)
        public static bool Prefix(Game __instance, int count, List<PickupItemData> previousItems, Random rnd, bool upgraded, ref List<PickupItemData> __result)
        {
            try
            {
                _logger.LogDebugPatchIsRunning(nameof(Game), nameof(Game.GetRandomItemRewards), nameof(GetRandomCombatItemRewardPatch), nameof(Prefix));

                var minimumRarity = EItemRarity.Normal;
                rnd ??= new Random(__instance.Data.CurrentRoomSeed);

                var rewards = GetCombatItemPatches.GetRandomUnlockedItems(count, previousItems, rnd, upgraded, minimumRarity);

                __result = rewards;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(GetRandomCombatItemRewardPatch), nameof(Prefix), ex);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }

    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch(nameof(Game.GetRandomItemReward))]
    public class GetRandomCombatItemRewardPatch
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

        // public PickupItemSetting GetRandomItemReward(EItemRarity minimumRarity = EItemRarity.Normal, System.Random random = null, List<PickupItemData> previousItems = null)
        public static bool Prefix(Game __instance, EItemRarity minimumRarity, Random random, List<PickupItemData> previousItems, ref PickupItemSetting __result)
        {
            try
            {
                _logger.LogDebugPatchIsRunning(nameof(Game), nameof(Game.GetRandomItemReward), nameof(GetRandomCombatItemRewardPatch), nameof(Prefix));

                random ??= new Random(__instance.Data.CurrentRoomSeed);

                var reward = GetCombatItemPatches.GetRandomUnlockedItem(previousItems, random, minimumRarity);

                __result = reward;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(GetRandomCombatItemRewardPatch), nameof(Prefix), ex);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
