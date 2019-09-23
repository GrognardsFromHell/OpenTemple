
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

    public class SummonMonsterTools
    {
        public static void SummonMonster_Rectify_Initiative(SpellPacketBody spell, FIXME proto_id)
        {
            var monster_obj = SummonMonster_GetHandle(spell, proto_id);
            if (monster_obj != null)
            {
                SummonMonster_Set_ID(monster_obj, RandomRange(1, Math.Pow(2, 30)));
                var caster_init_value = spell.caster.GetInitiative();
                monster_obj.set_initiative/*Unknown*/(caster_init_value - 1);
                UiSystems.Combat.Initiative.UpdateIfNeeded();
            }

            return;
        }
        public static GameObjectBody SummonMonster_GetHandle(SpellPacketBody spell, FIXME proto_id)
        {
            // Returns a handle that can be used to manipulate the familiar creature object
            foreach (var obj in ObjList.ListVicinity(spell.aoeCenter, ObjectListFilter.OLC_CRITTERS))
            {
                var stl = spell.aoeCenter;
                var (stlx, stly) = stl;
                var (ox, oy) = obj.GetLocation();
                if ((obj.GetNameId() == proto_id) && (Math.Pow((ox - stlx), 2) + Math.Pow((oy - stly), 2)) <= 25)
                {
                    if (!(SummonMonster_Get_ID(obj)))
                    {
                        return obj;
                    }

                }

            }

            return null;
        }
        public static int SummonMonster_Get_ID(GameObjectBody obj)
        {
            // Returns embedded ID number
            return obj.GetInt(obj_f.secretdoor_dc);
        }
        public static int SummonMonster_Set_ID(GameObjectBody obj, FIXME val)
        {
            // Embeds ID number into mobile object.  Returns ID number.
            obj.SetInt(obj_f.secretdoor_dc, val);
            return obj.GetInt(obj_f.secretdoor_dc);
        }
        public static void SummonMonster_Clear_ID(GameObjectBody obj)
        {
            // Clears embedded ID number from mobile object
            obj.SetInt(obj_f.secretdoor_dc, 0);
        }

    }
}
