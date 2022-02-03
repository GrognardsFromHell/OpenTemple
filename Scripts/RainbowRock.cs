
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
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts;

[ObjectScript(265)]
public class RainbowRock : BaseObjectScript
{
    public override bool OnUse(GameObject attachee, GameObject triggerer)
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
    public bool create_rainbow_undead()
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
    public override bool OnRemoveItem(GameObject attachee, GameObject triggerer)
    {
        if (((GetQuestState(27) == QuestState.Mentioned) || (GetQuestState(27) == QuestState.Accepted)))
        {
            SetQuestState(27, QuestState.Completed);
        }

        return RunDefault;
    }

}