using HarmonyLib;
using System;
using System.Collections;
using KaitoKid.ArchipelagoUtilities.Net;
using Gameplay;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Gameplay.Items.Data;
using System.Linq;
using Clawrchipelago.Archipelago;
using Clawrchipelago.Extensions;
using Gameplay.Combatants;
using UnityEngine;
using ILogger = KaitoKid.ArchipelagoUtilities.Net.Interfaces.ILogger;

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
            EndTurnPatch.Initialize(logger, archipelago, locationChecker);
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
                EndTurnPatch.CancelTurn();
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
    [HarmonyPatch(nameof(ClawMachine.EndTurn))]
    public class EndTurnPatch
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

        // public IEnumerator EndTurn()
        public static void Postfix(ClawMachine __instance, ref IEnumerator __result)
        {
            try
            {
                if (_archipelago.SlotData.DungeonClawlerDeathLink != DeathLinkOptions.Claw)
                {
                    return;
                }

                _logger.LogDebugPatchIsRunning(nameof(ClawMachine), nameof(ClawMachine.EndTurn), nameof(EndTurnPatch), nameof(Postfix));

                if (__instance.HasCancelled())
                {
                    return;
                }

                var gotNothingInCombat = __instance.NumberOfItemsThisTurn() <= 0 && __instance.CollectedFluffThisTurn() <= 0;
                var gotNothingInDungeon = Game.Instance.Dungeon == null || Game.Instance.Dungeon.RewardItems == null || !Game.Instance.Dungeon.RewardItems.Any();
                if (gotNothingInCombat && gotNothingInDungeon)
                {
                    _archipelago.SendDeathLink($"Wasted a perfectly good claw");
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(EndTurnPatch), nameof(Postfix), ex);
                return;
            }
        }

        public static void CancelTurn()
        {
            var clawMachine = Game.Instance?.ClawMachine;
            if (clawMachine == null || clawMachine.CurrentClaw == null || clawMachine.CurrentClaw.gameObject == null)
            {
                return;
            }

            clawMachine.SetHasCancelled(true);
            clawMachine.CancelTurn();

            //while ((object)game.ClawMachine.CollectItems())
            //if (!game.EarlyExitIfFightOver())
            //{
            //    game.ClawMachine.IncreaseRoundCount();
            //    game.StartCoroutine(game.ClawMachine.DespawnClaw());
            //    if (game.ClawMachine.IsTurnOver())
            //        break;
            //}
            //else
            //    break;

            //UnityEngine.Object.Destroy(clawMachine.CurrentClaw.gameObject);
            //clawMachine.IncreaseRoundCount();
            //Game.Instance.StartCoroutine(clawMachine.GetNextClaw());
            //if (clawMachine.IsTurnOver())
            //{
            //    clawMachine.CancelTurn();
            //}
            //else
            //{
            //    clawMachine._playerActionFinished = false;
            //    clawMachine._playerInputAllowed = true;
            //}
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

                // _logger.LogDebugPatchIsRunning(nameof(Dungeon), nameof(Dungeon.HasFinishedRoom), nameof(HasFinishedRoomPatch), nameof(Postfix));

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
