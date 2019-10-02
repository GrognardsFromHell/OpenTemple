
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
    [DialogScript(14)]
    public class MillerDialog : Miller, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "not npc.has_met(pc)");
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                    Trace.Assert(originalScript == "npc.has_met(pc)");
                    return npc.HasMet(pc);
                case 12:
                case 13:
                    Trace.Assert(originalScript == "game.quests[8].state == qs_accepted and game.global_flags[12] == 0");
                    return GetQuestState(8) == QuestState.Accepted && !GetGlobalFlag(12);
                case 14:
                case 16:
                    Trace.Assert(originalScript == "game.quests[8].state == qs_accepted and game.global_flags[12] == 1 and game.global_flags[13] == 1");
                    return GetQuestState(8) == QuestState.Accepted && GetGlobalFlag(12) && GetGlobalFlag(13);
                case 15:
                case 17:
                    Trace.Assert(originalScript == "game.quests[8].state == qs_accepted and game.global_flags[12] == 1 and game.global_flags[13] == 0 and pc.skill_level_get(npc,skill_bluff) >= 6");
                    return GetQuestState(8) == QuestState.Accepted && GetGlobalFlag(12) && !GetGlobalFlag(13) && pc.GetSkillLevel(npc, SkillId.bluff) >= 6;
                case 18:
                case 19:
                    Trace.Assert(originalScript == "game.quests[10].state == qs_accepted and game.global_flags[15] == 0");
                    return GetQuestState(10) == QuestState.Accepted && !GetGlobalFlag(15);
                case 20:
                case 21:
                    Trace.Assert(originalScript == "game.story_state >= 2 and game.areas[3] == 0");
                    return StoryState >= 2 && !IsAreaKnown(3);
                case 32:
                case 42:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 6");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6;
                case 33:
                case 41:
                case 82:
                case 92:
                case 94:
                case 172:
                case 202:
                case 205:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 10;
                case 34:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 6");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 6;
                case 111:
                case 113:
                    Trace.Assert(originalScript == "game.global_flags[13] == 1");
                    return GetGlobalFlag(13);
                case 112:
                case 114:
                    Trace.Assert(originalScript == "game.global_flags[13] == 0");
                    return !GetGlobalFlag(13);
                case 201:
                case 204:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 8;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 70:
                    Trace.Assert(originalScript == "make_worry( npc, pc )");
                    make_worry(npc, pc);
                    break;
                case 71:
                case 72:
                case 120:
                case 140:
                    Trace.Assert(originalScript == "game.quests[8].state = qs_completed");
                    SetQuestState(8, QuestState.Completed);
                    break;
                case 101:
                case 102:
                    Trace.Assert(originalScript == "game.global_flags[12] = 1");
                    SetGlobalFlag(12, true);
                    break;
                case 220:
                    Trace.Assert(originalScript == "game.global_flags[15] = 1; make_hate( npc, pc )");
                    SetGlobalFlag(15, true);
                    make_hate(npc, pc);
                    ;
                    break;
                case 240:
                    Trace.Assert(originalScript == "game.global_flags[15] = 1");
                    SetGlobalFlag(15, true);
                    break;
                case 260:
                    Trace.Assert(originalScript == "game.areas[3] = 1; game.story_state = 3");
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 261:
                case 262:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(3)");
                    // FIXME: worldmap_travel_by_dialog;
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
                case 15:
                case 17:
                case 34:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 6);
                    return true;
                case 32:
                case 42:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 6);
                    return true;
                case 33:
                case 41:
                case 82:
                case 92:
                case 94:
                case 172:
                case 202:
                case 205:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 10);
                    return true;
                case 201:
                case 204:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 8);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}