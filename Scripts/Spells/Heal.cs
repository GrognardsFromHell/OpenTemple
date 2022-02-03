
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
    [SpellScript(565)]
    public class Heal : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Heal OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Heal OnSpellEffect");
            spell.duration = 0;
            var target = spell.Targets[0].Object;
            Logger.Info("target = {0}", target);
            var is_tensers = check_for_tensers(target);
            Logger.Info("is_tensers = {0}", is_tensers);
            var npc = spell.caster; // added so NPC's can use potion
            if (npc.type != ObjectType.pc && npc.GetLeader() == null && spell.casterLevel <= 0)
            {
                spell.casterLevel = 10;
            }

            AttachParticles("sp-Heal", target);
            // check if target is undead
            if (target.IsMonsterCategory(MonsterCategory.undead))
            {
                // Harm target
                target.AddCondition("sp-Harm", spell.spellId, spell.duration, 0);
            }
            else
            {
                // Heal undead
                target.AddCondition("sp-Heal", spell.spellId, spell.duration, 0);
            }

            if (is_tensers)
            {
                replace_tensers(target);
            }

            spell.RemoveTarget(target);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Heal OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Heal OnEndSpellCast");
        }
        public static bool check_for_tensers(GameObject target)
        {
            if (Co8.find_spell_obj_with_flag(target, 6400, Co8SpellFlag.TensersTransformation) != null)
            {
                Co8.destroy_spell_obj_with_flag(target, 6400, Co8SpellFlag.TensersTransformation);
                return true;
            }
            else
            {
                return false;
            }

        }
        public void replace_tensers(GameObject target)
        {
            var spell_obj = GameSystems.MapObject.CreateObject(6400, target.GetLocation());
            Co8.set_spell_flag(spell_obj, Co8SpellFlag.TensersTransformation);
            spell_obj.AddConditionToItem("sp-Feeblemind", 0, 0, 0); // yeah it works, you can put spell effects on items and they will affect the holder
            spell_obj.AddConditionToItem("Saving Throw Resistance Bonus", 0, 9);
            spell_obj.AddConditionToItem("Saving Throw Resistance Bonus", 1, 4);
            spell_obj.AddConditionToItem("Saving Throw Resistance Bonus", 2, 4);
            spell_obj.AddConditionToItem("Attribute Enhancement Bonus", 0, -2); // divine power gives too much strength bonus
            spell_obj.AddConditionToItem("Attribute Enhancement Bonus", 1, 4);
            spell_obj.AddConditionToItem("Attribute Enhancement Bonus", 2, 4);
            spell_obj.AddConditionToItem("Amulet of Natural Armor", 4, 0);
            target.GetItem(spell_obj);
            Logger.Info("spell_obj = {0}", spell_obj);
        }

    }
}
