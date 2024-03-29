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

public class RangedWeaponMastery
{
    public static readonly FeatId BaseFeatId = (FeatId) ElfHash.Hash("Ranged Weapon Mastery");

    // Using the same type as melee weapon mastery since those bonuses should not be together
    private static readonly int bon_type_rwm = 856;
    private static readonly int bon_val = 2;

    public static void rwmToHit(in DispatcherCallbackArgs evt, DamageType featDamType)
    {
        var dispIo = evt.GetDispIoAttackBonus();
        var wpn = dispIo.attackPacket.GetWeaponUsed();
        if (wpn == null)
        {
            return;
        }

        var weapDamType = (DamageType) wpn.GetInt32(obj_f.weapon_attacktype);
        // Must be a ranged attack
        if ((dispIo.attackPacket.flags & D20CAF.RANGED) == D20CAF.NONE)
        {
            return;
        }

        if (weapDamType == featDamType || weapDamType == DamageType.SlashingAndBludgeoningAndPiercing)
        {
            dispIo.bonlist.AddBonusFromFeat(bon_val, bon_type_rwm, 114, BaseFeatId);
            return;
        }
        else if (featDamType == DamageType.Bludgeoning &&
                 (weapDamType == DamageType.BludgeoningAndPiercing ||
                  weapDamType == DamageType.SlashingAndBludgeoning))
        {
            dispIo.bonlist.AddBonusFromFeat(bon_val, bon_type_rwm, 114, BaseFeatId);
            return;
        }
        else if (featDamType == DamageType.Slashing &&
                 (weapDamType == DamageType.SlashingAndBludgeoning ||
                  weapDamType == DamageType.PiercingAndSlashing))
        {
            dispIo.bonlist.AddBonusFromFeat(bon_val, bon_type_rwm, 114, BaseFeatId);
            return;
        }
        else if (featDamType == DamageType.Piercing &&
                 (weapDamType == DamageType.BludgeoningAndPiercing ||
                  weapDamType == DamageType.PiercingAndSlashing))
        {
            dispIo.bonlist.AddBonusFromFeat(bon_val, bon_type_rwm, 114, BaseFeatId);
            return;
        }

        return;
    }

    public static void rwmToDam(in DispatcherCallbackArgs evt, DamageType featDamType)
    {
        var dispIo = evt.GetDispIoDamage();
        var wpn = dispIo.attackPacket.GetWeaponUsed();
        if (wpn == null)
        {
            return;
        }

        var weapDamType = (DamageType) wpn.GetInt32(obj_f.weapon_attacktype);
        // Must be a ranged attack
        if ((dispIo.attackPacket.flags & D20CAF.RANGED) == D20CAF.NONE)
        {
            return;
        }

        if (weapDamType == featDamType || weapDamType == DamageType.SlashingAndBludgeoningAndPiercing)
        {
            dispIo.damage.bonuses.AddBonusFromFeat(bon_val, bon_type_rwm, 114, BaseFeatId);
            return;
        }
        else if (featDamType == DamageType.Bludgeoning &&
                 (weapDamType == DamageType.BludgeoningAndPiercing ||
                  weapDamType == DamageType.SlashingAndBludgeoning))
        {
            dispIo.damage.bonuses.AddBonusFromFeat(bon_val, bon_type_rwm, 114, BaseFeatId);
            return;
        }
        else if (featDamType == DamageType.Slashing &&
                 (weapDamType == DamageType.SlashingAndBludgeoning ||
                  weapDamType == DamageType.PiercingAndSlashing))
        {
            dispIo.damage.bonuses.AddBonusFromFeat(bon_val, bon_type_rwm, 114, BaseFeatId);
            return;
        }
        else if (featDamType == DamageType.Piercing &&
                 (weapDamType == DamageType.BludgeoningAndPiercing ||
                  weapDamType == DamageType.PiercingAndSlashing))
        {
            dispIo.damage.bonuses.AddBonusFromFeat(bon_val, bon_type_rwm, 114, BaseFeatId);
            return;
        }

        return;
    }

    public static void rwmRangeIncrementBonus(in DispatcherCallbackArgs evt, DamageType featDamType)
    {
        var dispIo = evt.GetEvtObjRangeIncrementBonus();
        var wpn = dispIo.weaponUsed;
        if (wpn == null)
        {
            return;
        }

        var weapDamType = (DamageType) wpn.GetInt32(obj_f.weapon_attacktype);
        // Add 20 feet to the weapon range
        if (weapDamType == featDamType || weapDamType == DamageType.SlashingAndBludgeoningAndPiercing)
        {
            dispIo.rangeBonus += 20;
        }
        else if (featDamType == DamageType.Bludgeoning &&
                 (weapDamType == DamageType.BludgeoningAndPiercing ||
                  weapDamType == DamageType.SlashingAndBludgeoning))
        {
            dispIo.rangeBonus += 20;
        }
        else if (featDamType == DamageType.Slashing &&
                 (weapDamType == DamageType.SlashingAndBludgeoning ||
                  weapDamType == DamageType.PiercingAndSlashing))
        {
            dispIo.rangeBonus += 20;
        }
        else if (featDamType == DamageType.Piercing &&
                 (weapDamType == DamageType.BludgeoningAndPiercing ||
                  weapDamType == DamageType.PiercingAndSlashing))
        {
            dispIo.rangeBonus += 20;
        }

        return;
    }

    [FeatCondition("Ranged Weapon Mastery - Bludgeoning")]
    [AutoRegister]
    public static readonly ConditionSpec rwmBludg = ConditionSpec.Create("Ranged Weapon Mastery - Bludgeoning", 3, UniquenessType.Unique)
        .Configure(builder => builder
            .AddHandler(DispatcherType.ToHitBonus2, rwmToHit, DamageType.Bludgeoning)
            .AddHandler(DispatcherType.DealingDamage, rwmToDam, DamageType.Bludgeoning)
            .AddHandler(DispatcherType.RangeIncrementBonus, rwmRangeIncrementBonus, DamageType.Bludgeoning)
        );

    [FeatCondition("Ranged Weapon Mastery - Slashing")]
    [AutoRegister]
    public static readonly ConditionSpec rwmSlash = ConditionSpec.Create("Ranged Weapon Mastery - Slashing", 3, UniquenessType.Unique)
        .Configure(builder => builder
            .AddHandler(DispatcherType.ToHitBonus2, rwmToHit, DamageType.Slashing)
            .AddHandler(DispatcherType.DealingDamage, rwmToDam, DamageType.Slashing)
            .AddHandler(DispatcherType.RangeIncrementBonus, rwmRangeIncrementBonus, DamageType.Slashing)
        );

    [FeatCondition("Ranged Weapon Mastery - Piercing")]
    [AutoRegister]
    public static readonly ConditionSpec rwmPierc = ConditionSpec.Create("Ranged Weapon Mastery - Piercing", 3, UniquenessType.Unique)
        .Configure(builder => builder
            .AddHandler(DispatcherType.ToHitBonus2, rwmToHit, DamageType.Piercing)
            .AddHandler(DispatcherType.DealingDamage, rwmToDam, DamageType.Piercing)
            .AddHandler(DispatcherType.RangeIncrementBonus, rwmRangeIncrementBonus, DamageType.Piercing)
        );
}