
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
    [ObjectScript(370)]
    public class Innkeeper2 : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(961) == 1))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    if ((is_better_to_talk(attachee, PartyLeader)))
                    {
                        SetGlobalVar(961, 2);
                        attachee.TurnTowards(PartyLeader);
                        PartyLeader.BeginDialog(attachee, 160);
                    }

                }

            }

            return RunDefault;
        }
        public static bool set_room_flag(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(997, true);
            StartTimer(86390000, () => room_no_longer_available());
            GameSystems.RandomEncounter.UpdateSleepStatus();
            return RunDefault;
        }
        public static bool room_no_longer_available()
        {
            SetGlobalFlag(997, false);
            GameSystems.RandomEncounter.UpdateSleepStatus();
            return RunDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 25))
            {
                return true;
            }

            return false;
        }

    }
}
