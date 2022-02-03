
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

[ObjectScript(367)]
public class SportEncounters : BaseObjectScript
{
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5070 || attachee.GetMap() == 5071 || attachee.GetMap() == 5072 || attachee.GetMap() == 5073 || attachee.GetMap() == 5074 || attachee.GetMap() == 5075 || attachee.GetMap() == 5076 || attachee.GetMap() == 5077))
        {
            if ((attachee.GetNameId() == 14290))
            {
                // pirates spawn brigands
                if ((GetGlobalVar(564) == 1))
                {
                    var brig = GameSystems.MapObject.CreateObject(14574, attachee.GetLocation().OffsetTiles(-6, 0));
                    brig.Rotation = RandomRange(1, 5);
                    brig.SetConcealed(true);
                    brig.Unconceal();
                    SetGlobalVar(564, 2);
                }

            }
            else if ((attachee.GetNameId() == 14173))
            {
                // bugbears spawn orcs
                if ((GetGlobalVar(564) == 1))
                {
                    var orci = GameSystems.MapObject.CreateObject(14899, attachee.GetLocation().OffsetTiles(-6, 0));
                    orci.Rotation = RandomRange(1, 5);
                    orci.SetConcealed(true);
                    orci.Unconceal();
                    SetGlobalVar(564, 2);
                }

            }
            else if ((attachee.GetNameId() == 14912))
            {
                // bugbear archers spawn orc archers
                if ((GetGlobalVar(564) == 1))
                {
                    var orci = GameSystems.MapObject.CreateObject(14467, attachee.GetLocation().OffsetTiles(-10, 0));
                    orci.Rotation = RandomRange(1, 5);
                    orci.SetConcealed(true);
                    orci.Unconceal();
                    SetGlobalVar(564, 2);
                }

            }
            else if ((attachee.GetNameId() == 14572))
            {
                // hill giants spawn ettins
                if ((GetGlobalVar(564) == 1))
                {
                    var etti = GameSystems.MapObject.CreateObject(14573, attachee.GetLocation().OffsetTiles(-6, 0));
                    etti.Rotation = RandomRange(1, 5);
                    etti.SetConcealed(true);
                    etti.Unconceal();
                    SetGlobalVar(564, 2);
                }

            }
            else if ((attachee.GetNameId() == 14686))
            {
                // bugbears spawn female bugbears
                if ((GetGlobalVar(564) == 1))
                {
                    var bugi = GameSystems.MapObject.CreateObject(14216, attachee.GetLocation().OffsetTiles(-6, 0));
                    bugi.Rotation = RandomRange(1, 5);
                    bugi.SetConcealed(true);
                    bugi.Unconceal();
                    SetGlobalVar(564, 2);
                }

            }
            else if ((attachee.GetNameId() == 14123))
            {
                // zombies spawn lacedons
                if ((GetGlobalVar(564) == 1))
                {
                    var lace = GameSystems.MapObject.CreateObject(14688, attachee.GetLocation().OffsetTiles(-6, 0));
                    lace.Rotation = RandomRange(1, 5);
                    lace.SetConcealed(true);
                    lace.Unconceal();
                    SetGlobalVar(564, 2);
                }

            }

        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        return RunDefault;
    }

}