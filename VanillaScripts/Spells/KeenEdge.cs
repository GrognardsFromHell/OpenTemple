
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(261)]
    public class KeenEdge : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Keen Edge OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Keen Edge OnSpellEffect");
            spell.duration = 100 * spell.casterLevel;

            var target_item = spell.Targets[0];

            if ((target_item.Object.type == ObjectType.weapon) && (!target_item.Object.HasCondition(SpellEffects.SpellKeenEdge)))
            {
                target_item.Object.InitD20Status();
                target_item.Object.AddCondition("sp-Keen Edge", spell.spellId, spell.duration, 0);
                target_item.ParticleSystem = AttachParticles("sp-Detect Magic 2 Med", spell.caster);

            }
            else
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30003);
                AttachParticles("Fizzle", spell.caster);
                spell.RemoveTarget(target_item.Object);
                spell.EndSpell();
            }

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Keen Edge OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Keen Edge OnEndSpellCast");
        }


    }
}
