
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
    [ObjectScript(197)]
    public class Map : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public static bool zap(GameObject attachee, GameObject triggerer)
        {
            var damage_dice = Dice.Parse("5d4");
            AttachParticles("sp-Shocking Grasp", triggerer);
            if (triggerer.ReflexSaveAndDamage(null, 20, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, damage_dice, DamageType.Electricity, D20AttackPower.UNSPECIFIED, D20ActionType.UNSPECIFIED_MOVE))
            {
                // saving throw successful
                triggerer.FloatMesFileLine("mes/spell.mes", 30001);
            }
            else
            {
                // saving throw unsuccessful
                triggerer.FloatMesFileLine("mes/spell.mes", 30002);
            }

            return RunDefault;
        }

    }
}
