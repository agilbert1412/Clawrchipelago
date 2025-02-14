using Gameplay;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;

namespace Clawrchipelago.Archipelago;

public class ArchipelagoReceivedCondition : UnlockCondition
{
    private readonly ILogger _logger;
    private readonly ArchipelagoClient _archipelago;
    private readonly string _requiredItem;
    private readonly int _requiredAmount;

    public ArchipelagoReceivedCondition(ILogger logger, ArchipelagoClient archipelago, string requiredItem, int requiredAmount = 1)
    {
        _logger = logger;
        _archipelago = archipelago;
        _requiredItem = requiredItem;
        _requiredAmount = requiredAmount;
        _logger.LogDebug($"Creating {nameof(ArchipelagoReceivedCondition)} condition for {_requiredItem} (x{_requiredAmount})");
    }

    public override bool Check(GameData data)
    {
        var isUnlocked = _archipelago.GetReceivedItemCount(_requiredItem) >= _requiredAmount;
        _logger.LogDebug($"Checking {nameof(ArchipelagoReceivedCondition)} condition for {_requiredItem} (x{_requiredAmount}) [{isUnlocked}]");
        return isUnlocked;
    }

    public override string GetDescription()
    {
        var amountString = _requiredAmount > 1 ? $" {_requiredAmount} times" : "";
        return $"You need to receive the item {_requiredItem}{amountString}";
    }
}