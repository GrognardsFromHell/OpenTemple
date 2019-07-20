namespace SpicyTemple.Core.Systems.Script
{
    public enum SpellEvent
    {
        SpellEffect = 0,
        BeginSpellCast = 1, // activated inside animation callback 10017DD0
        EndSpellCast = 2,
        BeginRound = 3,
        EndRound = 4,
        BeginProjectile = 5,
        EndProjectile = 6,
        BeginRoundD20Ping = 7,
        EndRoundD20Ping = 8,
        AreaOfEffectHit = 9,
        SpellStruck = 10
    }
}