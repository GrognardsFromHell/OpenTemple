
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(137)]
    public class ManAtArmsPrisoner2 : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GetGlobalFlag(133)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 90);
            }
            else if ((!GetGlobalFlag(136)))
            {
                triggerer.BeginDialog(attachee, 70);
            }
            else
            {
                triggerer.BeginDialog(attachee, 120);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(137, true);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(137, false);
            return RunDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GetGlobalFlag(351)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var obj in triggerer.GetPartyMembers())
            {
                if ((obj.GetNameId() == 8030))
                {
                    triggerer.RemoveFollower(obj);
                }

            }

            return RunDefault;
        }


    }
}
