
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
    [DialogScript(73)]
    public class InnkeeperDialog : Innkeeper, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "not npc.has_met(pc)");
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                case 203:
                case 206:
                    Trace.Assert(originalScript == "game.quests[18].state == qs_accepted");
                    return GetQuestState(18) == QuestState.Accepted;
                case 6:
                case 7:
                    Trace.Assert(originalScript == "npc.has_met(pc)");
                    return npc.HasMet(pc);
                case 21:
                case 25:
                    Trace.Assert(originalScript == "pc.money_get() >= 200 and game.quests[18].state == qs_unknown and game.global_flags[57] == 0 and game.global_flags[58] == 0");
                    return pc.GetMoney() >= 200 && GetQuestState(18) == QuestState.Unknown && !GetGlobalFlag(57) && !GetGlobalFlag(58);
                case 22:
                case 26:
                    Trace.Assert(originalScript == "pc.money_get() < 200 and game.quests[18].state == qs_unknown and game.global_flags[57] == 0 and game.global_flags[58] == 0");
                    return pc.GetMoney() < 200 && GetQuestState(18) == QuestState.Unknown && !GetGlobalFlag(57) && !GetGlobalFlag(58);
                case 23:
                case 27:
                    Trace.Assert(originalScript == "pc.money_get() >= 200 and (game.quests[18].state > qs_unknown or game.global_flags[57] == 1 or game.global_flags[58] == 1)");
                    return pc.GetMoney() >= 200 && (GetQuestState(18) > QuestState.Unknown || GetGlobalFlag(57) || GetGlobalFlag(58));
                case 24:
                case 28:
                    Trace.Assert(originalScript == "pc.money_get() < 200 and (game.quests[18].state > qs_unknown or game.global_flags[57] == 1 or game.global_flags[58] == 1)");
                    return pc.GetMoney() < 200 && (GetQuestState(18) > QuestState.Unknown || GetGlobalFlag(57) || GetGlobalFlag(58));
                case 65:
                case 66:
                    Trace.Assert(originalScript == "pc.money_get() >= 200");
                    return pc.GetMoney() >= 200;
                case 101:
                case 103:
                    Trace.Assert(originalScript == "game.global_flags[56] == 0 and game.quests[18].state != qs_completed");
                    return !GetGlobalFlag(56) && GetQuestState(18) != QuestState.Completed;
                case 102:
                case 104:
                    Trace.Assert(originalScript == "game.quests[18].state == qs_mentioned and game.global_flags[58] == 0");
                    return GetQuestState(18) == QuestState.Mentioned && !GetGlobalFlag(58);
                case 105:
                case 106:
                    Trace.Assert(originalScript == "game.quests[18].state == qs_botched and game.global_flags[58] == 0");
                    return GetQuestState(18) == QuestState.Botched && !GetGlobalFlag(58);
                case 121:
                case 124:
                    Trace.Assert(originalScript == "game.global_flags[51] == 0 and game.global_flags[58] == 0");
                    return !GetGlobalFlag(51) && !GetGlobalFlag(58);
                case 122:
                case 125:
                    Trace.Assert(originalScript == "game.global_flags[51] == 1");
                    return GetGlobalFlag(51);
                case 123:
                case 126:
                    Trace.Assert(originalScript == "game.global_flags[51] == 0 and game.global_flags[58] == 1");
                    return !GetGlobalFlag(51) && GetGlobalFlag(58);
                case 141:
                case 142:
                    Trace.Assert(originalScript == "game.global_flags[56] == 0");
                    return !GetGlobalFlag(56);
                case 143:
                case 144:
                    Trace.Assert(originalScript == "game.global_flags[56] == 1");
                    return GetGlobalFlag(56);
                case 201:
                case 204:
                    Trace.Assert(originalScript == "game.quests[18].state == qs_unknown");
                    return GetQuestState(18) == QuestState.Unknown;
                case 202:
                case 205:
                    Trace.Assert(originalScript == "game.quests[18].state == qs_mentioned");
                    return GetQuestState(18) == QuestState.Mentioned;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 23:
                case 27:
                case 65:
                case 66:
                    Trace.Assert(originalScript == "pc.money_adj(-200)");
                    pc.AdjustMoney(-200);
                    break;
                case 40:
                case 210:
                    Trace.Assert(originalScript == "game.quests[18].state = qs_mentioned");
                    SetQuestState(18, QuestState.Mentioned);
                    break;
                case 70:
                    Trace.Assert(originalScript == "game.quests[18].state = qs_accepted");
                    SetQuestState(18, QuestState.Accepted);
                    break;
                case 110:
                    Trace.Assert(originalScript == "set_room_flag(npc,pc)");
                    set_room_flag(npc, pc);
                    break;
                case 140:
                    Trace.Assert(originalScript == "game.quests[18].state = qs_completed; npc.reaction_adj( pc,+20)");
                    SetQuestState(18, QuestState.Completed);
                    npc.AdjustReaction(pc, +20);
                    ;
                    break;
                case 150:
                    Trace.Assert(originalScript == "pc.money_adj(200)");
                    pc.AdjustMoney(200);
                    break;
                case 160:
                    Trace.Assert(originalScript == "game.quests[18].state = qs_botched");
                    SetQuestState(18, QuestState.Botched);
                    break;
                case 180:
                    Trace.Assert(originalScript == "game.quests[18].unbotch()");
                    UnbotchQuest(18);
                    break;
                case 230:
                    Trace.Assert(originalScript == "game.quests[19].unbotch(); game.quests[19].state = qs_accepted");
                    UnbotchQuest(19);
                    SetQuestState(19, QuestState.Accepted);
                    ;
                    break;
                case 231:
                    Trace.Assert(originalScript == "contest_who(npc)");
                    contest_who(npc);
                    break;
                case 250:
                    Trace.Assert(originalScript == "game.quests[19].state = qs_botched");
                    SetQuestState(19, QuestState.Botched);
                    break;
                case 261:
                    Trace.Assert(originalScript == "pc.condition_add_with_args(\"Fallen_Paladin\",0,0); contest_drink(npc,pc)");
                    pc.AddCondition("Fallen_Paladin", 0, 0);
                    contest_drink(npc, pc);
                    ;
                    break;
                case 290:
                    Trace.Assert(originalScript == "pc.reputation_add( 20 )");
                    pc.AddReputation(20);
                    break;
                case 291:
                    Trace.Assert(originalScript == "game.quests[19].state = qs_completed");
                    SetQuestState(19, QuestState.Completed);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
