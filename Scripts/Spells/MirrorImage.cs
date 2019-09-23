
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
    [SpellScript(315)]
    public class MirrorImage : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Mirror Image OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-illusion-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Mirror Image OnSpellEffect");
            spell.duration = 10 * spell.casterLevel;
            var target_item = spell.Targets[0];
            var dice = Dice.D4;
            dice = dice.WithModifier(1 + (spell.casterLevel / 3));
            var num_of_images = dice.Roll();
            if (num_of_images > 8)
            {
                num_of_images = 8;
            }

            // print "num of images=", num_of_images, "bonus=", 1 + (spell.caster_level / 3)
            AttachParticles("sp-Mirror Image", target_item.Object);
            target_item.Object.AddCondition("sp-Mirror Image", spell.spellId, spell.duration, num_of_images);
        }
        // target_item.partsys_id = game.particles( 'sp-Mirror Image', target_item.obj )

        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Mirror Image OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Mirror Image OnEndSpellCast");
        }

    }
}
