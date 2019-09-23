
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
    [ObjectScript(359)]
    public class Attorney : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((GetGlobalVar(969) == 1))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                return RunDefault;
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(969) == 2))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((GetGlobalVar(969) != 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((is_better_to_talk(attachee, PartyLeader)))
                {
                    if ((GetGlobalVar(969) == 2))
                    {
                        attachee.TurnTowards(triggerer);
                        PartyLeader.BeginDialog(attachee, 1);
                        SetGlobalVar(969, 3);
                    }
                    else if ((GetGlobalVar(969) == 4 && GetGlobalVar(993) == 8))
                    {
                        attachee.TurnTowards(triggerer);
                        PartyLeader.BeginDialog(attachee, 10);
                        SetGlobalVar(969, 5);
                    }
                    else if ((GetGlobalVar(969) == 4 && GetGlobalVar(993) != 8))
                    {
                        attachee.TurnTowards(triggerer);
                        PartyLeader.BeginDialog(attachee, 240);
                        SetGlobalVar(969, 5);
                    }

                }

            }

            return RunDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 20))
            {
                return true;
            }

            return false;
        }
        public static bool clear_reps(GameObjectBody attachee, GameObjectBody triggerer)
        {
            PartyLeader.RemoveReputation(35); // Constable Killer
            PartyLeader.RemoveReputation(34); // Slaughterer of Verbobonc
            if ((GetGlobalVar(993) == 8))
            {
                SetGlobalVar(993, 9); // removes Dyvers rescuer murder arrest status
            }

            return RunDefault;
        }

    }
}
