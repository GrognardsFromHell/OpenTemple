
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells;

[SpellScript(399)]
public class Resistance : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Resistance OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-transmutation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Resistance OnSpellEffect");
        spell.duration = 10;

        var target = spell.Targets[0];

        if (!target.Object.IsFriendly(spell.caster))
        {
            if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                AttachParticles("Fizzle", target.Object);
                spell.RemoveTarget(target.Object);
            }
            else
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                target.Object.AddCondition("sp-Resistance", spell.spellId, spell.duration, 0);
                target.ParticleSystem = AttachParticles("sp-Resistance", target.Object);

            }

        }
        else
        {
            target.Object.AddCondition("sp-Resistance", spell.spellId, spell.duration, 0);
            target.ParticleSystem = AttachParticles("sp-Resistance", target.Object);

        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Resistance OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Resistance OnEndSpellCast");
    }


}