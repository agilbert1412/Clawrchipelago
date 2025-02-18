using HarmonyLib;
using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Gameplay;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Platforms.Handler;

namespace Clawrchipelago.HarmonyPatches
{
    [HarmonyPatch(typeof(PCHandler))]
    [HarmonyPatch(nameof(PCHandler.SaveGame))]
    public class SavePatch
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

        // public override void SaveGame(int saveSlot, GameData game)
        public static bool Prefix(PCHandler __instance, int saveSlot, GameData game)
        {
            try
            {
                _logger.LogDebugPatchIsRunning(nameof(PCHandler), nameof(PCHandler.SaveGame), nameof(SavePatch), nameof(Prefix));

                if (game.MapData.Floor <= 1)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(SavePatch), nameof(Prefix), ex);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
