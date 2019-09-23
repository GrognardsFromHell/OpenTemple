
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
    [ObjectScript(523)]
    public class BookOfHeroes : BaseObjectScript
    {
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var loc = triggerer.GetLocation();
            var npc = GameSystems.MapObject.CreateObject(14577, loc);
            if ((npc.GetMap() == 5119))
            {
                triggerer.BeginDialog(npc, 60);
            }
            else if ((GetGlobalVar(994) != 0))
            {
                if ((GetQuestState(65) != QuestState.Completed))
                {
                    triggerer.BeginDialog(npc, 40);
                }
                else
                {
                    triggerer.BeginDialog(npc, 50);
                }

            }
            else
            {
                triggerer.BeginDialog(npc, 1);
            }

            return SkipDefault;
        }

    }
}
