
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

[SpellScript(367)]
public class ProtectionFromArrows : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Protection From Arrows OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-abjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Protection From Arrows OnSpellEffect");
        var damage_max = 10 * Math.Min(10, spell.casterLevel);
        spell.duration = 600 * spell.casterLevel;
        var target = spell.Targets[0];
        if (target.Object.IsFriendly(spell.caster))
        {
            target.Object.AddCondition("sp-Protection From Arrows", spell.spellId, spell.duration, damage_max);
            target.ParticleSystem = AttachParticles("sp-Protection From Arrows", target.Object);
        }
        else if (!target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
        {
            // saving throw unsuccessful
            target.Object.FloatMesFileLine("mes/spell.mes", 30002);
            target.Object.AddCondition("sp-Protection From Arrows", spell.spellId, spell.duration, damage_max);
            target.ParticleSystem = AttachParticles("sp-Protection From Arrows", target.Object);
        }
        else
        {
            // saving throw successful
            target.Object.FloatMesFileLine("mes/spell.mes", 30001);
            AttachParticles("Fizzle", target.Object);
            spell.RemoveTarget(target.Object);
        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Protection From Arrows OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Protection From Arrows OnEndSpellCast");
    }

}