
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(71)]
    public class Zert : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 120);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnNewMap(GameObject attachee, GameObject triggerer)
        {
            if (((attachee.GetMap() == 5066) || (attachee.GetMap() == 5067) || (attachee.GetMap() == 5078) || (attachee.GetMap() == 5080)))
            {
                var leader = attachee.GetLeader();

                if ((leader != null))
                {
                    var percent = Utilities.group_percent_hp(leader);

                    if ((percent < 30))
                    {
                        if ((Utilities.obj_percent_hp(attachee) > 70))
                        {
                            leader.BeginDialog(attachee, 320);
                        }

                    }

                }

            }

            return RunDefault;
        }


    }
}
