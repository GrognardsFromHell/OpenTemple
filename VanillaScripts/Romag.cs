
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts;

[ObjectScript(119)]
public class Romag : BaseObjectScript
{

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((!attachee.HasMet(triggerer)))
        {
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
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(104, true);
        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(104, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((Utilities.is_safe_to_talk(attachee, obj)))
                {
                    if ((!attachee.HasMet(obj)))
                    {
                        if ((GetGlobalFlag(91)))
                        {
                            obj.BeginDialog(attachee, 100);
                        }
                        else
                        {
                            obj.BeginDialog(attachee, 1);
                            DetachScript();

                        }

                    }

                }

            }

        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(347, false);
        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        if ((Utilities.obj_percent_hp(attachee) < 50 && (!attachee.HasMet(triggerer))))
        {
            GameObject found_pc = null;

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if (pc.type == ObjectType.pc)
                {
                    found_pc = pc;

                    attachee.AIRemoveFromShitlist(pc);
                }

            }

            if (found_pc != null)
            {
                found_pc.BeginDialog(attachee, 200);
                return SkipDefault;
            }

        }

        return RunDefault;
    }
    public static bool talk_Hedrack(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8046);

        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }
        else
        {
            triggerer.BeginDialog(attachee, 580);
        }

        return SkipDefault;
    }
    public static bool escort_below(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(144, true);
        attachee.SetStandpoint(StandPointType.Day, 267);
        attachee.SetStandpoint(StandPointType.Night, 267);
        FadeAndTeleport(0, 0, 0, 5080, 478, 451);
        return RunDefault;
    }


}