
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
    [SpellScript(179)]
    public class FlamingSphere : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Flaming Sphere OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Flaming Sphere OnSpellEffect");
            var dam = Dice.D4;
            spell.duration = 1 * spell.casterLevel;
            var weapon_proto = 4143;
            var weapon_portrait = 4320;
            var monster_proto_id = 14629;
            var npc = spell.caster;
            if (npc.GetNameId() == 8036 || npc.GetNameId() == 14425) // faction 7
            {
                monster_proto_id = 14621;
            }

            if (npc.GetNameId() == 14425 && npc.GetMap() == 5065) // faction 15
            {
                monster_proto_id = 14604;
            }

            // create monster
            spell.SummonMonsters(true, monster_proto_id);
            GameObject monster_obj = null;
            var m_list = ObjList.ListCone(spell.caster, ObjectListFilter.OLC_CRITTERS, 200, -180, 360);
            foreach (var m in m_list)
            {
                if (m.GetNameId() == monster_proto_id && m.ItemWornAt(EquipSlot.WeaponPrimary) == null && m.GetLeader() == spell.caster && Co8.are_spell_flags_null(m))
                {
                    monster_obj = m;
                    Co8.set_spell_flag(m, Co8SpellFlag.FlamingSphere);
                    m.SetInt(obj_f.critter_description_unknown, 20356);
                }

            }

            m_list = ObjList.ListCone(monster_obj, ObjectListFilter.OLC_CRITTERS, 5, -180, 360);
            foreach (var m in m_list)
            {
                if (m != spell.caster && !Co8.is_spell_flag_set(m, Co8SpellFlag.FlamingSphere))
                {
                    if (!m.SavingThrow(spell.dc, SavingThrowType.Reflex, D20SavingThrowFlag.NONE, monster_obj))
                    {
                        m.Damage(monster_obj, DamageType.Fire, dam, D20AttackPower.NORMAL);
                    }
                    else
                    {
                        m.FloatMesFileLine("mes/spell.mes", 30001);
                    }

                }

            }

            monster_obj.AddCondition("Amulet of Mighty Fists", -20, -10);
            monster_obj.AddCondition("Monster Plant", 0, 0);
            monster_obj.SetInt(obj_f.critter_portrait, weapon_portrait);
            var hit_points = 10 * spell.casterLevel;
            hit_points = 25 + hit_points;
            monster_obj.SetBaseStat(Stat.hp_max, hit_points);
            monster_obj.ClearObjectFlag(ObjectFlag.CLICK_THROUGH);
            // monster_obj.npc_flag_set(ONF_NO_ATTACK)
            // add monster to follower list for spell_caster
            if (!spell.caster.HasMaxFollowers())
            {
                spell.caster.RemoveAIFollower(monster_obj);
                spell.caster.AddFollower(monster_obj);
                monster_obj.AddCondition("sp-Endurance", spell.spellId, spell.duration, 0);
            }

            // add monster to target list
            spell.ClearTargets();
            spell.AddTarget(
                monster_obj,
                AttachParticles("sp-Fireball-proj", monster_obj)
            );
            // add spell indicator to spell caster
            spell.caster.AddCondition("sp-Endurance", spell.spellId, spell.duration, 0);
            Logger.Info("{0}", spell.Targets[0].Object);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Flaming Sphere OnBeginRound");
            var dam = Dice.D4;
            var m_list = ObjList.ListCone(spell.Targets[0].Object, ObjectListFilter.OLC_CRITTERS, 5, -180, 360);
            foreach (var m in m_list)
            {
                if (m != spell.Targets[0].Object && m != spell.caster && !Co8.is_spell_flag_set(m, Co8SpellFlag.FlamingSphere))
                {
                    if (!m.SavingThrow(spell.dc, SavingThrowType.Reflex, D20SavingThrowFlag.NONE, spell.Targets[0].Object))
                    {
                        m.Damage(null, DamageType.Fire, dam, D20AttackPower.NORMAL);
                    }
                    else
                    {
                        m.FloatMesFileLine("mes/spell.mes", 30001);
                    }

                }

            }

        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Flaming Sphere OnEndSpellCast");
        }

    }
}
