
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

[ObjectScript(491)]
public class Quarryportal : BaseObjectScript
{
    public override bool OnUse(GameObject door, GameObject triggerer)
    {
        if ((GetGlobalVar(972) <= 28))
        {
            var damage_dice = Dice.Parse("4d8");
            AttachParticles("Mon-EarthElem-Unconceal", triggerer);
            AttachParticles("Mon-EarthElem-body120", triggerer);
            AttachParticles("hit-BLUDGEONING-medium", triggerer);
            triggerer.FloatMesFileLine("mes/float.mes", 3);
            Sound(4042, 1);
            if ((triggerer.ReflexSaveAndDamage(null, 20, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, damage_dice, DamageType.Bludgeoning, D20AttackPower.UNSPECIFIED, D20ActionType.UNSPECIFIED_MOVE)))
            {
                triggerer.FloatMesFileLine("mes/spell.mes", 30001);
            }
            else
            {
                triggerer.FloatMesFileLine("mes/spell.mes", 30002);
            }

            return SkipDefault;
        }
        else
        {
            return RunDefault;
        }

    }

}