
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
    [ObjectScript(171)]
    public class Darley : BaseObjectScript
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
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.FloatLine(12023, triggerer);
            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
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
        public static bool change_to_demon(GameObjectBody darley, GameObjectBody pc, int line)
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
        public override bool OnTrueSeeing(GameObjectBody attachee, GameObjectBody triggerer)
        {
            change_to_demon(attachee, triggerer, -1);
            return RunDefault;
        }

    }
}
