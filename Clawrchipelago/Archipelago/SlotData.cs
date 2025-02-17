using System;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace Clawrchipelago.Archipelago
{
    public class SlotData : ISlotData
    {
        private const string GOAL_KEY = "goal";
        private const string SHUFFLE_FIGHTERS_KEY = "shuffle_fighters";
        private const string SHUFFLE_ITEMS_KEY = "shuffle_combat_items";
        private const string SHUFFLE_PERKS_KEY = "shuffle_perks";
        private const string ENEMYSANITY_KEY = "enemysanity";
        private const string TRAP_DIFFICULTY_KEY = "trap_difficulty";
        private const string DEATH_LINK_KEY = "death_link";
        private const string SEED_KEY = "seed";
        private const string MULTIWORLD_VERSION_KEY = "multiworld_version";

        private Dictionary<string, object> _slotDataFields;
        private ILogger _logger;

        public string SlotName { get; private set; }
        public Goal Goal { get; private set; }
        public ShuffleFighters ShuffleFighters { get; private set; }
        public bool ShuffleItems { get; private set; }
        public bool ShufflePerks { get; private set; }
        public bool Enemysanity { get; private set; }
        public TrapDifficulty TrapDifficulty { get; private set; }
        public bool DeathLink => DungeonClawlerDeathLink != DeathLinkOptions.Disabled;
        public DeathLinkOptions DungeonClawlerDeathLink { get; private set; }
        public int Seed { get; private set; }
        public string MultiworldVersion { get; private set; }

        public SlotData(string slotName, Dictionary<string, object> slotDataFields, ILogger logger)
        {
            SlotName = slotName;
            _slotDataFields = slotDataFields;
            _logger = logger;

            Goal = GetSlotSetting(GOAL_KEY, Goal.BeatNightmare);
            ShuffleFighters = GetSlotSetting(SHUFFLE_FIGHTERS_KEY, ShuffleFighters.FightersAndPaws);
            ShuffleItems = GetSlotSetting(SHUFFLE_ITEMS_KEY, true);
            ShufflePerks = GetSlotSetting(SHUFFLE_PERKS_KEY, true);
            Enemysanity = GetSlotSetting(ENEMYSANITY_KEY, true);
            TrapDifficulty = GetSlotSetting(TRAP_DIFFICULTY_KEY, TrapDifficulty.Medium);
            DungeonClawlerDeathLink = GetSlotSetting(DEATH_LINK_KEY, DeathLinkOptions.Disabled);
            Seed = GetSlotSetting(SEED_KEY, 0);
            MultiworldVersion = GetSlotSetting(MULTIWORLD_VERSION_KEY, "");
        }

        private int GetSlotSetting(IEnumerable<string> keys, int defaultValue)
        {
            foreach (var key in keys)
            {
                var value = GetSlotSetting(key, defaultValue);
                if (value != defaultValue)
                {
                    return value;
                }
            }

            return defaultValue;
        }

        private T GetSlotSetting<T>(string key, T defaultValue) where T : struct, Enum, IConvertible
        {
            return _slotDataFields.ContainsKey(key) ? (T)Enum.Parse(typeof(T), _slotDataFields[key].ToString(), true) : GetSlotDefaultValue(key, defaultValue);
        }

        private string GetSlotSetting(string key, string defaultValue)
        {
            return _slotDataFields.ContainsKey(key) ? _slotDataFields[key].ToString() : GetSlotDefaultValue(key, defaultValue);
        }

        private int GetSlotSetting(string key, int defaultValue)
        {
            return _slotDataFields.ContainsKey(key) ? (int)(long)_slotDataFields[key] : GetSlotDefaultValue(key, defaultValue);
        }

        private bool GetSlotSetting(string key, bool defaultValue)
        {
            if (_slotDataFields.ContainsKey(key) && _slotDataFields[key] != null)
            {
                if (_slotDataFields[key] is bool boolValue)
                {
                    return boolValue;
                }

                if (_slotDataFields[key] is long longValue)
                {
                    return longValue != 0;
                }

                if (_slotDataFields[key] is int intValue)
                {
                    return intValue != 0;
                }
            }

            return GetSlotDefaultValue(key, defaultValue);
        }

        private T GetSlotDefaultValue<T>(string key, T defaultValue)
        {
            _logger.LogWarning($"SlotData did not contain expected key: \"{key}\"");
            return defaultValue;
        }
    }

    public enum Goal
    {
        BeatNormal = 0,
        BeatHard = 1,
        BeatVery_hard = 2,
        BeatNightmare = 3,
        BeatNormalWithAllCharacters = 4,
        BeatHardWithAllCharacters = 5,
        BeatVery_hardWithAllCharacters = 6,
        BeatNightmareWithAllCharacters = 7,
        BeatFloor25 = 8,
        BeatFloor30 = 9,
        BeatFloor35 = 10,
        BeatFloor40 = 11,
        BeatFloor45 = 12,
        BeatFloor50 = 13,
    }

    public enum ShuffleFighters
    {
        None = 0,
        Fighters = 1,
        FightersAndPaws = 2,
    }

    public enum TrapDifficulty
    {
        NoTraps = 0,
        Easy = 1,
        Medium = 2,
        Hard = 3,
        Hell = 4,
        Nightmare = 5,
    }

    public enum DeathLinkOptions
    {
        Disabled = 0,
        Claw = 1,
        Death = 2,
    }
}
