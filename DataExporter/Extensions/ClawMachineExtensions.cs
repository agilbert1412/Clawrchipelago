using System.Collections;
using System.Reflection;
using Gameplay;

namespace DataExporter.Extensions
{
    public static class ClawMachineExtensions
    {
        public static int NumberOfItemsThisTurn(this ClawMachine clawMachine)
        {
            // internal int NumberOfItemsThisTurn;
            var numberOfItemsThisTurnField = typeof(ClawMachine).GetField("NumberOfItemsThisTurn", BindingFlags.NonPublic | BindingFlags.Instance);
            var numberOfItemsThisTurn = (int)(numberOfItemsThisTurnField.GetValue(clawMachine));
            return numberOfItemsThisTurn;
        }

        public static int CollectedFluffThisTurn(this ClawMachine clawMachine)
        {
            // internal int CollectedFluffThisTurn;
            var collectedFluffThisTurnField = typeof(ClawMachine).GetField("CollectedFluffThisTurn", BindingFlags.NonPublic | BindingFlags.Instance);
            var collectedFluffThisTurn = (int)(collectedFluffThisTurnField.GetValue(clawMachine));
            return collectedFluffThisTurn;
        }

        public static IEnumerator GetNextClaw(this ClawMachine clawMachine)
        {
            // private IEnumerator GetNextClaw()
            var GetNextClawMethod = typeof(ClawMachine).GetMethod("GetNextClaw", BindingFlags.NonPublic | BindingFlags.Instance);
            var getNextClawReturn = (IEnumerator)(GetNextClawMethod.Invoke(clawMachine, []));
            return getNextClawReturn;
        }

        public static bool PlayerActionFinished(this ClawMachine clawMachine)
        {
            // private bool _playerActionFinished;
            var playerActionFinishedField = typeof(ClawMachine).GetField("_playerActionFinished", BindingFlags.NonPublic | BindingFlags.Instance);
            var playerActionFinished = (bool)(playerActionFinishedField.GetValue(clawMachine));
            return playerActionFinished;
        }

        public static bool HasCancelled(this ClawMachine clawMachine)
        {
            // private bool _hasCancelled;
            var hasCancelledField = clawMachine.GetHasCancelledField();
            var hasCancelled = (bool)(hasCancelledField.GetValue(clawMachine));
            return hasCancelled;
        }

        public static void SetHasCancelled(this ClawMachine clawMachine, bool val)
        {
            // private bool _hasCancelled;
            var hasCancelledField = clawMachine.GetHasCancelledField();
            hasCancelledField.SetValue(clawMachine, val);
        }

        public static FieldInfo GetHasCancelledField(this ClawMachine clawMachine)
        {
            // private bool _hasCancelled;
            var hasCancelledField = typeof(ClawMachine).GetField("_hasCancelled", BindingFlags.NonPublic | BindingFlags.Instance);
            return hasCancelledField;
        }
    }
}
