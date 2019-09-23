
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
    [ObjectScript(586)]
    public class HbEastWest : BaseObjectScript
    {
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            destroy_gear(attachee, triggerer);
            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var webbed = break_free(attachee, 3);
            // MELEE TROOPS  #
            // dumb guys - ettin troops, stone giant troops, hill giant troops, gnoll troops, orc rundors  #
            if ((attachee.GetNameId() == 14985 || attachee.GetNameId() == 14986 || attachee.GetNameId() == 14988 || attachee.GetNameId() == 14475 || attachee.GetNameId() == 8610 || attachee.GetNameId() == 8611 || attachee.GetNameId() == 8612))
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
            // dumb guys with rage - bugbear troops, ogre troops  #
            else if ((attachee.GetNameId() == 14476 || attachee.GetNameId() == 14990))
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
            // RANGED TROOPS  #
            // dumb guys - orc bowmen  #
            else if ((attachee.GetNameId() == 14467))
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
            else if ((attachee.GetNameId() == 14746))
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
            else if ((attachee.GetNameId() == 14748))
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

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5095))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    if ((attachee != null && Utilities.critter_is_unconscious(attachee) != 1 && !attachee.D20Query(D20DispatcherKey.QUE_Prone) && attachee.GetLeader() == null))
                    {
                        if ((attachee.GetNameId() == 8610))
                        {
                            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                            {
                                if ((talk_40(attachee, obj)))
                                {
                                    attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
                                    attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                                    attachee.SetStandpoint(StandPointType.Night, 432);
                                    attachee.SetStandpoint(StandPointType.Day, 432);
                                    attachee.RunOff(new locXY(442, 402));
                                    StartTimer(8000, () => orc_rund_1_off(attachee, triggerer));
                                }

                            }

                        }
                        else if ((attachee.GetNameId() == 8611))
                        {
                            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                            {
                                if ((talk_40(attachee, obj)))
                                {
                                    attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
                                    attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                                    attachee.SetStandpoint(StandPointType.Night, 433);
                                    attachee.SetStandpoint(StandPointType.Day, 433);
                                    attachee.RunOff(new locXY(444, 384));
                                    StartTimer(8000, () => orc_rund_2_off(attachee, triggerer));
                                }

                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public static void destroy_gear(GameObjectBody attachee, GameObjectBody triggerer)
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
        public static int talk_40(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 40))
                {
                    return 1;
                }

            }

            return 0;
        }
        public static bool orc_rund_1_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }
        public static bool orc_rund_2_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }

    }
}
