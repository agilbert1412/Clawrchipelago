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
            SetStatFloorReachedPatch.Initialize(logger, archipelago, locationChecker);
            MapEnterRoomPatch.Initialize(logger, archipelago, locationChecker);
            DungeonOnEnemyDiePatch.Initialize(logger, archipelago, locationChecker);
            GetRandomPerkRewardPatch.Initialize(logger, archipelago, locationChecker);
            AddNewPerkPatch.Initialize(logger, archipelago, locationChecker);

            GenerateStartDeckPatch.Initialize(logger, archipelago, locationChecker);
            ItemUnlockPatch.Initialize(logger, archipelago, locationChecker);
            GetRandomCombatItemRewardPatch.Initialize(logger, archipelago, locationChecker);

            PreventSaveProgressPatch.Initialize(logger, archipelago, locationChecker);
            SelectSaveSlotPatch.Initialize(logger, archipelago, locationChecker);
            DisableAchievementsPatch.Initialize(logger, archipelago, locationChecker);
        }

        private static void InitializeDebugPatches(ILogger logger)
        {
            HandleInputPatch.Initialize(logger);
        }
    }
}
