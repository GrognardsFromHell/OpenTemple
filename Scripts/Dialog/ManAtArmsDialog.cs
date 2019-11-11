
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
    [DialogScript(65)]
    public class ManAtArmsDialog : ManAtArms, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                    originalScript = "game.global_flags[39] == 1";
                    return GetGlobalFlag(39);
                case 6:
                case 7:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
                case 21:
                case 22:
                    originalScript = "pc.follower_atmax() == 0 and pc.stat_level_get(stat_level_paladin) == 0";
                    return !pc.HasMaxFollowers() && pc.GetStat(Stat.level_paladin) == 0;
                case 25:
                case 26:
                    originalScript = "pc.follower_atmax() == 1";
                    return pc.HasMaxFollowers();
                case 27:
                case 28:
                    originalScript = "pc.follower_atmax() == 0 and pc.stat_level_get(stat_level_paladin) >= 1";
                    return !pc.HasMaxFollowers() && pc.GetStat(Stat.level_paladin) >= 1;
                case 31:
                case 32:
                    originalScript = "npc.area != 1 and npc.area != 3";
                    return npc.GetArea() != 1 && npc.GetArea() != 3;
                case 36:
                case 37:
                    originalScript = "npc.area == 1 or npc.area == 3";
                    return npc.GetArea() == 1 || npc.GetArea() == 3;
                case 38:
                case 39:
                    originalScript = "npc.leader_get() != OBJ_HANDLE_NULL";
                    return npc.GetLeader() != null;
                case 43:
                case 44:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 4";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 4;
                case 501:
                case 502:
                    originalScript = "game.global_flags[868] == 0";
                    return !GetGlobalFlag(868);
                case 561:
                    originalScript = "pc.money_get() >= 11000";
                    return pc.GetMoney() >= 11000;
                case 562:
                case 563:
                    originalScript = "pc.money_get() <= 10999";
                    return pc.GetMoney() <= 10999;
                case 723:
                case 732:
                    originalScript = "game.party_alignment != LAWFUL_NEUTRAL and game.party_alignment != LAWFUL_GOOD";
                    return PartyAlignment != Alignment.LAWFUL_NEUTRAL && PartyAlignment != Alignment.LAWFUL_GOOD;
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
                    originalScript = "game.global_vars[113] = 1";
                    SetGlobalVar(113, 1);
                    break;
                case 8:
                    originalScript = "respawn(npc)";
                    // TODO Co8 had a "respawn()" call here that doesnt exist
                    break;
                case 21:
                case 22:
                    originalScript = "pc.follower_add( npc )";
                    pc.AddFollower(npc);
                    break;
                case 41:
                case 42:
                case 12059:
                    originalScript = "game.global_flags[47] = 1";
                    SetGlobalFlag(47, true);
                    break;
                case 101:
                    originalScript = "pc.follower_remove(npc); run_off(npc,pc)";
                    pc.RemoveFollower(npc);
                    run_off(npc, pc);
                    ;
                    break;
                case 503:
                case 504:
                    originalScript = "game.global_flags[806] = 1";
                    SetGlobalFlag(806, true);
                    break;
                case 550:
                    originalScript = "pc.follower_remove( npc ); pc.follower_add( npc ); game.global_flags[806] = 0";
                    pc.RemoveFollower(npc);
                    pc.AddFollower(npc);
                    SetGlobalFlag(806, false);
                    ;
                    break;
                case 561:
                    originalScript = "game.global_flags[868] = 1; pc.money_adj(-11000); equip_transfer( npc, pc )";
                    SetGlobalFlag(868, true);
                    pc.AdjustMoney(-11000);
                    equip_transfer(npc, pc);
                    ;
                    break;
                case 721:
                case 731:
                    originalScript = "npc.destroy()";
                    npc.Destroy();
                    break;
                case 723:
                case 732:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
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
                case 43:
                case 44:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 4);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
