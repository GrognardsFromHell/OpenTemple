
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
    [ObjectScript(32006)]
    public class Trap6FireTrap : BaseObjectScript
    {
        public override bool OnTrap(TrapSprungEvent trap, GameObject triggerer)
        {
            // print "Trap sprung"
            if ((trap.Type.Id == 16))
            {
                // print "ID 16"
                // numP = 210 / (game.party_npc_size() + game.party_pc_size())
                // for obj in game.obj_list_vicinity( triggerer.location, OLC_CRITTERS ):
                // obj.stat_base_set(stat_experience, (obj.stat_level_get(stat_experience) - numP))
                AttachParticles(trap.Type.ParticleSystemId, trap.Object);
                AttachParticles("sp-Fireball-Hit", trap.Object);
                Sound(4024, 1);
                foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((obj.DistanceTo(trap.Object) <= 20))
                    {
                        if ((obj.HasLineOfSight(trap.Object) || !obj.HasLineOfSight(trap.Object)))
                        {
                            foreach (var dmg in trap.Type.Damage)
                            {
                                if ((dmg.Type == DamageType.Poison))
                                {
                                    if ((!obj.SavingThrow(21, SavingThrowType.Fortitude, D20SavingThrowFlag.POISON, trap.Object)))
                                    {
                                        obj.AddCondition("Poisoned", dmg.Dice.Modifier, 0);
                                    }

                                }
                                else
                                {
                                    obj.ReflexSaveAndDamage(trap.Object, 21, D20SavingThrowReduction.Half, D20SavingThrowFlag.SPELL_DESCRIPTOR_FIRE, dmg.Dice, dmg.Type, D20AttackPower.NORMAL);
                                }

                            }

                        }

                    }

                }

            }

            if ((triggerer.GetMap() == 5067))
            {
                GetGlobalFlag(874);
            }

            if ((trap.Type.Id == 6))
            {
                // print "ID 6"
                // numP = 210 / (game.party_npc_size() + game.party_pc_size())
                // for obj in game.obj_list_vicinity( triggerer.location, OLC_CRITTERS ):
                // obj.stat_base_set(stat_experience, (obj.stat_level_get(stat_experience) - numP))
                AttachParticles(trap.Type.ParticleSystemId, trap.Object);
                Sound(4024, 1);
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
                                    obj.ReflexSaveAndDamage(trap.Object, 16, D20SavingThrowReduction.Half, D20SavingThrowFlag.SPELL_DESCRIPTOR_FIRE, dmg.Dice, dmg.Type, D20AttackPower.NORMAL);
                                }

                            }

                        }

                    }

                }

            }

            // print "newsid = 0"
            // game.new_sid = 0
            Logger.Info("Trap script ID changed to 0");
            trap.Object.RemoveScript(ObjScriptEvent.Trap); // fixes re-arming when doing disable device
            return SkipDefault;
        }

    }
}
