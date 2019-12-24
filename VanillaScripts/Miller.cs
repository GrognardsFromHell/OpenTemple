
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(14)]
    public class Miller : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public static bool make_hate(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetReaction(triggerer) >= 20))
            {
                attachee.SetReaction(triggerer, 20);
            }

            return SkipDefault;
        }
        public static bool make_worry(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetReaction(triggerer) >= 40))
            {
                attachee.SetReaction(triggerer, 40);
            }

            return SkipDefault;
        }


    }
}
