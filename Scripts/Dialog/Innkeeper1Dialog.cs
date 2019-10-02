
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

namespace Scripts.Dialog
{
    [DialogScript(73)]
    public class Innkeeper1Dialog : Innkeeper1, IDialogScript
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
                    Trace.Assert(originalScript == "game.quests[97].state == qs_completed");
                    return GetQuestState(97) == QuestState.Completed;
                case 7:
                case 8:
                    Trace.Assert(originalScript == "npc.has_met(pc)");
                    return npc.HasMet(pc);
                case 11:
                case 13:
                case 65:
                case 67:
                    Trace.Assert(originalScript == "game.global_flags[694] == 0");
                    return !GetGlobalFlag(694);
                case 12:
                case 14:
                case 66:
                case 68:
                    Trace.Assert(originalScript == "game.global_flags[694] == 1");
                    return GetGlobalFlag(694);
                case 17:
                    Trace.Assert(originalScript == "(pc.item_find(6318) != OBJ_HANDLE_NULL and pc.stat_level_get( stat_gender ) == gender_female) or (pc.item_find(6320) != OBJ_HANDLE_NULL and pc.stat_level_get( stat_gender ) == gender_female)");
                    return (pc.FindItemByName(6318) != null && pc.GetGender() == Gender.Female) || (pc.FindItemByName(6320) != null && pc.GetGender() == Gender.Female);
                case 21:
                case 22:
                    Trace.Assert(originalScript == "pc.money_get() >= 200");
                    return pc.GetMoney() >= 200;
                case 23:
                case 24:
                    Trace.Assert(originalScript == "pc.money_get() <= 200");
                    return pc.GetMoney() <= 200;
                case 101:
                case 103:
                    Trace.Assert(originalScript == "game.quests[18].state != qs_completed and game.global_flags[694] == 0");
                    return GetQuestState(18) != QuestState.Completed && !GetGlobalFlag(694);
                case 102:
                case 104:
                    Trace.Assert(originalScript == "game.quests[18].state != qs_completed and game.global_flags[694] == 1");
                    return GetQuestState(18) != QuestState.Completed && GetGlobalFlag(694);
                case 105:
                case 106:
                    Trace.Assert(originalScript == "game.quests[18].state == qs_mentioned and game.global_flags[58] == 0");
                    return GetQuestState(18) == QuestState.Mentioned && !GetGlobalFlag(58);
                case 107:
                case 108:
                    Trace.Assert(originalScript == "game.quests[18].state == qs_botched and game.global_flags[58] == 0");
                    return GetQuestState(18) == QuestState.Botched && !GetGlobalFlag(58);
                case 109:
                case 110:
                case 201:
                case 204:
                    Trace.Assert(originalScript == "game.quests[18].state == qs_unknown");
                    return GetQuestState(18) == QuestState.Unknown;
                case 111:
                case 112:
                    Trace.Assert(originalScript == "game.quests[18].state == qs_completed");
                    return GetQuestState(18) == QuestState.Completed;
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
                case 202:
                case 205:
                    Trace.Assert(originalScript == "game.quests[18].state == qs_mentioned");
                    return GetQuestState(18) == QuestState.Mentioned;
                case 351:
                case 361:
                    Trace.Assert(originalScript == "game.global_vars[525] <= 7");
                    return GetGlobalVar(525) <= 7;
                case 352:
                case 362:
                    Trace.Assert(originalScript == "game.global_vars[525] >= 8");
                    return GetGlobalVar(525) >= 8;
                case 802:
                case 821:
                    Trace.Assert(originalScript == "pc.money_get() >= 400");
                    return pc.GetMoney() >= 400;
                case 804:
                case 822:
                    Trace.Assert(originalScript == "pc.money_get() <= 400");
                    return pc.GetMoney() <= 400;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                case 22:
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
                case 115:
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
                    Trace.Assert(originalScript == "contest_drink(npc,pc)");
                    contest_drink(npc, pc);
                    break;
                case 290:
                    Trace.Assert(originalScript == "pc.reputation_add( 66 )");
                    pc.AddReputation(66);
                    break;
                case 291:
                    Trace.Assert(originalScript == "game.quests[19].state = qs_completed");
                    SetQuestState(19, QuestState.Completed);
                    break;
                case 320:
                    Trace.Assert(originalScript == "game.sleep_status_update()");
                    GameSystems.RandomEncounter.UpdateSleepStatus();
                    break;
                case 351:
                case 361:
                    Trace.Assert(originalScript == "game.global_vars[525] = game.global_vars[525] + 1");
                    SetGlobalVar(525, GetGlobalVar(525) + 1);
                    break;
                case 360:
                    Trace.Assert(originalScript == "create_item_in_inventory(8006,pc)");
                    Utilities.create_item_in_inventory(8006, pc);
                    break;
                case 430:
                case 460:
                    Trace.Assert(originalScript == "game.picker( npc, spell_remove_disease, can_stay_behind, [ 440, 420, 450 ] )");
                    // FIXME: picker;
                    break;
                case 451:
                case 452:
                    Trace.Assert(originalScript == "mark_pc_dropoff(picker_obj)");
                    mark_pc_dropoff(PickedObject);
                    break;
                case 802:
                case 821:
                    Trace.Assert(originalScript == "pc.money_adj(-400)");
                    pc.AdjustMoney(-400);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillCheck(int lineNumber, out DialogSkillChecks skillChecks)
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
