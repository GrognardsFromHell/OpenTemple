
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

[ObjectScript(584)]
public class Bethany : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(triggerer);
        if ((attachee.GetMap() == 5070 || attachee.GetMap() == 5071 || attachee.GetMap() == 5072 || attachee.GetMap() == 5073 || attachee.GetMap() == 5074 || attachee.GetMap() == 5075 || attachee.GetMap() == 5076 || attachee.GetMap() == 5077))
        {
            if ((triggerer.GetRace() == RaceId.half_orc))
            {
                triggerer.BeginDialog(attachee, 6);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

        }
        else if ((attachee.GetMap() == 5171))
        {
            if ((GetGlobalFlag(560) && GetGlobalFlag(561) && GetGlobalFlag(562)))
            {
                if ((!GetGlobalFlag(549)))
                {
                    triggerer.BeginDialog(attachee, 240);
                }
                else
                {
                    if ((!GetGlobalFlag(962)))
                    {
                        triggerer.BeginDialog(attachee, 510);
                    }
                    else
                    {
                        triggerer.BeginDialog(attachee, 520);
                    }

                }

            }
            else
            {
                if ((GetGlobalFlag(563)))
                {
                    triggerer.BeginDialog(attachee, 540);
                }
                else
                {
                    if ((!ScriptDaemon.npc_get(attachee, 1)))
                    {
                        triggerer.BeginDialog(attachee, 530);
                        ScriptDaemon.npc_set(attachee, 1);
                    }
                    else
                    {
                        if ((!GetGlobalFlag(962)))
                        {
                            triggerer.BeginDialog(attachee, 550);
                        }
                        else
                        {
                            triggerer.BeginDialog(attachee, 560);
                        }

                    }

                }

            }

        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5171))
        {
            if ((!GetGlobalFlag(826) && GetQuestState(62) == QuestState.Accepted))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() == null))
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

        }

        SetGlobalFlag(826, true);
        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(826, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5071))
        {
            if ((!ScriptDaemon.npc_get(attachee, 3)))
            {
                attachee.SetInt(obj_f.hp_damage, 50);
                StartTimer(2000, () => talk_talk(attachee, triggerer));
                ScriptDaemon.npc_set(attachee, 3);
            }

            if ((ScriptDaemon.npc_get(attachee, 2) && !ScriptDaemon.npc_get(attachee, 4)))
            {
                StartTimer(200, () => beth_exit(attachee, triggerer));
                ScriptDaemon.npc_set(attachee, 4);
            }

        }

        return RunDefault;
    }
    public static bool face_holly(GameObject attachee, GameObject triggerer)
    {
        var holly = Utilities.find_npc_near(attachee, 8714);
        attachee.TurnTowards(holly);
        holly.TurnTowards(attachee);
        return RunDefault;
    }
    public static bool heal_beth(GameObject attachee, GameObject triggerer)
    {
        var dice = Dice.Parse("1d10+1000");
        attachee.Heal(null, dice);
        attachee.HealSubdual(null, dice);
        Sound(4182, 1);
        AttachParticles("sp-Heal", attachee);
        return RunDefault;
    }
    public static int is_25_and_under(GameObject speaker, GameObject listener)
    {
        if ((speaker.DistanceTo(listener) <= 25))
        {
            return 1;
        }

        return 0;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        attachee.RunOff();
        return RunDefault;
    }
    public static void talk_talk(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 1);
        ScriptDaemon.npc_set(attachee, 2);
        return;
    }
    public static bool beth_exit(GameObject attachee, GameObject triggerer)
    {
        attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
        attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
        attachee.RunOff(new locXY(480, 480));
        return RunDefault;
    }

}