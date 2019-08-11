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
        Options,
        Potions,
        Wands,
        Scrolls,
        CopyScroll, // wizard class ability
        SpellsWizard,
        SpellsSorcerer,
        SpellsBard,
        SpellsCleric,
        SpellsPaladin,
        SpellsDruid,
        SpellsRanger,
        SpellsDomain = 23, // above this are the spell numbers i.e. 0-9 for each class in the above order

        SpellsDismiss = 199
    }

}