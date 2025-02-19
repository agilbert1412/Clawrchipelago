using HarmonyLib;
using System;
using Clawrchipelago.Archipelago;
using Clawrchipelago.Extensions;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Gameplay;
using Gameplay.Enemies;

namespace Clawrchipelago.HarmonyPatches
{
    [HarmonyPatch(typeof(Dungeon))]
    [HarmonyPatch(nameof(Dungeon.OnEnemyDied))]
    public class DungeonOnEnemyDiePatch
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

        // public void OnEnemyDied(Enemy enemy)
        public static void Postfix(Dungeon __instance, Enemy enemy)
        {
            try
            {
                if (!_archipelago.SlotData.Enemysanity)
                {
                    return;
                }

                _logger.LogDebugPatchIsRunning(nameof(Dungeon), nameof(Dungeon.OnEnemyDied), nameof(DungeonOnEnemyDiePatch), nameof(Postfix));

                var data = enemy.GetEnemyData();

                if (data == null)
                {
                    _logger.LogDebug($"{nameof(DungeonOnEnemyDiePatch)}: Could not get Enemy Data");
                    return;
                }

                var enemyName = data.Setting.Name.ToEnglish();
                _logger.LogDebug($"{nameof(DungeonOnEnemyDiePatch)}: {enemyName} is dead!");
                var difficulty = Game.Instance.GetCurrentDifficulty();
                _locationChecker.AddCheckedLocation($"Kill {enemyName} - {difficulty}");
                if (ClawrchipelagoMod.Instance.Config.CheckLowerDifficultyLocations)
                {
                    foreach (var lowerDifficulty in Game.Instance.GetLowerDifficulties())
                    {
                        _locationChecker.AddCheckedLocation($"Kill {enemyName} - {lowerDifficulty}");
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(DungeonOnEnemyDiePatch), nameof(Postfix), ex);
                return;
            }
        }
    }
}
