
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
    [DialogScript(58)]
    public class WainwrightDialog : Wainwright, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 341:
                case 342:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                    originalScript = "npc.has_met( pc ) and game.story_state <= 1";
                    return npc.HasMet(pc) && StoryState <= 1;
                case 6:
                case 7:
                    originalScript = "game.party_alignment == LAWFUL_GOOD and game.story_state >= 2 and game.areas[3] == 0 and game.global_flags[35] == 1";
                    return PartyAlignment == Alignment.LAWFUL_GOOD && StoryState >= 2 && !IsAreaKnown(3) && GetGlobalFlag(35);
                case 8:
                case 10:
                    originalScript = "(game.party_alignment != LAWFUL_GOOD and game.story_state >= 2) or (game.party_alignment == LAWFUL_GOOD and game.story_state >= 2 and game.global_flags[35] == 0)";
                    return (PartyAlignment != Alignment.LAWFUL_GOOD && StoryState >= 2) || (PartyAlignment == Alignment.LAWFUL_GOOD && StoryState >= 2 && !GetGlobalFlag(35));
                case 11:
                case 12:
                    originalScript = "game.party_alignment == LAWFUL_GOOD and game.areas[3] == 1 and game.global_flags[35] == 1 and game.global_flags[153] == 0";
                    return PartyAlignment == Alignment.LAWFUL_GOOD && IsAreaKnown(3) && GetGlobalFlag(35) && !GetGlobalFlag(153);
                case 13:
                case 14:
                    originalScript = "game.global_flags[153] == 1 and game.quests[20].state != qs_botched";
                    return GetGlobalFlag(153) && GetQuestState(20) != QuestState.Botched;
                case 19:
                case 343:
                case 344:
                case 345:
                case 346:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 21:
                case 22:
                case 351:
                case 352:
                    originalScript = "game.party_alignment == LAWFUL_GOOD";
                    return PartyAlignment == Alignment.LAWFUL_GOOD;
                case 46:
                case 47:
                    originalScript = "game.story_state == 0";
                    return StoryState == 0;
                case 48:
                case 49:
                    originalScript = "game.story_state >= 1";
                    return StoryState >= 1;
                case 71:
                case 72:
                    originalScript = "game.party_alignment == LAWFUL_GOOD and game.global_flags[35] == 0";
                    return PartyAlignment == Alignment.LAWFUL_GOOD && !GetGlobalFlag(35);
                case 81:
                case 82:
                    originalScript = "game.global_flags[35] == 1";
                    return GetGlobalFlag(35);
                case 83:
                case 84:
                    originalScript = "game.global_flags[35] == 0";
                    return !GetGlobalFlag(35);
                case 123:
                case 124:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 127:
                    originalScript = "game.global_flags[153] == 1";
                    return GetGlobalFlag(153);
                case 151:
                case 152:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 153:
                case 154:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 155:
                case 156:
                    originalScript = "game.party_alignment != LAWFUL_GOOD";
                    return PartyAlignment != Alignment.LAWFUL_GOOD;
                case 157:
                case 158:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL;
                case 191:
                case 192:
                    originalScript = "game.story_state == 1";
                    return StoryState == 1;
                case 193:
                case 194:
                    originalScript = "game.story_state >= 2 and game.areas[3] == 0";
                    return StoryState >= 2 && !IsAreaKnown(3);
                case 195:
                case 196:
                    originalScript = "game.areas[3] == 1";
                    return IsAreaKnown(3);
                case 201:
                case 202:
                    originalScript = "game.global_flags[149] == 0";
                    return !GetGlobalFlag(149);
                case 203:
                case 204:
                    originalScript = "game.global_vars[17] == 0 and game.global_flags[160] == 0";
                    return GetGlobalVar(17) == 0 && !GetGlobalFlag(160);
                case 205:
                case 206:
                    originalScript = "game.global_vars[17] >= 1 and game.global_vars[17] <= 4";
                    return GetGlobalVar(17) >= 1 && GetGlobalVar(17) <= 4;
                case 207:
                case 208:
                    originalScript = "game.global_vars[17] >= 5";
                    return GetGlobalVar(17) >= 5;
                case 221:
                case 222:
                    originalScript = "game.quests[20].state != qs_botched";
                    return GetQuestState(20) != QuestState.Botched;
                case 223:
                case 224:
                    originalScript = "game.quests[20].state == qs_botched";
                    return GetQuestState(20) == QuestState.Botched;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                case 7:
                case 35:
                    originalScript = "game.quests[22].state = qs_completed";
                    SetQuestState(22, QuestState.Completed);
                    break;
                case 30:
                    originalScript = "game.global_flags[35] = 1; game.global_flags[67] = 1";
                    SetGlobalFlag(35, true);
                    SetGlobalFlag(67, true);
                    ;
                    break;
                case 40:
                case 120:
                    originalScript = "game.quests[20].state = qs_mentioned";
                    SetQuestState(20, QuestState.Mentioned);
                    break;
                case 50:
                    originalScript = "game.areas[2] = 1; game.story_state = 1";
                    MakeAreaKnown(2);
                    StoryState = 1;
                    ;
                    break;
                case 51:
                case 52:
                case 121:
                case 122:
                    originalScript = "game.quests[20].state = qs_accepted";
                    SetQuestState(20, QuestState.Accepted);
                    break;
                case 53:
                case 54:
                    originalScript = "game.quests[20].state = qs_accepted; game.worldmap_travel_by_dialog(2)";
                    SetQuestState(20, QuestState.Accepted);
                    // FIXME: worldmap_travel_by_dialog;
                    ;
                    break;
                case 110:
                    originalScript = "game.areas[3] = 1; game.story_state = 3";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 111:
                case 112:
                    originalScript = "game.worldmap_travel_by_dialog(3)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 130:
                    originalScript = "make_hate( npc, pc )";
                    make_hate(npc, pc);
                    break;
                case 150:
                    originalScript = "game.global_flags[38] = 1";
                    SetGlobalFlag(38, true);
                    break;
                case 151:
                case 152:
                case 153:
                case 154:
                case 155:
                case 156:
                    originalScript = "game.quests[20].state = qs_completed";
                    SetQuestState(20, QuestState.Completed);
                    break;
                case 157:
                case 158:
                case 221:
                case 222:
                case 310:
                    originalScript = "game.quests[20].state = qs_botched";
                    SetQuestState(20, QuestState.Botched);
                    break;
                case 160:
                    originalScript = "npc.reaction_adj( pc,+20); game.global_flags[160] = 0";
                    npc.AdjustReaction(pc, +20);
                    SetGlobalFlag(160, false);
                    ;
                    break;
                case 171:
                case 172:
                    originalScript = "npc.reaction_adj( pc,+20)";
                    npc.AdjustReaction(pc, +20);
                    break;
                case 173:
                case 174:
                    originalScript = "pc.money_adj(+10000)";
                    pc.AdjustMoney(+10000);
                    break;
                case 181:
                case 182:
                case 331:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 231:
                    originalScript = "create_item_in_inventory(6136,pc); game.global_vars[17] = 1";
                    Utilities.create_item_in_inventory(6136, pc);
                    SetGlobalVar(17, 1);
                    ;
                    break;
                case 251:
                    originalScript = "create_item_in_inventory(6136,pc); game.global_vars[17] = game.global_vars[17] + 1";
                    Utilities.create_item_in_inventory(6136, pc);
                    SetGlobalVar(17, GetGlobalVar(17) + 1);
                    ;
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
