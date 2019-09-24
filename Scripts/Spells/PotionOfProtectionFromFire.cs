
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
    [SpellScript(711)]
    public class PotionOfProtectionFromFire : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Potion of protection from fire OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-abjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Potion of protection from fire OnSpellEffect");
            spell.duration = spell.casterLevel * 100;
            int protection_pts;
            if ((spell.casterLevel < 10))
            {
                protection_pts = spell.casterLevel * 12;
            }
            else
            {
                protection_pts = 120;
            }

            var target = spell.Targets[0];
            target.Object.AddCondition("sp-Potion of protection from energy", spell.spellId, spell.duration, DamageType.Fire, protection_pts);
        }
        // target.partsys_id = game.particles( 'sp-Potion of protection from energy', spell.caster )
        // spell.target_list.remove_target( target.obj )
        // spell.spell_end( spell.id )

        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Potion of protection from fire OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Potion of protection from fire OnEndSpellCast");
        }

    }
}
