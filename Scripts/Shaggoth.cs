
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
    [ObjectScript(466)]
    public class Shaggoth : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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

        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
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
        public static int sight_distance(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 50))
            {
                return 1;
            }

            return 0;
        }
        public static bool run_out(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            Sound(4164, 1);
            Sound(4161, 1);
            Sound(4163, 1);
            AttachParticles("ef-splash", attachee);
            AttachParticles("ef-MinoCloud", attachee);
            return RunDefault;
        }
        public static bool drop_out(GameObjectBody attachee, GameObjectBody triggerer)
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
}
