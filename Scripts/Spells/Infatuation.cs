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