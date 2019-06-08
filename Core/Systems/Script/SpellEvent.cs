namespace SpicyTemple.Core.Systems.Script
{
    public enum SpellEvent
    {
        SpellEffect = 0,
        BeginSpellCast, // activated inside animation callback 10017DD0
        EndSpellCast,
        BeginRound,
        EndRound,
        BeginProjectile,
        EndProjectile,
        BeginRoundD20Ping,
        EndRoundD20Ping,
        AreaOfEffectHit,
        SpellStruck
    }
}