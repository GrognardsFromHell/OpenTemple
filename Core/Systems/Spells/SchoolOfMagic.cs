namespace OpenTemple.Core.Systems.Spells
{
    public enum SchoolOfMagic
    {
        None = 0,
        Abjuration = 1,
        Conjuration = 2,
        Divination = 3,
        Enchantment = 4,
        Evocation = 5,
        Illusion = 6,
        Necromancy = 7,
        Transmutation = 8,
    }

    public enum SubschoolOfMagic
    {

        //  For Conjuration (teleportation is missing)
        Calling = 1,
        Creation = 2,
        Healing = 3,
        Summoning = 4,

        // For Enchantment
        Charm = 5,
        Compulsion = 6,

        // For Illusion
        Figment = 7,
        Glamer = 8,
        Pattern = 9,
        Phantasm = 10,
        Shadow = 11,

        // For Divination
        Scrying = 12

    }
}