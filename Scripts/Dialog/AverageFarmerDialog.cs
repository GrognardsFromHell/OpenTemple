
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
    [DialogScript(3)]
    public class AverageFarmerDialog : AverageFarmer, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 111:
                case 112:
                    originalScript = "game.global_flags[81] == 0";
                    return !GetGlobalFlag(81);
                case 4:
                    originalScript = "game.global_flags[81] == 1 and game.global_vars[4] == 0";
                    return GetGlobalFlag(81) && GetGlobalVar(4) == 0;
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
                case 141:
                case 142:
                case 143:
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
                    originalScript = "game.global_flags[81] == 1";
                    return GetGlobalFlag(81);
                case 171:
                    originalScript = "game.global_flags[1] == 1";
                    return GetGlobalFlag(1);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "game.global_flags[81] = 1";
                    SetGlobalFlag(81, true);
                    break;
                case 96:
                case 191:
                case 192:
                    originalScript = "npc.reaction_adj( pc,+5)";
                    npc.AdjustReaction(pc, +5);
                    break;
                case 115:
                case 116:
                case 123:
                case 124:
                case 133:
                case 134:
                case 137:
                    originalScript = "game.global_vars[4] = 6";
                    SetGlobalVar(4, 6);
                    break;
                case 153:
                case 156:
                case 163:
                    originalScript = "game.global_vars[4] = 2; run_off( npc, pc )";
                    SetGlobalVar(4, 2);
                    run_off(npc, pc);
                    ;
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
                    originalScript = "game.global_vars[4] = 4; run_off( npc, pc )";
                    SetGlobalVar(4, 4);
                    run_off(npc, pc);
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
