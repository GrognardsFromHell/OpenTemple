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
    [SpellScript(758)]
    public class Infatuation : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Infatuation OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
        }

        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Infatuation OnSpellEffect");
            foreach (var target_item in spell.Targets)
            {
                if (!target_item.Object.IsFriendly(spell.caster) && target_item.Object.GetNameId() != 14455)
                {
                    if ((target_item.Object.type == ObjectType.pc) || (target_item.Object.type == ObjectType.npc))
                    {
                        spell.caster.AddAIFollower(target_item.Object);
                        // add target to initiative, just in case
                        target_item.Object.AddToInitiative();
                        UiSystems.Combat.Initiative.UpdateIfNeeded();
                    }
                    else
                    {
                        // not a person
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 31001);
                        AttachParticles("Fizzle", target_item.Object);
                        spell.RemoveTarget(target_item.Object);
                    }
                }
                else
                {
                    // can't target friendlies
                    AttachParticles("Fizzle", target_item.Object);
                    spell.RemoveTarget(target_item.Object);
                }
            }

            // can't target friendlies
            spell.EndSpell();
        }

        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Infatuation OnBeginRound");
        }

        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Infatuation OnEndSpellCast");
        }
    }
}