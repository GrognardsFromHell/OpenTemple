
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
    [SpellScript(497)]
    public class TensersTransformation : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Tensers Transformation OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Tensers Transformation OnSpellEffect");
            spell.duration = 1 * spell.casterLevel;
            var dam = Dice.Parse("1d1");
            dam = dam.WithCount(spell.casterLevel);
            spell.dc = 0;
            if (Co8.find_spell_obj_with_flag(spell.caster, 6400, Co8SpellFlag.TensersTransformation) == null)
            {
                var weapon = spell.caster.ItemWornAt(EquipSlot.WeaponPrimary);
                if (weapon != null)
                {
                    Co8.set_spell_flag(weapon, Co8SpellFlag.TensersTransformation);
                    weapon.SetInt(obj_f.weapon_type, 12);
                    weapon.SetItemFlag(ItemFlag.NO_DROP);
                    spell.dc = weapon.GetNameId(); // OKAY I am trying to avoid stuff like this but I need a variable, not otherwise used,and unique to this spell that I know will still be alive in OnEndSpellcast
                }

                var spell_obj = GameSystems.MapObject.CreateObject(6400, spell.caster.GetLocation());
                Co8.set_spell_flag(spell_obj, Co8SpellFlag.TensersTransformation);
                spell_obj.AddConditionToItem("sp-Feeblemind", 0, 0, 0); // yeah it works, you can put spell effects on items and they will affect the holder
                spell_obj.AddConditionToItem("Saving Throw Resistance Bonus", 0, 9);
                spell_obj.AddConditionToItem("Saving Throw Resistance Bonus", 1, 4);
                spell_obj.AddConditionToItem("Saving Throw Resistance Bonus", 2, 4);
                spell_obj.AddConditionToItem("Attribute Enhancement Bonus", 0, -2); // divine power gives too much strength bonus
                spell_obj.AddConditionToItem("Attribute Enhancement Bonus", 1, 4);
                spell_obj.AddConditionToItem("Attribute Enhancement Bonus", 2, 4);
                spell_obj.AddConditionToItem("Amulet of Natural Armor", 4, 0);
                spell.caster.GetItem(spell_obj);
                spell.caster.AddCondition("sp-Divine Power", spell.spellId, spell.duration, 0); // to make base attack bonus equal caster level did not put on item in order that a spell marker would appear on casters icon
                spell.caster.Damage(null, DamageType.Force, dam, D20AttackPower.NORMAL);
            }
            else
            {
                spell.caster.FloatMesFileLine("mes/spell.mes", 16007);
                spell.EndSpell();
            }

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Tensers Transformation OnBeginRound");
            Logger.Info("{0}", spell.Targets);
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Tensers Transformation OnEndSpellCast");
            Co8.destroy_spell_obj_with_flag(spell.caster, 6400, Co8SpellFlag.TensersTransformation);
            var item_wielded = spell.caster.ItemWornAt(EquipSlot.WeaponPrimary);
            if ((item_wielded != null))
            {
                if ((item_wielded.GetNameId() == spell.dc) && (Co8.is_spell_flag_set(item_wielded, Co8SpellFlag.TensersTransformation)))
                {
                    var new_weapon = GameSystems.MapObject.CreateObject(spell.dc, spell.caster.GetLocation());
                    item_wielded.SetInt(obj_f.weapon_type, new_weapon.GetInt(obj_f.weapon_type));
                    Co8.unset_spell_flag(item_wielded, Co8SpellFlag.TensersTransformation);
                    item_wielded.ClearItemFlag(ItemFlag.NO_DROP);
                    new_weapon.Destroy();
                }

            }
            else
            {
                if (spell.dc != 0)
                {
                    var weapon = Co8.find_spell_obj_with_flag(spell.caster, spell.dc, Co8SpellFlag.TensersTransformation);
                    if (weapon != null) // this shouldn't ever happen but better check
                    {
                        var new_weapon = GameSystems.MapObject.CreateObject(spell.dc, spell.caster.GetLocation());
                        weapon.SetInt(obj_f.weapon_type, new_weapon.GetInt(obj_f.weapon_type));
                        Co8.unset_spell_flag(weapon, Co8SpellFlag.TensersTransformation);
                        weapon.ClearItemFlag(ItemFlag.NO_DROP);
                        new_weapon.Destroy();
                    }

                }

            }

        }

    }
}
