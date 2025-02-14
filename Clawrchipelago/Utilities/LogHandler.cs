using BepInEx.Logging;
using UnityEngine;

namespace Clawrchipelago.Utilities
{
    public class LogHandler : KaitoKid.ArchipelagoUtilities.Net.Client.Logger
    {
        private readonly ManualLogSource _logger;

        public LogHandler(ManualLogSource logger)
        {
            _logger = logger;
        }

        public override void LogError(string message)
        {
            _logger.LogError(message);
        }

        public override void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        public override void LogMessage(string message)
        {
            _logger.LogMessage(message);
        }

        public override void LogInfo(string message)
        {
            _logger.LogInfo(message);
        }

        public override void LogDebug(string message)
        {
            _logger.LogDebug(message);
        }

        public void LogStatus(Transform transform, int numberTabs)
        {
            var tabs = new string('\t', numberTabs);
            _logger.LogMessage($"{tabs}transform: {transform}");
            _logger.LogMessage($"{tabs}transform.transform: {transform.transform}");
            _logger.LogMessage($"{tabs}transform.childCount: {transform.transform.childCount}");
            for (var i = 0; i < transform.childCount; i++)
            {
                _logger.LogMessage($"{tabs}\transform.GetChild(i): {transform.transform.GetChild(i)}");
            }
        }
    }
}
