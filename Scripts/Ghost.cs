
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
    [ObjectScript(395)]
    public class Ghost : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetNameId() == 14662 || attachee.GetNameId() == 14663))
            {
                // undead legion ghosts
                if ((attachee.GetMap() == 5121))
                {
                    // verbobonc exterior
                    if ((GetQuestState(83) == QuestState.Completed))
                    {
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                    }

                }

            }
            else if ((!Utilities.is_daytime()))
            {
                // is nighttime
                if ((GetGlobalVar(765) >= 1))
                {
                    // player has encountered Moathouse Ambush at any of the 3 locations, meaning they have killed Turuko, Zert, and Kobort and their ghosts will haunt them
                    if ((attachee.GetNameId() == 8699))
                    {
                        // turuko ghost
                        if ((attachee.GetMap() == 5146))
                        {
                            // castle level 4 - upper hall
                            if ((GetGlobalVar(696) == 0))
                            {
                                // turuko ghost not activated
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // turn on turuko ghost
                                attachee.FloatLine(1000, triggerer);
                                // turuko ghost screeches!
                                SetGlobalVar(696, 1);
                                // turuko ghost is now on
                                SetGlobalFlag(869, true);
                            }
                            // castle sleep impossible flag set
                            else if ((GetGlobalVar(696) == 6))
                            {
                                // kobort ghost has made his following speech and gone away
                                if (triggerer.GetPartyMembers().Any(o => o.HasItemByName(12612)) && triggerer.GetPartyMembers().Any(o => o.HasItemByName(12614)) && triggerer.GetPartyMembers().Any(o => o.HasItemByName(12616)))
                                {
                                    // player has all the ghosts' parts
                                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                                    // turn on turuko ghost
                                    attachee.FloatLine(1000, triggerer);
                                    // turuko ghost screeches!
                                    SetGlobalVar(696, 7);
                                }

                            }

                        }

                    }
                    // turuko ghost is now on
                    else if ((attachee.GetNameId() == 8859))
                    {
                        // zert ghost
                        if ((attachee.GetMap() == 5121))
                        {
                            // verbo exterior - around castle
                            if ((GetGlobalVar(696) == 2))
                            {
                                // turuko ghost has made his opening speech and gone away
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // turn on zert ghost
                                attachee.FloatLine(2000, triggerer);
                                // zert ghost screeches!
                                SetGlobalVar(696, 3);
                            }
                            // zert ghost is now on
                            else if ((GetGlobalVar(696) == 8))
                            {
                                // turuko ghost has made his concluding speech and gone away
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // turn on zert ghost
                                undead_legion(attachee, triggerer);
                                // zert ghost spawns the undead legion
                                attachee.SetObjectFlag(ObjectFlag.OFF);
                            }

                        }

                    }
                    // turn off zert ghost
                    else if ((attachee.GetNameId() == 8860))
                    {
                        // kobort ghost
                        if ((attachee.GetMap() == 5143))
                        {
                            // castle level 1 - basement
                            if ((GetGlobalVar(696) == 4))
                            {
                                // zert ghost has made his following speech and gone away
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // turn on kobort ghost
                                attachee.FloatLine(3000, triggerer);
                                // kobort ghost moans!
                                SetGlobalVar(696, 5);
                                // kobort ghost is now on
                                SetGlobalFlag(869, true);
                            }

                        }

                    }

                }

            }
            // castle sleep impossible flag set
            else if ((Utilities.is_daytime()))
            {
                // is daytime
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            // turn ghosts off because they only roll at night
            return RunDefault;
        }
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
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((attachee.GetNameId() == 8699))
                {
                    // turuko ghost
                    if ((attachee.GetMap() == 5146))
                    {
                        // castle level 4 - upper hall
                        if ((GetGlobalVar(696) == 1))
                        {
                            // turuko ghost activated
                            Co8.StopCombat(attachee, 0);
                            PartyLeader.BeginDialog(attachee, 100);
                            // turuko opening remarks, wants head back
                            return RunDefault;
                        }
                        else if ((GetGlobalVar(696) == 7))
                        {
                            // turuko ghost reactivated
                            Co8.StopCombat(attachee, 0);
                            PartyLeader.BeginDialog(attachee, 1);
                            // turuko concluding remarks, got their stuff
                            return RunDefault;
                        }

                    }

                }
                else if ((attachee.GetNameId() == 8859))
                {
                    // zert ghost
                    if ((attachee.GetMap() == 5121))
                    {
                        // verbo exterior - around castle
                        if ((GetGlobalVar(696) == 3))
                        {
                            // zert ghost activated
                            Co8.StopCombat(attachee, 0);
                            PartyLeader.BeginDialog(attachee, 200);
                            // zert following remarks, wants hands back
                            return RunDefault;
                        }

                    }

                }
                else if ((attachee.GetNameId() == 8860))
                {
                    // kobort ghost
                    if ((attachee.GetMap() == 5143))
                    {
                        // castle level 1 - basement
                        if ((GetGlobalVar(696) == 5))
                        {
                            // kobort ghost activated
                            Co8.StopCombat(attachee, 0);
                            PartyLeader.BeginDialog(attachee, 300);
                            // kobort following remarks, wants feet back
                            return RunDefault;
                        }

                    }

                }
                else
                {
                    // random rest attacking ghosts
                    Co8.StopCombat(attachee, 0);
                    attachee.FloatLine(4010, triggerer);
                    // generic screech
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                    return RunDefault;
                }

            }
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(696) >= 1))
            {
                if ((attachee.GetNameId() == 8699))
                {
                    // turuko ghost
                    if ((attachee.GetMap() == 5146))
                    {
                        // castle level 4 - upper hall
                        if ((!GameSystems.Combat.IsCombatActive()))
                        {
                            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                            {
                                if ((is_better_to_talk(attachee, obj)))
                                {
                                    attachee.FloatLine(1010, triggerer);
                                    // turuko ghost screeches!
                                    DetachScript();
                                }

                            }

                        }

                    }

                }
                else if ((attachee.GetNameId() == 8859))
                {
                    // zert ghost
                    if ((attachee.GetMap() == 5121))
                    {
                        // verbo exterior - around castle
                        if ((!GameSystems.Combat.IsCombatActive()))
                        {
                            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                            {
                                if ((is_cool_to_talk(attachee, obj)))
                                {
                                    attachee.FloatLine(2010, triggerer);
                                    // zert ghost screeches!
                                    DetachScript();
                                }

                            }

                        }

                    }

                }
                else if ((attachee.GetNameId() == 8860))
                {
                    // kobort ghost
                    if ((attachee.GetMap() == 5143))
                    {
                        // castle level 1 - basement
                        if ((!GameSystems.Combat.IsCombatActive()))
                        {
                            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                            {
                                if ((is_better_to_talk(attachee, obj)))
                                {
                                    attachee.FloatLine(3010, triggerer);
                                    // kobort ghost moans!
                                    DetachScript();
                                }

                            }

                        }

                    }

                }
                else if ((attachee.GetNameId() == 14819))
                {
                    attachee.Destroy();
                }

            }

            return RunDefault;
        }
        public static bool is_better_to_talk(GameObject speaker, GameObject listener)
        {
            if ((speaker.DistanceTo(listener) <= 10))
            {
                return true;
            }

            return false;
        }
        public static bool is_cool_to_talk(GameObject speaker, GameObject listener)
        {
            if ((speaker.DistanceTo(listener) <= 25))
            {
                return true;
            }

            return false;
        }
        public static bool go_ghost(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetNameId() == 8699))
            {
                // turuko ghost
                if ((attachee.GetMap() == 5146))
                {
                    // castle level 4 - upper hall
                    if ((GetGlobalVar(696) == 7))
                    {
                        SetGlobalVar(696, 8);
                        // increment var to turuko off
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                        // turn turuko ghost off
                        AttachParticles("hit-SUBDUAL-medium", attachee);
                        // play particles
                        Sound(4114, 1);
                    }
                    else
                    {
                        // play sound
                        SetGlobalVar(696, 2);
                        // increment var to turuko off
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                        // turn turuko ghost off
                        AttachParticles("hit-SUBDUAL-medium", attachee);
                        // play particles
                        Sound(4114, 1);
                        // play sound
                        SetGlobalFlag(869, false);
                    }

                }

            }
            // castle sleep impossible flag unset
            else if ((attachee.GetNameId() == 8859))
            {
                // zert ghost
                if ((attachee.GetMap() == 5121))
                {
                    // verbo exterior - around castle
                    SetGlobalVar(696, 4);
                    // increment var to zert off
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                    // turn zert ghost off
                    AttachParticles("hit-SUBDUAL-medium", attachee);
                    // play particles
                    Sound(4114, 1);
                }

            }
            // play sound
            else if ((attachee.GetNameId() == 8860))
            {
                // kobort ghost
                if ((attachee.GetMap() == 5143))
                {
                    // castle level 1 - basement
                    SetGlobalVar(696, 6);
                    // increment var to kobort off
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                    // turn kobort ghost off
                    AttachParticles("hit-SUBDUAL-medium", attachee);
                    // play particles
                    Sound(4114, 1);
                    // play sound
                    SetGlobalFlag(869, false);
                }

            }

            // castle sleep impossible flag unset
            return RunDefault;
        }
        public static bool dump_parts(GameObject attachee, GameObject triggerer)
        {
            Utilities.party_transfer_to(attachee, 12612);
            Utilities.party_transfer_to(attachee, 12614);
            Utilities.party_transfer_to(attachee, 12616);
            return RunDefault;
        }
        public static bool undead_legion(GameObject attachee, GameObject triggerer)
        {
            var q01 = GameSystems.MapObject.CreateObject(14662, new locXY(732, 393));
            q01.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(q01, triggerer));
            var q02 = GameSystems.MapObject.CreateObject(14663, new locXY(736, 393));
            q02.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(q02, triggerer));
            var q03 = GameSystems.MapObject.CreateObject(14663, new locXY(740, 393));
            q03.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(q03, triggerer));
            var q04 = GameSystems.MapObject.CreateObject(14663, new locXY(744, 393));
            q04.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(q04, triggerer));
            var q05 = GameSystems.MapObject.CreateObject(14663, new locXY(748, 397));
            q05.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(q05, triggerer));
            var q06 = GameSystems.MapObject.CreateObject(14662, new locXY(740, 389));
            q06.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(q06, triggerer));
            var q07 = GameSystems.MapObject.CreateObject(14663, new locXY(732, 389));
            q07.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(q07, triggerer));
            var q08 = GameSystems.MapObject.CreateObject(14663, new locXY(728, 389));
            q08.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(q08, triggerer));
            var q09 = GameSystems.MapObject.CreateObject(14663, new locXY(724, 389));
            q09.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(q09, triggerer));
            var q10 = GameSystems.MapObject.CreateObject(14663, new locXY(720, 389));
            q10.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(q10, triggerer));
            var q11 = GameSystems.MapObject.CreateObject(14662, new locXY(716, 389));
            q11.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(q11, triggerer));
            var q12 = GameSystems.MapObject.CreateObject(14663, new locXY(712, 389));
            q12.Rotation = 0.5f;
            StartTimer(30000, () => bye_bye(q12, triggerer));
            var q13 = GameSystems.MapObject.CreateObject(14663, new locXY(708, 389));
            q13.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(q13, triggerer));
            var q14 = GameSystems.MapObject.CreateObject(14663, new locXY(704, 389));
            q14.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(q14, triggerer));
            var q15 = GameSystems.MapObject.CreateObject(14663, new locXY(700, 389));
            q15.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(q15, triggerer));
            var q16 = GameSystems.MapObject.CreateObject(14662, new locXY(704, 393));
            q16.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(q16, triggerer));
            var q17 = GameSystems.MapObject.CreateObject(14663, new locXY(696, 389));
            q17.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(q17, triggerer));
            var q18 = GameSystems.MapObject.CreateObject(14663, new locXY(732, 385));
            q18.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(q18, triggerer));
            var q19 = GameSystems.MapObject.CreateObject(14663, new locXY(728, 385));
            q19.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(q19, triggerer));
            var q20 = GameSystems.MapObject.CreateObject(14663, new locXY(724, 385));
            q20.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(q20, triggerer));
            var q21 = GameSystems.MapObject.CreateObject(14662, new locXY(720, 385));
            q21.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(q21, triggerer));
            var q22 = GameSystems.MapObject.CreateObject(14663, new locXY(716, 385));
            q22.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(q22, triggerer));
            var q23 = GameSystems.MapObject.CreateObject(14663, new locXY(712, 385));
            q23.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(q23, triggerer));
            var q24 = GameSystems.MapObject.CreateObject(14663, new locXY(708, 385));
            q24.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(q24, triggerer));
            var q25 = GameSystems.MapObject.CreateObject(14663, new locXY(704, 385));
            q25.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(q25, triggerer));
            var q26 = GameSystems.MapObject.CreateObject(14662, new locXY(700, 385));
            q26.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(q26, triggerer));
            var q27 = GameSystems.MapObject.CreateObject(14663, new locXY(696, 385));
            q27.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(q27, triggerer));
            var q28 = GameSystems.MapObject.CreateObject(14663, new locXY(692, 385));
            q28.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(q28, triggerer));
            var q29 = GameSystems.MapObject.CreateObject(14663, new locXY(744, 389));
            q29.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(q29, triggerer));
            var q30 = GameSystems.MapObject.CreateObject(14663, new locXY(748, 393));
            q30.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(q30, triggerer));
            var r01 = GameSystems.MapObject.CreateObject(14662, new locXY(680, 449));
            r01.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(r01, triggerer));
            var r02 = GameSystems.MapObject.CreateObject(14663, new locXY(680, 453));
            r02.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(r02, triggerer));
            var r03 = GameSystems.MapObject.CreateObject(14663, new locXY(680, 457));
            r03.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(r03, triggerer));
            var r04 = GameSystems.MapObject.CreateObject(14663, new locXY(680, 461));
            r04.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(r04, triggerer));
            var r05 = GameSystems.MapObject.CreateObject(14663, new locXY(680, 465));
            r05.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(r05, triggerer));
            var r06 = GameSystems.MapObject.CreateObject(14662, new locXY(680, 469));
            r06.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(r06, triggerer));
            var r07 = GameSystems.MapObject.CreateObject(14663, new locXY(680, 473));
            r07.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(r07, triggerer));
            var r08 = GameSystems.MapObject.CreateObject(14663, new locXY(740, 409));
            r08.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(r08, triggerer));
            var r09 = GameSystems.MapObject.CreateObject(14663, new locXY(740, 413));
            r09.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(r09, triggerer));
            var r10 = GameSystems.MapObject.CreateObject(14663, new locXY(752, 409));
            r10.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(r10, triggerer));
            var r11 = GameSystems.MapObject.CreateObject(14662, new locXY(748, 405));
            r11.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(r11, triggerer));
            var r12 = GameSystems.MapObject.CreateObject(14663, new locXY(752, 405));
            r12.Rotation = 0.5f;
            StartTimer(30000, () => bye_bye(r12, triggerer));
            var r13 = GameSystems.MapObject.CreateObject(14663, new locXY(752, 401));
            r13.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(r13, triggerer));
            var r14 = GameSystems.MapObject.CreateObject(14663, new locXY(736, 401));
            r14.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(r14, triggerer));
            var r15 = GameSystems.MapObject.CreateObject(14663, new locXY(732, 401));
            r15.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(r15, triggerer));
            var r16 = GameSystems.MapObject.CreateObject(14662, new locXY(728, 401));
            r16.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(r16, triggerer));
            var r17 = GameSystems.MapObject.CreateObject(14663, new locXY(724, 401));
            r17.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(r17, triggerer));
            var r18 = GameSystems.MapObject.CreateObject(14663, new locXY(732, 397));
            r18.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(r18, triggerer));
            var r19 = GameSystems.MapObject.CreateObject(14663, new locXY(728, 397));
            r19.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(r19, triggerer));
            var r20 = GameSystems.MapObject.CreateObject(14663, new locXY(724, 397));
            r20.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(r20, triggerer));
            var r21 = GameSystems.MapObject.CreateObject(14662, new locXY(720, 397));
            r21.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(r21, triggerer));
            var r22 = GameSystems.MapObject.CreateObject(14663, new locXY(716, 397));
            r22.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(r22, triggerer));
            var r23 = GameSystems.MapObject.CreateObject(14663, new locXY(720, 401));
            r23.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(r23, triggerer));
            var r24 = GameSystems.MapObject.CreateObject(14663, new locXY(712, 397));
            r24.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(r24, triggerer));
            var r25 = GameSystems.MapObject.CreateObject(14663, new locXY(728, 393));
            r25.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(r25, triggerer));
            var r26 = GameSystems.MapObject.CreateObject(14662, new locXY(724, 393));
            r26.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(r26, triggerer));
            var r27 = GameSystems.MapObject.CreateObject(14663, new locXY(720, 393));
            r27.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(r27, triggerer));
            var r28 = GameSystems.MapObject.CreateObject(14663, new locXY(716, 393));
            r28.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(r28, triggerer));
            var r29 = GameSystems.MapObject.CreateObject(14663, new locXY(712, 393));
            r29.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(r29, triggerer));
            var r30 = GameSystems.MapObject.CreateObject(14663, new locXY(708, 393));
            r30.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(r30, triggerer));
            var s01 = GameSystems.MapObject.CreateObject(14662, new locXY(664, 417));
            s01.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(s01, triggerer));
            var s02 = GameSystems.MapObject.CreateObject(14663, new locXY(664, 421));
            s02.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(s02, triggerer));
            var s03 = GameSystems.MapObject.CreateObject(14663, new locXY(664, 425));
            s03.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(s03, triggerer));
            var s04 = GameSystems.MapObject.CreateObject(14663, new locXY(664, 429));
            s04.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(s04, triggerer));
            var s05 = GameSystems.MapObject.CreateObject(14663, new locXY(664, 437));
            s05.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(s05, triggerer));
            var s06 = GameSystems.MapObject.CreateObject(14662, new locXY(664, 441));
            s06.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(s06, triggerer));
            var s07 = GameSystems.MapObject.CreateObject(14663, new locXY(664, 449));
            s07.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(s07, triggerer));
            var s08 = GameSystems.MapObject.CreateObject(14663, new locXY(664, 453));
            s08.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(s08, triggerer));
            var s09 = GameSystems.MapObject.CreateObject(14663, new locXY(664, 457));
            s09.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(s09, triggerer));
            var s10 = GameSystems.MapObject.CreateObject(14663, new locXY(664, 461));
            s10.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(s10, triggerer));
            var s11 = GameSystems.MapObject.CreateObject(14662, new locXY(664, 465));
            s11.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(s11, triggerer));
            var s12 = GameSystems.MapObject.CreateObject(14663, new locXY(664, 469));
            s12.Rotation = 0.5f;
            StartTimer(30000, () => bye_bye(s12, triggerer));
            var s13 = GameSystems.MapObject.CreateObject(14663, new locXY(660, 421));
            s13.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(s13, triggerer));
            var s14 = GameSystems.MapObject.CreateObject(14663, new locXY(660, 425));
            s14.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(s14, triggerer));
            var s15 = GameSystems.MapObject.CreateObject(14663, new locXY(660, 429));
            s15.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(s15, triggerer));
            var s16 = GameSystems.MapObject.CreateObject(14662, new locXY(660, 433));
            s16.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(s16, triggerer));
            var s17 = GameSystems.MapObject.CreateObject(14663, new locXY(660, 437));
            s17.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(s17, triggerer));
            var s18 = GameSystems.MapObject.CreateObject(14663, new locXY(660, 449));
            s18.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(s18, triggerer));
            var s19 = GameSystems.MapObject.CreateObject(14663, new locXY(660, 453));
            s19.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(s19, triggerer));
            var s20 = GameSystems.MapObject.CreateObject(14663, new locXY(660, 457));
            s20.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(s20, triggerer));
            var s21 = GameSystems.MapObject.CreateObject(14662, new locXY(660, 461));
            s21.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(s21, triggerer));
            var s22 = GameSystems.MapObject.CreateObject(14663, new locXY(660, 465));
            s22.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(s22, triggerer));
            var s23 = GameSystems.MapObject.CreateObject(14663, new locXY(656, 425));
            s23.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(s23, triggerer));
            var s24 = GameSystems.MapObject.CreateObject(14663, new locXY(656, 429));
            s24.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(s24, triggerer));
            var s25 = GameSystems.MapObject.CreateObject(14663, new locXY(656, 433));
            s25.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(s25, triggerer));
            var s26 = GameSystems.MapObject.CreateObject(14662, new locXY(656, 437));
            s26.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(s26, triggerer));
            var s27 = GameSystems.MapObject.CreateObject(14663, new locXY(656, 441));
            s27.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(s27, triggerer));
            var s28 = GameSystems.MapObject.CreateObject(14663, new locXY(656, 445));
            s28.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(s28, triggerer));
            var s29 = GameSystems.MapObject.CreateObject(14663, new locXY(656, 449));
            s29.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(s29, triggerer));
            var s30 = GameSystems.MapObject.CreateObject(14663, new locXY(656, 457));
            s30.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(s30, triggerer));
            var t01 = GameSystems.MapObject.CreateObject(14662, new locXY(656, 461));
            t01.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(t01, triggerer));
            var t02 = GameSystems.MapObject.CreateObject(14663, new locXY(676, 445));
            t02.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(t02, triggerer));
            var t03 = GameSystems.MapObject.CreateObject(14663, new locXY(676, 449));
            t03.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(t03, triggerer));
            var t04 = GameSystems.MapObject.CreateObject(14663, new locXY(676, 453));
            t04.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(t04, triggerer));
            var t05 = GameSystems.MapObject.CreateObject(14663, new locXY(676, 457));
            t05.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(t05, triggerer));
            var t06 = GameSystems.MapObject.CreateObject(14662, new locXY(676, 461));
            t06.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(t06, triggerer));
            var t07 = GameSystems.MapObject.CreateObject(14663, new locXY(676, 465));
            t07.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(t07, triggerer));
            var t08 = GameSystems.MapObject.CreateObject(14663, new locXY(676, 469));
            t08.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(t08, triggerer));
            var t09 = GameSystems.MapObject.CreateObject(14663, new locXY(676, 473));
            t09.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(t09, triggerer));
            var t10 = GameSystems.MapObject.CreateObject(14663, new locXY(680, 433));
            t10.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(t10, triggerer));
            var t11 = GameSystems.MapObject.CreateObject(14662, new locXY(676, 433));
            t11.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(t11, triggerer));
            var t12 = GameSystems.MapObject.CreateObject(14663, new locXY(672, 433));
            t12.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(t12, triggerer));
            var t13 = GameSystems.MapObject.CreateObject(14663, new locXY(672, 437));
            t13.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(t13, triggerer));
            var t14 = GameSystems.MapObject.CreateObject(14663, new locXY(672, 441));
            t14.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(t14, triggerer));
            var t15 = GameSystems.MapObject.CreateObject(14663, new locXY(672, 445));
            t15.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(t15, triggerer));
            var t16 = GameSystems.MapObject.CreateObject(14662, new locXY(672, 449));
            t16.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(t16, triggerer));
            var t17 = GameSystems.MapObject.CreateObject(14663, new locXY(672, 453));
            t17.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(t17, triggerer));
            var t18 = GameSystems.MapObject.CreateObject(14663, new locXY(672, 457));
            t18.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(t18, triggerer));
            var t19 = GameSystems.MapObject.CreateObject(14663, new locXY(672, 461));
            t19.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(t19, triggerer));
            var t20 = GameSystems.MapObject.CreateObject(14663, new locXY(672, 465));
            t20.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(t20, triggerer));
            var t21 = GameSystems.MapObject.CreateObject(14662, new locXY(672, 469));
            t21.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(t21, triggerer));
            var t22 = GameSystems.MapObject.CreateObject(14663, new locXY(672, 473));
            t22.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(t22, triggerer));
            var t23 = GameSystems.MapObject.CreateObject(14663, new locXY(668, 421));
            t23.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(t23, triggerer));
            var t24 = GameSystems.MapObject.CreateObject(14663, new locXY(668, 425));
            t24.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(t24, triggerer));
            var t25 = GameSystems.MapObject.CreateObject(14663, new locXY(668, 429));
            t25.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(t25, triggerer));
            var t26 = GameSystems.MapObject.CreateObject(14662, new locXY(668, 441));
            t26.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(t26, triggerer));
            var t27 = GameSystems.MapObject.CreateObject(14663, new locXY(668, 445));
            t27.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(t27, triggerer));
            var t28 = GameSystems.MapObject.CreateObject(14663, new locXY(668, 449));
            t28.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(t28, triggerer));
            var t29 = GameSystems.MapObject.CreateObject(14663, new locXY(668, 461));
            t29.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(t29, triggerer));
            var t30 = GameSystems.MapObject.CreateObject(14663, new locXY(668, 473));
            t30.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(t30, triggerer));
            var u01 = GameSystems.MapObject.CreateObject(14662, new locXY(664, 413));
            u01.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(u01, triggerer));
            var u02 = GameSystems.MapObject.CreateObject(14663, new locXY(740, 417));
            u02.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(u02, triggerer));
            var u03 = GameSystems.MapObject.CreateObject(14663, new locXY(740, 421));
            u03.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(u03, triggerer));
            var u04 = GameSystems.MapObject.CreateObject(14663, new locXY(740, 425));
            u04.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(u04, triggerer));
            var u05 = GameSystems.MapObject.CreateObject(14663, new locXY(740, 429));
            u05.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(u05, triggerer));
            var u06 = GameSystems.MapObject.CreateObject(14662, new locXY(740, 433));
            u06.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(u06, triggerer));
            var u07 = GameSystems.MapObject.CreateObject(14663, new locXY(740, 437));
            u07.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(u07, triggerer));
            var u08 = GameSystems.MapObject.CreateObject(14663, new locXY(740, 441));
            u08.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(u08, triggerer));
            var u09 = GameSystems.MapObject.CreateObject(14663, new locXY(740, 445));
            u09.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(u09, triggerer));
            var u10 = GameSystems.MapObject.CreateObject(14663, new locXY(740, 449));
            u10.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(u10, triggerer));
            var u11 = GameSystems.MapObject.CreateObject(14662, new locXY(740, 453));
            u11.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(u11, triggerer));
            var u12 = GameSystems.MapObject.CreateObject(14663, new locXY(740, 457));
            u12.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(u12, triggerer));
            var u13 = GameSystems.MapObject.CreateObject(14663, new locXY(744, 413));
            u13.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(u13, triggerer));
            var u14 = GameSystems.MapObject.CreateObject(14663, new locXY(744, 417));
            u14.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(u14, triggerer));
            var u15 = GameSystems.MapObject.CreateObject(14663, new locXY(744, 421));
            u15.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(u15, triggerer));
            var u16 = GameSystems.MapObject.CreateObject(14662, new locXY(744, 425));
            u16.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(u16, triggerer));
            var u17 = GameSystems.MapObject.CreateObject(14663, new locXY(744, 429));
            u17.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(u17, triggerer));
            var u18 = GameSystems.MapObject.CreateObject(14663, new locXY(744, 433));
            u18.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(u18, triggerer));
            var u19 = GameSystems.MapObject.CreateObject(14663, new locXY(744, 437));
            u19.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(u19, triggerer));
            var u20 = GameSystems.MapObject.CreateObject(14663, new locXY(744, 441));
            u20.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(u20, triggerer));
            var u21 = GameSystems.MapObject.CreateObject(14662, new locXY(744, 445));
            u21.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(u21, triggerer));
            var u22 = GameSystems.MapObject.CreateObject(14663, new locXY(748, 413));
            u22.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(u22, triggerer));
            var u23 = GameSystems.MapObject.CreateObject(14663, new locXY(748, 417));
            u23.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(u23, triggerer));
            var u24 = GameSystems.MapObject.CreateObject(14663, new locXY(748, 421));
            u24.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(u24, triggerer));
            var u25 = GameSystems.MapObject.CreateObject(14663, new locXY(748, 425));
            u25.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(u25, triggerer));
            var u26 = GameSystems.MapObject.CreateObject(14662, new locXY(748, 429));
            u26.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(u26, triggerer));
            var u27 = GameSystems.MapObject.CreateObject(14663, new locXY(748, 433));
            u27.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(u27, triggerer));
            var u28 = GameSystems.MapObject.CreateObject(14663, new locXY(748, 437));
            u28.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(u28, triggerer));
            var u29 = GameSystems.MapObject.CreateObject(14663, new locXY(748, 441));
            u29.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(u29, triggerer));
            var u30 = GameSystems.MapObject.CreateObject(14663, new locXY(752, 413));
            u30.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(u30, triggerer));
            var v01 = GameSystems.MapObject.CreateObject(14662, new locXY(752, 417));
            v01.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(v01, triggerer));
            var v02 = GameSystems.MapObject.CreateObject(14663, new locXY(752, 421));
            v02.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(v02, triggerer));
            var v03 = GameSystems.MapObject.CreateObject(14663, new locXY(752, 425));
            v03.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(v03, triggerer));
            var v04 = GameSystems.MapObject.CreateObject(14663, new locXY(752, 429));
            v04.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(v04, triggerer));
            var v05 = GameSystems.MapObject.CreateObject(14663, new locXY(752, 433));
            v05.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(v05, triggerer));
            var v06 = GameSystems.MapObject.CreateObject(14662, new locXY(752, 437));
            v06.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(v06, triggerer));
            var v07 = GameSystems.MapObject.CreateObject(14663, new locXY(752, 441));
            v07.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(v07, triggerer));
            var v08 = GameSystems.MapObject.CreateObject(14663, new locXY(736, 417));
            v08.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(v08, triggerer));
            var v09 = GameSystems.MapObject.CreateObject(14663, new locXY(736, 429));
            v09.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(v09, triggerer));
            var v10 = GameSystems.MapObject.CreateObject(14663, new locXY(736, 433));
            v10.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(v10, triggerer));
            var v11 = GameSystems.MapObject.CreateObject(14662, new locXY(736, 437));
            v11.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(v11, triggerer));
            var v12 = GameSystems.MapObject.CreateObject(14663, new locXY(736, 441));
            v12.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(v12, triggerer));
            var v13 = GameSystems.MapObject.CreateObject(14663, new locXY(736, 445));
            v13.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(v13, triggerer));
            var v14 = GameSystems.MapObject.CreateObject(14663, new locXY(736, 449));
            v14.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(v14, triggerer));
            var v15 = GameSystems.MapObject.CreateObject(14663, new locXY(736, 453));
            v15.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(v15, triggerer));
            var v16 = GameSystems.MapObject.CreateObject(14662, new locXY(736, 457));
            v16.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(v16, triggerer));
            var v17 = GameSystems.MapObject.CreateObject(14663, new locXY(736, 461));
            v17.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(v17, triggerer));
            var v18 = GameSystems.MapObject.CreateObject(14663, new locXY(732, 441));
            v18.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(v18, triggerer));
            var v19 = GameSystems.MapObject.CreateObject(14663, new locXY(732, 445));
            v19.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(v19, triggerer));
            var v20 = GameSystems.MapObject.CreateObject(14663, new locXY(732, 449));
            v20.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(v20, triggerer));
            var v21 = GameSystems.MapObject.CreateObject(14662, new locXY(732, 453));
            v21.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(v21, triggerer));
            var v22 = GameSystems.MapObject.CreateObject(14663, new locXY(732, 457));
            v22.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(v22, triggerer));
            var v23 = GameSystems.MapObject.CreateObject(14663, new locXY(732, 461));
            v23.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(v23, triggerer));
            var v24 = GameSystems.MapObject.CreateObject(14663, new locXY(732, 465));
            v24.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(v24, triggerer));
            var v25 = GameSystems.MapObject.CreateObject(14663, new locXY(732, 469));
            v25.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(v25, triggerer));
            var v26 = GameSystems.MapObject.CreateObject(14662, new locXY(732, 473));
            v26.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(v26, triggerer));
            var v27 = GameSystems.MapObject.CreateObject(14663, new locXY(728, 445));
            v27.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(v27, triggerer));
            var v28 = GameSystems.MapObject.CreateObject(14663, new locXY(728, 449));
            v28.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(v28, triggerer));
            var v29 = GameSystems.MapObject.CreateObject(14663, new locXY(728, 453));
            v29.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(v29, triggerer));
            var v30 = GameSystems.MapObject.CreateObject(14663, new locXY(728, 457));
            v30.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(v30, triggerer));
            var w01 = GameSystems.MapObject.CreateObject(14662, new locXY(728, 461));
            w01.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(w01, triggerer));
            var w02 = GameSystems.MapObject.CreateObject(14663, new locXY(728, 465));
            w02.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(w02, triggerer));
            var w03 = GameSystems.MapObject.CreateObject(14663, new locXY(724, 445));
            w03.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(w03, triggerer));
            var w04 = GameSystems.MapObject.CreateObject(14663, new locXY(724, 449));
            w04.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(w04, triggerer));
            var w05 = GameSystems.MapObject.CreateObject(14663, new locXY(724, 453));
            w05.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(w05, triggerer));
            var w06 = GameSystems.MapObject.CreateObject(14662, new locXY(724, 457));
            w06.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(w06, triggerer));
            var w07 = GameSystems.MapObject.CreateObject(14663, new locXY(724, 461));
            w07.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(w07, triggerer));
            var w08 = GameSystems.MapObject.CreateObject(14663, new locXY(724, 465));
            w08.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(w08, triggerer));
            var w09 = GameSystems.MapObject.CreateObject(14663, new locXY(720, 449));
            w09.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(w09, triggerer));
            var w10 = GameSystems.MapObject.CreateObject(14663, new locXY(720, 453));
            w10.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(w10, triggerer));
            var w11 = GameSystems.MapObject.CreateObject(14662, new locXY(720, 457));
            w11.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(w11, triggerer));
            var w12 = GameSystems.MapObject.CreateObject(14663, new locXY(720, 461));
            w12.Rotation = 0.5f;
            StartTimer(30000, () => bye_bye(w12, triggerer));
            var w13 = GameSystems.MapObject.CreateObject(14663, new locXY(720, 465));
            w13.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(w13, triggerer));
            var w14 = GameSystems.MapObject.CreateObject(14663, new locXY(720, 469));
            w14.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(w14, triggerer));
            var w15 = GameSystems.MapObject.CreateObject(14663, new locXY(716, 449));
            w15.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(w15, triggerer));
            var w16 = GameSystems.MapObject.CreateObject(14662, new locXY(716, 453));
            w16.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(w16, triggerer));
            var w17 = GameSystems.MapObject.CreateObject(14663, new locXY(716, 457));
            w17.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(w17, triggerer));
            var w18 = GameSystems.MapObject.CreateObject(14663, new locXY(716, 461));
            w18.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(w18, triggerer));
            var w19 = GameSystems.MapObject.CreateObject(14663, new locXY(716, 465));
            w19.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(w19, triggerer));
            var w20 = GameSystems.MapObject.CreateObject(14663, new locXY(716, 469));
            w20.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(w20, triggerer));
            var w21 = GameSystems.MapObject.CreateObject(14662, new locXY(716, 473));
            w21.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(w21, triggerer));
            var w22 = GameSystems.MapObject.CreateObject(14663, new locXY(716, 477));
            w22.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(w22, triggerer));
            var w23 = GameSystems.MapObject.CreateObject(14663, new locXY(716, 481));
            w23.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(w23, triggerer));
            var w24 = GameSystems.MapObject.CreateObject(14663, new locXY(716, 485));
            w24.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(w24, triggerer));
            var w25 = GameSystems.MapObject.CreateObject(14663, new locXY(716, 489));
            w25.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(w25, triggerer));
            var w26 = GameSystems.MapObject.CreateObject(14662, new locXY(716, 493));
            w26.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(w26, triggerer));
            var w27 = GameSystems.MapObject.CreateObject(14663, new locXY(716, 497));
            w27.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(w27, triggerer));
            var w28 = GameSystems.MapObject.CreateObject(14663, new locXY(712, 453));
            w28.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(w28, triggerer));
            var w29 = GameSystems.MapObject.CreateObject(14663, new locXY(712, 457));
            w29.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(w29, triggerer));
            var w30 = GameSystems.MapObject.CreateObject(14663, new locXY(712, 461));
            w30.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(w30, triggerer));
            var x01 = GameSystems.MapObject.CreateObject(14662, new locXY(712, 465));
            x01.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(x01, triggerer));
            var x02 = GameSystems.MapObject.CreateObject(14663, new locXY(708, 453));
            x02.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(x02, triggerer));
            var x03 = GameSystems.MapObject.CreateObject(14663, new locXY(712, 473));
            x03.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(x03, triggerer));
            var x04 = GameSystems.MapObject.CreateObject(14663, new locXY(712, 477));
            x04.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(x04, triggerer));
            var x05 = GameSystems.MapObject.CreateObject(14663, new locXY(712, 481));
            x05.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(x05, triggerer));
            var x06 = GameSystems.MapObject.CreateObject(14662, new locXY(712, 485));
            x06.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(x06, triggerer));
            var x07 = GameSystems.MapObject.CreateObject(14663, new locXY(712, 489));
            x07.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(x07, triggerer));
            var x08 = GameSystems.MapObject.CreateObject(14663, new locXY(712, 493));
            x08.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(x08, triggerer));
            var x09 = GameSystems.MapObject.CreateObject(14663, new locXY(712, 497));
            x09.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(x09, triggerer));
            var x10 = GameSystems.MapObject.CreateObject(14663, new locXY(712, 501));
            x10.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(x10, triggerer));
            var x11 = GameSystems.MapObject.CreateObject(14662, new locXY(708, 457));
            x11.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(x11, triggerer));
            var x12 = GameSystems.MapObject.CreateObject(14663, new locXY(708, 461));
            x12.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(x12, triggerer));
            var x13 = GameSystems.MapObject.CreateObject(14663, new locXY(708, 469));
            x13.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(x13, triggerer));
            var x14 = GameSystems.MapObject.CreateObject(14663, new locXY(708, 473));
            x14.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(x14, triggerer));
            var x15 = GameSystems.MapObject.CreateObject(14663, new locXY(708, 477));
            x15.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(x15, triggerer));
            var x16 = GameSystems.MapObject.CreateObject(14662, new locXY(708, 481));
            x16.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(x16, triggerer));
            var x17 = GameSystems.MapObject.CreateObject(14663, new locXY(708, 485));
            x17.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(x17, triggerer));
            var x18 = GameSystems.MapObject.CreateObject(14663, new locXY(708, 489));
            x18.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(x18, triggerer));
            var x19 = GameSystems.MapObject.CreateObject(14663, new locXY(708, 493));
            x19.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(x19, triggerer));
            var x20 = GameSystems.MapObject.CreateObject(14663, new locXY(708, 497));
            x20.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(x20, triggerer));
            var x21 = GameSystems.MapObject.CreateObject(14662, new locXY(704, 457));
            x21.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(x21, triggerer));
            var x22 = GameSystems.MapObject.CreateObject(14663, new locXY(704, 461));
            x22.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(x22, triggerer));
            var x23 = GameSystems.MapObject.CreateObject(14663, new locXY(704, 469));
            x23.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(x23, triggerer));
            var x24 = GameSystems.MapObject.CreateObject(14663, new locXY(704, 473));
            x24.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(x24, triggerer));
            var x25 = GameSystems.MapObject.CreateObject(14663, new locXY(704, 477));
            x25.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(x25, triggerer));
            var x26 = GameSystems.MapObject.CreateObject(14662, new locXY(704, 481));
            x26.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(x26, triggerer));
            var x27 = GameSystems.MapObject.CreateObject(14663, new locXY(704, 485));
            x27.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(x27, triggerer));
            var x28 = GameSystems.MapObject.CreateObject(14663, new locXY(704, 489));
            x28.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(x28, triggerer));
            var x29 = GameSystems.MapObject.CreateObject(14663, new locXY(705, 493));
            x29.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(x29, triggerer));
            var x30 = GameSystems.MapObject.CreateObject(14663, new locXY(748, 409));
            x30.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(x30, triggerer));
            var y01 = GameSystems.MapObject.CreateObject(14662, new locXY(744, 409));
            y01.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(y01, triggerer));
            var y02 = GameSystems.MapObject.CreateObject(14663, new locXY(720, 485));
            y02.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(y02, triggerer));
            var y03 = GameSystems.MapObject.CreateObject(14663, new locXY(720, 489));
            y03.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(y03, triggerer));
            var y04 = GameSystems.MapObject.CreateObject(14663, new locXY(720, 493));
            y04.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(y04, triggerer));
            var y05 = GameSystems.MapObject.CreateObject(14663, new locXY(724, 493));
            y05.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(y05, triggerer));
            var y06 = GameSystems.MapObject.CreateObject(14662, new locXY(728, 493));
            y06.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(y06, triggerer));
            var y07 = GameSystems.MapObject.CreateObject(14663, new locXY(732, 489));
            y07.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(y07, triggerer));
            var y08 = GameSystems.MapObject.CreateObject(14663, new locXY(736, 489));
            y08.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(y08, triggerer));
            var y09 = GameSystems.MapObject.CreateObject(14663, new locXY(740, 489));
            y09.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(y09, triggerer));
            var y10 = GameSystems.MapObject.CreateObject(14663, new locXY(740, 485));
            y10.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(y10, triggerer));
            var y11 = GameSystems.MapObject.CreateObject(14662, new locXY(736, 481));
            y11.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(y11, triggerer));
            var y12 = GameSystems.MapObject.CreateObject(14663, new locXY(736, 477));
            y12.Rotation = 0.5f;
            StartTimer(30000, () => bye_bye(y12, triggerer));
            var y13 = GameSystems.MapObject.CreateObject(14663, new locXY(744, 485));
            y13.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(y13, triggerer));
            var y14 = GameSystems.MapObject.CreateObject(14663, new locXY(748, 481));
            y14.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(y14, triggerer));
            var y15 = GameSystems.MapObject.CreateObject(14663, new locXY(752, 481));
            y15.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(y15, triggerer));
            var y16 = GameSystems.MapObject.CreateObject(14662, new locXY(752, 477));
            y16.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(y16, triggerer));
            var y17 = GameSystems.MapObject.CreateObject(14663, new locXY(756, 477));
            y17.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(y17, triggerer));
            var y18 = GameSystems.MapObject.CreateObject(14663, new locXY(756, 473));
            y18.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(y18, triggerer));
            var y19 = GameSystems.MapObject.CreateObject(14663, new locXY(760, 469));
            y19.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(y19, triggerer));
            var y20 = GameSystems.MapObject.CreateObject(14663, new locXY(760, 465));
            y20.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(y20, triggerer));
            var y21 = GameSystems.MapObject.CreateObject(14662, new locXY(756, 465));
            y21.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(y21, triggerer));
            var y22 = GameSystems.MapObject.CreateObject(14663, new locXY(748, 461));
            y22.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(y22, triggerer));
            var y23 = GameSystems.MapObject.CreateObject(14663, new locXY(756, 425));
            y23.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(y23, triggerer));
            var y24 = GameSystems.MapObject.CreateObject(14663, new locXY(756, 429));
            y24.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(y24, triggerer));
            var y25 = GameSystems.MapObject.CreateObject(14663, new locXY(756, 433));
            y25.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(y25, triggerer));
            var y26 = GameSystems.MapObject.CreateObject(14662, new locXY(756, 437));
            y25.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(y26, triggerer));
            var y27 = GameSystems.MapObject.CreateObject(14663, new locXY(756, 441));
            y27.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(y27, triggerer));
            var y28 = GameSystems.MapObject.CreateObject(14663, new locXY(756, 445));
            y28.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(y28, triggerer));
            var y29 = GameSystems.MapObject.CreateObject(14663, new locXY(756, 421));
            y29.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(y29, triggerer));
            var y30 = GameSystems.MapObject.CreateObject(14663, new locXY(760, 429));
            y30.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(y30, triggerer));
            var z01 = GameSystems.MapObject.CreateObject(14662, new locXY(760, 433));
            z01.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(z01, triggerer));
            var z02 = GameSystems.MapObject.CreateObject(14663, new locXY(760, 437));
            z02.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(z02, triggerer));
            var z03 = GameSystems.MapObject.CreateObject(14663, new locXY(760, 441));
            z03.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(z03, triggerer));
            var z04 = GameSystems.MapObject.CreateObject(14663, new locXY(760, 445));
            z04.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(z04, triggerer));
            var z05 = GameSystems.MapObject.CreateObject(14663, new locXY(764, 433));
            z05.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(z05, triggerer));
            var z06 = GameSystems.MapObject.CreateObject(14662, new locXY(764, 437));
            z06.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(z06, triggerer));
            var z07 = GameSystems.MapObject.CreateObject(14663, new locXY(764, 441));
            z07.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(z07, triggerer));
            var z08 = GameSystems.MapObject.CreateObject(14663, new locXY(764, 445));
            z08.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(z08, triggerer));
            var z09 = GameSystems.MapObject.CreateObject(14663, new locXY(764, 449));
            z09.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(z09, triggerer));
            var z10 = GameSystems.MapObject.CreateObject(14663, new locXY(764, 461));
            z10.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(z10, triggerer));
            var z11 = GameSystems.MapObject.CreateObject(14662, new locXY(764, 457));
            z11.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(z11, triggerer));
            var z12 = GameSystems.MapObject.CreateObject(14663, new locXY(768, 433));
            z12.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(z12, triggerer));
            var z13 = GameSystems.MapObject.CreateObject(14663, new locXY(768, 437));
            z13.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(z13, triggerer));
            var z14 = GameSystems.MapObject.CreateObject(14663, new locXY(768, 441));
            z14.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(z14, triggerer));
            var z15 = GameSystems.MapObject.CreateObject(14663, new locXY(768, 445));
            z15.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(z15, triggerer));
            var z16 = GameSystems.MapObject.CreateObject(14662, new locXY(768, 449));
            z16.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(z16, triggerer));
            var z17 = GameSystems.MapObject.CreateObject(14663, new locXY(768, 453));
            z17.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(z17, triggerer));
            var z18 = GameSystems.MapObject.CreateObject(14663, new locXY(772, 437));
            z18.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(z18, triggerer));
            var z19 = GameSystems.MapObject.CreateObject(14663, new locXY(772, 441));
            z19.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(z19, triggerer));
            var z20 = GameSystems.MapObject.CreateObject(14663, new locXY(688, 445));
            z20.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(z20, triggerer));
            var z21 = GameSystems.MapObject.CreateObject(14662, new locXY(684, 445));
            z21.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(z21, triggerer));
            var z22 = GameSystems.MapObject.CreateObject(14663, new locXY(684, 449));
            z22.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(z22, triggerer));
            var z23 = GameSystems.MapObject.CreateObject(14663, new locXY(684, 453));
            z23.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(z23, triggerer));
            var z24 = GameSystems.MapObject.CreateObject(14663, new locXY(684, 457));
            z24.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(z24, triggerer));
            var z25 = GameSystems.MapObject.CreateObject(14663, new locXY(692, 465));
            z25.Rotation = 1.5f;
            StartTimer(30300, () => bye_bye(z25, triggerer));
            var z26 = GameSystems.MapObject.CreateObject(14662, new locXY(692, 469));
            z26.Rotation = 2.5f;
            StartTimer(30300, () => bye_bye(z26, triggerer));
            var z27 = GameSystems.MapObject.CreateObject(14663, new locXY(692, 473));
            z27.Rotation = 3.5f;
            StartTimer(30300, () => bye_bye(z27, triggerer));
            var z28 = GameSystems.MapObject.CreateObject(14663, new locXY(688, 473));
            z28.Rotation = 4.5f;
            StartTimer(30300, () => bye_bye(z28, triggerer));
            var z29 = GameSystems.MapObject.CreateObject(14663, new locXY(688, 457));
            z29.Rotation = 5.5f;
            StartTimer(30300, () => bye_bye(z29, triggerer));
            var z30 = GameSystems.MapObject.CreateObject(14663, new locXY(680, 445));
            z30.Rotation = 0.5f;
            StartTimer(30300, () => bye_bye(z30, triggerer));
            Sound(4115, 1);
            SetGlobalVar(696, 9);
            SetGlobalFlag(869, false);
            SetQuestState(83, QuestState.Completed);
            return RunDefault;
        }
        public static bool bye_bye(GameObject attachee, GameObject triggerer)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }

    }
}
