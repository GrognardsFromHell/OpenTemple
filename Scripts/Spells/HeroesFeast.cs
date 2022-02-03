
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

[SpellScript(225)]
public class HeroesFeast : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Heroes' Feast OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Heroes' Feast OnSpellEffect");
        spell.duration = 7200;
        foreach (var target_item in spell.Targets)
        {
            var return_val1 = target_item.Object.AddCondition("sp-Aid", spell.spellId, spell.duration, 1);
            if (return_val1)
            {
                target_item.ParticleSystem = AttachParticles("sp-Aid", target_item.Object);
            }

        }

        foreach (var target_item in spell.Targets)
        {
            var return_val2 = target_item.Object.AddCondition("sp-Neutralize Poison", spell.spellId, spell.duration, 1);
            if (return_val2)
            {
                AttachParticles("sp-Neutralize Poison", target_item.Object);
            }

        }

        foreach (var target_item in spell.Targets)
        {
            // return_val3 = target_item.obj.condition_add_with_args( 'sp-Remove Disease', spell.id, 0, 1 )
            // if return_val3 == 1:
            // game.particles( 'sp-Remove Disease', target_item.obj )
            // Removed this since it removes the target object, as the Remove Disease spell is instantaneous
            // Instead, using S_Remove_Disease
            // Not perfect since it doesn't cure Vrock Spores...
            target_item.Object.D20SendSignal(D20DispatcherKey.SIG_Remove_Disease);
        }

        foreach (var target_item in spell.Targets)
        {
            var return_val4 = target_item.Object.AddCondition("sp-Remove Fear", spell.spellId, spell.duration, 0);
            if (return_val4)
            {
                target_item.ParticleSystem = AttachParticles("sp-Remove Fear", target_item.Object);
            }

        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Heroes' Feast OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Heroes' Feast OnEndSpellCast");
    }

}