
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
    [ObjectScript(160)]
    public class OgreChief : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5114)) // Inside Ogre Cave
            {
                if ((GetQuestState(53) == 0))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
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

            if ((attachee.GetMap() == 5114)) // Inside Ogre Cave
            {
                SetGlobalVar(14, GetGlobalVar(14) + 1);
                if ((attachee.GetStat(Stat.subdual_damage) >= attachee.GetStat(Stat.hp_current)))
                {
                    SetGlobalVar(13, GetGlobalVar(13) - 1);
                }

            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5114)) // Inside Ogre Cave
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if ((obj.GetNameId() == 14248 || obj.GetNameId() == 14249))
                    {
                        obj.TurnTowards(triggerer);
                        obj.Attack(triggerer);
                    }

                }

                DetachScript();
            }

            return RunDefault;
        }
        public override bool OnExitCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5114)) // Inside Ogre Cave
            {
                if ((attachee.GetStat(Stat.subdual_damage) >= attachee.GetStat(Stat.hp_current)))
                {
                    SetGlobalVar(13, GetGlobalVar(13) + 1);
                }

                if (GetGlobalVar(13) > 5)
                {
                    SetGlobalVar(13, 5);
                }

            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5114)) // Inside Ogre Cave
            {
                SetGlobalVar(14, GetGlobalVar(14) - 1);
            }

            return RunDefault;
        }

    }
}
