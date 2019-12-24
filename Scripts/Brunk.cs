
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
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(168)]
    public class Brunk : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                if ((GetGlobalFlag(177)))
                {
                    triggerer.BeginDialog(attachee, 20);
                }
                else if ((triggerer.GetRace() == RaceId.half_orc))
                {
                    triggerer.BeginDialog(attachee, 10);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1);
                }

            }
            else if ((!GetGlobalFlag(178)))
            {
                triggerer.BeginDialog(attachee, 40);
            }
            else if ((GetGlobalFlag(177)))
            {
                triggerer.BeginDialog(attachee, 50);
            }
            else
            {
                triggerer.BeginDialog(attachee, 30);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(174, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(174, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(174)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }

    }
}
