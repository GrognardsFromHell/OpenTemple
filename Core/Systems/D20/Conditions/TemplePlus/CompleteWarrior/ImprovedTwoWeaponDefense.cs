
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

    public class ImprovedTwoWeaponDefense
    {
        public static void TwoWeaponDefenseAcBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var acBonus = 2;
            var weaponPrimary = evt.objHndCaller.ItemWornAt(EquipSlot.WeaponPrimary);
            var weaponSecondary = evt.objHndCaller.ItemWornAt(EquipSlot.WeaponSecondary);
            // Weapons must not be the same
            if (weaponPrimary == weaponSecondary)
            {
                return;
            }

            // Both hands must have a weapon
            if (weaponPrimary == null || weaponSecondary == null)
            {
                return;
            }

            if (evt.objHndCaller.D20Query(D20DispatcherKey.QUE_FightingDefensively)) // this also covers Total Defense
            {
                acBonus = acBonus * 2;
            }

            dispIo.bonlist.AddBonus(acBonus, 29, "Improved Two-Weapon Defense"); // Shield Bonus
        }
        // args are just-in-case placeholders

        [FeatCondition("Improved Two-Weapon Defense")]
        [AutoRegister] public static readonly ConditionSpec improvedTwoWeaponDefense = ConditionSpec
            .Create("Improved Two-Weapon Defense", 2)
            .SetUnique()
            .AddHandler(DispatcherType.GetAC, TwoWeaponDefenseAcBonus)
            .Build();
    }
}
