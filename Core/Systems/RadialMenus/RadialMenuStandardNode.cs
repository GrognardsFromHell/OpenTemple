namespace SpicyTemple.Core.Systems.RadialMenus
{
    /*
    Defines the standard nodes that are always present in the radial menu.
    */
    public enum RadialMenuStandardNode
    {
        // Groupings
        Root = 0,
        Spells = 1,
        Skills = 2,
        Feats = 3,
        Class = 4,
        Combat = 5,
        Items = 6,

        // Root->Skills->Alchemy
        Alchemy = 7,

        Movement = 8,
        Offense = 9,
        Tactical = 10,
        Options = 11,
        Potions = 12,
        Wands = 13,
        Scrolls = 14,
        CopyScroll = 15, // wizard class ability
        SpellsWizard = 16,
        SpellsSorcerer = 17,
        SpellsBard = 18,
        SpellsCleric = 19,
        SpellsPaladin = 20,
        SpellsDruid = 21,
        SpellsRanger = 22,
        SpellsDomain = 23, // above this are the spell numbers i.e. 0-9 for each class in the above order

        SomeCount = 72,

        SpellsDismiss = 199
    }

}