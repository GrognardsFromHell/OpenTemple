
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

[ObjectScript(32007)]
public class Trap8Scythe : BaseObjectScript
{

    public override bool OnTrap(TrapSprungEvent trap, GameObject triggerer)
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