
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
    [ObjectScript(491)]
    public class Quarryportal : BaseObjectScript
    {
        public override bool OnUse(GameObjectBody door, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(972) <= 28))
            {
                var damage_dice = Dice.Parse("4d8");
                AttachParticles("Mon-EarthElem-Unconceal", triggerer);
                AttachParticles("Mon-EarthElem-body120", triggerer);
                AttachParticles("hit-BLUDGEONING-medium", triggerer);
                triggerer.FloatMesFileLine("mes/float.mes", 3);
                Sound(4042, 1);
                if ((triggerer.ReflexSaveAndDamage(null, 20, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, damage_dice, DamageType.Bludgeoning, D20AttackPower.UNSPECIFIED, D20ActionType.UNSPECIFIED_MOVE, D20AttackPower.NORMAL)))
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
}
