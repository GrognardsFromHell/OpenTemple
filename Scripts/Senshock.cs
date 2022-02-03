
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
    [ObjectScript(142)]
    public class Senshock : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((Utilities.find_npc_near(attachee, 8032) != null))
            {
                return RunDefault;
            }

            if ((GetGlobalFlag(144)))
            {
                attachee.Attack(triggerer);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 110);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                SetGlobalVar(720, 0);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(147, true);
            return RunDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(779) == 0))
            {
                SetGlobalVar(779, 2);
                attachee.TurnTowards(PartyLeader);
            }
            else if ((GetGlobalVar(779) == 1 && GetGlobalFlag(823)))
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
                    SetGlobalVar(975, GetGlobalVar(975) + 1);
                    if ((GetGlobalVar(975) == 3 && !GetGlobalFlag(373) && !GetGlobalFlag(824) && GetGlobalVar(779) == 2 && !GetGlobalFlag(823)))
                    {
                        var barky_backup = GameSystems.MapObject.CreateObject(14226, new locXY(393, 545));
                        barky_backup.Move(new locXY(386, 535));
                        barky_backup.Rotation = 5.49778714378f;
                        Sound(4063, 1);
                        barky_backup.Unconceal();
                        var shocky = attachee.GetInitiative();
                        barky_backup.AddToInitiative();
                        barky_backup.SetInitiative(shocky);
                        UiSystems.Combat.Initiative.UpdateIfNeeded();
                        foreach (var obj in ObjList.ListVicinity(barky_backup.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            barky_backup.Attack(obj);
                        }

                        SetGlobalFlag(824, true);
                    }

                    if ((GetGlobalVar(975) == 3 && !GetGlobalFlag(374) && !GetGlobalFlag(825) && GetGlobalVar(779) == 2 && !GetGlobalFlag(823)))
                    {
                        var deggsy_backup = GameSystems.MapObject.CreateObject(14227, new locXY(393, 545));
                        deggsy_backup.Move(new locXY(386, 539));
                        deggsy_backup.Rotation = 5.49778714378f;
                        if ((GetGlobalFlag(373)))
                        {
                            Sound(4063, 1);
                        }

                        deggsy_backup.Unconceal();
                        var shocky = attachee.GetInitiative();
                        deggsy_backup.AddToInitiative();
                        deggsy_backup.SetInitiative(shocky);
                        UiSystems.Combat.Initiative.UpdateIfNeeded();
                        foreach (var obj in ObjList.ListVicinity(deggsy_backup.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            deggsy_backup.Attack(obj);
                        }

                        SetGlobalFlag(825, true);
                    }

                }

                if ((Utilities.obj_percent_hp(attachee) <= 25))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                    AttachParticles("sp-Teleport", attachee);
                    Sound(4035, 1);
                    SetGlobalFlag(990, true);
                }
                else if ((GetGlobalVar(743) == 15))
                {
                    attachee.SetInt(obj_f.critter_strategy, 451);
                    SetGlobalVar(743, GetGlobalVar(743) + 1);
                }
                else if ((GetGlobalVar(743) >= 16))
                {
                    attachee.SetInt(obj_f.critter_strategy, 452);
                }
                else
                {
                    attachee.SetInt(obj_f.critter_strategy, 461);
                    SetGlobalVar(743, GetGlobalVar(743) + 1);
                }

            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(147, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5080))
            {
                if ((GetGlobalFlag(147) || GetGlobalFlag(990)))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                    return SkipDefault;
                }
                else
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        var closest_jones = Utilities.party_closest(attachee);
                        if ((attachee.DistanceTo(closest_jones) <= 100))
                        {
                            SetGlobalVar(720, GetGlobalVar(720) + 1);
                            if ((attachee.GetLeader() == null))
                            {
                                if ((GetGlobalVar(720) == 4))
                                {
                                    attachee.CastSpell(WellKnownSpells.Stoneskin, attachee);
                                    attachee.PendingSpellsToMemorized();
                                }

                                if ((GetGlobalVar(720) == 8))
                                {
                                    attachee.CastSpell(WellKnownSpells.SeeInvisibility, attachee);
                                    attachee.PendingSpellsToMemorized();
                                }

                                if ((GetGlobalVar(720) == 12))
                                {
                                    attachee.CastSpell(WellKnownSpells.FalseLife, attachee);
                                    attachee.PendingSpellsToMemorized();
                                }

                                if ((GetGlobalVar(720) == 16))
                                {
                                    attachee.CastSpell(WellKnownSpells.MageArmor, attachee);
                                    attachee.PendingSpellsToMemorized();
                                }

                            }

                            if ((GetGlobalVar(720) >= 400))
                            {
                                SetGlobalVar(720, 0);
                            }

                        }

                        if ((!GetGlobalFlag(144)))
                        {
                            if ((!GetGlobalFlag(376)))
                            {
                                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                                {
                                    if ((is_22_and_under(attachee, obj)))
                                    {
                                        obj.TurnTowards(attachee);
                                        attachee.TurnTowards(obj);
                                        if ((Utilities.find_npc_near(attachee, 8035) != null))
                                        {
                                            var barky = Utilities.find_npc_near(attachee, 8035);
                                            barky.TurnTowards(obj);
                                        }

                                        if ((Utilities.find_npc_near(attachee, 8036) != null))
                                        {
                                            var deggy = Utilities.find_npc_near(attachee, 8036);
                                            deggy.TurnTowards(obj);
                                        }

                                        obj.BeginDialog(attachee, 1);
                                        SetGlobalFlag(376, true);
                                    }

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
        public static bool senshock_kills_hedrack(GameObject attachee, GameObject triggerer)
        {
            StartTimer(7200000, () => senshock_check_kill(attachee));
            return RunDefault;
        }
        public static bool senshock_check_kill(GameObject attachee)
        {
            if ((!GetGlobalFlag(146)))
            {
                if ((!GetGlobalFlag(147)))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                    SetGlobalFlag(146, true);
                }

            }

            return RunDefault;
        }
        public static bool is_22_and_under(GameObject speaker, GameObject listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 20))
                {
                    return true;
                }

            }

            return false;
        }

    }
}
