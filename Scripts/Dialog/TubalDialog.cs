
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
    [DialogScript(140)]
    public class TubalDialog : Tubal, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                case 221:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_male");
                    return pc.GetGender() == Gender.Male;
                case 26:
                case 27:
                case 76:
                case 77:
                    Trace.Assert(originalScript == "game.quests[52].state >= qs_accepted");
                    return GetQuestState(52) >= QuestState.Accepted;
                case 31:
                case 32:
                case 47:
                case 48:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 2206 )");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(2206));
                case 33:
                    Trace.Assert(originalScript == "game.quests[57].state == qs_completed");
                    return GetQuestState(57) == QuestState.Completed;
                case 34:
                case 35:
                    Trace.Assert(originalScript == "game.quests[57].state <= qs_mentioned");
                    return GetQuestState(57) <= QuestState.Mentioned;
                case 36:
                case 37:
                    Trace.Assert(originalScript == "game.quests[57].state == qs_accepted and game.global_flags[115] == 0 and game.global_flags[143] == 0");
                    return GetQuestState(57) == QuestState.Accepted && !GetGlobalFlag(115) && !GetGlobalFlag(143);
                case 43:
                case 44:
                    Trace.Assert(originalScript == "game.global_flags[115] == 0");
                    return !GetGlobalFlag(115);
                case 45:
                case 46:
                    Trace.Assert(originalScript == "game.global_flags[115] == 1");
                    return GetGlobalFlag(115);
                case 73:
                case 74:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 11");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 11;
                case 75:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 13");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 13;
                case 133:
                case 134:
                    Trace.Assert(originalScript == "game.global_flags[118] == 0 and ( game.quests[52].state == qs_mentioned or game.quests[52].state == qs_accepted )");
                    return !GetGlobalFlag(118) && (GetQuestState(52) == QuestState.Mentioned || GetQuestState(52) == QuestState.Accepted);
                case 135:
                case 136:
                    Trace.Assert(originalScript == "( game.quests[53].state == qs_mentioned or game.quests[53].state == qs_accepted )");
                    return (GetQuestState(53) == QuestState.Mentioned || GetQuestState(53) == QuestState.Accepted);
                case 137:
                case 138:
                    Trace.Assert(originalScript == "( game.quests[54].state == qs_mentioned or game.quests[54].state == qs_accepted ) and pc.item_find( 2203 ) == OBJ_HANDLE_NULL");
                    return (GetQuestState(54) == QuestState.Mentioned || GetQuestState(54) == QuestState.Accepted) && pc.FindItemByName(2203) == null;
                case 181:
                case 182:
                    Trace.Assert(originalScript == "game.global_flags[141] == 0 and game.global_flags[115] == 0 and pc.skill_level_get(npc, skill_sense_motive) >= 12");
                    return !GetGlobalFlag(141) && !GetGlobalFlag(115) && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 12;
                case 183:
                case 184:
                    Trace.Assert(originalScript == "game.global_flags[141] == 1 and game.global_flags[115] == 0");
                    return GetGlobalFlag(141) && !GetGlobalFlag(115);
                case 185:
                case 186:
                    Trace.Assert(originalScript == "game.global_flags[115] == 1 and pc.skill_level_get(npc, skill_diplomacy) >= 13 and game.global_flags[107] == 0");
                    return GetGlobalFlag(115) && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 13 && !GetGlobalFlag(107);
                case 187:
                case 188:
                    Trace.Assert(originalScript == "game.global_flags[107] == 1");
                    return GetGlobalFlag(107);
                case 191:
                case 192:
                    Trace.Assert(originalScript == "game.global_flags[143] == 0");
                    return !GetGlobalFlag(143);
                case 201:
                case 202:
                case 281:
                case 282:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 12");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 12;
                case 293:
                case 294:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_gather_information) >= 12");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 12;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
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
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 171:
                    Trace.Assert(originalScript == "party_transfer_to( npc, 2206 ); npc.item_transfer_to_by_proto(pc,6082); game.quests[57].state = qs_completed");
                    Utilities.party_transfer_to(npc, 2206);
                    npc.TransferItemByProtoTo(pc, 6082);
                    SetQuestState(57, QuestState.Completed);
                    ;
                    break;
                case 240:
                    Trace.Assert(originalScript == "game.quests[57].state = qs_mentioned");
                    SetQuestState(57, QuestState.Mentioned);
                    break;
                case 241:
                case 242:
                    Trace.Assert(originalScript == "game.quests[57].state = qs_accepted");
                    SetQuestState(57, QuestState.Accepted);
                    break;
                case 270:
                    Trace.Assert(originalScript == "game.global_flags[141] = 1");
                    SetGlobalFlag(141, true);
                    break;
                case 300:
                    Trace.Assert(originalScript == "game.global_flags[141] = 1; kill_antonio(npc)");
                    SetGlobalFlag(141, true);
                    kill_antonio(npc);
                    ;
                    break;
                case 320:
                    Trace.Assert(originalScript == "kill_alrrem(npc)");
                    kill_alrrem(npc);
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
                case 73:
                case 74:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 11);
                    return true;
                case 75:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 13);
                    return true;
                case 181:
                case 182:
                case 201:
                case 202:
                case 281:
                case 282:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 12);
                    return true;
                case 185:
                case 186:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 13);
                    return true;
                case 293:
                case 294:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 12);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
