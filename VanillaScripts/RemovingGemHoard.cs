
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
    [ObjectScript(260)]
    public class RemovingGemHoard : BaseObjectScript
    {

        public override bool OnRemoveItem(GameObjectBody attachee, GameObjectBody triggerer)
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


    }
}
