using System.Collections.Generic;
using System.Reflection;
using Gameplay;
using Gameplay.Enemies;
using Gameplay.Fighters;

namespace Clawrchipelago.Extensions
{
    public static class DungeonExtensions
    {
        public static Fighter Fighter(this Dungeon dungeon)
        {
            // internal Fighter Fighter;
            var fighterField = typeof(Dungeon).GetField("Fighter", BindingFlags.NonPublic | BindingFlags.Instance);
            var fighter = (Fighter)(fighterField.GetValue(dungeon));
            return fighter;
        }
        public static List<Enemy> Enemies(this Dungeon dungeon)
        {
            // internal List<Enemy> Enemies;
            var enemiesField = typeof(Dungeon).GetField("Fighter", BindingFlags.NonPublic | BindingFlags.Instance);
            var enemies = (List<Enemy>)(enemiesField.GetValue(dungeon));
            return enemies;
        }
    }
}
