
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
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
            if ((target_item.Object.type == ObjectType.weapon) || (target_item.Object.type == ObjectType.ammo))
            {
                target_item.Object.InitD20Status();
                target_item.Object.AddCondition("sp-Greater Magic Weapon", spell.spellId, spell.duration, enhancement_bonus);
                target_item.ParticleSystem = AttachParticles("sp-Detect Magic 2 Med", spell.caster);
            }
            else
            {
                // not an item!
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30003);
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
            }

            spell.EndSpell();
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
