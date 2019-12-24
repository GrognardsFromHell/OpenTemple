
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
    [DialogScript(131)]
    public class OrcPrisoner1Dialog : OrcPrisoner1, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                case 7:
                    originalScript = "pc.stat_level_get(stat_race) != race_halforc and pc.stat_level_get(stat_level_paladin) == 0";
                    return pc.GetRace() != RaceId.half_orc && pc.GetStat(Stat.level_paladin) == 0;
                case 8:
                case 9:
                    originalScript = "pc.stat_level_get(stat_race) == race_halforc and pc.stat_level_get(stat_level_paladin) == 0";
                    return pc.GetRace() == RaceId.half_orc && pc.GetStat(Stat.level_paladin) == 0;
                case 21:
                case 22:
                case 23:
                case 24:
                case 301:
                case 302:
                case 303:
                case 304:
                    originalScript = "pc.stat_level_get(stat_level_paladin) == 0";
                    return pc.GetStat(Stat.level_paladin) == 0;
                case 25:
                case 26:
                case 305:
                case 306:
                    originalScript = "pc.stat_level_get(stat_level_paladin) == 1";
                    return pc.GetStat(Stat.level_paladin) == 1;
                case 27:
                case 28:
                case 31:
                case 32:
                case 93:
                case 94:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 14681)";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
                case 33:
                case 34:
                case 91:
                case 92:
                    originalScript = "not anyone( pc.group_list(), \"has_follower\", 14681)";
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
                case 41:
                    originalScript = "game.party_size() <= 6 and (pc.follower_atmax() == 0) and (not anyone( pc.group_list(), \"has_follower\", 14681))";
                    return GameSystems.Party.PartySize <= 6 && (!pc.HasMaxFollowers()) && (!pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681)));
                case 42:
                    originalScript = "game.party_size() <= 6 and (pc.follower_atmax() == 0) and (anyone( pc.group_list(), \"has_follower\", 14681))";
                    return GameSystems.Party.PartySize <= 6 && (!pc.HasMaxFollowers()) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681)));
                case 43:
                case 44:
                    originalScript = "game.party_size() == 7 and (pc.follower_atmax() == 0)";
                    return GameSystems.Party.PartySize == 7 && (!pc.HasMaxFollowers());
                case 45:
                case 46:
                    originalScript = "pc.follower_atmax() == 1";
                    return pc.HasMaxFollowers();
                case 51:
                case 52:
                    originalScript = "game.party_npc_size() <= 1";
                    return GameSystems.Party.NPCFollowersSize <= 1;
                case 53:
                case 54:
                    originalScript = "game.party_npc_size() >= 2";
                    return GameSystems.Party.NPCFollowersSize >= 2;
                case 71:
                case 72:
                    originalScript = "game.party_size() <= 6 and (pc.follower_atmax() == 0)";
                    return GameSystems.Party.PartySize <= 6 && (!pc.HasMaxFollowers());
                case 153:
                case 154:
                    originalScript = "game.party_size() <= 6 and (pc.follower_atmax() == 0) and not anyone( pc.group_list(), \"has_follower\", 14681)";
                    return GameSystems.Party.PartySize <= 6 && (!pc.HasMaxFollowers()) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
                case 155:
                case 156:
                    originalScript = "game.party_size() <= 6 and (pc.follower_atmax() == 0) and anyone( pc.group_list(), \"has_follower\", 14681) and game.global_vars[692] == 7";
                    return GameSystems.Party.PartySize <= 6 && (!pc.HasMaxFollowers()) && pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681)) && GetGlobalVar(692) == 7;
                case 157:
                case 158:
                    originalScript = "game.party_size() <= 6 and (pc.follower_atmax() == 0) and anyone( pc.group_list(), \"has_follower\", 14681) and game.global_vars[692] != 7";
                    return GameSystems.Party.PartySize <= 6 && (!pc.HasMaxFollowers()) && pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681)) && GetGlobalVar(692) != 7;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    originalScript = "game.global_vars[133] = 1";
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
                    originalScript = "game.global_flags[131] = 1";
                    SetGlobalFlag(131, true);
                    break;
                case 27:
                case 28:
                    originalScript = "argue_ron(npc,pc,750)";
                    argue_ron(npc, pc, 750);
                    break;
                case 30:
                case 310:
                    originalScript = "game.map_flags( 5066, 1, 1 ); game.map_flags( 5066, 3, 1 ); game.map_flags( 5066, 4, 1 )";
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    // FIXME: map_flags;
                    ;
                    break;
                case 31:
                case 32:
                    originalScript = "argue_ron(npc,pc,755)";
                    argue_ron(npc, pc, 755);
                    break;
                case 42:
                    originalScript = "argue_ron(npc,pc,830)";
                    argue_ron(npc, pc, 830);
                    break;
                case 93:
                case 94:
                    originalScript = "argue_ron(npc,pc,770)";
                    argue_ron(npc, pc, 770);
                    break;
                case 131:
                case 181:
                    originalScript = "pc.follower_remove( npc )";
                    pc.RemoveFollower(npc);
                    break;
                case 155:
                case 156:
                    originalScript = "argue_ron(npc,pc,870)";
                    argue_ron(npc, pc, 870);
                    break;
                case 157:
                case 158:
                    originalScript = "argue_ron(npc,pc,850)";
                    argue_ron(npc, pc, 850);
                    break;
                case 160:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
