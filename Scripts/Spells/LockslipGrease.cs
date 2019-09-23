
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
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(788)]
    public class LockslipGrease : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Lockslip Grease OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Gentle Repose OnSpellEffect");
            var target = spell.Targets[0];
            var chest = target.Object;
            if (spell.caster.GetMap() == 5081)
            {
                AttachParticles("Fizzle", target.Object);
                target.Object.FloatMesFileLine("mes/spell.mes", 30000);
                target.Object.FloatMesFileLine("mes/narrative.mes", 160, TextFloaterColor.Yellow);
            }
            else if (target.Object.type == ObjectType.portal)
            {
                if (((target.Object.GetPortalFlags() & PortalFlag.LOCKED)) != 0)
                {
                    if (target.Object.GetInt(obj_f.portal_pad_i_1) == 0)
                    {
                        var x = target.Object.GetInt(obj_f.portal_lock_dc);
                        x = x - 1;
                        target.Object.SetInt(obj_f.portal_lock_dc, x);
                        target.Object.SetInt(obj_f.portal_pad_i_1, 1);
                        target.ParticleSystem = AttachParticles("sp-Knock", target.Object);
                    }
                    else
                    {
                        target.Object.FloatMesFileLine("mes/spell.mes", 16012);
                    }

                }
                else
                {
                    target.Object.FloatMesFileLine("mes/spell.mes", 30006);
                }

            }
            else if (target.Object.type == ObjectType.container)
            {
                if (((target.Object.GetContainerFlags() & ContainerFlag.LOCKED)) != 0)
                {
                    if (target.Object.GetInt(obj_f.container_pad_i_1) == 0)
                    {
                        var x = target.Object.GetInt(obj_f.container_lock_dc);
                        x = x - 1;
                        target.Object.SetInt(obj_f.container_lock_dc, x);
                        target.Object.SetInt(obj_f.container_pad_i_1, 1);
                        target.ParticleSystem = AttachParticles("sp-Knock", target.Object);
                    }
                    else
                    {
                        target.Object.FloatMesFileLine("mes/spell.mes", 16012);
                    }

                }
                else
                {
                    target.Object.FloatMesFileLine("mes/spell.mes", 30006);
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
            Logger.Info("Lockslip Grease OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Lockslip Grease OnEndSpellCast");
        }

    }
}
