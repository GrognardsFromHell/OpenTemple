
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

    public class Livonya
    {
        // game.global_flags[403] - Test Mode flag
        // Enables float message (which indicate chosen strategy, etc)
        // these are scripting routines I use all the time					##
        // note that I didn't write them all I just use them and modify them as needed	##
        // - Livonya									##
        // This tags and destroys the NPC's ranged weapon, melee weapon and shield		##
        // As a fail safe, if you run this script with an NPC that has muliple weapons		##
        // The extra weapons will vanish							##
        // obj_f_pad_i_7 records the type of thrown weapon					##
        // obj_f_pad_i_9 records the type of melee weapon					##
        // obj_f_pad_i_8 records quantity of thrown weapon					##

        public static void tag_weapons(GameObjectBody npc)
        {
            npc.WieldBestInAllSlots(); // equip something
            var weaponx = npc.ItemWornAt(EquipSlot.WeaponPrimary); // find weapon being used
            while ((weaponx != null))
            {
                // while(weaponx != OBJ_HANDLE_NULL):			## start loop
                npc.WieldBestInAllSlots(); // equip something
                var shieldx = npc.ItemWornAt(EquipSlot.Shield); // find shield being used
                if (shieldx != null) // was a shield found?
                {
                    var shield = shieldx.GetNameId(); // get shield's protos name
                    npc.SetInt(obj_f.pad_i_8, shield); // record shield's name
                    shieldx.Destroy(); // destroy shield
                    npc.WieldBestInAllSlots(); // equip something
                }

                var x1 = weaponx.GetInt(obj_f.weapon_flags); // check weapon flag type
                if ((x1 == 1024)) // if weapon is ranged
                {
                    var weapon = weaponx.GetNameId(); // get weapon's protos name
                    npc.SetInt(obj_f.pad_i_7, weapon); // record ranged weapon's name
                    weaponx.Destroy(); // destroy ranged weapon
                }
                else
                {
                    // else:						## if weapon is not ranged
                    var weapon = weaponx.GetNameId(); // get weapon's protos name
                    npc.SetInt(obj_f.pad_i_9, weapon); // record melee weapon's name
                    weaponx.Destroy(); // destroy melee weapon
                }

                npc.WieldBestInAllSlots(); // equip something
                weaponx = npc.ItemWornAt(EquipSlot.WeaponPrimary); // find weapon being used
            }

            return;
        }
        // This recalls all weapons and shield, and resets variables to 0			##

        public static void get_everything(GameObjectBody npc)
        {
            var weapon1 = npc.GetInt(obj_f.pad_i_9); // recall melee weapon
            var weapon2 = npc.GetInt(obj_f.pad_i_7); // recall ranged weapon
            var shield = npc.GetInt(obj_f.pad_i_8); // recall shield
            if (weapon1 == 0 && weapon2 == 0 && shield == 0) // nothing set?
            {
                return; // end routine
            }

            var weaponx1 = npc.FindItemByProto(weapon1); // does melee weapon already exist?
            if (weapon1 != 0 && weaponx1 == null) // if melee weapons doesn't exist then...
            {
                Utilities.create_item_in_inventory(weapon1, npc); // create melee weapon
            }

            var weaponx2 = npc.FindItemByProto(weapon2); // does ranged weapon already exist?
            if (weapon2 != 0 && weaponx2 == null) // if ranged weapon doesn't exist then...
            {
                Utilities.create_item_in_inventory(weapon2, npc); // create ranged weapon
            }

            var shieldx = npc.FindItemByProto(shield); // does shield already exist?
            if (shield != 0 && shieldx == null) // if shield doesn't exist then...
            {
                Utilities.create_item_in_inventory(shield, npc); // create shield
            }

            npc.WieldBestInAllSlots(); // equip everything
            npc.SetInt(obj_f.pad_i_9, 0); // reset to 0
            npc.SetInt(obj_f.pad_i_7, 0); // reset to 0
            npc.SetInt(obj_f.pad_i_8, 0); // reset to 0
            return;
        }
        // This switches to melee weapon and shield						##

        public static void get_melee_weapon(GameObjectBody npc)
        {
            if (Utilities.critter_is_unconscious(npc) == 1) // no reason to run script if unconcious
            {
                return;
            }

            var weapon1 = npc.GetInt(obj_f.pad_i_9); // recall melee weapon
            if (weapon1 == 0) // is this an actual number?
            {
                return; // if no, then end routine
            }

            var item = npc.ItemWornAt(EquipSlot.WeaponPrimary); // find ranged weapon being used
            if (item != null) // was ranged weapon found?
            {
                item.Destroy(); // if yes, then destroy ranged weapon
            }

            var weaponx1 = npc.FindItemByProto(weapon1); // does melee weapon already exist?
            if (weapon1 != 0 && weaponx1 == null) // if it doesn't exist then...
            {
                Utilities.create_item_in_inventory(weapon1, npc); // create melee weapon
            }

            var shield = npc.GetInt(obj_f.pad_i_8); // recall shield
            var shieldx = npc.FindItemByProto(shield); // does shield already exist?
            if (shield != 0 && shieldx == null) // if shield doesn't exist then...
            {
                Utilities.create_item_in_inventory(shield, npc); // create shield
            }

            npc.WieldBestInAllSlots(); // equip everything
            return;
        }
        // This switches to ranged weapon only							##

        public static void get_ranged_weapon(GameObjectBody npc)
        {
            if (Utilities.critter_is_unconscious(npc) == 1) // no reason to run script if unconcious
            {
                return;
            }

            var weapon2 = npc.GetInt(obj_f.pad_i_7); // recall ranged weapon
            if (weapon2 == 0) // is this an actual number?
            {
                return; // if no, then end routine
            }

            var item = npc.ItemWornAt(EquipSlot.WeaponPrimary); // find melee weapon being used
            if (item != null) // was melee weapon found?
            {
                item.Destroy(); // if yes, then destroy melee weapon
            }

            var item2 = npc.ItemWornAt(EquipSlot.Shield); // find shield being used
            if (item2 != null) // was shield found?
            {
                item2.Destroy(); // if yes, then destroy shield
            }

            // NOTE: it is important to destroy shield.
            // A one-handed ranged weapon will not equip if a shield is present.
            // Weird, but true, at least in all of my tests.
            var weaponx2 = npc.FindItemByProto(weapon2); // does ranged weapon already exist?
            if (weapon2 != 0 && weaponx2 == null) // if it doesn't exist then...
            {
                Utilities.create_item_in_inventory(weapon2, npc); // create ranged weapon
            }

            npc.WieldBestInAllSlots(); // equip everything
            return;
        }
        // This checks for any ammo. Not specific to ranged weapon.				##

        public static int detect_ammo(GameObjectBody npc)
        {
            var itemcount = 5004;
            while ((itemcount <= 5099))
            {
                // while (itemcount <= 5099):				## creates loop
                if ((npc.FindItemByName(itemcount) != null))
                {
                    return 1; // return 1 if ammo exists
                }

                itemcount = itemcount + 1;
            }

            return 0; // return 0 if no ammo
        }
        // This determines if weapon equiped is ranged or melee					##

        public static int detect_weapon_type(GameObjectBody npc)
        {
            var weapon1 = npc.GetInt(obj_f.pad_i_9); // recall melee weapon
            var weapon2 = npc.GetInt(obj_f.pad_i_7); // recall ranged weapon
            var weaponx = npc.ItemWornAt(EquipSlot.WeaponPrimary); // find weapon being used
            if (weaponx.GetNameId() == weapon1 && weaponx != null) // was melee weapon found?
            {
                return 1; // return melee weapon equiped (1)
            }

            if (weaponx.GetNameId() == weapon2 && weaponx != null) // was ranged weapon found?
            {
                return 2; // return ranged weapon equiped (2)
            }

            return 0; // return no weapon equiped (0)
        }
        // This will try to pick a strategy for a melee character (no reach weapon)		##
        // ##
        // It takes into account Feats, Health, and other limiting factors			##
        // ##
        // Useful for any Humanoid melee NPC type						##
        // NOTE:  All float messages need to be removed before final release!!!!

        public static void get_melee_strategy(GameObjectBody npc)
        {
            if (Utilities.critter_is_unconscious(npc) == 1) // no reason to run script if unconcious
            {
                return;
            }

            var enemyclose = 0;
            var enemymedium = 0;
            var enemyfar = 0;
            var enemyveryfar = 0;
            var prone = 0;
            var dying = 0;
            var helpless = 0;
            var friendclose = 0;
            var friendmedium = 0;
            var casterclose = 0;
            var webbed = break_free(npc, 3); // runs break_free script with range set to 3
            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                // for obj in game.party[0].group_list():	## find number of enemy and distances
                // 5' distance is evidently equivalent to obj.distance_to(npc) == 3, and 10' to 8
                if ((obj.DistanceTo(npc) < 3 && Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 1 && (!obj.D20Query(D20DispatcherKey.QUE_Prone)) && (!obj.D20Query(D20DispatcherKey.QUE_CoupDeGrace))))
                {
                    enemyclose = enemyclose + 1;
                }

                if ((obj.DistanceTo(npc) < 3 && Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 10 && (!obj.D20Query(D20DispatcherKey.QUE_Prone)) && (!obj.D20Query(D20DispatcherKey.QUE_CoupDeGrace)) && (obj.GetStat(Stat.level_sorcerer) >= 2 || obj.GetStat(Stat.level_wizard) >= 2)))
                {
                    casterclose = casterclose + 1;
                }

                if ((obj.DistanceTo(npc) < 8 && obj.DistanceTo(npc) >= 3 && Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 1 && (!obj.D20Query(D20DispatcherKey.QUE_Prone)) && (!obj.D20Query(D20DispatcherKey.QUE_CoupDeGrace))))
                {
                    enemymedium = enemymedium + 1;
                }

                if ((obj.DistanceTo(npc) <= 20 && obj.DistanceTo(npc) >= 8 && Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 1 && (!obj.D20Query(D20DispatcherKey.QUE_Prone)) && (!obj.D20Query(D20DispatcherKey.QUE_CoupDeGrace))))
                {
                    enemyfar = enemyfar + 1;
                }

                if ((obj.DistanceTo(npc) <= 30 && obj.DistanceTo(npc) >= 21 && Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 1 && (!obj.D20Query(D20DispatcherKey.QUE_Prone)) && (!obj.D20Query(D20DispatcherKey.QUE_CoupDeGrace))))
                {
                    enemyveryfar = enemyveryfar + 1;
                }

                if ((obj.DistanceTo(npc) <= 10 && obj.D20Query(D20DispatcherKey.QUE_Prone) && obj.GetStat(Stat.hp_current) >= 1 && (!obj.D20Query(D20DispatcherKey.QUE_Helpless))))
                {
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        obj.FloatMesFileLine("mes/skill_ui.mes", 106);
                    }

                    prone = prone + 1;
                }

                if ((obj.DistanceTo(npc) <= 5 && obj.D20Query(D20DispatcherKey.QUE_Helpless) && obj.GetStat(Stat.hp_current) >= 0))
                {
                    helpless = helpless + 1;
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        obj.FloatMesFileLine("mes/skill_ui.mes", 107);
                    }

                }

                if ((obj.DistanceTo(npc) <= 5 && obj.GetStat(Stat.hp_current) <= -1 && (!obj.D20Query(D20DispatcherKey.QUE_Dead))))
                {
                    dying = dying + 1;
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        obj.FloatMesFileLine("mes/skill_ui.mes", 108);
                    }

                }

            }

            foreach (var obj in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((obj.DistanceTo(npc) <= 8 && Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 1 && (!obj.D20Query(D20DispatcherKey.QUE_Prone)) && (!obj.D20Query(D20DispatcherKey.QUE_CoupDeGrace)) && obj.GetLeader() == null && obj != npc))
                {
                    friendclose = friendclose + 1;
                }

                if ((obj.DistanceTo(npc) >= 9 && obj.DistanceTo(npc) <= 16 && Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 1 && (!obj.D20Query(D20DispatcherKey.QUE_Prone)) && (!obj.D20Query(D20DispatcherKey.QUE_CoupDeGrace)) && obj.GetLeader() == null && obj != npc))
                {
                    friendmedium = friendmedium + 1;
                }

            }

            var hpnownpc = npc.GetStat(Stat.hp_current);
            var hpmaxnpc = npc.GetStat(Stat.hp_max);
            var dumbass = npc.GetStat(Stat.intelligence) < 8;
            if ((hpnownpc < (hpmaxnpc / 2) && (npc.FindItemByName(8006) || npc.FindItemByName(8007) || npc.FindItemByName(8014) || npc.FindItemByName(8037))))
            {
                npc.SetInt(obj_f.critter_strategy, 2); // Only Use Potion w/ 5 Foot Step (2)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 602);
                }

                return;
            }

            if ((webbed && enemyclose == 0))
            {
                npc.SetInt(obj_f.critter_strategy, 0); // Default (allows for break free)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 600);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.COMBAT_EXPERTISE) && hpnownpc <= (hpmaxnpc / 2) && casterclose == 0 && enemyclose >= 1 && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 38); // Combat Expertise 2 (38)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 638);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.COMBAT_EXPERTISE) && enemyclose >= 2))
            {
                npc.SetInt(obj_f.critter_strategy, 35); // Combat Expertise (35)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 635);
                }

                return;
            }

            if ((hpnownpc <= (hpmaxnpc / 3) && casterclose == 0 && enemyclose >= 2))
            {
                npc.SetInt(obj_f.critter_strategy, 37); // Evasive maneuver 2 (37)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 637);
                }

                return;
            }

            if ((hpnownpc <= (hpmaxnpc / 2) && casterclose == 0 && enemyclose >= 1 && (friendclose >= 1 || friendmedium >= 2)))
            {
                var num = RandomRange(1, 4);
                if (num == 1)
                {
                    npc.SetInt(obj_f.critter_strategy, 36); // Evasive maneuver (36)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 636);
                    }

                    return;
                }
                else
                {
                    npc.SetInt(obj_f.critter_strategy, 37); // Evasive maneuver 2 (37)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 637);
                    }

                    return;
                }

            }

            if ((npc.HasFeat(FeatId.BARBARIAN_RAGE) && npc.HasFeat(FeatId.POWER_ATTACK) && ((casterclose == 1 && enemyclose == 1) || (casterclose == 2 && enemyclose == 2)) && hpnownpc >= (2 * (hpmaxnpc / 3))))
            {
                npc.SetInt(obj_f.critter_strategy, 47); // Rage - power attack (47)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 647);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.POWER_ATTACK) && ((casterclose == 1 && enemyclose == 1) || (casterclose == 2 && enemyclose == 2)) && hpnownpc >= (2 * (hpmaxnpc / 3))))
            {
                npc.SetInt(obj_f.critter_strategy, 20); // Melee - power attack (20)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 620);
                }

                return;
            }

            if ((((casterclose == 1 && enemyclose == 1) || (casterclose == 2 && enemyclose == 2)) && hpnownpc >= (2 * (hpmaxnpc / 3)) && (friendclose >= 1 || friendmedium >= 2)))
            {
                npc.SetInt(obj_f.critter_strategy, 36); // Melee - ready vs. spell (36)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 636);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.POWER_ATTACK) && enemyclose == 0 && enemymedium == 0 && prone >= 1 && hpnownpc >= (2 * (hpmaxnpc / 3))))
            {
                npc.SetInt(obj_f.critter_strategy, 26); // Melee - attack prone w/ power attack (26)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 626);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.POWER_ATTACK) && enemyclose == 0 && enemymedium == 0 && helpless >= 1 && hpnownpc >= (2 * (hpmaxnpc / 3))))
            {
                npc.SetInt(obj_f.critter_strategy, 24); // Melee - attack helpless w/ power attack (24)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 624);
                }

                return;
            }

            if ((enemyclose == 0 && enemymedium == 0 && prone >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 23); // Melee - attack prone (23)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 623);
                }

                return;
            }

            if ((enemyclose == 0 && enemymedium == 0 && helpless >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 21); // Melee - attack helpless (21)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 621);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.BARBARIAN_RAGE) && npc.HasFeat(FeatId.IMPROVED_TRIP) && enemyclose == 1 && hpnownpc > (hpmaxnpc / 3)))
            {
                npc.SetInt(obj_f.critter_strategy, 44); // Rage - tripper close (44)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 644);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.IMPROVED_TRIP) && enemyclose == 1 && hpnownpc > (hpmaxnpc / 3)))
            {
                npc.SetInt(obj_f.critter_strategy, 27); // Tripper - close (27)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 627);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.BARBARIAN_RAGE) && npc.HasFeat(FeatId.IMPROVED_TRIP) && enemyclose >= 1 && hpnownpc > (hpmaxnpc / 2) && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 44); // Rage - tripper close (44)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 644);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.IMPROVED_TRIP) && enemyclose >= 1 && hpnownpc > (hpmaxnpc / 2) && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 27); // Tripper - close (27)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 627);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.IMPROVED_TRIP) && enemyclose == 0 && hpnownpc > (2 * (hpmaxnpc / 3)) && enemymedium >= 1 && (friendclose >= 1 || friendmedium >= 1)))
            {
                var num = RandomRange(1, 3);
                if (num == 1)
                {
                    npc.SetInt(obj_f.critter_strategy, 28); // Tripper - medium (28)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 628);
                    }

                    return;
                }
                else if (num == 2)
                {
                    npc.SetInt(obj_f.critter_strategy, 29); // Tripper - medium (29)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 629);
                    }

                    return;
                }
                else
                {
                    npc.SetInt(obj_f.critter_strategy, 30); // Tripper - medium (30)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 630);
                    }

                    return;
                }

            }

            if ((npc.HasFeat(FeatId.BARBARIAN_RAGE) && npc.HasFeat(FeatId.MOBILITY) && enemyclose == 1 && hpnownpc > (hpmaxnpc / 3) && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 42); // Rage - flanker - close w/move (42)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 642);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.MOBILITY) && enemyclose == 1 && hpnownpc > (hpmaxnpc / 3) && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 16); // Melee - Flanker - close w/move (16)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 616);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.BARBARIAN_RAGE) && npc.HasFeat(FeatId.MOBILITY) && enemyclose >= 1 && hpnownpc > (hpmaxnpc / 2) && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 41); // Rage - flanker - close (41)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 641);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.MOBILITY) && enemyclose >= 1 && hpnownpc > (hpmaxnpc / 2) && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 14); // Melee - Flanker - close (14)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 614);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.MOBILITY) && enemymedium >= 1 && enemyclose == 0 && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 15); // Melee - Flanker - medium (15)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 615);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.MOBILITY) && enemyfar >= 1 && (friendclose >= 1 || friendmedium >= 1)))
            {
                var num = RandomRange(1, 2);
                if (num == 1)
                {
                    npc.SetInt(obj_f.critter_strategy, 15); // Melee - Flanker - medium (15)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 615);
                    }

                    return;
                }
                else
                {
                    npc.SetInt(obj_f.critter_strategy, 22); // Melee - charge attack (22)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 622);
                    }

                    return;
                }

            }

            if ((npc.HasFeat(FeatId.MOBILITY) && npc.HasFeat(FeatId.BARBARIAN_RAGE) && enemyclose == 0 && enemymedium >= 1 && hpnownpc == hpmaxnpc && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 42); // Rage - flanker - close w/move (42)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 642);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.MOBILITY) && enemyclose == 0 && enemymedium >= 1 && hpnownpc == hpmaxnpc && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 16); // Melee - Flanker - close w/move (16)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 616);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.POWER_ATTACK) && enemyclose == 0 && enemymedium == 0 && dying >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 17); // Coup De Grace - w/ power attack (17)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 617);
                }

                return;
            }

            if ((enemyclose == 0 && enemymedium == 0 && dying >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 7); // Only Coup De Grace - (7)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 607);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.BARBARIAN_RAGE) && npc.HasFeat(FeatId.POWER_ATTACK) && enemyclose == 0 && hpnownpc == hpmaxnpc && enemymedium == 0 && enemyfar >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 45); // Rage - charge w/power attack (45)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 645);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.POWER_ATTACK) && enemyclose == 0 && hpnownpc == hpmaxnpc && enemymedium == 0 && enemyfar >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 25); // Melee - charge attack w/ power attack (25)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 625);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.BARBARIAN_RAGE) && enemyclose == 0 && enemymedium == 0 && enemyfar >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 46); // Rage - charge attack (46)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 646);
                }

                return;
            }

            if ((enemyclose == 0 && enemymedium == 0 && (enemyfar >= 1 || enemyveryfar >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 22); // Melee - charge attack (22)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 622);
                }

                return;
            }

            // can't find a good strategy??
            // randomly go to either original strategy or the default strategy
            var num = RandomRange(1, 2);
            if (num == 1)
            {
                npc.SetInt(obj_f.critter_strategy, 0); // Default
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 600);
                }

                return;
            }
            else
            {
                var xyx = npc.GetInt(obj_f.pad_i_0); // get original strategy
                npc.SetInt(obj_f.critter_strategy, xyx); // reset strategy to original
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 599); // Revert Strategy
                }

            }

            return;
        }
        // This will try to pick a strategy for a melee character with a reach weapon		##
        // ##
        // It takes into account Feats, Health, and other limiting factors			##
        // ##
        // Useful for any Humanoid melee NPC type that uses a reach weapon			##
        // NOTE:  All float messages need to be removed before final release!!!!

        public static void get_melee_reach_strategy(GameObjectBody npc)
        {
            return;
            if (Utilities.critter_is_unconscious(npc) == 1) // no reason to run script if unconcious
            {
                return;
            }

            var enemyadjacent = 0;
            var enemyclose = 0;
            var enemymedium = 0;
            var enemyfar = 0;
            var prone = 0;
            var dying = 0;
            var helpless = 0;
            var friendclose = 0;
            var friendmedium = 0;
            var casterclose = 0;
            var webbed = break_free(npc, 8); // runs break_free script with range set to 8
            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                // for obj in game.party[0].group_list():	## find number of enemy and distances
                // Note: 10' reach is equivalent to npc.distance_to(obj) == 8
                if ((obj.DistanceTo(npc) < 3 && Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 1 && (!obj.D20Query(D20DispatcherKey.QUE_Prone)) && (!obj.D20Query(D20DispatcherKey.QUE_CoupDeGrace))))
                {
                    enemyadjacent = enemyadjacent + 1;
                }

                if ((obj.DistanceTo(npc) < 8 && Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 1 && (!obj.D20Query(D20DispatcherKey.QUE_Prone)) && (!obj.D20Query(D20DispatcherKey.QUE_CoupDeGrace))))
                {
                    enemyclose = enemyclose + 1;
                }

                if ((obj.DistanceTo(npc) < 8 && Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 10 && (!obj.D20Query(D20DispatcherKey.QUE_Prone)) && (!obj.D20Query(D20DispatcherKey.QUE_CoupDeGrace)) && (obj.GetStat(Stat.level_sorcerer) >= 2 || obj.GetStat(Stat.level_wizard) >= 2)))
                {
                    casterclose = casterclose + 1;
                }

                if ((obj.DistanceTo(npc) < 13 && obj.DistanceTo(npc) >= 8 && Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 1 && (!obj.D20Query(D20DispatcherKey.QUE_Prone)) && (!obj.D20Query(D20DispatcherKey.QUE_CoupDeGrace))))
                {
                    enemymedium = enemymedium + 1;
                }

                if ((obj.DistanceTo(npc) <= 25 && obj.DistanceTo(npc) > 13 && Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 1 && (!obj.D20Query(D20DispatcherKey.QUE_Prone)) && (!obj.D20Query(D20DispatcherKey.QUE_CoupDeGrace))))
                {
                    enemyfar = enemyfar + 1;
                }

                if ((obj.DistanceTo(npc) <= 10 && obj.D20Query(D20DispatcherKey.QUE_Prone) && obj.GetStat(Stat.hp_current) >= 1 && (!obj.D20Query(D20DispatcherKey.QUE_Helpless))))
                {
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        obj.FloatMesFileLine("mes/skill_ui.mes", 106);
                    }

                    prone = prone + 1;
                }

                if ((obj.DistanceTo(npc) <= 5 && obj.D20Query(D20DispatcherKey.QUE_Helpless) && obj.GetStat(Stat.hp_current) >= 0))
                {
                    helpless = helpless + 1;
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        obj.FloatMesFileLine("mes/skill_ui.mes", 107);
                    }

                }

                if ((obj.DistanceTo(npc) <= 5 && obj.GetStat(Stat.hp_current) <= -1 && (!obj.D20Query(D20DispatcherKey.QUE_Dead))))
                {
                    dying = dying + 1;
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        obj.FloatMesFileLine("mes/skill_ui.mes", 108);
                    }

                }

            }

            foreach (var obj in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((obj.DistanceTo(npc) <= 8 && Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 1 && (!obj.D20Query(D20DispatcherKey.QUE_Prone)) && (!obj.D20Query(D20DispatcherKey.QUE_CoupDeGrace)) && obj.GetLeader() == null && obj != npc))
                {
                    friendclose = friendclose + 1;
                }

                if ((obj.DistanceTo(npc) > 8 && obj.DistanceTo(npc) <= 16 && Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 1 && (!obj.D20Query(D20DispatcherKey.QUE_Prone)) && (!obj.D20Query(D20DispatcherKey.QUE_CoupDeGrace)) && obj.GetLeader() == null && obj != npc))
                {
                    friendmedium = friendmedium + 1;
                }

            }

            var hpnownpc = npc.GetStat(Stat.hp_current);
            var hpmaxnpc = npc.GetStat(Stat.hp_max);
            var dumbass = npc.GetStat(Stat.intelligence) < 8;
            if ((hpnownpc < (hpmaxnpc / 2) && (npc.FindItemByName(8006) || npc.FindItemByName(8007) || npc.FindItemByName(8014) || npc.FindItemByName(8037))))
            {
                npc.SetInt(obj_f.critter_strategy, 2); // Only Use Potion w/ 5 Foot Step (2)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 602);
                }

                return;
            }

            if ((webbed && enemyclose == 0))
            {
                npc.SetInt(obj_f.critter_strategy, 0); // Default (allows for break free)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 600);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.COMBAT_EXPERTISE) && hpnownpc <= (hpmaxnpc / 2) && casterclose == 0 && enemyclose >= 1 && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 38); // Combat Expertise 2 (38)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 638);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.COMBAT_EXPERTISE) && enemyclose >= 2))
            {
                npc.SetInt(obj_f.critter_strategy, 38); // Combat Expertise 2 (38)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 638);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.BARBARIAN_RAGE) && npc.HasFeat(FeatId.POWER_ATTACK) && ((casterclose == 1 && enemyclose == 1) || (casterclose == 2 && enemyclose == 2)) && hpnownpc >= (2 * (hpmaxnpc / 3))))
            {
                npc.SetInt(obj_f.critter_strategy, 47); // Rage - power attack (47)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 647);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.POWER_ATTACK) && ((casterclose == 1 && enemyclose == 1) || (casterclose == 2 && enemyclose == 2)) && hpnownpc >= (2 * (hpmaxnpc / 3))))
            {
                npc.SetInt(obj_f.critter_strategy, 20); // Melee - power attack (20)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 620);
                }

                return;
            }

            if ((!dumbass && ((casterclose == 1 && enemyclose == 1) || (casterclose == 2 && enemyclose == 2)) && hpnownpc >= (2 * (hpmaxnpc / 3)) && (friendclose >= 1 || friendmedium >= 2)))
            {
                npc.SetInt(obj_f.critter_strategy, 36); // Melee - ready vs. spell (36)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 636);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.POWER_ATTACK) && enemyclose == 0 && enemymedium == 0 && prone >= 1 && hpnownpc >= (2 * (hpmaxnpc / 3))))
            {
                npc.SetInt(obj_f.critter_strategy, 26); // Melee - attack prone w/ power attack (26)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 626);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.POWER_ATTACK) && enemyclose == 0 && enemymedium == 0 && helpless >= 1 && hpnownpc >= (2 * (hpmaxnpc / 3))))
            {
                npc.SetInt(obj_f.critter_strategy, 24); // Melee - attack helpless w/ power attack (24)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 624);
                }

                return;
            }

            if ((enemyclose == 0 && enemymedium == 0 && prone >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 23); // Melee - attack prone (23)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 623);
                }

                return;
            }

            if ((enemyclose == 0 && enemymedium == 0 && helpless >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 21); // Melee - attack helpless (21)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 621);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.BARBARIAN_RAGE) && npc.HasFeat(FeatId.IMPROVED_TRIP) && enemyclose == 1 && hpnownpc > (hpmaxnpc / 3)))
            {
                npc.SetInt(obj_f.critter_strategy, 44); // Rage - tripper close (44)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 644);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.IMPROVED_TRIP) && enemyclose == 1 && hpnownpc > (hpmaxnpc / 3)))
            {
                npc.SetInt(obj_f.critter_strategy, 27); // Tripper - close (27)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 627);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.BARBARIAN_RAGE) && npc.HasFeat(FeatId.IMPROVED_TRIP) && enemyclose >= 1 && hpnownpc > (hpmaxnpc / 2) && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 44); // Rage - tripper close (44)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 644);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.IMPROVED_TRIP) && enemyclose >= 1 && hpnownpc > (hpmaxnpc / 2) && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 27); // Tripper - close (27)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 627);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.IMPROVED_TRIP) && enemyclose == 0 && hpnownpc > (2 * (hpmaxnpc / 3)) && enemymedium >= 1 && (friendclose >= 1 || friendmedium >= 1)))
            {
                var num = RandomRange(1, 3);
                if (num == 1)
                {
                    npc.SetInt(obj_f.critter_strategy, 28); // Tripper - medium (28)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 628);
                    }

                    return;
                }
                else if (num == 2)
                {
                    npc.SetInt(obj_f.critter_strategy, 29); // Tripper - medium (29)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 629);
                    }

                    return;
                }
                else
                {
                    npc.SetInt(obj_f.critter_strategy, 30); // Tripper - medium (30)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 630);
                    }

                    return;
                }

            }

            if ((npc.HasFeat(FeatId.BARBARIAN_RAGE) && npc.HasFeat(FeatId.MOBILITY) && enemyclose == 1 && hpnownpc > (hpmaxnpc / 3) && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 42); // Rage - flanker - close w/move (42)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 642);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.MOBILITY) && enemyclose == 1 && hpnownpc > (hpmaxnpc / 3) && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 16); // Melee - Flanker - close w/move (16)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 616);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.BARBARIAN_RAGE) && npc.HasFeat(FeatId.MOBILITY) && enemyclose >= 1 && hpnownpc > (hpmaxnpc / 2) && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 41); // Rage - flanker - close (41)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 641);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.MOBILITY) && enemyclose >= 1 && hpnownpc > (hpmaxnpc / 2) && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 14); // Melee - Flanker - close (14)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 614);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.MOBILITY) && enemymedium >= 1 && enemyclose == 0 && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 15); // Melee - Flanker - medium (15)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 615);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.MOBILITY) && enemyfar >= 1 && (friendclose >= 1 || friendmedium >= 1)))
            {
                var num = RandomRange(1, 2);
                if (num == 1)
                {
                    npc.SetInt(obj_f.critter_strategy, 15); // Melee - Flanker - medium (15)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 615);
                    }

                    return;
                }
                else
                {
                    npc.SetInt(obj_f.critter_strategy, 22); // Melee - charge attack (22)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 622);
                    }

                    return;
                }

            }

            if ((npc.HasFeat(FeatId.MOBILITY) && npc.HasFeat(FeatId.BARBARIAN_RAGE) && enemyclose == 0 && enemymedium >= 1 && hpnownpc == hpmaxnpc && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 42); // Rage - flanker - close w/move (42)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 642);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.MOBILITY) && enemyclose == 0 && enemymedium >= 1 && hpnownpc == hpmaxnpc && (friendclose >= 1 || friendmedium >= 1)))
            {
                npc.SetInt(obj_f.critter_strategy, 16); // Melee - Flanker - close w/move (16)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 616);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.POWER_ATTACK) && enemyclose == 0 && enemymedium == 0 && dying >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 17); // Coup De Grace - w/ power attack (17)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 617);
                }

                return;
            }

            if ((enemyclose == 0 && enemymedium == 0 && dying >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 7); // Only Coup De Grace - (7)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 607);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.BARBARIAN_RAGE) && npc.HasFeat(FeatId.POWER_ATTACK) && enemyclose == 0 && hpnownpc == hpmaxnpc && enemymedium == 0 && enemyfar >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 45); // Rage - charge w/power attack (45)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 645);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.POWER_ATTACK) && enemyclose == 0 && hpnownpc == hpmaxnpc && enemymedium == 0 && enemyfar >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 25); // Melee - charge attack w/ power attack (25)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 625);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.BARBARIAN_RAGE) && enemyclose == 0 && enemymedium == 0 && enemyfar >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 46); // Rage - charge attack (46)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 646);
                }

                return;
            }

            if ((enemyclose == 0 && enemymedium == 0 && enemyfar >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 22); // Melee - charge attack (22)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 622);
                }

                return;
            }

            if ((npc.HasFeat(FeatId.BARBARIAN_RAGE) && enemyclose >= 1))
            {
                npc.SetInt(obj_f.critter_strategy, 43); // Rage - reach weapon close (43)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 643);
                }

                return;
            }

            if ((enemyadjacent >= 1)) // attack victim and back off with a 5' step at the end
            {
                npc.SetInt(obj_f.critter_strategy, 19); // Melee - Reach Weapon Close (19)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 619);
                }

                return;
            }

            if ((enemyadjacent == 0 && enemyclose >= 1)) // stand in place and whack away
            {
                npc.SetInt(obj_f.critter_strategy, 7); // Only Coup De Grace - (7) (even though no coup de grace is there??? -SA)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 607);
                }

                return;
            }

            // can't find a good strategy??
            // randomly go to either original strategy or the default strategy
            var num = RandomRange(1, 2);
            if (num == 1)
            {
                npc.SetInt(obj_f.critter_strategy, 18); // Melee - Reach Weapon Default (18)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 618);
                }

                return;
            }
            else
            {
                var xyx = npc.GetInt(obj_f.pad_i_0); // get original strategy
                npc.SetInt(obj_f.critter_strategy, xyx); // reset strategy to original
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 599); // Revert Strategy
                }

            }

            return;
        }
        // This will try to pick a strategy for a ranged NPC					##
        // NOTE:  All float messages need to be removed before final release!!!!

        public static void get_ranged_strategy(GameObjectBody npc)
        {
            if (Utilities.critter_is_unconscious(npc) == 1) // no reason to run script if unconcious
            {
                return;
            }

            var caster = 0;
            var frenz = 0;
            foreach (var co_com in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                // for co_com in game.obj_list_vicinity(npc.location,OLC_NPC):	## find friendlies
                if ((co_com.IsFriendly(npc) && Utilities.critter_is_unconscious(co_com) == 0))
                {
                    frenz = frenz + 1;
                }

            }

            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                // for obj in game.party[0].group_list():	## find casters
                if ((obj.GetStat(Stat.hp_current) >= 1 && (!obj.D20Query(D20DispatcherKey.QUE_Helpless)) && (!obj.D20Query(D20DispatcherKey.QUE_CoupDeGrace)) && (obj.GetStat(Stat.level_sorcerer) >= 1 || obj.GetStat(Stat.level_wizard) >= 1 || obj.GetStat(Stat.level_bard) >= 2 || obj.GetStat(Stat.level_cleric) >= 2 || obj.GetStat(Stat.level_druid) >= 1)))
                {
                    caster = caster + 1;
                }

            }

            if ((caster >= 3 && frenz > 4))
            {
                var num = RandomRange(1, 14);
                if (num == 1)
                {
                    npc.SetInt(obj_f.critter_strategy, 10); // Ranged - Sniper 1 (10)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 610);
                    }

                    return;
                }
                else if (num == 2)
                {
                    npc.SetInt(obj_f.critter_strategy, 11); // Ranged - Sniper 2 (11)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 611);
                    }

                    return;
                }
                else if (num == 3)
                {
                    npc.SetInt(obj_f.critter_strategy, 12); // Ranged - Sniper 3 (12)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 612);
                    }

                    return;
                }
                else if (num == 4)
                {
                    npc.SetInt(obj_f.critter_strategy, 8); // Ranged - Sniper (8)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 608);
                    }

                    return;
                }
                else
                {
                    npc.SetInt(obj_f.critter_strategy, 9); // Ranged - Ready vs. Spell (9)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 609);
                    }

                    return;
                }

            }

            if (((caster >= 2 && frenz > 1) || (frenz > 5 && caster > 0)))
            {
                var num = RandomRange(1, 5);
                if (num == 1)
                {
                    npc.SetInt(obj_f.critter_strategy, 10); // Ranged - Sniper 1 (10)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 610);
                    }

                    return;
                }
                else if (num == 2)
                {
                    npc.SetInt(obj_f.critter_strategy, 11); // Ranged - Sniper 2 (11)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 611);
                    }

                    return;
                }
                else if (num == 3)
                {
                    npc.SetInt(obj_f.critter_strategy, 12); // Ranged - Sniper 3 (12)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 612);
                    }

                    return;
                }
                else if (num == 4)
                {
                    npc.SetInt(obj_f.critter_strategy, 8); // Ranged - Sniper (8)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 608);
                    }

                    return;
                }
                else
                {
                    npc.SetInt(obj_f.critter_strategy, 9); // Ranged - Ready vs. Spell (9)
                    if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                    {
                        npc.FloatMesFileLine("mes/skill_ui.mes", 609);
                    }

                    return;
                }

            }

            var num = RandomRange(1, 5);
            if (num == 1)
            {
                npc.SetInt(obj_f.critter_strategy, 10); // Ranged - Sniper 1 (10)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 610);
                }

                return;
            }
            else if (num == 2)
            {
                npc.SetInt(obj_f.critter_strategy, 11); // Ranged - Sniper 2 (11)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 611);
                }

                return;
            }
            else if (num == 3)
            {
                npc.SetInt(obj_f.critter_strategy, 12); // Ranged - Sniper 3 (12)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 612);
                }

                return;
            }
            else if (num == 4)
            {
                npc.SetInt(obj_f.critter_strategy, 8); // Ranged - Sniper (8)
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 608);
                }

                return;
            }
            else
            {
                var xyx = npc.GetInt(obj_f.pad_i_0); // get original strategy
                npc.SetInt(obj_f.critter_strategy, xyx); // reset strategy to original
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 599); // Revert Strategy
                }

            }

            return;
        }
        // This records the original strategy from the protos.tab				##

        public static void tag_strategy(GameObjectBody npc)
        {
            var xyx = npc.GetInt(obj_f.critter_strategy); // get original strategy
            npc.SetInt(obj_f.pad_i_0, xyx); // record the original strategy
            return;
        }
        // This picks a strategy at random from 5 options					##
        // 5 possible scripts

        public static void change_strategy_random(GameObjectBody npc, FIXME x1, FIXME x2, FIXME x3, FIXME x4, FIXME x5)
        {
            // def change_strategy_random(npc, x1, x2, x3, x4, x5):		## 5 possible scripts
            var num = RandomRange(1, 5); // random number picked
            if (num == 1)
            {
                npc.SetInt(obj_f.critter_strategy, x1); // set new strategy
            }

            if (num == 2)
            {
                npc.SetInt(obj_f.critter_strategy, x2); // set new strategy
            }

            if (num == 3)
            {
                npc.SetInt(obj_f.critter_strategy, x3); // set new strategy
            }

            if (num == 4)
            {
                npc.SetInt(obj_f.critter_strategy, x4); // set new strategy
            }
            else
            {
                npc.SetInt(obj_f.critter_strategy, x5); // set new strategy
            }

            return;
        }
        // This changes to a specific script							##
        // x1 is new script number

        public static void change_strategy(GameObjectBody npc, FIXME x1)
        {
            // def change_strategy(npc, x1):					## x1 is new script number
            npc.SetInt(obj_f.critter_strategy, x1); // set new strategy
            return;
        }
        // This reverts to the original AI NPC script						##

        public static void revert_strategy(GameObjectBody npc)
        {
            var xyx = npc.GetInt(obj_f.pad_i_0); // get original strategy
            npc.SetInt(obj_f.critter_strategy, xyx); // reset strategy to original
            return;
        }
        // Checks current strategy against original strategy					##

        public static int same_strategy(GameObjectBody npc)
        {
            var ors = npc.GetInt(obj_f.critter_strategy); // get current strategy
            var nes = npc.GetInt(obj_f.pad_i_0); // get original strategy
            if (ors == nes) // are they the same?
            {
                return 1; // return 1 if same
            }

            return 0; // return 0 if different
        }
        // Detects if a talker is present							##

        public static int detect_talker(GameObjectBody talker, GameObjectBody seeker)
        {
            if ((Utilities.find_npc_near(talker, seeker) != null))
            {
                return 1;
            }

            return 0;
        }
        // Changes to new talker								##

        public static bool change_talker(GameObjectBody talker, GameObjectBody seeker, GameObjectBody pc, int line)
        {
            var target = Utilities.find_npc_near(talker, seeker);
            pc.BeginDialog(target, line);
            target.TurnTowards(pc);
            pc.TurnTowards(target);
            return SkipDefault;
        }
        // This calls for the Break Free from Web routine					##

        public static bool break_free(GameObjectBody npc, int range = 5ft)
        {
            if (range == "10ft")
            {
                range = 8;
            }
            else if (range == "5ft")
            {
                range = 3;
            }

            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if ((obj.DistanceTo(npc) <= range && obj.GetStat(Stat.hp_current) >= -9))
                {
                    return RunDefault;
                }

            }

            while ((npc.FindItemByName(8903) != null))
            {
                npc.FindItemByName(8903).Destroy();
            }

            if ((npc.D20Query(D20DispatcherKey.QUE_Is_BreakFree_Possible)))
            {
                Utilities.create_item_in_inventory(8903, npc);
                return true;
            }

            return false;
        }
        // This tags and destroys the NPC's thrown weapon, melee weapon (no shield please)	##
        // As a fail safe, if you run this script with an NPC that has a shield			##
        // The shield will vanish								##
        // obj_f_pad_i_7 records the type of thrown weapon					##
        // obj_f_pad_i_9 records the type of melee weapon					##
        // obj_f_pad_i_8 records quantity of thrown weapon					##

        public static void tag_weapons_thrown(GameObjectBody npc)
        {
            npc.WieldBestInAllSlots(); // equip something
            var shieldx = npc.ItemWornAt(EquipSlot.Shield); // find shield being used
            if (shieldx != null) // was a shield found?
            {
                shieldx.Destroy(); // destroy shield
            }

            npc.WieldBestInAllSlots(); // equip something
            var weaponx = npc.ItemWornAt(EquipSlot.WeaponPrimary); // find weapon being used
            var rnum = 0; // reset counter
            while ((weaponx != null))
            {
                // while(weaponx != OBJ_HANDLE_NULL):			## start loop
                npc.WieldBestInAllSlots(); // equip something
                shieldx = npc.ItemWornAt(EquipSlot.Shield); // find shield being used
                if (shieldx != null) // was a shield found?
                {
                    shieldx.Destroy(); // destroy shield
                    npc.WieldBestInAllSlots(); // equip something
                }

                var x1 = weaponx.GetInt(obj_f.weapon_flags); // check weapon flag type
                if ((x1 == 1024)) // if weapon is ranged
                {
                    var weapon = weaponx.GetNameId(); // get weapon's protos name
                    npc.SetInt(obj_f.pad_i_7, weapon); // record thrown weapon's name
                    weaponx.Destroy(); // destroy thrown weapon
                    rnum = rnum + 1; // add to count
                }
                else
                {
                    // else:						## if weapon is not ranged
                    var weapon = weaponx.GetNameId(); // get weapon's protos name
                    npc.SetInt(obj_f.pad_i_9, weapon); // record melee weapon's name
                    weaponx.Destroy(); // destoy melee weapon
                }

                npc.WieldBestInAllSlots(); // equip something
                weaponx = npc.ItemWornAt(EquipSlot.WeaponPrimary); // find weapon being used
            }

            npc.SetInt(obj_f.pad_i_8, rnum); // record quantity of thrown weapon
            npc.WieldBestInAllSlots(); // equip something
            return;
        }
        // This recalls all weapons, and resets variables to 0					##

        public static void get_everything_thrown(GameObjectBody npc)
        {
            var weapon1 = npc.GetInt(obj_f.pad_i_9); // recall melee weapon
            var weapon2 = npc.GetInt(obj_f.pad_i_7); // recall thrown weapon
            var quantity = npc.GetInt(obj_f.pad_i_8); // recall quantity of thrown weapons
            if (weapon1 == 0 && weapon2 == 0 && quantity == 0) // nothing set?
            {
                return; // end routine
            }

            var weaponx1 = npc.FindItemByProto(weapon1); // does melee weapon already exist?
            if (weapon1 != 0 && weaponx1 == null) // if melee weapons doesn't exist then...
            {
                Utilities.create_item_in_inventory(weapon1, npc); // create melee weapon
            }

            while ((quantity >= 1))
            {
                // while(quantity >= 1):					## start loop
                Utilities.create_item_in_inventory(weapon2, npc); // create thrown weapon
                quantity = quantity - 1; // reduce counter
            }

            npc.SetInt(obj_f.pad_i_9, 0); // reset to 0
            npc.SetInt(obj_f.pad_i_7, 0); // reset to 0
            npc.SetInt(obj_f.pad_i_8, 0); // reset to 0
            return;
        }
        // This holds melee weapon only (should only follow a tag call)				##

        public static void hold_melee_weapon_thrown(GameObjectBody npc)
        {
            var weapon1 = npc.GetInt(obj_f.pad_i_9); // recall melee weapon
            if (weapon1 == 0) // is this an actual number?
            {
                return; // if no, then end routine
            }

            var weaponx1 = npc.FindItemByProto(weapon1); // does melee weapon already exist?
            if (weapon1 != 0 && weaponx1 == null) // if it doesn't exist then...
            {
                Utilities.create_item_in_inventory(weapon1, npc); // create melee weapon
            }

            npc.WieldBestInAllSlots(); // equip everything
            return;
        }
        // This holds ranged weapon only (should only follow a tag call)			##

        public static void hold_ranged_weapon_thrown(GameObjectBody npc)
        {
            var weapon2 = npc.GetInt(obj_f.pad_i_7); // recall ranged weapon
            if (weapon2 == 0) // is this an actual number?
            {
                return; // if no, then end routine
            }

            var quantity = npc.GetInt(obj_f.pad_i_8); // recall quantity
            while ((quantity >= 1))
            {
                // while(quantity >= 1):					## start loop
                Utilities.create_item_in_inventory(weapon2, npc); // create thrown weapon
                quantity = quantity - 1; // reduce counter
            }

            npc.SetInt(obj_f.pad_i_8, 0); // reset to 0
            npc.WieldBestInAllSlots(); // equip everything
            return;
        }
        // This switches to melee weapon and destroys thrown weapons... keeps a count		##

        public static void get_melee_weapon_thrown(GameObjectBody npc)
        {
            var weapon1 = npc.GetInt(obj_f.pad_i_9); // recall melee weapon
            if (weapon1 == 0) // is this an actual number?
            {
                return; // if no, then end routine
            }

            npc.WieldBestInAllSlots(); // equip something
            var weaponx = npc.ItemWornAt(EquipSlot.WeaponPrimary); // find weapon being used
            var rnum = 0; // reset counter
            while ((weaponx != null))
            {
                // while(weaponx != OBJ_HANDLE_NULL):			## start loop
                npc.WieldBestInAllSlots(); // equip something
                var shieldx = npc.ItemWornAt(EquipSlot.Shield); // find shield being used
                if (shieldx != null) // was a shield found?
                {
                    shieldx.Destroy(); // destroy shield
                    npc.WieldBestInAllSlots(); // equip something
                }

                var x1 = weaponx.GetInt(obj_f.weapon_flags); // check weapon flag type
                if ((x1 == 1024)) // if weapon is ranged
                {
                    weaponx.Destroy(); // destroy thrown weapon
                    rnum = rnum + 1; // add to count
                }
                else
                {
                    // else:						## if weapon is not ranged
                    weaponx.Destroy(); // destoy melee weapon
                }

                npc.WieldBestInAllSlots(); // equip something
                weaponx = npc.ItemWornAt(EquipSlot.WeaponPrimary); // find weapon being used
            }

            npc.SetInt(obj_f.pad_i_8, rnum); // record quantity of thrown weapon
            var weaponx1 = npc.FindItemByProto(weapon1); // does melee weapon already exist?
            if (weaponx1 == null) // if it doesn't exist then...
            {
                Utilities.create_item_in_inventory(weapon1, npc); // create melee weapon
            }

            npc.WieldBestInAllSlots(); // equip everything
            return;
        }
        // This switches to ranged weapon only							##

        public static void get_ranged_weapon_thrown(GameObjectBody npc)
        {
            var weapon2 = npc.GetInt(obj_f.pad_i_7); // recall ranged weapon
            if (weapon2 == 0) // is this an actual number?
            {
                return; // if no, then end routine
            }

            var item = npc.ItemWornAt(EquipSlot.WeaponPrimary); // find melee weapon being used
            if (item != null) // was melee weapon found?
            {
                item.Destroy(); // if yes, then destroy melee weapon
            }

            var item2 = npc.ItemWornAt(EquipSlot.Shield); // find shield being used
            if (item2 != null) // was shield found?
            {
                item2.Destroy(); // if yes, then destroy shield
            }

            // NOTE: it is important to destroy shield.
            // A one-handed ranged weapon will not equip if a shield is present.
            // Weird, but true, at least in all of my tests.
            var quantity = npc.GetInt(obj_f.pad_i_8); // recall quantity
            while ((quantity >= 1))
            {
                // while(quantity >= 1):					## start loop
                Utilities.create_item_in_inventory(weapon2, npc); // create thrown weapon
                quantity = quantity - 1; // reduce counter
            }

            npc.SetInt(obj_f.pad_i_8, 0); // reset to 0
            npc.WieldBestInAllSlots(); // equip everything
            return;
        }
        // This checks for thrown weapons							##

        public static int detect_thrown_ammo(GameObjectBody npc, FIXME type)
        {
            if (type == 1)
            {
                var quantity = npc.GetInt(obj_f.pad_i_8); // recall quantity
                if (quantity >= 1) // all gone...
                {
                    return 1; // return 1 if ammo exists
                }

                return 0; // return 0 if no ammo
            }
            else
            {
                npc.WieldBestInAllSlots(); // equip something
                var weaponx = npc.ItemWornAt(EquipSlot.WeaponPrimary); // find a weapon
                if ((weaponx == null)) // no weapon found
                {
                    npc.SetInt(obj_f.pad_i_8, 0); // set quantity to 0
                    return 0; // return 0 if no ammo
                }

                return 1; // return 1 if ammo exists
            }

        }
        public static int target_distance(GameObjectBody npc, FIXME distx)
        {
            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if ((obj.DistanceTo(npc) <= distx && Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 1))
                {
                    return 1;
                }

            }

            return 0;
        }
        // This attempts to find a suitable candidate for a area effect spell			##
        // distx = the size of area effect							##
        // fedead = maximum number of friends you are willing to hit				##
        // edead = minimum number of enemy units you want to hit				##
        // ##
        // If a target is found it will record the target as obj_f_npc_pad_i_5			##
        // This allows area effect spells to define this target as their target			##
        // ##
        // The script returns 0 if no target is found and it returns 1 if target found		##
        // NOTE: I don't know how to deal with projectiles, so for Fireball it is pointless to
        // to set "fedead" at anything less than 0 as I rely on the "cast fireball" call from
        // the strategy.tab to find the best target.  I hope to improve this at a later date.
        // Area effect spells that do work with "fedead" are:
        // Silence		(Needs to be called from strategy.tab by "cast single"
        // Web		(Needs to be called from strategy.tab by "cast single"
        // Note Silence variation allowed by adding an additional 1,000 to the obj_f_npc_pad_i_5

        public static int target_area_spell(GameObjectBody npc, FIXME distx, FIXME fdead, FIXME edead)
        {
            var caster = 0;
            var target = 666;
            var targetF = 0;
            var targetE = 0;
            var count = 0;
            var xyx = npc.GetInt(obj_f.npc_pad_i_5); // clears past targetting
            if (xyx >= 1000) // clears past targetting
            {
                npc.SetInt(obj_f.npc_pad_i_5, 0); // clears past targetting
            }

            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                // for obj in game.party[0].group_list():	## check each party member
                var friendlies = 0;
                var enemies = 0;
                foreach (var obj2 in ObjList.ListVicinity(obj.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    // for obj2 in game.obj_list_vicinity(obj.location,OLC_NPC):	## searches for friendlies
                    if ((obj2.DistanceTo(obj) <= (distx / 2) && obj2.IsFriendly(npc)) && obj2.GetStat(Stat.hp_current) >= 0)
                    {
                        friendlies = friendlies + 1;
                    }

                    if ((obj2.DistanceTo(obj) <= (distx / 2) && obj2.GetStat(Stat.hp_current) >= 0 && (!obj2.IsFriendly(npc))))
                    {
                        enemies = enemies + 1;
                    }

                }

                foreach (var obj2 in ObjList.ListVicinity(obj.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    // for obj2 in game.obj_list_vicinity(obj.location,OLC_PC):	## searches for friendlies
                    if ((obj2.DistanceTo(obj) <= (distx / 2) && obj2.IsFriendly(npc) && obj2.GetStat(Stat.hp_current) >= 0))
                    {
                        friendlies = friendlies + 1;
                    }

                    if ((obj2.DistanceTo(obj) <= (distx / 2) && obj2.GetStat(Stat.hp_current) >= 0 && (!obj2.IsFriendly(npc))))
                    {
                        enemies = enemies + 1;
                    }

                }

                if ((friendlies <= fdead && enemies >= targetE && enemies >= edead))
                {
                    target = count;
                    targetF = friendlies;
                    targetE = enemies;
                }

                count = count + 1;
            }

            if (target == 666)
            {
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 501);
                }

                return 0;
            }
            else
            {
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 580 + target);
                }

                npc.SetInt(obj_f.npc_pad_i_5, 1000 + target);
                npc.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(target));
            }

            return 1;
        }
        // Find candidate for Silence								##
        // type = kind of spell caster								##
        // ##
        // 0 = highest any kind									##
        // 1 = Sorcerer										##
        // 2 = Wizard										##
        // 3 = Druid										##
        // 4 = Cleric										##
        // 5 = Bard										##
        // ##
        // passes target to obj_f_npc_pad_i_5							##
        // if Silence is cast then Silence will re-target based on obj_f_npc_pad_i_5		##

        public static int target_for_silence(GameObjectBody npc, FIXME type)
        {
            var target = 666;
            var Hlevel = 0;
            var count = 0;
            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                var levelS = 0;
                var levelW = 0;
                var levelC = 0;
                var levelB = 0;
                var levelD = 0;
                var NewHlevel = 0;
                if ((Utilities.critter_is_unconscious(obj) == 0 && obj.GetStat(Stat.hp_current) >= 0 && (obj.GetStat(Stat.level_sorcerer) >= 1 || obj.GetStat(Stat.level_wizard) >= 1 || obj.GetStat(Stat.level_druid) >= 1 || obj.GetStat(Stat.level_cleric) >= 1 || obj.GetStat(Stat.level_bard) >= 1)))
                {
                    levelS = obj.GetStat(Stat.level_sorcerer);
                    levelW = obj.GetStat(Stat.level_wizard);
                    levelC = obj.GetStat(Stat.level_cleric);
                    levelB = obj.GetStat(Stat.level_bard);
                    levelD = obj.GetStat(Stat.level_druid);
                    if ((levelB >= levelS && levelB >= levelC && levelB >= levelW && levelB >= levelD && (type == 5 || type == 0)))
                    {
                        NewHlevel = levelB;
                    }

                    if ((levelC >= levelS && levelC >= levelB && levelC >= levelW && levelC >= levelD && (type == 4 || type == 0)))
                    {
                        NewHlevel = levelC;
                    }

                    if ((levelD >= levelS && levelD >= levelB && levelD >= levelW && levelD >= levelD && (type == 3 || type == 0)))
                    {
                        NewHlevel = levelD;
                    }

                    if ((levelW >= levelS && levelW >= levelC && levelW >= levelB && levelW >= levelD && (type == 2 || type == 0)))
                    {
                        NewHlevel = levelW;
                    }

                    if ((levelS >= levelW && levelS >= levelC && levelS >= levelB && levelS >= levelD && (type == 1 || type == 0)))
                    {
                        NewHlevel = levelS;
                    }

                    if ((NewHlevel >= Hlevel && NewHlevel != 0))
                    {
                        Hlevel = NewHlevel;
                        target = count;
                    }

                }

                count = count + 1;
            }

            if (target == 666)
            {
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 501);
                }

                return 0;
            }
            else
            {
                if (GetGlobalFlag(403) && GetGlobalFlag(405) && GetGlobalFlag(405))
                {
                    npc.FloatMesFileLine("mes/skill_ui.mes", 580 + target);
                }

                npc.SetInt(obj_f.npc_pad_i_5, 2000 + target);
                npc.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(target));
            }

            return 1;
        }
        // This detects area effect spells							##
        // ##
        // Area effect spells records their spell number as obj_f_pad_i_9			##
        // ##
        // The script returns 0 if none present returns ## if one is present			##
        // ##
        // when called second entry (type) should be set to:					##
        // 0 for any									##
        // spell number for specific spell							##
        // This works with the following area effect spells
        // Number is what is used/returned for each area effect
        // Silence	-- 434
        // Web -- 531
        // Fog Cloud -- 183
        // Stinking Cloud -- 460
        // Grease -- 200
        // Cloudkill -- 65
        // Consecrate -- 74
        // Desecrate -- 107
        // Entangle -- 153

        public static int find_area_effect_spell(GameObjectBody npc, FIXME type)
        {
            var distance = 0;
            var found = 0;
            var spell = 0;
            var count = 0;
            foreach (var obj in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_GENERIC))
            {
                var NEWdistance = 666;
                var NEWfound = 0;
                if ((obj.GetNameId() == 12003 && type == 0))
                {
                    NEWdistance = npc.DistanceTo(obj);
                    NEWfound = obj;
                }

                if ((obj.GetNameId() == 12003 && type != 0))
                {
                    spell = obj.GetInt(obj_f.pad_i_9);
                    if (spell == type)
                    {
                        NEWdistance = npc.DistanceTo(obj);
                        NEWfound = obj;
                    }

                }

                if (found != 0 && NEWdistance <= distance)
                {
                    found = NEWfound;
                    distance = NEWdistance;
                }

                if (found == 0 && NEWfound != 0)
                {
                    found = NEWfound;
                    distance = NEWdistance;
                }

            }

            if (found == 0)
            {
                return 0;
            }
            else
            {
                spell = found.GetInt(obj_f.pad_i_9);
            }

            return spell;
        }
        // Determines if target is in spell radius						##
        // type is spell number, and radius is the spell effect radius				##

        public static int am_I_in_the_effect(GameObjectBody npc, FIXME type, int radius)
        {
            foreach (var obj in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_GENERIC))
            {
                if ((obj.GetNameId() == 12003 && npc.DistanceTo(obj) <= radius / 2))
                {
                    var spell = obj.GetInt(obj_f.pad_i_9);
                    if (spell == type)
                    {
                        return 1; // yes, I am in the area effect
                    }

                }

            }

            return 0; // no, I am not in the area effect
        }
        // Destroy's area effect spell counters							##

        public static void kill_area_effect_spell(GameObjectBody npc)
        {
            foreach (var obj in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_GENERIC))
            {
                if ((obj.GetNameId() == 12003))
                {
                    obj.Destroy();
                }

            }

            return;
        }

    }
}
