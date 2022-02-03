
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

[ObjectScript(524)]
public class MasterOfTheArena : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if (GetGlobalVar(994) == 0)
        {
            attachee.TurnTowards(triggerer);
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }

        if (GetGlobalVar(994) == 1)
        {
            if (GetGlobalFlag(944))
            {
                triggerer.BeginDialog(attachee, 100);
                return SkipDefault;
            }

        }

        if (GetGlobalVar(994) == 2)
        {
            attachee.TurnTowards(triggerer);
            if (GetGlobalVar(987) == 3)
            {
                SetGlobalVar(987, 0);
                triggerer.BeginDialog(attachee, 90);
                return SkipDefault;
            }
            else
            {
                triggerer.BeginDialog(attachee, 110);
                return SkipDefault;
            }

        }

        if (GetGlobalVar(994) == 3)
        {
            attachee.TurnTowards(triggerer);
            if (GetGlobalVar(987) == 2)
            {
                SetGlobalVar(987, 0);
                triggerer.BeginDialog(attachee, 130);
                return SkipDefault;
            }
            else
            {
                triggerer.BeginDialog(attachee, 140);
                return SkipDefault;
            }

        }

        if (GetGlobalVar(994) == 4)
        {
            attachee.TurnTowards(triggerer);
            triggerer.BeginDialog(attachee, 160);
            return SkipDefault;
        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        AttachParticles("ef-MinoCloud", attachee);
        attachee.SetObjectFlag(ObjectFlag.OFF);
        Sound(4043, 1);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(945)))
        {
            AttachParticles("ef-MinoCloud", attachee);
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            Sound(4043, 1);
            attachee.Rotation = 1.5f;
            SetGlobalFlag(945, false);
            return RunDefault;
        }
        else if ((!GameSystems.Combat.IsCombatActive()))
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((is_better_to_talk(attachee, obj)))
                {
                    if (GetGlobalVar(994) == 0)
                    {
                        obj.BeginDialog(attachee, 1);
                        return RunDefault;
                    }

                    if (GetGlobalVar(994) == 1)
                    {
                        if (GetGlobalFlag(944))
                        {
                            obj.BeginDialog(attachee, 100);
                            return RunDefault;
                        }

                    }

                    if (GetGlobalVar(994) == 2)
                    {
                        attachee.TurnTowards(obj);
                        if (GetGlobalVar(987) == 3)
                        {
                            SetGlobalVar(987, 0);
                            obj.BeginDialog(attachee, 90);
                            return RunDefault;
                        }
                        else
                        {
                            obj.BeginDialog(attachee, 110);
                            return RunDefault;
                        }

                    }

                    if (GetGlobalVar(994) == 3)
                    {
                        attachee.TurnTowards(obj);
                        if (GetGlobalVar(987) == 2)
                        {
                            SetGlobalVar(987, 0);
                            obj.BeginDialog(attachee, 130);
                            return RunDefault;
                        }
                        else
                        {
                            obj.BeginDialog(attachee, 140);
                            return RunDefault;
                        }

                    }

                    if (GetGlobalVar(994) == 4)
                    {
                        obj.BeginDialog(attachee, 160);
                        return RunDefault;
                    }

                    return RunDefault;
                }

            }

        }

        return RunDefault;
    }
    public static bool is_better_to_talk(GameObject speaker, GameObject listener)
    {
        if ((speaker.DistanceTo(listener) <= 25))
        {
            return true;
        }

        return false;
    }
    public static bool disappear(GameObject attachee, GameObject triggerer)
    {
        AttachParticles("ef-MinoCloud", attachee);
        attachee.SetObjectFlag(ObjectFlag.OFF);
        Sound(4043, 1);
        return RunDefault;
    }
    public static bool spawn_owlbears(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(955, 1);
        return RunDefault;
    }
    public static bool spawn_giants(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(954, 1);
        return RunDefault;
    }
    public static bool spawn_undead(GameObject attachee, GameObject triggerer)
    {
        var sk1 = GameSystems.MapObject.CreateObject(14107, new locXY(491, 461));
        sk1.Rotation = 3.5f;
        sk1.SetConcealed(true);
        sk1.Unconceal();
        var sk2 = GameSystems.MapObject.CreateObject(14107, new locXY(491, 464));
        sk2.Rotation = 3.5f;
        sk2.SetConcealed(true);
        sk2.Unconceal();
        var sk3 = GameSystems.MapObject.CreateObject(14107, new locXY(491, 467));
        sk3.Rotation = 3.5f;
        sk3.SetConcealed(true);
        sk3.Unconceal();
        var sk4 = GameSystems.MapObject.CreateObject(14107, new locXY(491, 470));
        sk4.Rotation = 3.5f;
        sk4.SetConcealed(true);
        sk4.Unconceal();
        var sk5 = GameSystems.MapObject.CreateObject(14107, new locXY(512, 482));
        sk5.Rotation = 5.5f;
        sk5.SetConcealed(true);
        sk5.Unconceal();
        var sk6 = GameSystems.MapObject.CreateObject(14107, new locXY(509, 482));
        sk6.Rotation = 5.5f;
        sk6.SetConcealed(true);
        sk6.Unconceal();
        var sk7 = GameSystems.MapObject.CreateObject(14107, new locXY(506, 482));
        sk7.Rotation = 5.5f;
        sk7.SetConcealed(true);
        sk7.Unconceal();
        var sk8 = GameSystems.MapObject.CreateObject(14107, new locXY(503, 482));
        sk8.Rotation = 5.5f;
        sk8.SetConcealed(true);
        sk8.Unconceal();
        var zo1 = GameSystems.MapObject.CreateObject(14092, new locXY(491, 455));
        zo1.Rotation = 3.5f;
        zo1.SetConcealed(true);
        zo1.Unconceal();
        var go1 = GameSystems.MapObject.CreateObject(14095, new locXY(488, 455));
        go1.Rotation = 3.5f;
        go1.SetConcealed(true);
        go1.Unconceal();
        var zo2 = GameSystems.MapObject.CreateObject(14092, new locXY(491, 458));
        zo2.Rotation = 3.5f;
        zo2.SetConcealed(true);
        zo2.Unconceal();
        var go2 = GameSystems.MapObject.CreateObject(14095, new locXY(488, 458));
        go2.Rotation = 3.5f;
        go2.SetConcealed(true);
        go2.Unconceal();
        var zo3 = GameSystems.MapObject.CreateObject(14092, new locXY(518, 482));
        zo3.Rotation = 5.5f;
        zo3.SetConcealed(true);
        zo3.Unconceal();
        var zo4 = GameSystems.MapObject.CreateObject(14092, new locXY(515, 482));
        zo4.Rotation = 5.5f;
        zo4.SetConcealed(true);
        zo4.Unconceal();
        var la1 = GameSystems.MapObject.CreateObject(14130, new locXY(518, 485));
        la1.Rotation = 5.5f;
        la1.SetConcealed(true);
        la1.Unconceal();
        var la2 = GameSystems.MapObject.CreateObject(14130, new locXY(515, 485));
        la2.Rotation = 5.5f;
        la2.SetConcealed(true);
        la2.Unconceal();
        var zo5 = GameSystems.MapObject.CreateObject(14092, new locXY(491, 473));
        zo5.Rotation = 3.5f;
        zo5.SetConcealed(true);
        zo5.Unconceal();
        var go3 = GameSystems.MapObject.CreateObject(14095, new locXY(488, 473));
        go3.Rotation = 3.5f;
        go3.SetConcealed(true);
        go3.Unconceal();
        var zo6 = GameSystems.MapObject.CreateObject(14092, new locXY(491, 476));
        zo6.Rotation = 3.5f;
        zo6.SetConcealed(true);
        zo6.Unconceal();
        var go4 = GameSystems.MapObject.CreateObject(14095, new locXY(488, 476));
        go4.Rotation = 3.5f;
        go4.SetConcealed(true);
        go4.Unconceal();
        var zo7 = GameSystems.MapObject.CreateObject(14092, new locXY(500, 482));
        zo7.Rotation = 5.5f;
        zo7.SetConcealed(true);
        zo7.Unconceal();
        var zo8 = GameSystems.MapObject.CreateObject(14092, new locXY(497, 482));
        zo8.Rotation = 5.5f;
        zo8.SetConcealed(true);
        zo8.Unconceal();
        var la3 = GameSystems.MapObject.CreateObject(14130, new locXY(500, 485));
        la3.Rotation = 5.5f;
        la3.SetConcealed(true);
        la3.Unconceal();
        var la4 = GameSystems.MapObject.CreateObject(14130, new locXY(497, 485));
        la4.Rotation = 5.5f;
        la4.SetConcealed(true);
        la4.Unconceal();
        var sg1 = GameSystems.MapObject.CreateObject(14602, new locXY(488, 461));
        sg1.Rotation = 3.5f;
        sg1.SetConcealed(true);
        sg1.Unconceal();
        var sg2 = GameSystems.MapObject.CreateObject(14602, new locXY(488, 470));
        sg2.Rotation = 3.5f;
        sg2.SetConcealed(true);
        sg2.Unconceal();
        var sg3 = GameSystems.MapObject.CreateObject(14602, new locXY(512, 485));
        sg3.Rotation = 5.5f;
        sg3.SetConcealed(true);
        sg3.Unconceal();
        var sg4 = GameSystems.MapObject.CreateObject(14602, new locXY(503, 485));
        sg4.Rotation = 5.5f;
        sg4.SetConcealed(true);
        sg4.Unconceal();
        var uw1 = GameSystems.MapObject.CreateObject(14599, new locXY(488, 465));
        uw1.Rotation = 3.5f;
        uw1.SetConcealed(true);
        uw1.Unconceal();
        var uw2 = GameSystems.MapObject.CreateObject(14598, new locXY(507, 485));
        uw2.Rotation = 5.5f;
        uw2.SetConcealed(true);
        uw2.Unconceal();
        Sound(4045, 1);
        SetGlobalVar(987, 0);
        return RunDefault;
    }
    public static GameObject find_npc_near(GameObject obj, int name)
    {
        foreach (var npc in ObjList.ListVicinity(obj.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if ((npc.GetNameId() == name))
            {
                return npc;
            }

        }

        return null;
    }

}