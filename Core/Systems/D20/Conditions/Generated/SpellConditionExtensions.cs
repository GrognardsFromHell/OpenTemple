using SpicyTemple.Core.Systems.Spells;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    public static class SpellConditionExtensions
    {

        public static ConditionSpec.Builder MarkAsSpellEffect(this ConditionSpec.Builder builder, int spellEnum = 0)
        {
            builder.AddHandler(DispatcherType.D20Query, D20DispatcherKey.QUE_Critter_Has_Spell_Active, spellEnum);
            return builder;
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100d3620)]
        public static void   D20QHasSpellEffectActive(in DispatcherCallbackArgs evt, int spellEnum)
        {
            var dispIo = evt.GetDispIoD20Query();
            if ( dispIo.return_val != 1 )
            {
                var v2 = dispIo.data2;
                var spellEnum = dispIo.data1;
                if ( (v2 < 0)
                     || v2 <= 0 && spellEnum < WellKnownSpells.MagicCircleAgainstChaos
                     || v2 >= 0 && (v2 > 0 || spellEnum > WellKnownSpells.MagicCircleAgainstLaw) )
                {
                    var i = 0;
                    ConditionSpec condStruct;
                    while ( SpellCondStructPtrArray/*0x102e2600*/[i].spellEnum != spellEnum )
                    {
                        if ( (int)++i >= 261 )
                        {
                            return;
                        }
                    }
                    condStruct = SpellCondStructPtrArray/*0x102e2600*/[i].cond;
                    LABEL_12:
                    if ( evt.objHndCaller.HasCondition(condStruct) )
                    {
                        dispIo.return_val = 1;
                    }
                }
            }
        }
        
    }
}