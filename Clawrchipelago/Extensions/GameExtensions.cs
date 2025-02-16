using System.Reflection;
using Gameplay;
using Gameplay.Rooms;

namespace Clawrchipelago.Extensions
{
    public static class GameExtensions
    {
        public static void SetIsEnemyTurn(this Game game, bool val)
        {
            // internal bool IsEnemyTurn;
            var isEnemyTurnField = typeof(Game).GetField("IsEnemyTurn", BindingFlags.NonPublic | BindingFlags.Instance);
            isEnemyTurnField.SetValue(game, val);
        }
    }
}
