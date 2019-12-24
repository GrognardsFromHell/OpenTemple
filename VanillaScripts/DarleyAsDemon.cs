
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
    [ObjectScript(229)]
    public class DarleyAsDemon : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                if ((GetGlobalFlag(179)))
                {
                    if ((GetGlobalFlag(180)))
                    {
                        triggerer.BeginDialog(attachee, 170);
                    }
                    else
                    {
                        triggerer.BeginDialog(attachee, 210);
                    }

                }
                else
                {
                    triggerer.BeginDialog(attachee, 150);
                }

            }
            else if (((GetGlobalFlag(179)) && (!GetGlobalFlag(180))))
            {
                triggerer.BeginDialog(attachee, 120);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if (((GetGlobalFlag(179)) && (GetGlobalFlag(180))))
            {
                triggerer.BeginDialog(attachee, 130);
            }
            else
            {
                triggerer.BeginDialog(attachee, 140);
            }

            return SkipDefault;
        }


    }
}
