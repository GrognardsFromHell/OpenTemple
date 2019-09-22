
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
    [ObjectScript(32004)]
    public class Trap3PoisonNeedle : BaseObjectScript
    {

        public override bool OnTrap(TrapSprungEvent trap, GameObjectBody triggerer)
        {
            AttachParticles(trap.Type.ParticleSystemId, trap.Object);
            var result = trap.Attack(triggerer, 8, 20, false);

            if (((result & D20CAF.HIT)) != D20CAF.NONE)
            {
                if ((!triggerer.SavingThrow(13, SavingThrowType.Fortitude, D20SavingThrowFlag.POISON, trap.Object)))
                {
                    triggerer.AddCondition("Poisoned", trap.Type.Damage[0].Dice.Modifier, 0);
                }

                var d = trap.Type.Damage[1].Dice.Copy();

                if (((result & D20CAF.CRITICAL)) != D20CAF.NONE)
                {
                    d = d.WithCount(d.Count * 2);
                    d = d.WithModifier(d.Modifier * 2);
                }

                triggerer.Damage(trap.Object, trap.Type.Damage[1].Type, d);
            }

            if ((trap.Type.Id == 4))
            {
                foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((obj.DistanceTo(trap.Object) <= 10))
                    {
                        if ((obj.HasLineOfSight(trap.Object)))
                        {
                            obj.ReflexSaveAndDamage(trap.Object, 20, D20SavingThrowReduction.Half, D20SavingThrowFlag.SPELL_DESCRIPTOR_ACID, trap.Type.Damage[2].Dice, trap.Type.Damage[2].Type, D20AttackPower.NORMAL);
                        }

                    }

                }

            }

            if ((trap.Type.Id == 7))
            {
                foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((obj.DistanceTo(trap.Object) <= 10))
                    {
                        if ((obj.HasLineOfSight(trap.Object)))
                        {
                            if ((!obj.SavingThrow(15, SavingThrowType.Fortitude, D20SavingThrowFlag.POISON, trap.Object)))
                            {
                                obj.AddCondition("Poisoned", trap.Type.Damage[2].Dice.Modifier, 0);
                            }

                        }

                    }

                }

            }

            return SkipDefault;
        }


    }
}
