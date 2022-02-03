
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
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
    [DialogScript(73)]
    public class InnkeeperDialog : Innkeeper, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                case 203:
                case 206:
                    originalScript = "game.quests[18].state == qs_accepted";
                    return GetQuestState(18) == QuestState.Accepted;
                case 6:
                case 7:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
                case 21:
                case 25:
                    originalScript = "pc.money_get() >= 200 and game.quests[18].state == qs_unknown and game.global_flags[57] == 0 and game.global_flags[58] == 0";
                    return pc.GetMoney() >= 200 && GetQuestState(18) == QuestState.Unknown && !GetGlobalFlag(57) && !GetGlobalFlag(58);
                case 22:
                case 26:
                    originalScript = "pc.money_get() < 200 and game.quests[18].state == qs_unknown and game.global_flags[57] == 0 and game.global_flags[58] == 0";
                    return pc.GetMoney() < 200 && GetQuestState(18) == QuestState.Unknown && !GetGlobalFlag(57) && !GetGlobalFlag(58);
                case 23:
                case 27:
                    originalScript = "pc.money_get() >= 200 and (game.quests[18].state > qs_unknown or game.global_flags[57] == 1 or game.global_flags[58] == 1)";
                    return pc.GetMoney() >= 200 && (GetQuestState(18) > QuestState.Unknown || GetGlobalFlag(57) || GetGlobalFlag(58));
                case 24:
                case 28:
                    originalScript = "pc.money_get() < 200 and (game.quests[18].state > qs_unknown or game.global_flags[57] == 1 or game.global_flags[58] == 1)";
                    return pc.GetMoney() < 200 && (GetQuestState(18) > QuestState.Unknown || GetGlobalFlag(57) || GetGlobalFlag(58));
                case 65:
                case 66:
                    originalScript = "pc.money_get() >= 200";
                    return pc.GetMoney() >= 200;
                case 101:
                case 103:
                    originalScript = "game.global_flags[56] == 0 and game.quests[18].state != qs_completed";
                    return !GetGlobalFlag(56) && GetQuestState(18) != QuestState.Completed;
                case 102:
                case 104:
                    originalScript = "game.quests[18].state == qs_mentioned and game.global_flags[58] == 0";
                    return GetQuestState(18) == QuestState.Mentioned && !GetGlobalFlag(58);
                case 105:
                case 106:
                    originalScript = "game.quests[18].state == qs_botched and game.global_flags[58] == 0";
                    return GetQuestState(18) == QuestState.Botched && !GetGlobalFlag(58);
                case 121:
                case 124:
                    originalScript = "game.global_flags[51] == 0 and game.global_flags[58] == 0";
                    return !GetGlobalFlag(51) && !GetGlobalFlag(58);
                case 122:
                case 125:
                    originalScript = "game.global_flags[51] == 1";
                    return GetGlobalFlag(51);
                case 123:
                case 126:
                    originalScript = "game.global_flags[51] == 0 and game.global_flags[58] == 1";
                    return !GetGlobalFlag(51) && GetGlobalFlag(58);
                case 141:
                case 142:
                    originalScript = "game.global_flags[56] == 0";
                    return !GetGlobalFlag(56);
                case 143:
                case 144:
                    originalScript = "game.global_flags[56] == 1";
                    return GetGlobalFlag(56);
                case 201:
                case 204:
                    originalScript = "game.quests[18].state == qs_unknown";
                    return GetQuestState(18) == QuestState.Unknown;
                case 202:
                case 205:
                    originalScript = "game.quests[18].state == qs_mentioned";
                    return GetQuestState(18) == QuestState.Mentioned;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 23:
                case 27:
                case 65:
                case 66:
                    originalScript = "pc.money_adj(-200)";
                    pc.AdjustMoney(-200);
                    break;
                case 40:
                case 210:
                    originalScript = "game.quests[18].state = qs_mentioned";
                    SetQuestState(18, QuestState.Mentioned);
                    break;
                case 70:
                    originalScript = "game.quests[18].state = qs_accepted";
                    SetQuestState(18, QuestState.Accepted);
                    break;
                case 110:
                    originalScript = "set_room_flag(npc,pc)";
                    set_room_flag(npc, pc);
                    break;
                case 140:
                    originalScript = "game.quests[18].state = qs_completed; npc.reaction_adj( pc,+20)";
                    SetQuestState(18, QuestState.Completed);
                    npc.AdjustReaction(pc, +20);
                    ;
                    break;
                case 150:
                    originalScript = "pc.money_adj(200)";
                    pc.AdjustMoney(200);
                    break;
                case 160:
                    originalScript = "game.quests[18].state = qs_botched";
                    SetQuestState(18, QuestState.Botched);
                    break;
                case 180:
                    originalScript = "game.quests[18].unbotch()";
                    UnbotchQuest(18);
                    break;
                case 230:
                    originalScript = "game.quests[19].unbotch(); game.quests[19].state = qs_accepted";
                    UnbotchQuest(19);
                    SetQuestState(19, QuestState.Accepted);
                    ;
                    break;
                case 231:
                    originalScript = "contest_who(npc)";
                    contest_who(npc);
                    break;
                case 250:
                    originalScript = "game.quests[19].state = qs_botched";
                    SetQuestState(19, QuestState.Botched);
                    break;
                case 261:
                    originalScript = "pc.condition_add_with_args(\"Fallen_Paladin\",0,0); contest_drink(npc,pc)";
                    pc.AddCondition("Fallen_Paladin", 0, 0);
                    contest_drink(npc, pc);
                    ;
                    break;
                case 290:
                    originalScript = "pc.reputation_add( 20 )";
                    pc.AddReputation(20);
                    break;
                case 291:
                    originalScript = "game.quests[19].state = qs_completed";
                    SetQuestState(19, QuestState.Completed);
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
