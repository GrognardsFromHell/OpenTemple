namespace OpenTemple.Core.Systems.D20.Actions
{
    public enum D20TargetClassification
    {
        Target0 = 0,
        Movement = 1,
        SingleExcSelf = 2,
        CastSpell = 3,
        SingleIncSelf = 4,
        CallLightning = 5,
        ItemInteraction = 6, // includes: portals, container, dead critters

        Invalid = -1
    }
}