
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
    [ObjectScript(461)]
    public class TunnelDrains : BaseObjectScript
    {
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetNameId() == 8771))
            {
                if ((GetGlobalVar(537) != 1))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4145, 1);
                                SetGlobalVar(537, 1);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8772))
            {
                if ((GetGlobalVar(537) != 2))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4145, 1);
                                SetGlobalVar(537, 2);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8773))
            {
                if ((GetGlobalVar(537) != 3))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4145, 1);
                                SetGlobalVar(537, 3);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8774))
            {
                if ((GetGlobalVar(537) != 4))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4145, 1);
                                SetGlobalVar(537, 4);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8775))
            {
                if ((GetGlobalVar(537) != 5))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4145, 1);
                                SetGlobalVar(537, 5);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8776))
            {
                if ((GetGlobalVar(537) != 6))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4145, 1);
                                SetGlobalVar(537, 6);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8777))
            {
                if ((GetGlobalVar(537) != 7))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4146, 1);
                                SetGlobalVar(537, 7);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8778))
            {
                if ((GetGlobalVar(537) != 8))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4146, 1);
                                SetGlobalVar(537, 8);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8779))
            {
                if ((GetGlobalVar(537) != 9))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4146, 1);
                                SetGlobalVar(537, 9);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8780))
            {
                if ((GetGlobalVar(537) != 10))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4147, 1);
                                SetGlobalVar(537, 10);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8781))
            {
                if ((GetGlobalVar(537) != 11))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4147, 1);
                                SetGlobalVar(537, 11);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8782))
            {
                if ((GetGlobalVar(537) != 12))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4147, 1);
                                SetGlobalVar(537, 12);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8783))
            {
                if ((GetGlobalVar(537) != 13))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4147, 1);
                                SetGlobalVar(537, 13);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8784))
            {
                if ((GetGlobalVar(537) != 14))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4147, 1);
                                SetGlobalVar(537, 14);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8785))
            {
                if ((GetGlobalVar(537) != 15))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4147, 1);
                                SetGlobalVar(537, 15);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8786))
            {
                if ((GetGlobalVar(537) != 16))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4148, 1);
                                SetGlobalVar(537, 16);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8787))
            {
                if ((GetGlobalVar(537) != 17))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4148, 1);
                                SetGlobalVar(537, 17);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8788))
            {
                if ((GetGlobalVar(537) != 18))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4148, 1);
                                SetGlobalVar(537, 18);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8789))
            {
                if ((GetGlobalVar(537) != 19))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4148, 1);
                                SetGlobalVar(537, 19);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8790))
            {
                if ((GetGlobalVar(537) != 20))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4148, 1);
                                SetGlobalVar(537, 20);
                                if ((!GetGlobalFlag(529)))
                                {
                                    StartTimer(60000, () => reset_ggv_537(attachee));
                                    SetGlobalFlag(529, true);
                                }

                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public static int in_proximity(GameObjectBody sfx, GameObjectBody listener)
        {
            if ((sfx.DistanceTo(listener) <= 80))
            {
                return 1;
            }

            return 0;
        }
        public static void reset_ggv_537(GameObjectBody attachee)
        {
            SetGlobalVar(537, 0);
            SetGlobalFlag(529, false);
            return;
        }

    }
}
