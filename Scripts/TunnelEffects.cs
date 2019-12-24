
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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

namespace Scripts
{
    [ObjectScript(462)]
    public class TunnelEffects : BaseObjectScript
    {
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetNameId() == 8823))
            {
                if ((GetGlobalVar(546) != 1))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 1);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8824))
            {
                if ((GetGlobalVar(546) != 2))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 2);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8825))
            {
                if ((GetGlobalVar(546) != 3))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 3);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8826))
            {
                if ((GetGlobalVar(546) != 4))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 4);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8827))
            {
                if ((GetGlobalVar(546) != 5))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 5);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8828))
            {
                if ((GetGlobalVar(546) != 6))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 6);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8829))
            {
                if ((GetGlobalVar(546) != 7))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 7);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8830))
            {
                if ((GetGlobalVar(546) != 8))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 8);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8831))
            {
                if ((GetGlobalVar(546) != 9))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 9);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8832))
            {
                if ((GetGlobalVar(546) != 10))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 10);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8833))
            {
                if ((GetGlobalVar(546) != 11))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 11);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8834))
            {
                if ((GetGlobalVar(546) != 12))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 12);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8835))
            {
                if ((GetGlobalVar(546) != 13))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 13);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8836))
            {
                if ((GetGlobalVar(546) != 14))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 14);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8837))
            {
                if ((GetGlobalVar(546) != 15))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 15);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8838))
            {
                if ((GetGlobalVar(546) != 16))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 16);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8839))
            {
                if ((GetGlobalVar(546) != 17))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 17);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8840))
            {
                if ((GetGlobalVar(546) != 18))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 18);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8841))
            {
                if ((GetGlobalVar(546) != 19))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 19);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8842))
            {
                if ((GetGlobalVar(546) != 20))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 20);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8843))
            {
                if ((GetGlobalVar(546) != 21))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 21);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8844))
            {
                if ((GetGlobalVar(546) != 22))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 22);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8845))
            {
                if ((GetGlobalVar(546) != 23))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 23);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8846))
            {
                if ((GetGlobalVar(546) != 24))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 24);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8847))
            {
                if ((GetGlobalVar(546) != 25))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 25);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8848))
            {
                if ((GetGlobalVar(546) != 26))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 26);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8849))
            {
                if ((GetGlobalVar(546) != 27))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 27);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8850))
            {
                if ((GetGlobalVar(546) != 28))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 28);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8851))
            {
                if ((GetGlobalVar(546) != 29))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 29);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            if ((attachee.GetNameId() == 8852))
            {
                if ((GetGlobalVar(546) != 30))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((in_proximity(attachee, obj)))
                            {
                                var drop = RandomRange(1, 40);
                                SetGlobalVar(546, 30);
                                if ((drop == 1))
                                {
                                    Sound(4153);
                                }
                                else if ((drop == 2))
                                {
                                    Sound(4154);
                                }
                                else if ((drop == 3))
                                {
                                    Sound(4155);
                                }
                                else if ((drop == 4))
                                {
                                    Sound(4156);
                                }
                                else if ((drop == 5))
                                {
                                    Sound(4157);
                                }
                                else if ((drop == 6))
                                {
                                    Sound(4158);
                                }
                                else if ((drop == 7))
                                {
                                    Sound(4159);
                                }
                                else if ((drop == 8))
                                {
                                    Sound(4160);
                                }
                                else if ((drop == 9))
                                {
                                    Sound(4161);
                                }
                                else if ((drop == 10))
                                {
                                    Sound(4162);
                                }
                                else if ((drop >= 11))
                                {
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool in_proximity(GameObjectBody sfx, GameObjectBody listener)
        {
            return sfx.DistanceTo(listener) <= 60;
        }

    }
}
