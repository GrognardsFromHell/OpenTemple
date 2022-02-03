
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

[SpellScript(462)]
public class Stoneskin : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Stoneskin OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-abjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Stoneskin OnSpellEffect");
        spell.duration = 100 * spell.casterLevel;

        var damage_max = 10 * Math.Min(15, spell.casterLevel);

        var target = spell.Targets[0];

        var partsys_id = AttachParticles("sp-Stoneskin", target.Object);

        if (target.Object.IsFriendly(spell.caster))
        {
            target.Object.AddCondition("sp-Stoneskin", spell.spellId, spell.duration, partsys_id, damage_max);
        }
        else if (!target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
        {
            target.Object.FloatMesFileLine("mes/spell.mes", 30002);
            target.Object.AddCondition("sp-Stoneskin", spell.spellId, spell.duration, partsys_id, damage_max);
        }
        else
        {
            target.Object.FloatMesFileLine("mes/spell.mes", 30001);
            AttachParticles("Fizzle", target.Object);
            spell.RemoveTarget(target.Object);
        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Stoneskin OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Stoneskin OnEndSpellCast");
    }


}