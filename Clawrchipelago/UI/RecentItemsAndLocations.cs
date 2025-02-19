using Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using Clawrchipelago.Archipelago;
using Clawrchipelago.Extensions;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using Platforms;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ILogger = KaitoKid.ArchipelagoUtilities.Net.Interfaces.ILogger;
using Object = UnityEngine.Object;

namespace Clawrchipelago.UI
{
    public class RecentItemsAndLocations
    {
        public const int MAX_DISPLAYED_ENTRIES = 5;

        private ILogger _logger;
        private DungeonClawlerArchipelagoClient _archipelago;
        private TextMeshProUGUI _recentItemsLabel;
        private TextMeshProUGUI _recentItemsOriginLabel;
        private TextMeshProUGUI _recentLocationsLabel;
        private TextMeshProUGUI _recentLocationsRecipientsLabel;

        private List<ReceivedItem> _recentItems;
        private List<string> _recentLocations;
        // private List<Image> _icons;

        public RecentItemsAndLocations(ILogger logger, DungeonClawlerArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
            _recentItems = [];
            _recentLocations = [];
            //_icons = [];
            //for (var i = 0; i < MAX_DISPLAYED_ENTRIES; i++)
            //{
            //    _icons.Add(null);
            //}
        }

        public void UpdateItems()
        {
            if (!ClawrchipelagoMod.Instance.Config.ShowRecentItems || !_archipelago.MakeSureConnected())
            {
                return;
            }

            var allItems = _archipelago.GetAllReceivedItems();
            _recentItems = allItems.TakeLast(MAX_DISPLAYED_ENTRIES).Reverse().ToList();

            // UpdateIcons();
        }

        public void UpdateLocations(List<string> locationsInOrder)
        {
            if (!ClawrchipelagoMod.Instance.Config.ShowRecentLocations ||  !_archipelago.MakeSureConnected())
            {
                return;
            }

            _recentLocations = locationsInOrder.TakeLast(MAX_DISPLAYED_ENTRIES).Reverse().ToList();
        }

        public void Update()
        {
            if (ClawrchipelagoMod.Instance.Config.ShowRecentItems)
            {
                InstantiateRecentItemsLabel();
                UpdateRecentItemsLabel();
                // InstantiateIcons();
            }

            if (ClawrchipelagoMod.Instance.Config.ShowRecentLocations)
            {
                InstantiateRecentLocationsLabel();
                UpdateRecentLocationsLabel();
            }
        }

        private void UpdateRecentItemsLabel()
        {
            if (_recentItemsLabel == null || _recentItemsOriginLabel == null)
            {
                return;
            }

            var recentItemsText = $"Recent Items:";
            var recentItemsOriginText = "";
            foreach (var recentItem in _recentItems)
            {
                recentItemsText += $"{Environment.NewLine}  - {recentItem.ItemName}{Environment.NewLine}";
                recentItemsOriginText += $"{Environment.NewLine}    from {recentItem.PlayerName} at {recentItem.LocationName}{Environment.NewLine}{Environment.NewLine}";
            }

            _recentItemsLabel.text = recentItemsText;
            _recentItemsOriginLabel.text = recentItemsOriginText;
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
            _recentItemsLabel = InstantiateLabel(36, 2, 4.5f);
            _recentItemsOriginLabel = InstantiateLabel(24, 2.6f, 4.35f);
        }

        private void InstantiateRecentLocationsLabel()
        {
            if (_recentLocationsLabel != null)
            {
                return;
            }
            _recentLocationsLabel = InstantiateLabel(36, 7, 4.5f);
        }

        private TextMeshProUGUI InstantiateLabel(float fontSize, float downPos, float leftPos)
        {
            if (Game.Instance?.MapUI?.FloorLabel == null)
            {
                return null;
            }

            var label = Object.Instantiate(Game.Instance.MapUI.FloorLabel, Game.Instance.MapUI.transform, true);
            label.alignment = TextAlignmentOptions.TopLeft;
            label.fontSize = fontSize;
            label.transform.position += (Vector3.down * downPos);
            label.transform.position += (Vector3.left * leftPos);
            return label;
        }

        //private Image InstantiateIcon(float downPos, float leftPos)
        //{
        //    var currencyDisplays = Game.Instance?.MapUI?.CurrencyDisplay?.CurrencyDisplays();
        //    if (currencyDisplays == null || !currencyDisplays.Any())
        //    {
        //        return null;
        //    }

        //    var firstCurrencyDisplayIcon = currencyDisplays.First().Value?.Icon;
        //    if (firstCurrencyDisplayIcon == null || Game.Instance?.MapUI?.transform == null)
        //    {
        //        return null;
        //    }

        //    var image = Object.Instantiate(firstCurrencyDisplayIcon, Game.Instance.MapUI.transform, true);
        //    image.transform.position += (Vector3.down * downPos);
        //    image.transform.position += (Vector3.left * leftPos);
        //    return image;
        //}

        //private void UpdateIcons()
        //{
        //    for (var i = 0; i < _recentItems.Count; i++)
        //    {
        //        if (_icons[i] != null)
        //        {
        //            _logger.LogInfo($"Getting Image For {_recentItems[i]}");
        //            _icons[i].sprite = GetImageFor(_recentItems[i]);
        //        }
        //    }
        //}

        //private Sprite GetImageFor(string recentItem)
        //{
        //    foreach (var itemSetting in Runtime.Configuration.Items)
        //    {
        //        if (itemSetting.Name.ToEnglish() == recentItem)
        //        {
        //            return itemSetting.Image;
        //        }
        //    }

        //    return null;
        //}

        //private void InstantiateIcons()
        //{
        //    for (var i = 0; i < _recentItems.Count; i++)
        //    {
        //        if (_icons[i] == null)
        //        {
        //            _logger.LogInfo($"Instantiating icon For {_recentItems[i]}");
        //            _icons[i] = InstantiateIcon(1 + (i * 0.25f), 1);
        //        }
        //    }
        //}
    }
}
