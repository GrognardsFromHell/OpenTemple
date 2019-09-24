
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
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{

    public class SummonMonsterTools
    {
        public static void SummonMonster_Rectify_Initiative(SpellPacketBody spell, int proto_id)
        {
            var monster_obj = SummonMonster_GetHandle(spell, proto_id);
            if (monster_obj != null)
            {
                SummonMonster_Set_ID(monster_obj, RandomRange(1, 1 << 30));
                var caster_init_value = spell.caster.GetInitiative();
                monster_obj.SetInitiative(caster_init_value - 1);
                UiSystems.Combat.Initiative.UpdateIfNeeded();
            }

            return;
        }
        public static GameObjectBody SummonMonster_GetHandle(SpellPacketBody spell, int proto_id)
        {
            // Returns a handle that can be used to manipulate the familiar creature object
            foreach (var obj in ObjList.ListVicinity(spell.aoeCenter.location, ObjectListFilter.OLC_CRITTERS))
            {
                var (stlx, stly) = spell.aoeCenter.location;
                var (ox, oy) = obj.GetLocation();
                if ((obj.GetNameId() == proto_id) && (Math.Pow((ox - stlx), 2) + Math.Pow((oy - stly), 2)) <= 25)
                {
                    if (SummonMonster_Get_ID(obj) == 0)
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
        public static int SummonMonster_Set_ID(GameObjectBody obj, int val)
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

        public static List<int> GetSpellOptions(int key)
        {
            var lines = Tig.FS.ReadMesFile("mes/spells_radial_menu_options.mes");
            var options = new List<int>();
            var count = int.Parse(lines[key]);
            for (var i = 0; i < count; i++)
            {
                var protoId = int.Parse(lines[key + 1 + i]);
                options.Add(protoId);
            }

            return options;
        }

    }
}
