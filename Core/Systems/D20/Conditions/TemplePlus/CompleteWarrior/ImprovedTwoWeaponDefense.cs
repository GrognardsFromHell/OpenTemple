
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
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

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

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