
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
    [ObjectScript(623)]
    public class HbCave : BaseObjectScript
    {
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            var webbed = Livonya.break_free(attachee, 3);
            // MELEE TROOPS  #
            // dumb guys - gnoll troops, kallop, boonthag, naig lliht  #
            if ((attachee.GetNameId() == 8634 || attachee.GetNameId() == 8815 || attachee.GetNameId() == 8816 || attachee.GetNameId() == 8813))
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
            // dumb guys with rage - bugbear troops, orc fighters, hungous, ergo  #
            else if ((attachee.GetNameId() == 8632 || attachee.GetNameId() == 8633 || attachee.GetNameId() == 8635 || attachee.GetNameId() == 8803 || attachee.GetNameId() == 8814))
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
            // smart guys  - might be a bum strategy, too much running around  #
            // elif (attachee.name == xxxx):
            // leader = attachee.leader_get()
            // if (group_percent_hp(leader) >= 51):
            // for obj in game.party[0].group_list():
            // if obj.d20_query(Q_Prone):
            // attachee.obj_set_int(obj_f_critter_strategy, 545)
            // else:
            // attachee.obj_set_int(obj_f_critter_strategy, 550)
            // elif (group_percent_hp(leader) <= 50):
            // for obj in game.party[0].group_list():
            // if obj.d20_query(Q_Prone):
            // attachee.obj_set_int(obj_f_critter_strategy, 545)
            // else:
            // attachee.obj_set_int(obj_f_critter_strategy, 546)
            // smart guys with rage - might be a bum strategy, too much running around  #
            // elif (attachee.name == xxxx):
            // leader = attachee.leader_get()
            // if (group_percent_hp(leader) >= 51):
            // for obj in game.party[0].group_list():
            // if obj.d20_query(Q_Prone):
            // attachee.obj_set_int(obj_f_critter_strategy, 560)
            // else:
            // attachee.obj_set_int(obj_f_critter_strategy, 558)
            // elif (group_percent_hp(leader) <= 50):
            // for obj in game.party[0].group_list():
            // if obj.d20_query(Q_Prone):
            // attachee.obj_set_int(obj_f_critter_strategy, 560)
            // else:
            // attachee.obj_set_int(obj_f_critter_strategy, 561)
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
            // archery seekers with rage - ruff  #
            else if ((attachee.GetNameId() == 8817))
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
            // RANGED TROOPS  #
            // mage seekers - orc snipers  #
            else if ((attachee.GetNameId() == 8630 || attachee.GetNameId() == 8631))
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
            // spell responders - orc marksmen  #
            else if ((attachee.GetNameId() == 8628 || attachee.GetNameId() == 8629))
            {
                foreach (var obj in PartyLeader.GetPartyMembers())
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
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((attachee.GetNameId() == 8636))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((see_30(attachee, obj)))
                        {
                            if ((!ScriptDaemon.npc_get(attachee, 1)))
                            {
                                AttachParticles("Mon-Balor-Smokebody60", attachee);
                                AttachParticles("ef-fireburning", attachee);
                                Sound(4185, 1);
                                var damage_dice = Dice.Parse("4d20");
                                attachee.Damage(null, DamageType.Bludgeoning, damage_dice);
                                ScriptDaemon.npc_set(attachee, 1);
                                SetGlobalFlag(571, true);
                            }

                        }

                    }

                }
                else if ((attachee.GetNameId() == 8624 || attachee.GetNameId() == 8626 || attachee.GetNameId() == 8628 || attachee.GetNameId() == 8629 || attachee.GetNameId() == 8630 || attachee.GetNameId() == 8631 || attachee.GetNameId() == 8814 || attachee.GetNameId() == 8813))
                {
                    if ((GetGlobalFlag(571)))
                    {
                        attachee.Unconceal();
                        DetachScript();
                    }

                }
                else if ((attachee.GetNameId() == 8802))
                {
                    if ((GetGlobalFlag(571)))
                    {
                        attachee.Unconceal();
                        attachee.SetStandpoint(StandPointType.Night, 739);
                        attachee.SetStandpoint(StandPointType.Day, 739);
                        StartTimer(10000, () => wake_hungous());
                        DetachScript();
                    }

                }
                else if ((attachee.GetNameId() == 8625))
                {
                    if ((GetGlobalFlag(571)))
                    {
                        attachee.Unconceal();
                        attachee.SetStandpoint(StandPointType.Night, 740);
                        attachee.SetStandpoint(StandPointType.Day, 740);
                        DetachScript();
                    }

                }
                else if ((attachee.GetNameId() == 8817))
                {
                    if ((GetGlobalFlag(571)))
                    {
                        attachee.Unconceal();
                        attachee.SetStandpoint(StandPointType.Night, 741);
                        attachee.SetStandpoint(StandPointType.Day, 741);
                        DetachScript();
                    }

                }
                else if ((attachee.GetNameId() == 8815))
                {
                    if ((GetGlobalFlag(571)))
                    {
                        attachee.Unconceal();
                        attachee.SetStandpoint(StandPointType.Night, 438);
                        attachee.SetStandpoint(StandPointType.Day, 438);
                        DetachScript();
                    }

                }
                else if ((attachee.GetNameId() == 8634))
                {
                    if ((GetGlobalFlag(571)))
                    {
                        attachee.Unconceal();
                        attachee.SetStandpoint(StandPointType.Night, 736);
                        attachee.SetStandpoint(StandPointType.Day, 736);
                        DetachScript();
                    }

                }
                else if ((attachee.GetNameId() == 8632))
                {
                    if ((GetGlobalFlag(571)))
                    {
                        attachee.Unconceal();
                        attachee.SetStandpoint(StandPointType.Night, 737);
                        attachee.SetStandpoint(StandPointType.Day, 737);
                        DetachScript();
                    }

                }
                else if ((attachee.GetNameId() == 8633))
                {
                    if ((GetGlobalFlag(571)))
                    {
                        attachee.Unconceal();
                        attachee.SetStandpoint(StandPointType.Night, 437);
                        attachee.SetStandpoint(StandPointType.Day, 437);
                        DetachScript();
                    }

                }
                else if ((attachee.GetNameId() == 8635))
                {
                    if ((GetGlobalFlag(571)))
                    {
                        attachee.Unconceal();
                        attachee.SetStandpoint(StandPointType.Night, 436);
                        attachee.SetStandpoint(StandPointType.Day, 436);
                        DetachScript();
                    }

                }
                else if ((attachee.GetNameId() == 8816))
                {
                    if ((GetGlobalFlag(571)))
                    {
                        attachee.Unconceal();
                        attachee.SetStandpoint(StandPointType.Night, 435);
                        attachee.SetStandpoint(StandPointType.Day, 435);
                        DetachScript();
                    }

                }

            }

            return RunDefault;
        }
        public static bool see_30(GameObject speaker, GameObject listener)
        {
            return speaker.DistanceTo(listener) <= 30;
        }
        public static void wake_hungous()
        {
            SetGlobalVar(570, 1);
            return;
        }

    }
}
