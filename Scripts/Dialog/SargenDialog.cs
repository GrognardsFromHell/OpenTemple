
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
    [DialogScript(166)]
    public class SargenDialog : Sargen, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                case 22:
                case 23:
                case 24:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetStat(Stat.level_paladin) == 0;
                case 25:
                case 26:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) >= 1");
                    return pc.GetStat(Stat.level_paladin) >= 1;
                case 42:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_female");
                    return pc.GetGender() == Gender.Female;
                case 43:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_male");
                    return pc.GetGender() == Gender.Male;
                case 44:
                case 45:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 10;
                case 51:
                case 52:
                    Trace.Assert(originalScript == "not pc.follower_atmax()");
                    return !pc.HasMaxFollowers();
                case 53:
                case 54:
                    Trace.Assert(originalScript == "pc.follower_atmax()");
                    return pc.HasMaxFollowers();
                case 91:
                case 92:
                    Trace.Assert(originalScript == "game.global_flags[173] == 0 and pc.stat_level_get(stat_level_paladin) == 0");
                    return !GetGlobalFlag(173) && pc.GetStat(Stat.level_paladin) == 0;
                case 93:
                case 94:
                    Trace.Assert(originalScript == "game.global_flags[173] == 1 and pc.stat_level_get(stat_level_paladin) == 0");
                    return GetGlobalFlag(173) && pc.GetStat(Stat.level_paladin) == 0;
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
                    Trace.Assert(originalScript == "game.global_vars[139] = 1");
                    SetGlobalVar(139, 1);
                    break;
                case 60:
                    Trace.Assert(originalScript == "game.global_flags[173] = 1");
                    SetGlobalFlag(173, true);
                    break;
                case 80:
                    Trace.Assert(originalScript == "pc.follower_add(npc)");
                    pc.AddFollower(npc);
                    break;
                case 120:
                case 140:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
                    break;
                case 141:
                case 142:
                    Trace.Assert(originalScript == "schedule_reward(npc,pc)");
                    schedule_reward(npc, pc);
                    break;
                case 150:
                    Trace.Assert(originalScript == "run_off(npc,pc)");
                    run_off(npc, pc);
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
                case 44:
                case 45:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
