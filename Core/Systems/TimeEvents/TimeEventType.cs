namespace OpenTemple.Core.Systems.TimeEvents
{
    /// <summary>
    /// The time event system that are available and can handle events.
    /// </summary>
    public enum TimeEventType
    {
        Debug = 0,
        Anim = 1,
        BkgAnim = 2,
        FidgetAnim = 3,
        Script = 4,
        PythonScript = 5,
        Poison = 6,
        NormalHealing = 7,
        SubdualHealing = 8,
        Aging = 9,
        AI = 10,
        AIDelay = 11,
        Combat = 12,
        TBCombat = 13,
        AmbientLighting = 14,
        WorldMap = 15,
        Sleeping = 16,
        Clock = 17,
        NPCWaitHere = 18,
        MainMenu = 19,
        Light = 20,
        Lock = 21,
        NPCRespawn = 22,
        DecayDeadBodies = 23,
        ItemDecay = 24,
        CombatFocusWipe = 25,
        Fade = 26,
        GFadeControl = 27,
        Teleported = 28,
        SceneryRespawn = 29,
        RandomEncounters = 30,
        ObjFade = 31,
        ActionQueue = 32,
        Search = 33,
        IntgameTurnbased = 34,
        PythonDialog = 35,
        EncumberedComplain = 36,
        PythonRealtime = 37,
        TimeEventSystemCount
    }
}