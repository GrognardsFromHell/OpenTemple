
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
    [DialogScript(25)]
    public class WoodcutterDialog : Woodcutter, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 5:
                    Trace.Assert(originalScript == "not npc.has_met( pc )");
                    return !npc.HasMet(pc);
                case 3:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[2].state != qs_completed and game.global_vars[3] <= 1");
                    return npc.HasMet(pc) && GetQuestState(2) != QuestState.Completed && GetGlobalVar(3) <= 1;
                case 4:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[2].state == qs_completed");
                    return npc.HasMet(pc) && GetQuestState(2) == QuestState.Completed;
                case 6:
                case 7:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[2].state != qs_completed and game.global_vars[3] >= 2");
                    return npc.HasMet(pc) && GetQuestState(2) != QuestState.Completed && GetGlobalVar(3) >= 2;
                case 15:
                case 16:
                    Trace.Assert(originalScript == "game.global_flags[67] == 0");
                    return !GetGlobalFlag(67);
                case 23:
                case 27:
                case 33:
                case 37:
                    Trace.Assert(originalScript == "game.global_vars[3] <= 1");
                    return GetGlobalVar(3) <= 1;
                case 24:
                case 28:
                case 34:
                case 38:
                    Trace.Assert(originalScript == "game.global_vars[3] >= 2");
                    return GetGlobalVar(3) >= 2;
                case 25:
                case 35:
                    Trace.Assert(originalScript == "game.global_flags[1] == 1 and game.global_vars[3] <= 1");
                    return GetGlobalFlag(1) && GetGlobalVar(3) <= 1;
                case 62:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_GOOD");
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 68:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL");
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL;
                case 101:
                case 102:
                case 103:
                case 106:
                    Trace.Assert(originalScript == "game.quests[2].state == qs_unknown");
                    return GetQuestState(2) == QuestState.Unknown;
                case 104:
                case 107:
                    Trace.Assert(originalScript == "game.quests[2].state == qs_mentioned");
                    return GetQuestState(2) == QuestState.Mentioned;
                case 113:
                case 115:
                    Trace.Assert(originalScript == "game.story_state >= 2 and game.areas[3] == 0");
                    return StoryState >= 2 && !IsAreaKnown(3);
                case 192:
                case 193:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_deity ) == 16");
                    return pc.GetStat(Stat.deity) == 16;
                case 501:
                case 510:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD");
                    return PartyAlignment == Alignment.LAWFUL_GOOD;
                case 502:
                case 511:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_GOOD");
                    return PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 503:
                case 512:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_EVIL");
                    return PartyAlignment == Alignment.LAWFUL_EVIL;
                case 504:
                case 513:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 505:
                case 514:
                    Trace.Assert(originalScript == "game.party_alignment == TRUE_NEUTRAL");
                    return PartyAlignment == Alignment.NEUTRAL;
                case 506:
                case 515:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_GOOD");
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 507:
                case 516:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 508:
                case 517:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_NEUTRAL");
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 509:
                case 518:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_NEUTRAL");
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                case 7:
                case 24:
                case 28:
                case 34:
                case 38:
                    Trace.Assert(originalScript == "game.quests[2].state = qs_completed");
                    SetQuestState(2, QuestState.Completed);
                    break;
                case 20:
                case 30:
                    Trace.Assert(originalScript == "game.quests[2].state = qs_mentioned");
                    SetQuestState(2, QuestState.Mentioned);
                    break;
                case 23:
                case 27:
                case 33:
                case 37:
                case 41:
                case 42:
                case 51:
                case 52:
                case 64:
                case 65:
                case 104:
                case 107:
                    Trace.Assert(originalScript == "game.quests[2].state = qs_accepted");
                    SetQuestState(2, QuestState.Accepted);
                    break;
                case 70:
                    Trace.Assert(originalScript == "game.areas[10] = 1");
                    MakeAreaKnown(10);
                    break;
                case 72:
                case 73:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(10)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 140:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,+30)");
                    npc.AdjustReaction(pc, +30);
                    break;
                case 160:
                    Trace.Assert(originalScript == "game.areas[3] = 1; game.story_state = 3");
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 161:
                case 162:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(3)");
                    // FIXME: worldmap_travel_by_dialog;
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
