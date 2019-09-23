
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
    [ObjectScript(336)]
    public class FamiliarDeath : BaseObjectScript
    {
        private static readonly Dictionary<int, int> familiar_table = new Dictionary<int, int> {
{12045,14900},
{12046,14901},
{12047,14902},
{12048,14903},
{12049,14904},
{12050,14905},
{12051,14906},
{12052,14907},
{12053,14908},
{12054,14909},
}
;
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            StartTimer(86400000, () => RemoveDead(PartyLeader, attachee)); // remove the familiar from the party in 24 hours
                                                                           // identify familiar dying and match to familiar inventory icon
            var familiar_proto = FindFamiliarInvType(attachee);
            if ((familiar_proto == null))
            {
                return SkipDefault; // not a valid familiar type
            }

            // search for familiar inventory icon in all PCs' inventory
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
            {
                var inv_familiar = obj.FindItemByProto(familiar_proto);
                if ((inv_familiar != null))
                {
                    // check for correct ID number
                    if ((get_ID(inv_familiar) == get_ID(attachee)))
                    {
                        // destroys familiar in owner's inventory and removes experience from owner
                        inv_familiar.Destroy();
                        var curxp = obj.GetStat(Stat.experience);
                        var ownerlevel = GetLevel(obj);
                        if ((!obj.SavingThrow(15, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, attachee)))
                        {
                            var xploss = ownerlevel * 200;
                        }
                        else
                        {
                            var xploss = ownerlevel * 100;
                        }

                        if ((curxp >= xploss))
                        {
                            var newxp = curxp - xploss;
                        }
                        else
                        {
                            var newxp = 0;
                        }

                        obj.SetBaseStat(Stat.experience, newxp);
                        return SkipDefault;
                    }

                }

            }

            return RunDefault;
        }
        public static GameObjectBody FindFamiliarInvType(GameObjectBody attachee)
        {
            foreach (var (f, p) in familiar_table)
            {
                if ((attachee.GetNameId() == p))
                {
                    return f;
                }

            }

            return null;
        }
        public static int get_ID(GameObjectBody obj)
        {
            return obj.GetInt(obj_f.secretdoor_dc);
        }
        public static void clear_ID(GameObjectBody obj)
        {
            // Clears embedded ID number from mobile object
            obj.SetInt(obj_f.secretdoor_dc, 0);
        }
        public static int GetLevel(GameObjectBody npc)
        {
            var level = npc.GetStat(Stat.level_sorcerer) + npc.GetStat(Stat.level_wizard);
            return level;
        }
        public static bool StowFamiliar(GameObjectBody attachee, GameObjectBody pc)
        {
            // identify familiar  and match to familiar inventory icon
            var familiar_proto = FindFamiliarInvType(attachee);
            if ((familiar_proto == null))
            {
                return SkipDefault; // not a valid familiar type
            }

            // search for familiar inventory icon in all PCs' inventory
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
            {
                var inv_familiar = obj.FindItemByProto(familiar_proto);
                if ((inv_familiar != null))
                {
                    // check for correct ID number
                    if ((get_ID(inv_familiar) == get_ID(attachee)))
                    {
                        var max_hp = attachee.GetStat(Stat.hp_max);
                        inv_familiar.SetInt(obj_f.item_pad_i_1, max_hp);
                        var curr_hp = attachee.GetStat(Stat.hp_current);
                        inv_familiar.SetInt(obj_f.item_pad_i_2, curr_hp);
                        pc.RemoveFollower(attachee);
                        attachee.Destroy();
                        clear_ID(inv_familiar);
                        return SkipDefault;
                    }

                }

            }

            return SkipDefault;
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
