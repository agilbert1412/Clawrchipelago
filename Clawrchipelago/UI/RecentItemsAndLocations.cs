using Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clawrchipelago.Archipelago;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using ILogger = KaitoKid.ArchipelagoUtilities.Net.Interfaces.ILogger;
using Object = UnityEngine.Object;

namespace Clawrchipelago.UI
{
    public class RecentItemsAndLocations
    {
        private const int MAX_DISPLAYED_ENTRIES = 5;

        private ILogger _logger;
        private DungeonClawlerArchipelagoClient _archipelago;
        private TextMeshProUGUI _recentItemsLabel;
        private TextMeshProUGUI _recentLocationsLabel;

        private List<string> _recentItems;
        private List<string> _recentLocations;

        public RecentItemsAndLocations(ILogger logger, DungeonClawlerArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
            _recentItems = [];
            _recentLocations = [];
        }

        public void UpdateItems()
        {
            if (!_archipelago.MakeSureConnected())
            {
                return;
            }

            var allItems = _archipelago.GetAllReceivedItems();
            if (allItems.Count <= MAX_DISPLAYED_ENTRIES)
            {
                _recentItems = allItems.Select(x => x.ItemName).Reverse().ToList();
            }
            else
            {
                _recentItems = allItems.TakeLast(MAX_DISPLAYED_ENTRIES).Select(x => x.ItemName).Reverse().ToList();
            }
        }

        public void UpdateLocations(List<string> locationsInOrder)
        {
            if (!_archipelago.MakeSureConnected())
            {
                return;
            }

            _recentLocations = locationsInOrder.TakeLast(MAX_DISPLAYED_ENTRIES).Reverse().ToList();
        }

        public void Update()
        {
            InstantiateRecentItemsLabel();
            InstantiateRecentLocationsLabel();
            UpdateRecentItemsLabel();
            UpdateRecentLocationsLabel();
        }

        private void UpdateRecentItemsLabel()
        {
            if (_recentItemsLabel == null)
            {
                return;
            }

            var recentItemsText = $"Recent Items:";
            foreach (var recentItem in _recentItems)
            {
                recentItemsText += $"{Environment.NewLine}  - {recentItem}";
            }

            _recentItemsLabel.text = recentItemsText;
        }

        private void UpdateRecentLocationsLabel()
        {
            if (_recentLocationsLabel == null)
            {
                return;
            }

            var recentLocationsText = $"Recent Locations:";
            foreach (var recentItem in _recentLocations)
            {
                recentLocationsText += $"{Environment.NewLine}  - {recentItem}";
            }

            _recentLocationsLabel.text = recentLocationsText;
        }

        private void InstantiateRecentItemsLabel()
        {
            if (_recentItemsLabel != null)
            {
                return;
            }
            _recentItemsLabel = InstantiateLabel(2, 4.5f);
        }

        private void InstantiateRecentLocationsLabel()
        {
            if (_recentLocationsLabel != null)
            {
                return;
            }
            _recentLocationsLabel = InstantiateLabel(6, 4.5f);
        }

        private TextMeshProUGUI InstantiateLabel(float downPos, float leftPos)
        {
            if (Game.Instance?.FloorLabel == null)
            {
                return null;
            }

            var label = Object.Instantiate(Game.Instance.MapUI.FloorLabel, Game.Instance.MapUI.transform, true);
            label.alignment = TextAlignmentOptions.TopLeft;
            label.fontSize = 42;
            label.transform.position += (Vector3.down * downPos);
            label.transform.position += (Vector3.left * leftPos);
            return label;
        }
    }
}
