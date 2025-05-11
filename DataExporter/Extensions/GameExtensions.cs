using System.Reflection;
using Gameplay;

namespace DataExporter.Extensions
{
    public static class GameExtensions
    {
        public static void SetIsEnemyTurn(this Game game, bool val)
        {
            // internal bool IsEnemyTurn;
            var isEnemyTurnField = typeof(Game).GetField("IsEnemyTurn", BindingFlags.NonPublic | BindingFlags.Instance);
            isEnemyTurnField.SetValue(game, val);
        }
        
        public static bool ClawMachineLoopFinished(this Game game)
        {
            // private bool _clawMachineLoopFinished;
            var clawMachineLoopFinishedField = typeof(Game).GetField("_clawMachineLoopFinished", BindingFlags.NonPublic | BindingFlags.Instance);
            var clawMachineLoopFinished = (bool)clawMachineLoopFinishedField.GetValue(game);
            return clawMachineLoopFinished;
        }
    }
}
