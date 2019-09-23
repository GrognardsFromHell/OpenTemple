
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
    [ObjectScript(218)]
    public class GoldenSkull : BaseObjectScript
    {
        public override bool OnInsertItem(GameObjectBody attachee, GameObjectBody triggerer)
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
