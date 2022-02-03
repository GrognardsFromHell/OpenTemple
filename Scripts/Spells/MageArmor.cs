
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
    [SpellScript(280)]
    public class MageArmor : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Mage Armor OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Mage Armor OnSpellEffect");
            var armor_bonus = 4;
            spell.duration = 600 * spell.casterLevel;
            var target = spell.Targets[0];
            // check if target is friendly (willing target)
            if (target.Object.IsFriendly(spell.caster))
            {
                // HTN - WIP! this needs to be changed to a 'force_armor_bonus' that doesn't stack
                // with 'armor_bonus' (in addition, we need to allow non-corporeal monsters
                // to pass thru squares occupied by people without 'force_armor')
                target.Object.AddCondition("sp-Mage Armor", spell.spellId, spell.duration, armor_bonus);
                target.ParticleSystem = AttachParticles("sp-Mage Armor", target.Object);
            }
            else
            {
                // allow Will saving throw to negate
                if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    // saving throw successful
                    target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    AttachParticles("Fizzle", target.Object);
                    spell.RemoveTarget(target.Object);
                }
                else
                {
                    // saving throw unsuccessful
                    target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    // HTN - WIP! this needs to be changed to a 'force_armor_bonus' that doesn't stack
                    // with 'armor_bonus' (in addition, we need to allow non-corporeal monsters
                    // to pass thru squares occupied by people without 'force_armor')
                    target.Object.AddCondition("sp-Mage Armor", spell.spellId, spell.duration, armor_bonus);
                    target.ParticleSystem = AttachParticles("sp-Mage Armor", target.Object);
                }

            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Mage Armor OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Mage Armor OnEndSpellCast");
        }

    }
}
