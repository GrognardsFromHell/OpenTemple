
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
    [ObjectScript(129)]
    public class Wonnilon : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((!attachee.HasMet(triggerer)) || (GetGlobalFlag(129))))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 200);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(342)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            if ((GetGlobalFlag(128)))
            {
                var obj = attachee.GetLeader();
                if ((obj != null))
                {
                    attachee.StealFrom(obj);
                    obj.RemoveFollower(attachee);
                    obj.BeginDialog(attachee, 430);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            attachee.FloatLine(12014, triggerer);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            CombatStandardRoutines.ProtectTheInnocent(attachee, triggerer);
            attachee.FloatLine(12057, triggerer);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if (((GetQuestState(56) == QuestState.Unknown) && (attachee.GetLeader() != null)))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if (((obj.FindItemByName(2205) != null) && (obj.FindItemByName(4002) != null)))
                        {
                            obj.BeginDialog(attachee, 400);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RunOff();
            return RunDefault;
        }
        public static bool go_hideout(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // move Wonnilon to his hideout
            attachee.SetStandpoint(StandPointType.Night, 254);
            attachee.SetStandpoint(StandPointType.Day, 254);
            SetGlobalFlag(342, true);
            if ((attachee.GetMap() == 5066))
            {
                attachee.Move(new locXY(418, 374));
            }
            else
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public static bool disappear(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((attachee.HasLineOfSight(obj)))
                {
                    attachee.StealFrom(obj);
                }

            }

            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }

    }
}
