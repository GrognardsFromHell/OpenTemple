
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
    [ObjectScript(89)]
    public class Glora : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5006))
            {
                attachee.FloatLine(23000, triggerer);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5007))
            {
                if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                    if ((!GetGlobalFlag(908)))
                    {
                        StartTimer(3600000, () => respawn(attachee)); // 3600000ms is 1 hour
                        SetGlobalFlag(908, true);
                    }

                }

            }
            else if ((attachee.GetMap() == 5006))
            {
                if ((GetGlobalVar(510) != 2))
                {
                    if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }

                }
                else
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(961) == 3))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    if ((is_better_to_talk(attachee, PartyLeader)))
                    {
                        SetGlobalVar(961, 4);
                        attachee.TurnTowards(PartyLeader);
                        PartyLeader.BeginDialog(attachee, 90);
                    }

                }

            }

            return RunDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 10))
            {
                return true;
            }

            return false;
        }
        public static void respawn(GameObjectBody attachee)
        {
            var box = Utilities.find_container_near(attachee, 1001);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(3600000, () => respawn(attachee)); // 3600000ms is 1 hour
            return;
        }

    }
}