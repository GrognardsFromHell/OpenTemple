
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
    [DialogScript(149)]
    public class ThrommelDialog : Thrommel, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 61:
                case 62:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_bard) == 0");
                    return pc.GetStat(Stat.level_bard) == 0;
                case 4:
                case 5:
                case 63:
                case 64:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_bard) >= 1");
                    return pc.GetStat(Stat.level_bard) >= 1;
                case 53:
                case 54:
                case 75:
                case 76:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_EVIL");
                    return PartyAlignment == Alignment.LAWFUL_EVIL;
                case 111:
                case 112:
                    Trace.Assert(originalScript == "not pc.follower_atmax()");
                    return !pc.HasMaxFollowers();
                case 113:
                case 114:
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
                case 1:
                    Trace.Assert(originalScript == "game.global_flags[168] = 1");
                    SetGlobalFlag(168, true);
                    break;
                case 2:
                case 3:
                    Trace.Assert(originalScript == "check_follower_thrommel_comments(npc,pc)");
                    check_follower_thrommel_comments(npc, pc);
                    break;
                case 81:
                case 82:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 90:
                    Trace.Assert(originalScript == "npc.item_transfer_to(pc,3014)");
                    npc.TransferItemByNameTo(pc, 3014);
                    break;
                case 101:
                    Trace.Assert(originalScript == "run_off(npc,pc)");
                    run_off(npc, pc);
                    break;
                case 111:
                case 112:
                    Trace.Assert(originalScript == "pc.follower_add(npc)");
                    pc.AddFollower(npc);
                    break;
                case 121:
                case 122:
                case 130:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
                    break;
                case 150:
                    Trace.Assert(originalScript == "equip_transfer2( npc, pc )");
                    equip_transfer2(npc, pc);
                    break;
                case 151:
                    Trace.Assert(originalScript == "schedule_reward(npc,pc)");
                    schedule_reward(npc, pc);
                    break;
                case 22000:
                    Trace.Assert(originalScript == "game.global_vars[907] = 32");
                    SetGlobalVar(907, 32);
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
