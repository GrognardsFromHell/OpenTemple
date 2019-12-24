
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
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(32004)]
    public class Trap3PoisonNeedle : BaseObjectScript
    {
        public override bool OnTrap(TrapSprungEvent trap, GameObjectBody triggerer)
        {
            if ((trap.Type.Id == 2))
            {
                // numP = 210 / (game.party_npc_size() + game.party_pc_size())
                // for obj in game.obj_list_vicinity( triggerer.location, OLC_CRITTERS ):
                // obj.stat_base_set(stat_experience, (obj.stat_level_get(stat_experience) - numP))
                AttachParticles(trap.Type.ParticleSystemId, trap.Object);
                Sound(4023, 1);
                var result = trap.Attack(triggerer, 10, 20, false);
                if (((result & D20CAF.HIT)) != D20CAF.NONE)
                {
                    if ((!triggerer.SavingThrow(15, SavingThrowType.Fortitude, D20SavingThrowFlag.POISON, trap.Object)))
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

                foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((obj.DistanceTo(trap.Object) <= 15))
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

                DetachScript();
            }

            if ((trap.Type.Id == 3))
            {
                // numP = 210 / (game.party_npc_size() + game.party_pc_size())
                // for obj in game.obj_list_vicinity( triggerer.location, OLC_CRITTERS ):
                // obj.stat_base_set(stat_experience, (obj.stat_level_get(stat_experience) - numP))
                AttachParticles(trap.Type.ParticleSystemId, trap.Object);
                Sound(4023, 1);
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

                DetachScript();
            }

            if ((trap.Type.Id == 4))
            {
                // numP = 210 / (game.party_npc_size() + game.party_pc_size())
                // for obj in game.obj_list_vicinity( triggerer.location, OLC_CRITTERS ):
                // obj.stat_base_set(stat_experience, (obj.stat_level_get(stat_experience) - numP))
                AttachParticles(trap.Type.ParticleSystemId, trap.Object);
                Sound(4023, 1);
                var result = trap.Attack(triggerer, 11, 20, false);
                if (((result & D20CAF.HIT)) != D20CAF.NONE)
                {
                    if ((!triggerer.SavingThrow(16, SavingThrowType.Fortitude, D20SavingThrowFlag.POISON, trap.Object)))
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

                foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((obj.DistanceTo(trap.Object) <= 15))
                    {
                        if ((obj.HasLineOfSight(trap.Object)))
                        {
                            obj.ReflexSaveAndDamage(trap.Object, 20, D20SavingThrowReduction.Half, D20SavingThrowFlag.SPELL_DESCRIPTOR_ACID, trap.Type.Damage[2].Dice, trap.Type.Damage[2].Type, D20AttackPower.NORMAL);
                        }

                    }

                }

                DetachScript();
            }

            if ((trap.Type.Id == 7))
            {
                // numP = 210 / (game.party_npc_size() + game.party_pc_size())
                // for obj in game.obj_list_vicinity( triggerer.location, OLC_CRITTERS ):
                // obj.stat_base_set(stat_experience, (obj.stat_level_get(stat_experience) - numP))
                AttachParticles(trap.Type.ParticleSystemId, trap.Object);
                Sound(4023, 1);
                var result = trap.Attack(triggerer, 13, 20, false);
                if (((result & D20CAF.HIT)) != D20CAF.NONE)
                {
                    if ((!triggerer.SavingThrow(18, SavingThrowType.Fortitude, D20SavingThrowFlag.POISON, trap.Object)))
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

                foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((obj.DistanceTo(trap.Object) <= 10))
                    {
                        if ((obj.HasLineOfSight(trap.Object)))
                        {
                            if ((!obj.SavingThrow(18, SavingThrowType.Fortitude, D20SavingThrowFlag.POISON, trap.Object)))
                            {
                                obj.AddCondition("Poisoned", trap.Type.Damage[2].Dice.Modifier, 0);
                            }

                        }

                    }

                }

                DetachScript();
            }

            // code to retain TRAP!!!
            if ((trap.Type.Id == 11 && triggerer.GetMap() == 5080))
            {
                // numP = 210 / (game.party_npc_size() + game.party_pc_size())
                // for obj in game.obj_list_vicinity( triggerer.location, OLC_CRITTERS ):
                // obj.stat_base_set(stat_experience, (obj.stat_level_get(stat_experience) - numP))
                LocAndOffsets loct = LocAndOffsets.Zero;
                foreach (var chest in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_CONTAINER))
                {
                    if ((chest.GetNameId() == 1055 && chest.DistanceTo(trap.Object) <= 5))
                    {
                        var loc1 = new locXY(484, 566);
                        var loc2 = new locXY(476, 582);
                        var loc = chest.GetLocation();
                        loct = trap.Object.GetLocationFull();
                        if ((loc1 >= loc))
                        {
                            chest.Destroy();
                            var item = GameSystems.MapObject.CreateObject(1055, new locXY(484, 566));
                        }

                        if ((loc2 >= loc && loc1 <= loc))
                        {
                            chest.Destroy();
                            var item = GameSystems.MapObject.CreateObject(1055, new locXY(476, 582));
                        }

                    }

                }

                if (loct != LocAndOffsets.Zero)
                {
                    var npc = GameSystems.MapObject.CreateObject(14605, loct);
                    triggerer.BeginDialog(npc, 1000);
                }
            }

            return SkipDefault;
        }

    }
}
