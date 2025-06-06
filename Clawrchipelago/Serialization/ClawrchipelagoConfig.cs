﻿using Newtonsoft.Json;
using System.IO;

namespace Clawrchipelago.Serialization
{
    public class ClawrchipelagoConfig
    {
        private const string CONFIG_PATH = "ClawrchipelagoConfig.json";

        public int StartingMoneyAmount { get; set; } = 5;
        public int StartingCombatInventorySize { get; set; } = 2;
        public int StartingPerkInventorySize { get; set; } = 1;
        public bool ShowRecentItems { get; set; } = true;
        public bool ShowRecentLocations { get; set; } = false;
        public bool CheckLowerDifficultyLocations { get; set; } = false;
        public bool AllowStackingPerksWhenChecksAreDone { get; set; } = false;

        public ClawrchipelagoConfig() { }

        public static void SaveConfig(ClawrchipelagoConfig config)
        {
            var jsonObject = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(CONFIG_PATH, jsonObject);
        }

        public static ClawrchipelagoConfig LoadConfig()
        {
            if (!File.Exists(CONFIG_PATH))
            {
                var defaultConfig = new ClawrchipelagoConfig();
                SaveConfig(defaultConfig);
                return defaultConfig;
            }

            var jsonObject = File.ReadAllText(CONFIG_PATH);
            var config = JsonConvert.DeserializeObject<ClawrchipelagoConfig>(jsonObject);
            SaveConfig(config);
            return config;
        }
    }
}
