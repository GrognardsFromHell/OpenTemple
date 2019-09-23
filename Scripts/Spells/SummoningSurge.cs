
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(770)]
    public class SummoningSurge : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Summoning Surge OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Summoning Surge OnSpellEffect");
            var str_amount = 4;
            spell.duration = 1 * spell.casterLevel;
            // game.particles( 'sp-Mass Bulls Strength', spell.target_loc )
            foreach (var target_item in spell.Targets)
            {
                if (target_item.Object.IsFriendly(spell.caster))
                {
                    // target_item.obj.float_mesfile_line('mes\\spell.mes', 25005, tf_red)
                    if (target_item.Object.HasCondition(SpellEffects.SpellSummoned))
                    {
                        // target_item.obj.float_mesfile_line('mes\\spell.mes', 25007, tf_red)
                        target_item.Object.AddCondition("sp-Bulls Strength", spell.spellId, spell.duration, str_amount);
                        target_item.ParticleSystem = AttachParticles("sp-Bullstrength", target_item.Object);
                    }

                }

            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Summoning Surge OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Summoning Surge OnEndSpellCast");
        }

    }
}
