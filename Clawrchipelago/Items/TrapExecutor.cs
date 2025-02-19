using Archipelago.MultiClient.Net.Models;
using Clawrchipelago.Archipelago;
using Gameplay.Items.Settings;
using Gameplay.Liquid.Settings;
using Gameplay;
using Platforms;
using System;
using System.Collections;
using System.Collections.Generic;
using Clawrchipelago.Extensions;
using Gameplay.Abilities.ItemEffects;
using UnityEngine;
using ILogger = KaitoKid.ArchipelagoUtilities.Net.Interfaces.ILogger;
using Gameplay.Abilities;
using Gameplay.Combatants;
using Gameplay.Items;
using Gameplay.Values;
using System.Linq;
using Utils;
using Random = System.Random;
using static Gameplay.Abilities.Ability;
using System.Reflection;

namespace Clawrchipelago.Items
{
    public class TrapExecutor
    {
        private const string TRAP_SUFFIX = " Trap";

        private readonly ILogger _logger;
        private readonly DungeonClawlerArchipelagoClient _archipelago;
        private Queue<string> _trapQueue;

        public TrapExecutor(ILogger logger, DungeonClawlerArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
            _trapQueue = new Queue<string>();
        }

        public void Update()
        {
            if (Time.frameCount % 10 != 0)
            {
                return;
            }

            if (_trapQueue.Count <= 0)
            {
                return;
            }

            TryExecuteNextTrap();
        }

        public bool TryHandleReceivedTrap(ItemInfo item)
        {
            if (item.ItemName.EndsWith(TRAP_SUFFIX))
            {
                _trapQueue.Enqueue(item.ItemName);
                return true;
            }

            return false;
        }

        private void TryExecuteNextTrap()
        {
            // _logger.LogDebug($"TryHandleReceivedTrap: {item.ItemName}");
            var allItems = Runtime.Configuration?.Items;
            var allLiquids = Runtime.Configuration?.Liquids;
            var allEffects = Runtime.Configuration?.ItemEffects;
            var game = Game.Instance;
            var clawMachine = game?.ClawMachine;
            if (allItems == null || allLiquids == null || allEffects == null || game == null ||
                clawMachine == null || !clawMachine.gameObject.activeSelf || clawMachine.ClawMachineBox == null || game.ClawMachineLoopFinished())
            {
                return;
            }

            var trapName = _trapQueue.Dequeue();
            if (trapName.EndsWith(TRAP_SUFFIX))
            {
                trapName = trapName.Substring(0, trapName.Length - TRAP_SUFFIX.Length);
            }

            if (TryExecuteItemTrap(trapName, allItems, clawMachine))
            {
                return;
            }

            if (TryExecuteLiquidTrap(trapName, allLiquids, clawMachine))
            {
                return;
            }

            if (TryExecuteEffectTrap(trapName, allEffects, clawMachine))
            {
                return;
            }

            _logger.LogWarning($"Unrecognized Trap Name: {trapName}");
            return;
        }

        private bool TryExecuteItemTrap(string trapName, PickupItemSetting[] allItems, ClawMachine clawMachine)
        {
            var itemToAdd = GetTrapItemToAdd(trapName, allItems);
            if (itemToAdd == null)
            {
                return false;
            }

            var amount = _archipelago.SlotData.TrapDifficulty switch
            {
                TrapDifficulty.NoTraps => 0,
                TrapDifficulty.Easy => 1,
                TrapDifficulty.Medium => 2,
                TrapDifficulty.Hard => 4,
                TrapDifficulty.Hell => 8,
                TrapDifficulty.Nightmare => 16,
                _ => throw new ArgumentOutOfRangeException()
            };

            _logger.LogDebug($"Executing Trap Item: {itemToAdd.Name.ToEnglish()} ({amount})");
            Game.Instance.StartCoroutine(clawMachine.AddItems(itemToAdd, amount));
            return true;

        }

        private bool TryExecuteLiquidTrap(string trapName, LiquidSetting[] allLiquids, ClawMachine clawMachine)
        {
            var liquidToAdd = GetTrapLiquidToAdd(trapName, allLiquids);
            if (liquidToAdd == null)
            {
                return false;
            }

            _logger.LogDebug($"Executing Trap Liquid: {liquidToAdd.Name.ToEnglish()}");
            Game.Instance.StartCoroutine(FillMachineWithLiquid(clawMachine, liquidToAdd));
            return true;
        }

        private bool TryExecuteEffectTrap(string trapName, ItemEffectSetting[] allEffects, ClawMachine clawMachine)
        {
            var effectToAdd = GetTrapEffectToAdd(trapName, allEffects);
            if (effectToAdd == null)
            {
                return false;
            }

            var amount = _archipelago.SlotData.TrapDifficulty switch
            {
                TrapDifficulty.NoTraps => 0,
                TrapDifficulty.Easy => 2,
                TrapDifficulty.Medium => 4,
                TrapDifficulty.Hard => 8,
                TrapDifficulty.Hell => 16,
                TrapDifficulty.Nightmare => 32,
                _ => throw new ArgumentOutOfRangeException()
            };

            _logger.LogDebug($"Executing Trap Effect: {effectToAdd.Name.ToEnglish()}");
            Game.Instance.StartCoroutine(AddEffectToItems(clawMachine, effectToAdd, amount));
            return true;
        }

        private PickupItemSetting GetTrapItemToAdd(string trapName, PickupItemSetting[] allItems)
        {
            foreach (var potentialItem in allItems)
            {
                var name = potentialItem.Name.ToEnglish();
                if (name.Equals(trapName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return potentialItem;
                }
            }

            return null;
        }

        private LiquidSetting GetTrapLiquidToAdd(string trapName, LiquidSetting[] allLiquids)
        {
            foreach (var potentialLiquid in allLiquids)
            {
                var name = potentialLiquid.Name.ToEnglish();
                if (name.Equals(trapName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return potentialLiquid;
                }
            }

            return null;
        }

        private ItemEffectSetting GetTrapEffectToAdd(string trapName, ItemEffectSetting[] allEffects)
        {
            foreach (var potentialEffect in allEffects)
            {
                var name = potentialEffect.Name.ToEnglish();
                if (name.Equals(trapName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return potentialEffect;
                }
            }

            return null;
        }

        private IEnumerator FillMachineWithLiquid(ClawMachine clawMachine, LiquidSetting liquid)
        {
            yield return clawMachine.ClawMachineBox.WaterLevel.ChangeLiquid(liquid);
            yield return clawMachine.ClawMachineBox.WaterLevel.ChangeWaterLevel(null, 1.0f, null);
        }

        private IEnumerator AddEffectToItems(ClawMachine clawMachine, ItemEffectSetting effect, int amount)
        {
            var random = new Random();
            var itemCandidates = clawMachine.CurrentItems.OrderBy(x => random.NextDouble());
            var itemsChosen = itemCandidates.Take(amount);
            foreach (var item in itemsChosen)
            {
                item.ApplyItemEffect(Game.Instance.Data.Fighter, effect, true);
            }
            yield return true;
        }
    }
}
