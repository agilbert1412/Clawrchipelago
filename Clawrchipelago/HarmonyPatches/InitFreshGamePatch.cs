using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net;
using Gameplay;
using Gameplay.Rooms;
using Gameplay.Rooms.Data;
using UnityEngine;
using ILogger = KaitoKid.ArchipelagoUtilities.Net.Interfaces.ILogger;
using Random = System.Random;
using Platforms;
using Clawrchipelago.Extensions;
using Gameplay.Items.Data;
using Gameplay.Items.Settings;
using Clawrchipelago.Archipelago;

namespace Clawrchipelago.HarmonyPatches
{
    [HarmonyPatch(typeof(GameData))]
    [HarmonyPatch(nameof(GameData.InitFreshGame))]
    public class InitFreshGamePatch
    {
        private static ILogger _logger;
        private static DungeonClawlerArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        public static Vector2Int CombatShredderPosition;
        public static Vector2Int PerksShredderPosition;

        public static void Initialize(ILogger logger, DungeonClawlerArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public void InitFreshGame(System.Random rng)
        public static void Postfix(GameData __instance, Random rng)
        {
            try
            {
                _logger.LogDebugPatchIsRunning(nameof(GameData), nameof(GameData.InitFreshGame), nameof(InitFreshGamePatch),
                    nameof(Postfix));

                SetPerksDeck();
                SetStartingMoney(__instance);

                var mapData = __instance.MapData;
                var map = mapData.Map;

                var tilesToMoveTowardsExit = GetTilesToOffsetTowardsExit(map);

                OffsetTilesTowardsExit(map, tilesToMoveTowardsExit);

                CreateStartingShredders(mapData, map);

                return;
            }
            catch (Exception ex)
            {
                _logger.LogErrorException(nameof(InitFreshGamePatch), nameof(Postfix), ex);
                return;
            }
        }

        private static List<Point> GetTilesToOffsetTowardsExit(MapTileData[,] map)
        {
            var tilesToMoveTowardsExit = new List<Point>();

            for (var x = 0; x < map.GetLength(0); x++)
            {
                for (var y = 0; y < map.GetLength(1); y++)
                {
                    var tile = map[x, y];
                    if (tile.Type == EMapTileType.Nothing)
                    {
                        continue;
                    }

                    if (tile.Type == EMapTileType.Exit || tile.Type == EMapTileType.Entrance || tile.Type == EMapTileType.Empty)
                    {
                        // _logger.LogDebug($"[{x},{y}]: {tile.Type.ToString()}  (Ignore)");
                        continue;
                    }

                    // _logger.LogDebug($"[{x},{y}]: {tile.Type.ToString()} (Swap with right)");
                    tilesToMoveTowardsExit.Add(new Point(x, y));
                }
            }

            return tilesToMoveTowardsExit;
        }

        private static void OffsetTilesTowardsExit(MapTileData[,] map, List<Point> tilesToMoveTowardsExit)
        {
            foreach (var tileCoordinates in tilesToMoveTowardsExit)
            {
                // _logger.LogDebug($"Swapping [{tileCoordinates.X},{tileCoordinates.Y}] with [{tileCoordinates.X + 1},{tileCoordinates.Y}]");
                var tile = map[tileCoordinates.X, tileCoordinates.Y];
                var swapTile = map[tileCoordinates.X + 1, tileCoordinates.Y];
                var position = tile.Position;
                var swapPosition = swapTile.Position;
                var index = tile.TileIndex; // Should I swap these? seems to work without them...
                var swapIndex = swapTile.TileIndex;
                map[tileCoordinates.X, tileCoordinates.Y] = swapTile;
                map[tileCoordinates.X + 1, tileCoordinates.Y] = tile;
                tile.Position = swapPosition;
                swapTile.Position = position;
            }
        }

        private static void CreateStartingShredders(MapData mapData, MapTileData[,] map)
        {
            var shredderRoomSetting = Runtime.Configuration.Rooms.First(x => x.RoomType == EMapTileType.Shredder);
            for (var i = 1; i <= 2; i++)
            {
                var playerPosition = mapData.CurrentMapPosition;
                var tilePosition = playerPosition + (Vector2Int.right * i);
                if (i == 1)
                {
                    if (!_archipelago.SlotData.ShuffleItems)
                    {
                        continue;
                    }

                    if (Game.Instance.Data.Items.Count <= RecycleUIPatches.GetMaxStartingCombatItems())
                    {
                        CombatShredderPosition = new Vector2Int(-999, -999);
                        continue;
                    }

                    CombatShredderPosition = tilePosition;
                }
                else
                {
                    if (!_archipelago.SlotData.ShufflePerks)
                    {
                        continue;
                    }

                    if (Game.Instance.Data.Perks.Count - 1 <= RecycleUIPatches.GetMaxPerks())
                    {
                        PerksShredderPosition = new Vector2Int(-999, -999);
                        continue;
                    }

                    PerksShredderPosition = tilePosition;
                }
                var tileToAddStartingShredder = map[tilePosition.x, tilePosition.y];
                _logger.LogDebug($"Creating a starting shredder at [{tilePosition.x},{tilePosition.y}] ({tileToAddStartingShredder.Type.ToString()})");
                tileToAddStartingShredder.ChangeType(EMapTileType.Shredder);
                tileToAddStartingShredder.RoomSetting = shredderRoomSetting;
            }
        }

        private static void SetPerksDeck()
        {
            if (!_archipelago.SlotData.ShufflePerks)
            {
                return;
            }

            var deck = Game.Instance.Data.Perks;
            foreach (var item in Runtime.Configuration.Items)
            {
                if (item.Type != EPickupItemType.Perk || item.Rarity == EItemRarity.DontDrop)
                {
                    continue;
                }

                var itemName = item.Name.ToEnglish();
                var receivedCount = _archipelago.GetReceivedItemCount(itemName);

                bool IsThisItem(PickupItemData x) => x.Setting.Name.ToEnglish().Equals(itemName);

                if (receivedCount <= 0)
                {
                    deck.RemoveAll(IsThisItem);
                    continue;
                }

                if (!deck.Any(IsThisItem))
                {
                    deck.Add(item.GenerateData());
                }

                deck.First(IsThisItem).PerkCount = receivedCount;
            }
        }

        private static void SetStartingMoney(GameData gameData)
        {
            foreach (var currency in gameData.Currencies.Keys.ToArray())
            {
                gameData.Currencies[currency] = _archipelago.GetReceivedItemCount("Starting Money") * 10;
            }
        }
    }
}
