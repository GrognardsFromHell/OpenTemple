
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
    [DialogScript(158)]
    public class JaerDialog : Jaer, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 41:
                case 42:
                    originalScript = "game.global_flags[171] == 0";
                    return !GetGlobalFlag(171);
                case 61:
                case 62:
                case 91:
                case 92:
                    originalScript = "not pc.follower_atmax()";
                    return !pc.HasMaxFollowers();
                case 63:
                case 64:
                case 93:
                case 94:
                    originalScript = "pc.follower_atmax()";
                    return pc.HasMaxFollowers();
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
                    originalScript = "game.global_vars[121] = 1";
                    SetGlobalVar(121, 1);
                    break;
                case 70:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                case 120:
                case 140:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
                    break;
                case 141:
                    originalScript = "create_item_in_inventory( 4197, pc ); create_item_in_inventory( 4197, pc ); create_item_in_inventory( 4197, pc ); create_item_in_inventory( 4197, pc ); create_item_in_inventory( 4197, pc ); create_item_in_inventory( 4197, pc ); create_item_in_inventory( 4197, pc ); create_item_in_inventory( 4197, pc )";
                    Utilities.create_item_in_inventory(4197, pc);
                    Utilities.create_item_in_inventory(4197, pc);
                    Utilities.create_item_in_inventory(4197, pc);
                    Utilities.create_item_in_inventory(4197, pc);
                    Utilities.create_item_in_inventory(4197, pc);
                    Utilities.create_item_in_inventory(4197, pc);
                    Utilities.create_item_in_inventory(4197, pc);
                    Utilities.create_item_in_inventory(4197, pc);
                    ;
                    break;
                case 150:
                    originalScript = "run_off(npc,pc)";
                    run_off(npc, pc);
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
