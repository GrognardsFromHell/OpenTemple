
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
    [DialogScript(596)]
    public class HungousDialog : Hungous, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 11:
                case 21:
                case 31:
                case 41:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 0");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 0;
                case 3:
                case 12:
                case 22:
                case 32:
                case 42:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 0");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 0;
                case 4:
                case 13:
                case 23:
                case 33:
                case 43:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 0");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 0;
                case 5:
                case 14:
                case 24:
                case 34:
                case 44:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_gather_information) >= 0");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 0;
                case 61:
                    Trace.Assert(originalScript == "(game.global_vars[980] == 2 or game.global_vars[981] == 2 or game.global_vars[982] == 2 or game.global_vars[983] == 2 or game.global_vars[984] == 2) and pc.skill_level_get(npc,skill_diplomacy) >= 0");
                    return (GetGlobalVar(980) == 2 || GetGlobalVar(981) == 2 || GetGlobalVar(982) == 2 || GetGlobalVar(983) == 2 || GetGlobalVar(984) == 2) && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 0;
                case 62:
                    Trace.Assert(originalScript == "(game.global_vars[980] == 2 or game.global_vars[981] == 2 or game.global_vars[982] == 2 or game.global_vars[983] == 2 or game.global_vars[984] == 2) and pc.skill_level_get(npc,skill_bluff) >= 0");
                    return (GetGlobalVar(980) == 2 || GetGlobalVar(981) == 2 || GetGlobalVar(982) == 2 || GetGlobalVar(983) == 2 || GetGlobalVar(984) == 2) && pc.GetSkillLevel(npc, SkillId.bluff) >= 0;
                case 63:
                    Trace.Assert(originalScript == "(game.global_vars[980] == 2 or game.global_vars[981] == 2 or game.global_vars[982] == 2 or game.global_vars[983] == 2 or game.global_vars[984] == 2) and pc.skill_level_get(npc,skill_intimidate) >= 0");
                    return (GetGlobalVar(980) == 2 || GetGlobalVar(981) == 2 || GetGlobalVar(982) == 2 || GetGlobalVar(983) == 2 || GetGlobalVar(984) == 2) && pc.GetSkillLevel(npc, SkillId.intimidate) >= 0;
                case 64:
                    Trace.Assert(originalScript == "(game.global_vars[980] == 2 or game.global_vars[981] == 2 or game.global_vars[982] == 2 or game.global_vars[983] == 2 or game.global_vars[984] == 2) and pc.skill_level_get(npc,skill_gather_information) >= 0");
                    return (GetGlobalVar(980) == 2 || GetGlobalVar(981) == 2 || GetGlobalVar(982) == 2 || GetGlobalVar(983) == 2 || GetGlobalVar(984) == 2) && pc.GetSkillLevel(npc, SkillId.gather_information) >= 0;
                case 65:
                    Trace.Assert(originalScript == "game.global_vars[980] != 2 and game.global_vars[981] != 2 and game.global_vars[982] != 2 and game.global_vars[983] != 2 and game.global_vars[984] != 2 and pc.skill_level_get(npc,skill_diplomacy) >= 0");
                    return GetGlobalVar(980) != 2 && GetGlobalVar(981) != 2 && GetGlobalVar(982) != 2 && GetGlobalVar(983) != 2 && GetGlobalVar(984) != 2 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 0;
                case 66:
                    Trace.Assert(originalScript == "game.global_vars[980] != 2 and game.global_vars[981] != 2 and game.global_vars[982] != 2 and game.global_vars[983] != 2 and game.global_vars[984] != 2 and pc.skill_level_get(npc,skill_bluff) >= 0");
                    return GetGlobalVar(980) != 2 && GetGlobalVar(981) != 2 && GetGlobalVar(982) != 2 && GetGlobalVar(983) != 2 && GetGlobalVar(984) != 2 && pc.GetSkillLevel(npc, SkillId.bluff) >= 0;
                case 67:
                    Trace.Assert(originalScript == "game.global_vars[980] != 2 and game.global_vars[981] != 2 and game.global_vars[982] != 2 and game.global_vars[983] != 2 and game.global_vars[984] != 2 and pc.skill_level_get(npc,skill_intimidate) >= 0");
                    return GetGlobalVar(980) != 2 && GetGlobalVar(981) != 2 && GetGlobalVar(982) != 2 && GetGlobalVar(983) != 2 && GetGlobalVar(984) != 2 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 0;
                case 68:
                    Trace.Assert(originalScript == "game.global_vars[980] != 2 and game.global_vars[981] != 2 and game.global_vars[982] != 2 and game.global_vars[983] != 2 and game.global_vars[984] != 2 and pc.skill_level_get(npc,skill_gather_information) >= 0");
                    return GetGlobalVar(980) != 2 && GetGlobalVar(981) != 2 && GetGlobalVar(982) != 2 && GetGlobalVar(983) != 2 && GetGlobalVar(984) != 2 && pc.GetSkillLevel(npc, SkillId.gather_information) >= 0;
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
                    Trace.Assert(originalScript == "game.global_vars[802] = game.global_vars[802] + 1; increment_rep(npc,pc)");
                    SetGlobalVar(802, GetGlobalVar(802) + 1);
                    increment_rep(npc, pc);
                    ;
                    break;
                case 10:
                case 20:
                    Trace.Assert(originalScript == "buff_2(npc,pc)");
                    buff_2(npc, pc);
                    break;
                case 30:
                case 40:
                    Trace.Assert(originalScript == "buff_3(npc,pc)");
                    buff_3(npc, pc);
                    break;
                case 50:
                    Trace.Assert(originalScript == "buff_4(npc,pc)");
                    buff_4(npc, pc);
                    break;
                case 51:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 60:
                    Trace.Assert(originalScript == "buff_1(npc,pc)");
                    buff_1(npc, pc);
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
                case 2:
                case 11:
                case 21:
                case 31:
                case 41:
                case 61:
                case 65:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 0);
                    return true;
                case 3:
                case 12:
                case 22:
                case 32:
                case 42:
                case 62:
                case 66:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 0);
                    return true;
                case 4:
                case 13:
                case 23:
                case 33:
                case 43:
                case 63:
                case 67:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 0);
                    return true;
                case 5:
                case 14:
                case 24:
                case 34:
                case 44:
                case 64:
                case 68:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 0);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
