
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts;

[ObjectScript(193)]
public class MurderousThief : BaseObjectScript
{

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        triggerer.BeginDialog(attachee, 1);
        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        GameSystems.MapObject.CreateObject(14324, attachee.GetLocation());
        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(triggerer);
        attachee.FloatLine(1, triggerer);
        attachee.SetCritterFlag(CritterFlag.MUTE);
        return RunDefault;
    }
    public override bool OnExitCombat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetStat(Stat.subdual_damage) >= attachee.GetStat(Stat.hp_current)))
        {
            GameSystems.MapObject.CreateObject(14324, attachee.GetLocation());
        }

        return RunDefault;
    }


}