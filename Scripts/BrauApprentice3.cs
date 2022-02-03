
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

[ObjectScript(90)]
public class BrauApprentice3 : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(triggerer);
        if ((attachee.GetArea() != 3))
        {
            if ((GetQuestState(60) == QuestState.Completed))
            {
                triggerer.BeginDialog(attachee, 210);
            }
            else if ((GetGlobalFlag(86)))
            {
                triggerer.BeginDialog(attachee, 200);
            }
            else if ((attachee.GetMap() == 5037))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((attachee.GetMap() == 5007))
            {
                triggerer.BeginDialog(attachee, 320);
            }

        }
        else if ((attachee.GetArea() == 3))
        {
            if ((GetGlobalFlag(871)))
            {
                triggerer.BeginDialog(attachee, 400);
            }
            else
            {
                triggerer.BeginDialog(attachee, 60);
            }

        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if (((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2) && (attachee.GetArea() == 1)))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
        }
        else if ((GetGlobalFlag(859)))
        {
            // nulb business is over
            if ((attachee.GetMap() == 5060))
            {
                // waterside hostel
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else if ((attachee.GetMap() == 5037))
            {
                // brewhouse
                if ((Utilities.is_daytime()))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

                if ((!Utilities.is_daytime()))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5007))
            {
                // welcome wench
                if ((Utilities.is_daytime()))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else if ((!Utilities.is_daytime()))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }

        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        SetGlobalFlag(86, true);
        SetGlobalFlag(860, true);
        if ((!PartyLeader.HasReputation(9)))
        {
            PartyLeader.AddReputation(9);
        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5037))
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((obj.GetLeader() != null))
                {
                    obj.TurnTowards(triggerer);
                    obj.Attack(triggerer);
                }

            }

            DetachScript();
        }

        return RunDefault;
    }
    public override bool OnExitCombat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetStat(Stat.subdual_damage) >= attachee.GetStat(Stat.hp_current) && attachee.GetStat(Stat.hp_current) >= 1 && !GameSystems.Combat.IsCombatActive()))
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if (pc.type == ObjectType.pc)
                {
                    attachee.AIRemoveFromShitlist(pc);
                }

            }

            SetGlobalFlag(871, true);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(860, false);
        if ((!GetGlobalFlag(859) && attachee.GetMap() == 5060))
        {
            SetGlobalFlag(86, false);
        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(871) && attachee.GetMap() == 5060 && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone) && (attachee.GetStat(Stat.hp_current) - attachee.GetStat(Stat.subdual_damage)) != 0))
        {
            SetGlobalFlag(859, true);
            // game.global_flags[86] = 1
            // attachee.object_flag_set(OF_OFF)
            // attachee.runoff(attachee.location-3)
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((Utilities.is_safe_to_talk(attachee, obj)))
                {
                    obj.BeginDialog(attachee, 600);
                    return RunDefault;
                }

            }

            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }

        if ((GetGlobalFlag(871) && attachee.GetMap() != 5060 && !GetGlobalFlag(859)))
        {
            SetGlobalFlag(859, true);
        }

        if ((GetGlobalFlag(871) && attachee.FindItemByName(6016) == null && attachee.FindItemByName(6149) == null && attachee.GetMap() == 5037))
        {
            Utilities.create_item_in_inventory(6149, attachee);
            attachee.WieldBestInAllSlots();
        }

        if (((GetGlobalFlag(871) || GetGlobalFlag(858)) && attachee.FindItemByName(5815) != null && attachee.GetMap() == 5007))
        {
            var itemB = attachee.FindItemByName(5815);
            itemB.Destroy();
        }

        if ((attachee.FindItemByName(5815) == null && !GetGlobalFlag(858)))
        {
            SetGlobalFlag(858, true);
        }

        if ((GetGlobalFlag(871) && attachee.FindItemByName(6149) == null && attachee.GetMap() == 5007))
        {
            var itemB = attachee.FindItemByName(6016);
            if ((itemB != null))
            {
                itemB.Destroy();
            }

            var itemC = attachee.FindItemByName(4074);
            if ((itemC != null))
            {
                itemC.Destroy();
            }

            Utilities.create_item_in_inventory(6149, attachee);
            attachee.WieldBestInAllSlots();
        }

        if ((GetGlobalFlag(860)))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }

        return RunDefault;
    }
    public override bool OnWillKos(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(871)))
        {
            return SkipDefault;
        }

        return RunDefault;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        // attachee.standpoint_set( STANDPOINT_NIGHT, 255 )
        // attachee.standpoint_set( STANDPOINT_NIGHT, 38 )
        // game.global_flags[86] = 1
        SetGlobalFlag(859, true);
        attachee.SetObjectFlag(ObjectFlag.OFF);
        // attachee.runoff(attachee.location-3)
        return RunDefault;
    }

}