﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Platforms;
using System.IO;
using System.Linq;
using Gameplay.Enemies.Settings;
using Gameplay.Items.Settings;

namespace Clawrchipelago.DataExport
{
    public class DataExporter
    {
        public void ExportAll()
        {
            ExportItems();
            ExportEnemies();
            CustomExport();
        }

        private void ExportItems()
        {
            var itemsByType = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
            foreach (var itemType in Enum.GetNames(typeof(EPickupItemType)))
            {
                itemsByType.Add(itemType, new Dictionary<string, Dictionary<string, object>>());
            }

            foreach (var item in Runtime.Configuration.Items)
            {
                var type = item.Type.ToString();
                var name = item.GetName();
                while (itemsByType[type].ContainsKey(name))
                {
                    name = $"{name}_x";
                }

                itemsByType[type].Add(name, new Dictionary<string, object>());
                itemsByType[type][name].Add("Upgradable", item.CanBeUpgraded);
                itemsByType[type][name].Add("Description", item.Description.ToString());
                itemsByType[type][name].Add("Properties", item.Properties);
                itemsByType[type][name].Add("Tags", item.Tags.Select(x => x.ToString()));
                itemsByType[type][name].Add("Stats", item.RelatedStats.Select(x => x.ToString()));
                itemsByType[type][name].Add("StatusEffects", item.RelatedStatusEffects.Select(x => x.ToString()));
                itemsByType[type][name].Add("StackLimit", item.StackLimit);
                itemsByType[type][name].Add("IsStackable", item.IsStackable);
                itemsByType[type][name].Add("HasStackLimit", item.HasStackLimit);
            }
            var itemsAsJson = JsonConvert.SerializeObject(itemsByType, Formatting.Indented);
            File.WriteAllText("items.json", itemsAsJson);
        }

        private void ExportEnemies()
        {
            var enemiesByDifficulty = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
            foreach (var enemyDifficulty in Enum.GetNames(typeof(EEnemyDifficulty)))
            {
                enemiesByDifficulty.Add(enemyDifficulty, new Dictionary<string, Dictionary<string, object>>());
            }

            foreach (var enemy in Runtime.Configuration.Enemies)
            {
                var name = enemy.Name.ToString();
                var difficulty = enemy.Difficulty.ToString();
                while (enemiesByDifficulty[difficulty].ContainsKey(name))
                {
                    name = $"{name}_x";
                }
                enemiesByDifficulty[difficulty].Add(name, new Dictionary<string, object>());
                enemiesByDifficulty[difficulty][name].Add("Baselevel", enemy.BaseLevel);
                enemiesByDifficulty[difficulty][name].Add("Attacks", enemy.Attacks.Select(x => x.AttackName));
            }

            var enemiesAsJson = JsonConvert.SerializeObject(enemiesByDifficulty, Formatting.Indented);
            File.WriteAllText("enemies.json", enemiesAsJson);
        }

        private void CustomExport()
        {
            var exportText = "";
            foreach (var item in Runtime.Configuration.Items)
            {
                if (item.Type != EPickupItemType.Perk)
                {
                    continue;
                }

                var name = item.GetName();
                var lowerName = name.ToLower().Replace(" ", "_");

                var flags = $"[{string.Join(", ", item.Tags.Select(x => $"PerkFlags.{x.ToString().ToLower()}"))}]";

                var stackLimit = item.StackLimit;
                if (stackLimit <= 0)
                {
                    stackLimit = 10;
                }

                if (!item.IsStackable)
                {
                    stackLimit = 1;
                }

                var line = $"    {lowerName} = PerkData(\"{name}\", {stackLimit}, {flags})";
                exportText += Environment.NewLine + line;
            }
            File.WriteAllText("export.txt", exportText);
        }
    }
}
