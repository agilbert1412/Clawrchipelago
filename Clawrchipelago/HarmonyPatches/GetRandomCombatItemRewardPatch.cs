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
    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch(nameof(Game.GetRandomItemRewards))]
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

        // public List<PickupItemData> GetRandomItemRewards(int count, List<PickupItemData> previousItems = null, System.Random rnd = null, bool upgraded = false)
        public static bool Prefix(Game __instance, int count, List<PickupItemData> previousItems, Random rnd, bool upgraded, ref List<PickupItemData> __result)
        {
            try
            {
                _logger.LogDebugPatchIsRunning(nameof(Game), nameof(Game.GetRandomItemRewards), nameof(GetRandomCombatItemRewardPatch), nameof(Prefix));

                var minimumRarity = EItemRarity.Normal;
                rnd ??= new Random(__instance.Data.CurrentRoomSeed);

                var rewards = new List<PickupItemData>();
                var allItems = Runtime.Configuration.Items;
                var validItems = allItems.Where(i => i.CanDrop() && i.Type == EPickupItemType.Combat && i.Rarity >= minimumRarity && _archipelago.HasReceivedItem(i.Name.ToEnglish())).ToList();
                for (var index = 0; index < count; ++index)
                {
                    var pickupItemSetting = validItems.RandomItemProbability(newItem => rewards.Any(r => r.Setting == newItem) || previousItems != null && previousItems.Any(p => p.Setting == newItem) ? 0.0f : newItem.CalculateDropProbability(), rnd);
                    rewards.Add(pickupItemSetting.GenerateData(upgraded));
                }

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
}
