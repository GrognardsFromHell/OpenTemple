
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

public class TwoWeaponRend
{
    // List of creatures damaged by primary and secondary weapons respectively
    // Rend information would be lost if the game is saved in the middle of a full attack
    // but this shouldn't be a significant problem.

    private static readonly List<GameObject> primaryList = new();
    private static readonly List<GameObject> secondaryList = new();
    public static void TwoWeaponRendBeginRound(in DispatcherCallbackArgs evt)
    {
        // Clear out the list of enemies hit with the primary and secondary weapon
        primaryList.Clear();
        secondaryList.Clear();
    }

    public static void TwoWeaponRendDamageBonus(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoDamage();
        var weaponUsed = dispIo.attackPacket.GetWeaponUsed();
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

        var target = dispIo.attackPacket.victim;
        var bRend = false;
        if (weaponUsed == weaponPrimary)
        {
            // Check if the target has been hit by the secondary weapon already
            if ((secondaryList).Contains(weaponUsed))
            {
                // Rend only once per round.  If the target is not in the primary list then rend
                if (!((primaryList).Contains(weaponUsed)))
                {
                    primaryList.Add(target);
                    bRend = true;
                }

            }
            else
            {
                primaryList.Add(target);
            }

        }
        else if (weaponUsed == weaponSecondary)
        {
            // Check if the target has been hit by the primary weapon already
            if ((primaryList).Contains(target))
            {
                // Rend only once per round.  If the target is not in the secondary list then rend
                if (!((secondaryList).Contains(target)))
                {
                    secondaryList.Add(target);
                    bRend = true;
                }

            }
            else
            {
                secondaryList.Add(target);
            }

        }

        // Note:  Damge can be applied to the primary or secondary weapon depending on the order attacks hit.
        // This is a slight difference from the feat description but it would be a lot of work
        // to get the rend damage type to always match the secondary weapon and it would rarely make any
        // difference.
        if (bRend)
        {
            var damage_dice = Dice.D6;
            // Add 1 and a half times the strength score as the bonus
            var strScore = evt.objHndCaller.GetStat(Stat.strength);
            var strMod = (strScore - 10) / 2;
            damage_dice = damage_dice.WithModifier(strMod + strMod / 2);
            dispIo.damage.AddDamageDice(damage_dice, DamageType.Unspecified, 127);
            evt.objHndCaller.FloatLine("Rend!");
        }

        return;
    }

    // spare, spare
    [FeatCondition("Two-Weapon Rend")]
    [AutoRegister] public static readonly ConditionSpec twoWeaponRend = ConditionSpec.Create("Two-Weapon Rend", 2)
        .SetUnique()
        .AddHandler(DispatcherType.DealingDamage, TwoWeaponRendDamageBonus)
        .AddHandler(DispatcherType.BeginRound, TwoWeaponRendBeginRound)
        .Build();
}