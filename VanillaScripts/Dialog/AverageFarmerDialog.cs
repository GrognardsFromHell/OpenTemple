
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

namespace VanillaScripts.Dialog
{
    [DialogScript(3)]
    public class AverageFarmerDialog : AverageFarmer, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 111:
                case 112:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 4:
                    originalScript = "npc.has_met(pc) and game.global_vars[4] == 0";
                    return npc.HasMet(pc) && GetGlobalVar(4) == 0;
                case 5:
                case 6:
                    originalScript = "game.global_vars[4] == 4";
                    return GetGlobalVar(4) == 4;
                case 7:
                case 8:
                    originalScript = "game.global_vars[4] == 5";
                    return GetGlobalVar(4) == 5;
                case 23:
                case 24:
                case 33:
                case 34:
                    originalScript = "game.quests[9].state == qs_accepted";
                    return GetQuestState(9) == QuestState.Accepted;
                case 43:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 1";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 1;
                case 84:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 2 and game.quests[9].state == qs_accepted";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 2 && GetQuestState(9) == QuestState.Accepted;
                case 113:
                case 114:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
                case 171:
                    originalScript = "game.global_flags[1] == 1";
                    return GetGlobalFlag(1);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 96:
                case 191:
                case 192:
                    originalScript = "npc.reaction_adj( pc,+5)";
                    npc.AdjustReaction(pc, +5);
                    break;
                case 153:
                case 156:
                case 163:
                    originalScript = "game.global_vars[4] = 2";
                    SetGlobalVar(4, 2);
                    break;
                case 154:
                case 157:
                    originalScript = "game.global_vars[4] = 1; npc.attack( pc )";
                    SetGlobalVar(4, 1);
                    npc.Attack(pc);
                    ;
                    break;
                case 170:
                    originalScript = "pc.money_adj(+5000)";
                    pc.AdjustMoney(+5000);
                    break;
                case 171:
                case 172:
                case 173:
                case 181:
                    originalScript = "game.global_vars[4] = 4";
                    SetGlobalVar(4, 4);
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
                case 43:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 1);
                    return true;
                case 84:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 2);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
