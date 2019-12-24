
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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
    [SpellScript(43)]
    public class BreakEnchantment : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Break Enchantment OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-abjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Break Enchantment OnSpellEffect");
            // Dar's level check no longer needed thanks to Spellslinger's dll fix
            // if spell.caster_class == 13: #added to check for proper paladin slot level (darmagon)
            // if spell.spell_level < 4:
            // spell.caster.float_mesfile_line('mes\\spell.mes', 16008)
            // spell.spell_end(spell.id)
            // return
            var remove_list = new List<GameObjectBody>();
            spell.duration = 0;
            AttachParticles("sp-Break Enchantment-Area", spell.caster);
            foreach (var target_item in spell.Targets)
            {
                target_item.Object.AddCondition("sp-Break Enchantment", spell.spellId, spell.duration, 0);
                target_item.ParticleSystem = AttachParticles("sp-Break Enchantment-Hit", target_item.Object);
                remove_list.Add(target_item.Object);
            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Break Enchantment OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Break Enchantment OnEndSpellCast");
        }

    }
}
