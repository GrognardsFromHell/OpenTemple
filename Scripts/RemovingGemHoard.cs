
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
    [ObjectScript(260)]
    public class RemovingGemHoard : BaseObjectScript
    {
        public override bool OnRemoveItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (PartyLeader.GetMap() == 5079 && triggerer.GetNameId() == 1050) // Zuggtmoy level, taking from throne of gems
            {
                var zuggtmoy = Utilities.find_npc_near(triggerer, 8064);
                var loc = triggerer.GetLocation();
                var rot = triggerer.Rotation;
                triggerer.Destroy();
                var empty_throne = GameSystems.MapObject.CreateObject(1051, loc);
                empty_throne.Rotation = rot;
                UiSystems.CharSheet.Hide();
                if (((zuggtmoy != null) && (SelectedPartyLeader != null)))
                {
                    if ((GetGlobalFlag(181)))
                    {
                        SetGlobalFlag(181, false);
                        Zuggtmoy.transform_into_demon_form(zuggtmoy, SelectedPartyLeader, 320);
                    }
                    else
                    {
                        SelectedPartyLeader.BeginDialog(zuggtmoy, 320);
                    }

                }

                DetachScript();
                return RunDefault;
            }
            else
            {
                return SkipDefault;
            }

        }

    }
}
