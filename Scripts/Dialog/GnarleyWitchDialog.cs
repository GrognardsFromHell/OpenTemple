
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
    [DialogScript(550)]
    public class GnarleyWitchDialog : GnarleyWitch, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) >= 9");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 9;
                case 53:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) >= 12");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 12;
                case 55:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) <= 11");
                    return pc.GetSkillLevel(npc, SkillId.bluff) <= 11;
                case 71:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 8;
                case 103:
                case 152:
                case 162:
                    Trace.Assert(originalScript == "pc.money_get() >= 2000000");
                    return pc.GetMoney() >= 2000000;
                case 151:
                case 161:
                    Trace.Assert(originalScript == "pc.money_get() < 2000000");
                    return pc.GetMoney() < 2000000;
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
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 21:
                case 91:
                case 121:
                    Trace.Assert(originalScript == "npc.runoff(npc.location-3)");
                    npc.RunOff();
                    break;
                case 81:
                    Trace.Assert(originalScript == "create_item_in_inventory(6653,pc)");
                    Utilities.create_item_in_inventory(6653, pc);
                    break;
                case 82:
                case 111:
                    Trace.Assert(originalScript == "create_item_in_inventory(4193,pc)");
                    Utilities.create_item_in_inventory(4193, pc);
                    break;
                case 83:
                case 112:
                    Trace.Assert(originalScript == "create_item_in_inventory(6295,pc)");
                    Utilities.create_item_in_inventory(6295, pc);
                    break;
                case 84:
                case 113:
                    Trace.Assert(originalScript == "create_item_in_inventory(9668,pc)");
                    Utilities.create_item_in_inventory(9668, pc);
                    break;
                case 85:
                case 114:
                    Trace.Assert(originalScript == "create_item_in_inventory(12036,pc); create_item_in_inventory(12034,pc)");
                    Utilities.create_item_in_inventory(12036, pc);
                    Utilities.create_item_in_inventory(12034, pc);
                    ;
                    break;
                case 90:
                    Trace.Assert(originalScript == "game.particles( 'sp-Call Lightning', npc )");
                    AttachParticles("sp-Call Lightning", npc);
                    break;
                case 103:
                case 152:
                case 162:
                    Trace.Assert(originalScript == "pc.money_adj(-2000000); create_item_in_inventory(6655,pc)");
                    pc.AdjustMoney(-2000000);
                    Utilities.create_item_in_inventory(6655, pc);
                    ;
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
                case 4:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 9);
                    return true;
                case 53:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 12);
                    return true;
                case 71:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 8);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
