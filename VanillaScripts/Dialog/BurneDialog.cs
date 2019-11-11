
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
    [DialogScript(4)]
    public class BurneDialog : Burne, IDialogScript
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
                    originalScript = "npc.has_met(pc) and game.story_state <= 1";
                    return npc.HasMet(pc) && StoryState <= 1;
                case 5:
                    originalScript = "npc.has_met(pc) and game.story_state >= 2";
                    return npc.HasMet(pc) && StoryState >= 2;
                case 6:
                case 7:
                    originalScript = "game.global_flags[31] == 1 and game.quests[15].state != qs_completed";
                    return GetGlobalFlag(31) && GetQuestState(15) != QuestState.Completed;
                case 14:
                case 18:
                case 52:
                case 55:
                case 181:
                    originalScript = "game.quests[15].state == qs_unknown";
                    return GetQuestState(15) == QuestState.Unknown;
                case 21:
                case 28:
                    originalScript = "game.party_alignment == LAWFUL_GOOD and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.LAWFUL_GOOD && !GetGlobalFlag(67);
                case 22:
                case 29:
                    originalScript = "game.party_alignment == CHAOTIC_GOOD and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.CHAOTIC_GOOD && !GetGlobalFlag(67);
                case 23:
                case 30:
                    originalScript = "game.party_alignment == TRUE_NEUTRAL and game.global_flags[1] == 0";
                    return PartyAlignment == Alignment.NEUTRAL && !GetGlobalFlag(1);
                case 24:
                case 31:
                    originalScript = "(game.party_alignment == NEUTRAL_EVIL or game.party_alignment == NEUTRAL_GOOD) and game.global_flags[67] == 0";
                    return (PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.NEUTRAL_GOOD) && !GetGlobalFlag(67);
                case 25:
                case 32:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL && !GetGlobalFlag(67);
                case 26:
                case 33:
                    originalScript = "game.party_alignment == CHAOTIC_NEUTRAL and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL && !GetGlobalFlag(67);
                case 27:
                case 34:
                    originalScript = "game.party_alignment == LAWFUL_EVIL and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.LAWFUL_EVIL && !GetGlobalFlag(67);
                case 35:
                case 36:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL and game.global_flags[67] == 0";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL && !GetGlobalFlag(67);
                case 37:
                case 38:
                    originalScript = "game.global_flags[67] == 1";
                    return GetGlobalFlag(67);
                case 102:
                case 108:
                    originalScript = "game.quests[15].state == qs_unknown or game.areas[2] == 0";
                    return GetQuestState(15) == QuestState.Unknown || !IsAreaKnown(2);
                case 103:
                case 109:
                    originalScript = "game.quests[15].state == qs_mentioned";
                    return GetQuestState(15) == QuestState.Mentioned;
                case 105:
                case 110:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.story_state == 0";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && StoryState == 0;
                case 106:
                case 111:
                    originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.story_state == 0";
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && StoryState == 0;
                case 112:
                case 113:
                case 186:
                    originalScript = "game.quests[15].state != qs_unknown and game.areas[2] == 1 and game.story_state <= 1";
                    return GetQuestState(15) != QuestState.Unknown && IsAreaKnown(2) && StoryState <= 1;
                case 121:
                case 123:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.areas[3] == 0";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && !IsAreaKnown(3);
                case 122:
                case 124:
                    originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.areas[3] == 0";
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && !IsAreaKnown(3);
                case 128:
                case 129:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL) and game.story_state >= 4 and game.global_flags[195] == 0";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL) && StoryState >= 4 && !GetGlobalFlag(195);
                case 130:
                case 131:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL) and game.story_state >= 4 and game.global_flags[195] == 1";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL) && StoryState >= 4 && GetGlobalFlag(195);
                case 132:
                case 133:
                    originalScript = "game.global_flags[195] == 1";
                    return GetGlobalFlag(195);
                case 134:
                case 135:
                    originalScript = "game.global_flags[195] == 0 and (anyone( pc.group_list(), \"item_find\", 2203 ))";
                    return !GetGlobalFlag(195) && (pc.GetPartyMembers().Any(o => o.FindItemByName(2203) != null));
                case 171:
                case 174:
                    originalScript = "npc.leader_get() == OBJ_HANDLE_NULL";
                    return npc.GetLeader() == null;
                case 172:
                case 173:
                case 175:
                case 176:
                    originalScript = "npc.leader_get() != OBJ_HANDLE_NULL";
                    return npc.GetLeader() != null;
                case 182:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.areas[2] == 0 and game.quests[15].state != qs_unknown";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && !IsAreaKnown(2) && GetQuestState(15) != QuestState.Unknown;
                case 183:
                    originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.areas[2] == 0 and game.quests[15].state != qs_unknown";
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && !IsAreaKnown(2) && GetQuestState(15) != QuestState.Unknown;
                case 184:
                case 185:
                    originalScript = "game.quests[15].state != qs_unknown and game.areas[2] == 1 and game.story_state >= 2";
                    return GetQuestState(15) != QuestState.Unknown && IsAreaKnown(2) && StoryState >= 2;
                case 202:
                case 206:
                    originalScript = "pc.money_get() >= 1000";
                    return pc.GetMoney() >= 1000;
                case 203:
                case 207:
                    originalScript = "pc.money_get() >= 5000";
                    return pc.GetMoney() >= 5000;
                case 204:
                case 208:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 12";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 12;
                case 253:
                case 254:
                    originalScript = "pc.follower_atmax() == 0";
                    return !pc.HasMaxFollowers();
                case 255:
                case 256:
                    originalScript = "pc.follower_atmax() == 1";
                    return pc.HasMaxFollowers();
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 73:
                    originalScript = "game.quests[15].state = qs_mentioned";
                    SetQuestState(15, QuestState.Mentioned);
                    break;
                case 74:
                case 75:
                case 103:
                case 109:
                    originalScript = "game.quests[15].state = qs_accepted";
                    SetQuestState(15, QuestState.Accepted);
                    break;
                case 141:
                case 142:
                    originalScript = "game.quests[15].state = qs_completed";
                    SetQuestState(15, QuestState.Completed);
                    break;
                case 150:
                    originalScript = "pc.money_adj(+5000); npc.reaction_adj( pc,+20)";
                    pc.AdjustMoney(+5000);
                    npc.AdjustReaction(pc, +20);
                    ;
                    break;
                case 190:
                case 210:
                    originalScript = "game.areas[2] = 1; game.story_state = 1";
                    MakeAreaKnown(2);
                    StoryState = 1;
                    ;
                    break;
                case 193:
                case 194:
                case 211:
                case 212:
                case 221:
                case 222:
                    originalScript = "game.worldmap_travel_by_dialog(2)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 202:
                case 206:
                    originalScript = "pc.money_adj(-1000)";
                    pc.AdjustMoney(-1000);
                    break;
                case 203:
                case 207:
                    originalScript = "pc.money_adj(-5000)";
                    pc.AdjustMoney(-5000);
                    break;
                case 220:
                    originalScript = "game.areas[2] = 1; game.story_state = 1; npc.reaction_adj( pc,-10)";
                    MakeAreaKnown(2);
                    StoryState = 1;
                    npc.AdjustReaction(pc, -10);
                    ;
                    break;
                case 230:
                    originalScript = "game.areas[3] = 1; game.story_state = 3";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 231:
                case 232:
                case 261:
                case 262:
                    originalScript = "game.worldmap_travel_by_dialog(3)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 253:
                case 254:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                case 260:
                    originalScript = "game.areas[3] = 1; game.story_state = 3; npc.reaction_adj( pc,+10)";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    npc.AdjustReaction(pc, +10);
                    ;
                    break;
                case 341:
                case 481:
                case 482:
                    originalScript = "pc.follower_remove( npc )";
                    pc.RemoveFollower(npc);
                    break;
                case 420:
                    originalScript = "game.global_flags[358] = 1";
                    SetGlobalFlag(358, true);
                    break;
                case 450:
                case 470:
                    originalScript = "game.global_flags[195] = 1";
                    SetGlobalFlag(195, true);
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
                case 204:
                case 208:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 12);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
