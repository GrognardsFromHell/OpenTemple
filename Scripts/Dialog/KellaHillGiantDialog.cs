
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
    [DialogScript(151)]
    public class KellaHillGiantDialog : KellaHillGiant, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 12:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_druid) == 0");
                    return pc.GetStat(Stat.level_druid) == 0;
                case 13:
                case 14:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_druid) >= 1");
                    return pc.GetStat(Stat.level_druid) >= 1;
                case 23:
                case 24:
                case 31:
                case 32:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
                case 41:
                case 42:
                case 71:
                case 72:
                    Trace.Assert(originalScript == "not pc.follower_atmax()");
                    return !pc.HasMaxFollowers();
                case 43:
                case 44:
                case 73:
                case 74:
                    Trace.Assert(originalScript == "pc.follower_atmax()");
                    return pc.HasMaxFollowers();
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 10:
                    Trace.Assert(originalScript == "game.global_vars[137] = 1");
                    SetGlobalVar(137, 1);
                    break;
                case 61:
                    Trace.Assert(originalScript == "shapechange(npc,pc,1)");
                    shapechange(npc, pc, 1);
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
                case 23:
                case 24:
                case 31:
                case 32:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
