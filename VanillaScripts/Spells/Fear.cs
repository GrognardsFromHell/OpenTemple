
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

[SpellScript(165)]
public class Fear : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Fear OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-necromancy-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Fear OnSpellEffect");
        spell.duration = Math.Min(1 * spell.casterLevel, 10);

        AttachParticles("sp-Fear", spell.caster);
        foreach (var target_item in spell.Targets)
        {
            if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                target_item.Object.AddCondition("sp-Fear", spell.spellId, spell.duration, 0);
                target_item.ParticleSystem = AttachParticles("sp-Fear-Hit", target_item.Object);

            }
            else
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                target_item.Object.AddCondition("sp-Fear", spell.spellId, spell.duration, 1);
                target_item.ParticleSystem = AttachParticles("sp-Fear-Hit", target_item.Object);

            }

        }

    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Fear OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Fear OnEndSpellCast");
    }


}