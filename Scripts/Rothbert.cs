
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
    [ObjectScript(327)]
    public class Rothbert : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 10);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(550) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                return SkipDefault;
            }
            else
            {
                Co8.StopCombat(attachee, 0);
                attachee.FloatLine(1000, triggerer);
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetQuestState(109) == QuestState.Mentioned || GetQuestState(109) == QuestState.Accepted))
            {
                if ((GetGlobalVar(536) == 4 || GetGlobalVar(536) == 5))
                {
                    if ((attachee.GetNameId() == 8819))
                    {
                        if ((GetGlobalVar(540) == 1))
                        {
                            attachee.ClearObjectFlag(ObjectFlag.OFF);
                            // game.global_vars[540] = 5
                            SetGlobalVar(536, GetGlobalVar(536) + 1);
                        }

                    }

                    if ((attachee.GetNameId() == 8820))
                    {
                        if ((GetGlobalVar(540) == 2))
                        {
                            attachee.ClearObjectFlag(ObjectFlag.OFF);
                            // game.global_vars[540] = 5
                            SetGlobalVar(536, GetGlobalVar(536) + 1);
                        }

                    }

                    if ((attachee.GetNameId() == 8821))
                    {
                        if ((GetGlobalVar(540) == 3))
                        {
                            attachee.ClearObjectFlag(ObjectFlag.OFF);
                            // game.global_vars[540] = 5
                            SetGlobalVar(536, GetGlobalVar(536) + 1);
                        }

                    }

                    if ((attachee.GetNameId() == 8822))
                    {
                        if ((GetGlobalVar(540) == 4))
                        {
                            attachee.ClearObjectFlag(ObjectFlag.OFF);
                            // game.global_vars[540] = 5
                            SetGlobalVar(536, GetGlobalVar(536) + 1);
                        }

                    }

                }
                else if ((GetGlobalVar(536) == 6))
                {
                    if ((GetGlobalVar(540) == 1 && attachee.GetNameId() == 8819))
                    {
                        if ((!GameSystems.Combat.IsCombatActive()))
                        {
                            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                            {
                                if ((close_enough(attachee, obj)))
                                {
                                    if ((obj.GetSkillLevel(attachee, SkillId.spot) >= 19))
                                    {
                                        attachee.SetConcealed(false);
                                    }

                                }

                            }

                        }

                    }
                    else if ((GetGlobalVar(540) == 2 && attachee.GetNameId() == 8820))
                    {
                        if ((!GameSystems.Combat.IsCombatActive()))
                        {
                            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                            {
                                if ((close_enough(attachee, obj)))
                                {
                                    if ((obj.GetSkillLevel(attachee, SkillId.spot) >= 19))
                                    {
                                        attachee.SetConcealed(false);
                                    }

                                }

                            }

                        }

                    }
                    else if ((GetGlobalVar(540) == 3 && attachee.GetNameId() == 8821))
                    {
                        if ((!GameSystems.Combat.IsCombatActive()))
                        {
                            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                            {
                                if ((close_enough(attachee, obj)))
                                {
                                    if ((obj.GetSkillLevel(attachee, SkillId.spot) >= 19))
                                    {
                                        attachee.SetConcealed(false);
                                    }

                                }

                            }

                        }

                    }
                    else if ((GetGlobalVar(540) == 4 && attachee.GetNameId() == 8822))
                    {
                        if ((!GameSystems.Combat.IsCombatActive()))
                        {
                            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                            {
                                if ((close_enough(attachee, obj)))
                                {
                                    if ((obj.GetSkillLevel(attachee, SkillId.spot) >= 19))
                                    {
                                        attachee.SetConcealed(false);
                                    }

                                }

                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((attachee.GetNameId() == 8819))
                {
                    var obj = Utilities.find_npc_near(attachee, 8765);
                    if ((obj != null))
                    {
                        triggerer.AddFollower(obj);
                    }

                }
                else if ((attachee.GetNameId() == 8820))
                {
                    var obj = Utilities.find_npc_near(attachee, 8768);
                    if ((obj != null))
                    {
                        triggerer.AddFollower(obj);
                    }

                }
                else if ((attachee.GetNameId() == 8821))
                {
                    var obj = Utilities.find_npc_near(attachee, 8769);
                    if ((obj != null))
                    {
                        triggerer.AddFollower(obj);
                    }

                }
                else if ((attachee.GetNameId() == 8822))
                {
                    var obj = Utilities.find_npc_near(attachee, 8799);
                    if ((obj != null))
                    {
                        triggerer.AddFollower(obj);
                    }

                }

                SetGlobalVar(550, 1);
                return RunDefault;
            }
            return RunDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                if ((attachee.GetMap() != 5141))
                {
                    Sound(4109, 1);
                    triggerer.RemoveFollower(attachee);
                    attachee.RunOff();
                    if ((attachee.GetNameId() == 8819))
                    {
                        var becka = Utilities.find_npc_near(attachee, 8765);
                        triggerer.RemoveFollower(becka);
                        becka.RunOff(attachee.GetLocation().OffsetTiles(-3, 0));
                        SetGlobalVar(550, 2);
                    }
                    else if ((attachee.GetNameId() == 8820))
                    {
                        var becka = Utilities.find_npc_near(attachee, 8768);
                        triggerer.RemoveFollower(becka);
                        becka.RunOff(attachee.GetLocation().OffsetTiles(-3, 0));
                        SetGlobalVar(550, 2);
                    }
                    else if ((attachee.GetNameId() == 8821))
                    {
                        var becka = Utilities.find_npc_near(attachee, 8769);
                        triggerer.RemoveFollower(becka);
                        becka.RunOff(attachee.GetLocation().OffsetTiles(-3, 0));
                        SetGlobalVar(550, 2);
                    }
                    else if ((attachee.GetNameId() == 8822))
                    {
                        var becka = Utilities.find_npc_near(attachee, 8799);
                        triggerer.RemoveFollower(becka);
                        becka.RunOff(attachee.GetLocation().OffsetTiles(-3, 0));
                        SetGlobalVar(550, 2);
                    }

                }

            }

            return SkipDefault;
        }
        public static bool close_enough(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 10))
            {
                return true;
            }

            return false;
        }

    }
}
