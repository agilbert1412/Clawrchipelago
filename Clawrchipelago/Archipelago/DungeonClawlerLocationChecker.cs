using System;
using System.Collections.Generic;
using System.Text;
using Clawrchipelago.UI;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace Clawrchipelago.Archipelago
{
    internal class DungeonClawlerLocationChecker : LocationChecker
    {
        private readonly RecentItemsAndLocations _recentItemsAndLocations;

        public DungeonClawlerLocationChecker(ILogger logger, ArchipelagoClient archipelago, List<string> locationsAlreadyChecked, RecentItemsAndLocations recentItemsAndLocations) : base(logger, archipelago, locationsAlreadyChecked)
        {
            _recentItemsAndLocations = recentItemsAndLocations;
        }

        public override void SendAllLocationChecks()
        {
            base.SendAllLocationChecks();
            _recentItemsAndLocations.UpdateLocations();
        }
    }
}
