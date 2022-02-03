
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

[SpellScript(133)]
public class DispelMagic : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Dispel Magic OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Dispel Magic OnSpellEffect");
        if (spell.IsObjectSelected())
        {
            var target = spell.Targets[0];

            if ((target.Object.type == ObjectType.pc) || (target.Object.type == ObjectType.npc))
            {
                target.ParticleSystem = AttachParticles("sp-Dispel Magic - Targeted", target.Object);

                target.Object.AddCondition("sp-Dispel Magic", spell.spellId, 0, 0);
            }
            else if ((target.Object.type == ObjectType.portal) || (target.Object.type == ObjectType.container))
            {
                if ((target.Object.GetPortalFlags() & PortalFlag.MAGICALLY_HELD) != 0)
                {
                    target.ParticleSystem = AttachParticles("sp-Dispel Magic - Targeted", target.Object);

                    target.Object.ClearPortalFlag(PortalFlag.MAGICALLY_HELD);
                    spell.RemoveTarget(target.Object);
                }

            }

        }
        else
        {
            SpawnParticles("sp-Dispel Magic - Area", spell.aoeCenter);
            foreach (var target in spell.Targets)
            {
                if ((target.Object.type == ObjectType.pc) || (target.Object.type == ObjectType.npc))
                {
                    target.ParticleSystem = AttachParticles("sp-Dispel Magic - Targeted", target.Object);

                    target.Object.AddCondition("sp-Dispel Magic", spell.spellId, 0, 1);
                }
                else if ((target.Object.type == ObjectType.portal) || (target.Object.type == ObjectType.container))
                {
                    if ((target.Object.GetPortalFlags() & PortalFlag.MAGICALLY_HELD) != 0)
                    {
                        target.ParticleSystem = AttachParticles("sp-Dispel Magic - Targeted", target.Object);

                        target.Object.ClearPortalFlag(PortalFlag.MAGICALLY_HELD);
                        spell.RemoveTarget(target.Object);
                    }

                }

            }

        }

        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Dispel Magic OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Dispel Magic OnEndSpellCast");
    }


}