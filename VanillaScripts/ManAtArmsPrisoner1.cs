
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(136)]
    public class ManAtArmsPrisoner1 : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GetGlobalFlag(351)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(136, true);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(136, false);
            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
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
        public static bool get_rep(GameObjectBody attachee, GameObjectBody triggerer)
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
