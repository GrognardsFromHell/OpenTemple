
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
    [ObjectScript(587)]
    public class OrcShaman : BaseObjectScript
    {
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
            var webbed = Livonya.break_free(attachee, 3);
            if ((attachee != null && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone) && attachee.GetLeader() == null))
            {
                if ((Utilities.obj_percent_hp(attachee) <= 75))
                {
                    if ((attachee.GetNameId() == 8909))
                    {
                        // hb gatekeeper east
                        if ((GetGlobalVar(786) <= 17))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 541);
                            SetGlobalVar(786, GetGlobalVar(786) + 1);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 535);
                        }

                    }
                    else if ((attachee.GetNameId() == 8920))
                    {
                        // hb gatekeper west
                        if ((GetGlobalVar(789) <= 17))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 541);
                            SetGlobalVar(789, GetGlobalVar(789) + 1);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 535);
                        }

                    }
                    else if ((attachee.GetNameId() == 8960))
                    {
                        // hb ravine north
                        if ((GetGlobalVar(792) <= 17))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 541);
                            SetGlobalVar(792, GetGlobalVar(792) + 1);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 535);
                        }

                    }
                    else if ((attachee.GetNameId() == 8966))
                    {
                        // hb ravine south
                        if ((GetGlobalVar(793) <= 17))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 541);
                            SetGlobalVar(793, GetGlobalVar(793) + 1);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 535);
                        }

                    }

                }
                else if ((Utilities.obj_percent_hp(attachee) >= 76))
                {
                    if ((attachee.GetNameId() == 8909))
                    {
                        // hb gatekeeper east
                        var orcserg01 = Utilities.find_npc_near(attachee, 8894);
                        var orcdomi01 = Utilities.find_npc_near(attachee, 8895);
                        var orcbowm01 = Utilities.find_npc_near(attachee, 8899);
                        var orcbowm02 = Utilities.find_npc_near(attachee, 8900);
                        var orcarch01 = Utilities.find_npc_near(attachee, 8901);
                        var orcarch02 = Utilities.find_npc_near(attachee, 8902);
                        var orcsnip01 = Utilities.find_npc_near(attachee, 8903);
                        var hilgian01 = Utilities.find_npc_near(attachee, 8904);
                        var hilgian02 = Utilities.find_npc_near(attachee, 8905);
                        var hilgian03 = Utilities.find_npc_near(attachee, 8906);
                        var hilgian04 = Utilities.find_npc_near(attachee, 8907);
                        var hilgian05 = Utilities.find_npc_near(attachee, 8908);
                        var orcmedi01 = Utilities.find_npc_near(attachee, 8910);
                        if ((Utilities.obj_percent_hp(orcserg01) <= 75 || Utilities.obj_percent_hp(orcdomi01) <= 75 || Utilities.obj_percent_hp(orcbowm01) <= 75 || Utilities.obj_percent_hp(orcbowm02) <= 75 || Utilities.obj_percent_hp(orcarch01) <= 75 || Utilities.obj_percent_hp(orcarch02) <= 75 || Utilities.obj_percent_hp(orcsnip01) <= 75 || Utilities.obj_percent_hp(hilgian01) <= 75 || Utilities.obj_percent_hp(hilgian02) <= 75 || Utilities.obj_percent_hp(hilgian03) <= 75 || Utilities.obj_percent_hp(hilgian04) <= 75 || Utilities.obj_percent_hp(hilgian05) <= 75 || Utilities.obj_percent_hp(orcmedi01) <= 75))
                        {
                            if ((GetGlobalVar(786) <= 17))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 475);
                                SetGlobalVar(786, GetGlobalVar(786) + 1);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 535);
                            }

                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 539);
                        }

                    }
                    else if ((attachee.GetNameId() == 8920))
                    {
                        // hb gatekeper west
                        var ogrexxx01 = Utilities.find_npc_near(attachee, 8911);
                        var ettinxx01 = Utilities.find_npc_near(attachee, 8912);
                        var ettinxx02 = Utilities.find_npc_near(attachee, 8913);
                        var ettinxx03 = Utilities.find_npc_near(attachee, 8914);
                        var ettinxx04 = Utilities.find_npc_near(attachee, 8915);
                        var ettinxx05 = Utilities.find_npc_near(attachee, 8916);
                        var orcbowm01 = Utilities.find_npc_near(attachee, 8917);
                        var orcbowm02 = Utilities.find_npc_near(attachee, 8918);
                        var orcarch01 = Utilities.find_npc_near(attachee, 8919);
                        var orcmedi01 = Utilities.find_npc_near(attachee, 8921);
                        if ((Utilities.obj_percent_hp(ogrexxx01) <= 75 || Utilities.obj_percent_hp(ettinxx01) <= 75 || Utilities.obj_percent_hp(ettinxx02) <= 75 || Utilities.obj_percent_hp(ettinxx03) <= 75 || Utilities.obj_percent_hp(ettinxx04) <= 75 || Utilities.obj_percent_hp(ettinxx05) <= 75 || Utilities.obj_percent_hp(orcbowm01) <= 75 || Utilities.obj_percent_hp(orcbowm02) <= 75 || Utilities.obj_percent_hp(orcarch01) <= 75 || Utilities.obj_percent_hp(orcmedi01) <= 75))
                        {
                            if ((GetGlobalVar(789) <= 17))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 475);
                                SetGlobalVar(789, GetGlobalVar(789) + 1);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 535);
                            }

                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 539);
                        }

                    }
                    else if ((attachee.GetNameId() == 8960))
                    {
                        // hb ravine north
                        var gnollxx01 = Utilities.find_npc_near(attachee, 8931);
                        var gnollxx02 = Utilities.find_npc_near(attachee, 8932);
                        var gnollxx03 = Utilities.find_npc_near(attachee, 8933);
                        var gnollxx04 = Utilities.find_npc_near(attachee, 8934);
                        var gnollxx05 = Utilities.find_npc_near(attachee, 8935);
                        var bugbear01 = Utilities.find_npc_near(attachee, 8936);
                        var bugbear02 = Utilities.find_npc_near(attachee, 8937);
                        var bugbear03 = Utilities.find_npc_near(attachee, 8938);
                        var bugbear04 = Utilities.find_npc_near(attachee, 8939);
                        var ogrexxx01 = Utilities.find_npc_near(attachee, 8969);
                        var ogrexxx02 = Utilities.find_npc_near(attachee, 8970);
                        var ogrexxx03 = Utilities.find_npc_near(attachee, 8971);
                        var orcmedi01 = Utilities.find_npc_near(attachee, 8961);
                        var orcbowm01 = Utilities.find_npc_near(attachee, 8978);
                        var orcarch01 = Utilities.find_npc_near(attachee, 8979);
                        var orcsnip01 = Utilities.find_npc_near(attachee, 8980);
                        var orcmark01 = Utilities.find_npc_near(attachee, 8981);
                        var orcsnip02 = Utilities.find_npc_near(attachee, 8982);
                        var orcarch02 = Utilities.find_npc_near(attachee, 8983);
                        if ((Utilities.obj_percent_hp(gnollxx01) <= 75 || Utilities.obj_percent_hp(gnollxx02) <= 75 || Utilities.obj_percent_hp(gnollxx03) <= 75 || Utilities.obj_percent_hp(gnollxx04) <= 75 || Utilities.obj_percent_hp(gnollxx05) <= 75 || Utilities.obj_percent_hp(bugbear01) <= 75 || Utilities.obj_percent_hp(bugbear02) <= 75 || Utilities.obj_percent_hp(bugbear03) <= 75 || Utilities.obj_percent_hp(bugbear04) <= 75 || Utilities.obj_percent_hp(ogrexxx01) <= 75 || Utilities.obj_percent_hp(ogrexxx02) <= 75 || Utilities.obj_percent_hp(ogrexxx03) <= 75 || Utilities.obj_percent_hp(orcmedi01) <= 75 || Utilities.obj_percent_hp(orcbowm01) <= 75 || Utilities.obj_percent_hp(orcarch01) <= 75 || Utilities.obj_percent_hp(orcsnip01) <= 75 || Utilities.obj_percent_hp(orcmark01) <= 75 || Utilities.obj_percent_hp(orcsnip02) <= 75 || Utilities.obj_percent_hp(orcarch02) <= 75))
                        {
                            if ((GetGlobalVar(792) <= 17))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 475);
                                SetGlobalVar(792, GetGlobalVar(792) + 1);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 535);
                            }

                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 539);
                        }

                    }
                    else if ((attachee.GetNameId() == 8966))
                    {
                        // hb ravine south
                        var orcserg02 = Utilities.find_npc_near(attachee, 8950);
                        var bugbear06 = Utilities.find_npc_near(attachee, 8951);
                        var bugbear07 = Utilities.find_npc_near(attachee, 8952);
                        var bugbear08 = Utilities.find_npc_near(attachee, 8953);
                        var bugbear09 = Utilities.find_npc_near(attachee, 8954);
                        var gnollxx06 = Utilities.find_npc_near(attachee, 8955);
                        var gnollxx07 = Utilities.find_npc_near(attachee, 8956);
                        var gnollxx08 = Utilities.find_npc_near(attachee, 8957);
                        var gnollxx09 = Utilities.find_npc_near(attachee, 8958);
                        var ettinxx01 = Utilities.find_npc_near(attachee, 8975);
                        var ettinxx02 = Utilities.find_npc_near(attachee, 8976);
                        var ettinxx03 = Utilities.find_npc_near(attachee, 8977);
                        var orcmedi02 = Utilities.find_npc_near(attachee, 8965);
                        var orcarch04 = Utilities.find_npc_near(attachee, 8985);
                        var orcsnip03 = Utilities.find_npc_near(attachee, 8987);
                        var orcbowm03 = Utilities.find_npc_near(attachee, 8989);
                        var orcarch05 = Utilities.find_npc_near(attachee, 8991);
                        if ((Utilities.obj_percent_hp(orcserg02) <= 75 || Utilities.obj_percent_hp(bugbear06) <= 75 || Utilities.obj_percent_hp(bugbear07) <= 75 || Utilities.obj_percent_hp(bugbear08) <= 75 || Utilities.obj_percent_hp(bugbear09) <= 75 || Utilities.obj_percent_hp(gnollxx06) <= 75 || Utilities.obj_percent_hp(gnollxx07) <= 75 || Utilities.obj_percent_hp(gnollxx08) <= 75 || Utilities.obj_percent_hp(gnollxx09) <= 75 || Utilities.obj_percent_hp(ettinxx01) <= 75 || Utilities.obj_percent_hp(ettinxx02) <= 75 || Utilities.obj_percent_hp(ettinxx03) <= 75 || Utilities.obj_percent_hp(orcmedi02) <= 75 || Utilities.obj_percent_hp(orcarch04) <= 75 || Utilities.obj_percent_hp(orcsnip03) <= 75 || Utilities.obj_percent_hp(orcbowm03) <= 75 || Utilities.obj_percent_hp(orcarch05) <= 75))
                        {
                            if ((GetGlobalVar(793) <= 17))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 475);
                                SetGlobalVar(793, GetGlobalVar(793) + 1);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 535);
                            }

                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 539);
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
