using System;
using System.Collections.Generic;
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
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class ArmorTwilight
    {
        public static void TwilightSpellFailure(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var is_valid = false;
            var inv_idx = evt.GetConditionArg3();
            if (inv_idx == -1) // inventory tooltip query
            {
                is_valid = true;
            }
            else if (inv_idx == 205) // worn item query in ArmorSpellFailure callback
            {
                var equip_slot = (EquipSlot) dispIo.data2;
                if (equip_slot == EquipSlot.Armor)
                {
                    is_valid = true;
                }
            }

            if (is_valid)
            {
                dispIo.return_val += -10;
            }
        }

        // spare, spare, inv_idx
        [AutoRegister]
        public static readonly ConditionSpec Condition = ConditionSpec.Create("Armor Twilight", 3)
            .SetUnique()
            .AddHandler(DispatcherType.D20Query, D20DispatcherKey.QUE_Get_Arcane_Spell_Failure, TwilightSpellFailure)
            .Build();
    }
}