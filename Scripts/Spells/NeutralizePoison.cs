
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

[SpellScript(327)]
public class NeutralizePoison : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Neutralize Poison OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Neutralize Poison OnSpellEffect");
        if (spell.spellClass == 13) // added to check for proper paladin slot level (darmagon)
        {
            if (spell.spellKnownSlotLevel < 4)
            {
                spell.caster.FloatMesFileLine("mes/spell.mes", 16008);
                spell.EndSpell();
                return;
            }

        }

        if (spell.spellClass == 14)
        {
            if (spell.spellKnownSlotLevel < 3) // added to check for proper ranger slot level (darmagon)
            {
                spell.caster.FloatMesFileLine("mes/spell.mes", 16008);
                spell.EndSpell();
                return;
            }

        }

        spell.duration = 0;
        var target = spell.Targets[0];
        if (target.Object.IsFriendly(spell.caster))
        {
            // Neutralise any Hezrou Stench effects.
            Stench.neutraliseStench(target.Object, 600 * spell.casterLevel);
            target.Object.AddCondition("sp-Neutralize Poison", spell.spellId, spell.duration, 0);
            target.ParticleSystem = AttachParticles("sp-Neutralize Poison", target.Object);
        }
        else if (!target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
        {
            // saving throw unsuccesful
            target.Object.FloatMesFileLine("mes/spell.mes", 30002);
            // Neutralise any Hezrou Stench effects.
            Stench.neutraliseStench(target.Object, 600 * spell.casterLevel);
            target.Object.AddCondition("sp-Neutralize Poison", spell.spellId, spell.duration, 0);
            target.ParticleSystem = AttachParticles("sp-Neutralize Poison", target.Object);
        }
        else
        {
            // saving throw succesful
            target.Object.FloatMesFileLine("mes/spell.mes", 30001);
            AttachParticles("Fizzle", target.Object);
            spell.RemoveTarget(target.Object);
        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Neutralize Poison OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Neutralize Poison OnEndSpellCast");
    }

}