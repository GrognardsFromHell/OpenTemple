using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO.TabFiles;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.AI;

internal class AiStrategies
{
    private const int AI_PREFAB_STRAT_MAX = 10000;

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly List<AiStrategy> aiStrategies = new();

    private readonly Dictionary<int, AiStrategy> aiCustomStrats = new();

    internal void LoadStrategies(string path)
    {
        TabFile.ParseFile(path, ProcessStrategy);
    }

    private void ProcessStrategy(in TabFileRecord record)
    {
        var newStrategy = new AiStrategy(record[0].AsString());

        for (var i = 1; i + 3 < record.ColumnCount; i += 3)
        {
            var tacName = record[i].AsString();
            var middleString = record[i + 1].AsString();
            var spellString = record[i + 2].AsString();
            StrategyTabLineParseTactic(newStrategy, tacName, middleString, spellString);
        }

        aiStrategies.Add(newStrategy);
    }

    // this functions matches the tactic strings (3 strings) to a tactic def
    private void StrategyTabLineParseTactic(AiStrategy aiStrat, string tacName, string middleString, string spellString)
    {
        if (tacName.Length == 0) {
            return;
        }

        // first check the vanilla tactic defs
        if (DefaultTactics.TryGetByName(tacName, out var tacticDef))
        {
            aiStrat.aiTacDefs.Add(tacticDef);
            aiStrat.field54.Add(0);

            if (spellString.Length > 0 && GameSystems.Spell.TryParseSpellSpecString(spellString, out var parsedSpell))
            {
                aiStrat.spellsKnown.Add(parsedSpell);
            }
            else
            {
                aiStrat.spellsKnown.Add(new SpellStoreData(-1, 0, 0));
            }

            return;
        }

        Stub.TODO(); // TODO: New TemplePlus tactics

        Logger.Warn("Error: No Such Tactic {0} for Strategy {1}", tacName, aiStrat.name);
    }

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