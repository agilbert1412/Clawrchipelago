using System.Reflection;
using Gameplay.Rooms;

namespace Clawrchipelago.Extensions
{
    public static class MapExtensions
    {
        public static bool FloorChange(this Map map)
        {
            // internal bool FloorChange;
            var floorChangeField = typeof(Map).GetField("FloorChange", BindingFlags.NonPublic | BindingFlags.Instance);
            var floorChange = (bool)(floorChangeField.GetValue(map));
            return floorChange;
        }
    }
}
