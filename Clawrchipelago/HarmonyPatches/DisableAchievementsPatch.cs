using HarmonyLib;
using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Clawrchipelago.Archipelago;
using Platforms.Achievements;

namespace Clawrchipelago.HarmonyPatches
{
    [HarmonyPatch(typeof(AchievementSystem))]
    [HarmonyPatch(nameof(AchievementSystem.UnlockAchievement))]
    public class DisableAchievementsPatch
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

        // public static void UnlockAchievement(EAchievement achievement)
        public static bool Prefix(EAchievement achievement)
        {
            try
            {
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(DisableAchievementsPatch), nameof(Prefix), ex);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
