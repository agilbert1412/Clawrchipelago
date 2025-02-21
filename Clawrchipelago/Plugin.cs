using BepInEx;
using Clawrchipelago.Utilities;
using Clawrchipelago.HarmonyPatches;
using HarmonyLib;
using System.IO;
using UnityEngine;
using ILogger = KaitoKid.ArchipelagoUtilities.Net.Interfaces.ILogger;
using System.Threading;
using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Helpers;
using Clawrchipelago.Archipelago;
using Clawrchipelago.Items;
using Clawrchipelago.Serialization;
using Clawrchipelago.UI;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using Newtonsoft.Json;

namespace Clawrchipelago
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class ClawrchipelagoMod : BaseUnityPlugin
    {
        public static ClawrchipelagoMod Instance { get; private set; }

        public string Name => "Clawrchipelago";
        private ILogger _logger;
        private PatchInitializer _patcherInitializer;
        private Harmony _harmony;
        private DungeonClawlerArchipelagoClient _archipelago;
        private ArchipelagoConnectionInfo APConnectionInfo { get; set; }
        private DungeonClawlerLocationChecker _locationChecker;
        private ClawrchipelagoItemManager _itemManager;
        private TrapExecutor _trapExecutor;
        public ClawrchipelagoConfig Config { get; private set; }
        public PersistentData PersistentData { get; private set; }


        public RecentItemsAndLocations _recentItemsAndLocations;

        private void Awake()
        {
            Instance = this;
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

            Config = ClawrchipelagoConfig.LoadConfig();
            PersistentData = PersistentData.LoadData();
            InitializeBeforeConnection();
            ConnectToArchipelago();
            InitializeAfterConnection();

            _logger.LogInfo($"Connected to Archipelago!");
        }

        public void Update()
        {
            _recentItemsAndLocations?.Update();
            _trapExecutor?.Update();
        }

        private void InitializeBeforeConnection()
        {
            _patcherInitializer = new PatchInitializer();
            _archipelago = new DungeonClawlerArchipelagoClient(_logger, OnItemReceived);
            _recentItemsAndLocations = new RecentItemsAndLocations(_logger, _archipelago);
            _trapExecutor = new TrapExecutor(_logger, _archipelago);
            _itemManager = new ClawrchipelagoItemManager(_logger, _archipelago, _trapExecutor, PersistentData.ItemsParsed, _recentItemsAndLocations);
        }

        private void InitializeAfterConnection()
        {
            _locationChecker = new DungeonClawlerLocationChecker(_logger, _archipelago, new List<string>(), _recentItemsAndLocations);
            _locationChecker.VerifyNewLocationChecksWithArchipelago();
            _locationChecker.SendAllLocationChecks();
            _patcherInitializer.InitializeAllPatches(_logger, _harmony, _archipelago, _locationChecker);
            _recentItemsAndLocations.UpdateItems();
            _recentItemsAndLocations.UpdateLocations(_locationChecker.LocationsInOrder);
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
                _itemManager.ReceiveAllNewItems();
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(ex);
            }
            finally
            {
                PersistentData.ItemsParsed = _itemManager.GetAllItemsAlreadyProcessed();
                PersistentData.SaveData(PersistentData, _archipelago.SlotData.Seed);
            }
        }
    }
}