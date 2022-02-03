
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
using OpenTemple.Core.IO;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts;

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
    public static GameObject SummonMonster_GetHandle(SpellPacketBody spell, int proto_id)
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
    public static int SummonMonster_Get_ID(GameObject obj)
    {
        // Returns embedded ID number
        return obj.GetInt(obj_f.secretdoor_dc);
    }
    public static int SummonMonster_Set_ID(GameObject obj, int val)
    {
        // Embeds ID number into mobile object.  Returns ID number.
        obj.SetInt(obj_f.secretdoor_dc, val);
        return obj.GetInt(obj_f.secretdoor_dc);
    }
    public static void SummonMonster_Clear_ID(GameObject obj)
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