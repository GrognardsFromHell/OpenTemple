
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
    [DialogScript(393)]
    public class NightWatchmanDialog : NightWatchman, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                    Trace.Assert(originalScript == "not is_daytime()");
                    return !Utilities.is_daytime();
                case 12:
                    Trace.Assert(originalScript == "is_daytime()");
                    return Utilities.is_daytime();
                case 21:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 9");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 9;
                case 31:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_gather_information) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 10;
                case 41:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 11");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 11;
                case 65:
                case 71:
                case 105:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 12");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 12;
                case 151:
                    Trace.Assert(originalScript == "pc.money_get() >= 1000");
                    return pc.GetMoney() >= 1000;
                case 152:
                    Trace.Assert(originalScript == "pc.money_get() <= 900");
                    return pc.GetMoney() <= 900;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                case 13:
                case 22:
                case 32:
                case 43:
                case 51:
                case 123:
                case 143:
                case 153:
                    Trace.Assert(originalScript == "game.global_flags[260] = 1; game.fade_and_teleport(0,0,0,5121,555,308)");
                    SetGlobalFlag(260, true);
                    FadeAndTeleport(0, 0, 0, 5121, 555, 308);
                    ;
                    break;
                case 30:
                    Trace.Assert(originalScript == "game.global_vars[700] = 1");
                    SetGlobalVar(700, 1);
                    break;
                case 80:
                case 90:
                    Trace.Assert(originalScript == "game.global_flags[937] = 1; game.global_vars[700] = 2");
                    SetGlobalFlag(937, true);
                    SetGlobalVar(700, 2);
                    ;
                    break;
                case 151:
                    Trace.Assert(originalScript == "pc.money_adj(-1000)");
                    pc.AdjustMoney(-1000);
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
                case 21:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 9);
                    return true;
                case 31:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 10);
                    return true;
                case 41:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 11);
                    return true;
                case 65:
                case 71:
                case 105:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 12);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
