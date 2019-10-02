
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
    [ObjectScript(32014)]
    public class Trap15GlyphSonic : BaseObjectScript
    {
        public override bool OnTrap(TrapSprungEvent trap, GameObjectBody triggerer)
        {
            // numP = 210 / (game.party_npc_size() + game.party_pc_size())
            // for obj in game.obj_list_vicinity( triggerer.location, OLC_CRITTERS ):
            // obj.stat_base_set(stat_experience, (obj.stat_level_get(stat_experience) - numP))
            AttachParticles(trap.Type.ParticleSystemId, trap.Object);
            Sound(4029, 1);
            foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_CRITTERS))
            {
                if ((obj.DistanceTo(trap.Object) <= 5))
                {
                    if ((obj.HasLineOfSight(trap.Object)))
                    {
                        foreach (var dmg in trap.Type.Damage)
                        {
                            if ((dmg.Type == DamageType.Poison))
                            {
                                if ((!obj.SavingThrow(15, SavingThrowType.Fortitude, D20SavingThrowFlag.POISON, trap.Object)))
                                {
                                    obj.AddCondition("Poisoned", dmg.Dice.Modifier, 0);
                                }

                            }
                            else
                            {
                                obj.ReflexSaveAndDamage(trap.Object, 15, D20SavingThrowReduction.Half, D20SavingThrowFlag.SPELL_DESCRIPTOR_SONIC, dmg.Dice, dmg.Type, D20AttackPower.NORMAL);
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