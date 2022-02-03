
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

[SpellScript(192)]
public class GentleRepose : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Gentle Repose OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-necromancy-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Gentle Repose OnSpellEffect");
        var target = spell.Targets[0];
        if (!target.Object.IsFriendly(spell.caster))
        {
            if (!target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                // saving throw unsuccessful
                target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                AttachParticles("sp-Inflict Light Wounds", target.Object);
                target.Object.FloatMesFileLine("mes/spell.mes", 192);
                var x = target.Object.GetInt(obj_f.critter_flags2);
                x = x | 64;
                target.Object.SetInt(obj_f.critter_flags2, x);
                SetGlobalVar(900, target.Object.GetInt(obj_f.critter_flags2));
            }
            else
            {
                // saving throw successful
                target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                AttachParticles("Fizzle", target.Object);
            }

        }
        else
        {
            AttachParticles("sp-Inflict Light Wounds", target.Object);
            target.Object.FloatMesFileLine("mes/spell.mes", 192);
            var x = target.Object.GetInt(obj_f.critter_flags2);
            x = x | 64;
            target.Object.SetInt(obj_f.critter_flags2, x);
        }

        spell.RemoveTarget(target.Object);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Gentle Repose OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Gentle Repose OnEndSpellCast");
    }

}