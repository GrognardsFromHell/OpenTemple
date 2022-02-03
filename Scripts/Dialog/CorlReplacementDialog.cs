
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
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
    [DialogScript(39)]
    public class CorlReplacementDialog : CorlReplacement, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 12:
                case 15:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 6";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 6;
                case 13:
                case 16:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 3";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 3;
                case 17:
                case 18:
                    originalScript = "pc.stat_level_get(stat_level_druid) >= 1";
                    return pc.GetStat(Stat.level_druid) >= 1;
                case 31:
                case 32:
                    originalScript = "game.quests[3].state == qs_accepted";
                    return GetQuestState(3) == QuestState.Accepted;
                case 81:
                case 83:
                    originalScript = "game.global_flags[4] == 1";
                    return GetGlobalFlag(4);
                case 82:
                case 84:
                    originalScript = "game.global_flags[4] == 0";
                    return !GetGlobalFlag(4);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    originalScript = "game.global_flags[6] = 1";
                    SetGlobalFlag(6, true);
                    break;
                case 40:
                    originalScript = "game.global_flags[4] = 1";
                    SetGlobalFlag(4, true);
                    break;
                case 71:
                case 72:
                    originalScript = "discovered_and_leaves_field(npc,pc)";
                    discovered_and_leaves_field(npc, pc);
                    break;
                case 80:
                    originalScript = "turn_back_on2(npc)";
                    turn_back_on2(npc);
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
