
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
    [DialogScript(2)]
    public class BlackJayDialog : BlackJay, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 10;
                case 4:
                case 5:
                case 11:
                case 12:
                case 21:
                case 22:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 3000 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(3000));
                case 6:
                case 7:
                case 177:
                case 178:
                    originalScript = "game.quests[3].state == qs_mentioned";
                    return GetQuestState(3) == QuestState.Mentioned;
                case 122:
                case 123:
                    originalScript = "game.global_flags[300] == 0";
                    return !GetGlobalFlag(300);
                case 124:
                case 125:
                    originalScript = "game.global_flags[300] == 1";
                    return GetGlobalFlag(300);
                case 131:
                    originalScript = "game.party_alignment & ALIGNMENT_EVIL != 0";
                    return PartyAlignment.IsEvil();
                case 132:
                    originalScript = "game.party_alignment & ALIGNMENT_GOOD != 0";
                    return PartyAlignment.IsGood();
                case 133:
                    originalScript = "game.party_alignment & (ALIGNMENT_GOOD | ALIGNMENT_EVIL) == 0";
                    return (PartyAlignment & (Alignment.NEUTRAL_GOOD | Alignment.NEUTRAL_EVIL)) == 0;
                case 134:
                case 137:
                    originalScript = "game.party_alignment & ALIGNMENT_EVIL != 0 or game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment.IsEvil() || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 135:
                    originalScript = "game.party_alignment & ALIGNMENT_GOOD != 0 or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL";
                    return PartyAlignment.IsGood() || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL;
                case 171:
                case 172:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 3000 ) and game.quests[4].state <= qs_mentioned";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(3000)) && GetQuestState(4) <= QuestState.Mentioned;
                case 173:
                case 174:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 3000 ) and game.quests[4].state == qs_accepted";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(3000)) && GetQuestState(4) == QuestState.Accepted;
                case 175:
                case 176:
                    originalScript = "game.quests[4].state == qs_completed and game.quests[3].state == qs_unknown";
                    return GetQuestState(4) == QuestState.Completed && GetQuestState(3) == QuestState.Unknown;
                case 179:
                case 180:
                    originalScript = "game.quests[3].state == qs_accepted and game.global_flags[4] == 0 and game.global_flags[5] == 0 and game.global_flags[300] == 0";
                    return GetQuestState(3) == QuestState.Accepted && !GetGlobalFlag(4) && !GetGlobalFlag(5) && !GetGlobalFlag(300);
                case 181:
                case 182:
                    originalScript = "game.quests[3].state == qs_accepted and (game.global_flags[4] == 1 or game.global_flags[5] == 1)";
                    return GetQuestState(3) == QuestState.Accepted && (GetGlobalFlag(4) || GetGlobalFlag(5));
                case 183:
                case 184:
                case 351:
                case 352:
                    originalScript = "(game.story_state == 0 or game.story_state == 1) and game.global_flags[67] == 0 and game.party_alignment == CHAOTIC_GOOD";
                    return (StoryState == 0 || StoryState == 1) && !GetGlobalFlag(67) && PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 185:
                case 186:
                case 355:
                case 356:
                    originalScript = "game.story_state >= 2 and game.global_flags[20] == 0 and game.party_alignment == CHAOTIC_GOOD";
                    return StoryState >= 2 && !GetGlobalFlag(20) && PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 187:
                case 188:
                case 357:
                case 358:
                    originalScript = "game.global_flags[20] == 1 and game.party_alignment == CHAOTIC_GOOD";
                    return GetGlobalFlag(20) && PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 189:
                case 190:
                    originalScript = "game.quests[3].state == qs_accepted and game.global_flags[4] == 0 and game.global_flags[5] == 0 and game.global_flags[300] == 1";
                    return GetQuestState(3) == QuestState.Accepted && !GetGlobalFlag(4) && !GetGlobalFlag(5) && GetGlobalFlag(300);
                case 201:
                case 202:
                    originalScript = "pc.item_find(5800) != OBJ_HANDLE_NULL and game.global_flags[3] == 0 and pc.stat_level_get(stat_level_druid) == 0";
                    return pc.FindItemByName(5800) != null && !GetGlobalFlag(3) && pc.GetStat(Stat.level_druid) == 0;
                case 203:
                case 204:
                case 251:
                case 252:
                    originalScript = "game.global_flags[3] == 1";
                    return GetGlobalFlag(3);
                case 205:
                case 206:
                    originalScript = "pc.item_find(5800) != OBJ_HANDLE_NULL and pc.stat_level_get(stat_level_druid) >= 1";
                    return pc.FindItemByName(5800) != null && pc.GetStat(Stat.level_druid) >= 1;
                case 253:
                    originalScript = "game.global_flags[3] == 0";
                    return !GetGlobalFlag(3);
                case 261:
                case 262:
                    originalScript = "game.global_flags[4] == 1";
                    return GetGlobalFlag(4);
                case 263:
                case 264:
                    originalScript = "game.global_flags[5] == 1";
                    return GetGlobalFlag(5);
                case 331:
                case 332:
                    originalScript = "game.areas[3] == 0";
                    return !IsAreaKnown(3);
                case 333:
                case 334:
                case 335:
                    originalScript = "game.areas[3] == 1";
                    return IsAreaKnown(3);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 90:
                    originalScript = "game.quests[3].state = qs_mentioned";
                    SetQuestState(3, QuestState.Mentioned);
                    break;
                case 110:
                    originalScript = "game.quests[3].state = qs_accepted";
                    SetQuestState(3, QuestState.Accepted);
                    break;
                case 140:
                    originalScript = "game.quests[4].state = qs_accepted";
                    SetQuestState(4, QuestState.Accepted);
                    break;
                case 141:
                    originalScript = "party_transfer_to( npc, 3000 ); npc.item_transfer_to_by_proto(pc,5006); game.quests[4].state = qs_completed; npc.reaction_adj( pc,15)";
                    Utilities.party_transfer_to(npc, 3000);
                    npc.TransferItemByProtoTo(pc, 5006);
                    SetQuestState(4, QuestState.Completed);
                    npc.AdjustReaction(pc, 15);
                    ;
                    break;
                case 142:
                    originalScript = "party_transfer_to( npc, 3000 ); npc.identify_all(); npc.item_transfer_to_by_proto(pc,6057); game.quests[4].state = qs_completed; npc.reaction_adj( pc,15)";
                    Utilities.party_transfer_to(npc, 3000);
                    npc.IdentifyAll();
                    npc.TransferItemByProtoTo(pc, 6057);
                    SetQuestState(4, QuestState.Completed);
                    npc.AdjustReaction(pc, 15);
                    ;
                    break;
                case 143:
                    originalScript = "party_transfer_to( npc, 3000 ); npc.identify_all(); npc.item_transfer_to_by_proto(pc,6058); game.quests[4].state = qs_completed; npc.reaction_adj( pc,15)";
                    Utilities.party_transfer_to(npc, 3000);
                    npc.IdentifyAll();
                    npc.TransferItemByProtoTo(pc, 6058);
                    SetQuestState(4, QuestState.Completed);
                    npc.AdjustReaction(pc, 15);
                    ;
                    break;
                case 144:
                    originalScript = "party_transfer_to( npc, 3000 ); game.quests[4].state = qs_completed; npc.reaction_adj( pc,30)";
                    Utilities.party_transfer_to(npc, 3000);
                    SetQuestState(4, QuestState.Completed);
                    npc.AdjustReaction(pc, 30);
                    ;
                    break;
                case 146:
                    originalScript = "pc.item_transfer_to(npc,3000); game.quests[4].state = qs_completed; npc.reaction_adj( pc,30)";
                    pc.TransferItemByNameTo(npc, 3000);
                    SetQuestState(4, QuestState.Completed);
                    npc.AdjustReaction(pc, 30);
                    ;
                    break;
                case 300:
                case 310:
                    originalScript = "game.quests[3].state = qs_completed";
                    SetQuestState(3, QuestState.Completed);
                    break;
                case 323:
                    originalScript = "game.global_flags[67] = 1";
                    SetGlobalFlag(67, true);
                    break;
                case 326:
                    originalScript = "game.areas[2] = 1; game.story_state = 1";
                    MakeAreaKnown(2);
                    StoryState = 1;
                    ;
                    break;
                case 327:
                case 328:
                    originalScript = "game.worldmap_travel_by_dialog(2)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 333:
                case 334:
                case 337:
                case 338:
                    originalScript = "game.worldmap_travel_by_dialog(3)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 336:
                    originalScript = "game.areas[3] = 1; game.story_state = 3";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 340:
                    originalScript = "game.quests[24].state = qs_completed";
                    SetQuestState(24, QuestState.Completed);
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
                case 2:
                case 3:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
