
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
    [ObjectScript(463)]
    public class TunnelVoices : BaseObjectScript
    {
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetQuestState(109) == QuestState.Mentioned || GetQuestState(109) == QuestState.Accepted))
            {
                if ((GetGlobalVar(536) >= 1))
                {
                    if ((GetGlobalVar(550) == 0))
                    {
                        if ((attachee.GetNameId() == 8853))
                        {
                            if ((GetGlobalVar(547) != 1))
                            {
                                if ((!GameSystems.Combat.IsCombatActive()))
                                {
                                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                                    {
                                        if ((in_proximity(attachee, obj)))
                                        {
                                            var drop = RandomRange(1, 12);
                                            SetGlobalVar(547, 1);
                                            if ((drop == 1))
                                            {
                                                if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }
                                                else if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }
                                                else if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }
                                                else if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }

                                            }
                                            else if ((drop == 2))
                                            {
                                                if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }
                                                else if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }
                                                else if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }
                                                else if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }

                                            }
                                            else if ((drop == 3))
                                            {
                                                if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }
                                                else if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }
                                                else if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }
                                                else if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }

                                            }
                                            else if ((drop == 4))
                                            {
                                                if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }
                                                else if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }
                                                else if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }
                                                else if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }

                                            }
                                            else if ((drop >= 5))
                                            {
                                                return RunDefault;
                                            }

                                        }

                                    }

                                }

                            }

                        }

                        if ((attachee.GetNameId() == 8854))
                        {
                            if ((GetGlobalVar(547) != 2))
                            {
                                if ((!GameSystems.Combat.IsCombatActive()))
                                {
                                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                                    {
                                        if ((in_proximity(attachee, obj)))
                                        {
                                            var drop = RandomRange(1, 12);
                                            SetGlobalVar(547, 2);
                                            if ((drop == 1))
                                            {
                                                if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }
                                                else if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }
                                                else if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }
                                                else if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }

                                            }
                                            else if ((drop == 2))
                                            {
                                                if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }
                                                else if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }
                                                else if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }
                                                else if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }

                                            }
                                            else if ((drop == 3))
                                            {
                                                if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }
                                                else if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }
                                                else if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }
                                                else if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }

                                            }
                                            else if ((drop == 4))
                                            {
                                                if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }
                                                else if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }
                                                else if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }
                                                else if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }

                                            }
                                            else if ((drop >= 5))
                                            {
                                                return RunDefault;
                                            }

                                        }

                                    }

                                }

                            }

                        }

                        if ((attachee.GetNameId() == 8855))
                        {
                            if ((GetGlobalVar(547) != 3))
                            {
                                if ((!GameSystems.Combat.IsCombatActive()))
                                {
                                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                                    {
                                        if ((in_proximity(attachee, obj)))
                                        {
                                            var drop = RandomRange(1, 12);
                                            SetGlobalVar(547, 3);
                                            if ((drop == 1))
                                            {
                                                if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }
                                                else if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }
                                                else if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }
                                                else if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }

                                            }
                                            else if ((drop == 2))
                                            {
                                                if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }
                                                else if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }
                                                else if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }
                                                else if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }

                                            }
                                            else if ((drop == 3))
                                            {
                                                if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }
                                                else if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }
                                                else if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }
                                                else if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }

                                            }
                                            else if ((drop == 4))
                                            {
                                                if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }
                                                else if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }
                                                else if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }
                                                else if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }

                                            }
                                            else if ((drop >= 5))
                                            {
                                                return RunDefault;
                                            }

                                        }

                                    }

                                }

                            }

                        }

                        if ((attachee.GetNameId() == 8856))
                        {
                            if ((GetGlobalVar(547) != 4))
                            {
                                if ((!GameSystems.Combat.IsCombatActive()))
                                {
                                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                                    {
                                        if ((in_proximity(attachee, obj)))
                                        {
                                            var drop = RandomRange(1, 12);
                                            SetGlobalVar(547, 4);
                                            if ((drop == 1))
                                            {
                                                if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }
                                                else if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }
                                                else if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }
                                                else if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }

                                            }
                                            else if ((drop == 2))
                                            {
                                                if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }
                                                else if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }
                                                else if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }
                                                else if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }

                                            }
                                            else if ((drop == 3))
                                            {
                                                if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }
                                                else if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }
                                                else if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }
                                                else if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }

                                            }
                                            else if ((drop == 4))
                                            {
                                                if ((GetGlobalVar(532) == 0))
                                                {
                                                    Sound(4152);
                                                    SetGlobalVar(532, 1);
                                                }
                                                else if ((GetGlobalVar(529) == 0))
                                                {
                                                    Sound(4149);
                                                    SetGlobalVar(529, 1);
                                                }
                                                else if ((GetGlobalVar(530) == 0))
                                                {
                                                    Sound(4150);
                                                    SetGlobalVar(530, 1);
                                                }
                                                else if ((GetGlobalVar(531) == 0))
                                                {
                                                    Sound(4151);
                                                    SetGlobalVar(531, 1);
                                                }

                                            }
                                            else if ((drop >= 5))
                                            {
                                                return RunDefault;
                                            }

                                        }

                                    }

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
            return sfx.DistanceTo(listener) <= 80;
        }

    }
}
