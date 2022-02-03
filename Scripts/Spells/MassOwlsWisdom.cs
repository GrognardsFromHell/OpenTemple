
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

[SpellScript(576)]
public class MassOwlsWisdom : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Mass Owls Wisdom OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-transmutation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Mass Owls Wisdom OnSpellEffect");
        var wis_amount = 4;
        spell.duration = 10 * spell.casterLevel;
        foreach (var target_item in spell.Targets)
        {
            if (target_item.Object.IsFriendly(spell.caster))
            {
                target_item.Object.AddCondition("sp-Owls Wisdom", spell.spellId, spell.duration, wis_amount);
                target_item.ParticleSystem = AttachParticles("sp-Owls Wisdom", target_item.Object);
            }
            else if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                // saving throw unsuccesful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                target_item.Object.AddCondition("sp-Owls Wisdom", spell.spellId, spell.duration, wis_amount);
                target_item.ParticleSystem = AttachParticles("sp-Owls Wisdom", target_item.Object);
            }
            else
            {
                // saving throw successful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                AttachParticles("Fizzle", target_item.Object);
            }

        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Mass Owls Wisdom OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Mass Owls Wisdom OnEndSpellCast");
    }

}