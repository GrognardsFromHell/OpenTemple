
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Dialog
{
    [DialogScript(90)]
    public class BrauApprentice3Dialog : BrauApprentice3, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 63:
                case 64:
                    Trace.Assert(originalScript == "not npc.has_met( pc )");
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                case 8:
                    Trace.Assert(originalScript == "npc.has_met( pc )");
                    return npc.HasMet(pc);
                case 6:
                case 7:
                    Trace.Assert(originalScript == "( game.quests[60].state == qs_mentioned or game.quests[60].state == qs_accepted )");
                    return (GetQuestState(60) == QuestState.Mentioned || GetQuestState(60) == QuestState.Accepted);
                case 61:
                case 62:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.global_flags[357] == 0");
                    return npc.HasMet(pc) && !GetGlobalFlag(357);
                case 65:
                case 66:
                    Trace.Assert(originalScript == "( game.quests[34].state == qs_mentioned or game.quests[34].state == qs_accepted ) and npc.has_met( pc )");
                    return (GetQuestState(34) == QuestState.Mentioned || GetQuestState(34) == QuestState.Accepted) && npc.HasMet(pc);
                case 67:
                case 68:
                    Trace.Assert(originalScript == "( game.quests[60].state == qs_mentioned or game.quests[60].state == qs_accepted ) and npc.has_met( pc )");
                    return (GetQuestState(60) == QuestState.Mentioned || GetQuestState(60) == QuestState.Accepted) && npc.HasMet(pc);
                case 69:
                case 70:
                    Trace.Assert(originalScript == "game.global_flags[357] == 1");
                    return GetGlobalFlag(357);
                case 81:
                case 82:
                case 95:
                case 96:
                    Trace.Assert(originalScript == "game.global_flags[322] == 1");
                    return GetGlobalFlag(322);
                case 83:
                case 84:
                case 97:
                case 98:
                    Trace.Assert(originalScript == "game.global_flags[322] == 0");
                    return !GetGlobalFlag(322);
                case 91:
                case 92:
                case 213:
                case 214:
                case 263:
                case 264:
                    Trace.Assert(originalScript == "game.quests[34].state == qs_mentioned or game.quests[34].state == qs_accepted");
                    return GetQuestState(34) == QuestState.Mentioned || GetQuestState(34) == QuestState.Accepted;
                case 93:
                case 94:
                case 163:
                case 164:
                case 185:
                case 186:
                case 203:
                case 204:
                    Trace.Assert(originalScript == "game.quests[60].state == qs_mentioned or game.quests[60].state == qs_accepted");
                    return GetQuestState(60) == QuestState.Mentioned || GetQuestState(60) == QuestState.Accepted;
                case 133:
                case 134:
                case 231:
                case 232:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 7;
                case 183:
                case 184:
                    Trace.Assert(originalScript == "game.quests[60].state == qs_unknown");
                    return GetQuestState(60) == QuestState.Unknown;
                case 235:
                case 236:
                    Trace.Assert(originalScript == "pc.money_get() >= 50000");
                    return pc.GetMoney() >= 50000;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                case 60:
                    Trace.Assert(originalScript == "game.global_flags[356] = 1");
                    SetGlobalFlag(356, true);
                    break;
                case 150:
                case 300:
                    Trace.Assert(originalScript == "game.global_flags[86] = 1");
                    SetGlobalFlag(86, true);
                    break;
                case 161:
                case 162:
                case 261:
                case 262:
                case 281:
                    Trace.Assert(originalScript == "run_off(npc,pc)");
                    run_off(npc, pc);
                    break;
                case 185:
                case 186:
                case 190:
                    Trace.Assert(originalScript == "game.global_flags[357] = 1");
                    SetGlobalFlag(357, true);
                    break;
                case 260:
                case 290:
                    Trace.Assert(originalScript == "npc.item_transfer_to(pc,5815)");
                    npc.TransferItemByNameTo(pc, 5815);
                    break;
                case 263:
                case 264:
                    Trace.Assert(originalScript == "game.global_flags[86] = 1; run_off(npc,pc)");
                    SetGlobalFlag(86, true);
                    run_off(npc, pc);
                    ;
                    break;
                case 271:
                case 272:
                    Trace.Assert(originalScript == "pc.money_adj(-50000); npc.item_transfer_to(pc,5815)");
                    pc.AdjustMoney(-50000);
                    npc.TransferItemByNameTo(pc, 5815);
                    ;
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                case 133:
                case 134:
                case 231:
                case 232:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 7);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
