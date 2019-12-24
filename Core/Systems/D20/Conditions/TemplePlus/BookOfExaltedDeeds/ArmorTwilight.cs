using System;
using System.Collections.Generic;
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
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
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