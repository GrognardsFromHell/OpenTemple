
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
    [ObjectScript(549)]
    public class Letter : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(818)))
            {
                triggerer.BeginDialog(attachee, 30);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var loc = triggerer.GetLocation();
            var npc = GameSystems.MapObject.CreateObject(14678, loc);
            if ((attachee.GetMap() == 5001 || attachee.GetMap() == 5002 || attachee.GetMap() == 5051 || attachee.GetMap() == 5062 || attachee.GetMap() == 5094 || attachee.GetMap() == 5068 || attachee.GetMap() == 5093 || attachee.GetMap() == 5091 || attachee.GetMap() == 5069 || attachee.GetMap() == 5112 || attachee.GetMap() == 5113 || attachee.GetMap() == 5121 || attachee.GetMap() == 5132 || attachee.GetMap() == 5108 || attachee.GetMap() == 5095 || attachee.GetMap() == 5070 || attachee.GetMap() == 5071 || attachee.GetMap() == 5072 || attachee.GetMap() == 5073 || attachee.GetMap() == 5074 || attachee.GetMap() == 5075 || attachee.GetMap() == 5076 || attachee.GetMap() == 5077 || attachee.GetMap() == 5078))
            {
                triggerer.BeginDialog(npc, 1);
            }
            else
            {
                triggerer.BeginDialog(npc, 30);
            }

            return SkipDefault;
        }

    }
}
