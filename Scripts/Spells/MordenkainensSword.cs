
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
    [SpellScript(323)]
    public class MordenkainensSword : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Mordenkainens Sword OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Mordenkainens Sword OnSpellEffect");
            spell.duration = 1 * spell.casterLevel;
            var hp_bonus = 25;
            var weapon_proto = 4143;
            var weapon_portrait = 7980;
            var monster_proto_id = 14629;
            var npc = spell.caster;
            if (npc.GetNameId() == 8036 || npc.GetNameId() == 14425) // faction 7
            {
                monster_proto_id = 14621;
                spell.duration = 10;
                hp_bonus = 1000;
            }

            if (npc.GetNameId() == 14425 && npc.GetMap() == 5065) // faction 15
            {
                monster_proto_id = 14604;
            }

            // determine focus
            var focus = 0;
            if ((spell.caster.type != ObjectType.pc) && (spell.caster.GetLeader() == null))
            {
                // for NPCs not in game party
                focus = 1;
            }
            else
            {
                if (spell.caster.GetMoney() >= 25000)
                {
                    focus = 1;
                }

            }

            // check for focus
            if (focus == 1)
            {
                // create monster
                spell.SummonMonsters(true, monster_proto_id);
                GameObjectBody monster_obj = null;
                var m_list = ObjList.ListCone(spell.caster, ObjectListFilter.OLC_CRITTERS, 200, -180, 360);
                foreach (var m in m_list)
                {
                    if (m.GetNameId() == monster_proto_id && m.ItemWornAt(EquipSlot.WeaponPrimary) == null && m.GetLeader() == spell.caster && Co8.are_spell_flags_null(m))
                    {
                        monster_obj = m;
                        Co8.set_spell_flag(m, Co8SpellFlag.MordenkainensSword);
                        m.SetInt(obj_f.critter_description_unknown, 20355);
                        monster_obj.ClearObjectFlag(ObjectFlag.CLICK_THROUGH);
                    }

                }

                if (!Co8.is_in_party(spell.caster))
                {
                    monster_obj.ClearObjectFlag(ObjectFlag.INVULNERABLE);
                    monster_obj.AddCondition("Monster Energy Immunity", "Force", 0);
                }

                monster_obj.AddCondition("Monster Plant", 0, 0);
                monster_obj.SetInt(obj_f.critter_portrait, weapon_portrait);
                var hit_points = 10 * spell.casterLevel;
                hit_points = hp_bonus + hit_points;
                monster_obj.SetBaseStat(Stat.hp_max, hit_points);
                // equip the tempman with the appropriate weapon
                var weapon_obj = GameSystems.MapObject.CreateObject(weapon_proto, monster_obj.GetLocation());
                weapon_obj.SetInt(obj_f.weapon_damage_dice, 772);
                weapon_obj.SetInt(obj_f.weapon_attacktype, (int) DamageType.Force);
                weapon_obj.AddConditionToItem("Armor Bonus", -7, 0);
                var to_hit = spell.casterLevel + 3 - monster_obj.GetBaseStat(Stat.attack_bonus);
                if ((Stat) spell.spellClass == Stat.level_wizard)
                {
                    to_hit = to_hit + spell.caster.GetStat(Stat.int_mod);
                }
                else
                {
                    to_hit = to_hit + spell.caster.GetStat(Stat.cha_mod);
                }

                weapon_obj.AddConditionToItem("To Hit Bonus", to_hit, 0);
                weapon_obj.AddConditionToItem("Damage Bonus", 3, 0);
                monster_obj.GetItem(weapon_obj);
                weapon_obj.SetItemFlag(ItemFlag.NO_TRANSFER);
                monster_obj.WieldBestInAllSlots();
                // add monster to follower list for spell_caster
                if (Co8.is_in_party(spell.caster))
                {
                    if (!spell.caster.HasMaxFollowers())
                    {
                        spell.caster.RemoveAIFollower(monster_obj);
                        spell.caster.AddFollower(monster_obj);
                        monster_obj.AddCondition("sp-Endurance", spell.spellId, spell.duration, 0);
                    }

                }

                // add monster to target list
                spell.ClearTargets();
                spell.AddTarget(monster_obj, AttachParticles("sp-Spell Resistance", monster_obj));
                // add spell indicator to spell caster
                spell.caster.AddCondition("sp-Endurance", spell.spellId, spell.duration, 0);
            }
            else
            {
                // no focus
                spell.caster.FloatMesFileLine("mes/spell.mes", 16009);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Mordenkainens Sword OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Mordenkainens Sword OnEndSpellCast");
        }

    }
}
