
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
    [SpellScript(508)]
    public class TrueSeeing : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("True Seeing OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-divination-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("True Seeing OnSpellEffect");
            spell.duration = 60 * spell.casterLevel;
            var target_item = spell.Targets[0];
            if (target_item.Object.IsFriendly(spell.caster))
            {
                target_item.Object.AddCondition("sp-True Seeing", spell.spellId, spell.duration, 0);
                target_item.ParticleSystem = AttachParticles("sp-True Seeing", target_item.Object);
            }
            else if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                // saving throw unsuccesful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                target_item.Object.AddCondition("sp-True Seeing", spell.spellId, spell.duration, 0);
                target_item.ParticleSystem = AttachParticles("sp-True Seeing", target_item.Object);
            }
            else
            {
                // saving throw successful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                AttachParticles("Fizzle", target_item.Object);
                spell.RemoveTarget(target_item.Object);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("True Seeing OnBeginRound");
            var range = 120;
            var target_item = spell.Targets[0];
            Logger.Info("target_list0 is={0}", target_item.Object);
            // 40 == san_true_seeing
            foreach (var obj in ObjList.ListCone(target_item.Object, ObjectListFilter.OLC_CRITTERS, range, 0, 360))
            {
                Logger.Info("found obj={0}", obj);
                if ((obj.ExecuteObjectScript(target_item.Object, ObjScriptEvent.TrueSeeing) == 0))
                {
                    AttachParticles("Fizzle", obj);
                }

            }

        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("True Seeing OnEndSpellCast");
        }

    }
}
