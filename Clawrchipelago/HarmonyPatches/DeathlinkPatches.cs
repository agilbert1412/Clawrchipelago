using HarmonyLib;
using System;
using System.Collections;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Gameplay;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Gameplay.Items.Data;
using System.Linq;
using Clawrchipelago.Archipelago;
using Clawrchipelago.Extensions;
using Gameplay.Combatants;

namespace Clawrchipelago.HarmonyPatches
{
    public static class DeathlinkPatches
    {
        private static ILogger _logger;
        private static DungeonClawlerArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, DungeonClawlerArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            DespawnClawPatch.Initialize(logger, archipelago, locationChecker);
            HasFinishedRoomPatch.Initialize(logger, archipelago, locationChecker);
        }

        public static void ReceiveDeathink()
        {
            if (_archipelago.SlotData.DungeonClawlerDeathLink == DeathLinkOptions.Disabled)
            {
                return;
            }

            if (_archipelago.SlotData.DungeonClawlerDeathLink == DeathLinkOptions.Claw)
            {
                DespawnClawPatch.DespawnCurrentClaw();
                return;
            }

            if (_archipelago.SlotData.DungeonClawlerDeathLink == DeathLinkOptions.Death)
            {
                HasFinishedRoomPatch.IgnoreNextDeath = true;
                HasFinishedRoomPatch.KillFighter();
                return;
            }
        }
    }

    [HarmonyPatch(typeof(ClawMachine))]
    [HarmonyPatch(nameof(ClawMachine.DespawnClaw))]
    public class DespawnClawPatch
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

        // public IEnumerator DespawnClaw()
        public static void Postfix(ClawMachine __instance, ref IEnumerator __result)
        {
            try
            {
                if (_archipelago.SlotData.DungeonClawlerDeathLink != DeathLinkOptions.Claw)
                {
                    return;
                }

                _logger.LogDebugPatchIsRunning(nameof(ClawMachine), nameof(ClawMachine.DespawnClaw), nameof(DespawnClawPatch), nameof(Postfix));

                if (__instance.NumberOfItemsThisTurn() <= 0 && __instance.CollectedFluffThisTurn() <= 0 && __instance.CurrentItems.Any())
                {
                    _archipelago.SendDeathLink($"Wasted a perfectly good claw");
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(DespawnClawPatch), nameof(Postfix), ex);
                return;
            }
        }

        public static void DespawnCurrentClaw()
        {
            var clawMachine = Game.Instance?.ClawMachine;
            if (clawMachine == null || clawMachine.CurrentClaw == null)
            {
                return;
            }

            clawMachine.CurrentClaw.Despawn();
        }
    }

    [HarmonyPatch(typeof(Dungeon))]
    [HarmonyPatch(nameof(Dungeon.HasFinishedRoom))]
    public class HasFinishedRoomPatch
    {
        private static ILogger _logger;
        private static DungeonClawlerArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        public static bool IgnoreNextDeath;

        public static void Initialize(ILogger logger, DungeonClawlerArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            IgnoreNextDeath = false;
        }

        // public bool HasFinishedRoom()
        public static void Postfix(Dungeon __instance, ref bool __result)
        {
            try
            {
                if (_archipelago.SlotData.DungeonClawlerDeathLink == DeathLinkOptions.Disabled)
                {
                    return;
                }

                _logger.LogDebugPatchIsRunning(nameof(Dungeon), nameof(Dungeon.HasFinishedRoom), nameof(HasFinishedRoomPatch), nameof(Postfix));

                if (IgnoreNextDeath)
                {
                    IgnoreNextDeath = false;
                    return;
                }

                if (!__result || (__instance.Fighter().GetData().Health >= 1.0 && !__instance.Fighter().GetData().HasDied))
                {
                    return;
                }

                var enemy = __instance.Enemies().FirstOrDefault(x => !x.GetData().HasDied && x.GetData().Health >= 1);
                if (enemy != null)
                {
                    _archipelago.SendDeathLink($"Was killed by {enemy}");
                }
                else
                {
                    _archipelago.SendDeathLink($"Died on their own, somehow...");
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(HasFinishedRoomPatch), nameof(Postfix), ex);
                return;
            }
        }

        public static void KillFighter()
        {
            var fighter = Game.Instance?.Data.Fighter;
            if (fighter == null)
            {
                return;
            }

            fighter.Health = 0.0;
            fighter.HasDied = true;
        }
    }
}
