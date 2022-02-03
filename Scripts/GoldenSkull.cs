
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
    [ObjectScript(218)]
    public class GoldenSkull : BaseObjectScript
    {
        public override bool OnInsertItem(GameObject attachee, GameObject triggerer)
        {
            if (((triggerer.type == ObjectType.pc) && (!GetGlobalFlag(341))))
            {
                SetGlobalFlag(341, true);
                UiSystems.CharSheet.Hide();
                Fade(2, 0, 303, 0);
                var burne = Utilities.find_npc_near(triggerer, 8054);
                if ((burne != null))
                {
                    triggerer.BeginDialog(burne, 470);
                }

            }

            var gem = triggerer.FindItemByName(5808);
            if ((gem != null))
            {
                triggerer.D20SendSignal(D20DispatcherKey.SIG_Golden_Skull_Combine, gem);
                gem.Destroy();
            }

            gem = triggerer.FindItemByName(5809);
            if ((gem != null))
            {
                triggerer.D20SendSignal(D20DispatcherKey.SIG_Golden_Skull_Combine, gem);
                gem.Destroy();
            }

            gem = triggerer.FindItemByName(5810);
            if ((gem != null))
            {
                triggerer.D20SendSignal(D20DispatcherKey.SIG_Golden_Skull_Combine, gem);
                gem.Destroy();
            }

            gem = triggerer.FindItemByName(5811);
            if ((gem != null))
            {
                triggerer.D20SendSignal(D20DispatcherKey.SIG_Golden_Skull_Combine, gem);
                gem.Destroy();
            }

            return SkipDefault;
        }

    }
}
