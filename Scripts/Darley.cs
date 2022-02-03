
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
    [ObjectScript(171)]
    public class Darley : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
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
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            attachee.FloatLine(12023, triggerer);
            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            attachee.FloatLine(12014, triggerer);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public static bool change_to_demon(GameObject darley, GameObject pc, int line)
        {
            if ((!GetGlobalFlag(179)))
            {
                SetGlobalFlag(179, true);
                var loc = darley.GetLocation();
                var rot = darley.Rotation;
                darley.Destroy();
                var new_darley = GameSystems.MapObject.CreateObject(14421, loc);
                if ((new_darley != null))
                {
                    new_darley.Rotation = rot;
                    if ((line != -1))
                    {
                        pc.BeginDialog(new_darley, line);
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnTrueSeeing(GameObject attachee, GameObject triggerer)
        {
            change_to_demon(attachee, triggerer, -1);
            return RunDefault;
        }

    }
}
