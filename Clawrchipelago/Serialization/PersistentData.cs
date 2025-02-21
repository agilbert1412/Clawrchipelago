using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Clawrchipelago.Archipelago;
using KaitoKid.ArchipelagoUtilities.Net.Client;

namespace Clawrchipelago.Serialization
{
    public class PersistentData
    {
        public List<ReceivedItem> ItemsParsed { get; set; } = [];

        public static void SaveData(PersistentData config, SlotData slotData)
        {
            var file = GetPersistencyFilePath(slotData);
            var fileInfo = new FileInfo(file);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            var jsonObject = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(file, jsonObject);
        }

        public static PersistentData LoadData(SlotData slotData)
        {
            var file = GetPersistencyFilePath(slotData);
            if (!File.Exists(file))
            {
                var defaultData = new PersistentData();
                SaveData(defaultData, slotData);
                return defaultData;
            }

            var jsonObject = File.ReadAllText(file);
            var data = JsonConvert.DeserializeObject<PersistentData>(jsonObject);
            return data;
        }

        private static string GetPersistencyFilePath(SlotData slotData)
        {
            return string.Format(Persistency.PERSISTENCY_FILE_FORMAT, slotData.SlotName, slotData.Seed);
        }
    }
}
