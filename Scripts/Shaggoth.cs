
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

[ObjectScript(466)]
public class Shaggoth : BaseObjectScript
{
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetNameId() == 8857))
        {
            if ((GetGlobalVar(558) == 1))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

        }
        else if ((attachee.GetNameId() == 8858))
        {
            if ((GetGlobalVar(558) == 2))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetNameId() == 8857))
        {
            if ((GetGlobalVar(558) == 1))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((sight_distance(attachee, obj)))
                        {
                            StartTimer(1000, () => run_out(attachee, obj));
                            SetGlobalVar(558, 4);
                        }

                    }

                }

            }

        }
        else if ((attachee.GetNameId() == 8858))
        {
            if ((GetGlobalVar(558) == 2))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((sight_distance(attachee, obj)))
                        {
                            StartTimer(1000, () => drop_out(attachee, obj));
                            SetGlobalVar(558, 4);
                        }

                    }

                }

            }

        }

        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetNameId() == 8857))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            Sound(4163, 1);
            Sound(4161, 1);
            AttachParticles("ef-splash", attachee);
            AttachParticles("ef-ripples-huge", attachee);
        }
        else if ((attachee.GetNameId() == 8858))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            Sound(4164, 1);
            Sound(4161, 1);
            AttachParticles("ef-splash", attachee);
            AttachParticles("ef-ripples-huge", attachee);
        }

        return RunDefault;
    }
    public static bool sight_distance(GameObject speaker, GameObject listener)
    {
        return speaker.DistanceTo(listener) <= 50;
    }
    public static bool run_out(GameObject attachee, GameObject triggerer)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
        Sound(4164, 1);
        Sound(4161, 1);
        Sound(4163, 1);
        AttachParticles("ef-splash", attachee);
        AttachParticles("ef-MinoCloud", attachee);
        return RunDefault;
    }
    public static bool drop_out(GameObject attachee, GameObject triggerer)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
        Sound(4164, 1);
        Sound(4161, 1);
        Sound(4163, 1);
        AttachParticles("ef-splash", attachee);
        AttachParticles("ef-MinoCloud", attachee);
        return RunDefault;
    }

}