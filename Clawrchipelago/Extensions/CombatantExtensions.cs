using System.Reflection;
using Gameplay.Combatants;
using Gameplay.Enemies.Data;

namespace Clawrchipelago.Extensions
{
    public static class CombatantExtensions
    {
        public static EnemyData GetEnemyData(this Combatant combatant)
        {
            return combatant.GetData() as EnemyData;
        }

        public static CombatantData GetData(this Combatant combatant)
        {
            // internal CombatantData Data;
            var dataField = typeof(Combatant).GetField("Data", BindingFlags.NonPublic | BindingFlags.Instance);
            if (dataField == null)
            {
                return null;
            }

            var data = dataField.GetValue(combatant) as CombatantData;
            return data;
        }

        public static bool IsDead(this Combatant combatant)
        {
            // internal bool IsDead;
            var isDeadProperty = typeof(Combatant).GetProperty("IsDead", BindingFlags.NonPublic | BindingFlags.Instance);
            var isDead = (bool)(isDeadProperty.GetValue(combatant));
            return isDead;
        }
    }
}
