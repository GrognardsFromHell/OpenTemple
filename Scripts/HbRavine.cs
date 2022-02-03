
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
    [ObjectScript(583)]
    public class HbRavine : BaseObjectScript
    {
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetNameId() == 8960))
            {
                if ((!GetGlobalFlag(554)))
                {
                    SetGlobalFlag(553, true);
                }

            }
            else if ((attachee.GetNameId() == 8966))
            {
                if ((!GetGlobalFlag(553)))
                {
                    SetGlobalFlag(554, true);
                }

            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            var webbed = Livonya.break_free(attachee, 3);
            // ready vs approach guys with explosives  #
            if ((attachee.GetNameId() == 8932 || attachee.GetNameId() == 8936 || attachee.GetNameId() == 8940 || attachee.GetNameId() == 8944 || attachee.GetNameId() == 8948 || attachee.GetNameId() == 8952 || attachee.GetNameId() == 8956))
            {
                if ((Utilities.obj_percent_hp(attachee) <= 50))
                {
                    attachee.SetInt(obj_f.critter_strategy, 547);
                }
                else if ((Utilities.obj_percent_hp(attachee) >= 51))
                {
                    attachee.SetInt(obj_f.critter_strategy, 565);
                    foreach (var obj in GameSystems.Party.PartyMembers)
                    {
                        if ((ScriptDaemon.within_rect_by_corners(obj, 423, 453, 488, 554)))
                        {
                            var damage_dice = Dice.Parse("6d6");
                            AttachParticles("hit-BLUDGEONING-medium", obj);
                            AttachParticles("hit-FIRE-burst", obj);
                            obj.FloatMesFileLine("mes/float.mes", 1);
                            if ((obj.ReflexSaveAndDamage(null, 20, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, damage_dice, DamageType.Force, D20AttackPower.UNSPECIFIED, D20ActionType.UNSPECIFIED_MOVE)))
                            {
                                obj.FloatMesFileLine("mes/spell.mes", 30001);
                            }
                            else
                            {
                                obj.FloatMesFileLine("mes/spell.mes", 30002);
                            }

                            SetGlobalFlag(872, true);
                        }

                    }

                    if ((GetGlobalFlag(872)))
                    {
                        go_boom_one_time(attachee, triggerer);
                    }

                }

            }
            // ready vs approach guys normal  #
            else if ((attachee.GetNameId() == 8931 || attachee.GetNameId() == 8933 || attachee.GetNameId() == 8934 || attachee.GetNameId() == 8935 || attachee.GetNameId() == 8937 || attachee.GetNameId() == 8938 || attachee.GetNameId() == 8939 || attachee.GetNameId() == 8946 || attachee.GetNameId() == 8941 || attachee.GetNameId() == 8942 || attachee.GetNameId() == 8943 || attachee.GetNameId() == 8945 || attachee.GetNameId() == 8946 || attachee.GetNameId() == 8947 || attachee.GetNameId() == 8949 || attachee.GetNameId() == 8950 || attachee.GetNameId() == 8951 || attachee.GetNameId() == 8953 || attachee.GetNameId() == 8954 || attachee.GetNameId() == 8955 || attachee.GetNameId() == 8967 || attachee.GetNameId() == 8958 || attachee.GetNameId() == 8969 || attachee.GetNameId() == 8970 || attachee.GetNameId() == 8971 || attachee.GetNameId() == 8972 || attachee.GetNameId() == 8973 || attachee.GetNameId() == 8974 || attachee.GetNameId() == 8975 || attachee.GetNameId() == 8976 || attachee.GetNameId() == 8977))
            {
                if ((Utilities.obj_percent_hp(attachee) <= 50))
                {
                    attachee.SetInt(obj_f.critter_strategy, 547);
                }
                else if ((Utilities.obj_percent_hp(attachee) >= 51))
                {
                    attachee.SetInt(obj_f.critter_strategy, 565);
                }

            }
            // RANGED TROOPS  #
            // dumb guys - orc bowmen  #
            else if ((attachee.GetNameId() == 8978 || attachee.GetNameId() == 8986 || attachee.GetNameId() == 8989))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    attachee.SetInt(obj_f.critter_strategy, 552);
                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    attachee.SetInt(obj_f.critter_strategy, 553);
                }

            }
            // smart guys - orc archers  #
            else if ((attachee.GetNameId() == 8979 || attachee.GetNameId() == 8983 || attachee.GetNameId() == 8984 || attachee.GetNameId() == 8985 || attachee.GetNameId() == 8991))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    attachee.SetInt(obj_f.critter_strategy, 551);
                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    attachee.SetInt(obj_f.critter_strategy, 553);
                }

            }
            // mage seekers - orc snipers  #
            else if ((attachee.GetNameId() == 8980 || attachee.GetNameId() == 8982 || attachee.GetNameId() == 8987 || attachee.GetNameId() == 8990))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    attachee.SetInt(obj_f.critter_strategy, 555);
                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    attachee.SetInt(obj_f.critter_strategy, 553);
                }

            }
            // archery seekers - nobody atm  #
            // elif (attachee.name == xxxxx):
            // leader = attachee.leader_get()
            // if (group_percent_hp(leader) >= 51):
            // attachee.obj_set_int(obj_f_critter_strategy, 554)
            // elif (group_percent_hp(leader) <= 50):
            // attachee.obj_set_int(obj_f_critter_strategy, 553)
            // spell responders - orc marksmen  #
            else if ((attachee.GetNameId() == 8981 || attachee.GetNameId() == 8988))
            {
                foreach (var obj in GameSystems.Party.PartyMembers)
                {
                    if ((obj.GetStat(Stat.level_wizard) >= 1 || obj.GetStat(Stat.level_sorcerer) >= 1 || obj.GetStat(Stat.level_druid) >= 1 || obj.GetStat(Stat.level_bard) >= 1))
                    {
                        var leader = attachee.GetLeader();
                        if ((Utilities.group_percent_hp(leader) >= 34))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 557);
                        }
                        else if ((Utilities.group_percent_hp(leader) <= 33))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 553);
                        }

                    }
                    else
                    {
                        var leader = attachee.GetLeader();
                        if ((Utilities.group_percent_hp(leader) >= 34))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 554);
                        }
                        else if ((Utilities.group_percent_hp(leader) <= 33))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 553);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetNameId() == 8931))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 574);
                    attachee.SetStandpoint(StandPointType.Day, 574);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8932))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 575);
                    attachee.SetStandpoint(StandPointType.Day, 575);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8933))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 576);
                    attachee.SetStandpoint(StandPointType.Day, 576);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8934))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 579);
                    attachee.SetStandpoint(StandPointType.Day, 579);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8935))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 580);
                    attachee.SetStandpoint(StandPointType.Day, 580);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8936))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.Unconceal();
                    attachee.SetStandpoint(StandPointType.Night, 581);
                    attachee.SetStandpoint(StandPointType.Day, 581);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8937))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.Unconceal();
                    attachee.SetStandpoint(StandPointType.Night, 582);
                    attachee.SetStandpoint(StandPointType.Day, 582);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8938))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.Unconceal();
                    attachee.SetStandpoint(StandPointType.Night, 583);
                    attachee.SetStandpoint(StandPointType.Day, 583);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8939))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.Unconceal();
                    attachee.SetStandpoint(StandPointType.Night, 584);
                    attachee.SetStandpoint(StandPointType.Day, 584);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8969))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 585);
                    attachee.SetStandpoint(StandPointType.Day, 585);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8970))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 586);
                    attachee.SetStandpoint(StandPointType.Day, 586);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8971))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 587);
                    attachee.SetStandpoint(StandPointType.Day, 587);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8959))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 588);
                    attachee.SetStandpoint(StandPointType.Day, 588);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8960))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 589);
                    attachee.SetStandpoint(StandPointType.Day, 589);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8961))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 590);
                    attachee.SetStandpoint(StandPointType.Day, 590);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8978))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 591);
                    attachee.SetStandpoint(StandPointType.Day, 591);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8979))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 592);
                    attachee.SetStandpoint(StandPointType.Day, 592);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8980))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 593);
                    attachee.SetStandpoint(StandPointType.Day, 593);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8981))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 594);
                    attachee.SetStandpoint(StandPointType.Day, 594);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8982))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 595);
                    attachee.SetStandpoint(StandPointType.Day, 595);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8983))
            {
                if ((GetGlobalFlag(554)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 596);
                    attachee.SetStandpoint(StandPointType.Day, 596);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8950))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 597);
                    attachee.SetStandpoint(StandPointType.Day, 597);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8951))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 598);
                    attachee.SetStandpoint(StandPointType.Day, 598);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8952))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 599);
                    attachee.SetStandpoint(StandPointType.Day, 599);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8953))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 600);
                    attachee.SetStandpoint(StandPointType.Day, 600);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8954))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 695);
                    attachee.SetStandpoint(StandPointType.Day, 695);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8955))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 696);
                    attachee.SetStandpoint(StandPointType.Day, 696);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8956))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 697);
                    attachee.SetStandpoint(StandPointType.Day, 697);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8957))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 698);
                    attachee.SetStandpoint(StandPointType.Day, 698);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8958))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 699);
                    attachee.SetStandpoint(StandPointType.Day, 699);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8975))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 183);
                    attachee.SetStandpoint(StandPointType.Day, 183);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8976))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 184);
                    attachee.SetStandpoint(StandPointType.Day, 184);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8977))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 887);
                    attachee.SetStandpoint(StandPointType.Day, 887);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8965))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 890);
                    attachee.SetStandpoint(StandPointType.Day, 890);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8966))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 292);
                    attachee.SetStandpoint(StandPointType.Day, 292);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8967))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 888);
                    attachee.SetStandpoint(StandPointType.Day, 888);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8968))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 889);
                    attachee.SetStandpoint(StandPointType.Day, 889);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8985))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 293);
                    attachee.SetStandpoint(StandPointType.Day, 293);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8987))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 294);
                    attachee.SetStandpoint(StandPointType.Day, 294);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8989))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 295);
                    attachee.SetStandpoint(StandPointType.Day, 295);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8991))
            {
                if ((GetGlobalFlag(553)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 296);
                    attachee.SetStandpoint(StandPointType.Day, 296);
                    DetachScript();
                }

            }

            return RunDefault;
        }
        public static void go_boom_one_time(GameObject attachee, GameObject triggerer)
        {
            Sound(4170, 1);
            SetGlobalFlag(872, false);
            return;
        }

    }
}
