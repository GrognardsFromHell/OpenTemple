namespace SpicyTemple.Core.Systems.D20.Actions
{
    public enum D20TargetClassification
    {
        Target0 = 0,
        Movement = 1,
        SingleExcSelf,
        CastSpell,
        SingleIncSelf,
        CallLightning,
        ItemInteraction, // includes: portals, container, dead critters

        Invalid = -1
    }
}