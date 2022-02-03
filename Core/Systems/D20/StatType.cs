namespace OpenTemple.Core.Systems.D20;

public enum StatType
{
    Abilities = 0,
    Level = 1,
    HitPoints = 2,
    Combat = 3,
    Money = 4,
    AbilityMods = 5,
    Speed = 6,
    Feat = 7,
    Race = 8,
    Load = 9,
    SavingThrows = 10,
    SpellCasting = 11, // (originally missing, this was probably related to the stat_caster_level etc stats)
    Other = 12,
    Psi = 13
}