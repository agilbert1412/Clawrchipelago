using HarmonyLib;
using System;
using Clawrchipelago.Extensions;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Gameplay;
using Gameplay.Combatants;

namespace Clawrchipelago.HarmonyPatches
{
    [HarmonyPatch(typeof(Combatant))]
    [HarmonyPatch(nameof(Combatant.GetAttacked))]
    public class CombatantGetAttackedPatch
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

        // public IEnumerator GetAttacked(Combatant attacker, double amount, bool ignoreArmor, bool triggerAttackEffects, bool isCrit, System.Random rng)
        public static void Postfix(Combatant __instance, Combatant attacker, double amount, bool ignoreArmor, bool triggerAttackEffects, bool isCrit, Random rng)
        {
            try
            {
                _logger.LogDebugPatchIsRunning(nameof(Combatant), nameof(Combatant.GetAttacked), nameof(CombatantGetAttackedPatch), nameof(Postfix));

                if (!__instance.IsDead())
                {
                    return;
                }

                var data = __instance.GetEnemyData();
                if (data == null)
                {
                    return;
                }

                var enemyName = data.Setting.Name.ToEnglish();
                var difficulty = Game.Instance.GetCurrentDifficulty();
                _locationChecker.AddCheckedLocation($"Kill {enemyName} - {difficulty}");

                return;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(CombatantGetAttackedPatch), nameof(Postfix), ex);
                return;
            }
        }
    }
}
