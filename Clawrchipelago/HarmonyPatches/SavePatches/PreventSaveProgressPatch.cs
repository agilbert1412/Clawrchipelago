using HarmonyLib;
using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Clawrchipelago.Archipelago;
using Platforms.Handler;
using Platforms.Persistency;

namespace Clawrchipelago.HarmonyPatches.SavePatches
{
    [HarmonyPatch(typeof(PCHandler))]
    [HarmonyPatch(nameof(PCHandler.SaveProgress))]
    public class PreventSaveProgressPatch
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

        // public override void SaveProgress(PersistentGameData data)
        public static bool Prefix(PCHandler __instance, PersistentGameData data)
        {
            try
            {
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(PreventSaveProgressPatch), nameof(Prefix), ex);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
