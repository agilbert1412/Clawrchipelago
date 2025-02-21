using HarmonyLib;
using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Gameplay;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Gameplay.Items.Data;
using System.Linq;
using Clawrchipelago.Archipelago;
using Clawrchipelago.Extensions;

namespace Clawrchipelago.HarmonyPatches.PerkPatches
{
    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch(nameof(Game.AddNewPerk))]
    public class AddNewPerkPatch
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

        // public void AddNewPerk(PickupItemData perk)
        public static bool Prefix(Game __instance, PickupItemData perk)
        {
            try
            {
                if (!_archipelago.SlotData.ShufflePerks)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                _logger.LogDebugPatchIsRunning(nameof(Game), nameof(Game.AddNewPerk), nameof(AddNewPerkPatch), nameof(Prefix));

                var firstMissingLocation = GetNextPerkLocationName(perk);
                if (string.IsNullOrWhiteSpace(firstMissingLocation))
                {
                    if (ClawrchipelagoMod.Instance.Config.AllowStackingPerksWhenChecksAreDone &&
                        __instance.Data.Perks.Any(x => x.Setting == perk.Setting))
                    {
                        return MethodPrefix.RUN_ORIGINAL_METHOD;
                    }

                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                _locationChecker.AddCheckedLocation(firstMissingLocation);

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(AddNewPerkPatch), nameof(Prefix), ex);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static string GetNextPerkLocationName(PickupItemData perk)
        {
            var perkName = perk.Setting.Name.ToEnglish();
            var missingLocations = _locationChecker.GetAllMissingLocationNames();
            var missingLocationsMatchingThisPerk = missingLocations.Where(x => x.StartsWith($"{perkName} - Level"));
            var firstMissingLocation = missingLocationsMatchingThisPerk.OrderBy(x => int.Parse(x.Split(" ").Last())).FirstOrDefault();
            return firstMissingLocation;
        }
    }
}
