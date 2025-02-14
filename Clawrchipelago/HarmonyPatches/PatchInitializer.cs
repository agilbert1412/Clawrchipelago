using Clawrchipelago.Archipelago;
using Clawrchipelago.HarmonyPatches.DebugPatches;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace Clawrchipelago.HarmonyPatches
{
    public class PatchInitializer
    {
        public PatchInitializer()
        {
        }

        public void InitializeAllPatches(ILogger logger, Harmony harmony, DungeonClawlerArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            InitializeDebugPatches(logger);
            FinishedFloorPatch.Initialize(logger, archipelago, locationChecker);
            CombatantGetAttackedPatch.Initialize(logger, archipelago, locationChecker);
            GetRandomPerkRewardPatch.Initialize(logger, archipelago, locationChecker);
            AddNewPerkPatch.Initialize(logger, archipelago, locationChecker);
        }

        private static void InitializeDebugPatches(ILogger logger)
        {
            HandleInputPatch.Initialize(logger);
        }
    }
}
