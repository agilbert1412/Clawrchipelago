using BepInEx;
using Clawrchipelago.Utilities;
using Clawrchipelago.HarmonyPatches;
using HarmonyLib;
using System.IO;
using KaitoKid.ArchipelagoUtilities.Net;
using UnityEngine;
using ILogger = KaitoKid.ArchipelagoUtilities.Net.Interfaces.ILogger;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Helpers;
using Clawrchipelago.Archipelago;
using Clawrchipelago.Extensions;
using Clawrchipelago.Serialization;
using Gameplay;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using Newtonsoft.Json;
using Gameplay.Items.Settings;
using Platforms;
using Gameplay.Items.Data;
using TMPro;
using ItemManager = Clawrchipelago.Items.ItemManager;

namespace Clawrchipelago
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class ClawrchipelagoMod : BaseUnityPlugin
    {
        public string Name => "Clawrchipelago";
        private ILogger _logger;
        private PatchInitializer _patcherInitializer;
        private Harmony _harmony;
        private DungeonClawlerArchipelagoClient _archipelago;
        private ArchipelagoConnectionInfo APConnectionInfo { get; set; }
        private LocationChecker _locationChecker;
        private ItemManager _itemManager;


        public TextMeshProUGUI RecentItemsLabel;

        private void Awake()
        {
            try
            {
                _logger = new LogHandler(Logger);
                _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
                _harmony.PatchAll();
            }
            catch (FileNotFoundException fnfe)
            {
                _logger.LogError(
                    $"Cannot load {MyPluginInfo.PLUGIN_GUID}: A Necessary Dependency is missing [{fnfe.FileName}]");
                throw;
            }

            _logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            InitializeBeforeConnection();
            ConnectToArchipelago();
            InitializeAfterConnection();

            _logger.LogInfo($"Connected to Archipelago!");
        }

        public void Update()
        {
            UpdateRecentItemsLabel();
        }

        private void InitializeBeforeConnection()
        {
            _patcherInitializer = new PatchInitializer();
            _archipelago = new DungeonClawlerArchipelagoClient(_logger, OnItemReceived);
        }

        private void InitializeAfterConnection()
        {
            _locationChecker = new LocationChecker(_logger, _archipelago, new List<string>());
            _locationChecker.VerifyNewLocationChecksWithArchipelago();
            _locationChecker.SendAllLocationChecks();
            _patcherInitializer.InitializeAllPatches(_logger, _harmony, _archipelago, _locationChecker);
            _itemManager = new ItemManager(_archipelago);
        }

        private void ConnectToArchipelago()
        {
            ReadPersistentArchipelagoData();

            var errorMessage = "";
            if (APConnectionInfo != null && !_archipelago.IsConnected)
            {
                _archipelago.ConnectToMultiworld(APConnectionInfo, out errorMessage);
            }

            if (!_archipelago.IsConnected)
            {
                APConnectionInfo = null;
                var userMessage =
                    $"Could not connect to archipelago.{Environment.NewLine}Message: {errorMessage}{Environment.NewLine}Please verify the connection file ({Persistency.CONNECTION_FILE}) and that the server is available.{Environment.NewLine}";
                Logger.LogError(userMessage);
                const int timeUntilClose = 10;
                Logger.LogError($"The Game will close in {timeUntilClose} seconds");
                Thread.Sleep(timeUntilClose * 1000);
                Application.Quit();
                return;
            }

            Logger.LogMessage($"Connected to Archipelago as {_archipelago.SlotData.SlotName}.");
            WritePersistentArchipelagoData();
        }

        private void ReadPersistentArchipelagoData()
        {
            if (!File.Exists(Persistency.CONNECTION_FILE))
            {
                var defaultConnectionInfo = new ArchipelagoConnectionInfo("archipelago.gg", 38281, "Name", false);
                WritePersistentData(defaultConnectionInfo, Persistency.CONNECTION_FILE);
            }

            var jsonString = File.ReadAllText(Persistency.CONNECTION_FILE);
            var connectionInfo = JsonConvert.DeserializeObject<ArchipelagoConnectionInfo>(jsonString);
            if (connectionInfo == null)
            {
                return;
            }

            APConnectionInfo = connectionInfo;
        }

        private void WritePersistentArchipelagoData()
        {
            WritePersistentData(APConnectionInfo, Persistency.CONNECTION_FILE);
        }

        private void WritePersistentData(object data, string path)
        {
            var jsonObject = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, jsonObject);
        }

        private void OnItemReceived(ReceivedItemsHelper receivedItemHelper)
        {
            try
            {
                _itemManager.OnItemReceived();
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(ex);
            }
        }

        public void UpdateRecentItemsLabel()
        {
            if (RecentItemsLabel == null)
            {
                if (Game.Instance?.FloorLabel == null)
                {
                    return;
                }

                InstanciateRecentItemsLabel();
            }

            Logger.LogInfo($"Game.Instance.FloorLabel.text: {Game.Instance.FloorLabel.text}");
            Logger.LogInfo($"RecentItemsLabel.text: {RecentItemsLabel.text}");
            // Game.Instance.FloorLabel.text = "I am floor label";
            RecentItemsLabel.text = "I am Recent Items Label";
            Logger.LogInfo($"Game.Instance.FloorLabel.text: {Game.Instance.FloorLabel.text}");
            Logger.LogInfo($"RecentItemsLabel.text: {RecentItemsLabel.text}");
        }

        private void InstanciateRecentItemsLabel()
        {
            Logger.LogInfo($"Instanciating RecentItemsLabel");
            RecentItemsLabel = Instantiate(Game.Instance.FloorLabel, Game.Instance.FloorLabel.transform.parent, true);

            // RecentItemsLabel.PixelAdjustPoint(Vector2.down);
        }
    }
}