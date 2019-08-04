using System.Collections.Generic;
using SpicyTemple.Core.Logging;

namespace SpicyTemple.Core.Systems.AI
{
    internal class AiStrategies
    {
        private const int AI_PREFAB_STRAT_MAX = 10000;

        private static readonly ILogger Logger = new ConsoleLogger();

        private readonly List<AiStrategy> aiStrategies = new List<AiStrategy>();

        private readonly Dictionary<int, AiStrategy> aiCustomStrats = new Dictionary<int, AiStrategy>();

        internal AiStrategy GetAiStrategy(int stratId) {
            // Check if "normal" strategy ID
            if (stratId < AI_PREFAB_STRAT_MAX)
            {
                if (stratId < aiStrategies.Count)
                {
                    return aiStrategies[stratId];
                }
                Logger.Warn("Strategy {0} not found", stratId);
                return aiStrategies[0];
            }

            // otherwise - fetch custom strategy
            if (aiCustomStrats.TryGetValue(stratId, out var strat))
            {
                return strat;
            }

            Logger.Warn("Custom strat ID {0} not found!", stratId);
            return aiStrategies[0];
        }

    }


}