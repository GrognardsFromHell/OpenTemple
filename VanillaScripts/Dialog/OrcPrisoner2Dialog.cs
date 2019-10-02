
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
    [DialogScript(132)]
    public class OrcPrisoner2Dialog : OrcPrisoner2, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 12:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetStat(Stat.level_paladin) == 0;
                case 13:
                case 14:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 1");
                    return pc.GetStat(Stat.level_paladin) == 1;
                case 31:
                case 32:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_race) != race_half_orc");
                    return pc.GetRace() != RaceId.half_orc;
                case 33:
                case 34:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_race) == race_half_orc");
                    return pc.GetRace() == RaceId.half_orc;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 12:
                    Trace.Assert(originalScript == "game.global_flags[131] = 1");
                    SetGlobalFlag(131, true);
                    break;
                case 13:
                case 14:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 31:
                case 32:
                    Trace.Assert(originalScript == "tuelk_talk(npc,pc,90)");
                    tuelk_talk(npc, pc, 90);
                    break;
                case 33:
                case 34:
                    Trace.Assert(originalScript == "tuelk_talk(npc,pc,50)");
                    tuelk_talk(npc, pc, 50);
                    break;
                case 111:
                case 112:
                case 113:
                    Trace.Assert(originalScript == "pc.follower_remove( npc )");
                    pc.RemoveFollower(npc);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
