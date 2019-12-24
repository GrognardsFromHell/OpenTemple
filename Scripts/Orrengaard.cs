
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
    [ObjectScript(399)]
    public class Orrengaard : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 7);
            }

            if ((attachee.HasMet(triggerer) || GetGlobalVar(938) == 1))
            {
                triggerer.BeginDialog(attachee, 4);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            StartTimer(86400000, () => new_orrengaard(attachee)); // 1 day
            var boots = attachee.FindItemByName(6011);
            boots.SetObjectFlag(ObjectFlag.OFF);
            var gloves = attachee.FindItemByName(6012);
            gloves.SetObjectFlag(ObjectFlag.OFF);
            var clothes = attachee.FindItemByName(6344);
            clothes.SetObjectFlag(ObjectFlag.OFF);
            var staff = attachee.FindItemByName(4214);
            staff.SetObjectFlag(ObjectFlag.OFF);
            var robe = attachee.FindItemByName(6333);
            robe.SetObjectFlag(ObjectFlag.OFF);
            var circlet = attachee.FindItemByName(6335);
            circlet.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }
        // used by an NPC cabbage to respawn orrengaard

        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // def san_first_heartbeat( attachee, triggerer ):		#used by an NPC cabbage to respawn orrengaard
            if ((GetGlobalVar(938) == 2))
            {
                var orrengaard = GameSystems.MapObject.CreateObject(14570, new locXY(555, 572));
                orrengaard.Rotation = 3f;
                SetGlobalVar(938, 1);
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(938) != 1))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((is_better_to_talk(attachee, obj)))
                        {
                            attachee.TurnTowards(obj);
                            obj.BeginDialog(attachee, 1);
                            DetachScript();
                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5163))
            {
                SelectedPartyLeader.BeginDialog(attachee, 340);
            }
            else if ((attachee.GetMap() != 5184 && attachee.GetMap() != 5163))
            {
                triggerer.RemoveFollower(attachee);
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return SkipDefault;
        }
        public static bool new_orrengaard(GameObjectBody attachee)
        {
            SetGlobalVar(938, 2);
            return RunDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 10))
                {
                    return true;
                }

            }

            return false;
        }

    }
}
