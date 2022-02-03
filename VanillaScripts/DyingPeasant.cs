
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

namespace VanillaScripts
{
    [ObjectScript(191)]
    public class DyingPeasant : BaseObjectScript
    {

        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(28, GetGlobalVar(28) + 1);
            if ((GetGlobalVar(28) >= 5))
            {
                GameSystems.MapObject.CreateObject(14330, SelectedPartyLeader.GetLocation());
            }

            return RunDefault;
        }
        public override bool OnExitCombat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetStat(Stat.subdual_damage) >= attachee.GetStat(Stat.hp_current)))
            {
                SetGlobalVar(28, GetGlobalVar(28) + 1);
                if ((GetGlobalVar(28) >= 5))
                {
                    GameSystems.MapObject.CreateObject(14330, SelectedPartyLeader.GetLocation());
                }

            }

            return RunDefault;
        }


    }
}
