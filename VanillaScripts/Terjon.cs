
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
    [ObjectScript(23)]
    public class Terjon : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(28) && !triggerer.HasReputation(2)))
            {
                triggerer.AddReputation(2);
            }

            if ((attachee.HasMet(triggerer)))
            {
                if ((GetGlobalVar(5) >= 9))
                {
                    triggerer.BeginDialog(attachee, 50);
                }
                else if ((PartyAlignment == Alignment.NEUTRAL_EVIL))
                {
                    triggerer.BeginDialog(attachee, 90);
                }
                else if ((GetGlobalVar(5) <= 4))
                {
                    triggerer.BeginDialog(attachee, 30);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 40);
                }

            }
            else if ((PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL))
            {
                SetGlobalVar(5, 3);
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL))
            {
                SetGlobalVar(5, 5);
                triggerer.BeginDialog(attachee, 20);
            }
            else
            {
                SetGlobalVar(5, 4);
                triggerer.BeginDialog(attachee, 10);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(21)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(299, true);
            SetGlobalVar(23, GetGlobalVar(23) + 1);
            if (GetGlobalVar(23) >= 2)
            {
                PartyLeader.AddReputation(1);
            }

            return RunDefault;
        }


    }
}
