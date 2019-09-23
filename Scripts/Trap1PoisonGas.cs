
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
    [ObjectScript(32003)]
    public class Trap1PoisonGas : BaseObjectScript
    {
        public override bool OnTrap(TrapSprungEvent trap, GameObjectBody triggerer)
        {
            // numP = 210 / (game.party_npc_size() + game.party_pc_size())
            // for obj in game.obj_list_vicinity( triggerer.location, OLC_CRITTERS ):
            // obj.stat_base_set(stat_experience, (obj.stat_level_get(stat_experience) - numP))
            AttachParticles(trap.Type.ParticleSystemId, trap.Object);
            Sound(4021, 1);
            foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_CRITTERS))
            {
                if ((obj.DistanceTo(trap.Object) <= 10))
                {
                    if ((obj.HasLineOfSight(trap.Object) || !obj.HasLineOfSight(trap.Object)))
                    {
                        foreach (var dmg in trap.Type.Damage)
                        {
                            Logger.Info("dmg type={0}", dmg.Type);
                            if ((dmg.Type == DamageType.Poison))
                            {
                                if ((!obj.SavingThrow(15, SavingThrowType.Fortitude, D20CO8_F_POISON, trap.Object)))
                                {
                                    obj.AddCondition("Poisoned", dmg.Dice.Modifier, 0);
                                }

                            }
                            else
                            {
                                obj.Damage(trap.Object, dmg.Type, dmg.Dice);
                            }

                        }

                    }

                }

            }

            DetachScript();
            return SkipDefault;
        }

    }
}
