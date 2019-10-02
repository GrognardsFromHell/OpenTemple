
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
    [DialogScript(131)]
    public class OrcPrisoner1Dialog : OrcPrisoner1, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                case 7:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_race) != race_halforc and pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetRace() != RaceId.half_orc && pc.GetStat(Stat.level_paladin) == 0;
                case 8:
                case 9:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_race) == race_halforc and pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetRace() == RaceId.half_orc && pc.GetStat(Stat.level_paladin) == 0;
                case 21:
                case 22:
                case 23:
                case 24:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetStat(Stat.level_paladin) == 0;
                case 25:
                case 26:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 1");
                    return pc.GetStat(Stat.level_paladin) == 1;
                case 41:
                case 42:
                case 51:
                case 52:
                case 71:
                case 72:
                case 153:
                case 154:
                    Trace.Assert(originalScript == "game.party_npc_size() <= 1");
                    return GameSystems.Party.NPCFollowersSize <= 1;
                case 43:
                case 44:
                    Trace.Assert(originalScript == "game.party_npc_size() == 2");
                    return GameSystems.Party.NPCFollowersSize == 2;
                case 45:
                case 46:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 1");
                    return pc.HasMaxFollowers();
                case 53:
                case 54:
                    Trace.Assert(originalScript == "game.party_npc_size() >= 2");
                    return GameSystems.Party.NPCFollowersSize >= 2;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                case 7:
                case 8:
                case 9:
                case 23:
                case 24:
                    Trace.Assert(originalScript == "game.global_flags[131] = 1");
                    SetGlobalFlag(131, true);
                    break;
                case 30:
                    Trace.Assert(originalScript == "game.map_flags( 5066, 1, 1 ); game.map_flags( 5066, 3, 1 ); game.map_flags( 5066, 4, 1 )");
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    ;
                    break;
                case 131:
                case 181:
                    Trace.Assert(originalScript == "pc.follower_remove( npc )");
                    pc.RemoveFollower(npc);
                    break;
                case 160:
                    Trace.Assert(originalScript == "pc.follower_add(npc)");
                    pc.AddFollower(npc);
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
