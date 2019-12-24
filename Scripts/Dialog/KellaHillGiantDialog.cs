
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObject;
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
    [DialogScript(151)]
    public class KellaHillGiantDialog : KellaHillGiant, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 12:
                    originalScript = "pc.stat_level_get(stat_level_druid) == 0";
                    return pc.GetStat(Stat.level_druid) == 0;
                case 13:
                case 14:
                    originalScript = "pc.stat_level_get(stat_level_druid) >= 1";
                    return pc.GetStat(Stat.level_druid) >= 1;
                case 23:
                case 24:
                case 31:
                case 32:
                    originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
                case 41:
                case 42:
                case 71:
                case 72:
                    originalScript = "not pc.follower_atmax()";
                    return !pc.HasMaxFollowers();
                case 43:
                case 44:
                case 73:
                case 74:
                    originalScript = "pc.follower_atmax()";
                    return pc.HasMaxFollowers();
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 10:
                    originalScript = "game.global_vars[137] = 1";
                    SetGlobalVar(137, 1);
                    break;
                case 61:
                    originalScript = "shapechange(npc,pc,1)";
                    shapechange(npc, pc, 1);
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
