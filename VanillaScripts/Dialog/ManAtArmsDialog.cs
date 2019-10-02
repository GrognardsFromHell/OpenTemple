
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

namespace VanillaScripts.Dialog
{
    [DialogScript(65)]
    public class ManAtArmsDialog : ManAtArms, IDialogScript
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
                    Trace.Assert(originalScript == "game.global_flags[39] == 1");
                    return GetGlobalFlag(39);
                case 6:
                case 7:
                    Trace.Assert(originalScript == "npc.has_met(pc)");
                    return npc.HasMet(pc);
                case 21:
                case 22:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0 and pc.stat_level_get(stat_level_paladin) == 0");
                    return !pc.HasMaxFollowers() && pc.GetStat(Stat.level_paladin) == 0;
                case 25:
                case 26:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 1");
                    return pc.HasMaxFollowers();
                case 27:
                case 28:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0 and pc.stat_level_get(stat_level_paladin) >= 1");
                    return !pc.HasMaxFollowers() && pc.GetStat(Stat.level_paladin) >= 1;
                case 31:
                case 32:
                    Trace.Assert(originalScript == "npc.area != 1 and npc.area != 3");
                    return npc.GetArea() != 1 && npc.GetArea() != 3;
                case 36:
                case 37:
                    Trace.Assert(originalScript == "npc.area == 1 or npc.area == 3");
                    return npc.GetArea() == 1 || npc.GetArea() == 3;
                case 43:
                case 44:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 2");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 2;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                case 22:
                    Trace.Assert(originalScript == "pc.follower_add( npc )");
                    pc.AddFollower(npc);
                    break;
                case 41:
                case 42:
                case 12059:
                    Trace.Assert(originalScript == "game.global_flags[47] = 1");
                    SetGlobalFlag(47, true);
                    break;
                case 101:
                    Trace.Assert(originalScript == "pc.follower_remove(npc); run_off(npc,pc)");
                    pc.RemoveFollower(npc);
                    run_off(npc, pc);
                    ;
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
                case 43:
                case 44:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 2);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
