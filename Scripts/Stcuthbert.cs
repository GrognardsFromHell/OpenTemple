
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
    [ObjectScript(173)]
    public class Stcuthbert : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GetGlobalFlag(328)))
            {
                // cuthbert has not talked
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5080))
            {
                attachee.RemoveFromInitiative();
                attachee.SetObjectFlag(ObjectFlag.OFF);
                AttachParticles("sp-Magic Circle against Chaos-END", attachee);
                Sound(4043, 1);
                return SkipDefault;
            }
            else
            {
                var strategy = RandomRange(453, 460);
                if ((strategy == 453))
                {
                    attachee.SetInt(obj_f.critter_strategy, 453);
                }
                else if ((strategy == 454))
                {
                    attachee.SetInt(obj_f.critter_strategy, 454);
                }
                else if ((strategy == 455))
                {
                    attachee.SetInt(obj_f.critter_strategy, 455);
                }
                else if ((strategy == 456))
                {
                    attachee.SetInt(obj_f.critter_strategy, 456);
                }
                else if ((strategy == 457))
                {
                    attachee.SetInt(obj_f.critter_strategy, 457);
                }
                else if ((strategy == 458))
                {
                    attachee.SetInt(obj_f.critter_strategy, 458);
                }
                else if ((strategy == 459))
                {
                    attachee.SetInt(obj_f.critter_strategy, 459);
                }
                else if ((strategy == 460))
                {
                    attachee.SetInt(obj_f.critter_strategy, 460);
                }

            }

            return RunDefault;
        }
        public static bool switch_to_iuz(GameObjectBody cuthbert, GameObjectBody pc, int line)
        {
            var iuz = Utilities.find_npc_near(cuthbert, 8042);
            if ((iuz != null))
            {
                pc.BeginDialog(iuz, line);
                iuz.TurnTowards(cuthbert);
                cuthbert.TurnTowards(iuz);
            }
            else
            {
                turn_off_gods(cuthbert, pc);
            }

            return SkipDefault;
        }
        public static bool cuthbert_raise_good(GameObjectBody cuthbert, GameObjectBody pc)
        {
            // raise all PC's and PC followers
            foreach (var obj in GameSystems.Party.PartyMembers)
            {
                if ((obj.GetStat(Stat.hp_current) <= -10))
                {
                    obj.Resurrect(ResurrectionType.CuthbertResurrection, 0);
                }
                else
                {
                    // obj.obj_set_int( obj_f_hp_damage, 0 )
                    var dice = Dice.Parse("1d10+1000");
                    obj.Heal(null, dice);
                    obj.HealSubdual(null, dice);
                }

            }

            AttachParticles("sp-consecrate-END", cuthbert);
            Sound(4043, 1);
            return SkipDefault;
        }
        public static bool turn_off_gods(GameObjectBody cuthbert, GameObjectBody pc)
        {
            cuthbert.RemoveFromInitiative();
            cuthbert.SetObjectFlag(ObjectFlag.OFF);
            AttachParticles("sp-Death Knell-Target", cuthbert);
            var iuz = Utilities.find_npc_near(cuthbert, 8042);
            if ((iuz != null))
            {
                iuz.RemoveFromInitiative();
                iuz.SetObjectFlag(ObjectFlag.OFF);
                AttachParticles("sp-Death Knell-Target", iuz);
            }

            Sound(4165, 1);
            return SkipDefault;
        }
        public static bool unshit(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            return RunDefault;
        }

    }
}
