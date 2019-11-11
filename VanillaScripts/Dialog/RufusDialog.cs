
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
    [DialogScript(18)]
    public class RufusDialog : Rufus, IDialogScript
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
                case 8:
                    originalScript = "game.global_flags[31] == 1 and game.quests[15].state != qs_completed";
                    return GetGlobalFlag(31) && GetQuestState(15) != QuestState.Completed;
                case 51:
                    originalScript = "game.quests[15].state == qs_unknown";
                    return GetQuestState(15) == QuestState.Unknown;
                case 52:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.areas[2] == 0 and game.quests[15].state != qs_unknown";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && !IsAreaKnown(2) && GetQuestState(15) != QuestState.Unknown;
                case 53:
                    originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.areas[2] == 0 and game.quests[15].state != qs_unknown";
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && !IsAreaKnown(2) && GetQuestState(15) != QuestState.Unknown;
                case 82:
                case 86:
                    originalScript = "pc.money_get() >= 1000";
                    return pc.GetMoney() >= 1000;
                case 83:
                case 87:
                    originalScript = "pc.money_get() >= 5000";
                    return pc.GetMoney() >= 5000;
                case 84:
                case 88:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 15";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 15;
                case 102:
                case 107:
                    originalScript = "game.quests[15].state == qs_unknown or game.areas[2] == 0";
                    return GetQuestState(15) == QuestState.Unknown || !IsAreaKnown(2);
                case 103:
                case 108:
                    originalScript = "game.quests[15].state == qs_mentioned";
                    return GetQuestState(15) == QuestState.Mentioned;
                case 104:
                case 109:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.story_state == 0";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && StoryState == 0;
                case 105:
                case 110:
                    originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.story_state == 0";
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && StoryState == 0;
                case 121:
                case 123:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and game.areas[3] == 0";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && !IsAreaKnown(3);
                case 122:
                case 124:
                    originalScript = "(game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL) and game.areas[3] == 0";
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL) && !IsAreaKnown(3);
                case 127:
                case 128:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL) and game.story_state >= 4";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL) && StoryState >= 4;
                case 183:
                case 184:
                    originalScript = "pc.follower_atmax() == 0";
                    return !pc.HasMaxFollowers();
                case 185:
                case 186:
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
                case 60:
                    originalScript = "game.quests[15].state = qs_mentioned";
                    SetQuestState(15, QuestState.Mentioned);
                    break;
                case 61:
                case 62:
                case 103:
                case 108:
                    originalScript = "game.quests[15].state = qs_accepted";
                    SetQuestState(15, QuestState.Accepted);
                    break;
                case 70:
                case 130:
                case 140:
                    originalScript = "game.areas[2] = 1; game.story_state = 1";
                    MakeAreaKnown(2);
                    StoryState = 1;
                    ;
                    break;
                case 73:
                case 74:
                case 131:
                case 132:
                case 141:
                case 142:
                    originalScript = "game.worldmap_travel_by_dialog(2)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 82:
                case 86:
                    originalScript = "pc.money_adj(-1000)";
                    pc.AdjustMoney(-1000);
                    break;
                case 83:
                case 87:
                    originalScript = "pc.money_adj(-5000)";
                    pc.AdjustMoney(-5000);
                    break;
                case 150:
                case 160:
                    originalScript = "game.areas[3] = 1; game.story_state = 3";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 151:
                case 152:
                case 163:
                case 164:
                    originalScript = "game.worldmap_travel_by_dialog(3)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 183:
                case 184:
                    originalScript = "pc.follower_add( npc )";
                    pc.AddFollower(npc);
                    break;
                case 191:
                case 192:
                    originalScript = "game.quests[15].state = qs_completed";
                    SetQuestState(15, QuestState.Completed);
                    break;
                case 200:
                    originalScript = "pc.money_adj(+10000)";
                    pc.AdjustMoney(+10000);
                    break;
                case 221:
                case 231:
                    originalScript = "pc.follower_remove( npc )";
                    pc.RemoveFollower(npc);
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
                case 84:
                case 88:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 15);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
