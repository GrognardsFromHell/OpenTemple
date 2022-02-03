
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

[SpellScript(40)]
public class BlindnessDeafness : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Blindness/Deafness OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-transmutation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Blindness/Deafness OnSpellEffect");
        spell.duration = 14400;

        var target_item = spell.Targets[0];

        if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
        {
            target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
            AttachParticles("Fizzle", target_item.Object);
            spell.RemoveTarget(target_item.Object);
        }
        else
        {
            target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
            if (spell.GetMenuArg(RadialMenuParam.MinSetting) == 1)
            {
                var return_val = target_item.Object.AddCondition("sp-Blindness", spell.spellId, spell.duration, 0);

                if (return_val)
                {
                    target_item.ParticleSystem = AttachParticles("sp-Blindness-Deafness", target_item.Object);

                }

            }
            else
            {
                var return_val = target_item.Object.AddCondition("sp-Deafness", spell.spellId, spell.duration, 0);

                if (return_val)
                {
                    target_item.ParticleSystem = AttachParticles("sp-Blindness-Deafness", target_item.Object);

                }

            }

        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Blindness/Deafness OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Blindness/Deafness OnEndSpellCast");
    }


}