using OpenTemple.Core.GameObject;
using OpenTemple.Core.Startup.Discovery;

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