
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

[SpellScript(284)]
public class MagicCircleAgainstGood : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Magic Circle against Good OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-abjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Magic Circle against Good OnSpellEffect");
        spell.duration = 100 * spell.casterLevel;
        var target_item = spell.Targets[0];
        if (target_item.Object.IsFriendly(spell.caster))
        {
            target_item.Object.AddCondition("sp-Magic Circle Outward", spell.spellId, spell.duration, 2);
            target_item.ParticleSystem = AttachParticles("sp-Magic Circle against Good-OUT", target_item.Object);
        }
        else
        {
            target_item.Object.AddCondition("sp-Magic Circle Inward", spell.spellId, spell.duration, 2);
            target_item.ParticleSystem = AttachParticles("sp-Magic Circle against Good-IN", target_item.Object);
        }

    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Magic Circle against Good OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Magic Circle against Good OnEndSpellCast");
    }

}