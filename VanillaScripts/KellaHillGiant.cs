
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
