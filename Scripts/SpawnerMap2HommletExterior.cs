
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
    [ObjectScript(801)]
    public class SpawnerMap2HommletExterior : BaseObjectScript
    {
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5001))
            {
                if ((RandomRange(1, 3) == 1))
                {
                    var barrel = GameSystems.MapObject.CreateObject(1049, new locXY(641, 387));
                    if ((RandomRange(1, 3) <= 2))
                    {
                        Utilities.create_item_in_inventory(5007, barrel);
                    }

                }

                if ((RandomRange(1, 3) == 1))
                {
                    var barrel = GameSystems.MapObject.CreateObject(1049, new locXY(714, 381));
                    if ((RandomRange(1, 3) <= 2))
                    {
                        Utilities.create_item_in_inventory(5007, barrel);
                    }

                }

                if ((RandomRange(1, 3) == 1))
                {
                    var barrel = GameSystems.MapObject.CreateObject(1049, new locXY(573, 424));
                    if ((RandomRange(1, 3) <= 2))
                    {
                        Utilities.create_item_in_inventory(5007, barrel);
                    }

                }

                if ((RandomRange(1, 3) == 1))
                {
                    var barrel = GameSystems.MapObject.CreateObject(1049, new locXY(617, 460));
                    if ((RandomRange(1, 3) <= 2))
                    {
                        Utilities.create_item_in_inventory(5007, barrel);
                    }

                }

                if ((RandomRange(1, 3) == 1))
                {
                    var barrel = GameSystems.MapObject.CreateObject(1049, new locXY(592, 485));
                    if ((RandomRange(1, 3) <= 2))
                    {
                        Utilities.create_item_in_inventory(5007, barrel);
                    }

                }

                if ((RandomRange(1, 3) == 1))
                {
                    var barrel = GameSystems.MapObject.CreateObject(1049, new locXY(454, 442));
                    if ((RandomRange(1, 3) <= 2))
                    {
                        Utilities.create_item_in_inventory(5007, barrel);
                    }

                }

                if ((RandomRange(1, 3) == 1))
                {
                    var barrel = GameSystems.MapObject.CreateObject(1049, new locXY(472, 313));
                    if ((RandomRange(1, 3) <= 2))
                    {
                        Utilities.create_item_in_inventory(5007, barrel);
                    }

                }

                if ((RandomRange(1, 3) == 1))
                {
                    var barrel = GameSystems.MapObject.CreateObject(1049, new locXY(418, 378));
                    if ((RandomRange(1, 3) <= 2))
                    {
                        Utilities.create_item_in_inventory(5007, barrel);
                    }

                }

                if ((RandomRange(1, 3) == 1))
                {
                    var barrel = GameSystems.MapObject.CreateObject(1049, new locXY(364, 406));
                    if ((RandomRange(1, 3) <= 2))
                    {
                        Utilities.create_item_in_inventory(5007, barrel);
                    }

                }

                SetGlobalVar(766, RandomRange(1, 20));
                attachee.Destroy();
            }

            return RunDefault;
        }

    }
}
