using System;
using System.Collections.Generic;
using System.IO;

namespace OpenTemple.Core.Systems.D20;

public enum DamageType
{
    Unspecified = -1,
    Bludgeoning = 0,
    Piercing = 1,
    Slashing = 2,
    BludgeoningAndPiercing = 3,
    PiercingAndSlashing = 4,
    SlashingAndBludgeoning = 5,
    SlashingAndBludgeoningAndPiercing = 6,
    Acid = 7,
    Cold = 8,
    Electricity = 9,
    Fire = 10,
    Sonic = 11,
    NegativeEnergy = 12,
    Subdual = 13,
    Poison = 14,
    PositiveEnergy = 15,
    Force = 16,
    BloodLoss = 17,
    Magic = 18
}

public static class DamageTypes
{
    /// <summary>
    /// Used by both prototypes and trap definition files.
    /// </summary>
    [TempleDllLocation(0x102e3bc4)] public static readonly Dictionary<string, DamageType> NameToDamageType =
        new()
        {
            {"Bludgeoning", DamageType.Bludgeoning},
            {"Piercing", DamageType.Piercing},
            {"Slashing", DamageType.Slashing},
            {"Bludgeoning and Piercing", DamageType.BludgeoningAndPiercing},
            {"Piercing and Slashing", DamageType.PiercingAndSlashing},
            {"Slashing and Bludgeoning", DamageType.SlashingAndBludgeoning},
            {"Slashing and Bludgeoning and Piercing", DamageType.SlashingAndBludgeoningAndPiercing},
            {"Acid", DamageType.Acid},
            {"Cold", DamageType.Cold},
            {"Electricity", DamageType.Electricity},
            {"Fire", DamageType.Fire},
            {"Sonic", DamageType.Sonic},
            {"Negative Energy", DamageType.NegativeEnergy},
            {"Subdual", DamageType.Subdual},
            {"Poison", DamageType.Poison},
            {"Positive Energy", DamageType.PositiveEnergy},
            {"Force", DamageType.Force},
            {"Blood loss", DamageType.BloodLoss},
            {"Magic", DamageType.Magic},
        };

    public static DamageType GetDamageTypeByName(string name)
    {
        foreach (var (key, value) in NameToDamageType)
        {
            if (key.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

        throw new InvalidDataException($"Unknown damage type: '{name}");
    }
}