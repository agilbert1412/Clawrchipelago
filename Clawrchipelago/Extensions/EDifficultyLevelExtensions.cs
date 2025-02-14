using System;
using Gameplay;

namespace Clawrchipelago.Extensions
{
    public static class EDifficultyLevelExtensions
    {
        public static string GetCurrentDifficulty(this Game game)
        {
            return Game.Instance.Data.DifficultyLevel.GetName();
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
