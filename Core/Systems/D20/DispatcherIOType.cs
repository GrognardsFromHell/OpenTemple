namespace SpicyTemple.Core.Systems.D20
{
    public enum DispatcherIOType
    {
        None = 0, // not in actual use (other than init), first real type is the next one
        TypeCondStruct = 1,
        TypeBonusList = 2,
        TypeSavingThrow = 3,
        TypeDamage = 4,
        TypeAttackBonus = 5, // AC
        TypeSendSignal = 6, // Usages detected: dispTypeD20AdvanceTime (6), dispTypeBeginRound (48), and of course also dispTypeD20Signal (28)
        TypeQuery = 7,
        TypeTurnBasedStatus = 8,
        TypeTooltip = 9,
        TypeObjBonus = 10, // used for skill level, initiative level, and attacker concealment chance
        TypeDispelCheck = 11, // goes with dispTypeDispelCheck
        TypeD20ActionTurnBased = 12,
        TypeMoveSpeed = 13,
        TypeBonusListAndSpellEntry = 14,
        TypeReflexThrow = 15,
        Type16 = 16,
        TypeObjEvent = 17,
        Type18 = 18,
        AbilityLoss = 19,
        AttackDice = 20,
        ImmunityTrigger = 21,
        Type22 = 22,
        TypeImmunityHandler = 23,
        TypeEffectTooltip = 24,
        Type25 = 25,
        Type26 = 26,
        Type27 = 27,
        Type28 = 28,
        Type29 = 29,
        Type30 = 30,
        Type31 = 31,
        Type32 = 32,
        Type33 = 33,
        evtObjTypeSpellCaster = 34, // new! used for querying spell caster specs (caster level, learnable spells, etc.)
        evtObjTypeActionCost = 35 // new! used for modifying action cost
    }

}