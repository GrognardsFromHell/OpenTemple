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
}