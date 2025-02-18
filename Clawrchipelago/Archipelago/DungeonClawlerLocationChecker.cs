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
        public List<string> LocationsInOrder;

        public DungeonClawlerLocationChecker(ILogger logger, ArchipelagoClient archipelago, List<string> locationsAlreadyChecked, RecentItemsAndLocations recentItemsAndLocations) : base(logger, archipelago, locationsAlreadyChecked)
        {
            _recentItemsAndLocations = recentItemsAndLocations;
            LocationsInOrder = new List<string>(locationsAlreadyChecked);
        }

        protected override void RememberCheckedLocation(string locationName)
        {
            base.RememberCheckedLocation(locationName);
            AddNewLocation(locationName);
            _recentItemsAndLocations.UpdateLocations(LocationsInOrder);
        }

        public override void SendAllLocationChecks()
        {
            base.SendAllLocationChecks();
            _recentItemsAndLocations.UpdateLocations(LocationsInOrder);
        }

        public override void VerifyNewLocationChecksWithArchipelago()
        {
            base.VerifyNewLocationChecksWithArchipelago();
            AddNewLocations(GetAllLocationsAlreadyChecked());
            _recentItemsAndLocations.UpdateLocations(LocationsInOrder);
        }

        private void AddNewLocations(IEnumerable<string> locations)
        {
            foreach (var location in locations)
            {
                AddNewLocation(location);
            }
        }

        private void AddNewLocation(string location)
        {
            if (LocationsInOrder.Contains(location))
            {
                return;
            }

            LocationsInOrder.Add(location);
        }
    }
}
