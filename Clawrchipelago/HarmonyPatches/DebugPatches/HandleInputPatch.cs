using System;
using Clawrchipelago.DataExport;
using Gameplay;
using HarmonyLib;
using Rewired;
using UnityEngine;
using ILogger = KaitoKid.ArchipelagoUtilities.Net.Interfaces.ILogger;

namespace Clawrchipelago.HarmonyPatches.DebugPatches
{
    // public void Update()
    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch(nameof(Game.Update))]
    public static class HandleInputPatch
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        // private void HandleInput()
        public static bool Prefix(PlayerController __instance)
        {
            try
            {
                // _logger.LogDebugPatchIsRunning(nameof(PlayerController), "HandleInput", nameof(HandleInputPatch), nameof(Prefix));
                
                if (Input.GetKeyDown(KeyCode.E))
                {
                    _logger.LogInfo($"Exporting All Data...");
                    var dataExporter = new DataExporter();
                    dataExporter.ExportAll();
                    _logger.LogInfo($"Exported All Data");
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(HandleInputPatch), nameof(Prefix), ex);
                return true; // run original logic
            }
        }
    }
}