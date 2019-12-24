
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
    [ObjectScript(124)]
    public class Kelno : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                ScriptDaemon.record_time_stamp(516);
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 280);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(372)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                if ((attachee.GetLeader() == null) && !GameSystems.Combat.IsCombatActive())
                {
                    SetGlobalVar(715, 0);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(106, true);
            ScriptDaemon.record_time_stamp(458);
            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(346, false);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(106, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var (xx, yy) = attachee.GetLocation();
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if (xx == 545 && yy == 497) // Kelno is in his usual place
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((!attachee.HasMet(obj)))
                        {
                            if ((Utilities.is_safe_to_talk(attachee, obj)))
                            {
                                ScriptDaemon.record_time_stamp(516);
                                if (((GetGlobalFlag(104)) || (GetGlobalFlag(105)) || (GetGlobalFlag(107))))
                                {
                                    obj.TurnTowards(attachee); // added by Livonya
                                    attachee.TurnTowards(obj); // added by Livonya
                                    obj.BeginDialog(attachee, 460);
                                }
                                else
                                {
                                    obj.TurnTowards(attachee); // added by Livonya
                                    attachee.TurnTowards(obj); // added by Livonya
                                    obj.BeginDialog(attachee, 1);
                                }

                            }

                        }

                    }

                }
                else
                {
                    // game.new_sid = 0			## removed by Livonya
                    // else: #Kelno is in the Air Altar room - Air is on alert
                    foreach (var obj in GameSystems.Party.PartyMembers)
                    {
                        // attachee.turn_towards(obj)
                        if ((!attachee.HasMet(obj) && is_safe_to_talk2(attachee, obj, 25) == 1 && obj.type == ObjectType.pc))
                        {
                            ScriptDaemon.record_time_stamp(516);
                            if (((ScriptDaemon.get_v(453) & 4) == 0 && (ScriptDaemon.get_v(453) & 8) == 0 && (ScriptDaemon.get_v(453) & 16) == 0)) // Air Escort Variables
                            {
                                // How the hell did you get here? GTFO! (not escorted by anyone, probably dropped in from ceiling or snuck in)
                                // For now, ordinary dialogue
                                if (((GetGlobalFlag(104)) || (GetGlobalFlag(105)) || (GetGlobalFlag(107))))
                                {
                                    obj.TurnTowards(attachee); // added by Livonya
                                    attachee.TurnTowards(obj); // added by Livonya
                                    obj.BeginDialog(attachee, 460);
                                }
                                else
                                {
                                    obj.TurnTowards(attachee); // added by Livonya
                                    attachee.TurnTowards(obj); // added by Livonya
                                    obj.BeginDialog(attachee, 1);
                                }

                            }
                            else
                            {
                                if (((GetGlobalFlag(104)) || (GetGlobalFlag(105)) || (GetGlobalFlag(107))))
                                {
                                    obj.TurnTowards(attachee); // added by Livonya
                                    attachee.TurnTowards(obj); // added by Livonya
                                    obj.BeginDialog(attachee, 460);
                                }
                                else
                                {
                                    obj.TurnTowards(attachee); // added by Livonya
                                    attachee.TurnTowards(obj); // added by Livonya
                                    obj.BeginDialog(attachee, 1);
                                }

                            }

                        }

                    }

                }

            }

            if ((GetGlobalVar(715) == 0 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                attachee.CastSpell(WellKnownSpells.ProtectionFromElements, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(715) == 4 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                attachee.CastSpell(WellKnownSpells.ShieldOfFaith, attachee);
                attachee.PendingSpellsToMemorized();
            }

            SetGlobalVar(715, GetGlobalVar(715) + 1);
            return RunDefault;
        }
        public static bool escort_below(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // game.global_flags[144] = 1
            SetGlobalVar(691, 2);
            FadeAndTeleport(0, 0, 0, 5080, 478, 451);
            return RunDefault;
        }
        public static int is_safe_to_talk2(GameObjectBody speaker, GameObjectBody listener, int radius)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= radius))
                {
                    return 1;
                }

            }

            return 0;
        }
        public static void unlock_sw_doors()
        {
            foreach (var obj in ObjList.ListVicinity(new locXY(508, 501), ObjectListFilter.OLC_PORTAL))
            {
                var (x1, y1) = obj.GetLocation();
                if ((x1 == 508 && y1 == 501) || (x1 == 508 && y1 == 497))
                {
                    if (((obj.GetPortalFlags() & PortalFlag.LOCKED)) != 0)
                    {
                        obj.FloatMesFileLine("mes/spell.mes", 30004);
                        obj.ClearPortalFlag(PortalFlag.LOCKED);
                    }

                }

            }

        }
        public static int should_open_sw_doors()
        {
            foreach (var obj in ObjList.ListVicinity(new locXY(508, 501), ObjectListFilter.OLC_PORTAL))
            {
                var (x1, y1) = obj.GetLocation();
                if ((x1 == 508 && y1 == 501) || (x1 == 508 && y1 == 497))
                {
                    if (((obj.GetPortalFlags() & PortalFlag.LOCKED)) != 0)
                    {
                        return 1;
                    }

                }

            }

            return 0;
        }

    }
}
