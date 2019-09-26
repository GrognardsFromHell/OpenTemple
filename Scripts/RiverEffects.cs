
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
    [ObjectScript(467)]
    public class RiverEffects : BaseObjectScript
    {
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetNameId() == 8861))
            {
                if ((GetGlobalVar(563) != 1))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 1);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8862))
            {
                if ((GetGlobalVar(563) != 2))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 2);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8863))
            {
                if ((GetGlobalVar(563) != 3))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 3);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8864))
            {
                if ((GetGlobalVar(563) != 4))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 4);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8865))
            {
                if ((GetGlobalVar(563) != 5))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 5);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8866))
            {
                if ((GetGlobalVar(563) != 6))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 6);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8867))
            {
                if ((GetGlobalVar(563) != 7))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 7);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8868))
            {
                if ((GetGlobalVar(563) != 8))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 8);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8869))
            {
                if ((GetGlobalVar(563) != 9))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 9);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8870))
            {
                if ((GetGlobalVar(563) != 10))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 10);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8871))
            {
                if ((GetGlobalVar(563) != 11))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 11);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8872))
            {
                if ((GetGlobalVar(563) != 12))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 12);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8873))
            {
                if ((GetGlobalVar(563) != 13))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 13);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8874))
            {
                if ((GetGlobalVar(563) != 14))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 14);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8875))
            {
                if ((GetGlobalVar(563) != 15))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 15);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8876))
            {
                if ((GetGlobalVar(563) != 16))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 16);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8877))
            {
                if ((GetGlobalVar(563) != 17))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 17);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8878))
            {
                if ((GetGlobalVar(563) != 18))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 18);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8879))
            {
                if ((GetGlobalVar(563) != 19))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 19);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8880))
            {
                if ((GetGlobalVar(563) != 20))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 20);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8881))
            {
                if ((GetGlobalVar(563) != 21))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 21);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8882))
            {
                if ((GetGlobalVar(563) != 22))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 22);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8883))
            {
                if ((GetGlobalVar(563) != 23))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 23);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8884))
            {
                if ((GetGlobalVar(563) != 24))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 24);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8885))
            {
                if ((GetGlobalVar(563) != 25))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 25);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8886))
            {
                if ((GetGlobalVar(563) != 26))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 26);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8887))
            {
                if ((GetGlobalVar(563) != 27))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 27);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8888))
            {
                if ((GetGlobalVar(563) != 28))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 28);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8889))
            {
                if ((GetGlobalVar(563) != 29))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 29);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8890))
            {
                if ((GetGlobalVar(563) != 30))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                Sound(4039, 1);
                                SetGlobalVar(563, 30);
                                if ((!GetGlobalFlag(546)))
                                {
                                    StartTimer(30000, () => reset_ggv_563(attachee));
                                    SetGlobalFlag(546, true);
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8891))
            {
                if ((!GetGlobalFlag(547)))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity_180(attachee, obj)))
                            {
                                Sound(4040, 1);
                                SetGlobalFlag(547, true);
                                StartTimer(7000, () => reset_ggf_547(attachee));
                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8892))
            {
                if ((!GetGlobalFlag(548)))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity_60(attachee, obj)))
                            {
                                Sound(4020, 1);
                                SetGlobalFlag(548, true);
                                StartTimer(7300, () => reset_ggf_548(attachee));
                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool in_proximity(GameObjectBody sfx, GameObjectBody listener)
        {
            if ((sfx.DistanceTo(listener) <= 40))
            {
                return true;
            }

            return false;
        }
        public static int in_proximity_60(GameObjectBody sfx, GameObjectBody listener)
        {
            if ((sfx.DistanceTo(listener) <= 60))
            {
                return 1;
            }

            return 0;
        }
        public static int in_proximity_180(GameObjectBody sfx, GameObjectBody listener)
        {
            if ((sfx.DistanceTo(listener) <= 120))
            {
                return 1;
            }

            return 0;
        }
        public static void reset_ggv_563(GameObjectBody attachee)
        {
            SetGlobalVar(563, 0);
            SetGlobalFlag(546, false);
            return;
        }
        public static void reset_ggf_547(GameObjectBody attachee)
        {
            SetGlobalFlag(547, false);
            return;
        }
        public static void reset_ggf_548(GameObjectBody attachee)
        {
            SetGlobalFlag(548, false);
            return;
        }

    }
}
