using Clawrchipelago.Archipelago;
using Clawrchipelago.HarmonyPatches.DebugPatches;
using Clawrchipelago.HarmonyPatches.PerkPatches;
using Clawrchipelago.HarmonyPatches.SavePatches;
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
            // SetStatFloorReachedPatch.Initialize(logger, archipelago, locationChecker);
            MapEnterRoomPatch.Initialize(logger, archipelago, locationChecker);
            RunEndScreenPatch.Initialize(logger, archipelago, locationChecker);
            DungeonOnEnemyDiePatch.Initialize(logger, archipelago, locationChecker);
            GetRandomPerkRewardsPatch.Initialize(logger, archipelago, locationChecker);
            PerkDisplayPatches.Initialize(logger, archipelago, locationChecker);
            AddNewPerkPatch.Initialize(logger, archipelago, locationChecker);

            GenerateStartDeckPatch.Initialize(logger, archipelago, locationChecker);
            ItemUnlockPatch.Initialize(logger, archipelago, locationChecker);
            GetCombatItemPatches.Initialize(logger, archipelago, locationChecker);

            PreventSaveProgressPatch.Initialize(logger, archipelago, locationChecker);
            SelectSaveSlotPatch.Initialize(logger, archipelago, locationChecker);
            DisableAchievementsPatch.Initialize(logger, archipelago, locationChecker);
            DifficultyLevelPatch.Initialize(logger, archipelago, locationChecker);
            SavePatch.Initialize(logger, archipelago, locationChecker);

            InitFreshGamePatch.Initialize(logger, archipelago, locationChecker);
            RecycleUIPatches.Initialize(logger, archipelago, locationChecker);

            DeathlinkPatches.Initialize(logger, archipelago, locationChecker);
        }

        private static void InitializeDebugPatches(ILogger logger)
        {
            HandleInputPatch.Initialize(logger);
        }
    }
}
