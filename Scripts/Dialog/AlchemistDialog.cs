
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
    [DialogScript(477)]
    public class AlchemistDialog : Alchemist, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 32:
                    Trace.Assert(originalScript == "(game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL) and not get_2(npc)");
                    return (PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL) && !Scripts.get_2(npc);
                case 61:
                    Trace.Assert(originalScript == "pc.money_get() >= 75000");
                    return pc.GetMoney() >= 75000;
                case 72:
                    Trace.Assert(originalScript == "pc.item_worn_at(2) == OBJ_HANDLE_NULL");
                    return pc.ItemWornAt(EquipSlot.Gloves) == null;
                case 101:
                    Trace.Assert(originalScript == "not get_1(npc)");
                    return !Scripts.get_1(npc);
                case 103:
                case 141:
                    Trace.Assert(originalScript == "get_2(npc)");
                    return Scripts.get_2(npc);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 45:
                    Trace.Assert(originalScript == "npc_2(npc)");
                    Scripts.npc_2(npc);
                    break;
                case 61:
                    Trace.Assert(originalScript == "pc.money_adj(-75000)");
                    pc.AdjustMoney(-75000);
                    break;
                case 70:
                    Trace.Assert(originalScript == "create_item_in_inventory(8019,pc)");
                    Utilities.create_item_in_inventory(8019, pc);
                    break;
                case 72:
                    Trace.Assert(originalScript == "pc.reflex_save_and_damage( pc, 20, D20_Save_Reduction_Half, D20STD_F_NONE, 4, D20DT_ACID, D20DAP_UNSPECIFIED, D20A_CAST_SPELL, 0 )");
                    pc.ReflexSaveAndDamage(pc, 20, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, Dice.Constant(4), DamageType.Acid, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, 0);
                    break;
                case 101:
                    Trace.Assert(originalScript == "npc_1(npc)");
                    Scripts.npc_1(npc);
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
