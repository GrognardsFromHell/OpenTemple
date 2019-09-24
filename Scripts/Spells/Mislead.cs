
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
    [SpellScript(317)]
    public class Mislead : BaseSpellScript
    {
        private static readonly Dictionary<int, ValueTuple<int, int>> mislead_spell_list = new Dictionary<int, ValueTuple<int, int>>();

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Mislead OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-illusion-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Mislead OnSpellEffect");
            spell.duration = 1 * spell.casterLevel;
            var target_item = spell.Targets[0];
            var num_of_images = spell.casterLevel;
            // print "num of images=", num_of_images
            AttachParticles("sp-Mirror Image", target_item.Object);
            target_item.Object.AddCondition("sp-Mirror Image", spell.spellId, spell.duration, num_of_images);
            // target_item.partsys_id = game.particles( 'sp-Mirror Image', target_item.obj )
            // spell.id = spell.id + 1
            target_item.Object.AddCondition("sp-Improved Invisibility", spell.spellId, spell.duration, 0);
            target_item.ParticleSystem = AttachParticles("sp-Improved Invisibility", target_item.Object);
        }
        // spell.spell_end( spell.id, 1 )# do not end the spell, else the effect countdown is interrupted

        public override void OnBeginRound(SpellPacketBody spell)
        {
            // Crappy workaround to end the spell (otherwise it never ends...)
            // Note: OnBeginRound gets called num_of_images times each round, because sp-Mirror Image duplicates the target num_of_images times (to store the particle FX)
            // Thus we check the game time to prevent decrementing the duration multiple times
            var cur_time = CurrentTimeSeconds;
            if ((mislead_spell_list).ContainsKey(spell.spellId))
            {
                var entry = mislead_spell_list[spell.spellId];
                var entry_time = entry.Item2;
                if (cur_time > entry_time)
                {
                    entry.Item2 = cur_time;
                    entry.Item1 -= 1;
                    // mislead_spell_list[spell.id] = entry
                    Logger.Info("{0}", "Mislead OnBeginRound, duration: " + entry.Item1.ToString() + ", ID: " + spell.spellId.ToString());
                    if (entry.Item1 <= 0)
                    {
                        spell.EndSpell(true);
                    }

                }

                mislead_spell_list[spell.spellId] = entry;
            }
            else
            {
                Logger.Info("{0}", "Mislead OnBeginRound, duration: " + spell.duration.ToString() + ", ID: " + spell.spellId.ToString());
                mislead_spell_list[spell.spellId] = (spell.duration - 1, cur_time);
            }

        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Mislead OnEndSpellCast");
        }

    }
}
