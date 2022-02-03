
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

namespace Scripts
{
    [ObjectScript(527)]
    public class UndeadWizard : BaseObjectScript
    {
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalVar(987, GetGlobalVar(987) + 1);
            if (GetGlobalVar(987) == 1)
            {
                spawn_undead_backup(attachee, triggerer);
            }
            else if (GetGlobalVar(987) == 3)
            {
                SetGlobalFlag(945, true);
                SetGlobalVar(994, 4);
            }

            return RunDefault;
        }
        public static bool spawn_undead_backup(GameObject attachee, GameObject triggerer)
        {
            Sound(4045, 1);
            var ga1 = GameSystems.MapObject.CreateObject(14135, new locXY(504, 458));
            ga1.Rotation = 2.5f;
            ga1.SetConcealed(true);
            ga1.Unconceal();
            var ga2 = GameSystems.MapObject.CreateObject(14135, new locXY(501, 458));
            ga2.Rotation = 2.5f;
            ga2.SetConcealed(true);
            ga2.Unconceal();
            var ga3 = GameSystems.MapObject.CreateObject(14135, new locXY(498, 458));
            ga3.Rotation = 2.5f;
            ga3.SetConcealed(true);
            ga3.Unconceal();
            var ga4 = GameSystems.MapObject.CreateObject(14135, new locXY(495, 458));
            ga4.Rotation = 2.5f;
            ga4.SetConcealed(true);
            ga4.Unconceal();
            var sh1 = GameSystems.MapObject.CreateObject(14596, new locXY(503, 455));
            sh1.Rotation = 2.5f;
            sh1.SetConcealed(true);
            sh1.Unconceal();
            var sh2 = GameSystems.MapObject.CreateObject(14596, new locXY(496, 455));
            sh2.Rotation = 2.5f;
            sh2.SetConcealed(true);
            sh2.Unconceal();
            var uw3 = GameSystems.MapObject.CreateObject(14597, new locXY(499, 455));
            uw3.Rotation = 2.5f;
            uw3.SetConcealed(true);
            uw3.Unconceal();
            return RunDefault;
        }

    }
}
