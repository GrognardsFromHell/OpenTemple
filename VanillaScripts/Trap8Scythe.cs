
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(32007)]
    public class Trap8Scythe : BaseObjectScript
    {

        public override bool OnTrap(TrapSprungEvent trap, GameObjectBody triggerer)
        {
            AttachParticles(trap.Type.ParticleSystemId, trap.Object);
            var result = trap.Attack(triggerer, 8, 20, false);

            if (((result & D20CAF.HIT)) != D20CAF.NONE)
            {
                foreach (var dmg in trap.Type.Damage)
                {
                    if ((dmg.Type == DamageType.Poison))
                    {
                        if ((!triggerer.SavingThrow(13, SavingThrowType.Fortitude, D20SavingThrowFlag.POISON, trap.Object)))
                        {
                            triggerer.AddCondition("Poisoned", dmg.Dice.Modifier, 0);
                        }

                    }
                    else
                    {
                        var d = dmg.Dice.Copy();

                        if (((result & D20CAF.CRITICAL)) != D20CAF.NONE)
                        {
                            d = d.WithCount(d.Count * 3);
                            d = d.WithModifier(d.Modifier * 3);
                        }

                        triggerer.Damage(trap.Object, dmg.Type, d);
                    }

                }

            }

            return SkipDefault;
        }


    }
}
