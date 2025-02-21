using HarmonyLib;
using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Clawrchipelago.Archipelago;
using UI;
using Platforms;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gameplay;
using UnityEngine.UI;
using Utils;

namespace Clawrchipelago.HarmonyPatches.SavePatches
{
    [HarmonyPatch(typeof(SelectSaveSlotScreen))]
    [HarmonyPatch("Init")]
    public class SelectSaveSlotPatch
    {
        private static ILogger _logger;
        private static DungeonClawlerArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, DungeonClawlerArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // protected override void Init()
        public static bool Prefix(SelectSaveSlotScreen __instance)
        {
            try
            {
                CustomInit(__instance);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(SelectSaveSlotPatch), nameof(Prefix), ex);
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool CustomInit(SelectSaveSlotScreen saveSlotScreen)
        {
            // private List<SaveSlotDisplay> _saveSlots = new List<SaveSlotDisplay>();
            var saveSlotsField = typeof(SelectSaveSlotScreen).GetField("_saveSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            var saveSlots = (List<SaveSlotDisplay>)saveSlotsField.GetValue(saveSlotScreen);

            saveSlotScreen.SlotContainer.DestroyAllChildren();
            saveSlots = new List<SaveSlotDisplay>();
            for (var index = 0; index < 3; ++index)
            {
                // var num = index + 1;
                var num = _archipelago.SlotData.Seed + index;
                if (!Runtime.PlatformHandler.HasSave(num))
                {
                    var saveSlotDisplay = UnityEngine.Object.Instantiate(saveSlotScreen.SlotPrefab, saveSlotScreen.SlotContainer);
                    saveSlotDisplay.Init(saveSlotScreen, num, null);
                    saveSlots.Add(saveSlotDisplay);
                }
                else
                {
                    var saveSlotDisplay = UnityEngine.Object.Instantiate(saveSlotScreen.SlotPrefab, saveSlotScreen.SlotContainer);
                    var gameData = new GameData();
                    try
                    {
                        Runtime.PlatformHandler.LoadGame(num, gameData);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogErrorException(ex);
                        Runtime.PlatformHandler.DeleteSave(num);
                        gameData = null;
                    }
                    saveSlotDisplay.Init(saveSlotScreen, num, gameData);
                    saveSlots.Add(saveSlotDisplay);
                }
            }
            for (var index = 0; index < saveSlots.Count; ++index)
            {
                var saveSlot = saveSlots[index];
                var navigation = saveSlot.navigation with
                {
                    mode = Navigation.Mode.Explicit
                };
                if (index < saveSlots.Count - 1)
                {
                    navigation.selectOnRight = saveSlots[index + 1];
                }

                if (index > 0)
                {
                    navigation.selectOnLeft = saveSlots[index - 1];
                }

                saveSlot.navigation = navigation;
            }

            // private bool _shouldSelect;
            var shouldSelectField = typeof(SelectSaveSlotScreen).GetField("_shouldSelect", BindingFlags.NonPublic | BindingFlags.Instance);
            var shouldSelect = (bool)shouldSelectField.GetValue(saveSlotScreen);

            if (!shouldSelect)
            {
                return true;
            }

            var saveSlotDisplay1 = saveSlots.FirstOrDefault();
            if (!(saveSlotDisplay1 != null))
            {
                return true;
            }

            saveSlotDisplay1.Select();
            return false;
        }
    }
}
