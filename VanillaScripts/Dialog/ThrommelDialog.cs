
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
    [DialogScript(149)]
    public class ThrommelDialog : Thrommel, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 61:
                case 62:
                    originalScript = "pc.stat_level_get(stat_level_bard) == 0";
                    return pc.GetStat(Stat.level_bard) == 0;
                case 4:
                case 5:
                case 63:
                case 64:
                    originalScript = "pc.stat_level_get(stat_level_bard) >= 1";
                    return pc.GetStat(Stat.level_bard) >= 1;
                case 53:
                case 54:
                case 75:
                case 76:
                    originalScript = "game.party_alignment == LAWFUL_EVIL";
                    return PartyAlignment == Alignment.LAWFUL_EVIL;
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
                    originalScript = "game.global_flags[168] = 1";
                    SetGlobalFlag(168, true);
                    break;
                case 2:
                case 3:
                    originalScript = "check_follower_thrommel_comments(npc,pc)";
                    check_follower_thrommel_comments(npc, pc);
                    break;
                case 81:
                case 82:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 90:
                    originalScript = "npc.item_transfer_to(pc,3014)";
                    npc.TransferItemByNameTo(pc, 3014);
                    break;
                case 101:
                    originalScript = "run_off(npc,pc)";
                    run_off(npc, pc);
                    break;
                case 111:
                case 112:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                case 121:
                case 122:
                case 130:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
                    break;
                case 151:
                    originalScript = "schedule_reward(npc,pc)";
                    schedule_reward(npc, pc);
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
