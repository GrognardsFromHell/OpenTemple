
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
    [ObjectScript(32007)]
    public class Trap8Scythe : BaseObjectScript
    {
        public override bool OnTrap(TrapSprungEvent trap, GameObject triggerer)
        {
            // numP = 210 / (game.party_npc_size() + game.party_pc_size())
            // for obj in game.obj_list_vicinity( triggerer.location, OLC_CRITTERS ):
            // obj.stat_base_set(stat_experience, (obj.stat_level_get(stat_experience) - numP))
            AttachParticles(trap.Type.ParticleSystemId, trap.Object);
            Sound(4025, 1);
            var result = trap.Attack(triggerer, 8, 20, false);
            if (((result & D20CAF.HIT)) != D20CAF.NONE)
            {
                foreach (var dmg in trap.Type.Damage)
                {
                    Logger.Info("dmg type={0}", dmg.Type);
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

            DetachScript();
            return SkipDefault;
        }

    }
}
