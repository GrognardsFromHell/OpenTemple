
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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
    [ObjectScript(32011)]
    public class Trap12PoisonDarts : BaseObjectScript
    {

        public override bool OnTrap(TrapSprungEvent trap, GameObjectBody triggerer)
        {
            AttachParticles(trap.Type.ParticleSystemId, trap.Object);
            foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_CRITTERS))
            {
                if ((obj.DistanceTo(trap.Object) <= 10))
                {
                    if ((obj.HasLineOfSight(trap.Object)))
                    {
                        var result = trap.Attack(triggerer, 15, 20, true);

                        if (((result & D20CAF.HIT)) != D20CAF.NONE)
                        {
                            foreach (var dmg in trap.Type.Damage)
                            {
                                if ((dmg.Type == DamageType.Poison))
                                {
                                    if ((!obj.SavingThrow(11, SavingThrowType.Fortitude, D20SavingThrowFlag.POISON, trap.Object)))
                                    {
                                        obj.AddCondition("Poisoned", dmg.Dice.Modifier, 0);
                                    }

                                }
                                else
                                {
                                    var d = dmg.Dice.Copy();

                                    if (((result & D20CAF.CRITICAL)) != D20CAF.NONE)
                                    {
                                        d = d.WithCount(d.Count * 2);
                                        d = d.WithModifier(d.Modifier * 2);
                                    }

                                    obj.Damage(trap.Object, dmg.Type, d);
                                }

                            }

                        }

                    }

                }

            }

            return SkipDefault;
        }


    }
}
