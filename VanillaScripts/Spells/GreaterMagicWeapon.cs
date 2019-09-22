
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

namespace VanillaScripts
{
    [SpellScript(205)]
    public class GreaterMagicWeapon : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Greater Magic Weapon OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Greater Magic Weapon OnSpellEffect");
            var enhancement_bonus = spell.casterLevel / 4;

            spell.duration = 600 * spell.casterLevel;

            var target_item = spell.Targets[0];

            if (((target_item.Object.type == ObjectType.weapon) || (target_item.Object.type == ObjectType.ammo)) && (!target_item.Object.HasCondition(SpellEffects.SpellGreaterMagicWeapon)))
            {
                target_item.Object.InitD20Status();
                target_item.Object.AddCondition("sp-Greater Magic Weapon", spell.spellId, spell.duration, enhancement_bonus);
                target_item.ParticleSystem = AttachParticles("sp-Detect Magic 2 Med", spell.caster);

            }
            else
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30003);
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
                spell.EndSpell();
            }

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Greater Magic Weapon OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Greater Magic Weapon OnEndSpellCast");
        }


    }
}
