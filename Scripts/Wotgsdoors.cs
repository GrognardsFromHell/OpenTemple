
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

[ObjectScript(490)]
public class Wotgsdoors : BaseObjectScript
{
    // these scripts are used to get Hommlet interior doors that normally lead to Hommlet exterior to instead lead to the custom Hommlet substitute map where WotGS is staged while WotGS is active.

    public override bool OnUse(GameObject door, GameObject triggerer)
    {
        if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6))
        {
            if ((door.GetNameId() == 1701))
            {
                // prosperous farmer
                FadeAndTeleport(0, 0, 0, 5189, 717, 486);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1702))
            {
                // modest farmhouse
                FadeAndTeleport(0, 0, 0, 5189, 763, 433);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1703))
            {
                // woodcutter
                FadeAndTeleport(0, 0, 0, 5189, 733, 381);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1704))
            {
                // well kept farm
                FadeAndTeleport(0, 0, 0, 5189, 676, 507);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1705))
            {
                // prosperous farmhouse
                FadeAndTeleport(0, 0, 0, 5189, 658, 448);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1706))
            {
                // leatherworker
                FadeAndTeleport(0, 0, 0, 5189, 634, 450);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1707))
            {
                // moneychanger 2
                FadeAndTeleport(0, 0, 0, 5189, 548, 332);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1708))
            {
                // blacksmith
                FadeAndTeleport(0, 0, 0, 5189, 562, 444);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1709))
            {
                // new building
                FadeAndTeleport(0, 0, 0, 5189, 575, 409);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1710))
            {
                // weaver
                FadeAndTeleport(0, 0, 0, 5189, 548, 409);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1711))
            {
                // tailor
                FadeAndTeleport(0, 0, 0, 5189, 539, 380);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1712))
            {
                // strapping farmer
                FadeAndTeleport(0, 0, 0, 5189, 524, 393);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1713))
            {
                // cabinet maker 2
                FadeAndTeleport(0, 0, 0, 5189, 569, 284);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1714))
            {
                // teamster
                FadeAndTeleport(0, 0, 0, 5189, 448, 321);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1715))
            {
                // moneychanger 1
                FadeAndTeleport(0, 0, 0, 5189, 559, 325);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1716))
            {
                // cabinet maker 1
                FadeAndTeleport(0, 0, 0, 5189, 572, 275);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1717))
            {
                // potter
                FadeAndTeleport(0, 0, 0, 5189, 588, 298);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1718))
            {
                // brewhouse 1
                FadeAndTeleport(0, 0, 0, 5189, 647, 324);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1719))
            {
                // modest cottage
                FadeAndTeleport(0, 0, 0, 5189, 548, 242);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1720))
            {
                // cabinet maker 3
                FadeAndTeleport(0, 0, 0, 5189, 562, 293);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1721))
            {
                // cheesemaker
                FadeAndTeleport(0, 0, 0, 5189, 445, 368);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1722))
            {
                // mill
                FadeAndTeleport(0, 0, 0, 5189, 448, 410);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1723))
            {
                // reclusive farmer
                FadeAndTeleport(0, 0, 0, 5189, 397, 413);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1724))
            {
                // the grove
                FadeAndTeleport(0, 0, 0, 5189, 621, 520);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1725))
            {
                // herdsman barn 1
                FadeAndTeleport(0, 0, 0, 5189, 592, 473);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1726))
            {
                // wainwright
                FadeAndTeleport(0, 0, 0, 5189, 558, 484);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1727))
            {
                // village elder
                FadeAndTeleport(0, 0, 0, 5189, 496, 463);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1728))
            {
                // carpenter
                FadeAndTeleport(0, 0, 0, 5189, 523, 508);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1729))
            {
                // stone mason
                FadeAndTeleport(0, 0, 0, 5189, 452, 517);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1730))
            {
                // brewhouse 2
                FadeAndTeleport(0, 0, 0, 5189, 642, 327);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1731))
            {
                // inn first
                FadeAndTeleport(0, 0, 0, 5189, 627, 401);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1732))
            {
                // traders barn 1
                FadeAndTeleport(0, 0, 0, 5189, 492, 295);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1733))
            {
                // traders barn 2
                FadeAndTeleport(0, 0, 0, 5189, 488, 309);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1734))
            {
                // traders barn 3
                FadeAndTeleport(0, 0, 0, 5189, 460, 314);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1735))
            {
                // traders store 1
                FadeAndTeleport(0, 0, 0, 5189, 519, 302);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1736))
            {
                // traders store 2
                FadeAndTeleport(0, 0, 0, 5189, 493, 309);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1737))
            {
                // traders store 3
                FadeAndTeleport(0, 0, 0, 5189, 516, 329);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1738))
            {
                // main floor
                FadeAndTeleport(0, 0, 0, 5189, 495, 231);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1739))
            {
                // main hall
                FadeAndTeleport(0, 0, 0, 5189, 449, 607);
                return SkipDefault;
            }
            else if ((door.GetNameId() == 1740))
            {
                // herdsman barn 2
                FadeAndTeleport(0, 0, 0, 5189, 577, 492);
                return SkipDefault;
            }

        }

        return RunDefault;
    }

}