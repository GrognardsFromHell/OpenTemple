
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
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

namespace VanillaScripts.Dialog;

[DialogScript(131)]
public class OrcPrisoner1Dialog : OrcPrisoner1, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
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
                originalScript = "pc.stat_level_get(stat_level_paladin) == 0";
                return pc.GetStat(Stat.level_paladin) == 0;
            case 25:
            case 26:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 1";
                return pc.GetStat(Stat.level_paladin) == 1;
            case 41:
            case 42:
            case 51:
            case 52:
            case 71:
            case 72:
            case 153:
            case 154:
                originalScript = "game.party_npc_size() <= 1";
                return GameSystems.Party.NPCFollowersSize <= 1;
            case 43:
            case 44:
                originalScript = "game.party_npc_size() == 2";
                return GameSystems.Party.NPCFollowersSize == 2;
            case 45:
            case 46:
                originalScript = "pc.follower_atmax() == 1";
                return pc.HasMaxFollowers();
            case 53:
            case 54:
                originalScript = "game.party_npc_size() >= 2";
                return GameSystems.Party.NPCFollowersSize >= 2;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 6:
            case 7:
            case 8:
            case 9:
            case 23:
            case 24:
                originalScript = "game.global_flags[131] = 1";
                SetGlobalFlag(131, true);
                break;
            case 30:
                originalScript = "game.map_flags( 5066, 1, 1 ); game.map_flags( 5066, 3, 1 ); game.map_flags( 5066, 4, 1 )";
                // FIXME: map_flags;
                // FIXME: map_flags;
                // FIXME: map_flags;
                ;
                break;
            case 131:
            case 181:
                originalScript = "pc.follower_remove( npc )";
                pc.RemoveFollower(npc);
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