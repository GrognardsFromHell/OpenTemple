
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
    [DialogScript(39)]
    public class CorlReplacementDialog : CorlReplacement, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 12:
                case 15:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 6");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 6;
                case 13:
                case 16:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 3");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 3;
                case 17:
                case 18:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_druid) >= 1");
                    return pc.GetStat(Stat.level_druid) >= 1;
                case 31:
                case 32:
                    Trace.Assert(originalScript == "game.quests[3].state == qs_accepted");
                    return GetQuestState(3) == QuestState.Accepted;
                case 81:
                case 83:
                    Trace.Assert(originalScript == "game.global_flags[4] == 1");
                    return GetGlobalFlag(4);
                case 82:
                case 84:
                    Trace.Assert(originalScript == "game.global_flags[4] == 0");
                    return !GetGlobalFlag(4);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    Trace.Assert(originalScript == "game.global_flags[6] = 1");
                    SetGlobalFlag(6, true);
                    break;
                case 40:
                    Trace.Assert(originalScript == "game.global_flags[4] = 1");
                    SetGlobalFlag(4, true);
                    break;
                case 71:
                case 72:
                    Trace.Assert(originalScript == "discovered_and_leaves_field(npc,pc)");
                    discovered_and_leaves_field(npc, pc);
                    break;
                case 80:
                    Trace.Assert(originalScript == "turn_back_on2(npc)");
                    turn_back_on2(npc);
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
                case 12:
                case 15:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 6);
                    return true;
                case 13:
                case 16:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 3);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
