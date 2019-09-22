
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(262)]
    public class Knock : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Knock OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-abjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Knock OnSpellEffect");
            var target = spell.Targets[0];

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
            else if (target.Object.type == ObjectType.container)
            {
                target.ParticleSystem = AttachParticles("sp-Knock", target.Object);

                if (((target.Object.GetContainerFlags() & ContainerFlag.LOCKED)) != 0)
                {
                    target.Object.FloatMesFileLine("mes/spell.mes", 30004);
                    target.Object.ClearContainerFlag(ContainerFlag.LOCKED);
                }
                else
                {
                    target.Object.FloatMesFileLine("mes/spell.mes", 30006);
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
                target.Object.FloatMesFileLine("mes/spell.mes", 31000);
            }

            spell.RemoveTarget(target.Object);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Knock OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Knock OnEndSpellCast");
        }


    }
}
