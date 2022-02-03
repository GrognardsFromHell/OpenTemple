
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

namespace Scripts;

[ObjectScript(801)]
public class SpawnerMap2HommletExterior : BaseObjectScript
{
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
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