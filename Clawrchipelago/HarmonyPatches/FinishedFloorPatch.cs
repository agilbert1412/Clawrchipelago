using HarmonyLib;
using System;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Clawrchipelago.Archipelago;
using Clawrchipelago.Extensions;
using Gameplay.Statistics;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Gameplay;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Clawrchipelago.HarmonyPatches
{
    [HarmonyPatch(typeof(StatisticData))]
    [HarmonyPatch(nameof(StatisticData.SetStat))]
    public class FinishedFloorPatch
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

        // public void SetStat(EStatisticType type, double value)
        public static void Postfix(StatisticData __instance, EStatisticType type, double value)
        {
            try
            {
                _logger.LogDebugPatchIsRunning(nameof(StatisticData), nameof(StatisticData.SetStat), nameof(FinishedFloorPatch), nameof(Postfix));

                if (type != EStatisticType.FloorReached)
                {
                    return;
                }

                var floor = Game.Instance.Data.MapData.Floor;
                var difficulty = Game.Instance.GetCurrentDifficulty();
                _locationChecker.AddCheckedLocation($"Complete Floor {floor} - {difficulty}");

                if (floor >= 20)
                {
                    var fighterName = Game.Instance.Data.Fighter.Setting.Name.ToEnglish();
                    _locationChecker.AddCheckedLocation($"Win a game with {fighterName}");
                    CheckGoalsAfterWin(Game.Instance.Data.DifficultyLevel, floor, fighterName);
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(FinishedFloorPatch), nameof(Postfix), ex);
                return;
            }
        }

        private static void CheckGoalsAfterWin(EDifficultyLevel difficulty, int floor, string fighter)
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

        private static void RegisterFighterVictoryInDatastorage(EDifficultyLevel difficulty, EDifficultyLevel minimumDifficulty, string fighter)
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
}
