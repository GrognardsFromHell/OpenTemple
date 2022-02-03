
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

[SpellScript(229)]
public class HoldPortal : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Hold Portal OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-abjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Hold Portal OnSpellEffect");
        spell.duration = 10 * spell.casterLevel;
        var target = spell.Targets[0];
        // HTN - apply condition HOLD_PORTAL
        if (target.Object.type == ObjectType.portal)
        {
            target.Object.SetPortalFlag(PortalFlag.MAGICALLY_HELD);
            target.ParticleSystem = AttachParticles("sp-Hold Portal", target.Object);
            // HTN - need to add hold_portal spell condition to portal
            if (((target.Object.GetPortalFlags() & PortalFlag.OPEN)) != 0)
            {
                target.Object.TogglePortalOpen();
            }

        }
        else
        {
            AttachParticles("Fizzle", target.Object);
            target.Object.FloatMesFileLine("mes/spell.mes", 30000);
            target.Object.FloatMesFileLine("mes/spell.mes", 31000);
        }

        spell.RemoveTarget(target.Object);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Hold Portal OnBeginRound");
        spell.duration = spell.duration - 1;
        if (spell.duration <= 0)
        {
            if (spell.Targets.Length > 0)
            {
                var target = spell.Targets[0];
                target.Object.ClearPortalFlag(PortalFlag.MAGICALLY_HELD);
                GameSystems.ParticleSys.Remove(target.ParticleSystem);
            }

            spell.EndSpell();
        }

    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Hold Portal OnEndSpellCast");
    }

}