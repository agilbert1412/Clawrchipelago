using System.Reflection;
using Gameplay;

namespace Clawrchipelago.Extensions
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
    }
}
