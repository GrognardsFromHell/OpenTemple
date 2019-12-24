
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
    [SpellScript(601)]
    public class VrockScreech : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Vrock Screech OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Vrock Screech OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            spell.duration = 1;
            spell.dc = 22;
            AttachParticles("Mon-Vrock-Screech", spell.caster);
            foreach (var target_item in spell.Targets)
            {
                Logger.Info("target={0}", target_item.Object);
                var tar = target_item.Object;
                if (tar.GetNameId() == 14258 || tar.GetNameId() == 14110 || tar.GetNameId() == 14259 || tar.GetNameId() == 14263 || tar.GetNameId() == 14286 || tar.GetNameId() == 14358 || tar.GetNameId() == 14357 || tar.GetNameId() == 14360 || tar.GetNameId() == 14361)
                {
                    // if target_item.obj.is_category_type( mc_subtype_demon ):
                    remove_list.Add(target_item.Object);
                }
                else if (!target_item.Object.SavingThrow(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster))
                {
                    // saving throw unsuccessful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 20021);
                    target_item.Object.AddCondition("sp-Vrock Screech", spell.spellId, spell.duration, 0);
                    target_item.ParticleSystem = AttachParticles("Mon-Vrock-Screech-Hit", target_item.Object);
                }
                else
                {
                    // saving throw successful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    AttachParticles("Fizzle", target_item.Object);
                    remove_list.Add(target_item.Object);
                }

            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Vrock Screech OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Vrock Screech OnEndSpellCast");
        }

    }
}
