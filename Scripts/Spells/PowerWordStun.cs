
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

[SpellScript(358)]
public class PowerWordStun : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Power Word Stun OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-enchantment-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Power Word Stun OnSpellEffect");
        var target = spell.Targets[0];
        // If target has over 150 hit points, spell fails
        if (target.Object.GetStat(Stat.hp_current) > 150)
        {
            target.Object.FloatMesFileLine("mes/spell.mes", 32000);
            AttachParticles("Fizzle", target.Object);
        }
        else if (target.Object.GetStat(Stat.hp_current) > 100)
        {
            spell.duration = RandomRange(1, 4);
            // apply stun
            var return_val = target.Object.AddCondition("sp-Sound Burst", spell.spellId, spell.duration, 0);
            if (return_val)
            {
                target.ParticleSystem = AttachParticles("sp-Sound Burst", target.Object);
            }

        }
        else if (target.Object.GetStat(Stat.hp_current) > 50)
        {
            spell.duration = RandomRange(1, 4) + RandomRange(1, 4);
            // apply stun
            var return_val = target.Object.AddCondition("sp-Sound Burst", spell.spellId, spell.duration, 0);
            if (return_val)
            {
                target.ParticleSystem = AttachParticles("sp-Sound Burst", target.Object);
            }

        }
        else
        {
            spell.duration = RandomRange(1, 4) + RandomRange(1, 4) + RandomRange(1, 4) + RandomRange(1, 4);
            // apply stun
            var return_val = target.Object.AddCondition("sp-Sound Burst", spell.spellId, spell.duration, 0);
            if (return_val)
            {
                target.ParticleSystem = AttachParticles("sp-Sound Burst", target.Object);
            }

        }

        spell.RemoveTarget(target.Object);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Power Word Stun OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Power Word Stun OnEndSpellCast");
    }

}