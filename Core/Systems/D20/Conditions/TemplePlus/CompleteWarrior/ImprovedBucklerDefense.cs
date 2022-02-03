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

public class ImprovedBucklerDefense
{
    // Signal disables the Buckler penalty on the C++ side if two weapon fighting

    public static void DisableBucklerPenalty(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Query();
        var mainWeapon = evt.objHndCaller.ItemWornAt(EquipSlot.WeaponPrimary);
        var secondaryWeapon = evt.objHndCaller.ItemWornAt(EquipSlot.WeaponSecondary);
        dispIo.return_val = 0;
        // Disable the penalty if two weapon fighting.  It could be argued that this feat should apply to two-handed
        // weapons as well but I am interpreting it as only applying when two weapon fighting.
        if ((mainWeapon != secondaryWeapon) && (mainWeapon != null) && (secondaryWeapon != null))
        {
            if ((mainWeapon.type == ObjectType.weapon) && (secondaryWeapon.type == ObjectType.weapon))
            {
                dispIo.return_val = 1;
            }
        }
    }

    // args are just-in-case placeholders
    [FeatCondition("Improved Buckler Defense")]
    [AutoRegister] public static readonly ConditionSpec improvedBucklerDefense = ConditionSpec
        .Create("Improved Buckler Defense", 2)
        .AddQueryHandler("Disable Buckler Penalty", DisableBucklerPenalty)
        .Build();
}