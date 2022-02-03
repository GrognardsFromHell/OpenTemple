
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

namespace Scripts.Dialog;

[DialogScript(132)]
public class OrcPrisoner2Dialog : OrcPrisoner2, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 11:
            case 12:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 0";
                return pc.GetStat(Stat.level_paladin) == 0;
            case 13:
            case 14:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 1";
                return pc.GetStat(Stat.level_paladin) == 1;
            case 31:
            case 32:
                originalScript = "pc.stat_level_get(stat_race) != race_halforc";
                return pc.GetRace() != RaceId.half_orc;
            case 33:
            case 34:
                originalScript = "pc.stat_level_get(stat_race) == race_halforc";
                return pc.GetRace() == RaceId.half_orc;
            case 101:
                originalScript = "not anyone( pc.group_list(), \"has_follower\", 14681)";
                return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
            case 102:
                originalScript = "anyone( pc.group_list(), \"has_follower\", 14681)";
                return pc.GetPartyMembers().Any(o => o.HasFollowerByName(14681));
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 1:
                originalScript = "game.global_vars[132] = 1";
                SetGlobalVar(132, 1);
                break;
            case 11:
            case 12:
                originalScript = "game.global_flags[131] = 1";
                SetGlobalFlag(131, true);
                break;
            case 13:
            case 14:
                originalScript = "npc.attack( pc )";
                npc.Attack(pc);
                break;
            case 31:
            case 32:
                originalScript = "tuelk_talk(npc,pc,90)";
                tuelk_talk(npc, pc, 90);
                break;
            case 33:
            case 34:
                originalScript = "tuelk_talk(npc,pc,50)";
                tuelk_talk(npc, pc, 50);
                break;
            case 102:
                originalScript = "ron_talk(npc,pc,880)";
                ron_talk(npc, pc, 880);
                break;
            case 111:
            case 112:
            case 113:
                originalScript = "pc.follower_remove( npc )";
                pc.RemoveFollower(npc);
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