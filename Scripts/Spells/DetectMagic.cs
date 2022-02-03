
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

namespace Scripts.Spells
{
    [SpellScript(114)]
    public class DetectMagic : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Detect Magic OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-divination-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Detect Magic OnSpellEffect");
            spell.duration = 10 * spell.casterLevel;
            var target = spell.Targets[0];
            target.Object.AddCondition("sp-Detect Magic", spell.spellId, spell.duration, 0);
            target.ParticleSystem = AttachParticles("sp-Detect Magic", target.Object);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Detect Magic OnBeginRound");
            // get all targets in a 90' cone, apply magical_aura particle systems
            foreach (var obj in ObjList.ListCone(spell.caster, ObjectListFilter.OLC_ALL, spell.spellRange, -45, 90))
            {
                if (obj.type == ObjectType.portal)
                {
                    if ((obj.GetPortalFlags() & PortalFlag.MAGICALLY_HELD) != 0)
                    {
                        AttachParticles("sp-Detect Magic 1 Low", obj);
                    }

                }
                else if (Utilities.obj_is_item(obj))
                {
                    if ((obj.GetItemFlags() & ItemFlag.IS_MAGICAL) != 0)
                    {
                        AttachParticles("sp-Detect Magic 2 Med", obj);
                    }

                }
                else if (obj.type == ObjectType.pc || obj.type == ObjectType.npc)
                {
                    if (obj.IsAffectedBySpell())
                    {
                        AttachParticles("sp-Detect Magic 3 High", obj);
                    }

                }

            }

        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Detect Magic OnEndSpellCast");
        }

    }
}
