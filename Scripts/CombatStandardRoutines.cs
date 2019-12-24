
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

namespace Scripts
{

    public class CombatStandardRoutines
    {
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
            var level_array = new List<int>();
            foreach (var qq in GameSystems.Party.PartyMembers)
            {
                level_array.Add(qq.GetStat(Stat.level));
            }

            // sort
            level_array.Sort();
            // calculate average of the top 50%
            var level_sum = 0;
            var level_sum_count = 0;
            for (var i = level_array.Count / 2; i < level_array.Count; i++)
            {
                level_sum += level_array[i];
                level_sum_count++;
            }

            return level_sum / level_sum_count;
        }

        public static int CR_tot_new(int party_av_level, int CR_tot)
        {
            // functions returns the desired total CR (to used for calculating new obj_f_npc_challenge_rating)
            // such that parties with CL > 10 will get a more appropriate XP reward
            // party_av_level - the average CL to be simulated
            // CR_tot - the pre-adjusted total CR (natural CR + CL);
            // e.g. Rogue 15 with -2 CR mod -> CR_tot = 13; the -2 CR mod will (probably) get further adjusted by this function
            var expected_xp = calc_xp_proper(party_av_level, CR_tot);
            var best_CR_fit = CR_tot;
            for (var qq = CR_tot - 1; qq > Math.Min(5, CR_tot - 2); qq--)
            {
                if (Math.Abs(calc_xp_proper(10, qq) - expected_xp) < Math.Abs(calc_xp_proper(10, best_CR_fit) - expected_xp) && Math.Abs(calc_xp_proper(10, qq) - expected_xp) < Math.Abs(calc_xp_proper(10, CR_tot) - expected_xp))
                {
                    best_CR_fit = qq;
                }

            }

            return best_CR_fit;
        }
        public static int CR_mod_new(GameObjectBody attachee, int party_av_level)
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
            var xp_mult = Math.Pow(2, Math.Abs(CR_tot - party_av_level) / 2);
            if ((CR_tot - party_av_level) % 2 == 1)
            {
                xp_mult = xp_mult * 1.5f;
            }

            if (party_av_level > CR_tot)
            {
                return (int) (xp_gain / xp_mult);
            }
            else
            {
                return (int) (xp_gain * xp_mult);
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

    }
}
