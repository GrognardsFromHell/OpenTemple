
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
    [ObjectScript(161)]
    public class FemalePrisoner1 : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(131)))
            {
                if ((triggerer.GetGender() == Gender.Male))
                {
                    triggerer.BeginDialog(attachee, 40);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 50);
                }

            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public static bool get_rep(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (!triggerer.HasReputation(7))
            {
                triggerer.AddReputation(7);
            }

            SetGlobalVar(25, GetGlobalVar(25) + 2);
            if ((GetGlobalVar(25) >= 3 && !triggerer.HasReputation(8)))
            {
                triggerer.AddReputation(8);
            }

            return RunDefault;
        }
        public static bool free_rep(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (!triggerer.HasReputation(16))
            {
                triggerer.AddReputation(16);
            }

            SetGlobalVar(26, GetGlobalVar(26) + 1);
            if ((GetGlobalVar(26) >= 3 && !triggerer.HasReputation(17)))
            {
                triggerer.AddReputation(17);
            }

            return RunDefault;
        }


    }
}
