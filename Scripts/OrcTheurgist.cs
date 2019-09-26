
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
    [ObjectScript(589)]
    public class OrcTheurgist : BaseObjectScript
    {
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetNameId() == 8962))
            {
                var stone_backup_ravine_3 = GameSystems.MapObject.CreateObject(14986, new locXY(432, 506));
                stone_backup_ravine_3.Rotation = 4.71238898038f;
                stone_backup_ravine_3.SetConcealed(true);
                stone_backup_ravine_3.Unconceal();
                AttachParticles("Mon-EarthElem-Unconceal", stone_backup_ravine_3);
                foreach (var obj in ObjList.ListVicinity(stone_backup_ravine_3.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    stone_backup_ravine_3.Attack(obj);
                }

                var stone_backup_ravine_4 = GameSystems.MapObject.CreateObject(14986, new locXY(440, 506));
                stone_backup_ravine_4.Rotation = 4.71238898038f;
                stone_backup_ravine_4.SetConcealed(true);
                stone_backup_ravine_4.Unconceal();
                AttachParticles("Mon-EarthElem-Unconceal", stone_backup_ravine_4);
                Sound(4176, 1);
                UiSystems.Combat.Initiative.UpdateIfNeeded();
                foreach (var obj in ObjList.ListVicinity(stone_backup_ravine_4.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    stone_backup_ravine_4.Attack(obj);
                }

            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var webbed = Livonya.break_free(attachee, 3);
            if ((attachee != null && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone) && attachee.GetLeader() == null))
            {
                if ((attachee.GetNameId() == 8962 || attachee.GetNameId() == 8964))
                {
                    if ((Utilities.obj_percent_hp(attachee) <= 75))
                    {
                        if ((attachee.GetNameId() == 8962))
                        {
                            // hb ravine central 1
                            if ((GetGlobalVar(798) <= 15))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 542);
                                SetGlobalVar(798, GetGlobalVar(798) + 1);
                            }
                            else if ((GetGlobalVar(799) <= 3))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 538);
                                SetGlobalVar(799, GetGlobalVar(799) + 1);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 536);
                            }

                        }
                        else if ((attachee.GetNameId() == 8964))
                        {
                            // hb ravine central 2
                            if ((GetGlobalVar(800) <= 15))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 542);
                                SetGlobalVar(800, GetGlobalVar(800) + 1);
                            }
                            else if ((GetGlobalVar(801) <= 3))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 538);
                                SetGlobalVar(801, GetGlobalVar(801) + 1);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 536);
                            }

                        }

                    }
                    else if ((Utilities.obj_percent_hp(attachee) >= 76))
                    {
                        if ((attachee.GetNameId() == 8962))
                        {
                            // hb ravine central 1
                            var bugbear05 = Utilities.find_npc_near(attachee, 8940);
                            var orcserg01 = Utilities.find_npc_near(attachee, 8941);
                            var orcfigh01 = Utilities.find_npc_near(attachee, 8942);
                            var orcfigh02 = Utilities.find_npc_near(attachee, 8943);
                            var orcfigh03 = Utilities.find_npc_near(attachee, 8944);
                            var orcmurd01 = Utilities.find_npc_near(attachee, 8945);
                            var orcdomi01 = Utilities.find_npc_near(attachee, 8946);
                            var orcfigh04 = Utilities.find_npc_near(attachee, 8947);
                            var orcfigh05 = Utilities.find_npc_near(attachee, 8948);
                            var orcrund01 = Utilities.find_npc_near(attachee, 8949);
                            var stogian01 = Utilities.find_npc_near(attachee, 8972);
                            var hilgian01 = Utilities.find_npc_near(attachee, 8973);
                            var stogian02 = Utilities.find_npc_near(attachee, 8974);
                            var orctheu02 = Utilities.find_npc_near(attachee, 8964);
                            var orcarch03 = Utilities.find_npc_near(attachee, 8984);
                            var orcbowm02 = Utilities.find_npc_near(attachee, 8986);
                            var orcmark02 = Utilities.find_npc_near(attachee, 8988);
                            var orcsnip04 = Utilities.find_npc_near(attachee, 8990);
                            if ((Utilities.obj_percent_hp(bugbear05) <= 75 || Utilities.obj_percent_hp(orcserg01) <= 75 || Utilities.obj_percent_hp(orcfigh01) <= 75 || Utilities.obj_percent_hp(orcfigh02) <= 75 || Utilities.obj_percent_hp(orcfigh03) <= 75 || Utilities.obj_percent_hp(orcmurd01) <= 75 || Utilities.obj_percent_hp(orcdomi01) <= 75 || Utilities.obj_percent_hp(orcfigh04) <= 75 || Utilities.obj_percent_hp(orcfigh05) <= 75 || Utilities.obj_percent_hp(orcrund01) <= 75 || Utilities.obj_percent_hp(stogian01) <= 75 || Utilities.obj_percent_hp(hilgian01) <= 75 || Utilities.obj_percent_hp(stogian02) <= 75 || Utilities.obj_percent_hp(orctheu02) <= 75 || Utilities.obj_percent_hp(orcarch03) <= 75 || Utilities.obj_percent_hp(orcbowm02) <= 75 || Utilities.obj_percent_hp(orcmark02) <= 75 || Utilities.obj_percent_hp(orcsnip04) <= 75))
                            {
                                if ((GetGlobalVar(798) <= 15))
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 476);
                                    SetGlobalVar(798, GetGlobalVar(798) + 1);
                                }
                                else if ((GetGlobalVar(799) <= 3))
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 538);
                                    SetGlobalVar(799, GetGlobalVar(799) + 1);
                                }
                                else
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 536);
                                }

                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 544);
                            }

                        }
                        else if ((attachee.GetNameId() == 8964))
                        {
                            // hb ravine central 2
                            var bugbear05 = Utilities.find_npc_near(attachee, 8940);
                            var orcserg01 = Utilities.find_npc_near(attachee, 8941);
                            var orcfigh01 = Utilities.find_npc_near(attachee, 8942);
                            var orcfigh02 = Utilities.find_npc_near(attachee, 8943);
                            var orcfigh03 = Utilities.find_npc_near(attachee, 8944);
                            var orcmurd01 = Utilities.find_npc_near(attachee, 8945);
                            var orcdomi01 = Utilities.find_npc_near(attachee, 8946);
                            var orcfigh04 = Utilities.find_npc_near(attachee, 8947);
                            var orcfigh05 = Utilities.find_npc_near(attachee, 8948);
                            var orcrund01 = Utilities.find_npc_near(attachee, 8949);
                            var stogian01 = Utilities.find_npc_near(attachee, 8972);
                            var hilgian01 = Utilities.find_npc_near(attachee, 8973);
                            var stogian02 = Utilities.find_npc_near(attachee, 8974);
                            var orctheu01 = Utilities.find_npc_near(attachee, 8962);
                            var orcarch03 = Utilities.find_npc_near(attachee, 8984);
                            var orcbowm02 = Utilities.find_npc_near(attachee, 8986);
                            var orcmark02 = Utilities.find_npc_near(attachee, 8988);
                            var orcsnip04 = Utilities.find_npc_near(attachee, 8990);
                            if ((Utilities.obj_percent_hp(bugbear05) <= 75 || Utilities.obj_percent_hp(orcserg01) <= 75 || Utilities.obj_percent_hp(orcfigh01) <= 75 || Utilities.obj_percent_hp(orcfigh02) <= 75 || Utilities.obj_percent_hp(orcfigh03) <= 75 || Utilities.obj_percent_hp(orcmurd01) <= 75 || Utilities.obj_percent_hp(orcdomi01) <= 75 || Utilities.obj_percent_hp(orcfigh04) <= 75 || Utilities.obj_percent_hp(orcfigh05) <= 75 || Utilities.obj_percent_hp(orcrund01) <= 75 || Utilities.obj_percent_hp(stogian01) <= 75 || Utilities.obj_percent_hp(hilgian01) <= 75 || Utilities.obj_percent_hp(stogian02) <= 75 || Utilities.obj_percent_hp(orctheu01) <= 75 || Utilities.obj_percent_hp(orcarch03) <= 75 || Utilities.obj_percent_hp(orcbowm02) <= 75 || Utilities.obj_percent_hp(orcmark02) <= 75 || Utilities.obj_percent_hp(orcsnip04) <= 75))
                            {
                                if ((GetGlobalVar(800) <= 15))
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 476);
                                    SetGlobalVar(800, GetGlobalVar(800) + 1);
                                }
                                else if ((GetGlobalVar(801) <= 3))
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 538);
                                    SetGlobalVar(801, GetGlobalVar(801) + 1);
                                }
                                else
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 536);
                                }

                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 544);
                            }

                        }

                    }

                }
                else if ((attachee.GetNameId() == 8998 || attachee.GetNameId() == 9000))
                {
                    if ((Utilities.obj_percent_hp(attachee) <= 75))
                    {
                        if ((attachee.GetNameId() == 8998))
                        {
                            // hb pit west 1
                            if ((GetGlobalVar(803) <= 15))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 542);
                                SetGlobalVar(803, GetGlobalVar(803) + 1);
                            }
                            else if ((GetGlobalVar(804) <= 3))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 538);
                                SetGlobalVar(804, GetGlobalVar(804) + 1);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 536);
                            }

                        }
                        else if ((attachee.GetNameId() == 9000))
                        {
                            // hb pit east 1
                            if ((GetGlobalVar(805) <= 15))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 542);
                                SetGlobalVar(805, GetGlobalVar(805) + 1);
                            }
                            else if ((GetGlobalVar(806) <= 3))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 538);
                                SetGlobalVar(806, GetGlobalVar(806) + 1);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 536);
                            }

                        }

                    }
                    else if ((Utilities.obj_percent_hp(attachee) >= 76))
                    {
                        if ((attachee.GetNameId() == 8998))
                        {
                            // hb pit west 1
                            var orcmurd01 = Utilities.find_npc_near(attachee, 8992);
                            var orcserg01 = Utilities.find_npc_near(attachee, 8993);
                            var ogrexxx01 = Utilities.find_npc_near(attachee, 8994);
                            var orcsnip01 = Utilities.find_npc_near(attachee, 8995);
                            var orcsnip02 = Utilities.find_npc_near(attachee, 8996);
                            var orcmark01 = Utilities.find_npc_near(attachee, 8997);
                            if ((Utilities.obj_percent_hp(orcmurd01) <= 75 || Utilities.obj_percent_hp(orcserg01) <= 75 || Utilities.obj_percent_hp(ogrexxx01) <= 75 || Utilities.obj_percent_hp(orcsnip01) <= 75 || Utilities.obj_percent_hp(orcsnip02) <= 75 || Utilities.obj_percent_hp(orcmark01) <= 75))
                            {
                                if ((GetGlobalVar(803) <= 15))
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 476);
                                    SetGlobalVar(803, GetGlobalVar(803) + 1);
                                }
                                else if ((GetGlobalVar(804) <= 3))
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 538);
                                    SetGlobalVar(804, GetGlobalVar(804) + 1);
                                }
                                else
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 536);
                                }

                            }
                            else
                            {
                                if ((GetGlobalVar(804) <= 3))
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 538);
                                    SetGlobalVar(804, GetGlobalVar(804) + 1);
                                }
                                else
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 544);
                                }

                            }

                        }
                        else if ((attachee.GetNameId() == 9000))
                        {
                            // hb pit east 1
                            var orcassa01 = Utilities.find_npc_near(attachee, 8999);
                            var orcfigh01 = Utilities.find_npc_near(attachee, 8601);
                            var orcfigh02 = Utilities.find_npc_near(attachee, 8602);
                            var orcfigh03 = Utilities.find_npc_near(attachee, 8603);
                            var orcfigh04 = Utilities.find_npc_near(attachee, 8604);
                            var orcfigh05 = Utilities.find_npc_near(attachee, 8605);
                            var orcfigh06 = Utilities.find_npc_near(attachee, 8606);
                            var orcsnip01 = Utilities.find_npc_near(attachee, 8607);
                            var orcsnip02 = Utilities.find_npc_near(attachee, 8608);
                            var orcmark01 = Utilities.find_npc_near(attachee, 8609);
                            if ((Utilities.obj_percent_hp(orcassa01) <= 75 || Utilities.obj_percent_hp(orcfigh01) <= 75 || Utilities.obj_percent_hp(orcfigh02) <= 75 || Utilities.obj_percent_hp(orcfigh03) <= 75 || Utilities.obj_percent_hp(orcfigh04) <= 75 || Utilities.obj_percent_hp(orcfigh05) <= 75 || Utilities.obj_percent_hp(orcfigh06) <= 75 || Utilities.obj_percent_hp(orcsnip01) <= 75 || Utilities.obj_percent_hp(orcsnip02) <= 75 || Utilities.obj_percent_hp(orcmark01) <= 75))
                            {
                                if ((GetGlobalVar(805) <= 15))
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 476);
                                    SetGlobalVar(805, GetGlobalVar(805) + 1);
                                }
                                else if ((GetGlobalVar(806) <= 3))
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 538);
                                    SetGlobalVar(806, GetGlobalVar(806) + 1);
                                }
                                else
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 536);
                                }

                            }
                            else
                            {
                                if ((GetGlobalVar(806) <= 3))
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 538);
                                    SetGlobalVar(806, GetGlobalVar(806) + 1);
                                }
                                else
                                {
                                    attachee.SetInt(obj_f.critter_strategy, 544);
                                }

                            }

                        }

                    }

                }
                else if ((attachee.GetNameId() == 8624))
                {
                    // hb cave west
                    if ((Utilities.obj_percent_hp(attachee) <= 75))
                    {
                        if ((GetGlobalVar(807) <= 15))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 542);
                            SetGlobalVar(807, GetGlobalVar(807) + 1);
                        }
                        else if ((GetGlobalVar(808) <= 3))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 538);
                            SetGlobalVar(808, GetGlobalVar(808) + 1);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 536);
                        }

                    }
                    else if ((Utilities.obj_percent_hp(attachee) >= 76))
                    {
                        var orcwarl01 = Utilities.find_npc_near(attachee, 8626);
                        var orcwitc01 = Utilities.find_npc_near(attachee, 8627);
                        var orcmark01 = Utilities.find_npc_near(attachee, 8628);
                        var orcmark02 = Utilities.find_npc_near(attachee, 8629);
                        var orcsnip01 = Utilities.find_npc_near(attachee, 8630);
                        var orcsnip02 = Utilities.find_npc_near(attachee, 8631);
                        if ((Utilities.obj_percent_hp(orcwarl01) <= 75 || Utilities.obj_percent_hp(orcwitc01) <= 75 || Utilities.obj_percent_hp(orcmark01) <= 75 || Utilities.obj_percent_hp(orcmark02) <= 75 || Utilities.obj_percent_hp(orcsnip01) <= 75 || Utilities.obj_percent_hp(orcsnip02) <= 75))
                        {
                            if ((GetGlobalVar(807) <= 15))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 476);
                                SetGlobalVar(807, GetGlobalVar(807) + 1);
                            }
                            else if ((GetGlobalVar(808) <= 3))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 538);
                                SetGlobalVar(808, GetGlobalVar(808) + 1);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 536);
                            }

                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 544);
                        }

                    }

                }
                else if ((attachee.GetNameId() == 8625))
                {
                    // hb cave east
                    if ((Utilities.obj_percent_hp(attachee) <= 75))
                    {
                        if ((GetGlobalVar(809) <= 15))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 542);
                            SetGlobalVar(809, GetGlobalVar(809) + 1);
                        }
                        else if ((GetGlobalVar(810) <= 3))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 538);
                            SetGlobalVar(810, GetGlobalVar(810) + 1);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 536);
                        }

                    }
                    else if ((Utilities.obj_percent_hp(attachee) >= 76))
                    {
                        var orcfigh01 = Utilities.find_npc_near(attachee, 8632);
                        var orcfigh02 = Utilities.find_npc_near(attachee, 8633);
                        var gnollxx01 = Utilities.find_npc_near(attachee, 8634);
                        var bugbear01 = Utilities.find_npc_near(attachee, 8635);
                        var boonthagx = Utilities.find_npc_near(attachee, 8816);
                        var kallopxxx = Utilities.find_npc_near(attachee, 8815);
                        var ergoxxxxx = Utilities.find_npc_near(attachee, 8814);
                        var naiglliht = Utilities.find_npc_near(attachee, 8813);
                        var hungousxx = Utilities.find_npc_near(attachee, 8803);
                        var krunchxxx = Utilities.find_npc_near(attachee, 8802);
                        var ruffxxxxx = Utilities.find_npc_near(attachee, 8817);
                        if ((Utilities.obj_percent_hp(orcfigh01) <= 75 || Utilities.obj_percent_hp(orcfigh02) <= 75 || Utilities.obj_percent_hp(gnollxx01) <= 75 || Utilities.obj_percent_hp(bugbear01) <= 75 || Utilities.obj_percent_hp(boonthagx) <= 75 || Utilities.obj_percent_hp(kallopxxx) <= 75 || Utilities.obj_percent_hp(ergoxxxxx) <= 75 || Utilities.obj_percent_hp(naiglliht) <= 75 || Utilities.obj_percent_hp(hungousxx) <= 75 || Utilities.obj_percent_hp(krunchxxx) <= 75 || Utilities.obj_percent_hp(ruffxxxxx) <= 75))
                        {
                            if ((GetGlobalVar(809) <= 15))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 476);
                                SetGlobalVar(809, GetGlobalVar(809) + 1);
                            }
                            else if ((GetGlobalVar(810) <= 3))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 538);
                                SetGlobalVar(810, GetGlobalVar(810) + 1);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 536);
                            }

                        }
                        else
                        {
                            if ((GetGlobalVar(810) <= 3))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 538);
                                SetGlobalVar(810, GetGlobalVar(810) + 1);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 544);
                            }

                        }

                    }

                }

            }
            else if ((attachee.GetLeader() != null))
            {
                attachee.SetInt(obj_f.critter_strategy, 0);
            }

            return RunDefault;
        }

    }
}
