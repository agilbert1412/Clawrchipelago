using BepInEx.Logging;
using HarmonyLib;

namespace DataExporter
{
    public class PatchInitializer
    {
        public PatchInitializer()
        {
        }

        public void InitializeAllPatches(ManualLogSource logger, Harmony harmony)
        {
            HandleInputPatch.Initialize(logger);
        }
    }
}
