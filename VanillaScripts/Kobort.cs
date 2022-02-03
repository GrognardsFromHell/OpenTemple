
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
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
    [ObjectScript(68)]
    public class Kobort : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetLeader() == null))
            {
                if ((GetGlobalFlag(45)))
                {
                    triggerer.BeginDialog(attachee, 100);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1);
                }

            }
            else if ((GetGlobalFlag(45)))
            {
                triggerer.BeginDialog(attachee, 250);
            }
            else
            {
                triggerer.BeginDialog(attachee, 200);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(44, true);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(44, false);
            return RunDefault;
        }
        public override bool OnJoin(GameObject attachee, GameObject triggerer)
        {
            var obj = Utilities.find_npc_near(attachee, 8004);

            if ((obj != null))
            {
                triggerer.AddFollower(obj);
            }

            return RunDefault;
        }
        public override bool OnDisband(GameObject attachee, GameObject triggerer)
        {
            foreach (var obj in triggerer.GetPartyMembers())
            {
                if ((obj.GetNameId() == 8004))
                {
                    triggerer.RemoveFollower(obj);
                }

            }

            return RunDefault;
        }


    }
}
