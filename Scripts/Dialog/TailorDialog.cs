
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
    [DialogScript(20)]
    public class TailorDialog : Tailor, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 6:
                    Trace.Assert(originalScript == "not npc.has_met( pc )");
                    return !npc.HasMet(pc);
                case 3:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[1].state != qs_completed");
                    return npc.HasMet(pc) && GetQuestState(1) != QuestState.Completed;
                case 4:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[1].state == qs_completed");
                    return npc.HasMet(pc) && GetQuestState(1) == QuestState.Completed;
                case 5:
                case 7:
                    Trace.Assert(originalScript == "game.global_vars[1] == 1");
                    return GetGlobalVar(1) == 1;
                case 8:
                case 9:
                    Trace.Assert(originalScript == "game.global_vars[1] == 0 and game.quests[1].state == qs_completed");
                    return GetGlobalVar(1) == 0 && GetQuestState(1) == QuestState.Completed;
                case 22:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_race) == race_dwarf or pc.stat_level_get(stat_race) == race_halfling or pc.stat_level_get(stat_race) == race_gnome");
                    return pc.GetRace() == RaceId.derro || pc.GetRace() == RaceId.tallfellow || pc.GetRace() == RaceId.svirfneblin;
                case 101:
                case 106:
                    Trace.Assert(originalScript == "game.quests[1].state == qs_unknown");
                    return GetQuestState(1) == QuestState.Unknown;
                case 102:
                case 107:
                    Trace.Assert(originalScript == "game.quests[1].state == qs_mentioned");
                    return GetQuestState(1) == QuestState.Mentioned;
                case 105:
                case 108:
                case 113:
                case 116:
                    Trace.Assert(originalScript == "game.story_state >= 2 and game.areas[3] == 0");
                    return StoryState >= 2 && !IsAreaKnown(3);
                case 114:
                case 117:
                    Trace.Assert(originalScript == "game.global_vars[1] == 3");
                    return GetGlobalVar(1) == 3;
                case 115:
                case 118:
                    Trace.Assert(originalScript == "game.global_vars[1] == 2");
                    return GetGlobalVar(1) == 2;
                case 121:
                case 131:
                    Trace.Assert(originalScript == "game.quests[1].state != qs_completed");
                    return GetQuestState(1) != QuestState.Completed;
                case 122:
                case 132:
                    Trace.Assert(originalScript == "game.quests[1].state == qs_completed");
                    return GetQuestState(1) == QuestState.Completed;
                case 152:
                case 154:
                case 182:
                case 184:
                    Trace.Assert(originalScript == "pc.money_get() >= 2000");
                    return pc.GetMoney() >= 2000;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 5:
                case 7:
                    Trace.Assert(originalScript == "game.global_vars[1] = 2");
                    SetGlobalVar(1, 2);
                    break;
                case 8:
                case 9:
                    Trace.Assert(originalScript == "game.global_vars[1] = 4");
                    SetGlobalVar(1, 4);
                    break;
                case 40:
                    Trace.Assert(originalScript == "game.quests[1].state = qs_mentioned");
                    SetQuestState(1, QuestState.Mentioned);
                    break;
                case 51:
                case 53:
                    Trace.Assert(originalScript == "game.quests[1].state = qs_accepted");
                    SetQuestState(1, QuestState.Accepted);
                    break;
                case 130:
                    Trace.Assert(originalScript == "game.areas[3] = 1; game.story_state = 3");
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 133:
                case 134:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(3)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 152:
                case 154:
                case 182:
                case 184:
                    Trace.Assert(originalScript == "game.global_vars[1] = 3; pc.money_adj(-2000)");
                    SetGlobalVar(1, 3);
                    pc.AdjustMoney(-2000);
                    ;
                    break;
                case 220:
                    Trace.Assert(originalScript == "npc.rotation = 2.3");
                    npc.Rotation = 2.3f;
                    break;
                case 221:
                    Trace.Assert(originalScript == "npc.turn_towards(pc)");
                    npc.TurnTowards(pc);
                    break;
                case 225:
                    Trace.Assert(originalScript == "create_item_in_inventory(4489,pc)");
                    Utilities.create_item_in_inventory(4489, pc);
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
