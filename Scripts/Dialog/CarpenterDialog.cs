
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
    [DialogScript(8)]
    public class CarpenterDialog : Carpenter, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    Trace.Assert(originalScript == "not npc.has_met( pc )");
                    return !npc.HasMet(pc);
                case 7:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[5].state <= qs_mentioned");
                    return npc.HasMet(pc) && GetQuestState(5) <= QuestState.Mentioned;
                case 8:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[5].state == qs_accepted");
                    return npc.HasMet(pc) && GetQuestState(5) == QuestState.Accepted;
                case 9:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[5].state == qs_completed");
                    return npc.HasMet(pc) && GetQuestState(5) == QuestState.Completed;
                case 13:
                    Trace.Assert(originalScript == "game.quests[2].state == qs_mentioned or game.quests[2].state == qs_accepted");
                    return GetQuestState(2) == QuestState.Mentioned || GetQuestState(2) == QuestState.Accepted;
                case 14:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_gatherinformation) >= 12");
                    throw new NotSupportedException("Conversion failed.");
                case 15:
                case 16:
                case 103:
                case 106:
                case 112:
                case 113:
                case 141:
                case 142:
                    Trace.Assert(originalScript == "game.global_flags[2] == 1");
                    return GetGlobalFlag(2);
                case 31:
                case 32:
                case 47:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 8;
                case 55:
                    Trace.Assert(originalScript == "game.quests[6].state <= qs_mentioned");
                    return GetQuestState(6) <= QuestState.Mentioned;
                case 56:
                case 57:
                    Trace.Assert(originalScript == "game.quests[6].state == qs_accepted and game.global_flags[2] == 1 and game.quests[5].state == qs_unknown");
                    return GetQuestState(6) == QuestState.Accepted && GetGlobalFlag(2) && GetQuestState(5) == QuestState.Unknown;
                case 81:
                case 82:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 7;
                case 83:
                case 84:
                    Trace.Assert(originalScript == "game.quests[2].state == qs_completed");
                    return GetQuestState(2) == QuestState.Completed;
                case 85:
                    Trace.Assert(originalScript == "pc.money_get() >= 10000");
                    return pc.GetMoney() >= 10000;
                case 101:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_gather_information) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 10;
                case 107:
                case 108:
                    Trace.Assert(originalScript == "game.quests[5].state >= qs_mentioned");
                    return GetQuestState(5) >= QuestState.Mentioned;
                case 151:
                case 154:
                    Trace.Assert(originalScript == "game.global_flags[32] == 1");
                    return GetGlobalFlag(32);
                case 152:
                case 155:
                    Trace.Assert(originalScript == "game.quests[6].state == qs_accepted and game.global_flags[14] == 0");
                    return GetQuestState(6) == QuestState.Accepted && !GetGlobalFlag(14);
                case 153:
                case 156:
                    Trace.Assert(originalScript == "game.story_state >= 2 and game.areas[3] == 0");
                    return StoryState >= 2 && !IsAreaKnown(3);
                case 157:
                case 158:
                    Trace.Assert(originalScript == "game.global_flags[32] == 0");
                    return !GetGlobalFlag(32);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 20:
                case 210:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,-10)");
                    npc.AdjustReaction(pc, -10);
                    break;
                case 60:
                    Trace.Assert(originalScript == "game.quests[5].state = qs_mentioned");
                    SetQuestState(5, QuestState.Mentioned);
                    break;
                case 61:
                    Trace.Assert(originalScript == "buttin(npc,pc,120)");
                    buttin(npc, pc, 120);
                    break;
                case 93:
                    Trace.Assert(originalScript == "pc.reputation_add( 4 )");
                    pc.AddReputation(4);
                    break;
                case 170:
                    Trace.Assert(originalScript == "game.global_flags[14] = 1; npc.reaction_adj( pc,+10)");
                    SetGlobalFlag(14, true);
                    npc.AdjustReaction(pc, +10);
                    ;
                    break;
                case 180:
                    Trace.Assert(originalScript == "game.areas[3] = 1; game.story_state = 3; npc.reaction_adj( pc,+20)");
                    MakeAreaKnown(3);
                    StoryState = 3;
                    npc.AdjustReaction(pc, +20);
                    ;
                    break;
                case 182:
                case 193:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(3)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 190:
                    Trace.Assert(originalScript == "game.areas[3] = 1; game.story_state = 3");
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 200:
                    Trace.Assert(originalScript == "game.global_flags[32] = 1");
                    SetGlobalFlag(32, true);
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
                case 31:
                case 32:
                case 47:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 8);
                    return true;
                case 81:
                case 82:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 7);
                    return true;
                case 101:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
