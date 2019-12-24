
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
    [ObjectScript(151)]
    public class KellaHillGiant : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((triggerer.GetStat(Stat.level_druid) >= 1) || (PartyAlignment == Alignment.NEUTRAL)))
            {
                if ((!attachee.HasMet(triggerer)))
                {
                    triggerer.BeginDialog(attachee, 1);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 70);
                }

            }
            else
            {
                var line = (RandomRange(0, 2) * 10) + 80;
                triggerer.BeginDialog(attachee, line);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetCounter(0, GetCounter(0) + 1);
            if ((GetCounter(0) >= 3))
            {
                shapechange(attachee, triggerer, 10);
            }

            return SkipDefault;
        }
        public static bool shapechange(GameObjectBody attachee, GameObjectBody triggerer, int dialog_line)
        {
            var loc = attachee.GetLocation();
            attachee.Destroy();
            var kella = GameSystems.MapObject.CreateObject(14236, loc);
            if ((kella != null))
            {
                triggerer.BeginDialog(kella, dialog_line);
            }

            return SkipDefault;
        }

    }
}
