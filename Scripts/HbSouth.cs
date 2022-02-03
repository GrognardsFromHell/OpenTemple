
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
    [ObjectScript(582)]
    public class HbSouth : BaseObjectScript
    {
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetNameId() == 8911))
            {
                if ((!GetGlobalFlag(552)))
                {
                    SetGlobalFlag(551, true);
                }

            }
            else if ((attachee.GetNameId() == 8894))
            {
                if ((!GetGlobalFlag(551)))
                {
                    SetGlobalFlag(552, true);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            destroy_gear(attachee, triggerer);
            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            var webbed = Livonya.break_free(attachee, 3);
            // MELEE TROOPS  #
            // dumb guys - orc rundors, gnoll troops, ettin troops, stone giant troops, hill giant troops  #
            if ((attachee.GetNameId() == 8896 || attachee.GetNameId() == 8897 || attachee.GetNameId() == 8898 || attachee.GetNameId() == 8904 || attachee.GetNameId() == 8905 || attachee.GetNameId() == 8906 || attachee.GetNameId() == 8907 || attachee.GetNameId() == 8908 || attachee.GetNameId() == 8619 || attachee.GetNameId() == 8620 || attachee.GetNameId() == 8621))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 545);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 547);
                        }

                    }

                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 545);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 546);
                        }

                    }

                }

            }
            // dumb guys with rage - bugbear troops, orc fighters, ogre troops  #
            else if ((attachee.GetNameId() == 8911 || attachee.GetNameId() == 8614 || attachee.GetNameId() == 8615 || attachee.GetNameId() == 8618))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 559);
                        }

                    }

                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 561);
                        }

                    }

                }

            }
            // smart guys - kallop, boonthag, naig lliht  #
            else if ((attachee.GetNameId() == 8815 || attachee.GetNameId() == 8816 || attachee.GetNameId() == 8813))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 545);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 550);
                        }

                    }

                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 545);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 546);
                        }

                    }

                }

            }
            // smart guys with rage - hungous, orc trainer, ergo  #
            else if ((attachee.GetNameId() == 8803 || attachee.GetNameId() == 8922 || attachee.GetNameId() == 8814 || attachee.GetNameId() == 8617))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 558);
                        }

                    }

                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 561);
                        }

                    }

                }

            }
            // mage seekers - krunch  #
            else if ((attachee.GetNameId() == 8802))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 545);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 548);
                        }

                    }

                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 545);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 546);
                        }

                    }

                }

            }
            // mage seekers with rage - orc murderer, orc dominator  #
            else if ((attachee.GetNameId() == 8895))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 562);
                        }

                    }

                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 561);
                        }

                    }

                }

            }
            // archery seekers - nobody atm  #
            // elif (attachee.name == xxxxx or attachee.name == xxxxx):
            // leader = attachee.leader_get()
            // if (group_percent_hp(leader) >= 51):
            // for obj in game.party[0].group_list():
            // if obj.d20_query(Q_Prone):
            // attachee.obj_set_int(obj_f_critter_strategy, 545)
            // else:
            // attachee.obj_set_int(obj_f_critter_strategy, 549)
            // elif (group_percent_hp(leader) <= 50):
            // for obj in game.party[0].group_list():
            // if obj.d20_query(Q_Prone):
            // attachee.obj_set_int(obj_f_critter_strategy, 545)
            // else:
            // attachee.obj_set_int(obj_f_critter_strategy, 546)
            // archery seekers with rage - orc sergeant, ruff  #
            else if ((attachee.GetNameId() == 8817 || attachee.GetNameId() == 8894))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 563);
                        }

                    }

                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 561);
                        }

                    }

                }

            }
            // flankers - nobody atm  #
            // elif (attachee.name == xxxxx):
            // leader = attachee.leader_get()
            // if (group_percent_hp(leader) >= 51):
            // for obj in game.party[0].group_list():
            // if obj.d20_query(Q_Prone):
            // attachee.obj_set_int(obj_f_critter_strategy, 545)
            // else:
            // attachee.obj_set_int(obj_f_critter_strategy, 556)
            // elif (group_percent_hp(leader) <= 50):
            // for obj in game.party[0].group_list():
            // if obj.d20_query(Q_Prone):
            // attachee.obj_set_int(obj_f_critter_strategy, 545)
            // else:
            // attachee.obj_set_int(obj_f_critter_strategy, 553)
            // flankers with rage - orc assassin  #
            else if ((attachee.GetNameId() == 8616))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 564);
                        }

                    }

                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 561);
                        }

                    }

                }

            }
            // concealed guys - ettins  #
            else if ((attachee.GetNameId() == 8912 || attachee.GetNameId() == 8913 || attachee.GetNameId() == 8914 || attachee.GetNameId() == 8915 || attachee.GetNameId() == 8916))
            {
                if ((Utilities.obj_percent_hp(attachee) <= 75))
                {
                    attachee.SetInt(obj_f.critter_strategy, 547);
                }
                else if ((Utilities.obj_percent_hp(attachee) >= 76))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((is_close(attachee, obj)))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 547);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 565);
                        }

                    }

                }

            }
            // RANGED TROOPS  #
            // dumb guys - orc bowmen  #
            else if ((attachee.GetNameId() == 8899 || attachee.GetNameId() == 8900 || attachee.GetNameId() == 8917 || attachee.GetNameId() == 8918))
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
            else if ((attachee.GetNameId() == 8901 || attachee.GetNameId() == 8902 || attachee.GetNameId() == 8919))
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
            else if ((attachee.GetNameId() == 8903))
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
            // elif (attachee.name == 14749):
            // for obj in game.party[0].group_list():
            // if (obj.stat_level_get(stat_level_wizard) >= 1 or obj.stat_level_get(stat_level_sorcerer) >= 1 or obj.stat_level_get(stat_level_druid) >= 1 or obj.stat_level_get(stat_level_bard) >= 1):
            // leader = attachee.leader_get()
            // if (group_percent_hp(leader) >= 34):
            // attachee.obj_set_int(obj_f_critter_strategy, 557)
            // elif (group_percent_hp(leader) <= 33):
            // attachee.obj_set_int(obj_f_critter_strategy, 553)
            // else:
            // leader = attachee.leader_get()
            // if (group_percent_hp(leader) >= 34):
            // attachee.obj_set_int(obj_f_critter_strategy, 554)
            // elif (group_percent_hp(leader) <= 33):
            // attachee.obj_set_int(obj_f_critter_strategy, 553)
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetNameId() == 8894))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
                    attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                    attachee.SetStandpoint(StandPointType.Night, 601);
                    attachee.SetStandpoint(StandPointType.Day, 601);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8895))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 602);
                    attachee.SetStandpoint(StandPointType.Day, 602);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8896))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 603);
                    attachee.SetStandpoint(StandPointType.Day, 603);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8897))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 604);
                    attachee.SetStandpoint(StandPointType.Day, 604);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8898))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 605);
                    attachee.SetStandpoint(StandPointType.Day, 605);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8899))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.Unconceal();
                    attachee.SetStandpoint(StandPointType.Night, 606);
                    attachee.SetStandpoint(StandPointType.Day, 606);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8900))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.Unconceal();
                    attachee.SetStandpoint(StandPointType.Night, 607);
                    attachee.SetStandpoint(StandPointType.Day, 607);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8901))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.Unconceal();
                    attachee.SetStandpoint(StandPointType.Night, 608);
                    attachee.SetStandpoint(StandPointType.Day, 608);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8902))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.Unconceal();
                    attachee.SetStandpoint(StandPointType.Night, 609);
                    attachee.SetStandpoint(StandPointType.Day, 609);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8903))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 610);
                    attachee.SetStandpoint(StandPointType.Day, 610);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8904))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 611);
                    attachee.SetStandpoint(StandPointType.Day, 611);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8905))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 612);
                    attachee.SetStandpoint(StandPointType.Day, 612);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8906))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 613);
                    attachee.SetStandpoint(StandPointType.Day, 613);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8907))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 614);
                    attachee.SetStandpoint(StandPointType.Day, 614);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8908))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 615);
                    attachee.SetStandpoint(StandPointType.Day, 615);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8909))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 616);
                    attachee.SetStandpoint(StandPointType.Day, 616);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8910))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 617);
                    attachee.SetStandpoint(StandPointType.Day, 617);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8911))
            {
                if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 618);
                    attachee.SetStandpoint(StandPointType.Day, 618);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8912))
            {
                if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 619);
                    attachee.SetStandpoint(StandPointType.Day, 619);
                    attachee.SetInt(obj_f.critter_strategy, 547);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8913))
            {
                if ((GetGlobalFlag(552)))
                {
                    attachee.Unconceal();
                    attachee.SetStandpoint(StandPointType.Night, 620);
                    attachee.SetStandpoint(StandPointType.Day, 620);
                    attachee.SetInt(obj_f.critter_strategy, 547);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8914))
            {
                if ((GetGlobalFlag(552)))
                {
                    attachee.Unconceal();
                    attachee.SetStandpoint(StandPointType.Night, 621);
                    attachee.SetStandpoint(StandPointType.Day, 621);
                    attachee.SetInt(obj_f.critter_strategy, 547);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8915))
            {
                if ((GetGlobalFlag(552)))
                {
                    attachee.Unconceal();
                    attachee.SetStandpoint(StandPointType.Night, 622);
                    attachee.SetStandpoint(StandPointType.Day, 622);
                    attachee.SetInt(obj_f.critter_strategy, 547);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8916))
            {
                if ((GetGlobalFlag(552)))
                {
                    attachee.Unconceal();
                    attachee.SetStandpoint(StandPointType.Night, 623);
                    attachee.SetStandpoint(StandPointType.Day, 623);
                    attachee.SetInt(obj_f.critter_strategy, 547);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8917))
            {
                if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 624);
                    attachee.SetStandpoint(StandPointType.Day, 624);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8918))
            {
                if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 625);
                    attachee.SetStandpoint(StandPointType.Day, 625);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8919))
            {
                if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 626);
                    attachee.SetStandpoint(StandPointType.Day, 626);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8920))
            {
                if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 627);
                    attachee.SetStandpoint(StandPointType.Day, 627);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8921))
            {
                if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 628);
                    attachee.SetStandpoint(StandPointType.Day, 628);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8922))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 630);
                    attachee.SetStandpoint(StandPointType.Day, 630);
                    DetachScript();
                }
                else if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 644);
                    attachee.SetStandpoint(StandPointType.Day, 644);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8923))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 631);
                    attachee.SetStandpoint(StandPointType.Day, 631);
                    DetachScript();
                }
                else if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 645);
                    attachee.SetStandpoint(StandPointType.Day, 645);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8924))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 632);
                    attachee.SetStandpoint(StandPointType.Day, 632);
                    DetachScript();
                }
                else if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 646);
                    attachee.SetStandpoint(StandPointType.Day, 646);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8925))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 633);
                    attachee.SetStandpoint(StandPointType.Day, 633);
                    DetachScript();
                }
                else if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 647);
                    attachee.SetStandpoint(StandPointType.Day, 647);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8926))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 634);
                    attachee.SetStandpoint(StandPointType.Day, 634);
                    DetachScript();
                }
                else if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 648);
                    attachee.SetStandpoint(StandPointType.Day, 648);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8927))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 635);
                    attachee.SetStandpoint(StandPointType.Day, 635);
                    DetachScript();
                }
                else if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 649);
                    attachee.SetStandpoint(StandPointType.Day, 649);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8928))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 636);
                    attachee.SetStandpoint(StandPointType.Day, 636);
                    DetachScript();
                }
                else if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 571);
                    attachee.SetStandpoint(StandPointType.Day, 571);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8929))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 642);
                    attachee.SetStandpoint(StandPointType.Day, 642);
                    DetachScript();
                }
                else if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 572);
                    attachee.SetStandpoint(StandPointType.Day, 572);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8930))
            {
                if ((GetGlobalFlag(551)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 643);
                    attachee.SetStandpoint(StandPointType.Day, 643);
                    DetachScript();
                }
                else if ((GetGlobalFlag(552)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 573);
                    attachee.SetStandpoint(StandPointType.Day, 573);
                    DetachScript();
                }

            }

            return RunDefault;
        }
        public static bool is_close(GameObject attachee, GameObject obj)
        {
            return attachee.DistanceTo(obj) <= 15;
        }
        public static void destroy_gear(GameObject attachee, GameObject triggerer)
        {
            var dexterity_gloves_2 = attachee.FindItemByName(6199);
            dexterity_gloves_2.Destroy();
            var longbow_1 = attachee.FindItemByName(4191);
            longbow_1.Destroy();
            var flaming_longbow_1 = attachee.FindItemByName(4348);
            flaming_longbow_1.Destroy();
            var dexterity_gloves_4 = attachee.FindItemByName(6200);
            dexterity_gloves_4.Destroy();
            var longbow_2 = attachee.FindItemByName(4299);
            longbow_2.Destroy();
            var frost_longbow_2 = attachee.FindItemByName(4349);
            frost_longbow_2.Destroy();
            var dexterity_gloves_6 = attachee.FindItemByName(6201);
            dexterity_gloves_6.Destroy();
            var unholy_longbow_2 = attachee.FindItemByName(4482);
            unholy_longbow_2.Destroy();
            var unholy_longbow_2_electric = attachee.FindItemByName(4350);
            unholy_longbow_2_electric.Destroy();
            var resist_cloak_2_orange = attachee.FindItemByName(6692);
            resist_cloak_2_orange.Destroy();
            var resist_cloak_2_fur = attachee.FindItemByName(6682);
            resist_cloak_2_fur.Destroy();
            var resist_cloak_2_red = attachee.FindItemByName(6667);
            resist_cloak_2_red.Destroy();
            var unholy_heavy_mace = attachee.FindItemByName(4449);
            unholy_heavy_mace.Destroy();
            return;
        }

    }
}
