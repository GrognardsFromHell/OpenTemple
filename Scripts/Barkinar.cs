
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
    [ObjectScript(147)]
    public class Barkinar : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(144)))
            {
                attachee.Attack(triggerer);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                if ((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3010)) || triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3016)) || triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3017)) || triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3020))))
                {
                    triggerer.BeginDialog(attachee, 130);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1);
                }

            }
            else if ((GetGlobalFlag(163)))
            {
                triggerer.BeginDialog(attachee, 220);
            }
            else if ((GetGlobalFlag(157)))
            {
                if (((GetGlobalFlag(146)) && (GetGlobalFlag(147)) && (!GetGlobalFlag(153)) && (!GetGlobalFlag(156))))
                {
                    triggerer.BeginDialog(attachee, 140);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 150);
                }

            }
            else if ((GetGlobalFlag(162)))
            {
                triggerer.BeginDialog(attachee, 160);
            }
            else if ((GetGlobalFlag(158)))
            {
                triggerer.BeginDialog(attachee, 190);
            }
            else if ((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3010)) || triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3016)) || triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3017)) || triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3020))))
            {
                triggerer.BeginDialog(attachee, 130);
            }
            else
            {
                triggerer.BeginDialog(attachee, 210);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                SetGlobalVar(725, 0);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(373, true);
            return RunDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(779) == 0))
            {
                SetGlobalVar(779, 1);
                attachee.TurnTowards(PartyLeader);
            }
            else if ((GetGlobalVar(779) == 2 && GetGlobalFlag(824)))
            {
                attachee.RemoveFromInitiative();
                attachee.SetObjectFlag(ObjectFlag.OFF);
                return SkipDefault;
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee != null && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
            {
                if ((!Utilities.is_daytime()))
                {
                    SetGlobalVar(742, GetGlobalVar(742) + 1);
                    if ((GetGlobalVar(742) == 3 && !GetGlobalFlag(147) && !GetGlobalFlag(990) && !GetGlobalFlag(823) && GetGlobalVar(779) == 1 && !GetGlobalFlag(824)))
                    {
                        var shocky_backup = GameSystems.MapObject.CreateObject(14233, new locXY(415, 528));
                        shocky_backup.Rotation = 3.14159265359f;
                        Sound(4035, 1);
                        AttachParticles("sp-Teleport", shocky_backup);
                        var barky = attachee.GetInitiative();
                        shocky_backup.AddToInitiative();
                        shocky_backup.SetInitiative(barky);
                        UiSystems.Combat.Initiative.UpdateIfNeeded();
                        foreach (var obj in ObjList.ListVicinity(shocky_backup.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            shocky_backup.Attack(obj);
                        }

                        SetGlobalFlag(823, true);
                    }

                }

                if ((Utilities.obj_percent_hp(attachee) <= 33))
                {
                    SetGlobalVar(741, GetGlobalVar(741) + 1);
                    if ((GetGlobalVar(741) >= 3 && !GetGlobalFlag(242)))
                    {
                        var chance = RandomRange(1, 3);
                        if ((chance == 1))
                        {
                            if ((!Utilities.is_daytime()))
                            {
                                var bugbear_backup_1 = GameSystems.MapObject.CreateObject(14826, new locXY(437, 545));
                                bugbear_backup_1.Rotation = 0.7853981634f;
                                bugbear_backup_1.Unconceal();
                                var bugbear_backup_2 = GameSystems.MapObject.CreateObject(14826, new locXY(433, 545));
                                bugbear_backup_2.Rotation = 0.7853981634f;
                                bugbear_backup_2.Unconceal();
                                Sound(4063, 1);
                                var barky = attachee.GetInitiative();
                                bugbear_backup_1.AddToInitiative();
                                bugbear_backup_2.AddToInitiative();
                                bugbear_backup_1.SetInitiative(barky);
                                bugbear_backup_2.SetInitiative(barky);
                                UiSystems.Combat.Initiative.UpdateIfNeeded();
                                foreach (var obj in ObjList.ListVicinity(bugbear_backup_1.GetLocation(), ObjectListFilter.OLC_PC))
                                {
                                    bugbear_backup_1.Attack(obj);
                                }

                                foreach (var obj in ObjList.ListVicinity(bugbear_backup_2.GetLocation(), ObjectListFilter.OLC_PC))
                                {
                                    bugbear_backup_2.Attack(obj);
                                }

                                SetGlobalFlag(242, true);
                            }
                            else if ((Utilities.is_daytime()))
                            {
                                var bugbear_backup_1 = GameSystems.MapObject.CreateObject(14826, new locXY(386, 540));
                                bugbear_backup_1.Rotation = 5.49778714378f;
                                bugbear_backup_1.Unconceal();
                                var bugbear_backup_2 = GameSystems.MapObject.CreateObject(14826, new locXY(386, 536));
                                bugbear_backup_2.Rotation = 5.49778714378f;
                                bugbear_backup_2.Unconceal();
                                Sound(4063, 1);
                                var barky = attachee.GetInitiative();
                                bugbear_backup_1.AddToInitiative();
                                bugbear_backup_2.AddToInitiative();
                                bugbear_backup_1.SetInitiative(barky);
                                bugbear_backup_2.SetInitiative(barky);
                                UiSystems.Combat.Initiative.UpdateIfNeeded();
                                foreach (var obj in ObjList.ListVicinity(bugbear_backup_1.GetLocation(), ObjectListFilter.OLC_PC))
                                {
                                    bugbear_backup_1.Attack(obj);
                                }

                                foreach (var obj in ObjList.ListVicinity(bugbear_backup_2.GetLocation(), ObjectListFilter.OLC_PC))
                                {
                                    bugbear_backup_2.Attack(obj);
                                }

                                SetGlobalFlag(242, true);
                            }

                        }

                    }

                    if ((GetGlobalVar(740) <= 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 450);
                        SetGlobalVar(740, GetGlobalVar(740) + 1);
                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 465);
                    }

                }
                else if ((Utilities.obj_percent_hp(attachee) >= 34 && Utilities.obj_percent_hp(attachee) <= 67))
                {
                    if ((GetGlobalVar(737) <= 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 445);
                        SetGlobalVar(737, GetGlobalVar(737) + 1);
                    }
                    else if ((GetGlobalVar(738) <= 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 442);
                        SetGlobalVar(738, GetGlobalVar(738) + 1);
                    }
                    else if ((GetGlobalVar(739) <= 2))
                    {
                        if ((Utilities.find_npc_near(attachee, 8036) != null || Utilities.find_npc_near(attachee, 8729) != null))
                        {
                            var deggum = Utilities.find_npc_near(attachee, 8036);
                            var senshock = Utilities.find_npc_near(attachee, 8729);
                            if ((Utilities.obj_percent_hp(deggum) <= 50 || Utilities.obj_percent_hp(senshock) <= 50))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 447);
                                SetGlobalVar(739, GetGlobalVar(739) + 1);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 446);
                            }

                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 446);
                        }

                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 446);
                    }

                }
                else
                {
                    if ((GetGlobalVar(739) <= 2))
                    {
                        if ((Utilities.find_npc_near(attachee, 8036) != null || Utilities.find_npc_near(attachee, 8729) != null))
                        {
                            var deggum = Utilities.find_npc_near(attachee, 8036);
                            var senshock = Utilities.find_npc_near(attachee, 8729);
                            if ((Utilities.obj_percent_hp(deggum) <= 50 || Utilities.obj_percent_hp(senshock) <= 50))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 447);
                                SetGlobalVar(739, GetGlobalVar(739) + 1);
                            }
                            else if ((GetGlobalVar(738) <= 5))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 442);
                                SetGlobalVar(738, GetGlobalVar(738) + 1);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 446);
                            }

                        }
                        else if ((GetGlobalVar(738) <= 5))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 442);
                            SetGlobalVar(738, GetGlobalVar(738) + 1);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 446);
                        }

                    }
                    else if ((GetGlobalVar(738) <= 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 442);
                        SetGlobalVar(738, GetGlobalVar(738) + 1);
                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 446);
                    }

                }

            }

            return RunDefault;
        }
        // SCRIPT DETAIL FOR START COMBAT								##
        // if not dead, unconscious, or prone							##
        // if under 33% health								##
        // increment bugbear backup variable					##
        // if 4 or more turns have passed and bugbears have not been spawned	##
        // generate 1 in 3 chance						##
        // if chance is met						##
        // spawn bugbear backup					##
        // if haven't cast all 2 self protection spells				##
        // set strategy to self protection					##
        // increment self protection variable				##
        // otherwise								##
        // set strategy to defense						##
        // otherwise, if between 34% and 67% health					##
        // if haven't cast all 3 self healing spells				##
        // set strategy to self healing					##
        // increment self healing variable					##
        // otherwise, if haven't cast all 6 normal spells				##
        // set strategy to normal casting					##
        // increment normal casting variable				##
        // otherwise, if haven't cast all 3 friend healing spells			##
        // if deggum or senshock are present and not dead			##
        // if either of them are under 50% health			##
        // set strategy to friend healing			##
        // increment friend healing variable		##
        // otherwise						##
        // set strategy to melee				##
        // otherwise							##
        // set strategy to melee					##
        // otherwise (if have cast all protection, healing and normal spells)	##
        // set strategy to melee						##
        // otherwise (if over 66% health)							##
        // if haven't cast all 3 friend healing spells				##
        // if deggum or senshock are present and not dead			##
        // if either of them are under 50% health			##
        // set strategy to friend healing			##
        // increment friend healing variable		##
        // otherwise, if haven't cast all 6 normal spells		##
        // set strategy to normal casting			##
        // increment normal casting variable		##
        // otherwise						##
        // set strategy to melee				##
        // otherwise, if haven't cast all 6 normal spells			##
        // set strategy to normal casting				##
        // increment normal casting variable			##
        // otherwise							##
        // set strategy to melee					##
        // otherwise, if haven't cast all 6 normal spells				##
        // set strategy to normal casting					##
        // increment normal casting variable				##
        // otheriwse (if have cast all friend healing and normal spells)		##
        // set strategy to melee						##
        // run default										##

        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(373, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((GetGlobalFlag(373)))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                    return SkipDefault;
                }

                var closest_jones = Utilities.party_closest(attachee);
                if ((attachee.DistanceTo(closest_jones) <= 100))
                {
                    SetGlobalVar(725, GetGlobalVar(725) + 1);
                    if ((attachee.GetLeader() == null))
                    {
                        if ((GetGlobalVar(725) == 4))
                        {
                            attachee.CastSpell(WellKnownSpells.ShieldOfFaith, attachee);
                            attachee.PendingSpellsToMemorized();
                        }

                        if ((GetGlobalVar(725) == 8))
                        {
                            attachee.CastSpell(WellKnownSpells.Blur, attachee);
                            attachee.PendingSpellsToMemorized();
                        }

                        if ((GetGlobalVar(725) == 12))
                        {
                            attachee.CastSpell(WellKnownSpells.MagicCircleAgainstGood, attachee);
                            attachee.PendingSpellsToMemorized();
                        }

                    }

                    if ((GetGlobalVar(725) >= 400))
                    {
                        SetGlobalVar(725, 0);
                    }

                }

                if ((!GetGlobalFlag(144)))
                {
                    if ((!GetGlobalFlag(375)))
                    {
                        if ((!Utilities.is_daytime()))
                        {
                            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                            {
                                if ((is_28_and_under(attachee, obj)))
                                {
                                    obj.TurnTowards(attachee);
                                    attachee.TurnTowards(obj);
                                    if ((Utilities.find_npc_near(attachee, 8036) != null))
                                    {
                                        var deggum = Utilities.find_npc_near(attachee, 8036);
                                        deggum.TurnTowards(obj);
                                    }

                                    obj.BeginDialog(attachee, 1);
                                    SetGlobalFlag(375, true);
                                }

                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnWillKos(GameObject attachee, GameObject triggerer)
        {
            if ((triggerer.type == ObjectType.pc))
            {
                if ((GetGlobalFlag(144)))
                {
                    return RunDefault;
                }

            }

            return SkipDefault;
        }
        public static bool banter(GameObject attachee, GameObject triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8036);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 70);
            }

            return SkipDefault;
        }
        public static bool banter2(GameObject attachee, GameObject triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8036);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 80);
            }

            return SkipDefault;
        }
        public static bool is_28_and_under(GameObject speaker, GameObject listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 28))
                {
                    return true;
                }

            }

            return false;
        }

    }
}
