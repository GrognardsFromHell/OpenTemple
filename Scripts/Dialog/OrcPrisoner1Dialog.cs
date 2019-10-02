
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
                case 301:
                case 302:
                case 303:
                case 304:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetStat(Stat.level_paladin) == 0;
                case 25:
                case 26:
                case 305:
                case 306:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 1");
                    return pc.GetStat(Stat.level_paladin) == 1;
                case 27:
                case 28:
                case 31:
                case 32:
                case 93:
                case 94:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_follower\", 14681)");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
                case 33:
                case 34:
                case 91:
                case 92:
                    Trace.Assert(originalScript == "not anyone( pc.group_list(), \"has_follower\", 14681)");
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
                case 41:
                    Trace.Assert(originalScript == "game.party_size() <= 6 and (pc.follower_atmax() == 0) and (not anyone( pc.group_list(), \"has_follower\", 14681))");
                    return GameSystems.Party.PartySize <= 6 && (!pc.HasMaxFollowers()) && (!pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681)));
                case 42:
                    Trace.Assert(originalScript == "game.party_size() <= 6 and (pc.follower_atmax() == 0) and (anyone( pc.group_list(), \"has_follower\", 14681))");
                    return GameSystems.Party.PartySize <= 6 && (!pc.HasMaxFollowers()) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681)));
                case 43:
                case 44:
                    Trace.Assert(originalScript == "game.party_size() == 7 and (pc.follower_atmax() == 0)");
                    return GameSystems.Party.PartySize == 7 && (!pc.HasMaxFollowers());
                case 45:
                case 46:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 1");
                    return pc.HasMaxFollowers();
                case 51:
                case 52:
                    Trace.Assert(originalScript == "game.party_npc_size() <= 1");
                    return GameSystems.Party.NPCFollowersSize <= 1;
                case 53:
                case 54:
                    Trace.Assert(originalScript == "game.party_npc_size() >= 2");
                    return GameSystems.Party.NPCFollowersSize >= 2;
                case 71:
                case 72:
                    Trace.Assert(originalScript == "game.party_size() <= 6 and (pc.follower_atmax() == 0)");
                    return GameSystems.Party.PartySize <= 6 && (!pc.HasMaxFollowers());
                case 153:
                case 154:
                    Trace.Assert(originalScript == "game.party_size() <= 6 and (pc.follower_atmax() == 0) and not anyone( pc.group_list(), \"has_follower\", 14681)");
                    return GameSystems.Party.PartySize <= 6 && (!pc.HasMaxFollowers()) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
                case 155:
                case 156:
                    Trace.Assert(originalScript == "game.party_size() <= 6 and (pc.follower_atmax() == 0) and anyone( pc.group_list(), \"has_follower\", 14681) and game.global_vars[692] == 7");
                    return GameSystems.Party.PartySize <= 6 && (!pc.HasMaxFollowers()) && pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681)) && GetGlobalVar(692) == 7;
                case 157:
                case 158:
                    Trace.Assert(originalScript == "game.party_size() <= 6 and (pc.follower_atmax() == 0) and anyone( pc.group_list(), \"has_follower\", 14681) and game.global_vars[692] != 7");
                    return GameSystems.Party.PartySize <= 6 && (!pc.HasMaxFollowers()) && pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681)) && GetGlobalVar(692) != 7;
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
                    Trace.Assert(originalScript == "game.global_vars[133] = 1");
                    SetGlobalVar(133, 1);
                    break;
                case 6:
                case 7:
                case 8:
                case 9:
                case 23:
                case 24:
                case 303:
                case 304:
                case 311:
                case 312:
                    Trace.Assert(originalScript == "game.global_flags[131] = 1");
                    SetGlobalFlag(131, true);
                    break;
                case 27:
                case 28:
                    Trace.Assert(originalScript == "argue_ron(npc,pc,750)");
                    argue_ron(npc, pc, 750);
                    break;
                case 30:
                case 310:
                    Trace.Assert(originalScript == "game.map_flags( 5066, 1, 1 ); game.map_flags( 5066, 3, 1 ); game.map_flags( 5066, 4, 1 )");
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    ;
                    break;
                case 31:
                case 32:
                    Trace.Assert(originalScript == "argue_ron(npc,pc,755)");
                    argue_ron(npc, pc, 755);
                    break;
                case 42:
                    Trace.Assert(originalScript == "argue_ron(npc,pc,830)");
                    argue_ron(npc, pc, 830);
                    break;
                case 93:
                case 94:
                    Trace.Assert(originalScript == "argue_ron(npc,pc,770)");
                    argue_ron(npc, pc, 770);
                    break;
                case 131:
                case 181:
                    Trace.Assert(originalScript == "pc.follower_remove( npc )");
                    pc.RemoveFollower(npc);
                    break;
                case 155:
                case 156:
                    Trace.Assert(originalScript == "argue_ron(npc,pc,870)");
                    argue_ron(npc, pc, 870);
                    break;
                case 157:
                case 158:
                    Trace.Assert(originalScript == "argue_ron(npc,pc,850)");
                    argue_ron(npc, pc, 850);
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
        public bool TryGetSkillCheck(int lineNumber, out DialogSkillChecks skillChecks)
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
