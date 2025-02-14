using HarmonyLib;
using System;
using System.Reflection;
using Clawrchipelago.Archipelago;
using Clawrchipelago.Extensions;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Gameplay;
using Gameplay.Enemies;
using Gameplay.Fighters.Settings;
using Gameplay.Items.Settings;
using Platforms;
using Gameplay.Combatants;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using UI;

namespace Clawrchipelago.HarmonyPatches
{
    [HarmonyPatch(typeof(StartGameScreen))]
    [HarmonyPatch("Init")]
    public class ItemUnlockPatch
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

        // protected override void Init()
        public static bool Prefix(StartGameScreen __instance)
        {
            try
            {
                _logger.LogDebugPatchIsRunning(nameof(StartGameScreen), "Init", nameof(ItemUnlockPatch), nameof(Prefix));

                foreach (var fighter in Runtime.Configuration.Fighters)
                {
                    // internal bool TemporaryUnlocked;
                    var temporaryUnlockedField = typeof(FighterSetting).GetField("TemporaryUnlocked", BindingFlags.NonPublic | BindingFlags.Instance);
                    temporaryUnlockedField.SetValue(fighter, false);

                    fighter.IsLocked = !_archipelago.HasReceivedItem(fighter.Name.ToEnglish());

                    var locked = fighter.IsLocked ? "locked" : "unlocked";
                    _logger.LogInfo($"Fighter {fighter.Name.ToEnglish()} is currently {locked}");
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(ItemUnlockPatch), nameof(Prefix), ex);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
