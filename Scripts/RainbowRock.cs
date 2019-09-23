
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
    [ObjectScript(265)]
    public class RainbowRock : BaseObjectScript
    {
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((GetQuestState(27) == QuestState.Mentioned) || (GetQuestState(27) == QuestState.Accepted)))
            {
                SetQuestState(27, QuestState.Completed);
            }

            SetGlobalVar(708, 0);
            // game.particles( "sp-summon monster I", game.leader)
            var monsterA = GameSystems.MapObject.CreateObject(14601, new locXY(487, 505));
            monsterA.TurnTowards(triggerer);
            monsterA.Attack(triggerer);
            monsterA.SetConcealed(true);
            var monsterC = GameSystems.MapObject.CreateObject(14602, new locXY(487, 506));
            monsterC.TurnTowards(triggerer);
            monsterC.SetConcealed(true);
            var monsterD = GameSystems.MapObject.CreateObject(14602, new locXY(487, 504));
            monsterD.TurnTowards(triggerer);
            monsterD.SetConcealed(true);
            var monsterE = GameSystems.MapObject.CreateObject(14602, new locXY(488, 505));
            monsterE.TurnTowards(triggerer);
            monsterE.SetConcealed(true);
            DetachScript();
            return RunDefault;
        }
        public static bool create_rainbow_undead()
        {
            if (((GetQuestState(27) == QuestState.Mentioned) || (GetQuestState(27) == QuestState.Accepted)))
            {
                SetQuestState(27, QuestState.Completed);
            }

            SetGlobalVar(708, 0);
            // game.particles( "sp-summon monster I", game.leader)
            var monsterA = GameSystems.MapObject.CreateObject(14601, new locXY(487, 505));
            // monsterA.turn_towards(triggerer)
            var monsterC = GameSystems.MapObject.CreateObject(14602, new locXY(487, 506));
            var monsterD = GameSystems.MapObject.CreateObject(14602, new locXY(487, 504));
            var monsterE = GameSystems.MapObject.CreateObject(14602, new locXY(488, 505));
            DetachScript();
            return RunDefault;
        }
        public override bool OnRemoveItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((GetQuestState(27) == QuestState.Mentioned) || (GetQuestState(27) == QuestState.Accepted)))
            {
                SetQuestState(27, QuestState.Completed);
            }

            return RunDefault;
        }

    }
}
