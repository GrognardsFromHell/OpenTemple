
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Dialog
{
    [DialogScript(72)]
    public class BraumeisterDialog : Braumeister, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                case 6:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 43:
                case 44:
                case 61:
                case 62:
                case 212:
                case 215:
                    originalScript = "game.quests[14].state == qs_completed and game.global_flags[30] == 1";
                    return GetQuestState(14) == QuestState.Completed && GetGlobalFlag(30);
                case 53:
                case 54:
                case 81:
                case 82:
                    originalScript = "pc.stat_level_get( stat_deity ) == 16";
                    return pc.GetStat(Stat.deity) == 16;
                case 65:
                case 66:
                    originalScript = "game.quests[14].state <= qs_mentioned";
                    return GetQuestState(14) <= QuestState.Mentioned;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 130:
                case 150:
                    originalScript = "npc.reaction_adj( pc,-5)";
                    npc.AdjustReaction(pc, -5);
                    break;
                default:
                    originalScript = null;
                    return;
            }
        }
        public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
