
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
    [ObjectScript(136)]
    public class ManAtArmsPrisoner1 : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                if (((GetGlobalFlag(137)) && (GetGlobalFlag(138))))
                {
                    triggerer.BeginDialog(attachee, 160);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 100);
                }

            }
            else if (((!attachee.HasMet(triggerer)) || (!GetGlobalFlag(133))))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if (((GetGlobalFlag(137)) && (GetGlobalFlag(138))))
            {
                triggerer.BeginDialog(attachee, 190);
            }
            else
            {
                triggerer.BeginDialog(attachee, 150);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!GetGlobalFlag(351)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(136, true);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(136, false);
            return RunDefault;
        }
        public override bool OnJoin(GameObject attachee, GameObject triggerer)
        {
            attachee.SetLootSharingType(LootSharingType.nothing);
            var obj = Utilities.find_npc_near(attachee, 8029);

            if ((obj != null))
            {
                triggerer.AddFollower(obj);
                obj.SetLootSharingType(LootSharingType.nothing);
            }

            obj = Utilities.find_npc_near(attachee, 8030);

            if ((obj != null))
            {
                triggerer.AddFollower(obj);
                obj.SetLootSharingType(LootSharingType.nothing);
            }

            return RunDefault;
        }
        public override bool OnDisband(GameObject attachee, GameObject triggerer)
        {
            foreach (var obj in triggerer.GetPartyMembers())
            {
                if ((obj.GetNameId() == 8029))
                {
                    triggerer.RemoveFollower(obj);
                }

                if ((obj.GetNameId() == 8030))
                {
                    triggerer.RemoveFollower(obj);
                }

            }

            return RunDefault;
        }
        public static bool get_rep(GameObject attachee, GameObject triggerer)
        {
            if (!triggerer.HasReputation(16))
            {
                triggerer.AddReputation(16);
            }

            SetGlobalVar(26, GetGlobalVar(26) + 1);
            if ((GetGlobalVar(26) >= 3 && !triggerer.HasReputation(17)))
            {
                triggerer.AddReputation(17);
            }

            return RunDefault;
        }


    }
}
