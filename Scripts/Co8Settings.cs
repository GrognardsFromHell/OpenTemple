namespace Scripts;

public static class Co8Settings
{
    // configoverridebits(449, new []{0, 1, 2}, paramvalue);
    public static int PartyRunSpeed { get; set; }

    // configoverridebits(450, 0, paramvalue);
    public static bool DisableNewPlots { get; set; }

    // configoverridebits(450, 10, paramvalue);
    public static bool DisableTargetOfRevenge { get; set; }

    // configoverridebits(450, 11, paramvalue);
    public static bool DisableMoathouseAmbush { get; set; }

    // configoverridebits(450, 12, paramvalue);
    public static bool DisableArenaOfHeroes { get; set; }

    // configoverridebits(450, 13, paramvalue);
    public static bool DisableReactiveTemple { get; set; }

    // configoverridebits(450, 14, paramvalue);
    public static bool DisableScrengRecruitSpecial { get; set; }

    // configoverridebits(451, 0, paramvalue);
    public static bool EntangleOutdoorsOnly { get; set; }

    // configoverridebits(451, 1, paramvalue);
    public static bool ElementalSpellsAtElementalNodes { get; set; }

    // configoverridebits(451, 2, paramvalue);
    public static bool CharmSpellDCModifier { get; set; }

    // configoverridebits(451, 3, paramvalue);
    public static bool AIIgnoreSummons { get; set; }

    // configoverridebits(451, 4, paramvalue);
    public static bool AIIgnoreSpiritualWeapons { get; set; }

    // configoverridebits(451, new []{5, 6, 7}, paramvalue); // bits 5-7
    public static int RandomEncounterXPReduction { get; set; }

    // configoverridebits(451, 8, paramvalue);
    public static bool StinkingCloudDurationNerf { get; set; }

    #region TestMode Settings
    public static bool TestModeEnabled { get; set; }

    public static bool RandomEncountersDisabled { get; set; }

    public static bool QuickstartAutolootEnabled { get; set; }

    public static bool QuickstartAutolootAutoConvertJewels { get; set; }
    #endregion

}