
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

public class DeadlyDefense
{
    // Check if the weapons is usable with finesse
    public static bool IsFinesseWeapon(GameObject creature, GameObject weapon)
    {
        // Unarmed works
        if (weapon == null)
        {
            return true;
        }

        // Ranged weapons don't work
        if (GameSystems.Item.IsRangedWeapon(weapon))
        {
            return false;
        }

        // Light weapon works
        var wieldType = GameSystems.Item.GetWieldType(creature, weapon);
        if ((wieldType == 0))
        {
            return true;
        }

        // Whip, rapier, spiked chain works
        var WeaponType = weapon.GetWeaponType();
        if ((WeaponType == WeaponType.whip) || (WeaponType == WeaponType.spike_chain) || (WeaponType == WeaponType.rapier))
        {
            return true;
        }

        return false;
    }

    private static bool HasLightArmorNoShield(GameObject obj)
    {
        // Light armor or no armor
        if (!obj.IsWearingLightArmorOrLess())
        {
            return false;
        }

        // No Shield
        var shield = obj.ItemWornAt(EquipSlot.Shield);
        return shield == null;
    }

    public static void DeadlyDefenseDamageBonus(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoDamage();
        // Test the check box for fighting defensively only (the ability won't be active yet on the first attack)
        var IsFightingDefensively = evt.objHndCaller.D20Query("Fighting Defensively Checked");
        // Combat Expertise Penalty >= 2 will also trigger the bonus
        var CombatExpertiseValue = (int) GameSystems.D20.D20QueryReturnData(evt.objHndCaller, "Combat Expertise Value");
        if (IsFightingDefensively || (CombatExpertiseValue >= 2))
        {
            var LightOnly = HasLightArmorNoShield(evt.objHndCaller);
            var ValidWeapon = IsFinesseWeapon(evt.objHndCaller, dispIo.attackPacket.GetWeaponUsed());
            // No armor or shield and a weapon finesse usable weapon
            if (LightOnly && ValidWeapon)
            {
                var damage_dice = Dice.D6;
                dispIo.damage.AddDamageDice(damage_dice, DamageType.Unspecified, 127);
            }

        }
    }

    // args are just-in-case placeholders
    [FeatCondition("Deadly Defense")]
    [AutoRegister] public static readonly ConditionSpec Condition = ConditionSpec.Create("Deadly Defense", 2)
        .SetUnique()
        .AddHandler(DispatcherType.DealingDamage, DeadlyDefenseDamageBonus)
        .Build();

}