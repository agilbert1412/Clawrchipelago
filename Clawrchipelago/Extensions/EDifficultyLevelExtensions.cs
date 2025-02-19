using System;
using System.Collections.Generic;
using Gameplay;

namespace Clawrchipelago.Extensions
{
    public static class EDifficultyLevelExtensions
    {
        public static string GetCurrentDifficulty(this Game game)
        {
            return Game.Instance.Data.DifficultyLevel.GetName();
        }
        public static IEnumerable<string> GetLowerDifficulties(this Game game)
        {
            for (var i = 0; i < (int)Game.Instance.Data.DifficultyLevel; i++)
            {
                yield return ((EDifficultyLevel)i).GetName();
            }
        }

        public static string GetName(this EDifficultyLevel difficultyLevel)
        {
            var difficulty = difficultyLevel switch
            {
                EDifficultyLevel.Normal => "Normal",
                EDifficultyLevel.Hard => "Hard",
                EDifficultyLevel.VeryHard => "Very Hard",
                EDifficultyLevel.Nightmare => "Nightmare",
                _ => throw new ArgumentOutOfRangeException()
            };

            return difficulty;
        }
    }
}
