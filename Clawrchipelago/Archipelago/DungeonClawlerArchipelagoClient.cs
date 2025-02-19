using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Clawrchipelago.HarmonyPatches;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace Clawrchipelago.Archipelago
{
    public class DungeonClawlerArchipelagoClient : ArchipelagoClient
    {
        public override string GameName => "Dungeon Clawler";
        public override string ModName => "Clawrchipelago";
        public override string ModVersion => MyPluginInfo.PLUGIN_VERSION;

        public SlotData SlotData => (SlotData)_slotData;

        public DungeonClawlerArchipelagoClient(ILogger logger, Action<ReceivedItemsHelper> itemReceivedFunction) : 
            base(logger, new DataPackageCache("dungeon_clawler", "BepInEx", "plugins", "Clawrchipelago", "IdTables"), itemReceivedFunction)
        {
        }

        protected override void InitializeSlotData(string slotName, Dictionary<string, object> slotDataFields)
        {
            _slotData = new SlotData(slotName, slotDataFields, Logger);
        }

        protected override void OnMessageReceived(LogMessage message)
        {
            var fullMessage = string.Join(" ", message.Parts.Select(str => str.Text));
            Logger.LogInfo(fullMessage);
        }

        protected override void KillPlayerDeathLink(DeathLink deathLinkOptions)
        {
            Logger.LogInfo($"Receiving Death Link from {deathLinkOptions.Source} ({deathLinkOptions.Cause})");
            DeathlinkPatches.ReceiveDeathink();
        }
    }
}
