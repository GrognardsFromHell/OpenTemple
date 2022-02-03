
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

[SpellScript(262)]
public class Knock : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Knock OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-transmutation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Knock OnSpellEffect");
        var target = spell.Targets[0];
        var chest = target.Object;
        if (target.Object.type == ObjectType.portal)
        {
            target.ParticleSystem = AttachParticles("sp-Knock", target.Object);
            if (((target.Object.GetPortalFlags() & PortalFlag.LOCKED)) != 0)
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 30004);
                target.Object.ClearPortalFlag(PortalFlag.LOCKED);
            }
            else
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 30006);
            }

            if (((target.Object.GetPortalFlags() & PortalFlag.MAGICALLY_HELD)) != 0)
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 30005);
                target.Object.ClearPortalFlag(PortalFlag.MAGICALLY_HELD);
            }

            if ((target.Object.GetPortalFlags() & PortalFlag.OPEN) == 0)
            {
                target.Object.TogglePortalOpen();
            }

        }
        else if (chest.GetNameId() == 1053 || chest.GetNameId() == 1055)
        {
            AttachParticles("Fizzle", target.Object);
            target.Object.FloatMesFileLine("mes/spell.mes", 30000);
            target.Object.FloatMesFileLine("mes/spell.mes", 31014);
        }
        else if (chest.GetNameId() == 1056)
        {
            SetGlobalFlag(874, true);
            AttachParticles("Fizzle", target.Object);
            target.Object.FloatMesFileLine("mes/spell.mes", 30000);
            target.Object.FloatMesFileLine("mes/spell.mes", 31014);
        }
        else if (target.Object.type == ObjectType.container)
        {
            target.ParticleSystem = AttachParticles("sp-Knock", target.Object);
            if (((target.Object.GetContainerFlags() & ContainerFlag.LOCKED)) != 0)
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 30004);
                target.Object.ClearContainerFlag(ContainerFlag.LOCKED);
            }

            if (((target.Object.GetContainerFlags() & ContainerFlag.MAGICALLY_HELD)) != 0)
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 30005);
                target.Object.ClearContainerFlag(ContainerFlag.MAGICALLY_HELD);
            }

            if ((target.Object.GetContainerFlags() & ContainerFlag.OPEN) == 0)
            {
                target.Object.ToggleContainerOpen();
            }

        }
        else
        {
            AttachParticles("Fizzle", target.Object);
            target.Object.FloatMesFileLine("mes/spell.mes", 30000);
            target.Object.FloatMesFileLine("mes/spell.mes", 30003);
        }

        spell.RemoveTarget(target.Object);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Knock OnBeginRound");
    }
    // spell.duration = spell.duration - 1
    // if (was_magically_locked == 0) and (spell.target_list[0].portal_flags_get() & OPF_MAGICALLY_HELD):
    // was_magically_locked = 1
    // spell.target_list[0].float_mesfile_line( 'mes\\spell.mes', 30005 )
    // spell.target_list[0].portal_flag_unset( OPF_MAGICALLY_HELD )
    // if spell.duration <= 0:
    // if was_magically_locked == 1:
    // spell.target_list[0].portal_flag_set( OPF_MAGICALLY_HELD )
    // game.particles_kill( spell.partsys_id )
    // spell.spell_end( spell.id )

    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Knock OnEndSpellCast");
    }

}