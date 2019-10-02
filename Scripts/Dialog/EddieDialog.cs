
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
    [DialogScript(96)]
    public class EddieDialog : Eddie, IDialogScript
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
                    Trace.Assert(originalScript == "npc.has_met(pc) and (game.quests[21].state <= qs_mentioned or pc.stat_level_get( stat_gender ) == gender_male)");
                    return npc.HasMet(pc) && (GetQuestState(21) <= QuestState.Mentioned || pc.GetGender() == Gender.Male);
                case 6:
                case 7:
                    Trace.Assert(originalScript == "game.quests[21].state == qs_accepted and game.global_vars[9] == 1 and pc.stat_level_get( stat_gender ) == gender_female");
                    return GetQuestState(21) == QuestState.Accepted && GetGlobalVar(9) == 1 && pc.GetGender() == Gender.Female;
                case 8:
                case 9:
                    Trace.Assert(originalScript == "game.quests[21].state == qs_accepted and (game.global_vars[9] == 2 or game.global_vars[9] == 3) and pc.stat_level_get( stat_gender ) == gender_female");
                    return GetQuestState(21) == QuestState.Accepted && (GetGlobalVar(9) == 2 || GetGlobalVar(9) == 3) && pc.GetGender() == Gender.Female;
                case 10:
                case 11:
                    Trace.Assert(originalScript == "game.quests[21].state == qs_accepted and game.global_vars[9] == 4 and pc.stat_level_get( stat_gender ) == gender_female");
                    return GetQuestState(21) == QuestState.Accepted && GetGlobalVar(9) == 4 && pc.GetGender() == Gender.Female;
                case 12:
                case 13:
                    Trace.Assert(originalScript == "game.quests[21].state == qs_accepted and game.global_vars[9] == 5 and pc.stat_level_get( stat_gender ) == gender_female");
                    return GetQuestState(21) == QuestState.Accepted && GetGlobalVar(9) == 5 && pc.GetGender() == Gender.Female;
                case 14:
                case 15:
                    Trace.Assert(originalScript == "game.quests[21].state == qs_accepted and game.global_vars[9] >= 6 and pc.stat_level_get( stat_gender ) == gender_female");
                    return GetQuestState(21) == QuestState.Accepted && GetGlobalVar(9) >= 6 && pc.GetGender() == Gender.Female;
                case 16:
                    Trace.Assert(originalScript == "npc.has_met(pc)");
                    return npc.HasMet(pc);
                case 17:
                case 18:
                    Trace.Assert(originalScript == "game.quests[21].state == qs_completed and game.global_flags[68] == 1");
                    return GetQuestState(21) == QuestState.Completed && GetGlobalFlag(68);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 30:
                    Trace.Assert(originalScript == "game.global_vars[9] = 2");
                    SetGlobalVar(9, 2);
                    break;
                case 90:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,+10)");
                    npc.AdjustReaction(pc, +10);
                    break;
                case 100:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,+20)");
                    npc.AdjustReaction(pc, +20);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}