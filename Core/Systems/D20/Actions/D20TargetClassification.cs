namespace SpicyTemple.Core.Systems.D20.Actions
{
    public enum D20TargetClassification
    {
        Target0 = 0,
        D20TC_Movement = 1,
        D20TC_SingleExcSelf,
        D20TC_CastSpell,
        D20TC_SingleIncSelf,
        D20TC_CallLightning,
        D20TC_ItemInteraction, // includes: portals, container, dead critters

        D20TC_Invalid = -1
    }
}