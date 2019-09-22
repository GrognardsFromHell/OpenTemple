
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
    [ObjectScript(16)]
    public class ProsperousFarmer : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(196)))
            {
                triggerer.BeginDialog(attachee, 320);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                if ((GetQuestState(6) == QuestState.Completed))
                {
                    triggerer.BeginDialog(attachee, 20);
                }
                else if ((GetQuestState(6) <= QuestState.Accepted))
                {
                    if ((!GetGlobalFlag(10)))
                    {
                        triggerer.BeginDialog(attachee, 120);
                    }
                    else
                    {
                        triggerer.BeginDialog(attachee, 150);
                    }

                }

            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetQuestState(6, QuestState.Botched);
            SetGlobalFlag(333, true);
            SetGlobalVar(23, GetGlobalVar(23) + 1);
            if ((GetGlobalVar(23) >= 2))
            {
                PartyLeader.AddReputation(1);
            }

            return RunDefault;
        }
        public static bool give_sword(GameObjectBody pc)
        {
            var item = GameSystems.MapObject.CreateObject(4222, pc.GetLocation());

            pc.GetItem(item);
            return RunDefault;
        }


    }
}
