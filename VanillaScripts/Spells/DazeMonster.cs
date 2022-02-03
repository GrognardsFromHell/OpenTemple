
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

[SpellScript(556)]
public class DazeMonster : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Daze Monster OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-enchantment-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Daze Monster OnSpellEffect");
        spell.duration = 1;

        var target = spell.Targets[0];

        if (GameSystems.Critter.GetHitDiceNum(target.Object) < 7)
        {
            if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                AttachParticles("Fizzle", target.Object);
                spell.RemoveTarget(target.Object);
            }
            else
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                target.Object.AddCondition("sp-Daze", spell.spellId, spell.duration, 0);
                target.ParticleSystem = AttachParticles("sp-Daze", target.Object);

            }

        }
        else
        {
            AttachParticles("Fizzle", target.Object);
            spell.RemoveTarget(target.Object);
        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Daze Monster OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Daze Monster OnEndSpellCast");
    }


}