
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

namespace Scripts
{

    public class CombatStandardRoutines
    {
        public static void ProtectTheInnocent(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return; // Fuck this script
            Logger.Info("{0}", "ProtectTheInnocent, Attachee: " + attachee.ToString() + " Triggerer: " + triggerer.ToString());
            var handleList = new Dictionary<int, string> {
{8000,"attack"},
{8001,"runoff"},
{8014,"attack"},
{8015,"attack"},
{8021,"attack"},
{8022,"attack"},
{8031,"attack"},
{8039,"attack"},
{8054,"attack"},
{8060,"attack"},
{8069,"attack"},
{8071,"attack"},
{8072,"attack"},
{8714,"attack"},
{8730,"attack"},
{8731,"attack"},
}
            ;
            var specialCases = new Dictionary<int, int> {
{8014,0},
}
            ;
            if ((triggerer.type == ObjectType.pc))
            {
                foreach (var (f, p) in handleList)
                {
                    if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(f)))
                    {
                        var dude = Utilities.find_npc_near(triggerer, f);
                        if ((dude != null))
                        {
                            if ((attachee.GetNameId() == 8014 && dude.GetNameId() == 8000)) // otis & elmo
                            {
                                continue;

                            }
                            else
                            {
                                triggerer.RemoveFollower(dude);
                                dude.FloatLine(20000, triggerer);
                                if ((p == "attack"))
                                {
                                    dude.Attack(triggerer);
                                }
                                else
                                {
                                    dude.RunOff();
                                }

                            }

                        }

                    }

                }

            }

        }
        public static bool should_modify_CR(GameObjectBody attachee)
        {
            return false; // now done in the DLL properly!!! MWAHAHAHA
        }
        // uses npc_get flag # 31
        // party_av_level = get_av_level()
        // if party_av_level > 10 and npc_get(attachee, 31) == 0:
        // return 1
        // else:
        // return 0

        public static int get_av_level()
        {
            // calculates average level of top 50% of the party
            // (rounded down; for odd-sized parties, the middle is included in the top 50%)
            // record every party member's level
            var level_array = new List<GameObjectBody>();
            foreach (var qq in GameSystems.Party.PartyMembers)
            {
                level_array.Add(qq.GetStat(Stat.level));
            }

            // sort
            level_array.sort/*ObjectList*/();
            // calculate average of the top 50%
            var level_sum = 0;
            var rr = range(level_array.Count / 2, level_array.Count);
            foreach (var qq in rr)
            {
                level_sum = level_sum + level_array[qq];
            }

            var party_av_level = level_sum / rr.Count;
            return party_av_level;
        }
        public static FIXME CR_tot_new(FIXME party_av_level, FIXME CR_tot)
        {
            // functions returns the desired total CR (to used for calculating new obj_f_npc_challenge_rating)
            // such that parties with CL > 10 will get a more appropriate XP reward
            // party_av_level - the average CL to be simulated
            // CR_tot - the pre-adjusted total CR (natural CR + CL);
            // e.g. Rogue 15 with -2 CR mod -> CR_tot = 13; the -2 CR mod will (probably) get further adjusted by this function
            var expected_xp = calc_xp_proper(party_av_level, CR_tot);
            var best_CR_fit = CR_tot;
            foreach (var qq in range(CR_tot - 1, Math.Min(5, CR_tot - 2), -1))
            {
                if (Math.Abs(calc_xp_proper(10, qq) - expected_xp) < Math.Abs(calc_xp_proper(10, best_CR_fit) - expected_xp) && Math.Abs(calc_xp_proper(10, qq) - expected_xp) < Math.Abs(calc_xp_proper(10, CR_tot) - expected_xp))
                {
                    best_CR_fit = qq;
                }

            }

            return best_CR_fit;
        }
        public static void CR_mod_new(GameObjectBody attachee, int party_av_level)
        {
            if (party_av_level == -1)
            {
                party_av_level = get_av_level();
            }

            var CR_tot = attachee.GetStat(Stat.level) + attachee.GetInt(obj_f.npc_challenge_rating);
            return (CR_tot_new(party_av_level, CR_tot) - attachee.GetStat(Stat.level));
        }
        public static void modify_CR(GameObjectBody attachee, int party_av_level)
        {
            ScriptDaemon.npc_set(attachee, 31);
            if (party_av_level == -1)
            {
                party_av_level = get_av_level();
            }

            attachee.SetInt(obj_f.npc_challenge_rating, CR_mod_new(attachee, party_av_level));
        }
        public static int calc_xp_proper(int party_av_level, int CR_tot)
        {
            // returns expected XP award
            var xp_gain = party_av_level * 300;
            var xp_mult = Math.Pow(2, long(Math.Abs(CR_tot - party_av_level) / 2));
            if ((CR_tot - party_av_level) % 2 == 1)
            {
                xp_mult = xp_mult * 1.5f;
            }

            if (party_av_level > CR_tot)
            {
                return long(xp_gain / xp_mult);
            }
            else
            {
                return long(xp_gain * xp_mult);
            }

        }
        public static void create_break_free_potion(GameObjectBody attachee)
        {
            // creates a potion of breakfree (the workaround for making AI attempt break free)
            // ALSO, THIS IS USED FOR BREAK FREE
            while ((attachee.FindItemByName(8903) != null))
            {
                attachee.FindItemByName(8903).Destroy();
            }

        }
        public static void Spiritual_Weapon_Begone(GameObjectBody attachee)
        {
            // mwahaha this is no longer necessary!!!

            return;
        }
        public static void timed_restore_state(GameObjectBody attachee, FIXME closest_jones, FIXME orig_strat)
        {
            if (closest_jones.obj_get_int/*Unknown*/(obj_f.hp_damage) >= 1000)
            {
                closest_jones.obj_set_int/*Unknown*/(obj_f.hp_damage, closest_jones.obj_get_int/*Unknown*/(obj_f.hp_damage) - 1000);
                closest_jones.stat_base_set/*Unknown*/(Stat.hp_max, closest_jones.stat_base_get/*Unknown*/(Stat.hp_max) - 1000);
                attachee.SetInt(obj_f.critter_strategy, orig_strat);
            }

        }

    }
}
