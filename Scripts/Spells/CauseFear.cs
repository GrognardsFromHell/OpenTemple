
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

namespace Scripts.Spells;

[SpellScript(50)]
public class CauseFear : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Cause Fear OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-necromancy-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Cause Fear OnSpellEffect");
        var dice = Dice.D4;
        spell.duration = dice.Roll();
        var target_item = spell.Targets[0];
        if ((GameSystems.Critter.GetHitDiceNum(target_item.Object) < 6))
        {
            // allow Will saving throw to negate
            if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                // saving throw successful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                // game.particles( 'Fizzle', target_item.obj )
                // spell.target_list.remove_target( target_item.obj )
                // shaken
                var return_val = target_item.Object.AddCondition("sp-Cause Fear", spell.spellId, 1, 1);
                if (return_val)
                {
                    target_item.ParticleSystem = AttachParticles("sp-Cause Fear", target_item.Object);
                }

            }
            else
            {
                // saving throw unsuccessful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                // frightened
                var return_val = target_item.Object.AddCondition("sp-Cause Fear", spell.spellId, spell.duration, 0);
                if (return_val)
                {
                    target_item.ParticleSystem = AttachParticles("sp-Cause Fear", target_item.Object);
                }

            }

        }
        else
        {
            // cannot affect HD > 5
            target_item.Object.FloatMesFileLine("mes/spell.mes", 30003);
            AttachParticles("Fizzle", target_item.Object);
            spell.RemoveTarget(target_item.Object);
        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Cause Fear OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Cause Fear OnEndSpellCast");
    }

}