
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
    [SpellScript(434)]
    public class Silence : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Silence OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-illusion-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Silence OnSpellEffect");
            var npc = spell.caster; // added so NPC's can use wand/potion/scroll
            if (npc.type != ObjectType.pc && npc.GetLeader() == null && spell.casterLevel <= 0)
            {
                spell.casterLevel = 8;
            }

            if (npc.GetNameId() == 14425 && GetGlobalVar(711) == 1)
            {
                spell.casterLevel = 6;
                spell.dc = 17;
            }

            spell.duration = 10 * spell.casterLevel;
            // test whether we targeted the ground or an object
            if (spell.IsObjectSelected())
            {
                var target_item = spell.Targets[0];
                // allow Will saving throw to negate
                if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    // saving throw successful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    AttachParticles("Fizzle", target_item.Object);
                    spell.RemoveTarget(target_item.Object);
                }
                else
                {
                    // put sp-Silence condition on target
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    var spell_obj_partsys_id = AttachParticles("sp-Silence", target_item.Object);
                    target_item.Object.AddCondition("sp-Silence", spell.spellId, spell.duration, 0, spell_obj_partsys_id);
                }

            }
            else
            {
                // spawn one spell_object object
                GameObjectBody spell_obj;
                if (npc.GetNameId() == 14425 && npc.GetMap() == 5065 && GetGlobalVar(711) == 1)
                {
                    SetGlobalVar(711, 2);
                    spell_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, new locXY(486, 495));
                }
                else
                {
                    spell_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, spell.aoeCenter);
                }

                // add to d20initiative
                var caster_init_value = spell.caster.GetInitiative();
                spell_obj.InitD20Status();
                spell_obj.SetInitiative(caster_init_value);
                // put sp-Silence condition on obj
                var spell_obj_partsys_id = AttachParticles("sp-Silence", spell_obj);
                spell_obj.AddCondition("sp-Silence", spell.spellId, spell.duration, 0, spell_obj_partsys_id);
            }

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Silence OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Silence OnEndSpellCast");
        }
        public override void OnAreaOfEffectHit(SpellPacketBody spell)
        {
            Logger.Info("Silence OnAreaOfEffectHit");
        }
        public override void OnSpellStruck(SpellPacketBody spell)
        {
            Logger.Info("Silence OnSpellStruck");
        }

    }
}
