
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

[SpellScript(337)]
public class OtilukesResilientSphere : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Otiluke's Resilient Sphere OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public static void do_particles(SpellTarget target)
    {
        var part_str = "sp-Otilukes Resilient Sphere";
        if (GameSystems.Stat.DispatchGetSizeCategory(target.Object) <= SizeCategory.Medium)
        {
            part_str = "sp-Otilukes Resilient Sphere-MED";
        }
        else if (GameSystems.Stat.DispatchGetSizeCategory(target.Object) == SizeCategory.Large)
        {
            part_str = "sp-Otilukes Resilient Sphere-LARGE";
        }
        else
        {
            part_str = "sp-Otilukes Resilient Sphere-HUGE";
        }

        target.ParticleSystem = AttachParticles(part_str, target.Object);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Otiluke's Resilient Sphere OnSpellEffect");
        spell.duration = 10 * spell.casterLevel;
        var target = spell.Targets[0];
        var friend = target.Object.IsFriendly(spell.caster);
        bool saved;
        if (!friend)
        {
            saved = target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Reflex, D20SavingThrowFlag.NONE, spell.caster, spell.spellId);
            if (saved)
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 30001);
            }
            else
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 30002);
            }

        }
        else
        {
            saved = true;
        }

        if (friend || !saved)
        {
            target.Object.AddCondition("sp-Otilukes Resilient Sphere", spell.spellId, spell.duration, 0);
            do_particles(target);
        }
        else
        {
            AttachParticles("Fizzle", target.Object);
            spell.RemoveTarget(target.Object);
            spell.EndSpell();
        }

    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Otiluke's Resilient Sphere OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Otiluke's Resilient Sphere OnEndSpellCast");
    }

}