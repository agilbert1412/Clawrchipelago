using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using KaitoKid.ArchipelagoUtilities.Net.Client;

namespace Clawrchipelago.Serialization
{
    public class PersistentData
    {
        public List<ReceivedItem> ItemsParsed { get; set; } = [];

        public static void SaveData(PersistentData config, int slotSeed)
        {
            var file = GetPersistencyFilePath(slotSeed);
            var jsonObject = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(file, jsonObject);
        }

        public static PersistentData LoadData(int slotSeed)
        {
            var file = GetPersistencyFilePath(slotSeed);
            if (!File.Exists(file))
            {
                var defaultData = new PersistentData();
                SaveData(defaultData, slotSeed);
                return defaultData;
            }

            var jsonObject = File.ReadAllText(file);
            var data = JsonConvert.DeserializeObject<PersistentData>(jsonObject);
            return data;
        }

        private static string GetPersistencyFilePath(int slotSeed)
        {
            return string.Format(Persistency.PERSISTENCY_FILE_FORMAT, slotSeed);
        }
    }
}
