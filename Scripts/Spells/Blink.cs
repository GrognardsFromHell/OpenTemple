
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

[SpellScript(41)]
public class Blink : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Blink OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-transmutation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Blink OnSpellEffect");
        spell.duration = 1 * spell.casterLevel;
        var target_item = spell.Targets[0];
        // added for a form of movement barred by a Dimensional Anchor
        if (target_item.Object.HasCondition(SpellEffects.SpellDimensionalAnchor))
        {
            target_item.Object.FloatMesFileLine("mes/spell.mes", 30011);
            AttachParticles("Fizzle", target_item.Object);
        }
        else
        {
            target_item.Object.AddCondition("sp-Blink", spell.spellId, spell.duration, 242);
            target_item.ParticleSystem = AttachParticles("sp-Blink", target_item.Object);
        }

    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Blink OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Blink OnEndSpellCast");
    }

}