using System.IO;
using BepInEx;
using HarmonyLib;

namespace DataExporter
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class DataExporterMod : BaseUnityPlugin
    {
        public static DataExporterMod Instance { get; private set; }

        public string Name => "Clawrchipelago";
        private PatchInitializer _patcherInitializer;
        private Harmony _harmony;

        private void Awake()
        {
            Instance = this;
            try
            {
                _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
                _harmony.PatchAll();
            }
            catch (FileNotFoundException fnfe)
            {
                Logger.LogError($"Cannot load {MyPluginInfo.PLUGIN_GUID}: A Necessary Dependency is missing [{fnfe.FileName}]");
                throw;
            }

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            Initialize();

            Logger.LogInfo($"Initialized!");
        }

        public void Update()
        {
        }

        private void Initialize()
        {
            _patcherInitializer = new PatchInitializer();
            _patcherInitializer.InitializeAllPatches(Logger, _harmony);
        }
    }
}