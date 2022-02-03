
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

namespace VanillaScripts.Dialog;

[DialogScript(140)]
public class TubalDialog : Tubal, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 21:
            case 221:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_male";
                return pc.GetGender() == Gender.Male;
            case 26:
            case 27:
            case 76:
            case 77:
                originalScript = "game.quests[52].state >= qs_accepted";
                return GetQuestState(52) >= QuestState.Accepted;
            case 31:
            case 32:
            case 47:
            case 48:
                originalScript = "anyone( pc.group_list(), \"has_item\", 2206 )";
                return pc.GetPartyMembers().Any(o => o.HasItemByName(2206));
            case 33:
                originalScript = "game.quests[57].state == qs_completed";
                return GetQuestState(57) == QuestState.Completed;
            case 34:
            case 35:
                originalScript = "game.quests[57].state <= qs_mentioned";
                return GetQuestState(57) <= QuestState.Mentioned;
            case 36:
            case 37:
                originalScript = "game.quests[57].state == qs_accepted and game.global_flags[115] == 0 and game.global_flags[143] == 0";
                return GetQuestState(57) == QuestState.Accepted && !GetGlobalFlag(115) && !GetGlobalFlag(143);
            case 43:
            case 44:
                originalScript = "game.global_flags[115] == 0";
                return !GetGlobalFlag(115);
            case 45:
            case 46:
                originalScript = "game.global_flags[115] == 1";
                return GetGlobalFlag(115);
            case 73:
            case 74:
                originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 9";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 9;
            case 75:
                originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 11";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 11;
            case 133:
            case 134:
                originalScript = "game.global_flags[118] == 0 and ( game.quests[52].state == qs_mentioned or game.quests[52].state == qs_accepted )";
                return !GetGlobalFlag(118) && (GetQuestState(52) == QuestState.Mentioned || GetQuestState(52) == QuestState.Accepted);
            case 135:
            case 136:
                originalScript = "( game.quests[53].state == qs_mentioned or game.quests[53].state == qs_accepted )";
                return (GetQuestState(53) == QuestState.Mentioned || GetQuestState(53) == QuestState.Accepted);
            case 137:
            case 138:
                originalScript = "( game.quests[54].state == qs_mentioned or game.quests[54].state == qs_accepted ) and pc.item_find( 2203 ) == OBJ_HANDLE_NULL";
                return (GetQuestState(54) == QuestState.Mentioned || GetQuestState(54) == QuestState.Accepted) && pc.FindItemByName(2203) == null;
            case 181:
            case 182:
                originalScript = "game.global_flags[141] == 0 and game.global_flags[115] == 0 and pc.skill_level_get(npc, skill_sense_motive) >= 10";
                return !GetGlobalFlag(141) && !GetGlobalFlag(115) && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
            case 183:
            case 184:
                originalScript = "game.global_flags[141] == 1 and game.global_flags[115] == 0";
                return GetGlobalFlag(141) && !GetGlobalFlag(115);
            case 185:
            case 186:
                originalScript = "game.global_flags[115] == 1 and pc.skill_level_get(npc, skill_diplomacy) >= 11 and game.global_flags[107] == 0";
                return GetGlobalFlag(115) && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 11 && !GetGlobalFlag(107);
            case 187:
            case 188:
                originalScript = "game.global_flags[107] == 1";
                return GetGlobalFlag(107);
            case 191:
            case 192:
                originalScript = "game.global_flags[143] == 0";
                return !GetGlobalFlag(143);
            case 201:
            case 202:
            case 281:
            case 282:
                originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 10";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
            case 293:
            case 294:
                originalScript = "pc.skill_level_get(npc, skill_gather_information) >= 10";
                return pc.GetSkillLevel(npc, SkillId.gather_information) >= 10;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 11:
            case 12:
            case 28:
            case 29:
            case 51:
            case 52:
            case 61:
            case 62:
                originalScript = "npc.attack( pc )";
                npc.Attack(pc);
                break;
            case 171:
                originalScript = "party_transfer_to( npc, 2206 ); npc.item_transfer_to_by_proto(pc,6082); game.quests[57].state = qs_completed";
                Utilities.party_transfer_to(npc, 2206);
                npc.TransferItemByProtoTo(pc, 6082);
                SetQuestState(57, QuestState.Completed);
                ;
                break;
            case 240:
                originalScript = "game.quests[57].state = qs_mentioned";
                SetQuestState(57, QuestState.Mentioned);
                break;
            case 241:
            case 242:
                originalScript = "game.quests[57].state = qs_accepted";
                SetQuestState(57, QuestState.Accepted);
                break;
            case 270:
                originalScript = "game.global_flags[141] = 1";
                SetGlobalFlag(141, true);
                break;
            case 300:
                originalScript = "game.global_flags[141] = 1; kill_antonio(npc)";
                SetGlobalFlag(141, true);
                kill_antonio(npc);
                ;
                break;
            case 320:
                originalScript = "kill_alrrem(npc)";
                kill_alrrem(npc);
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
            case 73:
            case 74:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 9);
                return true;
            case 75:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 11);
                return true;
            case 181:
            case 182:
            case 201:
            case 202:
            case 281:
            case 282:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
                return true;
            case 185:
            case 186:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 11);
                return true;
            case 293:
            case 294:
                skillChecks = new DialogSkillChecks(SkillId.gather_information, 10);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}