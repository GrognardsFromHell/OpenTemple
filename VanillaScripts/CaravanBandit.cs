
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
    [ObjectScript(188)]
    public class CaravanBandit : BaseObjectScript
    {

        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(27, GetGlobalVar(27) + 1);
            if ((GetGlobalVar(27) >= 3))
            {
                GameSystems.MapObject.CreateObject(14316, attachee.GetLocation());
                SetGlobalVar(27, -1000);
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            attachee.FloatLine(RandomRange(1, 3), triggerer);
            return RunDefault;
        }
        public override bool OnExitCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetStat(Stat.subdual_damage) >= attachee.GetStat(Stat.hp_current)))
            {
                SetGlobalVar(27, GetGlobalVar(27) + 1);
                if ((GetGlobalVar(27) >= 3))
                {
                    GameSystems.MapObject.CreateObject(14316, attachee.GetLocation());
                    SetGlobalVar(27, -1000);
                }

            }

            return RunDefault;
        }


    }
}
