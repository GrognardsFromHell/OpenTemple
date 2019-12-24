
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
    [SpellScript(227)]
    public class HoldMonster : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Hold Monster OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Hold Monster OnSpellEffect");
            var npc = spell.caster;
            if (npc.GetNameId() == 14359) // Glabrezu Guardian
            {
                spell.dc = 20;
            }

            spell.duration = 1 * spell.casterLevel;
            var target = spell.Targets[0];
            if ((target.Object.type == ObjectType.pc) || (target.Object.type == ObjectType.npc))
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
                    // HTN - apply condition HOLD (paralyzed)
                    target.Object.AddCondition("sp-Hold Monster", spell.spellId, spell.duration, 0);
                    target.ParticleSystem = AttachParticles("sp-Hold Monster", target.Object);
                }

            }
            else
            {
                // not a critter
                target.Object.FloatMesFileLine("mes/spell.mes", 30000);
                target.Object.FloatMesFileLine("mes/spell.mes", 31001);
                AttachParticles("Fizzle", target.Object);
                spell.RemoveTarget(target.Object);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Hold Monster OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Hold Monster OnEndSpellCast");
        }

    }
}
