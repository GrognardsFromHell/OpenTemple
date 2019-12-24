
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

namespace Scripts.Dialog
{
    [DialogScript(73)]
    public class Innkeeper1Dialog : Innkeeper1, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
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
                    originalScript = "game.quests[97].state == qs_completed";
                    return GetQuestState(97) == QuestState.Completed;
                case 7:
                case 8:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
                case 11:
                case 13:
                case 65:
                case 67:
                    originalScript = "game.global_flags[694] == 0";
                    return !GetGlobalFlag(694);
                case 12:
                case 14:
                case 66:
                case 68:
                    originalScript = "game.global_flags[694] == 1";
                    return GetGlobalFlag(694);
                case 17:
                    originalScript = "(pc.item_find(6318) != OBJ_HANDLE_NULL and pc.stat_level_get( stat_gender ) == gender_female) or (pc.item_find(6320) != OBJ_HANDLE_NULL and pc.stat_level_get( stat_gender ) == gender_female)";
                    return (pc.FindItemByName(6318) != null && pc.GetGender() == Gender.Female) || (pc.FindItemByName(6320) != null && pc.GetGender() == Gender.Female);
                case 21:
                case 22:
                    originalScript = "pc.money_get() >= 200";
                    return pc.GetMoney() >= 200;
                case 23:
                case 24:
                    originalScript = "pc.money_get() <= 200";
                    return pc.GetMoney() <= 200;
                case 101:
                case 103:
                    originalScript = "game.quests[18].state != qs_completed and game.global_flags[694] == 0";
                    return GetQuestState(18) != QuestState.Completed && !GetGlobalFlag(694);
                case 102:
                case 104:
                    originalScript = "game.quests[18].state != qs_completed and game.global_flags[694] == 1";
                    return GetQuestState(18) != QuestState.Completed && GetGlobalFlag(694);
                case 105:
                case 106:
                    originalScript = "game.quests[18].state == qs_mentioned and game.global_flags[58] == 0";
                    return GetQuestState(18) == QuestState.Mentioned && !GetGlobalFlag(58);
                case 107:
                case 108:
                    originalScript = "game.quests[18].state == qs_botched and game.global_flags[58] == 0";
                    return GetQuestState(18) == QuestState.Botched && !GetGlobalFlag(58);
                case 109:
                case 110:
                case 201:
                case 204:
                    originalScript = "game.quests[18].state == qs_unknown";
                    return GetQuestState(18) == QuestState.Unknown;
                case 111:
                case 112:
                    originalScript = "game.quests[18].state == qs_completed";
                    return GetQuestState(18) == QuestState.Completed;
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
                case 202:
                case 205:
                    originalScript = "game.quests[18].state == qs_mentioned";
                    return GetQuestState(18) == QuestState.Mentioned;
                case 351:
                case 361:
                    originalScript = "game.global_vars[525] <= 7";
                    return GetGlobalVar(525) <= 7;
                case 352:
                case 362:
                    originalScript = "game.global_vars[525] >= 8";
                    return GetGlobalVar(525) >= 8;
                case 802:
                case 821:
                    originalScript = "pc.money_get() >= 400";
                    return pc.GetMoney() >= 400;
                case 804:
                case 822:
                    originalScript = "pc.money_get() <= 400";
                    return pc.GetMoney() <= 400;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                case 22:
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
                case 115:
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
                    originalScript = "contest_drink(npc,pc)";
                    contest_drink(npc, pc);
                    break;
                case 290:
                    originalScript = "pc.reputation_add( 66 )";
                    pc.AddReputation(66);
                    break;
                case 291:
                    originalScript = "game.quests[19].state = qs_completed";
                    SetQuestState(19, QuestState.Completed);
                    break;
                case 320:
                    originalScript = "game.sleep_status_update()";
                    GameSystems.RandomEncounter.UpdateSleepStatus();
                    break;
                case 351:
                case 361:
                    originalScript = "game.global_vars[525] = game.global_vars[525] + 1";
                    SetGlobalVar(525, GetGlobalVar(525) + 1);
                    break;
                case 360:
                    originalScript = "create_item_in_inventory(8006,pc)";
                    Utilities.create_item_in_inventory(8006, pc);
                    break;
                case 430:
                case 460:
                    originalScript = "game.picker( npc, spell_remove_disease, can_stay_behind, [ 440, 420, 450 ] )";
                    // FIXME: picker;
                    break;
                case 451:
                case 452:
                    originalScript = "mark_pc_dropoff(picker_obj)";
                    mark_pc_dropoff(PickedObject);
                    break;
                case 802:
                case 821:
                    originalScript = "pc.money_adj(-400)";
                    pc.AdjustMoney(-400);
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
