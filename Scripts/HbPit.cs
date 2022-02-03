
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

namespace Scripts;

[ObjectScript(622)]
public class HbPit : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        triggerer.BeginDialog(attachee, 1);
        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(568) == 1))
        {
            SetGlobalVar(569, GetGlobalVar(569) + 1);
            if ((GetGlobalVar(569) == 4))
            {
                Sound(4180, 1);
            }
            else if ((GetGlobalVar(569) == 7))
            {
                spawn_hydra(attachee, triggerer);
            }
            else if ((GetGlobalVar(569) == 10))
            {
                spawn_others(attachee, triggerer);
                SetGlobalVar(569, 0);
            }

        }

        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
        {
            if ((ScriptDaemon.within_rect_by_corners(obj, 538, 394, 531, 408)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                obj.AddCondition("Prone", 1, 0);
                AttachParticles("ef-splash", obj);
                Sound(4177, 1);
                StartTimer(300, () => relocate_west(attachee, obj));
                obj.BeginDialog(attachee, 1);
            }

            if ((ScriptDaemon.within_rect_by_corners(obj, 487, 398, 481, 412)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                obj.AddCondition("Prone", 1, 0);
                AttachParticles("ef-splash", obj);
                Sound(4177, 1);
                StartTimer(300, () => relocate_east(attachee, obj));
                obj.BeginDialog(attachee, 1);
            }

        }

        return RunDefault;
    }
    public static void spawn_hydra(GameObject attachee, GameObject triggerer)
    {
        var hydra = GameSystems.MapObject.CreateObject(14982, new locXY(511, 423));
        hydra.Move(new locXY(511, 424));
        hydra.Rotation = 5.49778714378f;
        hydra.SetConcealed(true);
        hydra.Unconceal();
        hydra.ClearNpcFlag(NpcFlag.KOS);
        AttachParticles("Mon-YellowMold-60", hydra);
        Sound(4179, 1);
        return;
    }
    public static void spawn_others(GameObject attachee, GameObject triggerer)
    {
        var picker1 = RandomRange(14978, 14981);
        var animal1 = GameSystems.MapObject.CreateObject(picker1, new locXY(511, 423));
        animal1.Move(new locXY(515, 424));
        animal1.Rotation = 5.39778714378f;
        animal1.SetConcealed(true);
        animal1.Unconceal();
        AttachParticles("Mon-YellowMold-20", animal1);
        var picker2 = RandomRange(14978, 14981);
        var animal2 = GameSystems.MapObject.CreateObject(picker2, new locXY(511, 423));
        animal2.Move(new locXY(507, 424));
        animal2.Rotation = 5.59778714378f;
        animal2.SetConcealed(true);
        animal2.Unconceal();
        AttachParticles("Mon-YellowMold-30", animal2);
        var picker3 = RandomRange(14978, 14981);
        var animal3 = GameSystems.MapObject.CreateObject(picker3, new locXY(511, 423));
        animal3.Move(new locXY(519, 424));
        animal3.Rotation = 5.29778714378f;
        animal3.SetConcealed(true);
        animal3.Unconceal();
        AttachParticles("Mon-YellowMold-40", animal3);
        var picker4 = RandomRange(14978, 14981);
        var animal4 = GameSystems.MapObject.CreateObject(picker4, new locXY(511, 423));
        animal4.Move(new locXY(503, 424));
        animal4.Rotation = 5.69778714378f;
        animal4.SetConcealed(true);
        animal4.Unconceal();
        AttachParticles("Mon-YellowMold-60", animal4);
        var picker5 = RandomRange(14978, 14981);
        var animal5 = GameSystems.MapObject.CreateObject(picker5, new locXY(511, 423));
        animal5.Move(new locXY(523, 424));
        animal5.Rotation = 5.19778714378f;
        animal5.SetConcealed(true);
        animal5.Unconceal();
        AttachParticles("Mon-YellowMold-30", animal5);
        var picker6 = RandomRange(14978, 14981);
        var animal6 = GameSystems.MapObject.CreateObject(picker6, new locXY(511, 423));
        animal6.Move(new locXY(499, 424));
        animal6.Rotation = 5.79778714378f;
        animal6.SetConcealed(true);
        animal6.Unconceal();
        AttachParticles("Mon-YellowMold-20", animal6);
        Sound(4181, 1);
        SetGlobalVar(568, 9);
        return;
    }
    public static void relocate_west(GameObject attachee, GameObject obj)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
        var coord_x = RandomRange(522, 526);
        var coord_y = RandomRange(405, 409);
        obj.Move(new locXY(coord_x, coord_y));
        AttachParticles("Mon-Phycomid-10", obj);
        Sound(4186, 1);
        obj.FloatMesFileLine("mes/float.mes", 2);
        SetGlobalFlag(557, true);
        if ((GetGlobalVar(568) == 0))
        {
            SetGlobalVar(568, 1);
        }

        return;
    }
    public static void relocate_east(GameObject attachee, GameObject obj)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
        var coord_x = RandomRange(499, 503);
        var coord_y = RandomRange(405, 409);
        obj.Move(new locXY(coord_x, coord_y));
        AttachParticles("Mon-Phycomid-10", obj);
        Sound(4186, 1);
        obj.FloatMesFileLine("mes/float.mes", 2);
        SetGlobalFlag(557, true);
        if ((GetGlobalVar(568) == 0))
        {
            SetGlobalVar(568, 1);
        }

        return;
    }

}