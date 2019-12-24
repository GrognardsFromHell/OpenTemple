
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
    [ObjectScript(222)]
    public class LarethdiaryBook : BaseObjectScript
    {

        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var loc = triggerer.GetLocation();

            var npc = GameSystems.MapObject.CreateObject(14413, loc);

            triggerer.BeginDialog(npc, 1);
            return SkipDefault;
        }


    }
}
