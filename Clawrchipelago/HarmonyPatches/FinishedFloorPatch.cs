using HarmonyLib;
using System;
using System.Collections;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Clawrchipelago.Archipelago;
using Clawrchipelago.Extensions;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Gameplay;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Rooms;
using Gameplay.Rooms.Data;
using UI;
using Gameplay.Fighters.Data;
using KaitoKid.ArchipelagoUtilities.Net.Constants;

namespace Clawrchipelago.HarmonyPatches
{
    public class FinishedFloorUtilities
    {
        private ILogger _logger;
        private DungeonClawlerArchipelagoClient _archipelago;
        private LocationChecker _locationChecker;

        public FinishedFloorUtilities(ILogger logger, DungeonClawlerArchipelagoClient archipelago,
            LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public void FinishedFloor(int floor)
        {
            var difficulty = Game.Instance.GetCurrentDifficulty();
            _locationChecker.AddCheckedLocation($"Complete Floor {floor} - {difficulty}");
            if (ClawrchipelagoMod.Instance.Config.CheckLowerDifficultyLocations)
            {
                foreach (var lowerDifficulty in Game.Instance.GetLowerDifficulties())
                {
                    _locationChecker.AddCheckedLocation($"Complete Floor {floor} - {lowerDifficulty}");
                }
            }

            if (floor >= 20)
            {
                var fighterName = Game.Instance.Data.Fighter.Setting.Name.ToEnglish();
                _locationChecker.AddCheckedLocation($"Win a game with {fighterName}");
                CheckGoalsAfterWin(Game.Instance.Data.DifficultyLevel, floor, fighterName);
            }
        }

        private void CheckGoalsAfterWin(EDifficultyLevel difficulty, int floor, string fighter)
        {
            switch (_archipelago.SlotData.Goal)
            {
                case Goal.BeatNormal:
                    if (difficulty >= EDifficultyLevel.Normal)
                    {
                        _archipelago.ReportGoalCompletion();
                    }

                    break;
                case Goal.BeatHard:
                    if (difficulty >= EDifficultyLevel.Hard)
                    {
                        _archipelago.ReportGoalCompletion();
                    }

                    break;
                case Goal.BeatVery_hard:
                    if (difficulty >= EDifficultyLevel.VeryHard)
                    {
                        _archipelago.ReportGoalCompletion();
                    }

                    break;
                case Goal.BeatNightmare:
                    if (difficulty >= EDifficultyLevel.Nightmare)
                    {
                        _archipelago.ReportGoalCompletion();
                    }

                    break;
                case Goal.BeatNormalWithAllCharacters:
                    RegisterFighterVictoryInDatastorage(difficulty, EDifficultyLevel.Normal, fighter);
                    break;
                case Goal.BeatHardWithAllCharacters:
                    RegisterFighterVictoryInDatastorage(difficulty, EDifficultyLevel.Hard, fighter);
                    break;
                case Goal.BeatVery_hardWithAllCharacters:
                    RegisterFighterVictoryInDatastorage(difficulty, EDifficultyLevel.VeryHard, fighter);
                    break;
                case Goal.BeatNightmareWithAllCharacters:
                    RegisterFighterVictoryInDatastorage(difficulty, EDifficultyLevel.Nightmare, fighter);
                    break;
                case Goal.BeatFloor25:
                    if (floor >= 25)
                    {
                        _archipelago.ReportGoalCompletion();
                    }

                    break;
                case Goal.BeatFloor30:
                    if (floor >= 30)
                    {
                        _archipelago.ReportGoalCompletion();
                    }

                    break;
                case Goal.BeatFloor35:
                    if (floor >= 35)
                    {
                        _archipelago.ReportGoalCompletion();
                    }

                    break;
                case Goal.BeatFloor40:
                    if (floor >= 40)
                    {
                        _archipelago.ReportGoalCompletion();
                    }

                    break;
                case Goal.BeatFloor45:
                    if (floor >= 45)
                    {
                        _archipelago.ReportGoalCompletion();
                    }

                    break;
                case Goal.BeatFloor50:
                    if (floor >= 50)
                    {
                        _archipelago.ReportGoalCompletion();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RegisterFighterVictoryInDatastorage(EDifficultyLevel difficulty,
            EDifficultyLevel minimumDifficulty, string fighter)
        {
            var session = _archipelago.GetSession();
            const string key = "FighterVictories";

            if (difficulty >= minimumDifficulty)
            {
                session.DataStorage[Scope.Slot, key].Initialize(JToken.FromObject(new Dictionary<string, bool>()));
                var victory = new Dictionary<string, bool> { { fighter, true } };
                session.DataStorage[Scope.Slot, key] += Operation.Update(victory);
            }

            var victories = session.DataStorage[Scope.Slot, key].To<Dictionary<string, bool>>();
            if (victories.Count(x => x.Value) >= 13)
            {
                _archipelago.ReportGoalCompletion();
            }
        }
    }

    [HarmonyPatch(typeof(Map))]
    [HarmonyPatch("EnterRoom")]
    public class MapEnterRoomPatch
    {
        private static ILogger _logger;
        private static DungeonClawlerArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static FinishedFloorUtilities _finishedFloorUtilities;

        public static void Initialize(ILogger logger, DungeonClawlerArchipelagoClient archipelago,
            LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _finishedFloorUtilities = new FinishedFloorUtilities(logger, archipelago, locationChecker);
        }

        // private void EnterRoom(MapTileData tile)
        public static void Postfix(Map __instance, MapTileData tile)
        {
            try
            {
                _logger.LogDebugPatchIsRunning(nameof(Map), "EnterRoom",
                    nameof(MapEnterRoomPatch), nameof(Postfix));

                var floor = Game.Instance.Data.MapData.Floor;

                if (__instance.FloorChange())
                {
                    _logger.LogInfo($"Detected Finished floor {floor}");
                    _finishedFloorUtilities.FinishedFloor(floor);
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(MapEnterRoomPatch), nameof(Postfix), ex);
                return;
            }
        }
    }

    [HarmonyPatch(typeof(RunEndScreen))]
    [HarmonyPatch(nameof(RunEndScreen.ShowAndWait))]
    public class RunEndScreenPatch
    {
        private static ILogger _logger;
        private static DungeonClawlerArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static FinishedFloorUtilities _finishedFloorUtilities;

        public static void Initialize(ILogger logger, DungeonClawlerArchipelagoClient archipelago,
            LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _finishedFloorUtilities = new FinishedFloorUtilities(logger, archipelago, locationChecker);
        }

        // public IEnumerator ShowAndWait(FighterData fighter)
        public static bool Prefix(RunEndScreen __instance, FighterData fighter, ref IEnumerator __result)
        {
            try
            {
                _logger.LogDebugPatchIsRunning(nameof(RunEndScreen), nameof(RunEndScreen.ShowAndWait),
                    nameof(RunEndScreenPatch), nameof(Prefix));

                _finishedFloorUtilities.FinishedFloor(20);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(RunEndScreenPatch), nameof(Prefix), ex);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
