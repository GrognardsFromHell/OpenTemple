
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

namespace Scripts;

[ObjectScript(445)]
public class SentryHeartbeat : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if (attachee.GetNameId() == 14181) // Water Temple Sentry
        {
            if (ScriptDaemon.get_v("water_sentry_pester") < 7)
            {
                var lll = RandomRange(0, 3);
                attachee.FloatLine(lll + 1100, triggerer);
                ScriptDaemon.set_v("water_sentry_pester", ScriptDaemon.get_v("water_sentry_pester") + 1);
            }
            else if (ScriptDaemon.get_v("water_sentry_pester") == 7)
            {
                var lll = RandomRange(0, 1);
                attachee.FloatLine(1104 + lll, triggerer);
                ScriptDaemon.set_v("water_sentry_pester", ScriptDaemon.get_v("water_sentry_pester") + 1);
            }
            else if (ScriptDaemon.get_v("water_sentry_pester") == 8)
            {
                attachee.FloatLine(1150, triggerer);
                ScriptDaemon.set_v("water_sentry_pester", ScriptDaemon.get_v("water_sentry_pester") + 1);
            }
            else if (ScriptDaemon.get_v("water_sentry_pester") > 8)
            {
                attachee.SetCritterFlag(CritterFlag.MUTE);
            }

        }
        else if ((!attachee.HasMet(triggerer)))
        {
            ScriptDaemon.record_time_stamp(501);
            if ((GetGlobalFlag(91)))
            {
                triggerer.BeginDialog(attachee, 100);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

        }
        else
        {
            triggerer.BeginDialog(attachee, 300);
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        var dummy = 1;
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if (attachee.GetNameId() == 14181) // Water Temple Bugbear
        {
            if ((!GameSystems.Combat.IsCombatActive() && (ScriptDaemon.get_v(453) & 2) == 0))
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    if (pc.type == ObjectType.pc && is_safe_to_talk2(attachee, pc, 20))
                    {
                        // attachee.turn_towards(pc)
                        // pc.begin_dialog(attachee, 1)
                        // game.particles('ef-minocloud', attachee)
                        attachee.ClearCritterFlag(CritterFlag.MUTE);
                        attachee.SetScriptId(ObjScriptEvent.Dialog, 445);
                        ScriptDaemon.set_v(453, ScriptDaemon.get_v(453) | 2); // escorting to Water flag
                        var trueCount = (ScriptDaemon.tsc(456, 475) ? 1 : 0)
                                        + (ScriptDaemon.tsc(458, 475) ? 1 : 0)
                                        + (ScriptDaemon.tsc(459, 475) ? 1 : 0);
                        if (trueCount >= 2)
                        {
                            attachee.FloatLine(1000, pc);
                        }
                        else if (ScriptDaemon.tsc(456, 475))
                        {
                            attachee.FloatLine(1001, pc);
                        }
                        else if (ScriptDaemon.tsc(458, 475))
                        {
                            attachee.FloatLine(1002, pc);
                        }
                        else if (ScriptDaemon.tsc(459, 475))
                        {
                            attachee.FloatLine(1003, pc);
                        }

                    }

                }

            }

        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if (obj.GetLeader() == null)
            {
                obj.SetNpcFlag(NpcFlag.KOS);
                obj.ClearNpcFlag(NpcFlag.KOS_OVERRIDE);
            }

        }

        attachee.RemoveScript(ObjScriptEvent.Heartbeat);
        if (attachee.GetNameId() == 14181)
        {
            // Water Temple sentry
            SetGlobalFlag(345, false);
            foreach (var statue in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_SCENERY))
            {
                if ((statue.GetNameId() == 1618))
                {
                    var (sx, sy) = statue.GetLocation();
                    if (sy > 566)
                    {
                        var loc = statue.GetLocation();
                        var rot = statue.Rotation;
                        statue.Destroy();
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                        {
                            if (obj.GetNameId() == 14244)
                            {
                                return RunDefault;
                            }

                        }

                        var juggernaut = GameSystems.MapObject.CreateObject(14244, loc);
                        juggernaut.Rotation = rot;
                        AttachParticles("ef-MinoCloud", juggernaut);
                    }

                }

            }

        }

        return RunDefault;
    }
    public static bool is_safe_to_talk2(GameObject speaker, GameObject listener, int radius)
    {
        if ((speaker.HasLineOfSight(listener) && listener.type == ObjectType.pc))
        {
            if ((speaker.DistanceTo(listener) <= radius))
            {
                return true;
            }

        }

        return false;
    }

}