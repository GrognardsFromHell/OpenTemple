
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
