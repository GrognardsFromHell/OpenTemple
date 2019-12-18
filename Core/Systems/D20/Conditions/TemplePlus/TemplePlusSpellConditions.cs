namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public static class TemplePlusSpellConditions
    {
        public static ConditionSpec.Builder AddSpellCountdownStandardHook(this ConditionSpec.Builder builder)
        {
            // adds an ET_OnBeginRound handler that (normally) does:
            // If countdown expired: (<0)
            //   1. Float text "Spell Expired"
            //   2. RemoveSpell() (has case-by-case handlers for Spell_End; Temple+ adds generic handling for wall spells here)
            //   3. RemoveSpellMod()
            // Else:
            //   Decrement count, update spell packet duration
            return builder.AddHandler(DispatcherType.BeginRound, SpellEffects.SpellModCountdownRemove, 0);
        }

        public static ConditionSpec.Builder AddAoESpellEndStandardHook(this ConditionSpec.Builder builder)
        {
            // adds a EK_S_Spell_End handler that:
            // 1. Ends particles for all spell objects
            // 2. RemoveSpellMod()
            return builder.AddSignalHandler(D20DispatcherKey.SIG_Spell_End, SpellEffects.AoESpellRemove, 0);
        }

        public static ConditionSpec.Builder AddSpellDismissStandardHook(this ConditionSpec.Builder builder)
        {
            // adds a EK_S_Spell_End handler that:
            // 1. Ends particles for all spell objects
            // 2. RemoveSpellMod()
            return builder.AddHandler(DispatcherType.ConditionAdd, SpellEffects.SpellAddDismissCondition);
        }
    }
}