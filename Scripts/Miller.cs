
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

namespace Scripts
{
    [ObjectScript(14)]
    public class Miller : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public static bool make_hate(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetReaction(triggerer) >= 20))
            {
                attachee.SetReaction(triggerer, 20);
            }

            return SkipDefault;
        }
        public static bool make_worry(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetReaction(triggerer) >= 40))
            {
                attachee.SetReaction(triggerer, 40);
            }

            return SkipDefault;
        }

    }
}
