
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
    [ObjectScript(11)]
    public class FarmerWife : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetNameId() != 20019))
            {
                DetachScript();
                return SkipDefault;
            }

            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() != 5080 || GetGlobalFlag(813)))
            {
                attachee.SetCritterFlag(CritterFlag.MUTE);
            }

            DetachScript();
            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            if ((attachee.GetStat(Stat.subdual_damage) >= 48 && !GetGlobalFlag(813) && attachee.GetMap() == 5080 && attachee.GetStat(Stat.hp_current) >= 1 && !GameSystems.Combat.IsCombatActive()))
            {
                // dice = dice_new("1d1+9")
                // attachee.healsubdual( OBJ_HANDLE_NULL, dice )
                foreach (var target in PartyLeader.GetPartyMembers())
                {
                    if ((attachee.GetNameId() == 14262))
                    {
                        if ((attachee.DistanceTo(target) <= 30 && target.type == ObjectType.pc))
                        {
                            if ((target.GetSkillLevel(attachee, SkillId.intimidate) >= 10 || target.GetSkillLevel(attachee, SkillId.diplomacy) >= 12 || target.GetSkillLevel(attachee, SkillId.bluff) >= 10))
                            {
                                DetachScript();
                                var dice = Dice.Parse("1d1+9");
                                attachee.HealSubdual(null, dice);
                                attachee.AIRemoveFromShitlist(target);
                                target.BeginDialog(attachee, 1000);
                                return SkipDefault;
                            }
                            else
                            {
                                DetachScript();
                                var dice = Dice.Parse("1d1+9");
                                attachee.HealSubdual(null, dice);
                                attachee.AIRemoveFromShitlist(target);
                                target.BeginDialog(attachee, 2000);
                                return SkipDefault;
                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnExitCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetStat(Stat.subdual_damage) >= 48 && !GetGlobalFlag(813) && attachee.GetMap() == 5080 && attachee.GetStat(Stat.hp_current) >= 1 && !GameSystems.Combat.IsCombatActive()))
            {
                // dice = dice_new("1d1+9")
                // attachee.healsubdual( OBJ_HANDLE_NULL, dice )
                foreach (var target in PartyLeader.GetPartyMembers())
                {
                    if ((attachee.GetNameId() == 14262))
                    {
                        if ((attachee.DistanceTo(target) <= 30 && target.type == ObjectType.pc))
                        {
                            if ((target.GetSkillLevel(attachee, SkillId.intimidate) >= 10 || target.GetSkillLevel(attachee, SkillId.diplomacy) >= 12 || target.GetSkillLevel(attachee, SkillId.bluff) >= 10))
                            {
                                DetachScript();
                                var dice = Dice.Parse("1d1+9");
                                attachee.HealSubdual(null, dice);
                                attachee.AIRemoveFromShitlist(target);
                                target.BeginDialog(attachee, 1000);
                                return SkipDefault;
                            }
                            else
                            {
                                DetachScript();
                                var dice = Dice.Parse("1d1+9");
                                attachee.HealSubdual(null, dice);
                                attachee.AIRemoveFromShitlist(target);
                                target.BeginDialog(attachee, 2000);
                                return SkipDefault;
                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // for pc in game.party:
            // attachee.ai_shitlist_remove( pc )
            // attachee.reaction_set( pc, 50 )
            attachee.RunOff();
            Co8.Timed_Destroy(attachee, 5000);
            return RunDefault;
        }

    }
}
