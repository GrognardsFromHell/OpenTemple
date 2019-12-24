using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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
    [SpellScript(760)]
    public class SummonFamiliar : BaseSpellScript
    {
        private static readonly Dictionary<int, int> familiar_table = new Dictionary<int, int>
        {
            {12045, 14900},
            {12046, 14901},
            {12047, 14902},
            {12048, 14903},
            {12049, 14904},
            {12050, 14905},
            {12051, 14906},
            {12052, 14907},
            {12053, 14908},
            {12054, 14909},
        };

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Summon Familiar OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        }
        // game.particles( "sp-conjuration-conjure", spell.caster )

        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Summon Familiar OnSpellEffect");
            spell.duration = 2147483647;
            var master = spell.caster;
            // get familiar inventory object handle
            var inv_proto = FindFamiliarProto(spell.caster, false);
            var familiar = spell.caster.FindItemByProto(inv_proto);
            if ((get_ID(familiar) != 0))
            {
                return;
            }

            // get the proto_id for this familiar
            var familiar_proto_id = FindFamiliarProto(spell.caster, true);
            if ((familiar_proto_id == 0)) // not a recognized familiar type
            {
                return;
            }

            // creates random ID number
            var ID_number = RandomRange(1, 2147483647);
            ID_number = ID_number ^ RandomRange(1,
                            2147483647); // xor with next "random" number in line, should be more random
            // create familiar
            spell.SummonMonsters(true, familiar_proto_id);
            // get familiar's handle
            var familiar_obj = Co8.GetCritterHandle(spell, familiar_proto_id);
            if ((familiar_obj == null)) // no new familiar present
            {
                return;
            }

            // summoning effect
            // game.particles( 'Orb-Summon-Air-Elemental', familiar_obj )
            // assigns familiar ownership
            set_ID(familiar_obj, ID_number);
            set_ID(familiar, ID_number);
            // game.particles( "sp-summon monster II", game.party[0] )
            // sets familiar's stat's and bonuses depending on it's masters level
            var master_level = GetLevel(spell.caster);
            var f_level = ((master_level + 1) / 2);
            var f_hp = ((spell.caster.GetStat(Stat.hp_max)) / 2); // familiar's hp = i/2 masters hp
            var base_hp = familiar_obj.GetStat(Stat.hp_max); // familiar's base hp from proto
            var prev_max_hp =
                familiar.GetInt(obj_f
                    .item_pad_i_1); // familiar's max hp from last time summoned ( 0 if never summoned before)
            var prev_curr_hp =
                familiar.GetInt(obj_f
                    .item_pad_i_2); // familiar's current xp from last time stowed ( 0 if never summoed before)
            int new_hp;
            if ((base_hp <= f_hp)) // if 1/2 master's hp is greater than base hp from proto, will use 1/2 masters hp
            {
                new_hp = familiar_obj.SetBaseStat(Stat.hp_max, f_hp);
            }

            var curr_max_hp = familiar_obj.GetStat(Stat.hp_max); // familiar's max hp from current summons
            var hp_diff =
                (curr_max_hp -
                 prev_max_hp); // difference between max hp from last time summoned and max hp now ( 0 if master has not gained a level since)
            if ((prev_max_hp != 0)) // has been summoned before
            {
                int hp_now;
                if ((hp_diff >= 1)) // adds gained hp if master has gained hp since last time summoned
                {
                    hp_now = prev_curr_hp + hp_diff;
                }
                else
                {
                    hp_now = prev_curr_hp;
                }

                var dam = Dice.Parse("1d1");
                dam = dam.WithCount(curr_max_hp - hp_now);
                if ((dam.Count >= 1))
                {
                    familiar_obj.Damage(null, DamageType.Force, dam, D20AttackPower.NORMAL);
                }
            }

            // This next bit gives the familiar it's masters BAB.  The familiar should have a BAB (without the masters BAB) of zero, but since
            // the game engine doesn't allow for Weapon Finesse with natural attacks( which would let the familiar use their dexterity modifier
            // instead of the strength modifier), I fiddled with the  "to hit" in the protos to counteract the negative attack modifier do to
            // low strength and add the dexterity modifier. - Ceruleran the Blue ##
            var f_to_hit = spell.caster.GetBaseStat(Stat.attack_bonus);
            var new_to_hit = familiar_obj.AddCondition("To Hit Bonus", f_to_hit, 0);
            var new_int = familiar_obj.SetBaseStat(Stat.intelligence, (5 + f_level)); // familiar INT bonus
            familiar_obj.SetInt(obj_f.npc_ac_bonus, (f_level)); // Natrual Armor bonus
            if ((master_level >= 11))
            {
                var spell_resistance =
                    familiar_obj.AddCondition("Monster Spell Resistance", (5 + master_level), 0); // spell resistance
            }

            // familiar uses masters saving throw bonuses if they are higher than it's own.
            var fortitude_bonus = Fortitude(spell.caster);
            if ((fortitude_bonus >= 3))
            {
                familiar_obj.SetInt(obj_f.npc_save_fortitude_bonus, fortitude_bonus);
            }

            var reflex_bonus = Reflex(spell.caster);
            if ((reflex_bonus >= 3))
            {
                familiar_obj.SetInt(obj_f.npc_save_reflexes_bonus, reflex_bonus);
            }

            var will_bonus = Will(spell.caster);
            if ((will_bonus >= 1))
            {
                familiar_obj.SetInt(obj_f.npc_save_willpower_bonus, will_bonus);
            }

            // add familiar to follower list for spell_caster
            if (!(spell.caster.HasMaxFollowers()))
            {
                spell.caster.AddFollower(familiar_obj);
            }
            else
            {
                spell.caster.AddAIFollower(familiar_obj);
            }

            // add familiar_obj to d20initiative, and set initiative to spell_caster's
            var caster_init_value = spell.caster.GetInitiative();
            familiar_obj.AddToInitiative();
            familiar_obj.SetInitiative(caster_init_value);
            UiSystems.Combat.Initiative.UpdateIfNeeded();
            // familiar should disappear when duration is over, apply "TIMED_DISAPPEAR" condition
            // familiar_obj.condition_add_with_args( 'sp-Summoned', spell.id, spell.duration, 0 )
            // add familiar to target list
            spell.ClearTargets();
            spell.AddTarget(familiar_obj);
            spell.EndSpell();
        }

        public override void OnBeginRound(SpellPacketBody spell)
        {
            var familiar_obj = spell.Targets[0].Object;
            if (familiar_obj.GetStat(Stat.hp_current) <= -10)
            {
                // Remove familiar if dead after one day.
                StartTimer(86400000, () => RemoveDead(spell.caster, familiar_obj)); // 1000 = 1 second
                return;
            }

            if (!(GameSystems.Party.PartyMembers).Contains(familiar_obj))
            {
                familiar_obj.Destroy();
                foreach (var (f, p) in familiar_table)
                {
                    var itemA = spell.caster.FindItemByProto(f);
                    if (itemA != null)
                    {
                        clear_ID(itemA);
                    }
                }
            }

            Logger.Info("Summon Familiar OnBeginRound");
        }

        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Summon Familiar OnEndSpellCast");
            foreach (var (f, p) in familiar_table)
            {
                var itemA = spell.caster.FindItemByProto(f);
                if (itemA != null)
                {
                    clear_ID(itemA);
                }
            }
        }

        public static int FindFamiliarProto(GameObjectBody master, bool x)
        {
            // Returns either the familiar creature's proto ID ( x = 1 ) or the familiar inventory object ( x = 0 )
            foreach (var (f, p) in familiar_table)
            {
                var itemC = master.FindItemByProto(f);
                if ((itemC != null))
                {
                    if (x)
                    {
                        return p;
                    }
                    else
                    {
                        return f;
                    }
                }
            }

            return 0;
        }

        public static GameObjectBody GetFamiliarHandle(SpellPacketBody spell, int familiar_proto_id)
        {
            // Returns a handle that can be used to manipulate the familiar creature object
            foreach (var npc in ObjList.ListVicinity(spell.aoeCenter.location, ObjectListFilter.OLC_CRITTERS))
            {
                if ((npc.GetNameId() == familiar_proto_id))
                {
                    if (get_ID(npc) == 0)
                    {
                        return npc;
                    }
                }
            }

            return null;
        }

        public static int get_ID(GameObjectBody obj)
        {
            // Returns embedded ID number
            return obj.GetInt(obj_f.secretdoor_dc);
        }

        public static int set_ID(GameObjectBody obj, int val)
        {
            // Embeds ID number into mobile object.  Returns ID number.
            obj.SetInt(obj_f.secretdoor_dc, val);
            return obj.GetInt(obj_f.secretdoor_dc);
        }

        public static void clear_ID(GameObjectBody obj)
        {
            // Clears embedded ID number from mobile object
            obj.SetInt(obj_f.secretdoor_dc, 0);
        }

        public static GameObjectBody FindMaster(GameObjectBody npc)
        {
            // Not actually used in the spell, but could be handy in the future.  Returns the character that is the master for a given summoned familiar ( npc )
            foreach (var p_master in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_CRITTERS))
            {
                foreach (var (x, y) in familiar_table)
                {
                    var item = p_master.FindItemByProto(x);
                    if ((item != null))
                    {
                        if ((get_ID(item) == get_ID(npc)))
                        {
                            return p_master;
                        }
                    }
                }
            }

            return null;
        }

        public static int GetLevel(GameObjectBody npc)
        {
            // Returns characters combined sorcerer and wizard levels
            var level = npc.GetStat(Stat.level_sorcerer) + npc.GetStat(Stat.level_wizard);
            return level;
        }

        public static int Fortitude(GameObjectBody npc)
        {
            // Returns Fortitude Save Bonus for all the casters class levels
            var bonus = 0;
            var level = npc.GetStat(Stat.level_barbarian) + npc.GetStat(Stat.level_cleric) +
                        npc.GetStat(Stat.level_druid) + npc.GetStat(Stat.level_fighter) +
                        npc.GetStat(Stat.level_paladin) + npc.GetStat(Stat.level_ranger) + npc.GetStat(Stat.level_monk);
            if ((level != 0))
            {
                bonus = ((level / 2) + 2);
            }

            level = npc.GetStat(Stat.level_bard) + npc.GetStat(Stat.level_rogue) + npc.GetStat(Stat.level_sorcerer) +
                    npc.GetStat(Stat.level_wizard);
            if ((level != 0))
            {
                bonus = bonus + (level / 3);
            }

            return bonus;
        }

        public static int Reflex(GameObjectBody npc)
        {
            // Returns Reflex Save Bonus for all the casters class levels
            var bonus = 0;
            var level = npc.GetStat(Stat.level_barbarian) + npc.GetStat(Stat.level_cleric) +
                        npc.GetStat(Stat.level_druid) + npc.GetStat(Stat.level_fighter) +
                        npc.GetStat(Stat.level_paladin) + npc.GetStat(Stat.level_sorcerer) +
                        npc.GetStat(Stat.level_wizard);
            if ((level != 0))
            {
                bonus = (level / 3);
            }

            level = npc.GetStat(Stat.level_ranger) + npc.GetStat(Stat.level_rogue) + npc.GetStat(Stat.level_monk) +
                    npc.GetStat(Stat.level_bard);
            if ((level != 0))
            {
                bonus = bonus + ((level / 2) + 2);
            }

            return bonus;
        }

        public static int Will(GameObjectBody npc)
        {
            // Returns Will Save Bonus for all the casters class levels
            var bonus = 0;
            var level = npc.GetStat(Stat.level_bard) + npc.GetStat(Stat.level_cleric) + npc.GetStat(Stat.level_druid) +
                        npc.GetStat(Stat.level_monk) + npc.GetStat(Stat.level_sorcerer) +
                        npc.GetStat(Stat.level_wizard);
            if ((level != 0))
            {
                bonus = ((level / 2) + 2);
            }

            level = npc.GetStat(Stat.level_barbarian) + npc.GetStat(Stat.level_fighter) +
                    npc.GetStat(Stat.level_paladin) + npc.GetStat(Stat.level_ranger) + npc.GetStat(Stat.level_rogue);
            if ((level != 0))
            {
                bonus = bonus + (level / 3);
            }

            return bonus;
        }

        public static void RemoveDead(GameObjectBody npc, GameObjectBody critter)
        {
            if (critter.GetStat(Stat.hp_current) <= -10)
            {
                npc.RemoveFollower(critter);
            }

            return;
        }
    }
}